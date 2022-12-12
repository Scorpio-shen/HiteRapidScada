using KpCommon.Extend;
using KpCommon.Model;
using KpMelsec.Extend;
using KpMelsec.Model.EnumType;
using Scada;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KpMelsec.Model
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
        private double maxrequestbytelength;

        /// <summary>
        /// 限制一组Tag最大请求数据字节长度
        /// </summary>
        public override double MaxRequestByteLength
        {
            get
            {
                if(maxrequestbytelength <= 0)
                    maxrequestbytelength = TagGroupDefaultValues.MaxAddressLength;
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
                var tag = Tags.Last();
                if (double.TryParse(tag.Address, out double address))
                {
                    var length = address + tag.DataType.GetByteCount() - StartAddress;
                    double dPart = length % 1; //小数部分
                    int iPart = (int)length;   //整数部分

                    model.Length += (ushort)iPart;

                    if (dPart > 0)
                        model.Length += 1;
                    if (tag.Length > 0)
                        model.Length += (ushort)tag.Length;
                }
                else
                    model.Length = 0;
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
                    if (Data.Length >= iPart + 1)
                    {
                        var bitArray = new BitArray(Data.Skip(iPart).Take(1).ToArray());
                        if (iPart < 8)
                        {
                            int bitIndex = (int)(dPart * 10);
                            result = bitArray.Get(bitIndex) ? 1.0 : 0.0;
                        }
                    }
                    return result;
                }
                var byteCount = tag.DataType.GetByteCount();
                if (Data.Length < iPart + byteCount)    //超出数据长度
                    return result;
                byte[] buf = Data.Skip(iPart).Take(byteCount).Reverse().ToArray();
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
