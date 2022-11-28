using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Scada.UI;

namespace Scada.Web.Plugins.ChartPro
{
	// Token: 0x02000023 RID: 35
	public class WFrmChartPeriod : Page
	{
		// Token: 0x0600013A RID: 314 RVA: 0x00005D03 File Offset: 0x00003F03
		private void a(DateTime A_0, DateTime A_1)
		{
			this.txtDateFrom.Text = Localization.ToLocalizedDateString(A_0);
			this.txtDateTo.Text = Localization.ToLocalizedDateString(A_1);
		}

		// Token: 0x0600013B RID: 315 RVA: 0x00005D28 File Offset: 0x00003F28
		private void b(int A_0)
		{
			DateTime dateTime;
			DateTime dateTime2;
			string text;
			if (RepUtils.ParseDates(this.txtDateFrom.Text, this.txtDateTo.Text, ref dateTime, ref dateTime2, ref text))
			{
				this.a(dateTime.AddDays((double)A_0), dateTime2.AddDays((double)A_0));
				return;
			}
			WebUtils.ShowAlert(this.pnlErrMsg, text);
		}

		// Token: 0x0600013C RID: 316 RVA: 0x00005D80 File Offset: 0x00003F80
		private void a(int A_0)
		{
			DateTime a_;
			DateTime a_2;
			string text;
			if (RepUtils.ParseDates(this.txtDateFrom.Text, this.txtDateTo.Text, ref a_, ref a_2, ref text))
			{
				this.a(this.a(a_, A_0), this.a(a_2, A_0));
				return;
			}
			WebUtils.ShowAlert(this.pnlErrMsg, text);
		}

		// Token: 0x0600013D RID: 317 RVA: 0x00005DD4 File Offset: 0x00003FD4
		private DateTime a(DateTime A_0, int A_1)
		{
			if (A_0.Day == DateTime.DaysInMonth(A_0.Year, A_0.Month))
			{
				A_0 = A_0.AddMonths(A_1);
				int num = DateTime.DaysInMonth(A_0.Year, A_0.Month) - A_0.Day;
				return A_0.AddDays((double)num);
			}
			return A_0.AddMonths(A_1);
		}

		// Token: 0x0600013E RID: 318 RVA: 0x00005E38 File Offset: 0x00004038
		protected void Page_Load(object sender, EventArgs e)
		{
			if (!UserData.GetUserData().LoggedOn)
			{
				throw new ScadaException(CommonPhrases.NoRights);
			}
			WebUtils.HideAlert(this.pnlErrMsg);
			if (!base.IsPostBack)
			{
				Translator.TranslatePage(this.Page, "Scada.Web.Plugins.ChartPro.WFrmChartPeriod");
				DateTime paramAsDate = WebUtils.GetParamAsDate(base.Request.QueryString, null, "year", "month", "day");
				int paramAsInt = WebUtils.GetParamAsInt(base.Request.QueryString, "period", 0);
				RepUtils.NormalizeTimeRange(ref paramAsDate, ref paramAsInt, false);
				this.a(paramAsDate, paramAsDate.AddDays((double)(paramAsInt - 1)));
			}
		}

		// Token: 0x0600013F RID: 319 RVA: 0x00005EDC File Offset: 0x000040DC
		protected void btnSubmit_Click(object sender, EventArgs e)
		{
			DateTime dateTime;
			DateTime dateTime2;
			string text;
			int num;
			if (RepUtils.ParseDates(this.txtDateFrom.Text, this.txtDateTo.Text, ref dateTime, ref dateTime2, ref text) && RepUtils.CheckDayPeriod(dateTime, dateTime2, ref num, ref text))
			{
				base.ClientScript.RegisterStartupScript(base.GetType(), "Startup", string.Format("closeFixedPeriodModal({0}, {1}, {2}, {3});", new object[]
				{
					dateTime.Year,
					dateTime.Month,
					dateTime.Day,
					num
				}), true);
				return;
			}
			WebUtils.ShowAlert(this.pnlErrMsg, text);
		}

		// Token: 0x06000140 RID: 320 RVA: 0x00005F83 File Offset: 0x00004183
		protected void btnPrevDay_Click(object sender, EventArgs e)
		{
			this.b(-1);
		}

		// Token: 0x06000141 RID: 321 RVA: 0x00005F8C File Offset: 0x0000418C
		protected void btnNextDay_Click(object sender, EventArgs e)
		{
			this.b(1);
		}

		// Token: 0x06000142 RID: 322 RVA: 0x00005F95 File Offset: 0x00004195
		protected void btnPrevWeek_Click(object sender, EventArgs e)
		{
			this.b(-7);
		}

		// Token: 0x06000143 RID: 323 RVA: 0x00005F9F File Offset: 0x0000419F
		protected void btnNextWeek_Click(object sender, EventArgs e)
		{
			this.b(7);
		}

		// Token: 0x06000144 RID: 324 RVA: 0x00005FA8 File Offset: 0x000041A8
		protected void btnPrevMonth_Click(object sender, EventArgs e)
		{
			this.a(-1);
		}

		// Token: 0x06000145 RID: 325 RVA: 0x00005FB1 File Offset: 0x000041B1
		protected void btnNextMonth_Click(object sender, EventArgs e)
		{
			this.a(1);
		}

		// Token: 0x040000AD RID: 173
		protected HtmlForm frmChartPeriod;

		// Token: 0x040000AE RID: 174
		protected Button btnSubmit;

		// Token: 0x040000AF RID: 175
		protected Panel pnlErrMsg;

		// Token: 0x040000B0 RID: 176
		protected Label lblErrMsg;

		// Token: 0x040000B1 RID: 177
		protected Label lblDateFrom;

		// Token: 0x040000B2 RID: 178
		protected TextBox txtDateFrom;

		// Token: 0x040000B3 RID: 179
		protected Label lblDateTo;

		// Token: 0x040000B4 RID: 180
		protected TextBox txtDateTo;

		// Token: 0x040000B5 RID: 181
		protected Button btnPrevDay;

		// Token: 0x040000B6 RID: 182
		protected Button btnNextDay;

		// Token: 0x040000B7 RID: 183
		protected Button btnPrevWeek;

		// Token: 0x040000B8 RID: 184
		protected Button btnNextWeek;

		// Token: 0x040000B9 RID: 185
		protected Button btnPrevMonth;

		// Token: 0x040000BA RID: 186
		protected Button btnNextMonth;
	}
}
