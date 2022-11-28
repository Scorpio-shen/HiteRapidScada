using Bit.Core.Web.Helpter;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TDengineDriver;

namespace Scada.Server.Modules.TDEngineExport
{
	internal class TDEngineDataSource : DataSource
	{
		public TDEngineDataSource()
		{
			base.DBType = DBType.TDEngine;
		}

		private void CheckConnection()
		{
			IntPtr connection = base.Connection;
			if (base.Connection == IntPtr.Zero)
			{
				throw new InvalidOperationException("Connection is not inited.");
			}
		}

		protected override void CreateConnection()
		{
			TDengine.Options(3, base.CfgFilePath);
			TDengine.Options(4, "60");
			TDengine.Init();
		}

		protected override void ClearPool()
		{
			this.CheckConnection();
			TDengine.Cleanup();
		}

		internal override void CreateTable(DbTableType dbTableType, List<int> cnlList)
		{
			this.Connect();
			this.CheckConnection();
			try
			{
				string text;
				if (dbTableType == DbTableType.EVentsAll)
				{
					text = "CREATE TABLE IF NOT EXISTS events (DateTime TIMESTAMP,Number int,CnlNum int,ObjNum int,KPNum int,ParamID int,OldCnlVal float,OldCnlStat int,NewCnlVal float,NewCnlStat int,Checked bool,UserID int,Descr NCHAR(100),Data NCHAR(50));";
					IntPtr intPtr = TDengine.Query(base.Connection, text.ToString());
					if (intPtr == IntPtr.Zero || TDengine.ErrorNo(intPtr) != 0)
					{
						throw new InvalidOperationException("CreateTable failure, " + text.ToString());
					}
					TDengine.FreeResult(intPtr);
				}
				else 
				{
					if (dbTableType == DbTableType.CnlData)
					{
						text = "CREATE TABLE IF NOT EXISTS Cnl@CnlNum (DateTime TIMESTAMP,CnlNum int,Val float,Stat int);";
					}
					else
					{
						if (dbTableType != DbTableType.Events)
						{
							return;
						}
						text = "CREATE TABLE IF NOT EXISTS EvCnl@CnlNum (DateTime TIMESTAMP, ObjNum int,KPNum int,ParamID int,OldCnlVal float,OldCnlStat int,NewCnlVal float,NewCnlStat int,Checked bool,UserID int,Descr NCHAR(100),Data NCHAR(50));";
					}
					for (int i = 0; i < cnlList.Count; i++)
					{
						string text2 = text.Replace("@CnlNum", cnlList[i].ToString());
						IntPtr intPtr = TDengine.Query(base.Connection, text2.ToString());
						if (intPtr == IntPtr.Zero || TDengine.ErrorNo(intPtr) != 0)
						{
							throw new InvalidOperationException("CreateTable failure, " + text2.ToString());
						}
						TDengine.FreeResult(intPtr);
					}
				}

			}
			finally
			{
				this.Disconnect();
			}
		}

		internal override void ExecuteNonQuery(string query)
		{
			this.CheckConnection();
			IntPtr intPtr = TDengine.Query(base.Connection, query);
			if (intPtr == IntPtr.Zero || TDengine.ErrorNo(intPtr) != 0)
			{
				string str = "ExecuteNonQuery failure, ";
				if (intPtr != IntPtr.Zero)
				{
					str += TDengine.Error(intPtr);
				}
				throw new InvalidOperationException(str + query);
			}
			TDengine.FreeResult(intPtr);
		}

		public override void Connect()
		{
			try
			{
				base.Connection = TDengine.Connect(base.Server, base.User, base.Password, base.Database, 0);
				if (base.Connection == IntPtr.Zero)
				{
					throw new InvalidOperationException("Connect to TDengine failed;");
				}
				this.UseDatabase(base.Database);
			}
			catch
			{
				this.ClearPool();
				throw;
			}
		}

		public override void Disconnect()
		{
			IntPtr connection = base.Connection;
			if (base.Connection != IntPtr.Zero)
			{
				TDengine.Close(base.Connection);
			}
		}

		private void UseDatabase(string database)
		{
			string text = "use " + database + ";";
			IntPtr intPtr = TDengine.Query(base.Connection, text);
			if (intPtr == IntPtr.Zero || TDengine.ErrorNo(intPtr) != 0)
			{
				throw new InvalidOperationException("use db failure," + text);
			}
			TDengine.FreeResult(intPtr);
		}

		public override int getCountTable(string tableName)
		{
			string countCmd = "select count(*) from scadadata.events ";
			int countNum = 0;

			this.Connect();
			this.CheckConnection();

            try
            {
				IntPtr intPtr = TDengine.Query(base.Connection, countCmd);
				if (intPtr == IntPtr.Zero || TDengine.ErrorNo(intPtr) != 0)
				{
					countNum = 0;
					throw new InvalidOperationException("use db failure," + countCmd);

				}
				LogHelpterAnDa.AddLog("1");
				IntPtr rowdata;
				List<TDengineMeta> metas = TDengine.FetchFields(intPtr);
				if ((rowdata = TDengine.FetchRows(intPtr)) != IntPtr.Zero)
				{
					TDengineMeta meta = metas[0];

					IntPtr data = Marshal.ReadIntPtr(rowdata, 0);

					if (data == IntPtr.Zero)
					{

						return 0;
					}

					countNum = Marshal.ReadInt32(data);
					LogHelpterAnDa.AddLog("2 " + countNum.ToString());

				}


				TDengine.FreeResult(intPtr);
			}
			finally
			{
				this.Disconnect();
			}

			return countNum;
		}

	}
}
