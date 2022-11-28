using System;
using System.Runtime.CompilerServices;
using System.Xml;
using Scada;

// Token: 0x02000009 RID: 9
internal class e
{
	// Token: 0x06000020 RID: 32 RVA: 0x000025C0 File Offset: 0x000007C0
	public e()
	{
		this.a(new int[0]);
		this.a("Arial");
		this.b("#ffffff");
	}

	// Token: 0x06000021 RID: 33 RVA: 0x000025EA File Offset: 0x000007EA
	[CompilerGenerated]
	public int[] a()
	{
		return this.a;
	}

	// Token: 0x06000022 RID: 34 RVA: 0x000025F2 File Offset: 0x000007F2
	[CompilerGenerated]
	public void a(int[] A_0)
	{
		this.a = A_0;
	}

	// Token: 0x06000023 RID: 35 RVA: 0x000025FB File Offset: 0x000007FB
	[CompilerGenerated]
	public string b()
	{
		return this.b;
	}

	// Token: 0x06000024 RID: 36 RVA: 0x00002603 File Offset: 0x00000803
	[CompilerGenerated]
	public void a(string A_0)
	{
		this.b = A_0;
	}

	// Token: 0x06000025 RID: 37 RVA: 0x0000260C File Offset: 0x0000080C
	[CompilerGenerated]
	public string c()
	{
		return this.c;
	}

	// Token: 0x06000026 RID: 38 RVA: 0x00002614 File Offset: 0x00000814
	[CompilerGenerated]
	public void b(string A_0)
	{
		this.c = A_0;
	}

	// Token: 0x06000027 RID: 39 RVA: 0x00002620 File Offset: 0x00000820
	public void a(XmlNode A_0)
	{
		if (A_0 == null)
		{
			throw new ArgumentNullException("xmlNode");
		}
		this.a(ScadaUtils.ParseIntArray(ScadaUtils.GetChildAsString(A_0, "ChartPadding", "")));
		this.a(ScadaUtils.GetChildAsString(A_0, "FontName", this.b()));
		this.b(ScadaUtils.GetChildAsString(A_0, "BackColor", this.c()));
	}

	// Token: 0x0400000B RID: 11
	[CompilerGenerated]
#pragma warning disable CS0102 // 类型“e”已经包含“a”的定义
	private int[] a;
#pragma warning restore CS0102 // 类型“e”已经包含“a”的定义

	// Token: 0x0400000C RID: 12
	[CompilerGenerated]
#pragma warning disable CS0102 // 类型“e”已经包含“b”的定义
	private string b;
#pragma warning restore CS0102 // 类型“e”已经包含“b”的定义

	// Token: 0x0400000D RID: 13
	[CompilerGenerated]
#pragma warning disable CS0102 // 类型“e”已经包含“c”的定义
	private string c;
#pragma warning restore CS0102 // 类型“e”已经包含“c”的定义
}
