using KpHiteMqtt.Mqtt;
using KpHiteMqtt.Mqtt.Model;
using KpHiteMqtt.Mqtt.Model.Enum;
using KpHiteMqtt.Mqtt.Model.Request;
using KpHiteMqtt.Mqtt.MqttHandle;
using MQTTnet.Protocol;
using Newtonsoft.Json;
using Scada.Data.Configuration;
using Scada.Data.Tables;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Scada.Comm.Devices
{
    public class KpHiteMqttLogic : KPLogic
    {
        DeviceTemplate deviceTemplate;
        Dictionary<int, InputChannelModel> InputChannelModels;
        HiteMqttClient mqttClient;
        //HiteMqttPayload mqttPayload;        //要Publish的Mqtt数据
        List<MqttModelContent> mqttContents;     //具体消息内容
        List<IMqttHandle> handles;          //Mqtt消息处理类

        public KpHiteMqttLogic(int number) : base(number) 
        {
            ConnRequired = false;
        }

        public override void OnAddedToCommLine()
        {
            try
            {
                string fileName = ReqParams.CmdLine.Trim();
                InitDeviceTemplate(fileName);

                InputChannelModels = deviceTemplate.InCnls.ToDictionary(incnl => incnl.CnlNum, incnl => new InputChannelModel
                {
                    InCnl = incnl,
                    Value = null
                });

               InitMqttClient();

                //加载Mqtt消息处理类
                InitMqttHandle();
            }
            catch(Exception ex)
            {
                WriteToLog($"KpHiteMqttLogic:OnAddedToCommLine,Logic初始化异常,{ex.Message}");
            }
           
        }

        private void InitMqttHandle()
        {
            var interfaceType = typeof(MqttHandleBase);
            var assembly = interfaceType.Assembly;
            var implements = assembly.GetTypes().Where(a => interfaceType.IsAssignableFrom(a) && a != interfaceType).ToList();

            handles = new List<IMqttHandle>();
            handles.AddRange(implements.Select(i => Activator.CreateInstance(i) as IMqttHandle));
            foreach(var handle in handles)
            {
                handle.DeviceTemplate = deviceTemplate;
                handle.MqttClient = mqttClient;
                handle.InputChannelModels = InputChannelModels;
                handle.WriteToLog = WriteToLog;
                if (handle.GetType().Name.Equals(typeof(MqttHandle_Cmd_Receive).Name))
                {
                    var cmdHandle = handle as MqttHandle_Cmd_Receive;
                    cmdHandle.SetCtrlCnlValue = SetCtrlCnlValue;

                }
            }
        }

        

        private void InitDeviceTemplate(string fileName)
        {
            deviceTemplate = new DeviceTemplate();
            string filePath = Path.IsPathRooted(fileName) ?
                        fileName : Path.Combine(AppDirs.ConfigDir, fileName);
            if (!deviceTemplate.Load(filePath, out string errMsg))
            {
                WriteToLog($"KpHiteMqttLogic:InitDeviceTemplate,初始化DeviceTemplate异常,{errMsg}");
                return;
            }


        }

        public override void OnCommLineStart()
        {
            //Mqtt连接
            ConnectMqttServer();
        }

        #region 数据轮询及推送Mqtt服务器
        public override void Session()
        {
            base.Session();
            if (deviceTemplate == null)
            {
                WriteToLog(Localization.UseRussian ?
                     "Нормальное взаимодействие с КП невозможно, т.к. шаблон устройства не загружен" :
                     "KpHiteMqttLogic device communication is impossible because device template has not been loaded");

                Thread.Sleep(ReqParams.Delay);
                lastCommSucc = false;
            }
            else
            {
                lastCommSucc = false;
                if (!mqttClient.IsConnected)
                {
                    ConnectMqttServer();
                }
                //获取通道数据值
                lastCommSucc = GetValues();
                //Mqtt Publish
                lastCommSucc = PublishData();

                //刷新状态
                CalcSessStats();
            }
        }

        private bool PublishData()
        {
            bool operateResult = false;
            try
            {
                if (!mqttClient.IsConnected)
                {
                    WriteToLog($"KpHiteMqttLogic:PublishData,Mqtt未连接到服务端");
                    return operateResult;
                }
                //判断是否
                foreach (MqttModelContent content in mqttContents)
                {
                    if (content.DataTypeValue == DataTypeEnum.Struct)
                    {
                        foreach (var spec in content.Parameters)
                        {
                            if (InputChannelModels.ContainsKey(spec.InCnlNum.Value))
                            {
                                spec.Value = InputChannelModels[spec.InCnlNum.Value]?.Value.ToString();
                            }
                        }
                    }
                    else
                    {
                        //获取通道值
                        if (InputChannelModels.ContainsKey(content.InCnlNum.Value))
                        {
                            content.Value = InputChannelModels[content.InCnlNum.Value].Value?.ToString();
                        }
                    }
                }

                var monitorData = new MqttMonitorData();
                monitorData.time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                monitorData.Data = mqttContents.Select(c=>new MonitorData
                {
                    name = c.Identifier,
                    value = c.Value,
                }).ToList();
                var pubTopic = ScadaSystemTopics.MqttTsModelData_Publish.Replace("{设备号}", deviceTemplate.ConnectionOptions.DeviceSn);
                var payload = JsonConvert.SerializeObject(monitorData);
                var result = mqttClient.PublishAsync(
                    topic: pubTopic,
                    payload: Encoding.UTF8.GetBytes(payload),
                    mqttQuality: MqttQualityOfServiceLevel.AtMostOnce).Result;

                WriteToLog($"KpHiteMqttLogic:PublishData,发布数据,Content:{payload},Result:{result}");
                operateResult = result;

            }
            catch(Exception ex)
            {
                WriteToLog($"KpHiteMqttLogic:PublishData,发布数据异常,{ex.Message}");
                operateResult = false;
            }

            return operateResult;
        }
        private bool GetValues()
        {
            bool result = false;
            try
            {
                if (CommLineSvc.ServerComm != null)
                {
                    SrezTableLight srezTable = new SrezTableLight();
                    CommLineSvc.ServerComm.ReceiveSrezTable("current.dat", srezTable);
                    var incnls = deviceTemplate.InCnls;
                    if (srezTable.SrezList.Count > 0 && incnls.Count > 0)
                    {
                        SrezTableLight.Srez srez = srezTable.SrezList.Values[0];
                        foreach (var incnl in incnls)
                        {
                            if (srez.GetCnlData(incnl.CnlNum, out SrezTableLight.CnlData cnlData))
                            {
                                if (InputChannelModels.ContainsKey(incnl.CnlNum))
                                {
                                    try
                                    {
                                        InputChannelModels[incnl.CnlNum].Value = cnlData.Val;
                                        if (incnl.FormatID == BaseValues.Formats.AsciiText)
                                        {
                                            string val = ScadaUtils.DecodeAscii(cnlData.Val);
                                            InputChannelModels[incnl.CnlNum].Value = val;
                                        }
                                        else if (incnl.FormatID == BaseValues.Formats.UnicodeText)
                                        {
                                            string val = ScadaUtils.DecodeUnicode(cnlData.Val);
                                            InputChannelModels[incnl.CnlNum].Value = val;
                                        }
                                        else if (incnl.FormatID == BaseValues.Formats.DateTime)
                                        {
                                            string val = ScadaUtils.DecodeDateTime(cnlData.Val).ToLocalizedString();
                                            InputChannelModels[incnl.CnlNum].Value = val;
                                        }
                                        else if (incnl.FormatID == BaseValues.Formats.Date)
                                        {
                                            string val = ScadaUtils.DecodeDateTime(cnlData.Val).ToLocalizedDateString();
                                            InputChannelModels[incnl.CnlNum].Value = val;
                                        }
                                        else if (incnl.FormatID == BaseValues.Formats.Time)
                                        {
                                            string val = ScadaUtils.DecodeDateTime(cnlData.Val).ToLocalizedTimeString();
                                            InputChannelModels[incnl.CnlNum].Value = val;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        //WriteToLog($"KpHiteOpcUaServerLogic:GetValues,刷新OPC节点值异常,{ex.Message},输入通道:{incnl.ToJsonString()}");
                                    }
                                }
                            }
                        }
                    }

                    result = true;
                }
            }
            catch (Exception ex)
            {
                result = false;
                WriteToLog($"KpHiteOpcUaServerLogic:GetValues,刷新OPC节点值异常,{ex.Message}");
            }

            return result;
        }
        #endregion

        #region 初始化、连接Mqtt服务器
        private void InitMqttClient()
        {
            try
            {
                //初始化MqttClient
                mqttClient = new HiteMqttClient(deviceTemplate.ConnectionOptions, WriteToLog);
                //Mqtt事件
                mqttClient.OnMqttMessageReceived += MqttClient_OnMqttMessageReceived;
                mqttClient.OnMqttDisConnected += MqttClient_OnMqttDisConnected;
                mqttClient.OnMqttConnected += MqttClient_OnMqttConnected;
                //初始化Mqtt Payload
                mqttContents = new List<MqttModelContent>();
                deviceTemplate.Properties.ForEach(p =>
                {
                    if (p.DataType == KpHiteMqtt.Mqtt.Model.Enum.DataTypeEnum.Array)
                    {
                        for (int i = 0; i < p.DataArraySpecs.ArrayLength; i++)
                        {

                            var mqttContent = new MqttModelContent
                            {
                                Id = p.Id.ToString(),
                                Identifier = p.Identifier + $"[{i}]",
                                //DataType = p.DataType.ToString(),
                                DataTypeValue = p.DataArraySpecs.DataType.ToDataTypeEnum(),
                                Name = p.Name,
                                Value = null,
                                Parameters = null,
                            };
                            if (i > p.DataArraySpecs.ArraySpecs.Count)
                                break;

                            var arrayspec = p.DataArraySpecs.ArraySpecs[i];
                            if (p.DataArraySpecs.DataType == ArrayDataTypeEnum.Struct)
                            {
                                mqttContent.Parameters = arrayspec.DataSpecs.Select(ad=>new MqttModelSpecs
                                {
                                    Identifier = ad.Identifier,
                                    //DataType = ad.DataType.ToString(),
                                    DataTypeValue = ad.DataType,
                                    ParameterName = ad.ParameterName,
                                    InCnlNum = ad.InCnlNum,
                                    CtrlCnlNum = ad.CtrlCnlNum,
                                    Value = null,
                                }).ToList();
                            }
                            else
                            {
                                mqttContent.InCnlNum = arrayspec.InCnlNum;
                                mqttContent.CtrlCnlNum = arrayspec.CtrlCnlNum;
                            }
                            mqttContents.Add(mqttContent);
                        }

                    }
                    else if (p.DataType == KpHiteMqtt.Mqtt.Model.Enum.DataTypeEnum.Struct)
                    {
                        var mqttContent = new MqttModelContent
                        {
                            Id = p.Id.ToString(),
                            //DataType = p.DataType.ToString(),
                            DataTypeValue = p.DataType,
                            Identifier = p.Identifier,
                            Name = p.Name,
                            Value = null,
                            Parameters = new List<MqttModelSpecs>()
                        };
                        foreach (var dataspec in p.DataSpecsList)
                        {
                            mqttContent.Parameters.Add(new MqttModelSpecs
                            {
                                Identifier = dataspec.Identifier,
                                //DataType = dataspec.DataType.ToString(),
                                DataTypeValue = dataspec.DataType,
                                ParameterName = dataspec.ParameterName,
                                InCnlNum = dataspec.InCnlNum,
                                CtrlCnlNum = dataspec.CtrlCnlNum,
                                Value = null
                            });
                        }
                        mqttContents.Add(mqttContent);
                    }
                    else
                    {
                        var mqttContent = new MqttModelContent
                        {
                            Id = p.Id.ToString(),
                            InCnlNum = p.CnlNum,
                            CtrlCnlNum = p.CtrlCnlNum,
                            Name = p.Name,
                            Identifier = p.Identifier,
                            //DataType = p.DataType.ToString(),
                            DataTypeValue = p.DataType,
                            Parameters = null,
                            Value = null
                        };
                        mqttContents.Add(mqttContent);

                    }
                });
            }
           catch (Exception ex)
            {
                WriteToLog($"KpHiteMqttLogic:InitMqttClient,初始化MqttClient异常,{ex.Message}");
            }
        }
        private void ConnectMqttServer()
        {
            try
            {
                if (mqttClient == null)
                    InitMqttClient();
                if (mqttClient != null)
                {
                    var result = mqttClient.ConnectServer().Result;
                    if (!result)
                    {
                        WriteToLog($"KpHiteMqttLogic:ConnectMqttServer,Mqtt连接失败");
                    }
                }
            }
           catch(Exception ex)
            {
                WriteToLog($"KpHiteMqttLogic:ConnectMqttServer,Mqtt连接异常,{ex.Message}");
            }
        }
        #endregion

        #region Mqtt事件
        private void MqttClient_OnMqttConnected(object sender, EventArgs e)
        {
            //订阅Topic
            var systemTopics = new List<string>()
            {
                ScadaSystemTopics.MqttCmdReply_Publish,
                ScadaSystemTopics.MqttCmd_Subscribe,
                ScadaSystemTopics.MqttTsModelDataReply_Subscribe,
                ScadaSystemTopics.MqttTsModelData_Publish
            };
            if (mqttClient.IsConnected && deviceTemplate.SubscribeTopics.Count > 0)
            {
                foreach(var topic in deviceTemplate.SubscribeTopics)
                {
                    var subTopic = topic;
                    if (systemTopics.Contains(topic))
                    {
                        subTopic = topic.Replace("{设备号}", deviceTemplate.ConnectionOptions.DeviceSn);
                    }
                    bool result = mqttClient.Subscribe(subTopic).Result;
                    WriteToLog($"KpHiteMqttLogic:MqttClient_OnMqttConnected,订阅Topic:{topic},订阅结果:{result}");
                }
            }
        }

        private void MqttClient_OnMqttDisConnected(object sender, EventArgs e)
        {
            //throw new NotImplementedException();

        }
        private void MqttClient_OnMqttMessageReceived(string topic, string content)
        {
            WriteToLog($"KpHiteMqttLogic:MqttClient_OnMqttMessageReceived,接收Mqtt数据,topic:{topic},content:{content}");

            foreach(var handle in handles)
            {
                if(handle.CanHandleTopic(topic))
                    handle.Handle(topic, content);
            }
        }
        #endregion

        #region 输出通道进行赋值
        private void SetCtrlCnlValue(int ctrlcnlnum,object value)
        {
            try
            {
                if (!deviceTemplate.CtrlCnls.Any(c => c.CtrlCnlNum == ctrlcnlnum))
                    WriteToLog($"KpHiteMqttLogic:SetCtrlCnlValue,通道号:{ctrlcnlnum}未在配置模板中，值:{value}");

                var ctrlcnl = deviceTemplate.CtrlCnls.FirstOrDefault(c => c.CtrlCnlNum == ctrlcnlnum);
                if (ctrlcnl.CmdTypeID == BaseValues.CmdTypes.Standard)
                {
                    CommLineSvc.ServerComm.SendStandardCommand(default, ctrlcnl.CtrlCnlNum, Convert.ToDouble(value), out bool result);
                }
                else if (ctrlcnl.CmdTypeID == BaseValues.CmdTypes.Binary)
                {
                    var values = Encoding.UTF8.GetBytes(value.ToString());
                    CommLineSvc.ServerComm.SendBinaryCommand(default, ctrlcnl.CtrlCnlNum, values, out bool result);
                }
            }
            catch(Exception ex)
            {
                WriteToLog($"KpHiteMqttLogic:SetCtrlCnlValue,,写值异常,通道号:{ctrlcnlnum},值:{value},{ex.Message}");
            }
        }
        #endregion
    }
}
