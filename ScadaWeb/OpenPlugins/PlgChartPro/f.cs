using System;
using System.Runtime.CompilerServices;
using System.Xml;
using Scada;

// Token: 0x0200000A RID: 10
internal class f
{
	// Token: 0x06000028 RID: 40 RVA: 0x00002684 File Offset: 0x00000884
	public f()
	{
		this.a(true);
		this.b(true);
		this.c(true);
		this.a(30);
		this.b(17);
		this.a("#333333");
	}

	// Token: 0x06000029 RID: 41 RVA: 0x000026BC File Offset: 0x000008BC
	[CompilerGenerated]
	public bool a()
	{
		return this.a;
	}

	// Token: 0x0600002A RID: 42 RVA: 0x000026C4 File Offset: 0x000008C4
	[CompilerGenerated]
	public void a(bool A_0)
	{
		this.a = A_0;
	}

	// Token: 0x0600002B RID: 43 RVA: 0x000026CD File Offset: 0x000008CD
	[CompilerGenerated]
	public bool b()
	{
		return this.b;
	}

	// Token: 0x0600002C RID: 44 RVA: 0x000026D5 File Offset: 0x000008D5
	[CompilerGenerated]
	public void b(bool A_0)
	{
		this.b = A_0;
	}

	// Token: 0x0600002D RID: 45 RVA: 0x000026DE File Offset: 0x000008DE
	[CompilerGenerated]
	public bool c()
	{
		return this.c;
	}

	// Token: 0x0600002E RID: 46 RVA: 0x000026E6 File Offset: 0x000008E6
	[CompilerGenerated]
	public void c(bool A_0)
	{
		this.c = A_0;
	}

	// Token: 0x0600002F RID: 47 RVA: 0x000026EF File Offset: 0x000008EF
	[CompilerGenerated]
	public int d()
	{
		return this.d;
	}

	// Token: 0x06000030 RID: 48 RVA: 0x000026F7 File Offset: 0x000008F7
	[CompilerGenerated]
	public void a(int A_0)
	{
		this.d = A_0;
	}

	// Token: 0x06000031 RID: 49 RVA: 0x00002700 File Offset: 0x00000900
	[CompilerGenerated]
	public int e()
	{
		return this.e;
	}

	// Token: 0x06000032 RID: 50 RVA: 0x00002708 File Offset: 0x00000908
	[CompilerGenerated]
	public void b(int A_0)
	{
		this.e = A_0;
	}

	// Token: 0x06000033 RID: 51 RVA: 0x00002711 File Offset: 0x00000911
	[CompilerGenerated]
#pragma warning disable CS0542 // “f”: 成员名不能与它们的封闭类型相同
	public string f()
#pragma warning restore CS0542 // “f”: 成员名不能与它们的封闭类型相同
	{
		return this.f;
	}

	// Token: 0x06000034 RID: 52 RVA: 0x00002719 File Offset: 0x00000919
	[CompilerGenerated]
	public void a(string A_0)
	{
		this.f = A_0;
	}

	// Token: 0x06000035 RID: 53 RVA: 0x00002724 File Offset: 0x00000924
	public void a(XmlNode A_0)
	{
		if (A_0 == null)
		{
			throw new ArgumentNullException("xmlNode");
		}
		this.a(ScadaUtils.GetChildAsBool(A_0, "ShowTitle", this.a()));
		this.b(ScadaUtils.GetChildAsBool(A_0, "ShowMenu", this.b()));
		this.c(ScadaUtils.GetChildAsBool(A_0, "ShowStatus", this.c()));
		this.a(ScadaUtils.GetChildAsInt(A_0, "Height", this.d()));
		this.b(ScadaUtils.GetChildAsInt(A_0, "FontSize", this.e()));
		this.a(ScadaUtils.GetChildAsString(A_0, "ForeColor", this.f()));
	}

	// Token: 0x0400000E RID: 14
	[CompilerGenerated]
#pragma warning disable CS0102 // 类型“f”已经包含“a”的定义
	private bool a;
#pragma warning restore CS0102 // 类型“f”已经包含“a”的定义

	// Token: 0x0400000F RID: 15
	[CompilerGenerated]
#pragma warning disable CS0102 // 类型“f”已经包含“b”的定义
	private bool b;
#pragma warning restore CS0102 // 类型“f”已经包含“b”的定义

	// Token: 0x04000010 RID: 16
	[CompilerGenerated]
#pragma warning disable CS0102 // 类型“f”已经包含“c”的定义
	private bool c;
#pragma warning restore CS0102 // 类型“f”已经包含“c”的定义

	// Token: 0x04000011 RID: 17
	[CompilerGenerated]
#pragma warning disable CS0102 // 类型“f”已经包含“d”的定义
	private int d;
#pragma warning restore CS0102 // 类型“f”已经包含“d”的定义

	// Token: 0x04000012 RID: 18
	[CompilerGenerated]
#pragma warning disable CS0102 // 类型“f”已经包含“e”的定义
	private int e;
#pragma warning restore CS0102 // 类型“f”已经包含“e”的定义

	// Token: 0x04000013 RID: 19
	[CompilerGenerated]
#pragma warning disable CS0542 // “f”: 成员名不能与它们的封闭类型相同
#pragma warning disable CS0102 // 类型“f”已经包含“f”的定义
	private string f;
#pragma warning restore CS0102 // 类型“f”已经包含“f”的定义
#pragma warning restore CS0542 // “f”: 成员名不能与它们的封闭类型相同
}
