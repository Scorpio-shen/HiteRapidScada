using HslCommunication;
using HslCommunication.Core;
using HslCommunication.Profinet.AllenBradley;
using HslCommunication.Profinet.Melsec;
using KpAllenBrandly.Model;
using KpAllenBrandly.Model.EnumType;
using KpCommon.Model;
using Newtonsoft.Json;
using Scada.Data.Configuration;
using Scada.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;

namespace Scada.Comm.Devices
{
    public class KpAllenBrandlyLogic : KPLogic
    {
        protected DeviceTemplate deviceTemplate;
        private HashSet<int> floatSignals;
        private List<KpAllenBrandly.Model.TagGroup> tagGroupsActive;
        AllenBradleyNet readWriteDevice;
        string templateName;
        bool IsConnected = false;

        int ActiveTagGroupCount
        {
            get=>tagGroupsActive?.Count > 0 ? tagGroupsActive.Count : 0;
        }

        public KpAllenBrandlyLogic(int number) : base(number)
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
                    var tagGroup = tagGroupsActive[tagGroupIndex];
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
                            tagGroup = tagGroupsActive[tagGroupIndex];
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
                KpAllenBrandly.Model.TagGroup tagGroup = null;
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
            tagGroupsActive = deviceTemplate.GetActiveTagGroups();
            CanSendCmd = deviceTemplate != null && deviceTemplate.CmdTagCount > 0;
        }

        public override void OnCommLineStart()
        {
            try
            {
                var option = deviceTemplate.ConnectionOptions;
                var ipaddress = option.IPAddress;
                var port = option.Port;

                OperateResult initResult = null;
                var timeOut = ReqParams.Timeout;
                if (timeOut <= 0)
                    timeOut = DefineReadOnlyValues.DefaultRequestTimeOut;
                switch (option.ProtocolType)
                {
                    case ProtocolTypeEnum.EtherNetIP:
                        {
                            var abNet = new AllenBradleyNet(ipaddress, port) { Slot = option.Slot};
                            abNet.ReceiveTimeOut= timeOut;
                            initResult = abNet.ConnectServer();
                            readWriteDevice = abNet;
                        }
                        break;
                    case ProtocolTypeEnum.MicroCip:
                        {
                            var abNet = new AllenBradleyMicroCip(ipaddress, port) { Slot = option.Slot};
                            abNet.ReceiveTimeOut= timeOut;
                            initResult = abNet.ConnectServer();
                            readWriteDevice = abNet;
                        }
                        break;
                    //case ProtocolTypeEnum.SLCNet:
                    //    {
                    //        var abNet = new AllenBradleySLCNet(ipaddress,port);
                    //        abNet.ReceiveTimeOut= timeOut;
                    //        initResult = abNet.ConnectServer();
                    //        readWriteDevice = abNet;
                    //    }
                    //    break;
                    //case ProtocolTypeEnum.McQna3EBinary:
                    //    {
                    //        var mc = new MelsecMcNet(ipaddress,port);
                    //        mc.ReceiveTimeOut= timeOut;
                    //        initResult = mc.ConnectServer();
                    //        readWriteDevice = mc;
                    //    }
                    //    break;
                    //case ProtocolTypeEnum.MCQna3EBinaryUdp:
                    //    {
                    //        var mc = new MelsecMcUdp(ipaddress,port);
                    //        initResult = new OperateResult { IsSuccess = false };
                    //        var status = mc.IpAddressPing();
                    //        mc.ReceiveTimeout= timeOut;
                    //        initResult.IsSuccess = status == IPStatus.Success;
                    //        initResult.Message = status.ToString();
                    //        readWriteDevice= mc;
                    //    }
                    //    break;
                    //case ProtocolTypeEnum.McQna3EASCII:
                    //    {
                    //        var mc = new MelsecMcAsciiNet(ipaddress,port);
                    //        mc.ReceiveTimeOut= timeOut;
                    //        initResult = mc.ConnectServer();
                    //        readWriteDevice = mc;
                    //    }
                    //    break;
                    //case ProtocolTypeEnum.MCQna3EASCIIUdp:
                    //    {
                    //        var mc = new MelsecMcAsciiUdp(ipaddress, port);
                    //        initResult = new OperateResult { IsSuccess = false };
                    //        var status = mc.IpAddressPing();
                    //        mc.ReceiveTimeout = timeOut;
                    //        initResult.IsSuccess = status == IPStatus.Success;
                    //        initResult.Message = status.ToString();
                    //        readWriteDevice = mc;
                    //    }
                    //    break;
                    //case ProtocolTypeEnum.MCRSerialQna3EBinary:
                    //    {
                    //        var mc = new MelsecMcRNet(ipaddress, port);
                    //        mc.ReceiveTimeOut = timeOut;
                    //        initResult = mc.ConnectServer();
                    //        readWriteDevice = mc;
                    //    }
                    //    break;
                    //case ProtocolTypeEnum.FxSerial:
                    //    {
                    //        var mc = new MelsecFxSerial() { IsNewVersion = option.NewVersionMessage };
                    //        mc.ReceiveTimeout= timeOut;
                    //        mc.SerialPortInni(option.PortName, option.BaudRate, option.DataBits, option.StopBits, option.Parity);
                    //        initResult = mc.Open();
                    //        readWriteDevice= mc;
                    //    }
                    //    break;
                    //case ProtocolTypeEnum.FxSerialOverTcp:
                    //    {
                    //        var mc = new MelsecFxSerialOverTcp(ipaddress, port) { IsNewVersion = option.NewVersionMessage };
                    //        mc.ReceiveTimeOut= timeOut;
                    //        initResult = mc.ConnectServer();
                    //        readWriteDevice = mc;
                    //    }
                    //    break;
                    //case ProtocolTypeEnum.FxLinks485:
                    //    {
                    //        var mc = new MelsecFxLinks() { Station = option.Station, SumCheck = option.SumCheck };
                    //        mc.ReceiveTimeout= timeOut;
                    //        mc.SerialPortInni(option.PortName, option.BaudRate, option.DataBits, option.StopBits, option.Parity);
                    //        initResult = mc.Open();
                    //        readWriteDevice= mc;
                    //    }
                    //    break;
                    //case ProtocolTypeEnum.FxLinksOverTcp:
                    //    {
                    //        var mc = new MelsecFxLinksOverTcp(ipaddress, port) { Station = option.Station, SumCheck = option.SumCheck };
                    //        mc.ReceiveTimeOut= timeOut;
                    //        initResult = mc.ConnectServer();
                    //        readWriteDevice = mc;
                    //    }
                    //    break;
                    //case ProtocolTypeEnum.Qna3c:
                    //    {
                    //        var mc = new MelsecA3CNet() { Station = option.Station, SumCheck = option.SumCheck, Format = option.Format };
                    //        mc.ReceiveTimeout = timeOut;
                    //        mc.SerialPortInni(option.PortName, option.BaudRate, option.DataBits, option.StopBits, option.Parity);
                    //        initResult = mc.Open();
                    //        readWriteDevice = mc;
                    //    }
                    //    break;
                    //case ProtocolTypeEnum.Qna3cOverTcp:
                    //    {
                    //        var mc = new MelsecA3CNetOverTcp(ipaddress, port) { Station = option.Station, SumCheck = option.SumCheck, Format = option.Format };
                    //        mc.ReceiveTimeOut = timeOut;
                    //        initResult = mc.ConnectServer();
                    //        readWriteDevice = mc;
                    //    }
                    //    break;
                    default:
                        initResult = new OperateResult() { IsSuccess = false,Message = "未知协议类型!"};
                        break;
                }

                IsConnected = initResult.IsSuccess;

                if (!initResult.IsSuccess)
                    WriteToLog($"Name:{Name},Number:{Number},初始化连接失败,{initResult.Message}");
            }
            catch(Exception ex)
            {
                IsConnected = false;
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
                foreach (KpAllenBrandly.Model.TagGroup group in deviceTemplate.TagGroups)
                {
                    var tagGroup = new TagGroup(group.Name);
                    tagGroups.Add(tagGroup);
                    foreach (Tag tag in group.Tags)
                    {
                        tagGroup.AddNewTag(tag.TagID, tag.Name);
                        if (tag.DataType == DataTypeEnum.Real || tag.DataType == DataTypeEnum.LReal)
                            floatSignals.Add(tag.TagID);
                    }
                }
            }

            return tagGroups;
        }
        #endregion


        #region 自定义驱动数据请求、下发


        private bool RequestReadData(KpAllenBrandly.Model.TagGroup tagGroup)
        {
            var model = tagGroup.GetRequestModel();
            WriteToLog($"Name:{Name},Number:{Number},开始请求数据,GroupName:{tagGroup.Name},起始地址:{model.Addresses.FirstOrDefault() ?? string.Empty},请求长度:{model.Lengths.FirstOrDefault()}");
            if (!IsConnected)
            {
                WriteToLog($"Name:{Name},Number:{Number},读取数据失败,未连接到设备,连接参数,{JsonConvert.SerializeObject(deviceTemplate.ConnectionOptions)}");
                return false;
            }

            try
            {
                var result = readWriteDevice.Read(model.Addresses, model.Lengths);
                WriteToLog($"Name:{Name},Number:{Number},数据请求结束,IsSuccess:{result.IsSuccess},Message:{result.Message},Data:{result.Content.ToJsonString()}");
                if (result.IsSuccess && result.Content?.Length > 0)
                {
                    //赋值
                    tagGroup.Data = result.Content;
                }
                return result.IsSuccess;
            }
            catch (Exception ex)
            {
                WriteToLog($"KpMelsecLogic_RequestData,Name:{Name},Number:{Number},数据请求异常,{ex.Message},StackTrace:{ex.StackTrace}");
                return false;
            }
        }

        private void SetTagsData(KpAllenBrandly.Model.TagGroup tagGroup)
        {
            for(int i = 0; i < tagGroup.Tags.Count; i++)
            {
                var tag = tagGroup.Tags[i]; 
                var data = tagGroup.GetTagVal(i);
                if (data != null)
                    SetCurData(tag.TagID > 0 ? tag.TagID - 1 : tag.TagID, (double)data, BaseValues.CnlStatuses.Defined);
            }
        }

        private bool RequestWriteData(Tag tag, KpAllenBrandly.Model.TagGroup tagGroup)
        {
            if (tag.Data == null)
                return false;
            //var memoryType = tagGroup.MemoryType;
            var address = $"{tag.Name}";
            WriteToLog($"Name:{Name},Number:{Number},开始写入数据,Name:{tag.Name},地址:{tag.Name},写入值:{JsonConvert.SerializeObject(tag.Data)}");
            try
            {
                OperateResult operateResult = new OperateResult { IsSuccess = false};
                switch (tag.DataType)
                {
                    case DataTypeEnum.Bool:
                        operateResult = readWriteDevice.Write(address,(bool)tag.Data);
                        break;
                    case DataTypeEnum.Int:
                        operateResult = readWriteDevice.Write(address, (short)tag.Data);
                        break;
                    case DataTypeEnum.DInt:
                        operateResult = readWriteDevice.Write(address,(int)tag.Data);
                        break;
                    case DataTypeEnum.LInt:
                        operateResult = readWriteDevice.Write(address, (long)tag.Data);
                        break;
                    case DataTypeEnum.UInt:
                    case DataTypeEnum.Word:
                        operateResult = readWriteDevice.Write(address,(ushort)tag.Data);
                        break;
                    case DataTypeEnum.UDInt:
                    case DataTypeEnum.DWord:
                        operateResult = readWriteDevice.Write(address,(uint)tag.Data);
                        break;
                    case DataTypeEnum.ULInt:
                    case DataTypeEnum.LWord:
                        operateResult = readWriteDevice.Write(address,(ulong)tag.Data);
                        break;
                    case DataTypeEnum.Real:
                        operateResult = readWriteDevice.Write(address,(float)tag.Data);
                        break;
                    case DataTypeEnum.LReal:
                        operateResult = readWriteDevice.Write(address,(double)tag.Data);
                        break;
                    case DataTypeEnum.String:
                        operateResult = readWriteDevice.Write(address,(string)tag.Data);
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
                WriteToLog($"KpMelsecLogic_RequestWriteData,Name:{Name},Number:{Number},写入数据异常,{ex.Message},StackTrace:{ex.StackTrace}");
                return false;
            }
        }
        #endregion
    }
}
