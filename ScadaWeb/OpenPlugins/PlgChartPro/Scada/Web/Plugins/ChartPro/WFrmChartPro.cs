using System;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Scada.Client;
using Scada.UI;
using Scada.Web.Plugins.Chart;

namespace Scada.Web.Plugins.ChartPro
{
	// Token: 0x02000025 RID: 37
	public class WFrmChartPro : Page
	{
		// Token: 0x0600014A RID: 330 RVA: 0x000060BC File Offset: 0x000042BC
		protected void Page_Load(object sender, EventArgs e)
		{
			AppData appData = AppData.GetAppData();
			UserData userData = UserData.GetUserData();
			b b = b.a();
			if (!userData.LoggedOn)
			{
				throw new ScadaException(CommonPhrases.NoRights);
			}
			q.a(new c(), appData.AppDirs.ConfigDir, appData.Log, false, true, "~/plugins/Registration/RegistrationNM.aspx?prodCode=PlgChartPro");
			Translator.TranslatePage(this.Page, "Scada.Web.Plugins.ChartPro.WFrmChartPro");
			string text = base.Request.QueryString["cnlNums"];
			int[] array = ScadaUtils.ParseIntArray(text);
			int[] paramAsIntArray = WebUtils.GetParamAsIntArray(base.Request.QueryString, "viewIDs");
			DateTime dateTime = WebUtils.GetParamAsDate(base.Request.QueryString, null, "year", "month", "day");
			ChartMode paramAsEnum = WebUtils.GetParamAsEnum<ChartMode>(base.Request.QueryString, "mode", ChartMode.Fixed);
			int num = WebUtils.GetParamAsInt(base.Request.QueryString, "period", 0);
			string text2 = base.Request.QueryString["title"];
			string a_ = base.Request.QueryString["config"];
			int num2;
			if (!userData.UserRights.CheckInCnlRights(array, paramAsIntArray, ref num2))
			{
				throw new ScadaException(CommonPhrases.NoRights);
			}
			BaseView view = userData.UserViews.GetView(num2, true, true);
			DateTime now = DateTime.Now;
			DateTime date = now.Date;
			ChartDataBuilder chartDataBuilder;
			if (paramAsEnum == ChartMode.Fixed)
			{
				chartDataBuilder = new ChartDataBuilder(array, dateTime, num, appData.DataAccess);
				chartDataBuilder.FillCnlProps();
				chartDataBuilder.FillData();
			}
			else
			{
				num = ((num == 0) ? 60 : Math.Abs(num));
				dateTime = now.AddMinutes((double)(-(double)num)).Date;
				int num3 = (int)(date - dateTime).TotalDays + 1;
				chartDataBuilder = new ChartDataBuilder(array, dateTime, num3, appData.DataAccess);
				chartDataBuilder.FillCnlProps();
			}
			if (!string.IsNullOrEmpty(text))
			{
				base.Title = text + " - " + base.Title;
			}
			string text3 = string.IsNullOrEmpty(text2) ? ((view == null) ? "" : view.Title) : text2;
			string arg = global::a.b() + Localization.ToLocalizedString(DateTime.Now);
			if (paramAsEnum == ChartMode.Fixed)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append(string.IsNullOrEmpty(text3) ? "" : ", ").Append(Localization.ToLocalizedDateString(chartDataBuilder.StartDate));
				if (chartDataBuilder.Period > 1)
				{
					stringBuilder.Append(" - ").Append(Localization.ToLocalizedDateString(chartDataBuilder.EndDate));
				}
				text3 += stringBuilder.ToString();
			}
			i i;
			string text4;
			bool flag = !b.a(a_, out i, out text4);
			i.b().a(Localization.Culture.Name);
			i.b().a(userData.WebSettings.ChartGap);
			i.b().b(paramAsEnum == ChartMode.Fixed);
			if (flag)
			{
				arg = text4.Replace("\\", "\\\\");
				if (i.b().h().Count == 0)
				{
					i.b().h().Add(new k());
				}
			}
			this.sbClientScript = new StringBuilder();
			i.b().a(this.sbClientScript);
			i.c().a(this.sbClientScript);
			chartDataBuilder.ToJs(this.sbClientScript);
			this.sbClientScript.AppendFormat("var chartMode = {0};", (int)paramAsEnum).AppendLine().AppendFormat("var period = {0};", num).AppendLine().AppendFormat("var startDateStr = '{0}';", chartDataBuilder.StartDate.ToString("yyyy-MM-dd")).AppendLine().AppendFormat("var endDateStr = '{0}';", chartDataBuilder.EndDate.ToString("yyyy-MM-dd")).AppendLine().AppendFormat("var todayStr = '{0}';", date.ToString("yyyy-MM-dd")).AppendLine().AppendFormat("var chartTitle = '{0}';", HttpUtility.JavaScriptStringEncode(text3)).AppendLine().AppendFormat("var chartStatus = '{0}';", arg).AppendLine().AppendFormat("var chartError = {0};", flag ? "true" : "false").AppendLine().AppendLine();
		}

		// Token: 0x040000C2 RID: 194
		private const int a = 60;

		// Token: 0x040000C3 RID: 195
		protected StringBuilder sbClientScript;

		// Token: 0x040000C4 RID: 196
		protected HtmlForm frmChartPro;

		// Token: 0x040000C5 RID: 197
		protected HyperLink hlSelectCnls;

		// Token: 0x040000C6 RID: 198
		protected HyperLink hlChangePeriod;

		// Token: 0x040000C7 RID: 199
		protected HyperLink hlShowData;

		// Token: 0x040000C8 RID: 200
		protected HyperLink hlHideData;

		// Token: 0x040000C9 RID: 201
		protected HyperLink hlExport;

		// Token: 0x040000CA RID: 202
		protected Button btnClose;

		// Token: 0x040000CB RID: 203
		protected Label lblNoData;
	}
}
