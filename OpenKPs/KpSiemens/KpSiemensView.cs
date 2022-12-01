using KpSiemens.Siemens.Model;
using KpSiemens.Siemens.View;
using Scada.Data.Configuration;
using Scada.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scada.Comm.Devices
{
    public class KpSiemensView : KPView
    {
        public KpSiemensView() : this(0)
        {

        }

        public KpSiemensView(int number) : base(number)
        {
            CanShowProps = true;
        }

        public override string KPDescr
        {
            get => "西门子PLC驱动";
        }

        public override KPCnlPrototypes DefaultCnls
        {
            get
            {
                string fileName = KPProps == null ? string.Empty : KPProps.CmdLine.Trim();

                if(string.IsNullOrEmpty(fileName))
                    return null;

                var filePath = Path.IsPathRooted(fileName) ? fileName : Path.Combine(AppDirs.ConfigDir, fileName);
                var deviceTemplate = new DeviceTemplate();

                if(!deviceTemplate.Load(filePath,out string errMsg))
                    throw new ScadaException(errMsg);

                return CreateCnlPrototypes(deviceTemplate);
            }
        }
        /// <summary>
        /// 获取驱动对应通道
        /// </summary>
        /// <param name="deviceTemplate"></param>
        /// <returns></returns>
        protected virtual KPCnlPrototypes CreateCnlPrototypes(DeviceTemplate deviceTemplate)
        {
            KPCnlPrototypes prototypes = new KPCnlPrototypes();
            List<InCnlPrototype> inCnls = prototypes.InCnls;
            List<CtrlCnlPrototype> ctrlCnls = prototypes.CtrlCnls;

            int signal = 1;
            foreach(var tagGroup in deviceTemplate.TagGroups)
            {
                foreach(var tag in tagGroup.Tags)
                {
                    bool isTS = tag.DataType == DataTypeEnum.Bool;
                    int typeID = isTS ? BaseValues.CnlTypes.TS : BaseValues.CnlTypes.TI;

                    InCnlPrototype inCnl = new InCnlPrototype(tag.Name, typeID);
                    inCnl.Signal = signal++;

                    if (isTS)
                    {
                        inCnl.ShowNumber = false;
                        inCnl.UnitName = BaseValues.UnitNames.OffOn;
                        inCnl.EvEnabled = true;
                        inCnl.EvOnChange = true;
                        
                    }

                    if(tag.DataType == DataTypeEnum.String)
                    {
                        //string类型设置格式类型
                        inCnl.FormatID = BaseValues.Formats.AsciiText;
                    }

                    inCnls.Add(inCnl);
                }
            }

            foreach(var tag in deviceTemplate.CmdTags)
            {
                var cmdTypeID = tag.DataType == DataTypeEnum.String ? BaseValues.CmdTypes.Binary : BaseValues.CmdTypes.Standard;
                CtrlCnlPrototype ctrlCnl = new CtrlCnlPrototype(tag.Name, cmdTypeID);
                ctrlCnl.CmdNum = tag.TagID;
                if (tag.DataType == DataTypeEnum.Bool)
                    ctrlCnl.CmdValName = BaseValues.CmdValNames.OffOn;

                ctrlCnls.Add(ctrlCnl);
            }

            return prototypes;
        }

        public override void ShowProps()
        {
            var frmDev = new FrmDevProps(Number, AppDirs,KPProps);
            frmDev.ShowDialog();
        }

        //protected virtual void Localize()
        //{
        //    if(!Localization.LoadDictionaries(AppDirs.LangDir, "KpSiemens",out string errMsg))
        //        ScadaUiUtils.ShowError(errMsg);

        //    Kp
        //}
    }
}
