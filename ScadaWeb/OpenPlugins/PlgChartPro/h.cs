using System;
using System.Runtime.CompilerServices;
using System.Xml;
using Scada;
using Scada.Web.Plugins.ChartPro.Config;

// Token: 0x0200000C RID: 12
internal class h
{
	// Token: 0x0600004E RID: 78 RVA: 0x000032D4 File Offset: 0x000014D4
	public h()
	{
		this.a(AreaPosition.Bottom);
		this.a(300);
		int[] array = new int[3];
		array[0] = 10;
		array[1] = 10;
		this.a(array);
		this.b(1);
		this.c(18);
		this.d(12);
		this.e(12);
		this.f(12);
		this.a("#000000");
	}

	// Token: 0x0600004F RID: 79 RVA: 0x00003341 File Offset: 0x00001541
	[CompilerGenerated]
	public AreaPosition a()
	{
		return this.a;
	}

	// Token: 0x06000050 RID: 80 RVA: 0x00003349 File Offset: 0x00001549
	[CompilerGenerated]
	public void a(AreaPosition A_0)
	{
		this.a = A_0;
	}

	// Token: 0x06000051 RID: 81 RVA: 0x00003352 File Offset: 0x00001552
	[CompilerGenerated]
	public int b()
	{
		return this.b;
	}

	// Token: 0x06000052 RID: 82 RVA: 0x0000335A File Offset: 0x0000155A
	[CompilerGenerated]
	public void a(int A_0)
	{
		this.b = A_0;
	}

	// Token: 0x06000053 RID: 83 RVA: 0x00003363 File Offset: 0x00001563
	[CompilerGenerated]
	public int[] c()
	{
		return this.c;
	}

	// Token: 0x06000054 RID: 84 RVA: 0x0000336B File Offset: 0x0000156B
	[CompilerGenerated]
	public void a(int[] A_0)
	{
		this.c = A_0;
	}

	// Token: 0x06000055 RID: 85 RVA: 0x00003374 File Offset: 0x00001574
	[CompilerGenerated]
	public int d()
	{
		return this.d;
	}

	// Token: 0x06000056 RID: 86 RVA: 0x0000337C File Offset: 0x0000157C
	[CompilerGenerated]
	public void b(int A_0)
	{
		this.d = A_0;
	}

	// Token: 0x06000057 RID: 87 RVA: 0x00003385 File Offset: 0x00001585
	[CompilerGenerated]
	public int e()
	{
		return this.e;
	}

	// Token: 0x06000058 RID: 88 RVA: 0x0000338D File Offset: 0x0000158D
	[CompilerGenerated]
	public void c(int A_0)
	{
		this.e = A_0;
	}

	// Token: 0x06000059 RID: 89 RVA: 0x00003396 File Offset: 0x00001596
	[CompilerGenerated]
	public int f()
	{
		return this.f;
	}

	// Token: 0x0600005A RID: 90 RVA: 0x0000339E File Offset: 0x0000159E
	[CompilerGenerated]
	public void d(int A_0)
	{
		this.f = A_0;
	}

	// Token: 0x0600005B RID: 91 RVA: 0x000033A7 File Offset: 0x000015A7
	[CompilerGenerated]
	public int g()
	{
		return this.g;
	}

	// Token: 0x0600005C RID: 92 RVA: 0x000033AF File Offset: 0x000015AF
	[CompilerGenerated]
	public void e(int A_0)
	{
		this.g = A_0;
	}

	// Token: 0x0600005D RID: 93 RVA: 0x000033B8 File Offset: 0x000015B8
	[CompilerGenerated]
#pragma warning disable CS0542 // “h”: 成员名不能与它们的封闭类型相同
	public int h()
#pragma warning restore CS0542 // “h”: 成员名不能与它们的封闭类型相同
	{
		return this.h;
	}

	// Token: 0x0600005E RID: 94 RVA: 0x000033C0 File Offset: 0x000015C0
	[CompilerGenerated]
	public void f(int A_0)
	{
		this.h = A_0;
	}

	// Token: 0x0600005F RID: 95 RVA: 0x000033C9 File Offset: 0x000015C9
	[CompilerGenerated]
	public string i()
	{
		return this.i;
	}

	// Token: 0x06000060 RID: 96 RVA: 0x000033D1 File Offset: 0x000015D1
	[CompilerGenerated]
	public void a(string A_0)
	{
		this.i = A_0;
	}

	// Token: 0x06000061 RID: 97 RVA: 0x000033DC File Offset: 0x000015DC
	public void a(XmlNode A_0)
	{
		if (A_0 == null)
		{
			throw new ArgumentNullException("xmlNode");
		}
		this.a(ScadaUtils.GetChildAsEnum<AreaPosition>(A_0, "Position", this.a()));
		this.a(ScadaUtils.GetChildAsInt(A_0, "ColumnWidth", this.b()));
		this.a(ScadaUtils.ParseIntArray(ScadaUtils.GetChildAsString(A_0, "ColumnMargin", "")));
		this.b(ScadaUtils.GetChildAsInt(A_0, "ColumnCount", this.d()));
		this.c(ScadaUtils.GetChildAsInt(A_0, "LineHeight", this.e()));
		this.d(ScadaUtils.GetChildAsInt(A_0, "IconWidth", this.f()));
		this.e(ScadaUtils.GetChildAsInt(A_0, "IconHeight", this.g()));
		this.f(ScadaUtils.GetChildAsInt(A_0, "FontSize", this.h()));
		this.a(ScadaUtils.GetChildAsString(A_0, "ForeColor", this.i()));
	}

	// Token: 0x0400001D RID: 29
	[CompilerGenerated]
#pragma warning disable CS0102 // 类型“h”已经包含“a”的定义
	private AreaPosition a;
#pragma warning restore CS0102 // 类型“h”已经包含“a”的定义

	// Token: 0x0400001E RID: 30
	[CompilerGenerated]
#pragma warning disable CS0102 // 类型“h”已经包含“b”的定义
	private int b;
#pragma warning restore CS0102 // 类型“h”已经包含“b”的定义

	// Token: 0x0400001F RID: 31
	[CompilerGenerated]
#pragma warning disable CS0102 // 类型“h”已经包含“c”的定义
	private int[] c;
#pragma warning restore CS0102 // 类型“h”已经包含“c”的定义

	// Token: 0x04000020 RID: 32
	[CompilerGenerated]
#pragma warning disable CS0102 // 类型“h”已经包含“d”的定义
	private int d;
#pragma warning restore CS0102 // 类型“h”已经包含“d”的定义

	// Token: 0x04000021 RID: 33
	[CompilerGenerated]
#pragma warning disable CS0102 // 类型“h”已经包含“e”的定义
	private int e;
#pragma warning restore CS0102 // 类型“h”已经包含“e”的定义

	// Token: 0x04000022 RID: 34
	[CompilerGenerated]
#pragma warning disable CS0102 // 类型“h”已经包含“f”的定义
	private int f;
#pragma warning restore CS0102 // 类型“h”已经包含“f”的定义

	// Token: 0x04000023 RID: 35
	[CompilerGenerated]
#pragma warning disable CS0102 // 类型“h”已经包含“g”的定义
	private int g;
#pragma warning restore CS0102 // 类型“h”已经包含“g”的定义

	// Token: 0x04000024 RID: 36
	[CompilerGenerated]
#pragma warning disable CS0542 // “h”: 成员名不能与它们的封闭类型相同
#pragma warning disable CS0102 // 类型“h”已经包含“h”的定义
	private int h;
#pragma warning restore CS0102 // 类型“h”已经包含“h”的定义
#pragma warning restore CS0542 // “h”: 成员名不能与它们的封闭类型相同

	// Token: 0x04000025 RID: 37
	[CompilerGenerated]
#pragma warning disable CS0102 // 类型“h”已经包含“i”的定义
	private string i;
#pragma warning restore CS0102 // 类型“h”已经包含“i”的定义
}
