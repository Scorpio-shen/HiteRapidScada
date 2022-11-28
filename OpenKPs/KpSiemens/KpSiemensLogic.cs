using HslCommunication;
using HslCommunication.Profinet.Siemens;
using KpSiemens.Siemens.Extend;
using KpSiemens.Siemens.Model;
using Newtonsoft.Json;
using Scada.Data.Configuration;
using Scada.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Scada.Comm.Devices
{
    public class KpSiemensLogic : KPLogic
    {
        protected DeviceTemplate deviceTemplate;
        private HashSet<int> floatSignals;                          //浮点类型读取点键值对集合
        private List<SiemensTagGroup> siemensTagGroupsActive;       //Active的读取通讯点
        private int activeTagGroupsCount;                           //Active的Group数
        private SiemensS7Net siemensS7Net;

        public KpSiemensLogic(int number) : base(number)
        {
            ConnRequired = false;
        }

        #region Override函数
        #region 定时轮询与指令发送
        public override void Session()
        {
            base.Session();
            if(deviceTemplate == null)
            {
                WriteToLog(Localization.UseRussian ?
                    "Нормальное взаимодействие с КП невозможно, т.к. шаблон устройства не загружен" :
                    "Normal device communication is impossible because device template has not been loaded");
                Thread.Sleep(ReqParams.Delay);
                lastCommSucc = false;
            }
            else if(activeTagGroupsCount > 0)
            {
                int tagGroupIndex = 0;
                while(tagGroupIndex < activeTagGroupsCount && lastCommSucc)
                {
                    var tagGroup = siemensTagGroupsActive[tagGroupIndex];
                    lastCommSucc = false;
                    int tryNum = 0;

                    while(RequestNeeded(ref tryNum))
                    {
                        if (RequestReadData(tagGroup))
                        {
                            lastCommSucc = true;
                            //通道、Tag单点赋值
                            SetTagsData(tagGroup);
                        }

                        FinishRequest();
                        tryNum++;
                    }

                    if (lastCommSucc)
                        tagGroupIndex++;
                    else if (tryNum > 0)
                    {
                        while(tagGroupIndex < activeTagGroupsCount)
                        {
                            tagGroup = siemensTagGroupsActive[tagGroupIndex];
                            if(tagGroup.TagCount > 0)
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
                SiemensTagGroup tagGroup = null;
                var tag = deviceTemplate.FindCmd(cmd.CmdNum,out tagGroup);
                if(tag != null)
                {
                    if (cmd.CmdTypeID == BaseValues.CmdTypes.Standard)
                        tag.SetCmdData( cmd.CmdVal);
                    else
                    {
                        if(tag.DataType == DataTypeEnum.String)
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
                    while(RequestNeeded(ref tryNum))
                    {
                        if (RequestWriteData(tag, tagGroup))
                            lastCommSucc = true;
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

            CalcCmdStats();
        }
        #endregion

        /// <summary>
        /// KPLogic对象创建
        /// </summary>
        public override void OnAddedToCommLine()
        {
            string fileName = ReqParams.CmdLine.Trim();
            InitDeviceTemplate(fileName);

            floatSignals = new HashSet<int>();
            //初始化通道
            var tagGroups = CreateTagGroups(deviceTemplate);
            InitKPTags(tagGroups);
            siemensTagGroupsActive = deviceTemplate.GetActiveTagGroups();
            activeTagGroupsCount = siemensTagGroupsActive.Count;
            CanSendCmd = deviceTemplate != null && deviceTemplate.CmdTagCount > 0;
        }
        /// <summary>
        /// KPLogic对象创建后，准备轮询前(初始化连接对象)
        /// </summary>
        public override void OnCommLineStart()
        {
            try
            {
                var option = deviceTemplate.ConnectionOptions;
                siemensS7Net = new SiemensS7Net(option.SiemensPLCTypeEnum.ToHslSiemensPLCS(), option.IPAddress);
                var result = siemensS7Net.ConnectServer();
                if (!result.IsSuccess)
                    WriteToLog($"KpSiemensLogic_OnCommLineStart,连接PLC失败,{result.Message}");
            }
            catch(Exception ex)
            {
                WriteToLog($"KpSiemensLogic_OnCommLineStart,连接PLC异常,{ex.Message}");
            }
        }
        #endregion


        private void InitDeviceTemplate(string fileName)
        {
            deviceTemplate = null;
            if (string.IsNullOrEmpty(fileName))
            {
                WriteToLog($"KpSiemensLogic_InitDeviceTemplate,初始化模板失败,找到相应模板文件");
                return; 
            }

            deviceTemplate = new DeviceTemplate();
            string filePath = Path.IsPathRooted(fileName) ?
                        fileName : Path.Combine(AppDirs.ConfigDir, fileName);
            if(!deviceTemplate.Load(filePath,out string errMsg))
                WriteToLog(errMsg);
            else
            {
                //初始话Tag和Command 点位顺序和地址
                deviceTemplate.RefreshTagGroupIndex();
                foreach(var tagGroup in deviceTemplate.TagGroups)
                    tagGroup.RefreshTagAddress();

            }

        }

        private List<TagGroup> CreateTagGroups(DeviceTemplate deviceTemplate)
        {
            List<TagGroup> tagGroups = new List<TagGroup>();
            if(deviceTemplate != null)
            {
                foreach(SiemensTagGroup siemensTagGroup in deviceTemplate.TagGroups)
                {
                    var tagGroup = new TagGroup(siemensTagGroup.Name);
                    tagGroups.Add(tagGroup);
                    foreach(Tag tag in siemensTagGroup.Tags)
                    {
                        tagGroup.AddNewTag(tag.TagID, tag.Name);
                        if (tag.DataType == DataTypeEnum.Real)
                            floatSignals.Add(tag.TagID);
                    }
                }
            }

            return tagGroups;
        }

        #region 自定义驱动数据请求、下发
        private bool RequestReadData(SiemensTagGroup tagGroup)
        {
            var model = tagGroup.GetRequestModel();
            WriteToLog($"开始请求数据,GroupName:{tagGroup.Name},寄存器类型:{tagGroup.MemoryType},起始地址:{model.Address},请求长度:{model.Length}");
            try
            {
                var result = siemensS7Net.Read(model.Address, model.Length);
                WriteToLog($"数据请求结束,IsSuccess:{result.IsSuccess},Message:{result.Message},Data:{result.Content.ToJsonString()}");
                if (result.IsSuccess && result.Content?.Length > 0)
                {
                    //赋值
                    tagGroup.Data = result.Content;
                }
                return result.IsSuccess;
            }
            catch(Exception ex)
            {
                WriteToLog($"KpSiemensLogic_RequestData,数据请求异常,{ex.Message},StackTrace:{ex.StackTrace}");
                return false;
            }

        }

        private bool RequestWriteData(Tag tag, SiemensTagGroup siemensTagGroup)
        {
            if (tag.Data == null)
                return false;
            var address = string.Empty;
            var memoryType = siemensTagGroup.MemoryType;
            var dbNum = siemensTagGroup.DBNum;
            if (memoryType == MemoryTypeEnum.DB)
                address = $"{memoryType}{dbNum}.{tag.Address}";
            else
                address = $"{memoryType}{tag.Address}";
            WriteToLog($"开始写入数据,Name:{tag.Name},寄存器类型:{memoryType},地址:{tag.Address},DBNum:{dbNum},写入值:{JsonConvert.SerializeObject(tag.Data)}");
            try
            {

                OperateResult result = siemensS7Net.Write(address, tag.Data);
                WriteToLog($"写入数据结束,Name:{tag.Name},写入结果:{result.IsSuccess},Message:{result.Message}");
                return result.IsSuccess;
            }
            catch(Exception ex)
            {
                WriteToLog($"KpSiemensLogic_RequestWriteData,数据写入异常,{ex.Message},StackTrace:{ex.StackTrace}");
                return false;
            }
        }

        private void SetTagsData(SiemensTagGroup tagGroup)
        {
            for (int i = 0; i < tagGroup.Tags.Count; i++)
            {
                var tag = tagGroup.Tags[i];
                var data = tagGroup.GetTagVal(i);
                if (data != null)
                    SetCurData(tag.TagID >0 ? tag.TagID - 1 : tag.TagID, (double)data, BaseValues.CnlStatuses.Defined);
            }
        }
        #endregion

    }
}
