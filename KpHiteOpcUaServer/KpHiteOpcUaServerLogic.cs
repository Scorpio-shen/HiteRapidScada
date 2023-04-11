using HslCommunication;
using KpHiteOpcUaServer.OPCUaServer.Model;
using KpHiteOpcUaServer.Server;
using Opc.Ua;
using Opc.Ua.Configuration;
using Org.BouncyCastle.Asn1.IsisMtt.X509;
using Org.BouncyCastle.Crypto.Generators;
using Scada.Comm.Devices;
using Scada.Data.Configuration;
using Scada.Data.Entities;
using Scada.Data.Tables;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Scada.Comm.Devices
{
    public class KpHiteOpcUaServerLogic : KPLogic
    {
        protected DeviceTemplate deviceTemplate;
        private ApplicationInstance applicationInstance;
        Dictionary<int, InputChannelModel> InputChannels;
        public KpHiteOpcUaServerLogic(int number) : base(number)
        {
            ConnRequired = false;
        }

        #region 对象创建
        public override void OnAddedToCommLine()
        {
            string fileName = ReqParams.CmdLine.Trim();
            InitDeviceTemplate(fileName);
            InputChannels = new Dictionary<int, InputChannelModel>();
            InputChannels = deviceTemplate.InCnls.ToDictionary(incnl => incnl.CnlNum, incnl =>
            {
                var model = new InputChannelModel
                {
                    InCnl = incnl,
                    Value = null,
                    
                };
                model.ValueChanged += Model_ValueChanged;
                return model;
            });
        }
        /// <summary>
        /// 输入通道数据值变化
        /// </summary>
        /// <param name="obj"></param>
        private void Model_ValueChanged(InputChannelModel model)
        {
            deviceTemplate?.ModelValueChanged?.Invoke(model);
        }

        private void InitDeviceTemplate(string fileName)
        {
            deviceTemplate = new DeviceTemplate();
            string filePath = Path.IsPathRooted(fileName) ?
                        fileName : Path.Combine(AppDirs.ConfigDir, fileName);
            if (!deviceTemplate.Load(filePath, out string errMsg))
            {
                WriteToLog($"KpHiteOpcUaServerLogic:InitDeviceTemplate,初始化DeviceTemplate异常,{errMsg}");
                return;
            }
            deviceTemplate.OpcClientSetValue += OpcClientSetValue;
            //初始化OPC Server
            applicationInstance = new ApplicationInstance();
            applicationInstance.ApplicationType = ApplicationType.Server;
            applicationInstance.ConfigSectionName = "HiteScadaSettingsServer";

            try
            {
                applicationInstance.LoadApplicationConfiguration(false).Wait();
                // check the application certificate.
                bool certOk = applicationInstance.CheckApplicationInstanceCertificate(false, 0).Result;
                if (!certOk)
                {
                    throw new Exception("Application instance certificate invalid!");
                }

                //applicationInstance.ApplicationConfiguration.ServerConfiguration.BaseAddresses.Clear();
                //applicationInstance.ApplicationConfiguration.ServerConfiguration.BaseAddresses.Add(deviceTemplate.OPCServerIP);
            }
            catch (Exception ex) 
            {
                WriteToLog($"KpHiteOpcUaServerLogic:InitDeviceTemplate,初始化OPC Server配置异常,{ex.Message}");
            }
        }

        public override void OnCommLineStart()
        {
            //启动OPC UA Server
            if (applicationInstance == null)
                return;

            try
            {
                WriteToLog($"KpHiteOpcUaServerLogic:OnCommLineStart,启动OPC服务器");
                var sharpNodeSettingsServer = new SharpNodeSettingsServer(deviceTemplate, WriteToLog);
                applicationInstance.Start(sharpNodeSettingsServer).Wait();

            }
            catch(Exception ex)
            {
                WriteToLog($"KpHiteOpcUaServerLogic:OnCommLineStart,OPC启动异常,{ex.Message}");
            }
        }
        #endregion

        #region 数据轮询
        public override void Session()
        {
            base.Session();
            if (deviceTemplate == null)
            {
                WriteToLog(Localization.UseRussian ?
                     "Нормальное взаимодействие с КП невозможно, т.к. шаблон устройства не загружен" :
                     "KpHiteOpcUaServerLogic device communication is impossible because device template has not been loaded");

                Thread.Sleep(ReqParams.Delay);
                lastCommSucc = false;
            }
            else
            {
                //获取通道数据值
                GetValues();
            }
        }
        /// <summary>
        /// 读取当前通道值
        /// </summary>
        private void GetValues()
        {
            try
            {
                if (CommLineSvc.ServerComm != null)
                {
                    SrezTableLight srezTable = new SrezTableLight();
                    CommLineSvc.ServerComm.ReceiveSrezTable("current.dat", srezTable);

                    if (srezTable.SrezList.Count > 0 && deviceTemplate.InCnls.Count > 0)
                    {
                        SrezTableLight.Srez srez = srezTable.SrezList.Values[0];
                        foreach (var incnl in deviceTemplate.InCnls)
                        {
                            if (srez.GetCnlData(incnl.CnlNum, out SrezTableLight.CnlData cnlData))
                            {
                                if (InputChannels.ContainsKey(incnl.CnlNum))
                                {
                                    try
                                    {
                                        InputChannels[incnl.CnlNum].Value = cnlData.Val;
                                        if (incnl.FormatID == BaseValues.Formats.AsciiText)
                                        {
                                            string val = ScadaUtils.DecodeAscii(cnlData.Val);
                                            InputChannels[incnl.CnlNum].Value = val;
                                        }
                                        else if (incnl.FormatID == BaseValues.Formats.UnicodeText)
                                        {
                                            string val = ScadaUtils.DecodeUnicode(cnlData.Val);
                                            InputChannels[incnl.CnlNum].Value = val;
                                        }
                                        else if (incnl.FormatID == BaseValues.Formats.DateTime)
                                        {
                                            string val = ScadaUtils.DecodeDateTime(cnlData.Val).ToLocalizedString();
                                            InputChannels[incnl.CnlNum].Value = val;
                                        }
                                        else if (incnl.FormatID == BaseValues.Formats.Date)
                                        {
                                            string val = ScadaUtils.DecodeDateTime(cnlData.Val).ToLocalizedDateString();
                                            InputChannels[incnl.CnlNum].Value = val;
                                        }
                                        else if (incnl.FormatID == BaseValues.Formats.Time)
                                        {
                                            string val = ScadaUtils.DecodeDateTime(cnlData.Val).ToLocalizedTimeString();
                                            InputChannels[incnl.CnlNum].Value = val;
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
                }
            }
            catch(Exception ex) 
            {
                WriteToLog($"KpHiteOpcUaServerLogic:GetValues,刷新OPC节点值异常,{ex.Message}");
            }
        }
        #endregion

        #region 外部OPCClient赋值
        private void OpcClientSetValue(OutputChannelModel model)
        {
            try
            {
                WriteToLog($"KpHiteOpcUaServerLogic:OpcClientSetValue,接收OPC Client写值指令,入参:{model.ToJsonString()}");
                var outputChannel = deviceTemplate.CtrlCnls.FirstOrDefault(c => c.CtrlCnlNum == model.CtrlCnlNum);
                if (outputChannel == null)
                    return;

                if (outputChannel.CmdTypeID == BaseValues.CmdTypes.Standard)
                {
                    CommLineSvc.ServerComm.SendStandardCommand(default, outputChannel.CtrlCnlNum, Convert.ToDouble(model.Value), out bool result);
                }
                else if(outputChannel.CmdTypeID == BaseValues.CmdTypes.Binary)
                {
                    var values = Encoding.UTF8.GetBytes(model.Value.ToString());
                    CommLineSvc.ServerComm.SendBinaryCommand(default, outputChannel.CtrlCnlNum, values, out bool result);
                }
            }
            catch (Exception ex)
            {
                WriteToLog($"KpHiteOpcUaServerLogic:OpcClientSetValue,接收OPC Client写值指令,写值异常,{ex.Message}");
            }
            
        }
        #endregion
    }
}
