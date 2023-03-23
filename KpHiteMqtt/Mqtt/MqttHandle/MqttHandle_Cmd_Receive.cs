using HslCommunication;
using KpCommon.Helper;
using KpHiteMqtt.Mqtt.Model;
using KpHiteMqtt.Mqtt.Model.Request;
using KpHiteMqtt.Mqtt.Model.Response;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpHiteMqtt.Mqtt.MqttHandle
{
    /// <summary>
    /// Mqtt下发指令接收
    /// </summary>
    public class MqttHandle_Cmd_Receive : MqttHandleBase
    {
        protected override string Topic
        {
            get
            {
                if (ScadaSystemTopics.MqttCmd_Subscribe.Contains("{设备号}"))
                {
                    return ScadaSystemTopics.MqttCmd_Subscribe.Replace("{设备号}",DeviceTemplate.ConnectionOptions.DeviceSn);
                }

                return ScadaSystemTopics.MqttCmd_Subscribe;
            }
        }

        /// <summary>
        /// 通道
        /// </summary>
        public Action<int, object> SetCtrlCnlValue;

        private List<Property> properties;

        #region 注释
        //public override void Handle(string topic, string content)
        //{
        //    properties = DeviceTemplate.Properties;
        //    try
        //    {
        //        var decodeContent = EncryptionHelper.Base64Decode(content);
        //        var payload = JsonConvert.DeserializeObject<HiteMqttPayload>(decodeContent);
        //        var mqttContents = JsonConvert.DeserializeObject<List<MqttModelContent>>(payload.Content);

        //        WriteToLog($"MqttHandle_Cmd_Receive:Handle,接收服务器下发指令,payload:{payload.ToJsonString()}");
        //        List<MqttCmdResponse> cmdResponses = new List<MqttCmdResponse>();
        //        foreach(var mqttcontent in mqttContents)
        //        {
        //            var cmdResponse = new MqttCmdResponse()
        //            {
        //                Id = mqttcontent.Id,
        //                Identifier = mqttcontent.Identifier,
        //                IsSuccess = true,
        //                Message = "操作成功"
        //            };
        //            cmdResponses.Add(cmdResponse);
        //            try
        //            {
        //                //查询到属性对象
        //                var property = properties.FirstOrDefault(p => p.Id.ToString() == mqttcontent.Id);
        //                if(property == null)
        //                {
        //                    cmdResponse.IsSuccess = false;
        //                    cmdResponse.Message = $"未找到Id为{mqttcontent.Id}的物模型";
        //                    continue;
        //                }
        //                if (property.DataType == Model.Enum.DataTypeEnum.Array)
        //                {
        //                    var pIdentifiers = new List<IdentifierArrayModel>();
        //                    for (int i = 0; i < property.DataArraySpecs.ArrayLength; i++)
        //                    {
        //                        pIdentifiers.Add(new IdentifierArrayModel()
        //                        {
        //                            Identifier = property.Identifier + $"[{i}]",
        //                            Index = i
        //                        });
        //                    }
        //                    if (property.DataArraySpecs.DataType == Model.Enum.ArrayDataTypeEnum.Struct)
        //                    {
        //                        var identifier = mqttcontent.Identifier;
        //                        var pIdentifier = pIdentifiers.FirstOrDefault(id => id.Identifier.Equals(identifier));
        //                        if (pIdentifier == null)
        //                        {
        //                            cmdResponse.IsSuccess = false;
        //                            cmdResponse.Message = $"未找到标识符为:{identifier}的物模型";
        //                            continue;
        //                        }
        //                        //获取索引
        //                        var index = pIdentifier.Index;
        //                        //获取Json参数
        //                        if(property.DataArraySpecs.ArraySpecs.Count > index)
        //                        {
        //                            var arraySpecs = property.DataArraySpecs.ArraySpecs[index];
        //                            foreach(var parameter in mqttcontent.Parameters)
        //                            {
        //                                var dataspecs = arraySpecs.DataSpecs.FirstOrDefault(d => d.Identifier == parameter.Identifier);
        //                                if(dataspecs != null)
        //                                {
        //                                    SetCtrlCnlValue(dataspecs.CtrlCnlNum, parameter.Value);
        //                                    WriteToLog($"MqttHandle_Cmd_Receive:Handle,执行服务器下发Cmd指令,输出通道号:{dataspecs.CtrlCnlNum},写入值:{parameter.Value},原始内容:{parameter.ToJsonString()}");
        //                                }
        //                            }
        //                        }

        //                    }
        //                    else
        //                    {
        //                        var identifier = mqttcontent.Identifier;
        //                        var pIdentifier = pIdentifiers.FirstOrDefault(id => id.Identifier.Equals(identifier));
        //                        if (pIdentifier == null)
        //                        {
        //                            cmdResponse.IsSuccess = false;
        //                            cmdResponse.Message = $"未找到标识符为:{identifier}的物模型";
        //                            continue;
        //                        }
        //                        //获取索引
        //                        var index = pIdentifier.Index;
        //                        //获取输出通道
        //                        if(mqttcontent.Value == null)
        //                        {
        //                            cmdResponse.IsSuccess = false;
        //                            cmdResponse.Message = "Value值不能为null";
        //                            continue;
        //                        }

        //                        if(property.DataArraySpecs?.ArraySpecs?.Count > index)
        //                        {
        //                            var ctrlcnlnum = property.DataArraySpecs.ArraySpecs[index].CtrlCnlNum;
        //                            SetCtrlCnlValue(ctrlcnlnum, mqttcontent.Value);
        //                            WriteToLog($"MqttHandle_Cmd_Receive:Handle,执行服务器下发Cmd指令,输出通道号:{ctrlcnlnum},写入值:{mqttcontent.Value},原始内容:{mqttcontent.ToJsonString()}");
        //                        }

        //                    }

        //                }
        //                else if (property.DataType == Model.Enum.DataTypeEnum.Struct)
        //                {
        //                    StringBuilder msgBulider = new StringBuilder();
        //                    foreach (var parameter in mqttcontent.Parameters)
        //                    {
        //                        var dataSpecs = property.DataSpecsList.FirstOrDefault(d => d.Identifier.Equals(parameter.Identifier));
        //                        if(dataSpecs == null)
        //                        {
        //                            msgBulider.AppendLine($"参数:{dataSpecs.Identifier},执行失败,未找到该参数;");
        //                            continue;
        //                        }
        //                        else
        //                        {
        //                            if(parameter.Value != null)
        //                            {
        //                                SetCtrlCnlValue(dataSpecs.CtrlCnlNum, parameter.Value);
        //                                WriteToLog($"MqttHandle_Cmd_Receive:Handle,执行服务器下发Cmd指令,输出通道号:{parameter.CtrlCnlNum},写入值:{parameter.Value},参数Parameter:{parameter.ToJsonString()}");
        //                            }
        //                            else
        //                            {
        //                                msgBulider.AppendLine($"参数:{parameter.Identifier},Value值不能为null");
        //                            }
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    //基础物模型
        //                    if (mqttcontent.Value != null)
        //                    {
        //                        SetCtrlCnlValue(property.CtrlCnlNum, mqttcontent.Value);
        //                        //记录日志
        //                        WriteToLog($"MqttHandle_Cmd_Receive:Handle,执行服务器下发Cmd指令,输出通道号:{property.CtrlCnlNum},写入值:{mqttcontent.Value},原始内容:{mqttcontent.ToJsonString()}");
        //                    }
        //                    else
        //                    {
        //                        cmdResponse.IsSuccess = false;
        //                        cmdResponse.Message = "Value值不能为null";
        //                    }
        //                }
        //            }
        //            catch(Exception ex)
        //            {
        //                cmdResponse.IsSuccess = false;
        //                cmdResponse.Message = $"操作失败,{ex.Message}";
        //            }
        //        }

        //        //回复服务器
        //        ResponServer(ScadaSystemTopics.MqttCmdReply_Publish, payload.MessageId, 200, "成功接收指令", cmdResponses);
        //    }
        //    catch (Exception ex)
        //    {
        //        WriteToLog($"MqttHandle_Cmd_Receive:Handle,接收服务器指令异常,{ex.Message}");
        //    }
        //}

        #endregion

        //public override bool CanHandleTopic(string topic)
        //{
        //    return Topic.Equals(topic);
        //}

        public override void Handle(string topic, string content)
        {
            properties = DeviceTemplate.Properties;
            try
            {
                //var decodeContent = EncryptionHelper.Base64Decode(content);
                var dataSet = JsonConvert.DeserializeObject<MqttMonitorDataSet>(content);
                WriteToLog($"MqttHandle_Cmd_Receive:Handle,接收服务器下发指令,Data:{dataSet.Data.ToJsonString()}");
                //List<MqttCmdResponse> cmdResponses = new List<MqttCmdResponse>();
                foreach(var data in dataSet.Data)
                {
                    var prop = properties.FirstOrDefault(p => p.Identifier.Equals(data.name));
                    if(prop == null)
                    {
                        WriteToLog($"MqttHandle_Cmd_Receive:Handle,未找到该物模型,data:{data.ToJsonString()}");
                        continue;
                    }
                    SetCtrlCnlValue(prop.CtrlCnlNum, data.value);
                    WriteToLog($"MqttHandle_Cmd_Receive:Handle,通道赋值,Identifier:{prop.Identifier},Value:{data.value}");
                }
                //回复服务器
                //ResponServer(ScadaSystemTopics.MqttCmdReply_Publish, payload.MessageId, 200, "成功接收指令", cmdResponses);
            }
            catch (Exception ex)
            {
                WriteToLog($"MqttHandle_Cmd_Receive:Handle,接收服务器指令异常,{ex.Message}");
            }
        }

        private void ResponServer(string topic, string messageId,int code,string message,List<MqttCmdResponse> cmdResponses)
        {
            
            var mqttResponse = new MqttResponse
            {
                Code = code,
                Message = message,
                Content = JsonConvert.SerializeObject(cmdResponses)
            };
            var payload = new HiteMqttPayload
            {
                MessageId = messageId,
                Time = DateTime.UtcNow.ToString("yyyy-MM-dd hh:mm:ss"),
                Content = JsonConvert.SerializeObject(mqttResponse)
            };

            MqttClient.PublishAsync(topic,payload).Wait();
        }
    }

    public class IdentifierArrayModel
    {
        public string Identifier { get; set; }
        public int Index { get; set; }
    }
}
