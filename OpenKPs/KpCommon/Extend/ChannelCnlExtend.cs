using Scada.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace KpCommon.Extend
{
    public static class ChannelCnlExtend
    {
        public static InCnl InCnlCopy(this InCnl inCnl)
        {
            return new InCnl()
            {
                CnlTypeID = inCnl.CnlTypeID,
                FormatID = inCnl.FormatID,
                ParamID = inCnl.ParamID,
                UnitID = inCnl.UnitID,
                Active = inCnl.Active,
                Averaging = inCnl.Averaging,
                CnlNum = inCnl.CnlNum,
                CtrlCnlNum = inCnl.CtrlCnlNum,
                EvEnabled = inCnl.EvEnabled,
                EvOnChange = inCnl.EvOnChange,
                EvOnUndef = inCnl.EvOnUndef,
                EvSound = inCnl.EvSound,
                Formula = inCnl.Formula,
                FormulaUsed = inCnl.FormulaUsed,
                KPNum = inCnl.KPNum,
                LimHigh = inCnl.LimHigh,
                LimHighCrash = inCnl.LimHighCrash,
                LimLow = inCnl.LimLow,
                LimLowCrash = inCnl.LimLowCrash,
                Name = inCnl.Name,
                ObjNum = inCnl.ObjNum,
                Signal = inCnl.Signal,
            };
        }

        public static CtrlCnl CtrlCnlCopy(this CtrlCnl ctrlCnl)
        {
            return new CtrlCnl
            {
                Active = ctrlCnl.Active,
                CmdNum = ctrlCnl.CmdNum,
                CmdTypeID = ctrlCnl.CmdTypeID,
                CmdValID = ctrlCnl.CmdValID,
                CtrlCnlNum = ctrlCnl.CtrlCnlNum,
                EvEnabled = ctrlCnl.EvEnabled,
                Formula = ctrlCnl.Formula,
                FormulaUsed = ctrlCnl.FormulaUsed,
                KPNum = ctrlCnl.KPNum,
                Name = ctrlCnl.Name,
                ObjNum = ctrlCnl.ObjNum,
            };
        }
    }
}
