/*
 * Copyright 2022-03-08 Manyan Ren
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
 * Module   : KpMqtt
 * Summary  : Editing device template form
 * 
 * Author   : Manyan Ren
 * Created  : 2022-03-08
 * Modified : 2022-03-08
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Scada.Comm.Devices.Mqtt.Config;
using Scada.UI;
using System.IO;
using StriderMqtt;
using System.Xml;


namespace Scada.Comm.Devices.Mqtt.UI
{
    public partial class FrmMqttDevTemplate : Form
    {
        private const string NewFileName = "KpMqtt_001.xml";

        private AppDirs appDirs;                 // 应用程序目录
        private string initialFileName;          // 启动表单时打开模板文件的名称
        private string fileName;                 // 设备模板文件名
       // private bool saveOnly;                   // 只允许保存文件
        int nKpNum; // 当前设备编号

        private DeviceConfig deviceConfigTemplate; // 保存当前 MQTT 配置文件的数据  

       // private bool modified;           // 设备模板更改标志
 
        private TreeNode selNode;        // 选定的树节点
        private TreeNode Publish;       // 元素组树节点 常规订阅
        private TreeNode Subscription;       // 命令树节点 指令订阅

        private TreeNode grsNodePubTopics;       // 命令树节点 发布设备数据主题
        private TreeNode grsNodePubCmds;        // 元素组树节点 发布指令
        private TreeNode grsNodeSubTopics;       // 命令树节点 发布设备数据主题
        private TreeNode grsNodeSubCmds;        // 元素组树节点 脚本订阅
        private TreeNode grsNodeSubJSs;        // 元素组树节点 脚本订阅



        public FrmMqttDevTemplate()
        {
            InitializeComponent();

            deviceConfigTemplate = new DeviceConfig();

            appDirs = null;
            initialFileName = "";
            fileName = "";
           // saveOnly = false;

          //  modified = false;

            selNode = null;
            Subscription = treeView.Nodes["Subscription"];
            Publish = treeView.Nodes["Publish"];

            grsNodePubTopics    = Publish.Nodes["grsNodePubTopics"];
            grsNodePubCmds      = Publish.Nodes["grsNodePubCmds"];
            grsNodeSubTopics    = Subscription.Nodes["grsNodeSubTopics"];
            grsNodeSubCmds      = Subscription.Nodes["grsNodeSubCmds"];
            grsNodeSubJSs       = Subscription.Nodes["grsNodeSubJSs"];

        }
        // 显示本身的对话框
        public static void ShowDialog(int kpNum, AppDirs appDirs)
        {
            FrmMqttDevTemplate frmDevTemplate = new FrmMqttDevTemplate
            {
                appDirs = appDirs ?? throw new ArgumentNullException("appDirs"),
                //appDirs.
                //uiCustomization = uiCustomization ?? throw new ArgumentNullException("uiCustomization"),
               // initialFileName = fileName,
                //saveOnly = saveOnly,
                nKpNum = kpNum
            };

            frmDevTemplate.ShowDialog();
            //fileName = frmDevTemplate.fileName;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            string strMsg = "";
            Save(Path.Combine(appDirs.ConfigDir,fileName),out strMsg);
        }
        public bool Save(string fileName, out string errMsg)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                XmlDeclaration xmlDecl = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);
                xmlDoc.AppendChild(xmlDecl);

                XmlElement rootElem = xmlDoc.CreateElement("DevTemplate");
                xmlDoc.AppendChild(rootElem);
                SaveToXml(rootElem);

                xmlDoc.Save(fileName);
                errMsg = "";
                return true;
            }
            catch (Exception ex)
            {
                errMsg = "MqttPhrases" + ":" + Environment.NewLine + ex.Message;
                return false;
            }
        }
        public  void SaveToXml(XmlElement rootElem)
        {
            XmlElement serverElem = rootElem.AppendElem("MqttParams");
            SaveServerToXml(serverElem,deviceConfigTemplate.ConnectionArgs);

            if(deviceConfigTemplate.SubTopics.Count > 0)
            {
                XmlElement MqttSubTopicsElm = rootElem.AppendElem("MqttSubTopics");
                foreach (MqttSubTopic subtopic in deviceConfigTemplate.SubTopics)
                {
                    SaveMqttSubTopicsToXml(MqttSubTopicsElm.AppendElem("Topic"), subtopic);
                }
            }          

            if(deviceConfigTemplate.PubTopics.Count > 0)
            {
                XmlElement MqttPubTopicsElem = rootElem.AppendElem("MqttPubTopics");
                foreach (MqttPubTopic pubTopic in deviceConfigTemplate.PubTopics)
                {
                    SaveMqttPubTopicsToXml(MqttPubTopicsElem.AppendElem("Topic"), pubTopic);
                }
            }
            
            if(deviceConfigTemplate.PubCmds.Count > 0)
            {
                XmlElement MqttPubCmdsElem = rootElem.AppendElem("MqttPubCmds");
                foreach (MqttPubCmd pubCmd in deviceConfigTemplate.PubCmds)
                {
                    SaveMqttPubCmdsToXml(MqttPubCmdsElem.AppendElem("Topic"), pubCmd);
                }
            }

            if (deviceConfigTemplate.SubCmds.Count > 0)
            {
                XmlElement MqttSubCmdsElem = rootElem.AppendElem("MqttSubCmds");
                foreach (MqttSubCmd subCmd in deviceConfigTemplate.SubCmds)
                {
                    SaveMqttSubCmdsToXml(MqttSubCmdsElem.AppendElem("Topic"), subCmd);
                }
            }

            if (deviceConfigTemplate.SubJSs.Count > 0)
            {
                XmlElement MqttSubJSsElem = rootElem.AppendElem("MqttSubJSs");
                foreach (MqttSubJS subJs in deviceConfigTemplate.SubJSs)
                {
                    SaveMqttSubJSsToXml(MqttSubJSsElem.AppendElem("Topic"), subJs);
                }
            }

        }

        public  void SaveServerToXml(XmlElement serverElem , MqttConnectionArgs connect)
        {
           // MqttConnectionArgs ConnectionArgs
            if (serverElem == null)
                throw new ArgumentNullException("serverElem");

            serverElem.SetAttribute("Hostname", connect.Hostname);
            serverElem.SetAttribute("ClientID", connect.ClientId);
            serverElem.SetAttribute("Port", connect.Port);
            serverElem.SetAttribute("UserName", connect.Username);
            serverElem.SetAttribute("Password", connect.Password);

        }
        public void SaveMqttSubTopicsToXml(XmlElement MqttSubTopicsElem,MqttSubTopic subTopic)
        {
            if (MqttSubTopicsElem == null)
                throw new ArgumentNullException("MqttSubTopicsElem");

            MqttSubTopicsElem.SetAttribute("TopicName", subTopic.TopicName);
            MqttSubTopicsElem.SetAttribute("QosLevel", (int)subTopic.QosLevel);
           
        }
        public void SaveMqttSubCmdsToXml(XmlElement MMqttSubCmdElem, MqttSubCmd subCmd)
        {
            if (MMqttSubCmdElem == null)
                throw new ArgumentNullException("MqttSubCmdElem");

            MMqttSubCmdElem.SetAttribute("TopicName", subCmd.TopicName);
            MMqttSubCmdElem.SetAttribute("QosLevel", (int)subCmd.QosLevel);
            MMqttSubCmdElem.SetAttribute("CmdType", subCmd.CmdType);
            MMqttSubCmdElem.SetAttribute("IDUser", subCmd.IDUser);
            MMqttSubCmdElem.SetAttribute("NumCnlCtrl", subCmd.NumCnlCtrl);


        }

        public void SaveMqttSubJSsToXml(XmlElement MqttSubJSsElem, MqttSubJS subJs)
        {
            if (MqttSubJSsElem == null)
                throw new ArgumentNullException("MqttSubJSsElem");

            MqttSubJSsElem.SetAttribute("TopicName", subJs.TopicName);
            MqttSubJSsElem.SetAttribute("QosLevel", (int)subJs.QosLevel);
            MqttSubJSsElem.SetAttribute("CnlCnt", subJs.CnlCnt);
            MqttSubJSsElem.SetAttribute("JSHandlerPath", subJs.JSHandlerPath);


        }
        public void SaveMqttPubTopicsToXml(XmlElement MqttPubTopicsElem, MqttPubTopic pubTopic)
        {
            if (MqttPubTopicsElem == null)
                throw new ArgumentNullException("MqttPubTopicsElem");

            MqttPubTopicsElem.SetAttribute("TopicName", pubTopic.TopicName);
            MqttPubTopicsElem.SetAttribute("QosLevel", (int)pubTopic.QosLevel);
            MqttPubTopicsElem.SetAttribute("NumCnl", pubTopic.NumCnl);
            MqttPubTopicsElem.SetAttribute("PubBehavior", pubTopic.PubBehavior);
            MqttPubTopicsElem.SetAttribute("Retain", pubTopic.Retain);
            MqttPubTopicsElem.SetAttribute("NDS", ".");
            MqttPubTopicsElem.SetAttribute("Prefix", pubTopic.Prefix);
            MqttPubTopicsElem.SetAttribute("Suffix", pubTopic.Suffix);
        }
        public void SaveMqttPubCmdsToXml(XmlElement MqttPubCmdsElem, MqttPubCmd pubTopic)
        {
            if (MqttPubCmdsElem == null)
                throw new ArgumentNullException("MqttPubCmdsElem");

            MqttPubCmdsElem.SetAttribute("TopicName", pubTopic.TopicName);
            MqttPubCmdsElem.SetAttribute("QosLevel", (int)pubTopic.QosLevel);
            MqttPubCmdsElem.SetAttribute("NumCmd", pubTopic.NumCmd);
            
        }
        private void btnAddElem_Click(object sender, EventArgs e)
        {      
            // 五中对象
            string strTag = "";
            string strTagReal = "";
            if (selNode != null)
            {
                object tag = selNode.Tag;

                MqttPubCmd pubCmd = tag as MqttPubCmd;
                MqttPubTopic pubTopic = tag as MqttPubTopic;
                MqttSubTopic subTopic = tag as MqttSubTopic;
                MqttSubCmd subCmds = tag as MqttSubCmd;
                MqttSubJS subJs = tag as MqttSubJS;

                if (pubCmd != null) strTagReal = "MqttPubCmds";
                else if (pubTopic != null) strTagReal = "MqttPubTopics";
                else if (subTopic != null && tag.GetType().Name != "MqttSubJS") strTagReal = "MqttSubTopics";
                else if (subCmds != null) strTagReal = "MqttSubCmds";
                else if (subJs != null) strTagReal = "MqttSubJSs";  // 1
                else strTagReal = (string)selNode.Tag; // 父节点


                switch (strTagReal)
                {
                    case "MqttPubCmds":
                        MqttPubCmd pubCmdR = new MqttPubCmd();
                        if (FrmMqttPubCmds.ShowDialog(ref pubCmdR, appDirs, nKpNum, deviceConfigTemplate.PubCmds, -1) == true)
                        {
                            TreeNode grNode = null;
                            int nIndex = 0;
                            if (pubCmd != null)
                            {
                                grNode = selNode.Parent;
                                nIndex = grNode.GetNodeCount(true);
                            }
                            else if (strTagReal == "MqttPubCmds") // 父节点                                                               
                            {
                                grNode = selNode;
                                nIndex = grNode.GetNodeCount(false) + 1;
                            }
                           
                            grNode.Nodes.Insert(nIndex, NewMqttPubCmd(pubCmdR));
                            // devi

                            deviceConfigTemplate.PubCmds.Add(pubCmdR);
                        }
                        break;
                    case "MqttPubTopics":
                        MqttPubTopic pubTopicR = new MqttPubTopic();

                        if (FrmMqttPubTopic.ShowDialog(ref pubTopicR,appDirs,nKpNum,deviceConfigTemplate.PubTopics, -1) == true)
                        {
                            TreeNode grNode = null;
                            int nIndex = 0;
                            if (pubTopic != null)
                            {
                                grNode = selNode.Parent;
                                nIndex = grNode.GetNodeCount(true);
                            }
                            else if (strTagReal == "MqttPubTopics") // 父节点                                                               
                            {
                                grNode = selNode;
                                nIndex = grNode.GetNodeCount(false) + 1;
                            }
                           
                            grNode.Nodes.Insert(nIndex, NewMqttPubTopic(pubTopicR));
                            deviceConfigTemplate.PubTopics.Add(pubTopicR);

                        }
                        break;
                    case "MqttSubCmds":
                        MqttSubCmd subCmsR = new MqttSubCmd();
                        if (FrmMqttSubCmds.ShowDialog(ref subCmsR, appDirs, nKpNum,deviceConfigTemplate.SubCmds, -1) == true)
                        {                         
                            TreeNode grNode = null;
                            int nIndex = 0;
                            if (subCmds != null)
                            {
                                grNode = selNode.Parent;
                                nIndex = grNode.GetNodeCount(true);
                            }
                            else if (strTagReal == "MqttSubCmds") // 父节点                                                               
                            {
                                grNode = selNode;
                                nIndex = grNode.GetNodeCount(false) + 1;
                            }
                           
                            grNode.Nodes.Insert(nIndex, NewMqttSubCmd(subCmsR));
                            deviceConfigTemplate.SubCmds.Add(subCmsR);

                        }
                        break;
                    case "MqttSubJSs":
                        MqttSubJS subJSR = new MqttSubJS();
                        if(FrmMqttSubJSs.ShowDialog(ref subJSR ,appDirs, deviceConfigTemplate.SubJSs, -1) == true)
                        {
                            TreeNode grNode = null;
                            int nIndex = 0;
                            if (subJs != null)
                            {
                                grNode = selNode.Parent;
                                nIndex = grNode.GetNodeCount(true);
                            }
                            else if (strTagReal == "MqttSubJSs") // 父节点                                                               
                            {
                                grNode = selNode;
                                nIndex = grNode.GetNodeCount(false) + 1;
                            }
                            
                            grNode.Nodes.Insert(nIndex, NewMqttSubJS(subJSR)); // 全部添加在最后
                            deviceConfigTemplate.SubJSs.Add(subJSR);

                        }
                        break;
                    case "MqttSubTopics":
                        MqttSubTopic subTopicR = new MqttSubTopic();
                        if(FrmMqttSubTopics.ShowDialog(ref subTopicR, deviceConfigTemplate.SubTopics, -1) == true)
                        {
                            TreeNode grNode = null;
                            int nIndex = 0;
                            if (subTopic != null)
                            {
                                grNode = selNode.Parent;
                                nIndex = grNode.GetNodeCount(true);
                            }
                            else if (strTagReal == "MqttSubTopics") // 父节点                                                               
                            {
                                grNode = selNode;
                                nIndex = grNode.GetNodeCount(false) + 1;                              
                            }
                            
                            grNode.Nodes.Insert(nIndex, NewMqttSubTopic(subTopicR));
                            deviceConfigTemplate.SubTopics.Add(subTopicR);

                        }

                        break;
                    default: break;
                }
            }



        }
        private void ConfigElem() // 配置节点
        {
            // 五中对象
            string strTag = "";
            string strTagReal = "";
            if (selNode != null)
            {
                object tag = selNode.Tag;

                MqttPubCmd pubCmd = tag as MqttPubCmd;
                MqttPubTopic pubTopic = tag as MqttPubTopic;
                MqttSubTopic subTopic = tag as MqttSubTopic ;
                MqttSubCmd subCmds = tag as MqttSubCmd;
                MqttSubJS subJs = tag as MqttSubJS; // 有继承关系  注意区分

                if (pubCmd != null) strTagReal = "MqttPubCmds";
                else if (pubTopic != null) strTagReal = "MqttPubTopics";
                else if (subTopic != null && tag.GetType().Name != "MqttSubJS") strTagReal = "MqttSubTopics";
                else if (subCmds != null) strTagReal = "MqttSubCmds";
                else if (subJs != null) strTagReal = "MqttSubJSs";

                switch (strTagReal)
                {
                    case "MqttPubCmds":
                        if (FrmMqttPubCmds.ShowDialog(ref pubCmd,appDirs,nKpNum, deviceConfigTemplate.PubCmds, selNode.Index) == true)
                        {
                            // 除了自己之外的内容不能重复
                            
                            deviceConfigTemplate.PubCmds[selNode.Index] = pubCmd;

                            treeView.BeginUpdate();
                            TreeNode elemNode = selNode;
                            elemNode.Text = pubCmd.TopicName + ":" +
                                            ((int)pubCmd.QosLevel).ToString() + ":" +
                                            pubCmd.NumCmd;
                            elemNode.Tag = pubCmd;
                            treeView.EndUpdate();

                        }
                        break;
                    case "MqttPubTopics":
                        if (FrmMqttPubTopic.ShowDialog(ref pubTopic, appDirs, nKpNum, deviceConfigTemplate.PubTopics, selNode.Index) == true)
                        {
                            
                            deviceConfigTemplate.PubTopics[selNode.Index] = pubTopic;

                            treeView.BeginUpdate();
                            TreeNode elemNode = selNode;
                            elemNode.Text = pubTopic.TopicName + ":" +
                                ((int)pubTopic.QosLevel).ToString() + ":" +
                                pubTopic.NumCnl;
                            elemNode.Tag = pubTopic;
                            treeView.EndUpdate();
                        }
                        break;
                    case "MqttSubCmds":
                        if (FrmMqttSubCmds.ShowDialog(ref subCmds, appDirs, nKpNum, deviceConfigTemplate.SubCmds, selNode.Index) == true)
                        {
                            // 除了自己之外的内容不能重复
                            
                            deviceConfigTemplate.SubCmds[selNode.Index] = subCmds;

                            treeView.BeginUpdate();
                            TreeNode elemNode = selNode;
                            elemNode.Text = subCmds.TopicName + ":" +
                                ((int)subCmds.QosLevel).ToString() + ":" +
                                subCmds.CmdType.ToString() + "：" +
                                subCmds.NumCnlCtrl;
                            elemNode.Tag = subCmds;
                            treeView.EndUpdate();
                        }

                        break;
                    case "MqttSubJSs":

                        if (FrmMqttSubJSs.ShowDialog(ref subJs,appDirs, deviceConfigTemplate.SubJSs, selNode.Index) == true)
                        {
                            // 除了自己之外的内容不能重复
                           
                            deviceConfigTemplate.SubJSs[selNode.Index] = subJs;

                            treeView.BeginUpdate();
                            TreeNode elemNode = selNode;
                            elemNode.Text = subJs.TopicName + ":" +
                                ((int)subJs.QosLevel).ToString() + ":" +
                                subJs.CnlCnt + ":" + 
                                Path.GetFileName(subJs.JSHandlerPath);
                            elemNode.Tag = subJs;
                            treeView.EndUpdate();
                        }
                        break;
                    case "MqttSubTopics":
                        if (FrmMqttSubTopics.ShowDialog(ref subTopic, deviceConfigTemplate.SubTopics, selNode.Index) == true)
                        {
                            
                            deviceConfigTemplate.SubTopics[selNode.Index] = subTopic;

                            treeView.BeginUpdate();
                            TreeNode elemNode = selNode;
                            elemNode.Text = subTopic.TopicName + ":" + ((int)subTopic.QosLevel).ToString();
                            elemNode.Tag = subTopic;      
                             treeView.EndUpdate();
                        }

                        break;
                    default: break;
                }
            }



        }
        private TreeNode NewMqttSubTopic(MqttSubTopic mqttSubTopic)
        {
            TreeNode MqttSubTopicNode = new TreeNode(mqttSubTopic.TopicName +":" + ((int)mqttSubTopic.QosLevel).ToString());
            MqttSubTopicNode.ImageIndex = MqttSubTopicNode.SelectedImageIndex = 3;

            MqttSubTopicNode.Tag = mqttSubTopic;
            return MqttSubTopicNode;
        }
        private TreeNode NewMqttSubJS(MqttSubJS mqttSubJS)
        {
            string strNodeName = mqttSubJS.TopicName + ":" + ((int)mqttSubJS.QosLevel).ToString() + ":" + mqttSubJS.CnlCnt + ":" + Path.GetFileName(mqttSubJS.JSHandlerPath);
            TreeNode MqttSubJSNode = new TreeNode(strNodeName);
            MqttSubJSNode.ImageIndex = MqttSubJSNode.SelectedImageIndex = 3;

            MqttSubJSNode.Tag = mqttSubJS;

            return MqttSubJSNode;
        }
        private TreeNode NewMqttSubCmd(MqttSubCmd mqttSubCmd)
        {
            string strCmdName = mqttSubCmd.TopicName + ":" +
                                ((int)mqttSubCmd.QosLevel).ToString() + ":" +
                                mqttSubCmd.CmdType.ToString() + ":" +
                                mqttSubCmd.NumCnlCtrl;
            TreeNode MqttSubCmdNode = new TreeNode(strCmdName);
            MqttSubCmdNode.ImageIndex = MqttSubCmdNode.SelectedImageIndex = 3;

            MqttSubCmdNode.Tag = mqttSubCmd;

            return MqttSubCmdNode;
        }
        private TreeNode NewMqttPubTopic(MqttPubTopic mqttPubTopic)
        {
            string topicName = mqttPubTopic.TopicName + ":" 
                + ((int)mqttPubTopic.QosLevel).ToString()+":"
                + mqttPubTopic.NumCnl;
            TreeNode MqttPubTopicNode = new TreeNode(topicName);

            MqttPubTopicNode.ImageIndex = MqttPubTopicNode.SelectedImageIndex = 3;
            MqttPubTopicNode.Tag = mqttPubTopic;
            return MqttPubTopicNode;
        }
        private TreeNode NewMqttPubCmd(MqttPubCmd mqttPubCmd)
        {
            string topicName = mqttPubCmd.TopicName + ":" 
                + ((int)mqttPubCmd.QosLevel).ToString() + ":"
                + mqttPubCmd.NumCmd.ToString();

            TreeNode MqttPubCmdNode = new TreeNode(topicName);
            MqttPubCmdNode.ImageIndex = MqttPubCmdNode.SelectedImageIndex = 3;

            MqttPubCmdNode.Tag = mqttPubCmd;
            return MqttPubCmdNode;
        }
        private void btnSaveAs_Click(object sender, EventArgs e)
        {

        }

        private void btnAddElemRoot_Click(object sender, EventArgs e)
        {
            // 不用添加   默认就是五个根节点

            


        }

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // 显示选定对象及其属性

            selNode = e.Node;  // 当前选择的节点对象
            //string strName = selNode.Name;


        }

        private void btnEditSettings_Click(object sender, EventArgs e)
        {
            // 配置服务器 网络端口的设置
            MqttConnectionArgs arg = deviceConfigTemplate.ConnectionArgs;
               
            if (FrmMqttServerConfig.ShowDialog( ref arg) == true)
            {

            }

        }
        private void TranslateTree()
        {
            Publish.Text = KpPhrases.Publish;
            Subscription.Text = KpPhrases.Subscription;
        }
        private void FrmMqttDevTemplate_Load(object sender, EventArgs e)
        {
            // 初始化的时候加载文件 读取配置的XML 文件
            Translator.TranslateForm(this, "Scada.Comm.Devices.Mqtt.UI.FrmMqttDevTemplate");
            openFileDialog.SetFilter(KpPhrases.TemplateFileFilter);
            saveFileDialog.SetFilter(KpPhrases.TemplateFileFilter);
            TranslateTree();

            //if (saveOnly)
            {
                btnNew.Visible = false;
                btnOpen.Visible = false;
                btnSaveAs.Visible = false;
                btnMoveUp.Visible = false;
                btnMoveDown.Visible = false;
                btnAddElemGroup.Visible = false;
            }

            initialFileName = DeviceConfig.GetFileName(appDirs.ConfigDir, nKpNum, "");
            fileName = Path.GetFileName(initialFileName);
 
            saveFileDialog.FileName = fileName;

            if (!File.Exists(Path.Combine(appDirs.ConfigDir, fileName)))
                return;
            else if (!deviceConfigTemplate.LoadXMLOnly(Path.Combine(appDirs.ConfigDir, fileName), out string errMsg))
                throw new ScadaException(errMsg);

            FillTree();
            //}
        }

        private void FillTree()
        {
            // 恢复树的初始化  根据配置文件内容 填充树

           // modified = false;

            treeView.BeginUpdate();

            treeView.SelectedNode = Publish;

            // 清空所有及节点的数据
            foreach(TreeNode tnPub in Publish.Nodes)
            {
                tnPub.Nodes.Clear();
            }
            foreach (TreeNode tnSub in Subscription.Nodes)
            {
                tnSub.Nodes.Clear();
            }

            foreach(MqttSubTopic subTopic in deviceConfigTemplate.SubTopics) // 订阅
            {
                grsNodeSubTopics.Nodes.Add(NewMqttSubTopic(subTopic));
            }
            foreach (MqttSubJS subJS in deviceConfigTemplate.SubJSs) // 订阅
            {
                grsNodeSubJSs.Nodes.Add(NewMqttSubJS(subJS));
            }
            foreach (MqttSubCmd subCmds in deviceConfigTemplate.SubCmds) // 订阅
            {
                grsNodeSubCmds.Nodes.Add(NewMqttSubCmd(subCmds));
            }
            foreach (MqttPubTopic pubTopic in deviceConfigTemplate.PubTopics) // 发布
            {
                grsNodePubTopics.Nodes.Add(NewMqttPubTopic(pubTopic));
            }
            foreach (MqttPubCmd pubCmds in deviceConfigTemplate.PubCmds) // 发布
            {
                grsNodePubCmds.Nodes.Add(NewMqttPubCmd(pubCmds));
            }
            Publish.Expand();
            Subscription.Expand();

            // 恢复树绘制
            treeView.EndUpdate();
          
        }

        private void treeView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            // 弹出对应的配置对话框
            ConfigElem();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            // 删除节点
            string strTagReal = "";
            if (selNode != null)
            {
                object tag = selNode.Tag;

                MqttPubCmd pubCmd = tag as MqttPubCmd;
                MqttPubTopic pubTopic = tag as MqttPubTopic;
                MqttSubTopic subTopic = tag as MqttSubTopic;
                MqttSubCmd subCmds = tag as MqttSubCmd;
                MqttSubJS subJs = tag as MqttSubJS; // 有继承关系  注意区分

                if (pubCmd != null) strTagReal = "MqttPubCmds";
                else if (pubTopic != null) strTagReal = "MqttPubTopics";
                else if (subTopic != null && tag.GetType().Name != "MqttSubJS") strTagReal = "MqttSubTopics";
                else if (subCmds != null) strTagReal = "MqttSubCmds";
                else if (subJs != null) strTagReal = "MqttSubJSs";

                switch (strTagReal)
                {
                    case "MqttPubCmds":
                        deviceConfigTemplate.PubCmds.RemoveAt(selNode.Index);
                        grsNodePubCmds.Nodes.Remove(selNode);
                        break;
                    case "MqttPubTopics":
                        deviceConfigTemplate.PubTopics.RemoveAt(selNode.Index);
                        grsNodePubTopics.Nodes.Remove(selNode);

                        break;
                    case "MqttSubCmds":
                        deviceConfigTemplate.SubCmds.RemoveAt(selNode.Index);
                        grsNodeSubCmds.Nodes.Remove(selNode);


                        break;
                    case "MqttSubJSs":
                        deviceConfigTemplate.SubJSs.RemoveAt(selNode.Index);
                        grsNodeSubJSs.Nodes.Remove(selNode);

                        break;
                    case "MqttSubTopics":
                        deviceConfigTemplate.SubTopics.RemoveAt(selNode.Index);
                        grsNodeSubTopics.Nodes.Remove(selNode);
                        break;
                    default: break;
                }
            }

        }
    }
}
