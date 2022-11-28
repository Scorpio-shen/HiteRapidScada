using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml;
using Scada;
using Scada.Web;
using Scada.Web.Plugins;

// Token: 0x0200000D RID: 13
internal class i : ISettings
{
	// Token: 0x06000062 RID: 98 RVA: 0x000034CA File Offset: 0x000016CA
	public i()
	{
		this.a();
	}

	// Token: 0x06000063 RID: 99 RVA: 0x000034D8 File Offset: 0x000016D8
	[CompilerGenerated]
	public g b()
	{
		return this.b;
	}

	// Token: 0x06000064 RID: 100 RVA: 0x000034E0 File Offset: 0x000016E0
	[CompilerGenerated]
	private void a(g A_0)
	{
		this.b = A_0;
	}

	// Token: 0x06000065 RID: 101 RVA: 0x000034E9 File Offset: 0x000016E9
	[CompilerGenerated]
	public d c()
	{
		return this.c;
	}

	// Token: 0x06000066 RID: 102 RVA: 0x000034F1 File Offset: 0x000016F1
	[CompilerGenerated]
	private void a(d A_0)
	{
		this.c = A_0;
	}

	// Token: 0x06000067 RID: 103 RVA: 0x000034FA File Offset: 0x000016FA
	private void a()
	{
		this.a(new g());
		this.a(new d());
	}

	// Token: 0x06000068 RID: 104 RVA: 0x00003512 File Offset: 0x00001712
	public ISettings Create()
	{
		return new i();
	}

	// Token: 0x06000069 RID: 105 RVA: 0x00003519 File Offset: 0x00001719
	public bool Equals(ISettings settings)
	{
		return settings == this;
	}

	// Token: 0x0600006A RID: 106 RVA: 0x00003520 File Offset: 0x00001720
	public bool LoadFromFile(string fileName, out string errMsg)
	{
		this.a();
		bool result;
		try
		{
			if (!File.Exists(fileName))
			{
				throw new FileNotFoundException(string.Format(CommonPhrases.NamedFileNotFound, fileName));
			}
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(fileName);
			XmlElement documentElement = xmlDocument.DocumentElement;
			XmlNode xmlNode = documentElement.SelectSingleNode("DisplayOptions");
			if (xmlNode != null)
			{
				this.b().a(xmlNode);
			}
			XmlNode xmlNode2 = documentElement.SelectSingleNode("BehaviorOptions");
			if (xmlNode2 != null)
			{
				this.c().a(xmlNode2);
			}
			errMsg = "";
			result = true;
		}
		catch (Exception ex)
		{
			errMsg = string.Format(WebPhrases.LoadPluginConfigError, PlgChartProSpec.PlgName, ex.Message);
			result = false;
		}
		return result;
	}

	// Token: 0x0600006B RID: 107 RVA: 0x000035CC File Offset: 0x000017CC
	public bool SaveToFile(string fileName, out string errMsg)
	{
		throw new NotImplementedException();
	}

	// Token: 0x04000026 RID: 38
#pragma warning disable CS0102 // 类型“i”已经包含“a”的定义
	public const string a = "PlgChartPro.xml";
#pragma warning restore CS0102 // 类型“i”已经包含“a”的定义

	// Token: 0x04000027 RID: 39
	[CompilerGenerated]
#pragma warning disable CS0102 // 类型“i”已经包含“b”的定义
	private g b;
#pragma warning restore CS0102 // 类型“i”已经包含“b”的定义

	// Token: 0x04000028 RID: 40
	[CompilerGenerated]
#pragma warning disable CS0102 // 类型“i”已经包含“c”的定义
	private d c;
#pragma warning restore CS0102 // 类型“i”已经包含“c”的定义
}
