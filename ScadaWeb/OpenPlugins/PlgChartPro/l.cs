using System;
using System.Runtime.CompilerServices;
using System.Xml;
using Scada;

// Token: 0x02000010 RID: 16
internal class l
{
	// Token: 0x0600009C RID: 156 RVA: 0x00003AD8 File Offset: 0x00001CD8
	public l()
	{
		this.a(30);
		this.a(true);
		this.b(true);
		this.b(4);
		this.c(2);
		this.c(true);
		this.a(new int[]
		{
			2,
			3
		});
		this.d(12);
		this.a("#808080");
		this.b("#000000");
	}

	// Token: 0x0600009D RID: 157 RVA: 0x00003B48 File Offset: 0x00001D48
	[CompilerGenerated]
	public int a()
	{
		return this.a;
	}

	// Token: 0x0600009E RID: 158 RVA: 0x00003B50 File Offset: 0x00001D50
	[CompilerGenerated]
	public void a(int A_0)
	{
		this.a = A_0;
	}

	// Token: 0x0600009F RID: 159 RVA: 0x00003B59 File Offset: 0x00001D59
	[CompilerGenerated]
	public bool b()
	{
		return this.b;
	}

	// Token: 0x060000A0 RID: 160 RVA: 0x00003B61 File Offset: 0x00001D61
	[CompilerGenerated]
	public void a(bool A_0)
	{
		this.b = A_0;
	}

	// Token: 0x060000A1 RID: 161 RVA: 0x00003B6A File Offset: 0x00001D6A
	[CompilerGenerated]
	public bool c()
	{
		return this.c;
	}

	// Token: 0x060000A2 RID: 162 RVA: 0x00003B72 File Offset: 0x00001D72
	[CompilerGenerated]
	public void b(bool A_0)
	{
		this.c = A_0;
	}

	// Token: 0x060000A3 RID: 163 RVA: 0x00003B7B File Offset: 0x00001D7B
	[CompilerGenerated]
	public int d()
	{
		return this.d;
	}

	// Token: 0x060000A4 RID: 164 RVA: 0x00003B83 File Offset: 0x00001D83
	[CompilerGenerated]
	public void b(int A_0)
	{
		this.d = A_0;
	}

	// Token: 0x060000A5 RID: 165 RVA: 0x00003B8C File Offset: 0x00001D8C
	[CompilerGenerated]
	public int e()
	{
		return this.e;
	}

	// Token: 0x060000A6 RID: 166 RVA: 0x00003B94 File Offset: 0x00001D94
	[CompilerGenerated]
	public void c(int A_0)
	{
		this.e = A_0;
	}

	// Token: 0x060000A7 RID: 167 RVA: 0x00003B9D File Offset: 0x00001D9D
	[CompilerGenerated]
	public bool f()
	{
		return this.f;
	}

	// Token: 0x060000A8 RID: 168 RVA: 0x00003BA5 File Offset: 0x00001DA5
	[CompilerGenerated]
	public void c(bool A_0)
	{
		this.f = A_0;
	}

	// Token: 0x060000A9 RID: 169 RVA: 0x00003BAE File Offset: 0x00001DAE
	[CompilerGenerated]
	public int[] g()
	{
		return this.g;
	}

	// Token: 0x060000AA RID: 170 RVA: 0x00003BB6 File Offset: 0x00001DB6
	[CompilerGenerated]
	public void a(int[] A_0)
	{
		this.g = A_0;
	}

	// Token: 0x060000AB RID: 171 RVA: 0x00003BBF File Offset: 0x00001DBF
	[CompilerGenerated]
	public int h()
	{
		return this.h;
	}

	// Token: 0x060000AC RID: 172 RVA: 0x00003BC7 File Offset: 0x00001DC7
	[CompilerGenerated]
	public void d(int A_0)
	{
		this.h = A_0;
	}

	// Token: 0x060000AD RID: 173 RVA: 0x00003BD0 File Offset: 0x00001DD0
	[CompilerGenerated]
	public string i()
	{
		return this.i;
	}

	// Token: 0x060000AE RID: 174 RVA: 0x00003BD8 File Offset: 0x00001DD8
	[CompilerGenerated]
	public void a(string A_0)
	{
		this.i = A_0;
	}

	// Token: 0x060000AF RID: 175 RVA: 0x00003BE1 File Offset: 0x00001DE1
	[CompilerGenerated]
	public string j()
	{
		return this.j;
	}

	// Token: 0x060000B0 RID: 176 RVA: 0x00003BE9 File Offset: 0x00001DE9
	[CompilerGenerated]
	public void b(string A_0)
	{
		this.j = A_0;
	}

	// Token: 0x060000B1 RID: 177 RVA: 0x00003BF4 File Offset: 0x00001DF4
	public void a(XmlNode A_0)
	{
		if (A_0 == null)
		{
			throw new ArgumentNullException("xmlNode");
		}
		this.a(ScadaUtils.GetChildAsInt(A_0, "Height", this.a()));
		this.a(ScadaUtils.GetChildAsBool(A_0, "ShowGridLines", this.b()));
		this.b(ScadaUtils.GetChildAsBool(A_0, "ShowDates", this.c()));
		this.b(ScadaUtils.GetChildAsInt(A_0, "MajorTickSize", this.d()));
		this.c(ScadaUtils.GetChildAsInt(A_0, "MinorTickSize", this.e()));
		this.c(ScadaUtils.GetChildAsBool(A_0, "ShowMinorTicks", this.f()));
		this.a(ScadaUtils.ParseIntArray(ScadaUtils.GetChildAsString(A_0, "LabelMargin", "")));
		this.d(ScadaUtils.GetChildAsInt(A_0, "FontSize", this.h()));
		this.a(ScadaUtils.GetChildAsString(A_0, "LineColor", this.i()));
		this.b(ScadaUtils.GetChildAsString(A_0, "TextColor", this.j()));
	}

	// Token: 0x0400003F RID: 63
	[CompilerGenerated]
#pragma warning disable CS0102 // 类型“l”已经包含“a”的定义
	private int a;
#pragma warning restore CS0102 // 类型“l”已经包含“a”的定义

	// Token: 0x04000040 RID: 64
	[CompilerGenerated]
#pragma warning disable CS0102 // 类型“l”已经包含“b”的定义
	private bool b;
#pragma warning restore CS0102 // 类型“l”已经包含“b”的定义

	// Token: 0x04000041 RID: 65
	[CompilerGenerated]
#pragma warning disable CS0102 // 类型“l”已经包含“c”的定义
	private bool c;
#pragma warning restore CS0102 // 类型“l”已经包含“c”的定义

	// Token: 0x04000042 RID: 66
	[CompilerGenerated]
#pragma warning disable CS0102 // 类型“l”已经包含“d”的定义
	private int d;
#pragma warning restore CS0102 // 类型“l”已经包含“d”的定义

	// Token: 0x04000043 RID: 67
	[CompilerGenerated]
#pragma warning disable CS0102 // 类型“l”已经包含“e”的定义
	private int e;
#pragma warning restore CS0102 // 类型“l”已经包含“e”的定义

	// Token: 0x04000044 RID: 68
	[CompilerGenerated]
#pragma warning disable CS0102 // 类型“l”已经包含“f”的定义
	private bool f;
#pragma warning restore CS0102 // 类型“l”已经包含“f”的定义

	// Token: 0x04000045 RID: 69
	[CompilerGenerated]
#pragma warning disable CS0102 // 类型“l”已经包含“g”的定义
	private int[] g;
#pragma warning restore CS0102 // 类型“l”已经包含“g”的定义

	// Token: 0x04000046 RID: 70
	[CompilerGenerated]
#pragma warning disable CS0102 // 类型“l”已经包含“h”的定义
	private int h;
#pragma warning restore CS0102 // 类型“l”已经包含“h”的定义

	// Token: 0x04000047 RID: 71
	[CompilerGenerated]
#pragma warning disable CS0102 // 类型“l”已经包含“i”的定义
	private string i;
#pragma warning restore CS0102 // 类型“l”已经包含“i”的定义

	// Token: 0x04000048 RID: 72
	[CompilerGenerated]
#pragma warning disable CS0102 // 类型“l”已经包含“j”的定义
	private string j;
#pragma warning restore CS0102 // 类型“l”已经包含“j”的定义
}
