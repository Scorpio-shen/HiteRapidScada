﻿using HslCommunication;
using HslCommunication.ModBus;
using KpHiteModbus.Modbus.Model;
using Newtonsoft.Json;
using Scada.Data.Configuration;
using Scada.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Scada.Comm.Devices
{
    public class KpHiteModbusLogic : KPLogic
    {
        protected DeviceTemplate deviceTemplate;
        private HashSet<int> floatSignals;
        private List<ModbusTagGroup> modbustagGroupsActive;
        IModbus modbus;

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
                    }
                }
            }
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
                        if ()
                    }
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
        }

        public override void OnCommLineStart()
        {
            try
            {
                var option = deviceTemplate.ConnectionOptions;
                var station = option.Station;
                OperateResult initResult = null;
                switch (option.ConnectionType)
                {
                    case KpHiteModbus.Modbus.Model.EnumType.ModbusConnectionTypeEnum.SerialPort:
                        if (option.ModbusMode == KpHiteModbus.Modbus.Model.EnumType.ModbusModeEnum.Ascii)
                        {
                            var modbusAscii = new ModbusAscii(station);
                            modbusAscii.SerialPortInni(option.PortName, option.BaudRate, option.DataBits, option.StopBits, option.Parity);
                            initResult = modbusAscii.Open();
                            if(initResult.IsSuccess)
                                modbus = modbusAscii;
                        }
                        else
                        {
                            var modbusRtu= new ModbusRtu(station);
                            modbusRtu.SerialPortInni(option.PortName, option.BaudRate, option.DataBits, option.StopBits, option.Parity);
                            initResult = modbusRtu.Open();
                            if (initResult.IsSuccess)
                                modbus = modbusRtu;
                        }
                        break;
                    case KpHiteModbus.Modbus.Model.EnumType.ModbusConnectionTypeEnum.TcpIP:
                        var modbusTcp = new ModbusTcpNet(option.IPAddress,option.Port,station);
                        initResult = modbusTcp.ConnectServer();
                        if (initResult.IsSuccess)
                            modbus = modbusTcp;
                            break;
                    case KpHiteModbus.Modbus.Model.EnumType.ModbusConnectionTypeEnum.Udp:
                        var modbusUdp = new ModbusUdpNet(option.IPAddress,option.Port,station);
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
                            initResult = modbusAsciiOverTcp.ConnectServer();
                            if(initResult.IsSuccess)
                                modbus = modbusAsciiOverTcp;
                        }
                        else
                        {
                            var modbusRtuOverTcp = new ModbusRtuOverTcp(option.IPAddress, option.Port, station);
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
                    WriteToLog($"KpHiteModbusLogic_OnCommLineStart,初始化连接失败,{initResult.Message}");
            }
            catch(Exception ex)
            {
                WriteToLog($"KpSiemensLogic_OnCommLineStart,连接PLC异常,{ex.Message}");
            }
        }

        private void InitDeviceTemplate(string fileName)
        {
            deviceTemplate = null;
            if (string.IsNullOrEmpty(fileName))
            {
                WriteToLog($"KpHiteModbusLogic_InitDeviceTemplate,初始化模板失败,找到相应模板文件");
                return;
            }

            deviceTemplate = new DeviceTemplate();
            string filePath = Path.IsPathRooted(fileName) ?
                        fileName : Path.Combine(AppDirs.ConfigDir, fileName);
            if (!deviceTemplate.Load(filePath, out string errMsg))
                WriteToLog(errMsg);
            else
            {
                //初始话Tag和Command 点位顺序和地址
                deviceTemplate.RefreshTagGroupIndex();
                foreach (var tagGroup in deviceTemplate.TagGroups)
                    tagGroup.RefreshTagAddress();

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
            WriteToLog($"开始请求数据,GroupName:{tagGroup.Name},寄存器类型:{tagGroup.RegisterType},起始地址:{model.Address},请求长度:{model.Length}");

            try
            {
                var result = modbus.Read(model.Address,model.Length);
                WriteToLog($"数据请求结束,IsSuccess:{result.IsSuccess},Message:{result.Message},Data:{result.Content.ToJsonString()}");
                if(result.IsSuccess && result.Content?.Length > 0)
                    tagGroup.Data = result.Content;

                return result.IsSuccess;
            }
            catch(Exception ex)
            {
                WriteToLog($"KpHiteModbusLogic_RequestData,数据请求异常,{ex.Message},StackTrace:{ex.StackTrace}");
                return false;
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
            var address = string.Empty;
            var functionCode = tagGroup.GetFunctionCode(true);
            address =  $"x={functionCode};{tag.Address}";

            WriteToLog($"开始写入数据,Name:{tag.Name},寄存器类型:{tag.RegisterType},地址:{tag.Address},写入值:{JsonConvert.SerializeObject(tag.Data)}");
            try
            {
                switch (tag.DataType)
                {
                    case DataTypeEnum.Bool:
                        modbus.Write(address, (bool)tag.Data);
                        break;
                        case DataTypeEnum
                }
                OperateResult result = modbus.Write
            }
        }
        #endregion
    }
}
