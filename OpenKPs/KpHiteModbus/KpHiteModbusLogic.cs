using HslCommunication;
using HslCommunication.ModBus;
using KpCommon.Hsl.ModBus;
using KpCommon.Model;
using KpHiteModbus.Modbus;
using KpHiteModbus.Modbus.Extend;
using KpHiteModbus.Modbus.Model;
using Newtonsoft.Json;
using Scada.Data.Configuration;
using Scada.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Scada.Comm.Devices
{
    public class KpHiteModbusLogic : KPLogic
    {
        protected DeviceTemplate deviceTemplate;
        private HashSet<int> floatSignals;
        private List<ModbusTagGroup> modbustagGroupsActive;
        DispatchRequest dispatchRequest;
        IModbus modbus;
        string templateName;

        int ActiveTagGroupCount
        {
            get=>modbustagGroupsActive?.Count > 0 ? modbustagGroupsActive.Count : 0;
        }

        public KpHiteModbusLogic(int number) : base(number)
        {
            ConnRequired = false;
        }

        #region 定时轮询或指令下发
        public override void Session()
        {
            base.Session();
            if (deviceTemplate == null)
            {
                WriteToLog(Localization.UseRussian ?
                     "Нормальное взаимодействие с КП невозможно, т.к. шаблон устройства не загружен" :
                     "Normal device communication is impossible because device template has not been loaded");

                Thread.Sleep(ReqParams.Delay);
                lastCommSucc = false;
            }
            else if (ActiveTagGroupCount > 0)
            {
                int tagGroupIndex = 0;
                while (tagGroupIndex < ActiveTagGroupCount && lastCommSucc)
                {
                    var tagGroup = modbustagGroupsActive[tagGroupIndex];
                    lastCommSucc = false;

                    int tryNum = 0;

                    while (RequestNeeded(ref tryNum))
                    {
                        if (RequestReadData(tagGroup))
                        {
                            lastCommSucc = true;
                            SetTagsData(tagGroup);
                        }

                        FinishRequest();
                        tryNum++;
                    }

                    if(lastCommSucc)
                        tagGroupIndex++;
                    else if(tryNum > 0)
                    {
                        while(tagGroupIndex < ActiveTagGroupCount)
                        {
                            tagGroup = modbustagGroupsActive[tagGroupIndex];
                            if (tagGroup.TagCount > 0)
                                InvalidateCurData(tagGroup.StartKpTagIndex, tagGroup.TagCount);
                            tagGroupIndex++;
                        }
                    }
                }
            }
            else
            {
                WriteToLog(Localization.UseRussian ?
                    "Отсутствуют элементы для запроса" :
                    "No elements for request");
                Thread.Sleep(ReqParams.Delay);
            }

            // 状态刷新
            CalcSessStats();
        }


        public override void SendCmd(Command cmd)
        {
            base.SendCmd(cmd);
            if (CanSendCmd)
            {
                ModbusTagGroup tagGroup = null;
                var tag = deviceTemplate.FindCmd(cmd.CmdNum, out tagGroup);
                if (tag != null)
                {
                    if (cmd.CmdTypeID == BaseValues.CmdTypes.Standard)
                        tag.SetCmdData(cmd.CmdVal);
                    else
                    {
                        if (tag.DataType == DataTypeEnum.String)
                        {
                            try
                            {
                                var strData = Encoding.ASCII.GetString(cmd.CmdData);
                                tag.Data = strData;
                            }
                            catch
                            {
                                tag.Data = null;
                            }
                        }
                    }

                    lastCommSucc = false;
                    int tryNum = 0;
                    while (RequestNeeded(ref tryNum))
                    {
                        if (RequestWriteData(tag,tagGroup))
                            lastCommSucc = true;
                        FinishRequest();
                        tryNum++;
                    }
                }
                else
                {
                    lastCommSucc= false;
                    WriteToLog(CommPhrases.IllegalCommand);
                }

            }
        }
        #endregion

        #region 对象创建初始化
        public override void OnAddedToCommLine()
        {
            string fileName = ReqParams.CmdLine.Trim();
            InitDeviceTemplate(fileName);

            floatSignals = new HashSet<int>();
            //初始化通道
            var tagGroups = CreateTagGroups(deviceTemplate);
            InitKPTags(tagGroups);
            modbustagGroupsActive = deviceTemplate.GetActiveTagGroups();
            CanSendCmd = deviceTemplate != null && deviceTemplate.CmdTagCount > 0;
            //初始化分包请求类
            dispatchRequest = new DispatchRequest(RequestUnitMethod);
        }

        public override void OnCommLineStart()
        {
            try
            {
                var option = deviceTemplate.ConnectionOptions;
                var station = option.Station;
                OperateResult initResult = null;
                var timeOut = ReqParams.Timeout;
                if (timeOut <= 0)
                    timeOut = DefineReadOnlyValues.DefaultRequestTimeOut;
                switch (option.ConnectionType)
                {
                    case KpHiteModbus.Modbus.Model.EnumType.ModbusConnectionTypeEnum.SerialPort:
                        if (option.ModbusMode == KpHiteModbus.Modbus.Model.EnumType.ModbusModeEnum.Ascii)
                        {
                            var modbusAscii = new ModbusAscii(station);
                            modbusAscii.ReceiveTimeout = timeOut;
                            modbusAscii.SerialPortInni(option.PortName, option.BaudRate, option.DataBits, option.StopBits, option.Parity);
                            initResult = modbusAscii.Open();
                            if(initResult.IsSuccess)
                                modbus = modbusAscii;
                        }
                        else
                        {
                            var modbusRtu= new ModbusRtu(station);
                            modbusRtu.ReceiveTimeout = timeOut;
                            modbusRtu.SerialPortInni(option.PortName, option.BaudRate, option.DataBits, option.StopBits, option.Parity);
                            initResult = modbusRtu.Open();
                            if (initResult.IsSuccess)
                                modbus = modbusRtu;
                        }
                        break;
                    case KpHiteModbus.Modbus.Model.EnumType.ModbusConnectionTypeEnum.TcpIP:
                        var modbusTcp = new ModbusTcpNet(option.IPAddress,option.Port,station);
                        modbusTcp.ReceiveTimeOut = timeOut;
                        initResult = modbusTcp.ConnectServer();
                        if (initResult.IsSuccess)
                            modbus = modbusTcp;
                            break;
                    case KpHiteModbus.Modbus.Model.EnumType.ModbusConnectionTypeEnum.Udp:
                        var modbusUdp = new ModbusUdpNet(option.IPAddress,option.Port,station);
                        modbusUdp.ReceiveTimeout = timeOut;
                        initResult = new OperateResult();
                        var pingResult = modbusUdp.IpAddressPing();
                        if (pingResult == System.Net.NetworkInformation.IPStatus.Success)
                        {
                            initResult.IsSuccess = true;
                            modbus = modbusUdp;
                        }
                        else
                        {
                            initResult.IsSuccess = false;
                            initResult.Message = $"ping服务失败,{pingResult}";
                        }
                        break;
                    case KpHiteModbus.Modbus.Model.EnumType.ModbusConnectionTypeEnum.RTUASCIIOverTcp:
                        if(option.ModbusMode == KpHiteModbus.Modbus.Model.EnumType.ModbusModeEnum.Ascii)
                        {
                            var modbusAsciiOverTcp = new ModbusAsciiOverTcp(option.IPAddress, option.Port, station);
                            modbusAsciiOverTcp.ReceiveTimeOut = timeOut;
                            initResult = modbusAsciiOverTcp.ConnectServer();
                            if(initResult.IsSuccess)
                                modbus = modbusAsciiOverTcp;
                        }
                        else
                        {
                            var modbusRtuOverTcp = new ModbusRtuOverTcp(option.IPAddress, option.Port, station);
                            modbusRtuOverTcp.ReceiveTimeOut = timeOut;
                            initResult = modbusRtuOverTcp.ConnectServer();
                            if (initResult.IsSuccess)
                                modbus = modbusRtuOverTcp;
                        }
                        break;
                    //case KpHiteModbus.Modbus.Model.EnumType.ModbusConnectionTypeEnum.RTUASCIIOverUdp:
                    //    if(option.ModbusMode == KpHiteModbus.Modbus.Model.EnumType.ModbusModeEnum.Ascii)
                    //    {
                    //        var modbusAsciiOverUdp = new ModbusA
                    //    }
                    //    else
                    //    {

                    //    }
                    //    break;

                }

                if (!initResult.IsSuccess)
                    WriteToLog($"Name:{Name},Number:{Number},初始化连接失败,{initResult.Message}");
            }
            catch(Exception ex)
            {
                WriteToLog($"KpHiteModbusLogic_OnCommLineStart,Name:{Name},Number:{Number},连接PLC异常,{ex.Message}");
            }
        }

        private void InitDeviceTemplate(string fileName)
        {
            deviceTemplate = null;
            if (string.IsNullOrEmpty(fileName))
            {
                WriteToLog($"Name:{Name},Number:{Number},初始化模板失败,找到相应模板文件");
                return;
            }

            deviceTemplate = new DeviceTemplate();
            string filePath = Path.IsPathRooted(fileName) ?
                        fileName : Path.Combine(AppDirs.ConfigDir, fileName);
            if (!deviceTemplate.Load(filePath, out string errMsg))
                WriteToLog(errMsg);
            else
            {
                templateName = Path.GetFileName(filePath);
                //初始话Tag和Command 点位顺序和地址
                deviceTemplate.RefreshTagGroupIndex();
                foreach (var tagGroup in deviceTemplate.TagGroups)
                    tagGroup.RefreshTagIndex();

            }
        }

        private List<TagGroup> CreateTagGroups(DeviceTemplate deviceTemplate)
        {
            List<TagGroup> tagGroups = new List<TagGroup>();
            if (deviceTemplate != null)
            {
                foreach (ModbusTagGroup siemensTagGroup in deviceTemplate.TagGroups)
                {
                    var tagGroup = new TagGroup(siemensTagGroup.Name);
                    tagGroups.Add(tagGroup);
                    foreach (Tag tag in siemensTagGroup.Tags)
                    {
                        tagGroup.AddNewTag(tag.TagID, tag.Name);
                        if (tag.DataType == DataTypeEnum.Double || tag.DataType == DataTypeEnum.Float)
                            floatSignals.Add(tag.TagID);
                    }
                }
            }

            return tagGroups;
        }
        #endregion


        #region 自定义驱动数据请求、下发


        private bool RequestReadData(ModbusTagGroup tagGroup)
        {
            var model = tagGroup.GetRequestModel();
            WriteToLog($"Name:{Name},Number:{Number},开始请求数据,GroupName:{tagGroup.Name},寄存器类型:{tagGroup.RegisterType},起始地址:{model.Address},请求长度:{model.Length}");

            try
            {
                //当前寄存器时Coil或DI时进行分包操作
                if(modbus == null)
                {
                    WriteToLog($"Name:{Name},Number:{Number},数据请求失败,Modbus对象为null");
                    return false;
                }
                var result = dispatchRequest.Request(model, tagGroup.RegisterType, out string errorMsg);
                WriteToLog($"Name:{Name},Number:{Number},数据请求结束,Result:{result},Msg:{errorMsg}");
                if (!result)
                    return false;

                var responses = dispatchRequest.Response();
                var responseResult = responses.SelectMany(r =>r.Buffer);
                tagGroup.Data = responseResult.ToArray();
                return true;
            }
            catch(Exception ex)
            {
                WriteToLog($"KpHiteModbusLogic_RequestData,Name:{Name},Number:{Number},数据请求异常,{ex.Message},StackTrace:{ex.StackTrace}");
                return false;
            }
        }

        private void RequestUnitMethod(RequestUnit requestUnit)
        {
            try
            {
                
                var functionCode = requestUnit.RegisterType.GetFunctionCode(iswrite: false);
                var address = $"x={functionCode};{requestUnit.StartAddress}";
                WriteToLog($"Name:{Name},Number:{Number},开始请求,Index:{requestUnit.Index},Address:{address},RequestLength:{requestUnit.RequestLength}");
                var result = modbus.Read(address, requestUnit.RequestLength);
                if (result.IsSuccess)
                    requestUnit.Buffer = result.Content;
                else
                {
                    //失败的时候填充默认字节
                    byte[] buffer;
                    if (requestUnit.RegisterType == RegisterTypeEnum.DiscretesInputs || requestUnit.RegisterType == RegisterTypeEnum.Coils)
                    {
                        var byteCount = requestUnit.RequestLength / 8;
                        var left = requestUnit.RequestLength % 8;
                        buffer = new byte[byteCount + left > 0 ? 1 : 0];
                    }
                    else
                        buffer = new byte[requestUnit.RequestLength * 2];

                    requestUnit.Buffer = buffer;
                }
                WriteToLog($"Name:{Name},Number:{Number},请求结束,结果:{JsonConvert.SerializeObject(result)}");
            }
            catch(Exception ex )
            {
                WriteToLog($"KpHiteModbusLogic_RequestUnitMethod,Name:{Name},Number:{Number},数据请求异常,{ex.Message},StackTrace:{ex.StackTrace}");
            }
        }



        private void SetTagsData(ModbusTagGroup tagGroup)
        {
            for(int i = 0; i < tagGroup.Tags.Count; i++)
            {
                var tag = tagGroup.Tags[i]; 
                var data = tagGroup.GetTagVal(i);
                if (data != null)
                    SetCurData(tag.TagID > 0 ? tag.TagID - 1 : tag.TagID, (double)data, BaseValues.CnlStatuses.Defined);
            }
        }

        private bool RequestWriteData(Tag tag,ModbusTagGroup tagGroup)
        {
            if (tag.Data == null)
                return false;


            var address =  $"{tag.Address}";
            WriteToLog($"Name:{Name},Number:{Number},开始写入数据,Name:{tag.Name},寄存器类型:{tag.RegisterType},地址:{tag.Address},写入值:{JsonConvert.SerializeObject(tag.Data)}");
            try
            {
                OperateResult operateResult = new OperateResult { IsSuccess = false};
                switch (tag.DataType)
                {
                    case DataTypeEnum.Bool:
                        var functionCode = tagGroup.RegisterType.GetFunctionCode(iswrite: true, ismultiple: true);
                        address = $"x={functionCode};{tag.Address}";
                        operateResult = modbus.Write(address,new bool[] { (bool)tag.Data });
                        break;
                    case DataTypeEnum.Byte:
                        operateResult = modbus.Write(address,((byte)tag.Data));
                        break;
                    case DataTypeEnum.Short:
                        operateResult = modbus.Write(address,(short)tag.Data);
                        break;
                    case DataTypeEnum.UShort:
                        operateResult = modbus.Write(address,(ushort)tag.Data);
                        break;
                    case DataTypeEnum.Int:
                        operateResult = modbus.Write(address,(int)tag.Data);
                        break;
                    case DataTypeEnum.UInt:
                        operateResult = modbus.Write(address,(uint)tag.Data);
                        break; 
                    case DataTypeEnum.Long:
                        operateResult = modbus.Write(address,(long)tag.Data);
                        break; 
                    case DataTypeEnum.ULong:
                        operateResult = modbus.Write(address,(ulong)tag.Data);
                        break;
                    case DataTypeEnum.Float:
                        operateResult = modbus.Write(address,(float)tag.Data);
                        break;
                    case DataTypeEnum.Double:
                        operateResult = modbus.Write(address,(double)tag.Data);
                        break;
                    case DataTypeEnum.String:
                        operateResult = modbus.Write(address,(string)tag.Data);
                        break;
                    default:
                        operateResult.Message = $"未知数据类型,{tag.DataType}";
                        break;
                }
                WriteToLog($"Name:{Name},Number:{Number},写入数据结束,Name:{tag.Name},写入结果:{operateResult.IsSuccess},Message:{operateResult.Message}");
                return operateResult.IsSuccess;
            }
            catch(Exception ex)
            {
                WriteToLog($"KpHiteModbusLogic_RequestWriteData,Name:{Name},Number:{Number},写入数据异常,{ex.Message},StackTrace:{ex.StackTrace}");
                return false;
            }
        }
        #endregion
    }
}
