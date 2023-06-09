﻿using HslCommunication;
using HslCommunication.Core;
using HslCommunication.Profinet.AllenBradley;
using HslCommunication.Profinet.Omron;
using KpCommon.Hsl.Profinet.AllenBradley.InterFace;
using KpCommon.Model;
using KpOmron.Model;
using KpOmron.Model.EnumType;
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
    public class KpOmronLogic : KPLogic
    {
        readonly int MaxCharCount = 256;        //欧姆龙CIP通讯方式一次最多256字符长度的Tag
        readonly int MaxByteCount = 476;        //欧姆龙CIP通讯方式一次最大读取字节数
        protected DeviceTemplate deviceTemplate;
        private HashSet<int> floatSignals;
        private List<KpOmron.Model.TagGroup> tagGroupsActive;
        IReadWriteDevice readWriteDevice;
        string templateName;
        bool IsConnected = false;

        int ActiveTagGroupCount
        {
            get=>tagGroupsActive?.Count > 0 ? tagGroupsActive.Count : 0;
        }

        public KpOmronLogic(int number) : base(number)
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
                KpOmron.Model.TagGroup tagGroup = null;
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
                var unitnumber = option.UnitNumber;
                var sid = option.SID;
                var da2 = option.DA2;
                var sa1 = option.SA1;
                var sa2 = option.SA2;
                var slot = option.Slot; 
                OperateResult initResult = null;
                IPStatus status;
                var timeOut = ReqParams.Timeout;
                if (timeOut <= 0)
                    timeOut = DefineReadOnlyValues.DefaultRequestTimeOut;
                switch (option.ProtocolType)
                {
                    case ProtocolTypeEnum.FinsTcp:
                        {
                            var omronFinsNet = new OmronFinsNet(ipaddress, port)
                            {
                                DA2= da2
                            };
                            initResult = omronFinsNet.ConnectServer();
                            readWriteDevice = omronFinsNet;
                        }
                        break;
                    case ProtocolTypeEnum.FinsUdp:
                        {
                            var omronFinsUdp = new OmronFinsUdp(ipaddress, port) { SA1 = sa1};
                            initResult = new OperateResult { IsSuccess = false };
                            status = omronFinsUdp.IpAddressPing();
                            initResult.IsSuccess = status == IPStatus.Success;
                            initResult.Message = status.ToString();
                            readWriteDevice = omronFinsUdp;
                        }
                        break;
                        case ProtocolTypeEnum.EtherNetIP_CIP:
                        {
                            var omronCipNet = new OmronCipNet(ipaddress, port) { Slot = slot};
                            initResult = omronCipNet.ConnectServer();
                            readWriteDevice= omronCipNet;
                        }
                        break;
                        case ProtocolTypeEnum.ConnectedCIP:
                        {
                            var omronConnectedCipNet = new OmronConnectedCipNet(ipaddress, port) { };
                            initResult = omronConnectedCipNet.ConnectServer();
                            readWriteDevice= omronConnectedCipNet;
                        }
                        break;
                        case ProtocolTypeEnum.HostLinkSerial:
                        {
                            var omronHostLink = new OmronHostLink() { UnitNumber = unitnumber,SID = sid,DA2 = da2,SA2 = sa2};
                            omronHostLink.SerialPortInni(o =>
                            {
                                o.PortName = option.PortName;
                                o.BaudRate = option.BaudRate;
                                o.Parity = option.Parity;
                                o.StopBits = option.StopBits;
                                o.DataBits = option.DataBits;
                            });
                            initResult = omronHostLink.Open();
                            readWriteDevice= omronHostLink;
                        }
                        break;
                    case ProtocolTypeEnum.HostLinkOverTcp:
                        {
                            var omronHostLink = new OmronHostLinkOverTcp(ipaddress,port) { UnitNumber = unitnumber, SID = sid, DA2 = da2, SA2 = sa2 };
                            initResult = omronHostLink.ConnectServer();
                            readWriteDevice= omronHostLink;
                        }
                        break;
                    case ProtocolTypeEnum.HostLinkCMode:
                        {
                            var omronHostLink = new OmronHostLinkCMode() { UnitNumber = unitnumber };
                            omronHostLink.SerialPortInni(o =>
                            {
                                o.PortName = option.PortName;
                                o.PortName = option.PortName;
                                o.BaudRate = option.BaudRate;
                                o.Parity = option.Parity;
                                o.StopBits = option.StopBits;
                                o.DataBits = option.DataBits;
                            });
                            initResult= omronHostLink.Open();
                            readWriteDevice= omronHostLink;
                        }
                        break;
                        case ProtocolTypeEnum.CModeOverTcp:
                        {
                            var omronHostLink = new OmronHostLinkCModeOverTcp(ipaddress, port) { UnitNumber = unitnumber };
                            initResult = omronHostLink.ConnectServer();
                            readWriteDevice= omronHostLink;
                        }
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
                foreach (KpOmron.Model.TagGroup group in deviceTemplate.TagGroups)
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


        private bool RequestReadData(KpOmron.Model.TagGroup tagGroup)
        {
            var model = tagGroup.GetRequestModel();
            WriteToLog($"Name:{Name},Number:{Number},开始请求数据,GroupName:{tagGroup.Name},寄存器类型:{tagGroup.MemoryType},起始地址:{model.Address},请求长度:{model.Length}");
            if (!IsConnected)
            {
                WriteToLog($"Name:{Name},Number:{Number},读取数据失败,未连接到设备,连接参数,{JsonConvert.SerializeObject(deviceTemplate.ConnectionOptions)}");
                return false;
            }

            try
            {
                //根据协议切换读取方式
                OperateResult<byte[]> result;
                if(deviceTemplate.ConnectionOptions.ProtocolType == ProtocolTypeEnum.EtherNetIP_CIP || deviceTemplate.ConnectionOptions.ProtocolType == ProtocolTypeEnum.ConnectedCIP)
                {
                    var cipModel = tagGroup.GetCIPRequestModel();
                    result = RequestUnit(readWriteDevice as IAbReadWriteCip, cipModel);
                }
                else
                {
                     result = readWriteDevice.Read(model.Address, model.Length);
                }
              
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
                WriteToLog($"KpOmronLogic_RequestData,Name:{Name},Number:{Number},数据请求异常,{ex.Message},StackTrace:{ex.StackTrace}");
                return false;
            }
        }

        private OperateResult<byte[]> RequestUnit(IAbReadWriteCip readWriteCip,CIPRequestModel model)
        {
            //判断字符串长度是否超过256
            OperateResult<byte[]> result = new OperateResult<byte[]>() { IsSuccess = false };
            int index = 0;
            List<string> addresses = new List<string>();
            List<int> lengths = new List<int>();
            List<int> byteCounts = new List<int>();
            List<byte> buffer = new List<byte>();
            do
            {
                addresses.Add(model.Addresses[index]);
                lengths.Add(model.Lengths[index]);
                var charCount = addresses.SelectMany(a => a).Count();
                var byteCount = byteCounts.Sum();

                if (charCount > MaxCharCount || byteCount > MaxByteCount)
                {
                    //判断是否是单个数组类型超出限制范围
                    if (byteCount > MaxByteCount && addresses.Count == 1)
                    {

                    }

                    addresses.RemoveAt(addresses.Count - 1);
                    lengths.RemoveAt(lengths.Count - 1);
                    byteCounts.RemoveAt(byteCounts.Count - 1);
                    index--;
                    //进行一次数据请求
                    result = readWriteCip.Read(addresses.ToArray(), lengths.ToArray());
                    if (!result.IsSuccess)
                    {
                        WriteToLog($"Name:{Name},Number:{Number},读取数据失败,Message:{result.Message}");
                        //读取失败,添加空字节数组
                        var temp = new byte[byteCount];
                        buffer.AddRange(temp);
                    }
                    else
                        //读取成功
                        buffer.AddRange(result.Content);
                    addresses.Clear();
                    lengths.Clear();
                    byteCounts.Clear();
                }
                else
                {
                    //判断是否到最后的数组
                    if (index + 1 >= model.Addresses.Count)
                    {
                        //直接请求
                        result = readWriteCip.Read(addresses.ToArray(), lengths.ToArray());
                        if (!result.IsSuccess)
                        {
                            WriteToLog($"Name:{Name},Number:{Number},读取数据失败,Message:{result.Message}");
                            var temp = new byte[byteCount];
                            buffer.AddRange(temp);
                        }
                        else
                            //读取成功
                            buffer.AddRange(result.Content);
                        addresses.Clear();
                        lengths.Clear();
                        byteCounts.Clear();
                    }
                }
                index++;
            }
            while(index < model.Addresses.Count);

            result.IsSuccess = true;
            result.Content = buffer.ToArray();
            return result;
        }

        private void SetTagsData(KpOmron.Model.TagGroup tagGroup)
        {
            for(int i = 0; i < tagGroup.Tags.Count; i++)
            {
                var tag = tagGroup.Tags[i]; 
                var data = tagGroup.GetTagVal(i);
                if (data != null)
                    SetCurData(tag.TagID > 0 ? tag.TagID - 1 : tag.TagID, (double)data, BaseValues.CnlStatuses.Defined);
            }
        }

        private bool RequestWriteData(Tag tag, KpOmron.Model.TagGroup tagGroup)
        {
            if (tag.Data == null)
                return false;
            var address = string.Empty;
            var memoryType = tagGroup.MemoryType;
            address = $"{memoryType}{tag.Address}";
            WriteToLog($"Name:{Name},Number:{Number},开始写入数据,Name:{tag.Name},寄存器类型:{memoryType},地址:{tag.Address},写入值:{JsonConvert.SerializeObject(tag.Data)}");
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
                WriteToLog($"KpOmronLogic_RequestWriteData,Name:{Name},Number:{Number},写入数据异常,{ex.Message},StackTrace:{ex.StackTrace}");
                return false;
            }
        }
        #endregion
    }
}
