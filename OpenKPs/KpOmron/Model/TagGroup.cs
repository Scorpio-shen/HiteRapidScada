using KpCommon.Extend;
using KpCommon.Model;
using KpOmron.Extend;
using KpOmron.Model.EnumType;
using Scada;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace KpOmron.Model
{
    public class TagGroup : GroupUnit<Tag>
    {
        public int MaxTagCount = ushort.MaxValue;

        #region 构造函数
        public TagGroup()
        {
            Tags = new List<Tag>();
        }

        public TagGroup(MemoryTypeEnum registerType)
        {
            Tags = new List<Tag>();
            MemoryType = registerType;
            Active = true;
            StartKpTagIndex = -1;
        }
        #endregion

        #region 属性
        /// <summary>
        /// 是否全部支持写入
        /// </summary>
        public bool AllCanWrite
        {
            get => Tags?.Count > 0 && Tags.All(t => t.CanWrite > 0);
        }

        private MemoryTypeEnum memoryType;
        /// <summary>
        /// 寄存器类型
        /// </summary>
        public MemoryTypeEnum MemoryType
        {
            get=>memoryType;
            set
            {
                memoryType = value;
                SetMemoryType(memoryType);
                OnPropertyChanged(nameof(MemoryType));
            }
        }
        private void SetMemoryType(MemoryTypeEnum registerType)=> Tags.ForEach(t => t.MemoryType = memoryType);
        private int maxrequestbytelength;

        /// <summary>
        /// 限制一组Tag最大请求数据字节长度
        /// </summary>
        public override int MaxRequestByteLength
        {
            get
            {
                if(maxrequestbytelength <= 0)
                    maxrequestbytelength = TagGroupDefaultValues.MaxAddressRequestLength;
                return maxrequestbytelength;
            }
            set => maxrequestbytelength = value;
        }

        //private int requestlength;
        ///// <summary>
        ///// 请求寄存器个数
        ///// </summary>
        //public int RequestLength
        //{
        //    get => requestlength;
        //    set=> requestlength = value;
        //}
        #endregion

        #region 载入存储Xml
        #endregion

        #region 批量添加点集合
        public override bool CheckAndAddTags(List<Tag> addTags, out string errorMsg, bool needClear = false)
        {
            errorMsg= string.Empty;
            //将原有对象先拷贝
            bool result = true;
            var tagsOld = new List<Tag>();
            tagsOld.AddRange(Tags.Select(t => t.Clone() as Tag));

            if (needClear)
                Tags.Clear();
            Tags.AddRange(addTags);
            SortTags();
            //验证是否超出最大点数限制
            if ((TagCount + StartKpTagIndex > DefineReadOnlyValues.MaxTagCount))
            {
                Tags.Clear();
                Tags.AddRange(tagsOld);
                errorMsg = "超出点数限制!";
                result = false;
            }
            var model = GetRequestModel();
            int byteCount = default;
            //验证是否超出最大地址限制
            if (byteCount > MaxRequestByteLength)
            {
                Tags.Clear();
                Tags.AddRange(tagsOld);
                errorMsg = "数据超出范围!";
                result = false;
            }
            RefreshTagIndex();
            OnPropertyChanged(nameof(Tags));
            return result;
        }

        public RequestModel GetRequestModel()
        {
            var model = new RequestModel();
            model.Address = $"{MemoryType}{StartAddress}";
            if (TagCount == 0)
                model.Length = 0;
            else
            {
                var tagLast = Tags.Last();
                if (double.TryParse(tagLast.Address, out double address))
                {
                    //根据最后一个Tag的数据类型占据的字节数
                    var lastByteCount = tagLast.DataType.GetByteCount();
                    if (tagLast.DataType == DataTypeEnum.String)
                        lastByteCount += tagLast.Length;
                    //计算需要请求的字的个数
                    var length = address - StartAddress;
                    //除以2看需要请求几个字
                    var regCount = (ushort)(lastByteCount / 2 + lastByteCount % 2);
                    model.Length += (ushort)length;
                    model.Length += regCount;
                }
                else
                    model.Length = 0;
            }

            return model;
        }
        public CIPRequestModel GetCIPRequestModel()
        {
            var model = new CIPRequestModel();
            List<string> addresses = new List<string>();
            List<ushort> lengths = new List<ushort>();
            //List<int> byteCounts = new List<int>();


            model.Addresses = addresses;
            model.Lengths = lengths;
            //model.BytesCount= byteCounts;

            foreach (var tag in Tags)
            {
                if (tag.IsArray)
                {
                    addresses.Add(tag.Name);
                    ushort length;
                    if (tag.DataType == DataTypeEnum.Bool)
                    {
                        length = (ushort)(tag.Length / 8 + (tag.Length % 8 > 0 ? 1 : 0));
                    }
                    else
                    {
                        length = (ushort)(tag.DataType.GetByteCount() * tag.Length);
                    }
                    lengths.Add(length);
                }
                else
                {
                    ushort length;
                    if (tag.DataType == DataTypeEnum.String)
                    {
                        length = (ushort)tag.Length;
                    }
                    else
                        length = (ushort)tag.DataType.GetByteCount();

                    lengths.Add(length);
                }
            }

            return model;
        }
        #endregion

        #region 设置Tag是否支持写入
        public void SetTagCanWrite(bool canwrite)
        {
            Tags.ForEach(t => t.CanWriteBool = canwrite);
        }
        #endregion

        public double? GetTagVal(int index)
        {
            double? result = null;
            try
            {
                if (TagCount == 0 || index >= Tags.Count)
                    return result;
                var tag = Tags[index];
                if (tag == null)
                    return result;
                if (Data == null || Data.Length == 0)
                    return result;

                double address = tag.Address.ToDouble() - StartAddress;
                double dPart = address % 1; //小数部分
                int iPart = (int)address;   //整数部分

                if (tag.DataType == DataTypeEnum.Bool)
                {
                    if (Data.Length >= iPart + 2)
                    {
                        var bitArray = new BitArray(Data.Skip(iPart).Take(2).Reverse().ToArray());
                        if (iPart <= 15)
                        {
                            int bitIndex = (int)(dPart * 100);
                            result = bitArray.Get(bitIndex) ? 1.0 : 0.0;
                        }
                    }
                    return result;
                }
                var skipByte = iPart * 2;
                var byteCount = tag.DataType.GetByteCount();
                if (skipByte + byteCount > Data.Length)
                    return result;
                byte[] buf = Data.Skip(skipByte).Take(byteCount).Reverse().ToArray();
                switch (tag.DataType)
                {
                    case DataTypeEnum.Int:
                        return BitConverter.ToInt16(buf, 0);
                    case DataTypeEnum.DInt:
                        return BitConverter.ToInt32(buf, 0);
                    case DataTypeEnum.LInt:
                        return BitConverter.ToInt64(buf, 0);

                    case DataTypeEnum.Word:
                    case DataTypeEnum.UInt:
                        return BitConverter.ToUInt16(buf, 0);
                    case DataTypeEnum.DWord:
                    case DataTypeEnum.UDInt:
                        return BitConverter.ToUInt32(buf, 0);
                    case DataTypeEnum.LWord:
                    case DataTypeEnum.ULInt:
                        return BitConverter.ToUInt64(buf, 0);
                    case DataTypeEnum.Real:
                        return BitConverter.ToSingle(buf, 0);
                    case DataTypeEnum.LReal:
                        return BitConverter.ToDouble(buf, 0);
                    case DataTypeEnum.String:
                        //取实际内容部分
                        try
                        {
                            buf = Data.Skip(iPart + byteCount).Take(tag.Length).ToArray();
                            var str = Encoding.ASCII.GetString(buf).TrimEnd('\0');
                            return ScadaUtils.EncodeAscii(str);
                        }
                        catch
                        {
                            return null;
                        }
                }

                return result;
            }
            catch
            {
                return null;
            }
        }
    }
}
