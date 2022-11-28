using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Scada.Data.Models;
using Scada.Data.Tables;
using Scada.Server.Modules.TDEngineExport;
using Utils;
using static Utils.Log;

namespace Scada.Server.Modules
{
	public class ModTDEngineExportLogic : ModLogic
	{
		public ModTDEngineExportLogic()
		{
			this.normalWork = true;
			this.workState = (Localization.UseRussian ? "норма" : "normal");
			this.log = null;
			this.infoFileName = "";
			this.infoThread = null;
			this.config = null;
			this.exporters = null;
		}

		public override string Name
		{
			get
			{
				return "ModTDEngineExport";
			}
		}

		private void GetCmdParams(Command cmd, out string dataSourceName, out DateTime dateTime)
		{
			string[] array = cmd.GetCmdDataStr().Split(new char[]
			{
				'\n'
			});
			dataSourceName = array[0];
			try
			{
				dateTime = ScadaUtils.XmlParseDateTime(array[1]);
			}
			catch
			{
				dateTime = DateTime.MinValue;
			}
		}

		private Exporter FindExporter(string dataSourceName)
		{
			foreach (Exporter exporter in this.exporters)
			{
				if (exporter.DataSource.Name == dataSourceName)
				{
					return exporter;
				}
			}
			return null;
		}

		private void ExportCurDataFromFile(Exporter exporter)
		{
			SrezTableLight srezTableLight = new SrezTableLight();
			SrezAdapter srezAdapter = new SrezAdapter();
			srezAdapter.FileName = ServerUtils.BuildCurFileName(base.Settings.ArcDir);
			try
			{
				srezAdapter.Fill(srezTableLight);
			}
			catch (Exception ex)
			{
				this.log.WriteAction(string.Format(Localization.UseRussian ? "Ошибка при загрузке текущего среза из файла {0}: {1}" : "Error loading current data from file {0}: {1}", srezAdapter.FileName, ex.Message));
			}
			if (srezTableLight.SrezList.Count > 0)
			{
				SrezTableLight.Srez srez = srezTableLight.SrezList.Values[0];
				SrezTableLight.Srez curSrez = new SrezTableLight.Srez(DateTime.Now, srez.CnlNums, srez);
				exporter.EnqueueCurData(curSrez);
				this.log.WriteAction(Localization.UseRussian ? "Текущие данные добавлены в очередь экспорта" : "Current data added to export queue");
				return;
			}
			this.log.WriteAction(Localization.UseRussian ? "Отсутствуют текущие данные для экспорта" : "No current data to export");
		}

		private void ExportArcDataFromFile(Exporter exporter, DateTime dateTime)
		{
			SrezTableLight srezTableLight = new SrezTableLight();
			SrezAdapter srezAdapter = new SrezAdapter();
			srezAdapter.FileName = ServerUtils.BuildMinFileName(base.Settings.ArcDir, dateTime);
			try
			{
				srezAdapter.Fill(srezTableLight);
			}
			catch (Exception ex)
			{
				this.log.WriteAction(string.Format(Localization.UseRussian ? "Ошибка при загрузке таблицы минутных срезов из файла {0}: {1}" : "Error loading minute data table from file {0}: {1}", srezAdapter.FileName, ex.Message));
			}
			SrezTableLight.Srez srez = srezTableLight.GetSrez(dateTime);
			if (srez == null)
			{
				this.log.WriteAction(Localization.UseRussian ? "Отсутствуют архивные данные для экспорта" : "No archive data to export");
				return;
			}
			exporter.EnqueueArcData(srez);
			this.log.WriteAction(Localization.UseRussian ? "Архивные данные добавлены в очередь экспорта" : "Archive data added to export queue");
		}

		private void ExportEventsFromFile(Exporter exporter, DateTime date)
		{
#if guocj_test_tdenger
			EventTableLight eventTableLight = new EventTableLight();
			EventAdapter eventAdapter = new EventAdapter();
			eventAdapter.FileName = ServerUtils.BuildEvFileName(base.Settings.ArcDir, date);
			try
			{
				eventAdapter.Fill(eventTableLight);
			}
			catch (Exception ex)
			{
				this.log.WriteAction(string.Format(Localization.UseRussian ? "Ошибка при загрузке таблицы событий из файла {0}: {1}" : "Error loading event table from file {0}: {1}", eventAdapter.FileName, ex.Message));
			}
			if (eventTableLight.AllEvents.Count > 0)
			{
				foreach (EventTableLight.Event ev in eventTableLight.AllEvents)
				{
					exporter.EnqueueEvent(ev);
				}
				this.log.WriteAction(Localization.UseRussian ? "События добавлены в очередь экспорта" : "Events added to export queue");
				return;
			}
			this.log.WriteAction(Localization.UseRussian ? "Отсутствуют события для экспорта" : "No events to export");
#endif       
		}

		private void WriteInfo()
		{
			try
			{
				StringBuilder stringBuilder = new StringBuilder();
				if (Localization.UseRussian)
				{
					stringBuilder.AppendLine("Модуль экспорта данных").AppendLine("----------------------").Append("Состояние: ").AppendLine(this.workState).AppendLine().AppendLine("Источники данных").AppendLine("----------------");
				}
				else
				{
					stringBuilder.AppendLine("Export Data Module").AppendLine("------------------").Append("State: ").AppendLine(this.workState).AppendLine().AppendLine("Data Sources").AppendLine("------------");
				}
				int num = (this.exporters == null) ? 0 : this.exporters.Count;
				if (num > 0)
				{
					for (int i = 0; i < num; i++)
					{
						stringBuilder.Append((i + 1).ToString()).Append(". ").AppendLine(this.exporters[i].GetInfo());
					}
				}
				else
				{
					stringBuilder.AppendLine(Localization.UseRussian ? "Нет" : "No");
				}
				using (StreamWriter streamWriter = new StreamWriter(this.infoFileName, false, Encoding.UTF8))
				{
					streamWriter.Write(stringBuilder.ToString());
				}
			}
			catch (ThreadAbortException)
			{
			}
			catch (Exception ex)
			{
				this.log.WriteAction(ModPhrases.WriteInfoError + ": " + ex.Message, ActTypes.Exception);
			}
		}

		public override void OnServerStart()
		{
			this.log = new Log(0)
			{
				Encoding = Encoding.UTF8,
				FileName = base.AppDirs.LogDir + "ModTDEngineExport.log"
			};
			this.log.WriteBreak();
			this.log.WriteAction(string.Format(ModPhrases.StartModule, this.Name));
			this.infoFileName = base.AppDirs.LogDir + "ModTDEngineExport.txt";
			this.config = new ModConfig(base.AppDirs.ConfigDir);
			string text;
			if (this.config.Load(out text))
			{
				this.exporters = new List<Exporter>();
				foreach (ModConfig.ExportDestination expDest in this.config.ExportDestinations)
				{
					Exporter exporter = new Exporter(expDest, this.log);
					this.exporters.Add(exporter);
					exporter.Start();
				}
				this.infoThread = new Thread(delegate()
				{
					for (;;)
					{
						this.WriteInfo();
						Thread.Sleep(500);
					}
				});
				this.infoThread.Start();
				return;
			}
			this.normalWork = false;
			this.workState = (Localization.UseRussian ? "ошибка" : "error");
			this.WriteInfo();
			this.log.WriteAction(text);
			this.log.WriteAction(ModPhrases.NormalModExecImpossible);
		}

		public override void OnServerStop()
		{
			foreach (Exporter exporter in this.exporters)
			{
				exporter.Terminate();
			}
			DateTime now = DateTime.Now;
			DateTime t = now;
			DateTime t2 = now.AddMilliseconds(7000.0);
			bool flag;
			do
			{
				flag = false;
				using (List<Exporter>.Enumerator enumerator = this.exporters.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (enumerator.Current.Running)
						{
							flag = true;
							break;
						}
					}
				}
				if (flag)
				{
					Thread.Sleep(100);
				}
				now = DateTime.Now;
			}
			while (t <= now && now <= t2 && flag);
			if (flag)
			{
				foreach (Exporter exporter2 in this.exporters)
				{
					if (exporter2.Running)
					{
						exporter2.Abort();
					}
				}
			}
			if (this.infoThread != null)
			{
				this.infoThread.Abort();
				this.infoThread = null;
			}
			this.workState = (Localization.UseRussian ? "остановлен" : "stopped");
			this.WriteInfo();
			this.log.WriteAction(string.Format(ModPhrases.StopModule, this.Name));
			this.log.WriteBreak();
		}

		public override void OnCurDataProcessed(int[] cnlNums, SrezTableLight.Srez curSrez)
		{
			if (this.normalWork)
			{
				SrezTableLight.Srez curSrez2 = new SrezTableLight.Srez(DateTime.Now, cnlNums, curSrez);
				foreach (Exporter exporter in this.exporters)
				{
					exporter.EnqueueCurData(curSrez2);
				}
			}
		}

		public override void OnArcDataProcessed(int[] cnlNums, SrezTableLight.Srez arcSrez)
		{
			if (this.normalWork)
			{
				SrezTableLight.Srez arcSrez2 = new SrezTableLight.Srez(arcSrez.DateTime, cnlNums, arcSrez);
				foreach (Exporter exporter in this.exporters)
				{
					exporter.EnqueueArcData(arcSrez2);
				}
			}
		}

			public override void OnEventCreated(EventTableLight.Event ev)
		{
			if (this.normalWork)
			{
				foreach (Exporter exporter in this.exporters)
				{
					exporter.EnqueueEvent(ev);
				}
			}
		}

		public override void OnCommandReceived(int ctrlCnlNum, Command cmd, int userID, ref bool passToClients)
		{
			if (this.normalWork)
			{
				bool flag = ctrlCnlNum == this.config.CurDataCtrlCnlNum;
				bool flag2 = ctrlCnlNum == this.config.ArcDataCtrlCnlNum;
				bool flag3 = ctrlCnlNum == this.config.EventsCtrlCnlNum;
				bool flag4 = true;
				if (flag)
				{
					this.log.WriteAction(Localization.UseRussian ? "Получена команда экспорта текущих данных" : "Export current data command received");
				}
				else if (flag2)
				{
					this.log.WriteAction(Localization.UseRussian ? "Получена команда экспорта архивных данных" : "Export archive data command received");
				}
				else if (flag3)
				{
					this.log.WriteAction(Localization.UseRussian ? "Получена команда экспорта событий" : "Export events command received");
				}
				else
				{
					flag4 = false;
				}
				if (flag4)
				{
					passToClients = false;
					if (cmd.CmdTypeID == 1)
					{
						string text;
						DateTime dateTime;
						this.GetCmdParams(cmd, out text, out dateTime);
						if (text == "")
						{
							this.log.WriteLine(string.Format(Localization.UseRussian ? "Источник данных не задан" : "Data source is not specified", new object[0]));
							return;
						}
						Exporter exporter = this.FindExporter(text);
						if (exporter == null)
						{
							this.log.WriteLine(string.Format(Localization.UseRussian ? "Неизвестный источник данных {0}" : "Unknown data source {0}", text));
							return;
						}
						this.log.WriteLine(string.Format(Localization.UseRussian ? "Источник данных: {0}" : "Data source: {0}", text));
						if (flag)
						{
							this.ExportCurDataFromFile(exporter);
							return;
						}
						if (flag2)
						{
							if (dateTime == DateTime.MinValue)
							{
								this.log.WriteLine(string.Format(Localization.UseRussian ? "Некорректная дата и время" : "Incorrect date and time", new object[0]));
								return;
							}
							this.log.WriteLine(string.Format(Localization.UseRussian ? "Дата и время: {0:G}" : "Date and time: {0:G}", dateTime));
							this.ExportArcDataFromFile(exporter, dateTime);
							return;
						}
						else
						{
							if (dateTime == DateTime.MinValue)
							{
								this.log.WriteLine(string.Format(Localization.UseRussian ? "Некорректная дата" : "Incorrect date", new object[0]));
								return;
							}
							this.log.WriteLine(string.Format(Localization.UseRussian ? "Дата: {0:d}" : "Date: {0:d}", dateTime));
							this.ExportEventsFromFile(exporter, dateTime);
							return;
						}
					}
					else
					{
						this.log.WriteAction(ModPhrases.IllegalCommand);
					}
				}
			}
		}

		internal const string LogFileName = "ModTDEngineExport.log";

		private const string InfoFileName = "ModTDEngineExport.txt";

		private const int InfoThreadDelay = 500;

		private bool normalWork;

		private string workState;

		private Log log;

		private string infoFileName;

		private Thread infoThread;

		private ModConfig config;

		private List<Exporter> exporters;
	}
}
