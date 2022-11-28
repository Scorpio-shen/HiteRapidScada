using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Scada.Server.Modules.TDEngineExport
{

	internal class ModConfig
	{

		private ModConfig()
		{
		}

		public ModConfig(string configDir)
		{
			this.FileName = ScadaUtils.NormalDir(configDir) + "ModTDEngineExport.xml";
			this.SetToDefault();
		}


		public string FileName { get; private set; }

		public List<ModConfig.ExportDestination> ExportDestinations { get; private set; }


		public int CurDataCtrlCnlNum { get; set; }

		public int ArcDataCtrlCnlNum { get; set; }

		public int EventsCtrlCnlNum { get; set; }

		private void SetToDefault()
		{
			if (this.ExportDestinations == null)
			{
				this.ExportDestinations = new List<ModConfig.ExportDestination>();
			}
			else
			{
				this.ExportDestinations.Clear();
			}
			this.CurDataCtrlCnlNum = 1;
			this.ArcDataCtrlCnlNum = 2;
			this.EventsCtrlCnlNum = 3;
		}

		public bool Load(out string errMsg)
		{
			this.SetToDefault();
			bool result;
			try
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.Load(this.FileName);
				XmlNode xmlNode = xmlDocument.DocumentElement.SelectSingleNode("ExportDestinations");
				if (xmlNode != null)
				{
					foreach (object obj in xmlNode.SelectNodes("ExportDestination"))
					{
						XmlElement xmlElement = (XmlElement)obj;
						DataSource dataSource = null;
						XmlNode xmlNode2 = xmlElement.SelectSingleNode("DataSource");
						if (xmlNode2 != null)
						{
							DBType dbtype;
							if (!Enum.TryParse<DBType>(ScadaUtils.GetChildAsString(xmlNode2, "DBType", ""), out dbtype))
							{
								dbtype = DBType.Undefined;
							}
							if (dbtype == DBType.TDEngine)
							{
								dataSource = new TDEngineDataSource();
							}
							else
							{
								dataSource = null;
							}
							if (dataSource != null)
							{
								dataSource.Server = ScadaUtils.GetChildAsString(xmlNode2, "Server", "");
								dataSource.Database = ScadaUtils.GetChildAsString(xmlNode2, "Database", "");
								dataSource.User = ScadaUtils.GetChildAsString(xmlNode2, "User", "");
								dataSource.Password = ScadaUtils.Decrypt(ScadaUtils.GetChildAsString(xmlNode2, "Password", ""));
								dataSource.CfgFilePath = ScadaUtils.GetChildAsString(xmlNode2, "CfgFilePath", "");
							}
						}
						if (dataSource != null)
						{
							XmlNode xmlNode3 = xmlElement.SelectSingleNode("ExportParams");
							if (xmlNode3 != null)
							{
								ModConfig.ExportParams exportParams = new ModConfig.ExportParams();
								exportParams.LoadFromXml(xmlNode3);
								ModConfig.ExportDestination item = new ModConfig.ExportDestination(dataSource, exportParams);
								this.ExportDestinations.Add(item);
							}
						}
					}
					this.ExportDestinations.Sort();
				}
				XmlNode xmlNode4 = xmlDocument.DocumentElement.SelectSingleNode("ManualExport");
				if (xmlNode4 != null)
				{
					this.CurDataCtrlCnlNum = ScadaUtils.GetChildAsInt(xmlNode4, "CurDataCtrlCnlNum", 0);
					this.ArcDataCtrlCnlNum = ScadaUtils.GetChildAsInt(xmlNode4, "ArcDataCtrlCnlNum", 0);
					this.EventsCtrlCnlNum = ScadaUtils.GetChildAsInt(xmlNode4, "EventsCtrlCnlNum", 0);
				}
				errMsg = "";
				result = true;
			}
			catch (FileNotFoundException ex)
			{
				errMsg = string.Concat(new string[]
				{
					ModPhrases.LoadModSettingsError,
					": ",
					ex.Message,
					Environment.NewLine,
					ModPhrases.ConfigureModule
				});
				result = false;
			}
			catch (Exception ex2)
			{
				errMsg = ModPhrases.LoadModSettingsError + ": " + ex2.Message;
				result = false;
			}
			return result;
		}

		public bool Save(out string errMsg)
		{
			bool result;
			try
			{
				XmlDocument xmlDocument = new XmlDocument();
				XmlDeclaration newChild = xmlDocument.CreateXmlDeclaration("1.0", "utf-8", null);
				xmlDocument.AppendChild(newChild);
				XmlElement xmlElement = xmlDocument.CreateElement("ModTDEngineExport");
				xmlDocument.AppendChild(xmlElement);
				XmlElement xmlElement2 = ScadaUtils.AppendElem(xmlElement, "ExportDestinations", null);
				foreach (ModConfig.ExportDestination exportDestination in this.ExportDestinations)
				{
					XmlElement xmlElement3 = ScadaUtils.AppendElem(xmlElement2, "ExportDestination", null);
					DataSource dataSource = exportDestination.DataSource;
					XmlElement xmlElement4 = ScadaUtils.AppendElem(xmlElement3, "DataSource", null);
					ScadaUtils.AppendElem(xmlElement4, "DBType", dataSource.DBType);
					ScadaUtils.AppendElem(xmlElement4, "Server", dataSource.Server);
					ScadaUtils.AppendElem(xmlElement4, "Database", dataSource.Database);
					ScadaUtils.AppendElem(xmlElement4, "User", dataSource.User);
					ScadaUtils.AppendElem(xmlElement4, "Password", ScadaUtils.Encrypt(dataSource.Password));
					ScadaUtils.AppendElem(xmlElement4, "CfgFilePath", dataSource.CfgFilePath);
					exportDestination.ExportParams.SaveToXml(ScadaUtils.AppendElem(xmlElement3, "ExportParams", null));
				}
				XmlElement xmlElement5 = ScadaUtils.AppendElem(xmlElement, "ManualExport", null);
				ScadaUtils.AppendElem(xmlElement5, "CurDataCtrlCnlNum", this.CurDataCtrlCnlNum);
				ScadaUtils.AppendElem(xmlElement5, "ArcDataCtrlCnlNum", this.ArcDataCtrlCnlNum);
				ScadaUtils.AppendElem(xmlElement5, "EventsCtrlCnlNum", this.EventsCtrlCnlNum);
				xmlDocument.Save(this.FileName);
				errMsg = "";
				result = true;
			}
			catch (Exception ex)
			{
				errMsg = ModPhrases.SaveModSettingsError + ": " + ex.Message;
				result = false;
			}
			return result;
		}

		public ModConfig Clone()
		{
			ModConfig modConfig = new ModConfig
			{
				FileName = this.FileName,
				ExportDestinations = new List<ModConfig.ExportDestination>()
			};
			foreach (ModConfig.ExportDestination exportDestination in this.ExportDestinations)
			{
				modConfig.ExportDestinations.Add(exportDestination.Clone());
			}
			modConfig.CurDataCtrlCnlNum = this.CurDataCtrlCnlNum;
			modConfig.ArcDataCtrlCnlNum = this.ArcDataCtrlCnlNum;
			modConfig.EventsCtrlCnlNum = this.EventsCtrlCnlNum;
			return modConfig;
		}

		private const string ConfigFileName = "ModTDEngineExport.xml";

		public class ExportParams
		{

			public ExportParams()
			{
				this.ExportCurData = false;
				this.ExportCurDataQuery = "";
				this.ExportEventCnls = "";
				this.ExportArcData = false;
				this.ExportArcDataQuery = "";
				this.ExportArcDataCnls = "";
				this.ExportEvents = false;
				this.ExportEventQuery = "";
				this.ExportEventCnls = "";
				this.MaxQueueSize = 100;
			}


			public bool ExportCurData { get; set; }


			public string ExportCurDataQuery { get; set; }


			public string ExportCurDataCnls { get; set; }

			public bool ExportArcData { get; set; }

			public string ExportArcDataQuery { get; set; }


			public string ExportArcDataCnls { get; set; }

			public bool ExportEvents { get; set; }


			public string ExportEventQuery { get; set; }


			public string ExportEventCnls { get; set; }

			public int MaxQueueSize { get; set; }

			public void LoadFromXml(XmlNode xmlNode)
			{
				if (xmlNode == null)
				{
					throw new ArgumentNullException("xmlNode");
				}
				this.ExportCurDataQuery = ScadaUtils.GetChildAsString(xmlNode, "ExportCurDataQuery", "");
				this.ExportCurData = (!string.IsNullOrEmpty(this.ExportCurDataQuery) && ScadaUtils.GetChildAsBool(xmlNode, "ExportCurData", false));
				this.ExportCurDataCnls = ScadaUtils.GetChildAsString(xmlNode, "ExportCurDataCnls", "");
				this.ExportArcDataQuery = ScadaUtils.GetChildAsString(xmlNode, "ExportArcDataQuery", "");
				this.ExportArcData = (!string.IsNullOrEmpty(this.ExportArcDataQuery) && ScadaUtils.GetChildAsBool(xmlNode, "ExportArcData", false));
				this.ExportArcDataCnls = ScadaUtils.GetChildAsString(xmlNode, "ExportArcDataCnls", "");
				this.ExportEventQuery = ScadaUtils.GetChildAsString(xmlNode, "ExportEventQuery", "");
				this.ExportEvents = (!string.IsNullOrEmpty(this.ExportEventQuery) && ScadaUtils.GetChildAsBool(xmlNode, "ExportEvents", false));
				this.ExportEventCnls = ScadaUtils.GetChildAsString(xmlNode, "ExportEventCnls", "");
				this.MaxQueueSize = ScadaUtils.GetChildAsInt(xmlNode, "MaxQueueSize", this.MaxQueueSize);
			}

			public void SaveToXml(XmlElement xmlElem)
			{
				if (xmlElem == null)
				{
					throw new ArgumentNullException("xmlElem");
				}
				ScadaUtils.AppendElem(xmlElem, "ExportCurData", this.ExportCurData);
				ScadaUtils.AppendElem(xmlElem, "ExportCurDataQuery", this.ExportCurDataQuery);
				ScadaUtils.AppendElem(xmlElem, "ExportCurDataCnls", this.ExportCurDataCnls);
				ScadaUtils.AppendElem(xmlElem, "ExportArcData", this.ExportArcData);
				ScadaUtils.AppendElem(xmlElem, "ExportArcDataQuery", this.ExportArcDataQuery);
				ScadaUtils.AppendElem(xmlElem, "ExportArcDataCnls", this.ExportArcDataCnls);
				ScadaUtils.AppendElem(xmlElem, "ExportEvents", this.ExportEvents);
				ScadaUtils.AppendElem(xmlElem, "ExportEventQuery", this.ExportEventQuery);
				ScadaUtils.AppendElem(xmlElem, "ExportEventCnls", this.ExportEventCnls);
				ScadaUtils.AppendElem(xmlElem, "MaxQueueSize", this.MaxQueueSize);
			}

			public ModConfig.ExportParams Clone()
			{
				return new ModConfig.ExportParams
				{
					ExportCurData = this.ExportCurData,
					ExportCurDataQuery = this.ExportCurDataQuery,
					ExportCurDataCnls = this.ExportCurDataCnls,
					ExportArcData = this.ExportArcData,
					ExportArcDataQuery = this.ExportArcDataQuery,
					ExportArcDataCnls = this.ExportArcDataCnls,
					ExportEvents = this.ExportEvents,
					ExportEventQuery = this.ExportEventQuery,
					ExportEventCnls = this.ExportEventCnls,
					MaxQueueSize = this.MaxQueueSize
				};
			}
		}

		public class ExportDestination : IComparable<ModConfig.ExportDestination>
		{

			private ExportDestination()
			{
			}

			public ExportDestination(DataSource dataSource, ModConfig.ExportParams exportParams)
			{
				if (dataSource == null)
				{
					throw new ArgumentNullException("dataSource");
				}
				if (exportParams == null)
				{
					throw new ArgumentNullException("exportParams");
				}
				this.DataSource = dataSource;
				this.ExportParams = exportParams;
			}

			public DataSource DataSource { get; private set; }

			public ModConfig.ExportParams ExportParams { get; private set; }

			public ModConfig.ExportDestination Clone()
			{
				return new ModConfig.ExportDestination(this.DataSource.Clone(), this.ExportParams.Clone());
			}

			public int CompareTo(ModConfig.ExportDestination other)
			{
				return this.DataSource.CompareTo(other.DataSource);
			}
		}
	}
}
