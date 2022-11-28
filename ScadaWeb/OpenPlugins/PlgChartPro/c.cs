using System;
using Scada.Web.Plugins;
using Utils.KeyGen;

// Token: 0x02000006 RID: 6
#pragma warning disable CS0535 // “c”不实现接口成员“IAppReg.ProdCode”
#pragma warning disable CS0535 // “c”不实现接口成员“IAppReg.ProdName”
#pragma warning disable CS0535 // “c”不实现接口成员“IAppReg.KeyFileName”
internal sealed class c : IAppReg
#pragma warning restore CS0535 // “c”不实现接口成员“IAppReg.KeyFileName”
#pragma warning restore CS0535 // “c”不实现接口成员“IAppReg.ProdName”
#pragma warning restore CS0535 // “c”不实现接口成员“IAppReg.ProdCode”
{
	// Token: 0x06000014 RID: 20 RVA: 0x0000251B File Offset: 0x0000071B
#pragma warning disable CS0470 // 方法“c.get_ProdCode()”无法实现类型“c”的接口访问器“IAppReg.ProdCode.get” 请使用显式接口实现。
	public string get_ProdCode()
#pragma warning restore CS0470 // 方法“c.get_ProdCode()”无法实现类型“c”的接口访问器“IAppReg.ProdCode.get” 请使用显式接口实现。
	{
		return "PlgChartPro";
	}

	// Token: 0x06000015 RID: 21 RVA: 0x00002522 File Offset: 0x00000722
#pragma warning disable CS0470 // 方法“c.get_ProdName()”无法实现类型“c”的接口访问器“IAppReg.ProdName.get” 请使用显式接口实现。
	public string get_ProdName()
#pragma warning restore CS0470 // 方法“c.get_ProdName()”无法实现类型“c”的接口访问器“IAppReg.ProdName.get” 请使用显式接口实现。
	{
		return PlgChartProSpec.PlgName;
	}

	// Token: 0x06000016 RID: 22 RVA: 0x00002529 File Offset: 0x00000729
#pragma warning disable CS0470 // 方法“c.get_KeyFileName()”无法实现类型“c”的接口访问器“IAppReg.KeyFileName.get” 请使用显式接口实现。
	public string get_KeyFileName()
#pragma warning restore CS0470 // 方法“c.get_KeyFileName()”无法实现类型“c”的接口访问器“IAppReg.KeyFileName.get” 请使用显式接口实现。
	{
		return this.get_ProdCode() + "_Reg.xml";
	}
}
