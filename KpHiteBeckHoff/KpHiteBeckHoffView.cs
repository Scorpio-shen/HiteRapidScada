﻿using KpHiteBeckHoff.Model;
using KpHiteBeckHoff.Model.EnumType;
using KpHiteBeckHoff.View;
using Scada.Data.Configuration;
using System.Collections.Generic;
using System.IO;

namespace Scada.Comm.Devices
{
    public class KpHiteBeckHoffView : KPView
    {
        public override string KPDescr => "Hite 倍福驱动v1.0";


        public KpHiteBeckHoffView() : base(0)
        {

        }

        public KpHiteBeckHoffView(int number) : base(number)
        {
            CanShowProps = true;
        }

        public override KPCnlPrototypes DefaultCnls
        {
            get
            {
                string fileName = KPProps == null ? string.Empty : KPProps.CmdLine.Trim();

                if (string.IsNullOrEmpty(fileName))
                    return null;

                var filePath = Path.IsPathRooted(fileName) ? fileName : Path.Combine(AppDirs.ConfigDir, fileName);
                var deviceTemplate = new DeviceTemplate();

                if (!deviceTemplate.Load(filePath, out string errMsg))
                    throw new ScadaException(errMsg);

                return CreateCnlPrototypes(deviceTemplate);
            }
        }

        protected virtual KPCnlPrototypes CreateCnlPrototypes(DeviceTemplate deviceTemplate)
        {
            KPCnlPrototypes prototypes = new KPCnlPrototypes();
            List<InCnlPrototype> inCnls = prototypes.InCnls;
            List<CtrlCnlPrototype> ctrlCnls = prototypes.CtrlCnls;

            int signal = 1;
            foreach (var tagGroup in deviceTemplate.TagGroups)
            {
                foreach (var tag in tagGroup.Tags)
                {
                    bool isTS = tag.DataType == DataTypeEnum.Bool;
                    int typeID = isTS ? BaseValues.CnlTypes.TS : BaseValues.CnlTypes.TI;

                    List<InCnlPrototype> addInCnls = new List<InCnlPrototype>();
                    //判读是否是数组类型
                    if(tag.IsArray)
                    {
                        //数组类型
                        for(int i = 0;i < tag.Length; i++)
                        {
                            var tagName = tag.Name + $"[{i}]";
                            InCnlPrototype inCnl = new InCnlPrototype(tagName, typeID);
                            inCnl.Signal = signal++;

                            if (isTS)
                            {
                                inCnl.ShowNumber = false;
                                inCnl.UnitName = BaseValues.UnitNames.OffOn;
                                inCnl.EvEnabled = true;
                                inCnl.EvOnChange = true;

                            }

                            if (tag.DataType == DataTypeEnum.String)
                            {
                                //string类型设置格式类型
                                inCnl.FormatID = BaseValues.Formats.AsciiText;
                            }
                            addInCnls.Add(inCnl);

                        }
                    }
                    else
                    {
                        InCnlPrototype inCnl = new InCnlPrototype(tag.Name, typeID);
                        inCnl.Signal = signal++;

                        if (isTS)
                        {
                            inCnl.ShowNumber = false;
                            inCnl.UnitName = BaseValues.UnitNames.OffOn;
                            inCnl.EvEnabled = true;
                            inCnl.EvOnChange = true;

                        }

                        if (tag.DataType == DataTypeEnum.String)
                        {
                            //string类型设置格式类型
                            inCnl.FormatID = BaseValues.Formats.AsciiText;
                        }
                        addInCnls.Add(inCnl);

                    }

                    inCnls.AddRange(addInCnls);
                }
            }

            signal = 1;
            foreach (var tag in deviceTemplate.CmdTags)
            {
                var cmdTypeID = tag.DataType == DataTypeEnum.String ? BaseValues.CmdTypes.Binary : BaseValues.CmdTypes.Standard;
                if(tag.IsArray)
                {
                    //数组类型
                    for(int i = 0;i < tag.Length; i++)
                    {
                        var tagName = tag.Name + $"[{i}]";
                        CtrlCnlPrototype ctrlCnl = new CtrlCnlPrototype(tagName, cmdTypeID);
                        ctrlCnl.CmdNum = signal++;
                        if (tag.DataType == DataTypeEnum.Bool)
                            ctrlCnl.CmdValName = BaseValues.CmdValNames.OffOn;
                        ctrlCnls.Add(ctrlCnl);
                    }
                }
                else
                {
                    CtrlCnlPrototype ctrlCnl = new CtrlCnlPrototype(tag.Name, cmdTypeID);
                    ctrlCnl.CmdNum = tag.TagID;
                    if (tag.DataType == DataTypeEnum.Bool)
                        ctrlCnl.CmdValName = BaseValues.CmdValNames.OffOn;

                    ctrlCnls.Add(ctrlCnl);
                }
            }

            return prototypes;
        }

        public override void ShowProps()
        {
            var frm = new FrmDevProps(Number, AppDirs, KPProps);
            frm.ShowDialog();
        }
    }
}
