using Scada;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace KpMyModbus.Modbus.Protocol
{
    public class MyTagGroup : DataUnit
    {
        public bool Active { get; set; } = true;
        public int StartKPTagIndex { get; set; }
        public int StartSignal { get; set; }

        
        public List<Tag> Tags { get; set; }
        private MyTagGroup() : base()
        {
        }
        public MyTagGroup(ModbusRegisterType registerType)
        {
            Active = true;
            Tags = new List<Tag>();

            // определение кодов функций
            ModbusRegisterType = registerType;
            UpdateFuncCode();
            ExcFuncCode = (byte)(FuncCode | 0x80);
        }
        
        public virtual void LoadFromXml(XmlElement groupElement)
        {
            if (groupElement == null)
                throw new ArgumentNullException("groupElem");

            Name = groupElement.GetAttribute("name");
            Address = (ushort)groupElement.GetAttrAsInt("address");
            Active = groupElement.GetAttrAsBool("active", true);

            XmlNodeList tagsNodes = groupElement.SelectNodes("Tag");
            int maxCount = MaxElemCnt;
            TagDataType dataType = DefElemType;

            if (Tags.Count >= maxCount)
                return;
            foreach(XmlElement tagNode in tagsNodes)
            {
                var tag = new Tag();
                tag.Name = tagNode.GetAttribute("name");
                tag.DataType = tagNode.GetAttrAsEnum("type", dataType);
                Tags.Add(tag);
            }

        }

        public virtual void SaveToXml(XmlElement xmlElement)
        {
            if (xmlElement == null)
                throw new ArgumentNullException("xmlElement");
            xmlElement.SetAttribute("active", Active);
            xmlElement.SetAttribute("modbusRegisterType", ModbusRegisterType);
            xmlElement.SetAttribute("address", Address);
            xmlElement.SetAttribute("name", Name);

            foreach(var tag in Tags)
            {
                var element = xmlElement.AppendElem("Tag");
                element.SetAttribute("name", tag.Name);
                if(TagDataTypeEnabled)
                    element.SetAttribute("type", tag.DataType.ToString().ToLowerInvariant());
            }
        }

        public void UpdateFuncCode()
        {
            switch (ModbusRegisterType)
            {
                case ModbusRegisterType.DiscreteInputs:
                    FuncCode = FunctionCodes.ReadDiscreteInputs;
                    break;
                case ModbusRegisterType.Coils:
                    FuncCode = FunctionCodes.ReadCoils;
                    break;
                case ModbusRegisterType.InputRegisters:
                    FuncCode = FunctionCodes.ReadInputRegisters;
                    break;
                default: // TableTypes.HoldingRegisters:
                    FuncCode = FunctionCodes.ReadHoldingRegisters;
                    break;
            }
        }

        /// <summary>
        /// 获取请求数据长度
        /// </summary>
        protected override int GetRequestByteLength()
        {
            int length = default;
            if(ModbusRegisterType == ModbusRegisterType.Coils || ModbusRegisterType == ModbusRegisterType.DiscreteInputs)
            {
                //每个Tag占一个bit
                length += Tags.Count / 8;
                length += Tags.Count % 8 > 0 ? 1 : 0;
            }
            else
            {
                foreach(var tag in Tags)
                    length += tag.ByteCount;
            }

            return length;
        }

        /// <summary>
        /// 响应数据赋值
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public override bool DecodeResponseContent(byte[] buffer, out string errMsg)
        {
            var result = base.DecodeResponseContent(buffer, out errMsg);

            try
            {
                if (ModbusRegisterType == ModbusRegisterType.Coils || ModbusRegisterType == ModbusRegisterType.InputRegisters)
                {
                    //bit数据
                    var bitArray = new BitArray(buffer);
                    for (int i = 0; i < Tags.Count; i++)
                    {
                        var tag = Tags[i];
                        tag.Data = new byte[] { bitArray[i] ? (byte)1 : (byte)0 };
                    }
                }
                else
                {
                    int byteNum = 0;
                    foreach (var tag in Tags)
                    {
                        if (buffer.Length > byteNum + tag.ByteCount)
                            tag.Data = buffer.Skip(byteNum).Take(tag.ByteCount).ToArray();
                        byteNum += tag.ByteCount;
                    }
                }
                result = true;
            }
            catch(Exception ex)
            {
                errMsg = $"MyTagGroup_DecodeResponseContent异常,{ex.Message}";
                result = false;
            }
            
            return result;
        }

        public double GetTagVal(int tagIndex)
        {
            if (tagIndex >= Tags.Count)
                return default;
            var tag = Tags[tagIndex];
            if (tag.Data == null)
                return default;
            var buf = tag.Data;
            
            switch (tag.DataType)
            {
                case TagDataType.UShort:
                    return BitConverter.ToUInt16(buf, 0);
                case TagDataType.Short:
                    return BitConverter.ToInt16(buf, 0);
                case TagDataType.UInt:
                    return BitConverter.ToUInt32(buf, 0);
                case TagDataType.Int:
                    return BitConverter.ToInt32(buf, 0);
                case TagDataType.ULong:
                    return BitConverter.ToUInt64(buf, 0);
                case TagDataType.Long:
                    return BitConverter.ToInt64(buf, 0);
                case TagDataType.Float:
                    return BitConverter.ToSingle(buf, 0);
                case TagDataType.Double:
                    return BitConverter.ToDouble(buf, 0);
                case TagDataType.Bool:
                    return buf[0] > 0 ? 1.0 : 0.0;
                default:
                    return 0.0;
            }
        }
    }
}
