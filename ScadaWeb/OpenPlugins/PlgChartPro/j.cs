using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml;
using Scada;

// Token: 0x0200000E RID: 14
internal class j
{
	// Token: 0x0600006C RID: 108 RVA: 0x000035D4 File Offset: 0x000017D4
	public j()
	{
		this.a("#808080");
		this.b("#e0e0e0");
		this.c("#ffffff");
		this.d("#000000");
		this.e("#6aaaea");
		this.a(1);
		this.a(new List<string>());
	}

	// Token: 0x0600006D RID: 109 RVA: 0x00003630 File Offset: 0x00001830
	[CompilerGenerated]
	public string a()
	{
		return this.a;
	}

	// Token: 0x0600006E RID: 110 RVA: 0x00003638 File Offset: 0x00001838
	[CompilerGenerated]
	public void a(string A_0)
	{
		this.a = A_0;
	}

	// Token: 0x0600006F RID: 111 RVA: 0x00003641 File Offset: 0x00001841
	[CompilerGenerated]
	public string b()
	{
		return this.b;
	}

	// Token: 0x06000070 RID: 112 RVA: 0x00003649 File Offset: 0x00001849
	[CompilerGenerated]
	public void b(string A_0)
	{
		this.b = A_0;
	}

	// Token: 0x06000071 RID: 113 RVA: 0x00003652 File Offset: 0x00001852
	[CompilerGenerated]
	public string c()
	{
		return this.c;
	}

	// Token: 0x06000072 RID: 114 RVA: 0x0000365A File Offset: 0x0000185A
	[CompilerGenerated]
	public void c(string A_0)
	{
		this.c = A_0;
	}

	// Token: 0x06000073 RID: 115 RVA: 0x00003663 File Offset: 0x00001863
	[CompilerGenerated]
	public string d()
	{
		return this.d;
	}

	// Token: 0x06000074 RID: 116 RVA: 0x0000366B File Offset: 0x0000186B
	[CompilerGenerated]
	public void d(string A_0)
	{
		this.d = A_0;
	}

	// Token: 0x06000075 RID: 117 RVA: 0x00003674 File Offset: 0x00001874
	[CompilerGenerated]
	public string e()
	{
		return this.e;
	}

	// Token: 0x06000076 RID: 118 RVA: 0x0000367C File Offset: 0x0000187C
	[CompilerGenerated]
	public void e(string A_0)
	{
		this.e = A_0;
	}

	// Token: 0x06000077 RID: 119 RVA: 0x00003685 File Offset: 0x00001885
	[CompilerGenerated]
	public int f()
	{
		return this.f;
	}

	// Token: 0x06000078 RID: 120 RVA: 0x0000368D File Offset: 0x0000188D
	[CompilerGenerated]
	public void a(int A_0)
	{
		this.f = A_0;
	}

	// Token: 0x06000079 RID: 121 RVA: 0x00003696 File Offset: 0x00001896
	[CompilerGenerated]
	public List<string> g()
	{
		return this.g;
	}

	// Token: 0x0600007A RID: 122 RVA: 0x0000369E File Offset: 0x0000189E
	[CompilerGenerated]
	private void a(List<string> A_0)
	{
		this.g = A_0;
	}

	// Token: 0x0600007B RID: 123 RVA: 0x000036A8 File Offset: 0x000018A8
	public void a(XmlNode A_0)
	{
		if (A_0 == null)
		{
			throw new ArgumentNullException("xmlNode");
		}
		this.a(ScadaUtils.GetChildAsString(A_0, "FrameColor", this.a()));
		this.b(ScadaUtils.GetChildAsString(A_0, "GridColor", this.b()));
		this.c(ScadaUtils.GetChildAsString(A_0, "BackColor", this.c()));
		this.d(ScadaUtils.GetChildAsString(A_0, "MarkerColor", this.d()));
		this.e(ScadaUtils.GetChildAsString(A_0, "SelectionColor", this.e()));
		this.a(ScadaUtils.GetChildAsInt(A_0, "LineWidth", this.f()));
		XmlNode xmlNode = A_0.SelectSingleNode("TrendColors");
		if (xmlNode != null)
		{
			foreach (object obj in xmlNode.SelectNodes("Color"))
			{
				XmlNode xmlNode2 = (XmlNode)obj;
				this.g().Add(xmlNode2.InnerText);
			}
		}
	}

	// Token: 0x04000029 RID: 41
	[CompilerGenerated]
#pragma warning disable CS0102 // 类型“j”已经包含“a”的定义
	private string a;
#pragma warning restore CS0102 // 类型“j”已经包含“a”的定义

	// Token: 0x0400002A RID: 42
	[CompilerGenerated]
#pragma warning disable CS0102 // 类型“j”已经包含“b”的定义
	private string b;
#pragma warning restore CS0102 // 类型“j”已经包含“b”的定义

	// Token: 0x0400002B RID: 43
	[CompilerGenerated]
#pragma warning disable CS0102 // 类型“j”已经包含“c”的定义
	private string c;
#pragma warning restore CS0102 // 类型“j”已经包含“c”的定义

	// Token: 0x0400002C RID: 44
	[CompilerGenerated]
#pragma warning disable CS0102 // 类型“j”已经包含“d”的定义
	private string d;
#pragma warning restore CS0102 // 类型“j”已经包含“d”的定义

	// Token: 0x0400002D RID: 45
	[CompilerGenerated]
#pragma warning disable CS0102 // 类型“j”已经包含“e”的定义
	private string e;
#pragma warning restore CS0102 // 类型“j”已经包含“e”的定义

	// Token: 0x0400002E RID: 46
	[CompilerGenerated]
#pragma warning disable CS0102 // 类型“j”已经包含“f”的定义
	private int f;
#pragma warning restore CS0102 // 类型“j”已经包含“f”的定义

	// Token: 0x0400002F RID: 47
	[CompilerGenerated]
#pragma warning disable CS0102 // 类型“j”已经包含“g”的定义
	private List<string> g;
#pragma warning restore CS0102 // 类型“j”已经包含“g”的定义
}
