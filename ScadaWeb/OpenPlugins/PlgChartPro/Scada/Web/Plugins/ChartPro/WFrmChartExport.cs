using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Scada.UI;

namespace Scada.Web.Plugins.ChartPro
{
	// Token: 0x02000022 RID: 34
	public class WFrmChartExport : Page
	{
		// Token: 0x06000137 RID: 311 RVA: 0x00005B70 File Offset: 0x00003D70
		protected void Page_Load(object sender, EventArgs e)
		{
			if (!UserData.GetUserData().LoggedOn)
			{
				throw new ScadaException(CommonPhrases.NoRights);
			}
			WebUtils.HideAlert(this.pnlErrMsg);
			if (!base.IsPostBack)
			{
				Translator.TranslatePage(this.Page, "Scada.Web.Plugins.ChartPro.WFrmChartExport");
				DateTime paramAsDate = WebUtils.GetParamAsDate(base.Request.QueryString, null, "year", "month", "day");
				int num = (WebUtils.GetParamAsEnum<ChartMode>(base.Request.QueryString, "mode", ChartMode.Fixed) == ChartMode.Fixed) ? WebUtils.GetParamAsInt(base.Request.QueryString, "period", 0) : 1;
				RepUtils.NormalizeTimeRange(ref paramAsDate, ref num, false);
				this.txtDateFrom.Text = Localization.ToLocalizedDateString(paramAsDate);
				this.txtDateTo.Text = Localization.ToLocalizedDateString(paramAsDate.AddDays((double)(num - 1)));
			}
		}

		// Token: 0x06000138 RID: 312 RVA: 0x00005C4C File Offset: 0x00003E4C
		protected void btnExportExcel_Click(object sender, EventArgs e)
		{
			DateTime dateTime;
			DateTime dateTime2;
			string text;
			int num;
			if (RepUtils.ParseDates(this.txtDateFrom.Text, this.txtDateTo.Text, ref dateTime, ref dateTime2, ref text) && RepUtils.CheckDayPeriod(dateTime, dateTime2, ref num, ref text))
			{
				base.ClientScript.RegisterStartupScript(base.GetType(), "Startup", string.Format("closeModal({0}, {1}, {2}, {3}, {4});", new object[]
				{
					"'xml'",
					dateTime.Year,
					dateTime.Month,
					dateTime.Day,
					num
				}), true);
				return;
			}
			WebUtils.ShowAlert(this.pnlErrMsg, text);
		}

		// Token: 0x040000A3 RID: 163
		protected HtmlForm frmChartExport;

		// Token: 0x040000A4 RID: 164
		protected Panel pnlErrMsg;

		// Token: 0x040000A5 RID: 165
		protected Label lblErrMsg;

		// Token: 0x040000A6 RID: 166
		protected Button btnExportPdf;

		// Token: 0x040000A7 RID: 167
		protected Button btnExportPng;

		// Token: 0x040000A8 RID: 168
		protected Button btnExportExcel;

		// Token: 0x040000A9 RID: 169
		protected Label lblDateFrom;

		// Token: 0x040000AA RID: 170
		protected TextBox txtDateFrom;

		// Token: 0x040000AB RID: 171
		protected Label lblDateTo;

		// Token: 0x040000AC RID: 172
		protected TextBox txtDateTo;
	}
}
