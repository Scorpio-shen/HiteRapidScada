using System;
using System.Collections.Generic;

namespace Scada.Server.Modules.TDEngineExport
{
	internal abstract class DataSource : IComparable<DataSource>
	{
		public DataSource()
		{
			this.DBType = DBType.Undefined;
			this.Server = "";
			this.Database = "";
			this.User = "";
			this.Password = "";
			this.CfgFilePath = "";
			this.Connection = IntPtr.Zero;
			this.ExportCurDataQuery = null;
			this.ExportArcDataQuery = null;
			this.ExportEventQuery = null;
		}

		public DBType DBType { get; set; }

		public string Server { get; set; }

		public string Database { get; set; }

		public string User { get; set; }

		public string Password { get; set; }

		public string CfgFilePath { get; set; }

		public string Name
		{
			get
			{
				return this.DBType.ToString() + (string.IsNullOrEmpty(this.Server) ? "" : (" - " + this.Server));
			}
		}

		public IntPtr Connection { get; protected set; }

		public string ExportCurDataQuery { get; protected set; }

		public string ExportArcDataQuery { get; protected set; }

		public string ExportEventQuery { get; protected set; }

		protected abstract void CreateConnection();

		protected abstract void ClearPool();

		internal virtual void CreateTable(DbTableType dbTableType, List<int> cnlList)
		{
		}

		public virtual int getCountTable(string tableName)
		{
			return 0;
		}

			protected void ExtractHostAndPort(string server, int defaultPort, out string host, out int port)
		{
			int num = server.IndexOf(':');
			if (num >= 0)
			{
				host = server.Substring(0, num);
				try
				{
					port = int.Parse(server.Substring(num + 1));
					return;
				}
				catch
				{
					port = defaultPort;
					return;
				}
			}
			host = server;
			port = defaultPort;
		}

		public void InitConnection()
		{
			this.CreateConnection();
		}

		public abstract void Connect();

		public abstract void Disconnect();

		public void InitCommands(string exportCurDataQuery, string exportArcDataQuery, string exportEventQuery)
		{
			this.ExportCurDataQuery = (string.IsNullOrEmpty(exportCurDataQuery) ? null : exportCurDataQuery);
			this.ExportArcDataQuery = (string.IsNullOrEmpty(exportArcDataQuery) ? null : exportArcDataQuery);
			this.ExportEventQuery = (string.IsNullOrEmpty(exportEventQuery) ? null : exportEventQuery);
		}

		public string SetQueryParam(string query, string paramName, object value)
		{
			if (query == null)
			{
				throw new ArgumentNullException("query");
			}
			paramName = ("@" + paramName).ToLower();
			if (query.IndexOf(paramName) > -1)
			{
				if (value is DateTime)
				{
					query = query.Replace(paramName, "'" + ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss.fff") + "'");
				}
				else if (value is bool)
				{
					query = query.Replace(paramName, "'" + ((bool)value).ToString().ToLower() + "'");
				}
				else
				{
					query = query.Replace(paramName, string.Format("'{0}'", value));
				}
			}
			return query;
		}

		internal abstract void ExecuteNonQuery(string query);

		public virtual DataSource Clone()
		{
			DataSource dataSource = (DataSource)Activator.CreateInstance(base.GetType());
			dataSource.DBType = this.DBType;
			dataSource.Server = this.Server;
			dataSource.Database = this.Database;
			dataSource.User = this.User;
			dataSource.Password = this.Password;
			dataSource.CfgFilePath = this.CfgFilePath;
			return dataSource;
		}

		public override string ToString()
		{
			return this.Name;
		}

		public int CompareTo(DataSource other)
		{
			int num = this.DBType.CompareTo(other.DBType);
			if (num != 0)
			{
				return num;
			}
			return this.Name.CompareTo(other.Name);
		}

		protected const string HiddenPassword = "●●●●●";
	}
}
