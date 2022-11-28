using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Web.Script.Serialization;
using Scada.Client;
using Scada.Data.Models;
using Scada.Data.Tables;
using Scada.Web.Plugins.Chart;

namespace Scada.Web.Plugins.ChartPro
{
	// Token: 0x02000026 RID: 38
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
	[ServiceContract(Namespace = "")]
	public class ChartSvc
	{
		// Token: 0x0600014C RID: 332 RVA: 0x00006514 File Offset: 0x00004714
#pragma warning disable CS0246 // 未能找到类型或命名空间名“UserRights”(是否缺少 using 指令或程序集引用?)
		private IList<int> a(string A_0, string A_1, UserRights A_2)
#pragma warning restore CS0246 // 未能找到类型或命名空间名“UserRights”(是否缺少 using 指令或程序集引用?)
		{
			int[] array = ScadaUtils.ParseIntArray(A_0);
			int[] array2 = ScadaUtils.ParseIntArray(A_1);
			if (!A_2.CheckInCnlRights(array, array2))
			{
				throw new ScadaException(CommonPhrases.NoRights);
			}
			return array;
		}

		// Token: 0x0600014D RID: 333 RVA: 0x00006548 File Offset: 0x00004748
		private ChartSvc.ChartDTO a(IList<int> A_0, DateTime A_1, DateTime A_2)
		{
			ChartSvc.ChartDTO chartDTO = new ChartSvc.ChartDTO(A_1);
			DateTime date = A_2.Date;
			chartDTO.ReqDateStr = date.ToString("yyyy-MM-dd");
			DataAccess dataAccess = ChartSvc.d.DataAccess;
			int num = (A_0 == null) ? 0 : A_0.Count;
			Trend[] array = new Trend[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = dataAccess.DataCache.GetMinTrend(date, A_0[i]);
			}
			TrendBundle trendBundle = new TrendBundle();
			trendBundle.Init(array, A_2);
			List<TrendBundle.Point> series = trendBundle.Series;
			int count = series.Count;
			double[] array2 = new double[count];
			chartDTO.TimePoints = array2;
			for (int j = 0; j < count; j++)
			{
				array2[j] = ScadaUtils.EncodeDateTime(series[j].DateTime);
			}
			chartDTO.TrendDataArr = new ChartSvc.TrendData[num];
			for (int k = 0; k < num; k++)
			{
				object[][] array3 = new object[count][];
				ChartSvc.TrendData trendData = new ChartSvc.TrendData
				{
					TrendPoints = array3
				};
				chartDTO.TrendDataArr[k] = trendData;
				InCnlProps cnlProps = dataAccess.GetCnlProps(A_0[k]);
				for (int l = 0; l < count; l++)
				{
					SrezTableLight.CnlData cnlData = series[l].CnlData[k];
					double val = cnlData.Val;
					int stat = cnlData.Stat;
					string text;
					string textWithUnit;
					ChartSvc.c.FormatCnlVal(val, stat, cnlProps, ref text, ref textWithUnit);
					CnlStatProps cnlStatProps = dataAccess.GetCnlStatProps(stat);
					string cnlValColor = ChartSvc.c.GetCnlValColor(val, stat, cnlProps, cnlStatProps);
					array3[l] = ChartSvc.TrendData.CreateTrendPoint(val, stat, text, textWithUnit, cnlValColor);
				}
			}
			return chartDTO;
		}

		// Token: 0x0600014E RID: 334 RVA: 0x000066F0 File Offset: 0x000048F0
		[WebGet]
		[OperationContract]
		public string GetDailyChartData(string cnlNums, string viewIDs, double timeOffset)
		{
			string result;
			try
			{
				UserRights a_;
				ChartSvc.d.CheckLoggedOn(ref a_, true);
				IList<int> a_2 = this.a(cnlNums, viewIDs, a_);
				ChartSvc.ChartDTO obj = this.a(a_2, DateTime.Now, ScadaUtils.DecodeDateTime(timeOffset));
				result = ChartSvc.b.Serialize(obj);
			}
			catch (Exception ex)
			{
				ChartSvc.d.Log.WriteException(ex, Localization.UseRussian ? "Ошибка при получении данных графика за сутки, где каналы={0}, ид. представлений={1}" : "Error getting daily chart data where channels={0}, view ids={1}", new object[]
				{
					cnlNums,
					viewIDs
				});
				result = WebUtils.GetErrorJson(ChartSvc.b, ex);
			}
			return result;
		}

		// Token: 0x0600014F RID: 335 RVA: 0x0000678C File Offset: 0x0000498C
		[WebGet]
		[OperationContract]
		public string GetTailChartData(string cnlNums, string viewIDs, double timeOffset, int period)
		{
			string result;
			try
			{
				UserRights a_;
				ChartSvc.d.CheckLoggedOn(ref a_, true);
				IList<int> a_2 = this.a(cnlNums, viewIDs, a_);
				DateTime now = DateTime.Now;
				DateTime endDT = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);
				DateTime dateTime = endDT.AddMinutes((double)(-(double)period));
				DateTime dateTime2 = ScadaUtils.DecodeDateTime(timeOffset);
				ChartSvc.ChartDTO chartDTO = this.a(a_2, now, (dateTime > dateTime2) ? dateTime : dateTime2);
				if (chartDTO.TimePoints.Length != 0)
				{
					endDT = ScadaUtils.DecodeDateTime(chartDTO.TimePoints[chartDTO.TimePoints.Length - 1]);
					dateTime = endDT.AddMinutes((double)(-(double)period));
				}
				chartDTO.SetTimeRange(dateTime, endDT);
				result = ChartSvc.b.Serialize(chartDTO);
			}
			catch (Exception ex)
			{
				ChartSvc.d.Log.WriteException(ex, Localization.UseRussian ? "Ошибка при получении последних данных графика, где каналы={0}, ид. представлений={1}" : "Error getting tail chart data where channels={0}, view ids={1}", new object[]
				{
					cnlNums,
					viewIDs
				});
				result = WebUtils.GetErrorJson(ChartSvc.b, ex);
			}
			return result;
		}

		// Token: 0x040000CC RID: 204
#pragma warning disable CS0102 // 类型“ChartSvc”已经包含“a”的定义
		private const int a = 10485760;
#pragma warning restore CS0102 // 类型“ChartSvc”已经包含“a”的定义

		// Token: 0x040000CD RID: 205
		private static readonly JavaScriptSerializer b = new JavaScriptSerializer
		{
			MaxJsonLength = 10485760
		};

		// Token: 0x040000CE RID: 206
		private static readonly DataFormatter c = new DataFormatter();

		// Token: 0x040000CF RID: 207
#pragma warning disable CS0246 // 未能找到类型或命名空间名“AppData”(是否缺少 using 指令或程序集引用?)
		private static readonly AppData d = AppData.GetAppData();
#pragma warning restore CS0246 // 未能找到类型或命名空间名“AppData”(是否缺少 using 指令或程序集引用?)

		// Token: 0x02000027 RID: 39
		protected class TrendData
		{
			// Token: 0x1700000B RID: 11
			// (get) Token: 0x06000152 RID: 338 RVA: 0x000068E7 File Offset: 0x00004AE7
			// (set) Token: 0x06000153 RID: 339 RVA: 0x000068EF File Offset: 0x00004AEF
			public object[][] TrendPoints { get; set; }

			// Token: 0x06000154 RID: 340 RVA: 0x000068F8 File Offset: 0x00004AF8
			public static object[] CreateTrendPoint(double val, int stat, string text, string textWithUnit, string color)
			{
				double d = (stat > 0) ? val : double.NaN;
				return new object[]
				{
					double.IsNaN(d) ? "NaN" : val,
					text,
					textWithUnit,
					color
				};
			}

			// Token: 0x040000D0 RID: 208
			public const int ValInd = 0;

			// Token: 0x040000D1 RID: 209
			public const int TextInd = 1;

			// Token: 0x040000D2 RID: 210
			public const int TextWithUnitInd = 2;

			// Token: 0x040000D3 RID: 211
			public const int ColorInd = 3;

			// Token: 0x040000D4 RID: 212
			public const int PointArrLen = 4;

			// Token: 0x040000D5 RID: 213
			[CompilerGenerated]
			private object[][] a;
		}

		// Token: 0x02000028 RID: 40
#pragma warning disable CS0246 // 未能找到类型或命名空间名“DataTransferObject”(是否缺少 using 指令或程序集引用?)
		protected class ChartDTO : DataTransferObject
#pragma warning restore CS0246 // 未能找到类型或命名空间名“DataTransferObject”(是否缺少 using 指令或程序集引用?)
		{
			// Token: 0x06000156 RID: 342 RVA: 0x0000694C File Offset: 0x00004B4C
			public ChartDTO(DateTime nowDT)
			{
				this.TimePoints = null;
				this.TrendDataArr = null;
				this.StartTime = 0.0;
				this.StartDateStr = "";
				this.ReqDateStr = "";
				this.TodayStr = nowDT.ToString("yyyy-MM-dd");
				this.NowStr = Localization.ToLocalizedString(nowDT);
			}

			// Token: 0x1700000C RID: 12
			// (get) Token: 0x06000157 RID: 343 RVA: 0x000069B0 File Offset: 0x00004BB0
			// (set) Token: 0x06000158 RID: 344 RVA: 0x000069B8 File Offset: 0x00004BB8
			public double[] TimePoints { get; set; }

			// Token: 0x1700000D RID: 13
			// (get) Token: 0x06000159 RID: 345 RVA: 0x000069C1 File Offset: 0x00004BC1
			// (set) Token: 0x0600015A RID: 346 RVA: 0x000069C9 File Offset: 0x00004BC9
			public ChartSvc.TrendData[] TrendDataArr { get; set; }

			// Token: 0x1700000E RID: 14
			// (get) Token: 0x0600015B RID: 347 RVA: 0x000069D2 File Offset: 0x00004BD2
			// (set) Token: 0x0600015C RID: 348 RVA: 0x000069DA File Offset: 0x00004BDA
			public double StartTime { get; set; }

			// Token: 0x1700000F RID: 15
			// (get) Token: 0x0600015D RID: 349 RVA: 0x000069E3 File Offset: 0x00004BE3
			// (set) Token: 0x0600015E RID: 350 RVA: 0x000069EB File Offset: 0x00004BEB
			public double EndTime { get; set; }

			// Token: 0x17000010 RID: 16
			// (get) Token: 0x0600015F RID: 351 RVA: 0x000069F4 File Offset: 0x00004BF4
			// (set) Token: 0x06000160 RID: 352 RVA: 0x000069FC File Offset: 0x00004BFC
			public string StartDateStr { get; set; }

			// Token: 0x17000011 RID: 17
			// (get) Token: 0x06000161 RID: 353 RVA: 0x00006A05 File Offset: 0x00004C05
			// (set) Token: 0x06000162 RID: 354 RVA: 0x00006A0D File Offset: 0x00004C0D
			public string ReqDateStr { get; set; }

			// Token: 0x17000012 RID: 18
			// (get) Token: 0x06000163 RID: 355 RVA: 0x00006A16 File Offset: 0x00004C16
			// (set) Token: 0x06000164 RID: 356 RVA: 0x00006A1E File Offset: 0x00004C1E
			public string TodayStr { get; private set; }

			// Token: 0x17000013 RID: 19
			// (get) Token: 0x06000165 RID: 357 RVA: 0x00006A27 File Offset: 0x00004C27
			// (set) Token: 0x06000166 RID: 358 RVA: 0x00006A2F File Offset: 0x00004C2F
			public string NowStr { get; private set; }

			// Token: 0x06000167 RID: 359 RVA: 0x00006A38 File Offset: 0x00004C38
			public void SetTimeRange(DateTime startDT, DateTime endDT)
			{
				this.StartTime = ScadaUtils.EncodeDateTime(startDT);
				this.EndTime = ScadaUtils.EncodeDateTime(endDT);
				this.StartDateStr = startDT.ToString("yyyy-MM-dd");
			}

			// Token: 0x040000D6 RID: 214
			public const string DateFormat = "yyyy-MM-dd";

			// Token: 0x040000D7 RID: 215
			[CompilerGenerated]
			private double[] a;

			// Token: 0x040000D8 RID: 216
			[CompilerGenerated]
			private ChartSvc.TrendData[] b;

			// Token: 0x040000D9 RID: 217
			[CompilerGenerated]
			private double c;

			// Token: 0x040000DA RID: 218
			[CompilerGenerated]
			private double d;

			// Token: 0x040000DB RID: 219
			[CompilerGenerated]
			private string e;

			// Token: 0x040000DC RID: 220
			[CompilerGenerated]
			private string f;

			// Token: 0x040000DD RID: 221
			[CompilerGenerated]
			private string g;

			// Token: 0x040000DE RID: 222
			[CompilerGenerated]
			private string h;
		}
	}
}
