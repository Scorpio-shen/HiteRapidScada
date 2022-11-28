using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;

// Token: 0x0200000B RID: 11
internal class g
{
	// Token: 0x06000036 RID: 54 RVA: 0x000027CC File Offset: 0x000009CC
	public g()
	{
		this.a("en-GB");
		this.a(90);
		this.b(true);
		this.a(new e());
		this.a(new f());
		this.a(new j());
		this.a(new l());
		this.a(new List<k>());
		this.a(new h());
	}

	// Token: 0x06000037 RID: 55 RVA: 0x0000283B File Offset: 0x00000A3B
	[CompilerGenerated]
	public string a()
	{
		return this.a;
	}

	// Token: 0x06000038 RID: 56 RVA: 0x00002843 File Offset: 0x00000A43
	[CompilerGenerated]
	public void a(string A_0)
	{
		this.a = A_0;
	}

	// Token: 0x06000039 RID: 57 RVA: 0x0000284C File Offset: 0x00000A4C
	[CompilerGenerated]
	public int b()
	{
		return this.b;
	}

	// Token: 0x0600003A RID: 58 RVA: 0x00002854 File Offset: 0x00000A54
	[CompilerGenerated]
	public void a(int A_0)
	{
		this.b = A_0;
	}

	// Token: 0x0600003B RID: 59 RVA: 0x0000285D File Offset: 0x00000A5D
	[CompilerGenerated]
	public bool c()
	{
		return this.c;
	}

	// Token: 0x0600003C RID: 60 RVA: 0x00002865 File Offset: 0x00000A65
	[CompilerGenerated]
	public void b(bool A_0)
	{
		this.c = A_0;
	}

	// Token: 0x0600003D RID: 61 RVA: 0x0000286E File Offset: 0x00000A6E
	[CompilerGenerated]
	public e d()
	{
		return this.d;
	}

	// Token: 0x0600003E RID: 62 RVA: 0x00002876 File Offset: 0x00000A76
	[CompilerGenerated]
	private void a(e A_0)
	{
		this.d = A_0;
	}

	// Token: 0x0600003F RID: 63 RVA: 0x0000287F File Offset: 0x00000A7F
	[CompilerGenerated]
	public f e()
	{
		return this.e;
	}

	// Token: 0x06000040 RID: 64 RVA: 0x00002887 File Offset: 0x00000A87
	[CompilerGenerated]
	private void a(f A_0)
	{
		this.e = A_0;
	}

	// Token: 0x06000041 RID: 65 RVA: 0x00002890 File Offset: 0x00000A90
	[CompilerGenerated]
	public j f()
	{
		return this.f;
	}

	// Token: 0x06000042 RID: 66 RVA: 0x00002898 File Offset: 0x00000A98
	[CompilerGenerated]
	private void a(j A_0)
	{
		this.f = A_0;
	}

	// Token: 0x06000043 RID: 67 RVA: 0x000028A1 File Offset: 0x00000AA1
	[CompilerGenerated]
#pragma warning disable CS0542 // “g”: 成员名不能与它们的封闭类型相同
	public l g()
#pragma warning restore CS0542 // “g”: 成员名不能与它们的封闭类型相同
	{
		return this.g;
	}

	// Token: 0x06000044 RID: 68 RVA: 0x000028A9 File Offset: 0x00000AA9
	[CompilerGenerated]
	private void a(l A_0)
	{
		this.g = A_0;
	}

	// Token: 0x06000045 RID: 69 RVA: 0x000028B2 File Offset: 0x00000AB2
	[CompilerGenerated]
	public List<k> h()
	{
		return this.h;
	}

	// Token: 0x06000046 RID: 70 RVA: 0x000028BA File Offset: 0x00000ABA
	[CompilerGenerated]
	private void a(List<k> A_0)
	{
		this.h = A_0;
	}

	// Token: 0x06000047 RID: 71 RVA: 0x000028C3 File Offset: 0x00000AC3
	[CompilerGenerated]
	public h i()
	{
		return this.i;
	}

	// Token: 0x06000048 RID: 72 RVA: 0x000028CB File Offset: 0x00000ACB
	[CompilerGenerated]
	private void a(h A_0)
	{
		this.i = A_0;
	}

	// Token: 0x06000049 RID: 73 RVA: 0x000028D4 File Offset: 0x00000AD4
	private string a(ICollection<int> A_0)
	{
		if (A_0 != null && A_0.Count != 0)
		{
			return "[" + string.Join<int>(", ", A_0) + "]";
		}
		return "[]";
	}

	// Token: 0x0600004A RID: 74 RVA: 0x00002901 File Offset: 0x00000B01
	private string a(ICollection<string> A_0)
	{
		if (A_0 != null && A_0.Count != 0)
		{
			return "['" + string.Join("', '", A_0) + "']";
		}
		return "[]";
	}

	// Token: 0x0600004B RID: 75 RVA: 0x0000292E File Offset: 0x00000B2E
	private string a(bool A_0)
	{
		if (!A_0)
		{
			return "false";
		}
		return "true";
	}

	// Token: 0x0600004C RID: 76 RVA: 0x00002940 File Offset: 0x00000B40
	public void a(XmlNode A_0)
	{
		if (A_0 == null)
		{
			throw new ArgumentNullException("xmlNode");
		}
		XmlNode xmlNode = A_0.SelectSingleNode("ChartArea");
		if (xmlNode != null)
		{
			this.d().a(xmlNode);
		}
		XmlNode xmlNode2 = A_0.SelectSingleNode("ChartTitle");
		if (xmlNode2 != null)
		{
			this.e().a(xmlNode2);
		}
		XmlNode xmlNode3 = A_0.SelectSingleNode("PlotArea");
		if (xmlNode3 != null)
		{
			this.f().a(xmlNode3);
		}
		XmlNode xmlNode4 = A_0.SelectSingleNode("XAxis");
		if (xmlNode4 != null)
		{
			this.g().a(xmlNode4);
		}
		XmlNode xmlNode5 = A_0.SelectSingleNode("YAxes");
		if (xmlNode5 != null)
		{
			foreach (object obj in xmlNode5.SelectNodes("YAxis"))
			{
				XmlNode a_ = (XmlNode)obj;
				k k = new k();
				k.a(a_);
				this.h().Add(k);
			}
		}
		XmlNode xmlNode6 = A_0.SelectSingleNode("Legend");
		if (xmlNode6 != null)
		{
			this.i().a(xmlNode6);
		}
	}

	// Token: 0x0600004D RID: 77 RVA: 0x00002A68 File Offset: 0x00000C68
	public void a(StringBuilder A_0)
	{
		A_0.AppendLine("var displayOptions = new scada.chart.DisplayOptions();").Append("displayOptions.locale = '").Append(this.a()).AppendLine("';").Append("displayOptions.gapBetweenPoints = ").Append(this.b()).AppendLine(";").Append("displayOptions.alignXToGrid = ").Append(this.a(this.c())).AppendLine(";").AppendLine("displayOptions.chartArea = {").Append("    chartPadding: ").Append(this.a(this.d().a())).AppendLine(",").Append("    fontName: '").Append(this.d().b()).AppendLine("',").Append("    backColor: '").Append(this.d().c()).AppendLine("'").AppendLine("};").AppendLine("displayOptions.titleConfig = {").Append("    showTitle: ").Append(this.a(this.e().a())).AppendLine(",").Append("    showMenu: ").Append(this.a(this.e().b())).AppendLine(",").Append("    showStatus: ").Append(this.a(this.e().c())).AppendLine(",").Append("    height: ").Append(this.e().d()).AppendLine(",").Append("    fontSize: ").Append(this.e().e()).AppendLine(",").Append("    foreColor: '").Append(this.e().f()).AppendLine("'").AppendLine("};").AppendLine("displayOptions.plotArea = {").Append("    frameColor: '").Append(this.f().a()).AppendLine("',").Append("    gridColor: '").Append(this.f().b()).AppendLine("',").Append("    backColor: '").Append(this.f().c()).AppendLine("',").Append("    markerColor: '").Append(this.f().d()).AppendLine("',").Append("    selectionColor: '").Append(this.f().e()).AppendLine("',").Append("    lineWidth: ").Append(this.f().f()).AppendLine(",").Append("    trendColors: ").Append(this.a(this.f().g())).AppendLine().AppendLine("};").AppendLine("displayOptions.xAxis = {").Append("    height: ").Append(this.g().a()).AppendLine(",").Append("    showGridLines: ").Append(this.a(this.g().b())).AppendLine(",").Append("    showDates: ").Append(this.a(this.g().c())).AppendLine(",").Append("    majorTickSize: ").Append(this.g().d()).AppendLine(",").Append("    minorTickSize: ").Append(this.g().e()).AppendLine(",").Append("    showMinorTicks: ").Append(this.a(this.g().f())).AppendLine(",").Append("    labelMargin: ").Append(this.a(this.g().g())).AppendLine(",").Append("    fontSize: ").Append(this.g().h()).AppendLine(",").Append("    lineColor: '").Append(this.g().i()).AppendLine("',").Append("    textColor: '").Append(this.g().j()).AppendLine("'").AppendLine("};").AppendLine("displayOptions.yAxes = [");
		foreach (k k in this.h())
		{
			A_0.AppendLine("{").Append("    position: ").Append((int)k.a()).AppendLine(",").Append("    autoWidth: ").Append(this.a(k.b())).AppendLine(",").Append("    width: ").Append(k.c()).AppendLine(",").Append("    showTitle: ").Append(this.a(k.d())).AppendLine(",").Append("    showGridLines: ").Append(this.a(k.e())).AppendLine(",").Append("    majorTickSize: ").Append(k.f()).AppendLine(",").Append("    minorTickSize: ").Append(k.g()).AppendLine(",").Append("    minorTickCount: ").Append(k.h()).AppendLine(",").Append("    labelMargin: ").Append(this.a(k.i())).AppendLine(",").Append("    fontSize: ").Append(k.j()).AppendLine(",").Append("    lineColor: '").Append(k.k()).AppendLine("',").Append("    textColor: '").Append(k.l()).AppendLine("',").Append("    trendColor: '").Append(k.m()).AppendLine("',").Append("    includeZero: ").Append((int)k.n()).AppendLine(",").Append("    quantityIDs: ").Append(this.a(k.o())).AppendLine("}, ");
		}
		A_0.AppendLine("];").AppendLine("displayOptions.legend = {").Append("    position: ").Append((int)this.i().a()).AppendLine(",").Append("    columnWidth: ").Append(this.i().b()).AppendLine(",").Append("    columnMargin: ").Append(this.a(this.i().c())).AppendLine(",").Append("    columnCount: ").Append(this.i().d()).AppendLine(",").Append("    lineHeight: ").Append(this.i().e()).AppendLine(",").Append("    iconWidth: ").Append(this.i().f()).AppendLine(",").Append("    iconHeight: ").Append(this.i().g()).AppendLine(",").Append("    fontSize: ").Append(this.i().h()).AppendLine(",").Append("    foreColor: '").Append(this.i().i()).AppendLine("'").AppendLine("};").AppendLine();
	}

	// Token: 0x04000014 RID: 20
	[CompilerGenerated]
#pragma warning disable CS0102 // 类型“g”已经包含“a”的定义
	private string a;
#pragma warning restore CS0102 // 类型“g”已经包含“a”的定义

	// Token: 0x04000015 RID: 21
	[CompilerGenerated]
#pragma warning disable CS0102 // 类型“g”已经包含“b”的定义
	private int b;
#pragma warning restore CS0102 // 类型“g”已经包含“b”的定义

	// Token: 0x04000016 RID: 22
	[CompilerGenerated]
#pragma warning disable CS0102 // 类型“g”已经包含“c”的定义
	private bool c;
#pragma warning restore CS0102 // 类型“g”已经包含“c”的定义

	// Token: 0x04000017 RID: 23
	[CompilerGenerated]
#pragma warning disable CS0102 // 类型“g”已经包含“d”的定义
	private e d;
#pragma warning restore CS0102 // 类型“g”已经包含“d”的定义

	// Token: 0x04000018 RID: 24
	[CompilerGenerated]
#pragma warning disable CS0102 // 类型“g”已经包含“e”的定义
	private f e;
#pragma warning restore CS0102 // 类型“g”已经包含“e”的定义

	// Token: 0x04000019 RID: 25
	[CompilerGenerated]
#pragma warning disable CS0102 // 类型“g”已经包含“f”的定义
	private j f;
#pragma warning restore CS0102 // 类型“g”已经包含“f”的定义

	// Token: 0x0400001A RID: 26
	[CompilerGenerated]
#pragma warning disable CS0102 // 类型“g”已经包含“g”的定义
#pragma warning disable CS0542 // “g”: 成员名不能与它们的封闭类型相同
	private l g;
#pragma warning restore CS0542 // “g”: 成员名不能与它们的封闭类型相同
#pragma warning restore CS0102 // 类型“g”已经包含“g”的定义

	// Token: 0x0400001B RID: 27
	[CompilerGenerated]
#pragma warning disable CS0102 // 类型“g”已经包含“h”的定义
	private List<k> h;
#pragma warning restore CS0102 // 类型“g”已经包含“h”的定义

	// Token: 0x0400001C RID: 28
	[CompilerGenerated]
#pragma warning disable CS0102 // 类型“g”已经包含“i”的定义
	private h i;
#pragma warning restore CS0102 // 类型“g”已经包含“i”的定义
}
