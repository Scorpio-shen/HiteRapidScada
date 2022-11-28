using System;
using Scada.Server.Modules.TDEngineExport;

namespace Scada.Server.Modules
{

	public class ModTDEngineExportView : ModView
	{
		public ModTDEngineExportView()
		{
			base.CanShowProps = true;
		}

		public override string Descr
		{
			get
			{
				return "Real time data export to TDEngine.";
			}
		}

		public override string Version
		{
			get
			{
				return "5.0.1.1";
			}
		}

		public override void ShowProps()
		{
			FrmDBExportConfig.ShowDialog(base.AppDirs, base.ServerComm);
		}

		internal const string ModVersion = "5.0.1.1";
	}
}
