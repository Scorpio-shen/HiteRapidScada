﻿using HslCommunication;
using HslCommunication.Core;
using HslCommunication.Profinet.AllenBradley;
using HslCommunication.Profinet.Melsec;
using KpHiteBeckHoff.Extend;
using KpHiteBeckHoff.Model;
using KpHiteBeckHoff.Model.EnumType;
using KpCommon.Hsl.Profinet.AllenBradley.InterFace;
using KpCommon.Model;
using Newtonsoft.Json;
using Scada.Data.Configuration;
using Scada.Data.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using HslCommunication.Profinet.Beckhoff;
using KpCommon.Extend;
using static Scada.Comm.Devices.KPView;

namespace Scada.Comm.Devices
{
    public class KpHiteBeckHoffLogic : KPLogic
    {
        readonly int MaxCharCount = 256;            //倍福 PLC这种通讯方式最多一次256个字符长度的Tag
        readonly int MaxByteCount = 476;            //倍福 PLC一次支持最大读取字节数组
        protected DeviceTemplate deviceTemplate;
        private HashSet<int> floatSignals;
        private List<KpHiteBeckHoff.Model.TagGroup> tagGroupsActive;

        BeckhoffAdsNet beckhoffAdsNet;
        string templateName;
        bool IsConnected = false;

        int ActiveTagGroupCount
        {
            get=>tagGroupsActive?.Count > 0 ? tagGroupsActive.Count : 0;
        }

        public KpHiteBeckHoffLogic(int number) : base(number)
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
                KpHiteBeckHoff.Model.TagGroup tagGroup = null;
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

                #region 多种协议类型时根据协议选择实例化对象
                //switch (option.ProtocolType)
                //{
                //    case ProtocolTypeEnum.EtherNetIP:
                //        {
                //            var abNet = new AllenBradleyNet(ipaddress, port) { Slot = option.Slot};
                //            abNet.ReceiveTimeOut= timeOut;
                //            initResult = abNet.ConnectServer();
                //            readWriteDevice = abNet;
                //        }
                //        break;
                //    case ProtocolTypeEnum.MicroCip:
                //        {
                //            var abNet = new AllenBradleyMicroCip(ipaddress, port) { Slot = option.Slot};
                //            abNet.ReceiveTimeOut= timeOut;
                //            initResult = abNet.ConnectServer();
                //            readWriteDevice = abNet;
                //        }
                //        break;
                //    case ProtocolTypeEnum.ConnectedCIP:
                //        {
                //            var abNet = new AllenBradleyConnectedCipNet(ipaddress, port);
                //            abNet.ReceiveTimeOut = timeOut;
                //            initResult = abNet.ConnectServer();
                //            readWriteDevice = abNet;
                //        }
                //        break;
                //    //case ProtocolTypeEnum.SLCNet:
                //    //    {
                //    //        var abNet = new AllenBradleySLCNet(ipaddress,port);
                //    //        abNet.ReceiveTimeOut= timeOut;
                //    //        initResult = abNet.ConnectServer();
                //    //        readWriteDevice = abNet;
                //    //    }
                //    //    break;
                //    //case ProtocolTypeEnum.McQna3EBinary:
                //    //    {
                //    //        var mc = new MelsecMcNet(ipaddress,port);
                //    //        mc.ReceiveTimeOut= timeOut;
                //    //        initResult = mc.ConnectServer();
                //    //        readWriteDevice = mc;
                //    //    }
                //    //    break;
                //    //case ProtocolTypeEnum.MCQna3EBinaryUdp:
                //    //    {
                //    //        var mc = new MelsecMcUdp(ipaddress,port);
                //    //        initResult = new OperateResult { IsSuccess = false };
                //    //        var status = mc.IpAddressPing();
                //    //        mc.ReceiveTimeout= timeOut;
                //    //        initResult.IsSuccess = status == IPStatus.Success;
                //    //        initResult.Message = status.ToString();
                //    //        readWriteDevice= mc;
                //    //    }
                //    //    break;
                //    //case ProtocolTypeEnum.McQna3EASCII:
                //    //    {
                //    //        var mc = new MelsecMcAsciiNet(ipaddress,port);
                //    //        mc.ReceiveTimeOut= timeOut;
                //    //        initResult = mc.ConnectServer();
                //    //        readWriteDevice = mc;
                //    //    }
                //    //    break;
                //    //case ProtocolTypeEnum.MCQna3EASCIIUdp:
                //    //    {
                //    //        var mc = new MelsecMcAsciiUdp(ipaddress, port);
                //    //        initResult = new OperateResult { IsSuccess = false };
                //    //        var status = mc.IpAddressPing();
                //    //        mc.ReceiveTimeout = timeOut;
                //    //        initResult.IsSuccess = status == IPStatus.Success;
                //    //        initResult.Message = status.ToString();
                //    //        readWriteDevice = mc;
                //    //    }
                //    //    break;
                //    //case ProtocolTypeEnum.MCRSerialQna3EBinary:
                //    //    {
                //    //        var mc = new MelsecMcRNet(ipaddress, port);
                //    //        mc.ReceiveTimeOut = timeOut;
                //    //        initResult = mc.ConnectServer();
                //    //        readWriteDevice = mc;
                //    //    }
                //    //    break;
                //    //case ProtocolTypeEnum.FxSerial:
                //    //    {
                //    //        var mc = new MelsecFxSerial() { IsNewVersion = option.NewVersionMessage };
                //    //        mc.ReceiveTimeout= timeOut;
                //    //        mc.SerialPortInni(option.PortName, option.BaudRate, option.DataBits, option.StopBits, option.Parity);
                //    //        initResult = mc.Open();
                //    //        readWriteDevice= mc;
                //    //    }
                //    //    break;
                //    //case ProtocolTypeEnum.FxSerialOverTcp:
                //    //    {
                //    //        var mc = new MelsecFxSerialOverTcp(ipaddress, port) { IsNewVersion = option.NewVersionMessage };
                //    //        mc.ReceiveTimeOut= timeOut;
                //    //        initResult = mc.ConnectServer();
                //    //        readWriteDevice = mc;
                //    //    }
                //    //    break;
                //    //case ProtocolTypeEnum.FxLinks485:
                //    //    {
                //    //        var mc = new MelsecFxLinks() { Station = option.Station, SumCheck = option.SumCheck };
                //    //        mc.ReceiveTimeout= timeOut;
                //    //        mc.SerialPortInni(option.PortName, option.BaudRate, option.DataBits, option.StopBits, option.Parity);
                //    //        initResult = mc.Open();
                //    //        readWriteDevice= mc;
                //    //    }
                //    //    break;
                //    //case ProtocolTypeEnum.FxLinksOverTcp:
                //    //    {
                //    //        var mc = new MelsecFxLinksOverTcp(ipaddress, port) { Station = option.Station, SumCheck = option.SumCheck };
                //    //        mc.ReceiveTimeOut= timeOut;
                //    //        initResult = mc.ConnectServer();
                //    //        readWriteDevice = mc;
                //    //    }
                //    //    break;
                //    //case ProtocolTypeEnum.Qna3c:
                //    //    {
                //    //        var mc = new MelsecA3CNet() { Station = option.Station, SumCheck = option.SumCheck, Format = option.Format };
                //    //        mc.ReceiveTimeout = timeOut;
                //    //        mc.SerialPortInni(option.PortName, option.BaudRate, option.DataBits, option.StopBits, option.Parity);
                //    //        initResult = mc.Open();
                //    //        readWriteDevice = mc;
                //    //    }
                //    //    break;
                //    //case ProtocolTypeEnum.Qna3cOverTcp:
                //    //    {
                //    //        var mc = new MelsecA3CNetOverTcp(ipaddress, port) { Station = option.Station, SumCheck = option.SumCheck, Format = option.Format };
                //    //        mc.ReceiveTimeOut = timeOut;
                //    //        initResult = mc.ConnectServer();
                //    //        readWriteDevice = mc;
                //    //    }
                //    //    break;
                //    default:
                //        initResult = new OperateResult() { IsSuccess = false,Message = "未知协议类型!"};
                //        break;
                //}
                #endregion

                beckhoffAdsNet = new BeckhoffAdsNet(ipaddress, port);
                if (option.AutoAmsNetId)
                {
                    beckhoffAdsNet.UseAutoAmsNetID = true;
                    if (!string.IsNullOrEmpty(option.AmsPort))
                        beckhoffAdsNet.AmsPort = option.AmsPort.ToInt();
                }
                else
                {
                    beckhoffAdsNet.SetTargetAMSNetId(option.TaggetNetId);
                    beckhoffAdsNet.SetSenderAMSNetId(option.SenderNetId);
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

                //初始化Tag点的signal号
                int signal = 1;
                foreach (var tagGroup in deviceTemplate.TagGroups)
                {
                    foreach (var tag in tagGroup.Tags)
                    {
                        //判读是否是数组类型
                        if (tag.IsArray)
                        {
                            //数组类型
                            for (int i = 0; i < tag.Length; i++)
                                tag.Signals.Add(signal++);
                        }
                        else
                        {
                            tag.Signals.Add(signal++);
                        }
                    }
                }
            }
        }

        private List<TagGroup> CreateTagGroups(DeviceTemplate deviceTemplate)
        {
            List<TagGroup> tagGroups = new List<TagGroup>();
            if (deviceTemplate != null)
            {
                foreach (KpHiteBeckHoff.Model.TagGroup group in deviceTemplate.TagGroups)
                {
                    var tagGroup = new TagGroup(group.Name);
                    tagGroups.Add(tagGroup);
                    foreach (Tag tag in group.Tags)
                    {
                        tagGroup.AddNewTag(tag.TagID, tag.Name);
                        if (tag.DataType == DataTypeEnum.Float || tag.DataType == DataTypeEnum.Double)
                            floatSignals.Add(tag.TagID);
                    }
                }
            }

            return tagGroups;
        }
        #endregion


        #region 自定义驱动数据请求、下发


        private bool RequestReadData(KpHiteBeckHoff.Model.TagGroup tagGroup)
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
                OperateResult<byte[]> result = null;
                result = RequestUnit(beckhoffAdsNet, model);
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

        private OperateResult<byte[]> RequestUnit(BeckhoffAdsNet abNet, KpHiteBeckHoff.Model.RequestModel model)
        {
            //判断字符串长度是否超过256
            OperateResult <byte[]> result = new OperateResult<byte[]>() { IsSuccess = false};
            int index = 0;
            List<string> addresses = new List<string>();
            List<ushort> lengths = new List<ushort>();
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
                    if(byteCount > MaxByteCount && addresses.Count == 1)
                    {
                        
                    }

                    addresses.RemoveAt(addresses.Count - 1);
                    lengths.RemoveAt(lengths.Count - 1);
                    byteCounts.RemoveAt(byteCounts.Count - 1);
                    index--;
                    //进行一次数据请求
                    result = abNet.Read(addresses.ToArray(), lengths.ToArray());
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
                        result = abNet.Read(addresses.ToArray(), lengths.ToArray());
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
            while (index < model.Addresses.Count);

            result.IsSuccess= true;
            result.Content = buffer.ToArray();
            return result;
        }

        private void SetTagsData(KpHiteBeckHoff.Model.TagGroup tagGroup)
        {
            int byteIndex = default;
            var data = tagGroup.Data;
            for (int i = 0; i < tagGroup.Tags.Count; i++)
            {
                var tag = tagGroup.Tags[i];
                if (tag.IsArray)
                {
                    //数组类型处理
                    //获取Tag数组对应的字节数组
                    var byteCount = default(int);
                    if (tag.DataType == DataTypeEnum.Bool)
                    {
                        var iPart = tag.Length / 8;
                        byteCount = iPart;
                        if (tag.Length % 8 > 0)
                            byteCount += 1;
                    }
                    else
                        byteCount = tag.DataType.GetByteCount() * tag.Length;

                    //赋值父Tag
                    tag.ReadData = data.Skip(byteIndex).Take(byteCount).ToArray();
                    byteIndex += byteCount;
                }
                else
                {
                    //非数组类型处理
                    var byteCount = tag.DataType.GetByteCount();
                    if(tag.DataType == DataTypeEnum.String)
                    {
                        byteCount = 2 + 4 + tag.Length;
                    }
                    tag.ReadData = data.Skip(byteIndex).Take(byteCount).ToArray();
                    byteIndex += byteCount;
                }
            }

            int signal = 1;
            //赋值通道
            foreach(var tag in tagGroup.Tags)
            {
                if (tag.IsArray)
                {
                    var tagData = tag.ReadData;
                    if(tag.DataType == DataTypeEnum.Bool)
                    {
                        var bitArray = new BitArray(tagData);
                        for(int i =  0; i < tag.Length; i++)
                        {
                            SetCurData(signal, bitArray[i] ? 1.0 : default, BaseValues.CnlStatuses.Defined);
                            signal++;
                        }
                    }
                    else
                    {
                        var byteCount = tag.DataType.GetByteCount();
                        for (int i = 0; i < tag.Length; i++)
                        {
                            var buf = tagData.Skip(i * byteCount).Take(byteCount).ToArray();
                            GetTagVal(buf, tag.DataType);
                            signal++;
                        }
                    }
                  
                }
                else
                {
                    var tagData = tag.GetTagVal();
                    if (tagData != null)
                        SetCurData(signal, (double)tagData, BaseValues.CnlStatuses.Defined);
                    signal++;
                }
               
            }
        }

        private double? GetTagVal(byte[] buffer,DataTypeEnum datatype)
        {
            double? result = null;
            try
            {
                if (buffer == null || buffer.Length == 0)
                    return result;
                switch (datatype)
                {
                    case DataTypeEnum.Bool:
                        result = buffer[0] > 0 ? 1d : 0d;
                        break;
                    case DataTypeEnum.Byte:
                        result = buffer[0];
                        break;
                    case DataTypeEnum.SByte:
                        result = buffer[0];
                        break;
                    case DataTypeEnum.Short:
                        result = BitConverter.ToInt16(buffer, 0);
                        break;
                    case DataTypeEnum.Int:
                        result = BitConverter.ToInt32(buffer, 0);
                        break;
                    case DataTypeEnum.Long:
                        result = BitConverter.ToInt64(buffer, 0);
                        break;
                    case DataTypeEnum.UShort:
                        result = BitConverter.ToUInt16(buffer, 0);
                        break;
                    case DataTypeEnum.UInt:
                        result = BitConverter.ToUInt32(buffer, 0);
                        break;
                    case DataTypeEnum.ULong:
                        result = BitConverter.ToUInt64(buffer, 0);
                        break;
                    case DataTypeEnum.Float:
                        result = BitConverter.ToSingle(buffer, 0);
                        break;
                    case DataTypeEnum.Double:
                        result = BitConverter.ToDouble(buffer, 0);
                        break;
                    case DataTypeEnum.String:
                        //跳过头两个字节,获取int类型有效长度,在读取字符串
                        var length = BitConverter.ToInt32(buffer, 2);
                        var buf = buffer.Skip(2 + 4).Take(length).ToArray();
                        result = ScadaUtils.EncodeAscii(Encoding.ASCII.GetString(buf));
                        break;
                }
            }
            catch
            {
                result = null;
            }
            return result;
        }

        private bool RequestWriteData(Tag tag, KpHiteBeckHoff.Model.TagGroup tagGroup)
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
                        operateResult = beckhoffAdsNet.Write(address,(bool)tag.Data);
                        break;
                    case DataTypeEnum.Byte:
                        operateResult = beckhoffAdsNet.Write(address, (byte)tag.Data);
                        break;
                    case DataTypeEnum.SByte:
                        operateResult = beckhoffAdsNet.Write(address, (sbyte)tag.Data);
                        break;
                    case DataTypeEnum.Short:
                        operateResult = beckhoffAdsNet.Write(address, (short)tag.Data);
                        break;
                    case DataTypeEnum.Int:
                        operateResult = beckhoffAdsNet.Write(address, (int)tag.Data);
                        break;
                    case DataTypeEnum.Long:
                        operateResult = beckhoffAdsNet.Write(address,(long)tag.Data);
                        break;
                    case DataTypeEnum.UShort:
                        operateResult = beckhoffAdsNet.Write(address, (ushort)tag.Data);
                        break;
                    case DataTypeEnum.UInt:
                        operateResult = beckhoffAdsNet.Write(address, (uint)tag.Data);
                        break;
                    case DataTypeEnum.ULong:
                        operateResult = beckhoffAdsNet.Write(address, (ulong)tag.Data);
                        break;
                    case DataTypeEnum.Float:
                        operateResult = beckhoffAdsNet.Write(address, (float)tag.Data);
                        break;
                    case DataTypeEnum.Double:
                        operateResult = beckhoffAdsNet.Write(address, (double)tag.Data);
                        break;
                    case DataTypeEnum.String:
                        operateResult = beckhoffAdsNet.Write(address,(string)tag.Data);
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
