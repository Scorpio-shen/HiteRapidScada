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
using System.IO;
using Jint;
using StriderMqtt;
using Scada.Comm.Devices;
using Scada.UI;

namespace Scada.Comm.Devices.Mqtt.UI
{
    public partial class FrmMqttSubJSs : Form
    {
        // 定义五中类型的数据
        private bool modified; // 窗口对应的值被修改 此值置位true
        private string strOldTopic = "";
        private int nOldQosLevel = -1;
        private int nOldChannelCount = -1;
        private string strOldFileName = "";
       // private string strOldServerConfig = "";
        private AppDirs appDirs;                 // 应用程序目录

        int nSelIndex;
        List<MqttSubJS> SubJSs;

        List<string> listJsName = new List<string>();
        ListViewEx listView_JS = new ListViewEx();
             
        public FrmMqttSubJSs()
        {
            InitializeComponent();

            this.panelJS.Controls.Add(listView_JS);
            
        }

        public static bool ShowDialog(ref MqttSubJS subJs,AppDirs appDirs, List<MqttSubJS> SubJSs, int nSelIndex)
        {
            if (subJs == null)
                throw new ArgumentNullException("subJs");

            FrmMqttSubJSs form = new FrmMqttSubJSs();
            form.appDirs = appDirs;
            form.SubJSs = SubJSs;
            form.nSelIndex = nSelIndex;
            form.SettingsToControls(subJs);

            if (form.ShowDialog() == DialogResult.OK)
            {
                form.ControlsToSettings(subJs);

                if (form.modified == true)
                {
                    return true;
                }
                else return false;
            }
            else
            {
                return false;
            }
        }
        private void SettingsToControls(MqttSubJS subJs)
        {
            if (Localization.UseRussian)
            {
                comboBox_QosLevel.Items.Insert(0, "最多一次");
                comboBox_QosLevel.Items.Insert(1, "至少一次");
                comboBox_QosLevel.Items.Insert(2, "只有一次");
            }
            else
            {
                comboBox_QosLevel.Items.Insert(0, "AtMostOnce");
                comboBox_QosLevel.Items.Insert(1, "AtLeastOnce");
                comboBox_QosLevel.Items.Insert(2, "ExactlyOnce");
            }
            nOldQosLevel = comboBox_QosLevel.SelectedIndex = (int)subJs.QosLevel;

            strOldTopic = textBox_Topic.Text = subJs.TopicName;

            textBox_CnlCnt.Text = subJs.CnlCnt.ToString();

            nOldChannelCount = subJs.CnlCnt;

            if(!string.IsNullOrEmpty(Path.GetDirectoryName(subJs.JSHandlerPath)))
            {

                //strOldServerConfig = Path.GetDirectoryName (subJs.JSHandlerPath);
                //textBox_ServerConfig.Text = strOldServerConfig;
            }
            else
            {
                //strOldServerConfig = textBox_ServerConfig.Text;
            }
            strOldFileName = textBox_JSHandlePath.Text = Path.GetFileName(subJs.JSHandlerPath);
            if(string.IsNullOrEmpty(strOldFileName))
            {
                strOldFileName = textBox_JSHandlePath.Text = "KpMqtt_Job.js";
            }

            modified = false;
        }

        private void ControlsToSettings(MqttSubJS subJs)
        {
            subJs.QosLevel = (StriderMqtt.MqttQos)comboBox_QosLevel.SelectedIndex;

            subJs.TopicName=  textBox_Topic.Text;

            subJs.CnlCnt  = int.Parse(textBox_CnlCnt.Text);

            subJs.JSHandlerPath = Path.Combine("C:\\SCADA\\ScadaComm\\Config", textBox_JSHandlePath.Text);

            if (comboBox_QosLevel.SelectedIndex != nOldQosLevel
                || textBox_Topic.Text != strOldTopic
                || int.Parse(textBox_CnlCnt.Text) != nOldChannelCount
                || textBox_JSHandlePath.Text != strOldFileName   )
            {
                modified = true;
            }

        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (comboBox_QosLevel.SelectedIndex != nOldQosLevel
               || textBox_Topic.Text != strOldTopic
               || int.Parse(textBox_CnlCnt.Text) != nOldChannelCount
               || textBox_JSHandlePath.Text != strOldFileName)
            {
                modified = true;
            }

            for (int index = 0; index < SubJSs.Count; index++)
            {
                if (nSelIndex != index)
                {
                    if (textBox_Topic.Text == SubJSs[index].TopicName)
                    {
                        MessageBox.Show(string.Format(Localization.UseRussian ?
                            "{0} 命名重复" : "The Topic Name {0} was Repeated", textBox_Topic.Text));

                        return;
                    }
                }
            }
            // 把内容 整理出来  并保存为文件

            listJsName.Clear();

            string strContent = "var data = JSON.parse(InMsg);\r\n\r";

            for (int i = 1; i <= listView_JS.Items.Count; i++)
            {

                string strTag = listView_JS.Items[i - 1].SubItems[1].Text;
                //jsvals[0].TagName = "temp";
                // jsvals[0].Stat = data.Stat;
                //jsvals[0].Val = data.temp;
                string strTemp;

                strTemp = string.Format("jsvals[{0}].TagName = \"{1}\";\r", i - 1, strTag);
                strContent += strTemp;

                strTemp = string.Format("jsvals[{0}].Stat = data.Stat;\r", i - 1);
                strContent += strTemp;

                strTemp = string.Format("jsvals[{0}].Val = data.{1};\r\r", i - 1, strTag);
                strContent += strTemp;


            }
            strContent += "mylog(\"Script completed successfully\");";
            // 写进文件

            // 拼接字符串
            //"KpMqtt_Job.js"
            if(modified)
            {
                if (!string.IsNullOrEmpty(textBox_JSHandlePath.Text.Trim()))
                {

                    if(File.Exists(Path.Combine(appDirs.ConfigDir, textBox_JSHandlePath.Text.Trim())))
                    {
                        // 提示是否替换
                        string strTipsContent = string.Format(Localization.UseRussian ?
                                "文件 {0} 已存在，确定替换?" : "The file {0} already exists,are you sure to replace it？", textBox_JSHandlePath.Text.Trim());
                        string strTips = string.Format(Localization.UseRussian ?
                                "提示" : "Tips");
                        if (MessageBox.Show(strTipsContent, strTips, MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == System.Windows.Forms.DialogResult.OK)
                        {
                            System.IO.File.WriteAllText(Path.Combine(appDirs.ConfigDir, textBox_JSHandlePath.Text.Trim()), strContent);
                        }
                        else
                        {
                            return;
                        }
                    }
                
                }
                else
                {
                    System.IO.File.WriteAllText(Path.Combine(appDirs.ConfigDir, "KpMqtt_Job.js"), strContent);

                }

            }         
            this.DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {

        }

        private void FrmMqttSubJSs_Load(object sender, EventArgs e)
        {
            // strOldServerConfig = textBox_ServerConfig.Text;
            Translator.TranslateForm(this, "Scada.Comm.Devices.Mqtt.UI.FrmMqttSubJSs");

            InitListView(listView_JS);
            loadJsFile();
            UpdateListView();
        }

        private void button_SelJShandlerPath_Click(object sender, EventArgs e)
        {
            // 选择文件   文件拷贝   设置当前控件的显示
            //openFileDialog.FileName = appDirs.ConfigDir;
            openFileDialog.InitialDirectory = appDirs.ConfigDir;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string strFileName = Path.GetFileName(openFileDialog.FileName);
                textBox_JSHandlePath.Text = strFileName;  // 需要配置

                // 查找当前配置文件的目录中是否有此文件
                if (!File.Exists(Path.Combine(appDirs.ConfigDir, strFileName)))
                {
                    // System.IO.FileInfo file =
                    System.IO.File.Copy(openFileDialog.FileName, Path.Combine(appDirs.ConfigDir, strFileName));//复制文件
                }
                else if (openFileDialog.FileName != Path.Combine(appDirs.ConfigDir, strFileName))
                {
                    string strTipsContent = string.Format(Localization.UseRussian ?
                            "文件已存在，确定替换?" : "The file already exists,are you sure to replace it？");
                    string strTips = string.Format(Localization.UseRussian ?
                            "提示" : "Tips");
                    if (MessageBox.Show(strTipsContent, strTips, MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == System.Windows.Forms.DialogResult.OK)
                    {
                        System.IO.File.Copy(openFileDialog.FileName, Path.Combine(appDirs.ConfigDir, strFileName), true);
                    }                  
                }
                else
                {
                    
                    
                }

            }
        }

        private void button_Add_Click(object sender, EventArgs e)
        {
            if (listView_JS.Items.Count >= int.Parse(textBox_CnlCnt.Text))
            {
                MessageBox.Show(string.Format(Localization.UseRussian ?
                            "添加数量不能大于设定值" : "The Number of List Items cannot greater than the setting value"));
                return; 
            }
            if (!string.IsNullOrEmpty(textBox_Lable.Text.Trim()))
            {
                //listJsName.Add(textBox_Lable.Text.Trim());

                ListViewItem li = new ListViewItem((listView_JS.Items.Count + 1).ToString() );
                li.SubItems.Add(textBox_Lable.Text.Trim());

                listView_JS.Items.Add(li);
                modified = true;
            }
                
        }

        private void button_Delete_Click(object sender, EventArgs e)
        {
            // 删除选中的某一列

            if(listView_JS.SelectedItems.Count >0)
            {
                modified = true;
                listView_JS.Items.Remove(listView_JS.SelectedItems[0]);
                // 重排序号
            
                for(int i= 1;i<= listView_JS.Items.Count;i++)
                {
                    listView_JS.Items[i-1].Text = i.ToString();
                    //listView_JS.Items.c
                }
            }



        }
        public void InitListView(ListViewEx lv)
        {
            lv.GridLines = true;//显示网格线

            lv.FullRowSelect = true;//单击某项的时候选择多行

            lv.View = View.Details;//显示格式；

            lv.Scrollable = true;//添加滚动条

            lv.MultiSelect = false;//不允许选择多行；

            lv.HeaderStyle = ColumnHeaderStyle.Nonclickable;

            lv.Dock = DockStyle.Fill;

            //添加列名
            ColumnHeader c1 = new ColumnHeader();
            c1.Width = 100;
            
            c1.Text = string.Format(Localization.UseRussian ? "序号" : "Number"); 
            ColumnHeader c2 = new ColumnHeader();
            c2.Width = 100;
            c2.Text = string.Format(Localization.UseRussian ? "名称" : "Name");

            //设置属性
            //lv.GridLines = true;  //显示网格线
            //lv.FullRowSelect = false;  //显示全行
            //lv.MultiSelect = false;  //设置只能单选
            //lv.View = View.List;  //设置显示模式为详细
            //lv.HoverSelection = true;  //当鼠标停留数秒后自动选择
            //lv.LabelEdit = true;
            //把列名添加到listview中
            lv.Columns.Add(c1);
            lv.Columns.Add(c2);
           
        }
        public static string ToAlphaNumeric(string input)
        {
            int j = 0;
            char[] newCharArr = new char[input.Length];

            for (int i = 0; i < input.Length; i++)
            {
                if (char.IsLetterOrDigit(input[i]))
                {
                    newCharArr[j] = input[i];
                    j++;
                }
            }

            Array.Resize(ref newCharArr, j);

            return new string(newCharArr);
        }
        public void loadJsFile()
        {
            // 从当前文件中 读取标签名称信息
            string FilePath = Path.Combine(appDirs.ConfigDir, strOldFileName);
            if (File.Exists(FilePath))
            {
                // 加载文件 读取标签
                string strContent = LoadJSHandler(FilePath);


                
                int nIndexTagNameStart = strContent.IndexOf("TagName");
                while(nIndexTagNameStart > 0)
                {
                    string strSub = strContent.Substring(nIndexTagNameStart);

                    int nIndexTagNameEqual = strSub.IndexOf("=");
                    int nIndexTagNameEnd = strSub.IndexOf(";");

                    string strLable = strSub.Substring(nIndexTagNameEqual + 1, nIndexTagNameEnd - nIndexTagNameEqual - 1);

                    strLable = ToAlphaNumeric(strLable);

                    listJsName.Add(strLable.Trim());

                    strContent = strSub.Substring(nIndexTagNameEnd);

                    nIndexTagNameStart = strContent.IndexOf("TagName");
                }


            }
            
        }
        public void  UpdateListView()
        {
            int nIndex = 1;
            foreach(string strname in listJsName)
            {
                ListViewItem li = new ListViewItem(nIndex.ToString());
                li.SubItems.Add(listJsName[nIndex - 1]);
               
                listView_JS.Items.Add(li);
                nIndex++;
            }
        }
        public string LoadJSHandler(string strFilePath)
        {
            string strFileContent = "";
            if (string.IsNullOrEmpty(strFilePath))
            {
                return strFileContent;
            }
            else
            {
                using (StreamReader reader = new StreamReader(strFilePath))
                {
                    strFileContent =  reader.ReadToEnd();
                }
                return strFileContent;

            }
        }

    }
    public class ListViewEx : ListView

    {

        private TextBox m_tb;

        private ComboBox m_cb;



        public ListViewEx()

        {

            m_tb = new TextBox();

            m_cb = new ComboBox();

            m_tb.Multiline = true;

            m_tb.Visible = false;

            m_cb.Visible = false;

            this.Controls.Add(m_tb);//把当前的textbox加入到当前容器里

            this.Controls.Add(m_cb);

            // this.Controls.

        }

        private void EditItem(ListViewItem.ListViewSubItem subItem)

        {

            if (this.SelectedItems.Count <= 0)

            {

                return;

            }



            Rectangle _rect = subItem.Bounds;



            m_tb.Bounds = _rect;

            m_tb.BringToFront();



            m_tb.Text = subItem.Text;

            m_tb.Leave += new EventHandler(tb_Leave);

            m_tb.TextChanged += new EventHandler(m_tb_TextChanged);

            m_tb.Visible = true;

            m_tb.Tag = subItem;

            m_tb.Select();

        }

        private void EditItem(ListViewItem.ListViewSubItem subItem, Rectangle rt)

        {

            if (this.SelectedItems.Count <= 0)

            {

                return;

            }



            Rectangle _rect = rt;

            m_cb.Bounds = _rect;

            m_cb.BringToFront();

            m_cb.Items.Add(subItem.Text);

            m_cb.Text = subItem.Text;

            m_cb.Leave += new EventHandler(lstb_Leave);

            m_cb.TextChanged += new EventHandler(m_lstb_TextChanged);

            m_cb.Visible = true;

            m_cb.Tag = subItem;

            m_cb.Select();

        }





        protected override void OnKeyDown(KeyEventArgs e)

        {

            if (e.KeyCode == Keys.F2)

            {



                if (this.SelectedItems.Count > 0)

                {

                    //this.SelectedItems[0].BeginEdit();

                    ListViewItem lvi = this.SelectedItems[0];

                    EditItem(lvi.SubItems[0], new Rectangle(lvi.Bounds.Left, lvi.Bounds.Top, this.Columns[0].Width, lvi.Bounds.Height - 2));

                }

            }

            base.OnKeyDown(e);

        }



        protected override void OnSelectedIndexChanged(EventArgs e)

        {

            this.m_tb.Visible = false;

            this.m_cb.Visible = false;

            base.OnSelectedIndexChanged(e);

        }



        //protected override void OnDoubleClick(EventArgs e)

        //{

        //    Point tmpPoint = this.PointToClient(Cursor.Position);

        //    ListViewItem item = this.GetItemAt(tmpPoint.X, tmpPoint.Y);

        //    if (item != null)

        //    {

        //        if (tmpPoint.X > this.Columns[0].Width && tmpPoint.X < this.Width)

        //        {

        //            EditItem(1);

        //        }

        //    }



        //    base.OnDoubleClick(e);

        //}

        protected override void OnDoubleClick(EventArgs e)

        {

            Point tmpPoint = this.PointToClient(Cursor.Position);



            ListViewItem.ListViewSubItem subitem = this.HitTest(tmpPoint).SubItem;

            ListViewItem item = this.HitTest(tmpPoint).Item;

            if (subitem != null)

            {

                if (item.SubItems[0].Equals(subitem))

                {

                    //EditItem(subitem, new Rectangle(item.Bounds.Left, item.Bounds.Top, this.Columns[2].Width, item.Bounds.Height - 2));

                }

                else

                {

                    EditItem(subitem);

                }

            }



            base.OnDoubleClick(e);

        }





        protected override void WndProc(ref Message m)

        {

            if (m.Msg == 0x115 || m.Msg == 0x114)

            {

                this.m_tb.Visible = false;

            }

            base.WndProc(ref m);

        }



        private void tb_Leave(object sender, EventArgs e)

        {

            m_tb.TextChanged -= new EventHandler(m_tb_TextChanged);

            (sender as TextBox).Visible = false;

        }



        private void m_tb_TextChanged(object sender, EventArgs e)

        {

            if ((sender as TextBox).Tag is ListViewItem.ListViewSubItem)

            {

                (this.m_tb.Tag as ListViewItem.ListViewSubItem).Text = this.m_tb.Text;

            }



        }

        private void lstb_Leave(object sender, EventArgs e)

        {

            m_cb.TextChanged -= new EventHandler(m_lstb_TextChanged);

        }

        private void m_lstb_TextChanged(object sender, EventArgs e)

        {

            if ((sender as ListBox).Tag is ListViewItem.ListViewSubItem)

            {

                (this.m_cb.Tag as ListViewItem.ListViewSubItem).Text = this.m_cb.Text;

            }

        }

    }
}
