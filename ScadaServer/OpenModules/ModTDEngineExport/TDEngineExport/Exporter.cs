using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Scada.Data.Tables;
using Utils;

namespace Scada.Server.Modules.TDEngineExport
{
	internal class Exporter
	{

		private Exporter()
		{
		}


		public Exporter(ModConfig.ExportDestination expDest, Log log)
		{
			if (expDest == null)
			{
				throw new ArgumentNullException("expDest");
			}
			if (log == null)
			{
				throw new ArgumentNullException("log");
			}
			this.log = log;
			this.maxQueueSize = expDest.ExportParams.MaxQueueSize;
			this.curSrezQueue = new Queue<SrezTableLight.Srez>(this.maxQueueSize);
			this.arcSrezQueue = new Queue<SrezTableLight.Srez>(this.maxQueueSize);
			this.evQueue = new Queue<EventTableLight.Event>(this.maxQueueSize);
			this.thread = null;
			this.terminated = false;
			this.running = false;
			this.ResetStats();
			this.DataSource = expDest.DataSource;
			this.ExportParams = expDest.ExportParams;

		}

		public DataSource DataSource { get; private set; }

		public ModConfig.ExportParams ExportParams { get; private set; }

		public bool Running
		{
			get
			{
				return this.running;
			}
		}

		private void InitExportCnlNums()
		{
			if (this.ExportParams.ExportCurData)
			{
				this.log.WriteInfo("InitExportCnlNums:" + this.ExportParams.ExportCurDataCnls);
				this.GenerateCnlNumList(this.expCurCnlList, this.ExportParams.ExportCurDataCnls);
				this.DataSource.CreateTable(DbTableType.CnlData, this.expCurCnlList);
			}
			if (this.ExportParams.ExportArcData)
			{
				this.log.WriteInfo("InitExportCnlNums:" + this.ExportParams.ExportArcDataCnls);
				this.GenerateCnlNumList(this.expArcCnlList, this.ExportParams.ExportArcDataCnls);
				this.DataSource.CreateTable(DbTableType.CnlData, this.expArcCnlList);
			}
			if (this.ExportParams.ExportEvents)
			{
				this.log.WriteInfo("InitExportCnlNums:" + this.ExportParams.ExportEventCnls);
				this.GenerateCnlNumList(this.expEventCnlList, this.ExportParams.ExportEventCnls);
				this.DataSource.CreateTable(DbTableType.Events, this.expEventCnlList);
			}
			if (this.ExportParams.ExportEvents)
			{
				this.log.WriteInfo("InitExportCnlNums:" + this.ExportParams.ExportEventCnls);
				this.GenerateCnlNumList(this.expEventCnlList, this.ExportParams.ExportEventCnls);
				this.DataSource.CreateTable(DbTableType.EVentsAll, this.expEventCnlList);
			}

			eventNum = this.DataSource.getCountTable("events");
			this.log.WriteInfo("count:  "+ eventNum.ToString());

		}

		private void GenerateCnlNumList(List<int> cnlNumList, string cnlNumStr)
		{
			try
			{
				if (cnlNumList == null)
				{
					cnlNumList = new List<int>();
				}
				cnlNumList.Clear();
				if (!string.IsNullOrWhiteSpace(cnlNumStr))
				{
					foreach (string text in cnlNumStr.Split(new char[]
					{
						','
					}))
					{
						if (text.Contains('-'))
						{
							int[] array2 = (from x in text.Split(new char[]
							{
								'-'
							})
							select int.Parse(x.Trim())).ToArray<int>();
							IEnumerable<int> collection = Enumerable.Range(array2[0], array2[1] - array2[0] + 1);
							cnlNumList.AddRange(collection);
						}
						else
						{
							cnlNumList.Add(int.Parse(text.Trim()));
						}
					}
				}
			}
			catch (Exception ex)
			{
				this.log.WriteInfo("InitExportCnlNums Error" + ex.Message);
				cnlNumList = new List<int>();
			}
		}

		private bool InitDataSource()
		{
			bool result;
			try
			{
				this.DataSource.InitConnection();
				this.DataSource.InitCommands(this.ExportParams.ExportCurData ? this.ExportParams.ExportCurDataQuery : "", this.ExportParams.ExportArcData ? this.ExportParams.ExportArcDataQuery : "", this.ExportParams.ExportEvents ? this.ExportParams.ExportEventQuery : "");
				result = true;
			}
			catch (Exception ex)
			{
				this.log.WriteAction(string.Format("Error initializing data source {0}: {1}", this.DataSource.Name, ex.Message));
				result = false;
			}
			return result;
		}

		private void ResetStats()
		{
			this.fatalError = false;
			this.exportError = false;
			this.expCurSrezCnt = 0;
			this.expArcSrezCnt = 0;
			this.expEvCnt = 0;
			this.skipCurSrezCnt = 0;
			this.skipArcSrezCnt = 0;
			this.skipEvCnt = 0;
		}

		private void Execute()
		{
			try
			{



				while (!this.terminated)
				{
					try
					{
						if (this.Connect())
						{
							this.ExportCurData();
							this.ExportArcData();
							this.ExportEvents();
						}
					}
					catch (ThreadAbortException)
					{
						this.log.WriteAction(Localization.UseRussian ? "Экспорт прерван. Не все данные экспортированы" : "Export is aborted. Not all data is exported");
					}
					finally
					{
						this.Disconnect();
					}
					Thread.Sleep(100);
				}
			}
			finally
			{
				this.running = false;
			}
		}

		private bool Connect()
		{
			bool result;
			try
			{
				this.DataSource.Connect();
				result = true;
			}
			catch (Exception ex)
			{
				this.log.WriteAction(string.Format("Error connecting to DB {0}: {1}", this.DataSource.Name, ex.Message));
				this.exportError = true;
				Thread.Sleep(1000);
				result = false;
			}
			return result;
		}

		private void Disconnect()
		{
			try
			{
				this.DataSource.Disconnect();
			}
			catch (Exception ex)
			{
				this.log.WriteAction(string.Format(Localization.UseRussian ? "Ошибка при разъединении с БД {0}: {1}" : "Error disconnecting from DB {0}: {1}", this.DataSource.Name, ex.Message));
			}
		}

		private void ExportCurData()
		{
			if (this.ExportParams.ExportCurData)
			{
				SrezTableLight.Srez srez = null;
				try
				{
					for (int i = 0; i < 10; i++)
					{
						Queue<SrezTableLight.Srez> obj = this.curSrezQueue;
						lock (obj)
						{
							if (this.curSrezQueue.Count <= 0)
							{
								break;
							}
							srez = this.curSrezQueue.Dequeue();
						}
						this.ExportCurDataSrez(this.DataSource.ExportCurDataQuery, srez);
						this.expCurSrezCnt++;
						this.exportError = false;
					}
				}
				catch (Exception ex)
				{
					if (srez != null)
					{
						Queue<SrezTableLight.Srez> obj = this.curSrezQueue;
						lock (obj)
						{
							this.curSrezQueue.Enqueue(srez);
						}
					}
					this.log.WriteAction(string.Format(Localization.UseRussian ? "Ошибка при экспорте текущих данных в БД {0}: {1}" : "Error export current data to DB {0}: {1}", this.DataSource.Name, ex.Message));
					this.exportError = true;
					Thread.Sleep(1000);
				}
			}
		}

		private void ExportArcData()
		{
			if (this.ExportParams.ExportArcData)
			{
				SrezTableLight.Srez srez = null;
				try
				{
					for (int i = 0; i < 10; i++)
					{
						Queue<SrezTableLight.Srez> obj = this.arcSrezQueue;
						lock (obj)
						{
							if (this.arcSrezQueue.Count <= 0)
							{
								break;
							}
							srez = this.arcSrezQueue.Dequeue();
						}
						this.ExportArcDataSrez(this.DataSource.ExportArcDataQuery, srez);
						this.expArcSrezCnt++;
						this.exportError = false;
					}
				}
				catch (Exception ex)
				{
					if (srez != null)
					{
						Queue<SrezTableLight.Srez> obj = this.arcSrezQueue;
						lock (obj)
						{
							this.arcSrezQueue.Enqueue(srez);
						}
					}
					this.log.WriteAction(string.Format(Localization.UseRussian ? "Ошибка при экспорте архивных данных в БД {0}: {1}" : "Error export archive data to DB {0}: {1}", this.DataSource.Name, ex.Message));
					this.exportError = true;
					Thread.Sleep(1000);
				}
			}
		}

		private void ExportEvents()
		{
			if (this.ExportParams.ExportEvents)
			{
				EventTableLight.Event @event = null;
				try
				{
					for (int i = 0; i < 10; i++)
					{
						Queue<EventTableLight.Event> obj = this.evQueue;
						lock (obj)
						{
							if (this.evQueue.Count <= 0)
							{
								break;
							}
							@event = this.evQueue.Dequeue();
						}
						this.ExportEvent(this.DataSource.ExportEventQuery, @event);
						this.expEvCnt++;
						this.exportError = false;
					}
				}
				catch (Exception ex)
				{
					if (@event != null)
					{
						Queue<EventTableLight.Event> obj = this.evQueue;
						lock (obj)
						{
							this.evQueue.Enqueue(@event);
						}
					}
					this.log.WriteAction(string.Format(Localization.UseRussian ? "Ошибка при экспорте событий в БД {0}: {1}" : "Error export events to DB {0}: {1}", this.DataSource.Name, ex.Message));
					this.exportError = true;
					Thread.Sleep(1000);
				}
			}
		}

		private void ExportArcDataSrez(string query, SrezTableLight.Srez srez)
		{
			query = query.ToLower();
			foreach (int num in srez.CnlNums)
			{
				SrezTableLight.CnlData cnlData;
				if ((!this.expArcCnlList.Any<int>() || this.expArcCnlList.Contains(num)) && srez.GetCnlData(num, out cnlData))
				{
					query = query.Replace("#cnlnum", string.Format("{0}", num));
					query = this.DataSource.SetQueryParam(query, "dateTime", srez.DateTime);
					query = this.DataSource.SetQueryParam(query, "cnlNum", num);
					query = this.DataSource.SetQueryParam(query, "val", cnlData.Val);
					query = this.DataSource.SetQueryParam(query, "stat", cnlData.Stat);
					this.DataSource.ExecuteNonQuery(query);
				}
			}
		}

		private void ExportCurDataSrez(string query, SrezTableLight.Srez srez)
		{
			string queryTmp = query.ToLower();
			foreach (int num in srez.CnlNums)
			{
				SrezTableLight.CnlData cnlData;
				if ((!this.expCurCnlList.Any<int>() || this.expCurCnlList.Contains(num)) && srez.GetCnlData(num, out cnlData))
				{
					queryTmp = query.ToLower().Replace("#cnlnum", string.Format("{0}", num));
					queryTmp = this.DataSource.SetQueryParam(queryTmp, "dateTime", srez.DateTime);
					queryTmp = this.DataSource.SetQueryParam(queryTmp, "cnlNum", num);
					queryTmp = this.DataSource.SetQueryParam(queryTmp, "val", cnlData.Val);
					queryTmp = this.DataSource.SetQueryParam(queryTmp, "stat", cnlData.Stat);
					this.DataSource.ExecuteNonQuery(queryTmp);
					//this.log.WriteAction("cur sql  : " + queryTmp);
				}
			}
		}

		public static int dataFlg = 0;
		public static DateTime lastDate;
		public static int eventNum=0;
		public static bool firstFlg=true;
		private void ExportEvent(string query, EventTableLight.Event ev)
		{
			//Guocj
			DateTime lastDateTmp;
			string queryAll = "INSERT INTO ScadaData.events (DateTime,Number,CnlNum,ObjNum,KPNum,ParamID,OldCnlVal,OldCnlStat,NewCnlVal,NewCnlStat,Checked,UserID,Descr,Data)VALUES (@dateTime,@Number,@CnlNum, @objNum, @kpNum, @paramID,@oldCnlVal, @oldCnlStat, @newCnlVal, @newCnlStat, @checked, @userID, @descr, @data);";
			queryAll = queryAll.ToLower();

			query = query.ToLower();
			if (!this.expEventCnlList.Any<int>() || this.expEventCnlList.Contains(ev.CnlNum))
			{
				query = query.Replace("#cnlnum", string.Format("{0}", ev.CnlNum));
				query = this.DataSource.SetQueryParam(query, "dateTime", ev.DateTime);
				query = this.DataSource.SetQueryParam(query, "objNum", ev.ObjNum);
				query = this.DataSource.SetQueryParam(query, "kpNum", ev.KPNum);
				query = this.DataSource.SetQueryParam(query, "paramID", ev.ParamID);
				query = this.DataSource.SetQueryParam(query, "cnlNum", ev.CnlNum);
				query = this.DataSource.SetQueryParam(query, "oldCnlVal", ev.OldCnlVal);
				query = this.DataSource.SetQueryParam(query, "oldCnlStat", ev.OldCnlStat);
				query = this.DataSource.SetQueryParam(query, "newCnlVal", ev.NewCnlVal);
				query = this.DataSource.SetQueryParam(query, "newCnlStat", ev.NewCnlStat);
				query = this.DataSource.SetQueryParam(query, "checked", ev.Checked);
				query = this.DataSource.SetQueryParam(query, "userID", ev.UserID);
				query = this.DataSource.SetQueryParam(query, "descr", ev.Descr);
				query = this.DataSource.SetQueryParam(query, "data", ev.Data);
				//this.log.WriteAction("q1  "+ query);
				this.DataSource.ExecuteNonQuery(query);

				if (lastDate == ev.DateTime) 
				{
					dataFlg++;
					lastDateTmp = ev.DateTime.AddMilliseconds(dataFlg);
				}
				else
				{
					lastDateTmp = ev.DateTime;
					lastDate = ev.DateTime;
					dataFlg = 0;
				}

				queryAll = this.DataSource.SetQueryParam(queryAll, "dateTime", lastDateTmp);
				queryAll = this.DataSource.SetQueryParam(queryAll, "Number", eventNum++);
				queryAll = this.DataSource.SetQueryParam(queryAll, "CnlNum", ev.CnlNum);
				queryAll = this.DataSource.SetQueryParam(queryAll, "objNum", ev.ObjNum);
				queryAll = this.DataSource.SetQueryParam(queryAll, "kpNum", ev.KPNum);
				queryAll = this.DataSource.SetQueryParam(queryAll, "paramID", ev.ParamID);
				queryAll = this.DataSource.SetQueryParam(queryAll, "oldCnlVal", ev.OldCnlVal);
				queryAll = this.DataSource.SetQueryParam(queryAll, "oldCnlStat", ev.OldCnlStat);
				queryAll = this.DataSource.SetQueryParam(queryAll, "newCnlVal", ev.NewCnlVal);
				queryAll = this.DataSource.SetQueryParam(queryAll, "newCnlStat", ev.NewCnlStat);
				queryAll = this.DataSource.SetQueryParam(queryAll, "checked", ev.Checked);
				queryAll = this.DataSource.SetQueryParam(queryAll, "userID", ev.UserID);
				queryAll = this.DataSource.SetQueryParam(queryAll, "descr", ev.Descr);
				queryAll = this.DataSource.SetQueryParam(queryAll, "data", ev.Data);
				//this.log.WriteAction("q2  " + queryAll);
				this.DataSource.ExecuteNonQuery(queryAll);

			}

		}

		public void Start()
		{
			if (this.InitDataSource())
			{
				this.InitExportCnlNums();
				this.ResetStats();
				this.terminated = false;
				this.running = true;
				this.thread = new Thread(new ThreadStart(this.Execute));
				this.thread.Start();
				return;
			}
			this.fatalError = true;
		}

		public void Terminate()
		{
			this.terminated = true;
		}

		public void Abort()
		{
			if (this.thread != null)
			{
				this.thread.Abort();
			}
		}

		public void EnqueueCurData(SrezTableLight.Srez curSrez)
		{
			if (this.ExportParams.ExportCurData)
			{
				Queue<SrezTableLight.Srez> obj = this.curSrezQueue;
				lock (obj)
				{
					if (this.curSrezQueue.Count < this.maxQueueSize)
					{
						this.curSrezQueue.Enqueue(curSrez);
					}
					else
					{
						this.skipCurSrezCnt++;
						this.log.WriteAction(string.Format(Localization.UseRussian ? "Невозможно добавить в очередь текущие данные. Максимальный размер очереди {0} превышен" : "Unable to enqueue current data. The maximum size of the queue {0} is exceeded", this.maxQueueSize));
					}
				}
			}
		}

		public void EnqueueArcData(SrezTableLight.Srez arcSrez)
		{
			if (this.ExportParams.ExportArcData)
			{
				Queue<SrezTableLight.Srez> obj = this.arcSrezQueue;
				lock (obj)
				{
					if (this.arcSrezQueue.Count < this.maxQueueSize)
					{
						this.arcSrezQueue.Enqueue(arcSrez);
					}
					else
					{
						this.skipArcSrezCnt++;
						this.log.WriteAction(string.Format(Localization.UseRussian ? "Невозможно добавить в очередь архивные данные. Максимальный размер очереди {0} превышен" : "Unable to enqueue archive data. The maximum size of the queue {0} is exceeded", this.maxQueueSize));
					}
				}
			}
		}


		public void EnqueueEvent(EventTableLight.Event ev)
		{
			if (this.ExportParams.ExportEvents)
			{
				Queue<EventTableLight.Event> obj = this.evQueue;
				lock (obj)
				{
					if (this.evQueue.Count < this.maxQueueSize)
					{
						this.evQueue.Enqueue(ev);
					}
					else
					{
						this.skipEvCnt++;
						this.log.WriteAction(string.Format(Localization.UseRussian ? "Невозможно добавить в очередь событие. Максимальный размер очереди {0} превышен" : "Unable to enqueue an event. The maximum size of the queue {0} is exceeded", this.maxQueueSize));
					}
				}
			}
		}

		public string GetInfo()
		{
			StringBuilder stringBuilder = new StringBuilder(this.DataSource.Name);
			Queue<SrezTableLight.Srez> obj = this.curSrezQueue;
			int count;
			lock (obj)
			{
				count = this.curSrezQueue.Count;
			}
			obj = this.arcSrezQueue;
			int count2;
			lock (obj)
			{
				count2 = this.arcSrezQueue.Count;
			}
			Queue<EventTableLight.Event> obj2 = this.evQueue;
			int count3;
			lock (obj2)
			{
				count3 = this.evQueue.Count;
			}
			if (Localization.UseRussian)
			{
				string value;
				if (this.fatalError)
				{
					value = "фатальная ошибка";
				}
				else if (this.exportError)
				{
					value = "ошибка экспорта";
				}
				else
				{
					value = "норма";
				}
				stringBuilder.Append("; состояние: ").Append(value).Append("; в очереди тек/арх/соб: ").Append(count).Append("/").Append(count2).Append("/").Append(count3).Append("; экспортировано тек/арх/соб: ").Append(this.expCurSrezCnt).Append("/").Append(this.expArcSrezCnt).Append("/").Append(this.expEvCnt).Append("; пропущено тек/арх/соб: ").Append(this.skipCurSrezCnt).Append("/").Append(this.skipArcSrezCnt).Append("/").Append(this.skipEvCnt);
			}
			else
			{
				string value;
				if (this.fatalError)
				{
					value = "fatal error";
				}
				else if (this.exportError)
				{
					value = "export error";
				}
				else
				{
					value = "normal";
				}
				stringBuilder.Append("; state: ").Append(value).Append("; in queue cur/arc/ev: ").Append(count).Append("/").Append(count2).Append("/").Append(count3).Append("; exported cur/arc/ev: ").Append(this.expCurSrezCnt).Append("/").Append(this.expArcSrezCnt).Append("/").Append(this.expEvCnt).Append("; skipped cur/arc/ev: ").Append(this.skipCurSrezCnt).Append("/").Append(this.skipArcSrezCnt).Append("/").Append(this.skipEvCnt);
			}
			return stringBuilder.ToString();
		}

		private const int BundleSize = 10;

		private const int ErrorDelay = 1000;

		private readonly Log log;

		private readonly int maxQueueSize;

		private Queue<SrezTableLight.Srez> curSrezQueue;

		private Queue<SrezTableLight.Srez> arcSrezQueue;

		private Queue<EventTableLight.Event> evQueue;

		private Thread thread;

		private volatile bool terminated;

		private volatile bool running;

		private bool fatalError;

		private bool exportError;

		private int expCurSrezCnt;

		private int expArcSrezCnt;

		private int expEvCnt;

		private int skipCurSrezCnt;

		private int skipArcSrezCnt;

		private int skipEvCnt;

		private List<int> expCurCnlList = new List<int>();

		private List<int> expArcCnlList = new List<int>();

		private List<int> expEventCnlList = new List<int>();
	}
}
