/*
 * Copyright 2021 Mikhail Shiryaev
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 * 
 * Product  : Rapid SCADA
 * Module   : KpModbus
 * Summary  : Device driver communication logic
 * 
 * Author   : Mikhail Shiryaev
 * Created  : 2012
 * Modified : 2021
 */

//using Scada.Comm.Devices.Modbus;
//using Scada.Comm.Devices.Modbus.Protocol;

using Scada.Comm.Devices.rttask;
using Scada.Comm.Devices.rttask.Protocol;
using Scada.Data.Configuration;
using Scada.Data.Models;
using Scada.Data.Tables;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text.RegularExpressions;
using System.Threading;

namespace Scada.Comm.Devices
{
    /// <summary>
    /// Device driver communication logic.
    /// <para>Логика работы драйвера КП.</para>
    /// </summary>
    public class KpRTtaskLogic : KPLogic
    {
        //string G_EPLC_FILEPATH_SHM_SOFTUNIT = @"D:\mw\program\mwC#\WinFormsApp1\WinFormsLibrary1\bin\Debug\netcoreapp3.1\shm_SoftUnit.eplc";
        //string G_EPLC_FILEPATH_SHM_SOFTUNIT = @"C:\Program Files (x86)\hite\EContrlPLC4.4\shm_SoftUnit.eplc";
        string G_EPLC_FILENAME_SHM_SOFTUNIT = "Global\\shm_SoftUnit.eplc";
        int SOFTUNIT_TOTAL_LEN = 262144;
        int SOFTUNIT_SHM_TOTAL_LEN = 1048576;  //(SOFTUNIT_TOTAL_LEN * 4)
        int SOFTUNIT_M_MAX_NUM = 262144;
        int SOFTUNIT_SPM_MAX_NUM = 65536;
        int SOFTUNIT_D_MAX_NUM = 262144;
        int SOFTUNIT_SPD_MAX_NUM = 65536;

        int SOFTUNIT_M_mmvspos;
        int SOFTUNIT_SPM_mmvspos;
        int SOFTUNIT_D_mmvspos;
        int SOFTUNIT_SPD_mmvspos;
        MemoryMappedFile mmfRttask;
        MemoryMappedViewStream mmvsRttask;

        //mw tmp
        int tick_tmp = 0;
        //tmp
        public void pause() //used for debug attached task
        {
            tick_tmp = 0;
            while (tick_tmp == 0)
            {
                Thread.Sleep(500);
            }
        }


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
        
        /// <summary>
        /// Периодичность попыток установки TCP-соединения, с
        /// </summary>
        private const int TcpConnectPer = 5;

        private TransMode transMode;                // режим передачи данных
        private ModbusPoll modbusPoll;              // объект для опроса КП
        private ModbusPoll.RequestDelegate request; // метод выполнения запроса
        private byte devAddr;                       // адрес устройства
        private List<ElemGroup> elemGroups;         // активные запрашиваемые группы элементов
        private int elemGroupCnt;                   // 有源组元素数量
        private HashSet<int> floatSignals;          // множество сигналов, форматируемых как вещественное число

        /// <summary>
        /// Шаблон устройства, используемый данным КП
        /// </summary>
        protected DeviceTemplate deviceTemplate;


        /// <summary>
        /// Конструктор
        /// </summary>
        public KpRTtaskLogic(int number)
            : base(number)
        {
            CanSendCmd = true;
            ConnRequired = false;

            //tmp
            pause();

            mmfRttask = MemoryMappedFile.CreateOrOpen(G_EPLC_FILENAME_SHM_SOFTUNIT, SOFTUNIT_SHM_TOTAL_LEN);
            mmvsRttask = mmfRttask.CreateViewStream();

            SOFTUNIT_M_mmvspos = 0;
            SOFTUNIT_SPM_mmvspos = SOFTUNIT_M_mmvspos + (SOFTUNIT_M_MAX_NUM / 8);
            SOFTUNIT_D_mmvspos = SOFTUNIT_SPM_mmvspos + (SOFTUNIT_SPM_MAX_NUM / 8);
            SOFTUNIT_SPD_mmvspos = SOFTUNIT_D_mmvspos + (SOFTUNIT_D_MAX_NUM * 2);

            byte[] data = new byte[8];
            Int32 s32_value;
            float f32_value;
            //mmvsRttask.Read(data, 0, 2);
            //retVal = BitConverter.ToInt16(data, 0);

            mmvsRttask.Position = SOFTUNIT_D_mmvspos;
            mmvsRttask.Read(data, 0, 4);
            //p_char = Encoding.ASCII.GetChars(data);
            s32_value = BitConverter.ToInt32(data, 0);
            f32_value = BitConverter.ToSingle(data, 0);
        }


        /// <summary>
        /// Gets the key of the template dictionary.
        /// </summary>
        protected virtual string TemplateDictKey
        {
            get
            {
                //return "Modbus.Templates";
                return "rttask.Templates";
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
                        deviceTemplate = GetTemplateFactory().CreateDeviceTemplate();
                        deviceTemplate.CopyFrom(existingTemplate);
                    }
                }
                else
                {
                    DeviceTemplate newTemplate = GetTemplateFactory().CreateDeviceTemplate();
                    WriteToLog(string.Format(Localization.UseRussian ?
                        "{0} Загрузка шаблона устройства из файла {1}" :
                        "{0} Load device template from file {1}", CommUtils.GetNowDT(), fileName));
                    string filePath = Path.IsPathRooted(fileName) ?
                        fileName : Path.Combine(AppDirs.ConfigDir, fileName);

                    //mw
                    DataTable dt = new DataTable();
                    if (newTemplate.Load(filePath, out string errMsg, ref dt))
                        deviceTemplate = newTemplate;
                    else
                        WriteToLog(errMsg);

                    templateDict.Add(fileName, deviceTemplate);
                }
            }
        }

        /// <summary>
        /// Initializes an object for polling data.
        /// </summary>
        private void InitShmPoll()
        {
            string plc_adTp, plc_addr, rttask_tp;

            if (deviceTemplate != null)
            {
                deviceTemplate.dtShm.Clear();
                deviceTemplate.dtShm.Columns.Add("plc_addr_type", typeof(Int32)); //PLC地址类型
                deviceTemplate.dtShm.Columns.Add("plc_addr", typeof(Int32)); //PLC数据地址
                deviceTemplate.dtShm.Columns.Add("rttask_type", typeof(Int32)); //RTTASK数据类型

                foreach (DataRow dr in deviceTemplate.dtElems.Rows)
                {
                    DataRow drShm = deviceTemplate.dtShm.NewRow();
                    plc_adTp = Regex.Replace((string )dr["变量地址"], @"[0-9]+", "");
                    plc_addr = Regex.Replace((string)dr["变量地址"], @"[^0-9]+", "");
                    rttask_tp = (string)dr["RTTASK数据类型"];
                    switch (plc_adTp)
                    {
                        case RTTASK_BaseValues.PLC_addr_types_text.addrM:
                            drShm["plc_addr_type"] = RTTASK_BaseValues.PLC_addr_types.addrM;
                            drShm["plc_addr"] = Convert.ToInt32(plc_addr);
                            break;

                        case RTTASK_BaseValues.PLC_addr_types_text.addrSPM:
                            drShm["plc_addr_type"] = RTTASK_BaseValues.PLC_addr_types.addrSPM;
                            drShm["plc_addr"] = Convert.ToInt32(plc_addr);
                            break;

                        case RTTASK_BaseValues.PLC_addr_types_text.addrD:
                            drShm["plc_addr_type"] = RTTASK_BaseValues.PLC_addr_types.addrD;
                            drShm["plc_addr"] = Convert.ToInt32(plc_addr);
                            break;

                        case RTTASK_BaseValues.PLC_addr_types_text.addrSPD:
                            drShm["plc_addr_type"] = RTTASK_BaseValues.PLC_addr_types.addrSPD;
                            drShm["plc_addr"] = Convert.ToInt32(plc_addr);

                            break;

                        default:
                            //some error message
                            break;
                    }

                    switch (rttask_tp)
                    {
                        case RTTASK_BaseValues.RTTASK_data_types_text.BOOL:
                            drShm["rttask_type"] = RTTASK_BaseValues.RTTASK_data_types.BOOL;
                            break;

                        case RTTASK_BaseValues.RTTASK_data_types_text.U16:
                            drShm["rttask_type"] = RTTASK_BaseValues.RTTASK_data_types.U16;
                            break;

                        case RTTASK_BaseValues.RTTASK_data_types_text.U32:
                            drShm["rttask_type"] = RTTASK_BaseValues.RTTASK_data_types.U32;
                            break;

                        case RTTASK_BaseValues.RTTASK_data_types_text.S16:
                            drShm["rttask_type"] = RTTASK_BaseValues.RTTASK_data_types.S16;
                            break;

                        case RTTASK_BaseValues.RTTASK_data_types_text.S32:
                            drShm["rttask_type"] = RTTASK_BaseValues.RTTASK_data_types.S32;
                            break;

                        case RTTASK_BaseValues.RTTASK_data_types_text.FLOAT:
                            drShm["rttask_type"] = RTTASK_BaseValues.RTTASK_data_types.FLOAT;
                            break;

                        case RTTASK_BaseValues.RTTASK_data_types_text.DOUBLE:
                            drShm["rttask_type"] = RTTASK_BaseValues.RTTASK_data_types.DOUBLE;
                            break;

                        default:
                            //some error message
                            break;
                    }
                    deviceTemplate.dtShm.Rows.Add(drShm);
                }

                deviceTemplate.WriteToLog = WriteToLog;
            }
        }

        /// <summary>
        /// Initializes an object for polling data.
        /// </summary>
        private void InitModbusPoll()
        {
            if (deviceTemplate != null)
            {
                // find the required size of the input buffer
                int inBufSize = 0;
                foreach (ElemGroup elemGroup in elemGroups)
                {
                    if (inBufSize < elemGroup.RespAduLen)
                        inBufSize = elemGroup.RespAduLen;
                }

                foreach (ModbusCmd cmd in deviceTemplate.Cmds)
                {
                    if (inBufSize < cmd.RespAduLen)
                        inBufSize = cmd.RespAduLen;
                }

                // create an object for polling data
                modbusPoll = new ModbusPoll(inBufSize)
                {
                    Timeout = ReqParams.Timeout,
                    Connection = Connection,
                    WriteToLog = WriteToLog
                };
            }
        }

        /// <summary>
        /// Установить окончание строки в соединении для режима ASCII
        /// 设置ASCII模式连接中的字符串的末尾
        /// </summary>
        private void SetNewLine()
        {
            if (Connection != null && transMode == TransMode.ASCII)
                Connection.NewLine = ModbusUtils.CRLF;
        }

        //mw
        //获取共享内存中的值
        public double GetElemVal(int elemInd, DataRow dr)
        {
            byte[] data = new byte[8];
            Int32 s32_val = 0;
            double retVal = 0.0;
            Int32 addr = 0;
            Int32 offset = 0;
            Int32 addr_type = (Int32)dr["plc_addr_type"];

            switch (addr_type)
            {
                case RTTASK_BaseValues.PLC_addr_types.addrM:
                    addr = (Int32)dr["plc_addr"] / 8;
                    offset = (Int32)dr["plc_addr"] % 8;
                    mmvsRttask.Position = SOFTUNIT_M_mmvspos + addr;
                    break;

                case RTTASK_BaseValues.PLC_addr_types.addrSPM:
                    addr = (Int32)dr["plc_addr"] / 8;
                    offset = (Int32)dr["plc_addr"] % 8;
                    mmvsRttask.Position = SOFTUNIT_SPM_mmvspos + addr;
                    mmvsRttask.Read(data, 0, 4);
                    s32_val = (BitConverter.ToInt32(data, 0) & (1 << offset));
                    break;

                case RTTASK_BaseValues.PLC_addr_types.addrD:
                    addr = (Int32)dr["plc_addr"] * 2;
                    mmvsRttask.Position = SOFTUNIT_D_mmvspos + addr;
                    break;

                case RTTASK_BaseValues.PLC_addr_types.addrSPD:
                    addr = (Int32)dr["plc_addr"] * 2;
                    mmvsRttask.Position = SOFTUNIT_SPD_mmvspos + addr;
                    break;

                default:
                    //error msg
                    break;
            }


            switch (dr["rttask_type"])
            {
                case RTTASK_BaseValues.RTTASK_data_types.BOOL:
                    mmvsRttask.Read(data, 0, 4);
                    s32_val = (BitConverter.ToInt32(data, 0) & (1 << offset));
                    retVal = s32_val;
                    break;

                case RTTASK_BaseValues.RTTASK_data_types.U16:
                    mmvsRttask.Read(data, 0, 2);
                    retVal = BitConverter.ToUInt16(data, 0);
                    break;

                case RTTASK_BaseValues.RTTASK_data_types.U32:
                    mmvsRttask.Read(data, 0, 4);
                    retVal = BitConverter.ToUInt32(data, 0);
                    break;

                case RTTASK_BaseValues.RTTASK_data_types.S16:
                    mmvsRttask.Read(data, 0, 2);
                    retVal = BitConverter.ToInt16(data, 0);
                    break;

                case RTTASK_BaseValues.RTTASK_data_types.S32:
                    mmvsRttask.Read(data, 0, 4);
                    retVal = BitConverter.ToInt32(data, 0);
                    break;

                case RTTASK_BaseValues.RTTASK_data_types.FLOAT:
                case RTTASK_BaseValues.RTTASK_data_types.DOUBLE:    //PLC 底层无double类型
                    mmvsRttask.Read(data, 0, 4);
                    retVal = BitConverter.ToSingle(data, 0);
                    //Data = BitConverter.GetBytes((float)cmdVal);
                    break;

                default:
                    //error msg
                    break;
            }

            return retVal;
        }

        //mw
        /// <summary>
        /// Установить значения тегов КП в соответствии со значениями элементов группы
        /// </summary>
        private void SetTagsData(DataTable dt)
        {
            int i = 0;
            foreach (DataRow dr in dt.Rows)
            {
                SetCurData(i, GetElemVal(i, dr), BaseValues.CnlStatuses.Defined);
                i++;
            }
        }

        /// <summary>
        /// Установить значения тегов КП в соответствии со значениями элементов группы
        /// </summary>
        private void SetTagsData(ElemGroup elemGroup)
        {
            for (int i = 0, j = elemGroup.StartKPTagInd + i, cnt = elemGroup.Elems.Count; i < cnt; i++, j++)
            {
                SetCurData(j, elemGroup.GetElemVal(i), BaseValues.CnlStatuses.Defined);
            }
        }


        /// <summary>
        /// Gets a device template factory.
        /// </summary>
        protected virtual DeviceTemplateFactory GetTemplateFactory()
        {
            return KpUtils.TemplateFactory;
        }

        /// <summary>
        /// Creates tag groups according to the specified template.
        /// </summary>
        protected virtual List<TagGroup> CreateTagGroups(DeviceTemplate deviceTemplate, ref int tagInd)
        {
            List<TagGroup> tagGroups = new List<TagGroup>();

            if (deviceTemplate != null)
            {
                //mw,todo
                TagGroup tagGroup = new TagGroup("rttask_group");
                tagGroups.Add(tagGroup);
                deviceTemplate.StartKPTagInd = tagInd;
                foreach (DataRow dr in deviceTemplate.dtElems.Rows)
                {
                    int signal = ++tagInd;
                    tagGroup.AddNewTag(signal, (string)dr["变量名"]);

                    //可能要做处理2022.4.5
                    if ((string)dr["RTTASK数据类型"] == "FLOAT" || (string)dr["RTTASK数据类型"] == "DOUBLE")
                        floatSignals.Add(signal);
                }

                //foreach (ElemGroup elemGroup in deviceTemplate.ElemGroups)
                //{
                //    TagGroup tagGroup = new TagGroup(elemGroup.Name);
                //    tagGroups.Add(tagGroup);
                //    elemGroup.StartKPTagInd = tagInd;

                //    foreach (Elem elem in elemGroup.Elems)
                //    {
                //        int signal = ++tagInd;
                //        tagGroup.AddNewTag(signal, elem.Name);

                //        if (elem.ElemType == ElemType.Float || elem.ElemType == ElemType.Double)
                //            floatSignals.Add(signal);
                //    }
                //}
            }

            return tagGroups;
        }

        /// <summary>
        /// Преобразовать данные тега КП в строку
        /// 将标记数据转换为行
        /// </summary>
        protected override string ConvertTagDataToStr(int signal, SrezTableLight.CnlData tagData)
        {
            if (tagData.Stat > 0 && !floatSignals.Contains(signal))
                return tagData.Val.ToString("F0");
            else
                return base.ConvertTagDataToStr(signal, tagData);
        }


        /// <summary>
        /// Выполнить сеанс опроса КП
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
            else if (deviceTemplate.dtElems.Rows.Count > 0)
            {
                //todo：读取共享内存中的数据，也可以实际赋值的时候读
                //tmp
                WriteToLog("mw Session:deviceTemplate.dtElems.Rows.Count > 0");

                lastCommSucc = false;
                int tryNum = 0;
                while (RequestNeeded(ref tryNum))
                {
                    SetTagsData(deviceTemplate.dtShm); // установка значений тегов КП
                    FinishRequest();
                    tryNum++;
                }

                lastCommSucc = true;
            }
            else
            {
                WriteToLog(Localization.UseRussian ?
                    "Отсутствуют элементы для запроса" :
                    "No elements for request");
                Thread.Sleep(ReqParams.Delay);
            }


            //if (deviceTemplate == null)
            //{
            //    WriteToLog(Localization.UseRussian ? 
            //        "Нормальное взаимодействие с КП невозможно, т.к. шаблон устройства не загружен" :
            //        "Normal device communication is impossible because device template has not been loaded");
            //    Thread.Sleep(ReqParams.Delay);
            //    lastCommSucc = false;
            //}
            //else if (elemGroupCnt > 0)
            //{
            //    // выполнение запросов по группам элементов
            //    // 完成元素组的请求
            //    int elemGroupInd = 0;
            //    while (elemGroupInd < elemGroupCnt && lastCommSucc)
            //    {
            //        ElemGroup elemGroup = elemGroups[elemGroupInd];
            //        lastCommSucc = false;
            //        int tryNum = 0;

            //        while (RequestNeeded(ref tryNum))
            //        {
            //            // выполнение запроса
            //            // 请求执行
            //            if (request(elemGroup))
            //            {
            //                lastCommSucc = true;
            //                SetTagsData(elemGroup); // установка значений тегов КП,设置KP标签的值
            //            }

            //            // завершение запроса
            //            // 完成请求
            //            FinishRequest();
            //            tryNum++;
            //        }

            //        if (lastCommSucc)
            //        {
            //            // переход к следующей группе элементов
            //            // 转到下一组元素
            //            elemGroupInd++;
            //        }
            //        else if (tryNum > 0)
            //        {
            //            // установка неопределённого статуса тегов КП текущей и следующих групп, если запрос неудачный
            //            // 如果请求不成功，请安装当前和以下组的CP标签的未定义状态
            //            while (elemGroupInd < elemGroupCnt)
            //            {
            //                elemGroup = elemGroups[elemGroupInd];
            //                InvalidateCurData(elemGroup.StartKPTagInd, elemGroup.Elems.Count);
            //                elemGroupInd++;
            //            }
            //        }
            //    }
            //}
            //else
            //{
            //    WriteToLog(Localization.UseRussian ?
            //        "Отсутствуют элементы для запроса" : 
            //        "No elements for request");
            //    Thread.Sleep(ReqParams.Delay);
            //}

            // расчёт статистики
            //计算统计
            CalcSessStats();
        }

        /// <summary>
        /// Отправить команду ТУ
        /// </summary>
        public override void SendCmd(Command cmd)
        {
            base.SendCmd(cmd);

            if (CanSendCmd)
            {
                DataRow drCmd = deviceTemplate.FindShmCmd(cmd.CmdNum);

                if (drCmd != null)
                {
                    deviceTemplate.SetCmdData(cmd.CmdVal, drCmd);
                    //todo:写入共享内存
                    //tmp
                    WriteToLog("mw SendCmd:drCmd != null");
                    //deviceTemplate.writeToShm(...);

                    lastCommSucc = true;
                }
                else
                {
                    lastCommSucc = false;
                    WriteToLog(CommPhrases.IllegalCommand);
                }
            }



            //if (CanSendCmd)
            //{
            //    ModbusCmd modbusCmd = deviceTemplate.FindCmd(cmd.CmdNum);

            //    if (modbusCmd != null &&
            //        (modbusCmd.Multiple && (cmd.CmdTypeID == BaseValues.CmdTypes.Standard || 
            //        cmd.CmdTypeID == BaseValues.CmdTypes.Binary) ||
            //        !modbusCmd.Multiple && cmd.CmdTypeID == BaseValues.CmdTypes.Standard))
            //    {
            //        // формирование команды Modbus
            //        if (modbusCmd.Multiple)
            //        {
            //            modbusCmd.Value = 0;

            //            if (cmd.CmdTypeID == BaseValues.CmdTypes.Standard)
            //                modbusCmd.SetCmdData(cmd.CmdVal);
            //            else
            //                modbusCmd.Data = cmd.CmdData;
            //        }
            //        else
            //        {
            //            modbusCmd.Value = modbusCmd.TableType == TableType.HoldingRegisters ?
            //                (ushort)cmd.CmdVal :
            //                cmd.CmdVal > 0 ? (ushort)1 : (ushort)0;
            //            modbusCmd.SetCmdData(cmd.CmdVal);
            //        }

            //        modbusCmd.InitReqPDU();
            //        modbusCmd.InitReqADU(devAddr, transMode);

            //        // отправка команды устройству
            //        lastCommSucc = false;
            //        int tryNum = 0;

            //        while (RequestNeeded(ref tryNum))
            //        {
            //            // выполнение запроса
            //            if (request(modbusCmd))
            //                lastCommSucc = true;

            //            // завершение запроса
            //            FinishRequest();
            //            tryNum++;
            //        }
            //    }
            //    else
            //    {
            //        lastCommSucc = false;
            //        WriteToLog(CommPhrases.IllegalCommand);
            //    }
            //}

            // расчёт статистики
            //计算统计
            CalcCmdStats();
        }

        /// <summary>
        /// Выполнить действия после добавления КП на линию связи
        /// 在每个通信线路添加KP后执行操作
        /// </summary>
        public override void OnAddedToCommLine()
        {
            // получение или загрузка шаблона устройства
            //获取或下载设备模板
            string fileName = ReqParams.CmdLine.Trim();
            PrepareTemplate(fileName);

            // инициализация тегов КП на основе шаблона устройства
            //基于设备模板的KP标签初始化
            floatSignals = new HashSet<int>();
            int tagInd = 0;
            List<TagGroup> tagGroups = CreateTagGroups(deviceTemplate, ref tagInd);
            InitKPTags(tagGroups);

            //可以不需要
            // определение режима передачи данных
            transMode = CustomParams.GetEnumParam("TransMode", false, TransMode.RTU);

            if (deviceTemplate == null)
            {
                elemGroups = null;
                elemGroupCnt = 0;
            }
            else
            {
                //todo:
                //init input

                //init ouput
                foreach (DataRow dr in deviceTemplate.dtElems.Rows)
                {

                }


                // получение активных групп элементов
                //获取活动组元素组
                //elemGroups = deviceTemplate.GetActiveElemGroups();
                //elemGroupCnt = elemGroups.Count;

                //// формирование PDU и ADU
                ////形成PDU和ADU
                //devAddr = (byte)Address;
                //foreach (ElemGroup elemGroup in elemGroups)
                //{
                //    elemGroup.InitReqPDU();
                //    elemGroup.InitReqADU(devAddr, transMode);
                //}

                //foreach (ModbusCmd cmd in deviceTemplate.Cmds)
                //{
                //    cmd.InitReqPDU();
                //    cmd.InitReqADU(devAddr, transMode);
                //}
            }


            // определение возможности отправки команд
            //确定发送命令的能力
            CanSendCmd = deviceTemplate != null && deviceTemplate.Cmds.Count > 0;
        }

        /// <summary>
        /// Выполнить действия при запуске линии связи
        /// 启动链接时执行操作
        /// </summary>
        public override void OnCommLineStart()
        {
            InitShmPoll();



            // инициализация объекта для опроса КП
            //InitModbusPoll();

            // выбор метода запроса
            //request = modbusPoll.GetRequestMethod(transMode);
        }

        /// <summary>
        /// Выполнить действия после установки соединения
        /// 安装连接后执行操作
        /// </summary>
        public override void OnConnectionSet()
        {
            if (deviceTemplate != null)
            {

            }



            //SetNewLine();

            //if (modbusPoll != null)
            //    modbusPoll.Connection = Connection;
        }
    }
}
