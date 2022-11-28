using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Scada.Web.Plugins;
using Scada.Web.Plugins.Chart;
using Utils.KeyGen;

namespace Scada.Web.Plugins
{
	// Token: 0x02000003 RID: 3
#pragma warning disable CS0246 // 未能找到类型或命名空间名“PluginSpec”(是否缺少 using 指令或程序集引用?)
	public class PlgChartProSpec : PluginSpec
#pragma warning restore CS0246 // 未能找到类型或命名空间名“PluginSpec”(是否缺少 using 指令或程序集引用?)
	{
		// Token: 0x17000003 RID: 3
		// (get) Token: 0x06000004 RID: 4 RVA: 0x000022CB File Offset: 0x000004CB
#pragma warning disable CS0115 // “PlgChartProSpec.Name”: 没有找到适合的方法来重写
		public override string Name
#pragma warning restore CS0115 // “PlgChartProSpec.Name”: 没有找到适合的方法来重写
		{
			get
			{
				return PlgChartProSpec.PlgName;
			}
		}

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x06000005 RID: 5 RVA: 0x000022D2 File Offset: 0x000004D2
#pragma warning disable CS0115 // “PlgChartProSpec.Descr”: 没有找到适合的方法来重写
		public override string Descr
#pragma warning restore CS0115 // “PlgChartProSpec.Descr”: 没有找到适合的方法来重写
		{
			get
			{
				if (!Localization.UseRussian)
				{
					return "The plugin provides displaying, scaling and export of multiple charts.";
				}
				return "Плагин обеспечивает отображение, масштабирование и экспорт нескольких графиков.";
			}
		}

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x06000006 RID: 6 RVA: 0x000022E6 File Offset: 0x000004E6
#pragma warning disable CS0115 // “PlgChartProSpec.Version”: 没有找到适合的方法来重写
		public override string Version
#pragma warning restore CS0115 // “PlgChartProSpec.Version”: 没有找到适合的方法来重写
		{
			get
			{
				return "5.1.0.1";
			}
		}

		// Token: 0x17000006 RID: 6
		// (get) Token: 0x06000007 RID: 7 RVA: 0x000022ED File Offset: 0x000004ED
#pragma warning disable CS0246 // 未能找到类型或命名空间名“ScriptPaths”(是否缺少 using 指令或程序集引用?)
#pragma warning disable CS0115 // “PlgChartProSpec.ScriptPaths”: 没有找到适合的方法来重写
		public override ScriptPaths ScriptPaths
#pragma warning restore CS0115 // “PlgChartProSpec.ScriptPaths”: 没有找到适合的方法来重写
#pragma warning restore CS0246 // 未能找到类型或命名空间名“ScriptPaths”(是否缺少 using 指令或程序集引用?)
		{
			get
			{
				return new ScriptPaths
				{
					ChartScriptPath = "~/plugins/ChartPro/js/chartdialog.js"
				};
			}
		}

		// Token: 0x17000007 RID: 7
		// (get) Token: 0x06000008 RID: 8 RVA: 0x000022FF File Offset: 0x000004FF
		public static string PlgName
		{
			get
			{
				if (!Localization.UseRussian)
				{
					return "Chart Pro";
				}
				return "Графики Про";
			}
		}

		// Token: 0x06000009 RID: 9 RVA: 0x00002314 File Offset: 0x00000514
#pragma warning disable CS0115 // “PlgChartProSpec.Init()”: 没有找到适合的方法来重写
		public override void Init()
#pragma warning restore CS0115 // “PlgChartProSpec.Init()”: 没有找到适合的方法来重写
		{
			this.d = new DictUpdater(string.Format("{0}ChartPro{1}lang{1}", base.AppDirs.PluginsDir, Path.DirectorySeparatorChar), "PlgChartPro", new Action(PlgChartProSpec.a.a.a), base.Log);
		}

		// Token: 0x0600000A RID: 10 RVA: 0x00002378 File Offset: 0x00000578
#pragma warning disable CS0246 // 未能找到类型或命名空间名“UserData”(是否缺少 using 指令或程序集引用?)
		public override void OnUserLogin(UserData userData)
#pragma warning restore CS0246 // 未能找到类型或命名空间名“UserData”(是否缺少 using 指令或程序集引用?)
		{
			this.d.Update();
			Localization.Dict dict;
			if (Localization.Dictionaries.TryGetValue("KeyGen", out dict))
			{
				n.a(dict.Phrases);
			}
			WebRegPublic.AddToSession(new c(), AppData.GetAppData().AppDirs.ConfigDir);
		}

		// Token: 0x0600000B RID: 11 RVA: 0x000023C8 File Offset: 0x000005C8
#pragma warning disable CS0246 // 未能找到类型或命名空间名“MenuItem”(是否缺少 using 指令或程序集引用?)
#pragma warning disable CS0246 // 未能找到类型或命名空间名“UserData”(是否缺少 using 指令或程序集引用?)
		public override List<MenuItem> GetMenuItems(UserData userData)
#pragma warning restore CS0246 // 未能找到类型或命名空间名“UserData”(是否缺少 using 指令或程序集引用?)
#pragma warning restore CS0246 // 未能找到类型或命名空间名“MenuItem”(是否缺少 using 指令或程序集引用?)
		{
			if (userData.UserRights.ConfigRight)
			{
				List<MenuItem> list = new List<MenuItem>();
				MenuItem menuItem = MenuItem.FromStandardMenuItem(8);
				menuItem.Subitems.Add(new MenuItem(this.Name, "~/plugins/Registration/Registration.aspx?prodCode=PlgChartPro", 0));
				list.Add(menuItem);
				return list;
			}
			return null;
		}

		// Token: 0x04000003 RID: 3
#pragma warning disable CS0102 // 类型“PlgChartProSpec”已经包含“a”的定义
		internal const string a = "5.1.0.1";
#pragma warning restore CS0102 // 类型“PlgChartProSpec”已经包含“a”的定义

		// Token: 0x04000004 RID: 4
		internal const string b = "~/plugins/Registration/Registration.aspx?prodCode=PlgChartPro";

		// Token: 0x04000005 RID: 5
		internal const string c = "~/plugins/Registration/RegistrationNM.aspx?prodCode=PlgChartPro";

		// Token: 0x04000006 RID: 6
#pragma warning disable CS0246 // 未能找到类型或命名空间名“DictUpdater”(是否缺少 using 指令或程序集引用?)
		private DictUpdater d;
#pragma warning restore CS0246 // 未能找到类型或命名空间名“DictUpdater”(是否缺少 using 指令或程序集引用?)

		// Token: 0x0200001F RID: 31
		[CompilerGenerated]
		[Serializable]
		private sealed class a
		{
			// Token: 0x0600012B RID: 299 RVA: 0x000056B0 File Offset: 0x000038B0
#pragma warning disable CS0542 // “a”: 成员名不能与它们的封闭类型相同
			internal void a()
#pragma warning restore CS0542 // “a”: 成员名不能与它们的封闭类型相同
			{
				ChartPhrases.Init();
				global::a.a();
			}

			// Token: 0x04000087 RID: 135
#pragma warning disable CS0542 // “a”: 成员名不能与它们的封闭类型相同
#pragma warning disable CS0102 // 类型“PlgChartProSpec.a”已经包含“a”的定义
			public static readonly PlgChartProSpec.a a = new PlgChartProSpec.a();
#pragma warning restore CS0102 // 类型“PlgChartProSpec.a”已经包含“a”的定义
#pragma warning restore CS0542 // “a”: 成员名不能与它们的封闭类型相同

			// Token: 0x04000088 RID: 136
			public static Action b;
		}
	}
}
