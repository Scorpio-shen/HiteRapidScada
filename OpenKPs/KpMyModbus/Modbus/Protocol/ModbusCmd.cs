using Scada;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace KpMyModbus.Modbus.Protocol
{
    public class ModbusCmd : DataUnit
    {
        public TagDataType TagDataType { get; set; }
        public bool Multiple { get; set; }
        public int TagCount { get; set; }
        public int CmdNum { get; set; }
        public ushort Value { get; set; }
        //public byte[] Data { get; set; }
        public dynamic WriteData { get; set; }  

        //public dynamic WriteData { get; set; }
        private ModbusCmd() : base()
        {

        }

        public ModbusCmd(ModbusRegisterType registerType,bool multiple) : base(registerType)
        {
            if (!(registerType == ModbusRegisterType.Coils || registerType == ModbusRegisterType.HoldingRegisters))
                throw new InvalidOperationException("错误寄存器类型");
            FuncCode = (byte)(registerType == ModbusRegisterType.Coils ? 5 : 6);
        }

        public override bool TagDataTypeEnabled
        {
            get
            {
                return ModbusRegisterType == ModbusRegisterType.HoldingRegisters && Multiple;
            }
        }

        public virtual void LoadFromXml(XmlElement cmdElement)
        {
            if (cmdElement == null)
                throw new ArgumentNullException("cmdElem");

            Address = (ushort)cmdElement.GetAttrAsInt("address");
            TagDataType = cmdElement.GetAttrAsEnum("tagType", DefElemType);
            TagCount = cmdElement.GetAttrAsInt("tagCount", 1);
            Name = cmdElement.GetAttribute("name");
            CmdNum = cmdElement.GetAttrAsInt("cmdNum");
        }

        public virtual void SaveToXml(XmlElement cmdElement)
        {
            if (cmdElement == null)
                throw new ArgumentException("cmdElement");

            cmdElement.SetAttribute("modbusRegisterType", ModbusRegisterType);
            cmdElement.SetAttribute("multiple",Multiple);
            cmdElement.SetAttribute("address",Address);

            if(TagDataTypeEnabled)
                cmdElement.SetAttribute("tagDataType", TagDataType.ToString().ToLowerInvariant());
            if (Multiple)
                cmdElement.SetAttribute("tagCount", TagCount);

            cmdElement.SetAttribute("cmdNum", CmdNum);
            cmdElement.SetAttribute("name", Name);
        }

        //public void SetCmdData(double cmdVal)
        //{
        //    bool reverse = true;

        //    switch (TagDataType)
        //    {
        //        case TagDataType.UShort:
        //            Data = BitConverter.GetBytes((ushort)cmdVal);
        //            break;
        //        case TagDataType.Short:
        //            Data = BitConverter.GetBytes((short)cmdVal);
        //            break;
        //        case TagDataType.UInt:
        //            Data = BitConverter.GetBytes((uint)cmdVal);
        //            break;
        //        case TagDataType.Int:
        //            Data = BitConverter.GetBytes((int)cmdVal);
        //            break;
        //        case TagDataType.ULong:
        //            Data = BitConverter.GetBytes((ulong)cmdVal);
        //            break;
        //        case TagDataType.Long:
        //            Data = BitConverter.GetBytes((long)cmdVal);
        //            break;
        //        case TagDataType.Float:
        //            Data = BitConverter.GetBytes((float)cmdVal);
        //            break;
        //        case TagDataType.Double:
        //            Data = BitConverter.GetBytes(cmdVal);
        //            break;
        //        case TagDataType.Bool:
        //            byte[] boolData = new byte[2] { 0x00, 0x00 };
        //            if (cmdVal > 0)
        //                boolData[0] = 0xFF;
        //            Data = boolData;
        //            break;
        //        default:
        //            Data = BitConverter.GetBytes(cmdVal);
        //            reverse = false;
        //            break;
        //    }

        //    if (reverse)
        //        Array.Reverse(Data);
        //}

        public void SetCmdData(double cmdVal)
        {
            switch (TagDataType)
            {
                case TagDataType.UShort:
                    WriteData = (ushort)cmdVal;
                    break;
                case TagDataType.Short:
                    WriteData = (short)cmdVal;
                    break;
                case TagDataType.UInt:
                    WriteData = (uint)cmdVal;
                    break;
                case TagDataType.Int:
                    WriteData = (int)cmdVal;
                    break;
                case TagDataType.ULong:
                    WriteData = (ulong)cmdVal;
                    break;
                case TagDataType.Long:
                    WriteData = (long)cmdVal;
                    break;
                case TagDataType.Float:
                    WriteData = (float)cmdVal;
                    break;
                case TagDataType.Double:
                    WriteData =cmdVal;
                    break;
                case TagDataType.Bool:
                    WriteData = cmdVal > 0 ? true : false;
                    break;
                default:
                    WriteData =(double)cmdVal;
                    break;
            }
        }


        public void UpdateFuncCode()
        {
            if (ModbusRegisterType == ModbusRegisterType.Coils)
                FuncCode = Multiple ? FunctionCodes.WriteMultipleCoils : FunctionCodes.WriteSingleCoil;
            else
                FuncCode = Multiple ? FunctionCodes.WriteMultipleRegisters : FunctionCodes.WriteSingleRegister;
        }

        protected override int GetRequestByteLength()
        {
            throw new Exception("ModbusCmd无法获取长度!"); 
        }
    }
}
