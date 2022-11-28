using System;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using Scada;

// Token: 0x02000008 RID: 8
internal class d
{
	// Token: 0x0600001B RID: 27 RVA: 0x00002543 File Offset: 0x00000743
	public d()
	{
		this.a(60000);
	}

	// Token: 0x0600001C RID: 28 RVA: 0x00002556 File Offset: 0x00000756
	[CompilerGenerated]
	public int a()
	{
		return this.a;
	}

	// Token: 0x0600001D RID: 29 RVA: 0x0000255E File Offset: 0x0000075E
	[CompilerGenerated]
	public void a(int A_0)
	{
		this.a = A_0;
	}

	// Token: 0x0600001E RID: 30 RVA: 0x00002567 File Offset: 0x00000767
	public void a(XmlNode A_0)
	{
		if (A_0 == null)
		{
			throw new ArgumentNullException("xmlNode");
		}
		this.a(ScadaUtils.GetChildAsInt(A_0, "RefreshRate", this.a()));
	}

	// Token: 0x0600001F RID: 31 RVA: 0x0000258E File Offset: 0x0000078E
	public void a(StringBuilder A_0)
	{
		A_0.AppendLine("var behaviorOptions = new scada.chart.BehaviorOptions();").Append("behaviorOptions.refreshRate = ").Append(this.a()).AppendLine(";").AppendLine();
	}

	// Token: 0x0400000A RID: 10
	[CompilerGenerated]
#pragma warning disable CS0102 // 类型“d”已经包含“a”的定义
	private int a;
#pragma warning restore CS0102 // 类型“d”已经包含“a”的定义
}
