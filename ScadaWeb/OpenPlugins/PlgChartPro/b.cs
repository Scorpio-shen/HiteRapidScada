using System;
using System.Collections.Generic;
using System.IO;
using Scada.Web;

// Token: 0x02000005 RID: 5
internal class b
{
	// Token: 0x06000011 RID: 17 RVA: 0x00002451 File Offset: 0x00000651
	private b()
	{
		this.b = new Dictionary<string, SettingsUpdater>();
	}

	// Token: 0x06000012 RID: 18 RVA: 0x00002464 File Offset: 0x00000664
	public bool a(string A_0, out i A_1, out string A_2)
	{
		A_0 = (string.IsNullOrWhiteSpace(A_0) ? "PlgChartPro.xml" : A_0.Trim());
		Dictionary<string, SettingsUpdater> obj = this.b;
		bool result;
		lock (obj)
		{
			SettingsUpdater settingsUpdater;
			if (!this.b.TryGetValue(A_0, out settingsUpdater))
			{
				AppData appData = AppData.GetAppData();
				settingsUpdater = new SettingsUpdater(new i(), Path.Combine(appData.AppDirs.ConfigDir, A_0), true, appData.Log);
				this.b[A_0] = settingsUpdater;
			}
			result = settingsUpdater.Update(ref A_2);
			A_1 = (i)settingsUpdater.Settings;
		}
		return result;
	}

	// Token: 0x06000013 RID: 19 RVA: 0x00002514 File Offset: 0x00000714
	public static b a()
	{
		return global::b.a;
	}

	// Token: 0x04000008 RID: 8
#pragma warning disable CS0102 // 类型“b”已经包含“a”的定义
	private static readonly b a = new b();
#pragma warning restore CS0102 // 类型“b”已经包含“a”的定义

	// Token: 0x04000009 RID: 9
#pragma warning disable CS0542 // “b”: 成员名不能与它们的封闭类型相同
#pragma warning disable CS0246 // 未能找到类型或命名空间名“SettingsUpdater”(是否缺少 using 指令或程序集引用?)
	private Dictionary<string, SettingsUpdater> b;
#pragma warning restore CS0246 // 未能找到类型或命名空间名“SettingsUpdater”(是否缺少 using 指令或程序集引用?)
#pragma warning restore CS0542 // “b”: 成员名不能与它们的封闭类型相同
}
