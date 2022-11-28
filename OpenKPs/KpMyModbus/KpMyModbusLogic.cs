using HslCommunication;
using HslCommunication.ModBus;
using KpMyModbus.Modbus;
using KpMyModbus.Modbus.Protocol;
using Newtonsoft.Json;
using Scada.Comm.Channels;
using Scada.Data.Configuration;
using Scada.Data.Models;
using Scada.Data.Tables;
using Scada.Extend;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Utils;

namespace Scada.Comm.Devices
{
    public class KpMyModbusLogic : KPLogic
    {
        /// <summary>
        /// Словарь шаблонов устройств для общих свойств линии связи
        /// </summary>
        private class TemplateDict : Dictionary<string, DeviceTemplate>
        {
            public override string ToString()
            {
                return "Dictionary of " + Count + " templates";
            }
        }

        private const int TcpConnectPer = 5;

        private TransMode transMode;                    // 数据传输模式
        private byte devAddr;                           // 设备地址
        private List<MyTagGroup> mytagGroups;               // 活动请求的项目组
        private int tagGroupCount;                      // 活动项目组的数量
        private HashSet<int> floatSignals;              // 一组格式化为实数的信号
        private IModbus modbus;                         //Modbus数据访问对象

        //protected Log.WriteLineDelegate WriteToLog;     //日志记录
        protected DeviceTemplate deviceTemplate;


        /// <summary>
        /// Конструктор
        /// </summary>
        public KpMyModbusLogic(int number)
            : base(number)
        {
            ConnRequired = false;//不用原有连接,LineParameter配置中选择Undefined
        }

        /// <summary>
        /// Creates tag groups according to the specified template.
        /// </summary>
        protected virtual List<TagGroup> CreateTagGroups(DeviceTemplate deviceTemplate, ref int tagInd)
        {
            List<TagGroup> tagGroups = new List<TagGroup>();

            if (deviceTemplate != null)
            {
                foreach (MyTagGroup myTagGroup in deviceTemplate.TagGroups)
                {
                    TagGroup tagGroup = new TagGroup(myTagGroup.Name);
                    tagGroups.Add(tagGroup);
                    myTagGroup.StartKPTagIndex = tagInd;

                    foreach (Tag elem in myTagGroup.Tags)
                    {
                        int signal = ++tagInd;
                        tagGroup.AddNewTag(signal, elem.Name);

                        if (elem.DataType == TagDataType.Float || elem.DataType == TagDataType.Double)
                            floatSignals.Add(signal);
                    }
                }
            }

            return tagGroups;
        }

        /// <summary>
        /// Преобразовать данные тега КП в строку
        /// </summary>
        protected override string ConvertTagDataToStr(int signal, SrezTableLight.CnlData tagData)
        {
            if (tagData.Stat > 0 && !floatSignals.Contains(signal))
                return tagData.Val.ToString("F0");
            else
                return base.ConvertTagDataToStr(signal, tagData);
        }


        /// <summary>
        /// 轮询通讯点
        /// </summary>
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
            else if (tagGroupCount > 0)
            {
                // выполнение запросов по группам элементов
                int elemGroupInd = 0;
                while (elemGroupInd < tagGroupCount && lastCommSucc)
                {
                    MyTagGroup tagGroup = mytagGroups[elemGroupInd];
                    lastCommSucc = false;
                    int tryNum = 0;

                    while (RequestNeeded(ref tryNum))
                    {
                        // выполнение запроса
                        if (RequestReadData(tagGroup))
                        {
                            lastCommSucc = true;
                            SetTagsData(tagGroup); // установка значений тегов КП
                        }

                        // завершение запроса
                        FinishRequest();
                        tryNum++;
                    }

                    if (lastCommSucc)
                    {
                        // переход к следующей группе элементов
                        elemGroupInd++;
                    }
                    else if (tryNum > 0)
                    {
                        // установка неопределённого статуса тегов КП текущей и следующих групп, если запрос неудачный
                        while (elemGroupInd < tagGroupCount)
                        {
                            tagGroup = mytagGroups[elemGroupInd];
                            InvalidateCurData(tagGroup.StartKPTagIndex, tagGroup.Tags.Count);
                            elemGroupInd++;
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

            // расчёт статистики
            CalcSessStats();
        }

        /// <summary>
        /// 指令下发
        /// </summary>
        /// <param name="cmd"></param>
        public override void SendCmd(Command cmd)
        {
            base.SendCmd(cmd);

            if (CanSendCmd)
            {
                ModbusCmd modbusCmd = deviceTemplate.FindCmd(cmd.CmdNum);

                if (modbusCmd != null &&
                    (modbusCmd.Multiple && (cmd.CmdTypeID == BaseValues.CmdTypes.Standard ||
                    cmd.CmdTypeID == BaseValues.CmdTypes.Binary) ||
                    !modbusCmd.Multiple && cmd.CmdTypeID == BaseValues.CmdTypes.Standard))
                {
                    // формирование команды Modbus
                    if (modbusCmd.Multiple)
                    {
                        modbusCmd.Value = 0;

                        if (cmd.CmdTypeID == BaseValues.CmdTypes.Standard)
                            modbusCmd.SetCmdData(cmd.CmdVal);
                        else
                            modbusCmd.WriteData = cmd.CmdData;
                    }
                    else
                    {
                        modbusCmd.Value = modbusCmd.ModbusRegisterType == ModbusRegisterType.HoldingRegisters ?
                            (ushort)cmd.CmdVal :
                            cmd.CmdVal > 0 ? (ushort)1 : (ushort)0;
                        modbusCmd.SetCmdData(cmd.CmdVal);
                    }


                    // отправка команды устройству
                    lastCommSucc = false;
                    int tryNum = 0;
                    
                    while (RequestNeeded(ref tryNum))
                    {
                        // выполнение запроса
                        if (RequestWriteData(modbusCmd))
                            lastCommSucc = true;

                        // завершение запроса
                        FinishRequest();
                        tryNum++;
                    }
                }
                else
                {
                    lastCommSucc = false;
                    WriteToLog(CommPhrases.IllegalCommand);
                }
            }

            // расчёт статистики
            CalcCmdStats();
        }

        /// <summary>
        /// KPLogic对象创建
        /// </summary>
        public override void OnAddedToCommLine()
        {
            // получение или загрузка шаблона устройства
            string fileName = ReqParams.CmdLine.Trim();
            PrepareTemplate(fileName);

            // инициализация тегов КП на основе шаблона устройства
            floatSignals = new HashSet<int>();
            int tagInd = 0;
            List<TagGroup> tagGroups = CreateTagGroups(deviceTemplate, ref tagInd);
            InitKPTags(tagGroups);

            //载入模板Template
            mytagGroups = deviceTemplate.GetActiveTagGroups();
            tagGroupCount = mytagGroups.Count;

            CanSendCmd = deviceTemplate != null && deviceTemplate.ModbusCmds.Count > 0;
        }

        /// <summary>
        /// KPLogic对象创建后，准备轮询前
        /// </summary>
        public override void OnCommLineStart()
        {
            if (deviceTemplate != null)
            {
                //初始化连接Modbus连接对象
                try
                {
                    var option = deviceTemplate.ConnectionOptions;
                    ModbusTcpNet modbusTcp = new ModbusTcpNet(option.ServerIpAddress, option.ServerPort.ToInt(), (byte)Address);
                    modbusTcp.ConnectServer();
                    modbus = modbusTcp;
                }
                catch(Exception ex)
                {
                    WriteToLog($"KpMyModbusLogic_OnCommLineStart,连接Modbus异常,{ex.Message}");
                }

            }
        }

        /// <summary>
        /// Gets the key of the template dictionary.
        /// </summary>
        protected virtual string TemplateDictKey
        {
            get
            {
                return "Modbus.Templates";
            }
        }

        /// <summary>
        /// Gets or creates the template dictionary from the common line properties.
        /// </summary>
        private TemplateDict GetTemplateDictionary()
        {
            TemplateDict templateDict = CommonProps.ContainsKey(TemplateDictKey) ?
                CommonProps[TemplateDictKey] as TemplateDict : null;

            if (templateDict == null)
            {
                templateDict = new TemplateDict();
                CommonProps.Add(TemplateDictKey, templateDict);
            }

            return templateDict;
        }

        /// <summary>
        /// Gets existing or create a new device template.
        /// </summary>
        private void PrepareTemplate(string fileName)
        {
            deviceTemplate = null;

            if (string.IsNullOrEmpty(fileName))
            {
                WriteToLog(string.Format(Localization.UseRussian ?
                    "{0} Ошибка: Не задан шаблон устройства для {1}" :
                    "{0} Error: Template is undefined for the {1}", CommUtils.GetNowDT(), Caption));
            }
            else
            {
                TemplateDict templateDict = GetTemplateDictionary();

                if (templateDict.TryGetValue(fileName, out DeviceTemplate existingTemplate))
                {
                    if (existingTemplate != null)
                    {
                        deviceTemplate = new DeviceTemplate();
                        deviceTemplate.CopyFrom(existingTemplate);
                    }
                }
                else
                {
                    DeviceTemplate newTemplate = new DeviceTemplate();
                    WriteToLog(string.Format(Localization.UseRussian ?
                        "{0} Загрузка шаблона устройства из файла {1}" :
                        "{0} Load device template from file {1}", CommUtils.GetNowDT(), fileName));
                    string filePath = Path.IsPathRooted(fileName) ?
                        fileName : Path.Combine(AppDirs.ConfigDir, fileName);

                    if (newTemplate.Load(filePath, out string errMsg))
                        deviceTemplate = newTemplate;
                    else
                        WriteToLog(errMsg);

                    templateDict.Add(fileName, deviceTemplate);
                }
            }
        }

        private void SetTagsData(MyTagGroup tagGroup)
        {
            try
            {
                int index = 0;
                for (int i = 0, j = tagGroup.StartKPTagIndex + i; i < tagGroup.Tags.Count; i++)
                {
                    index = i;
                    j = tagGroup.StartKPTagIndex + i;
                    Debug.WriteLine($"{DateTime.Now},{index}");
                    SetCurData(j, tagGroup.GetTagVal(i), BaseValues.CnlStatuses.Defined);
                }
                
            }
           catch(Exception ex)
            {
                WriteToLog($"KpMyModbusLogic_SetTagsData异常:{ex.Message}");
            }
        }

        #region 自定义驱动数据请求

        private bool RequestReadData(DataUnit dataUnit)
        {
            MyTagGroup tagGroup = dataUnit as MyTagGroup;
            if(tagGroup == null)
            {
                WriteToLog($"KpMyModbusLogic_RequestReadData,请求失败,无法转为MyTagGroup类型");
                return false;   
            }
            var byteCount =(ushort)dataUnit.RequestByteCount;
            var readAddress = $"x={dataUnit.FuncCode};{dataUnit.Address}";
            var readTagCount = (ushort)tagGroup?.Tags.Count;
            WriteToLog($"KpMyModbusLogic_RequestReadData,数据请求起始地址:{dataUnit.Address},请求字节长度:{byteCount},通讯点数:{readTagCount},寄存器类型:{dataUnit.ModbusRegisterType.GetDescription()}");
            
            var result = modbus.Read(readAddress, readTagCount);
            WriteToLog($"KpMyModbusLogic_RequestReadData,请求结果:{result.IsSuccess},数据:{result.ToJsonString()}");
            if (!result.IsSuccess)
               return false;

            //数据进行赋值
            if(dataUnit.DecodeResponseContent(result.Content,out string errMsg))
            {
                WriteToLog(ModbusPhrases.OK);
                return true;
            }
            else
            {
                WriteToLog(errMsg + "!");
                return false;
            }
        }

        private bool RequestWriteData(ModbusCmd cmd)
        {
            WriteToLog($"KpMyModbusLogic_RequestWriteData,数据请求起始地址:{cmd.Address},寄存器类型:{cmd.ModbusRegisterType.GetDescription()},写入值:{JsonConvert.SerializeObject(cmd.WriteData)}");
            OperateResult result = Write(modbus, cmd.Address.ToString(), cmd.WriteData);
            WriteToLog($"KpMyModbusLogic_RequestData,请求结果:{result.IsSuccess},Message:{result.Message}");

            return result.IsSuccess;
            
        }

        private OperateResult Write(IModbus modbus,string address,dynamic value)
        {
            OperateResult operateResult = new OperateResult()
            {
                ErrorCode = -100,
                IsSuccess = false,
                Message = "IModbus实现类型未知,无法进行转换操作"
            };
            var modbustypeName = modbus.GetType().Name;
            if (modbustypeName.Equals(typeof(ModbusTcpNet).Name))
            {
                ModbusTcpNet modbusTcpNet = modbus as ModbusTcpNet;
               operateResult = modbusTcpNet.Write(address,value);
            }

            return operateResult;
        }
        #endregion
    }
}
