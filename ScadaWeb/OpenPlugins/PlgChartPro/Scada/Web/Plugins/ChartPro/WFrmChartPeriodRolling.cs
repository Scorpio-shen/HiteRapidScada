using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Scada.UI;

namespace Scada.Web.Plugins.ChartPro
{
	// Token: 0x02000024 RID: 36
	public class WFrmChartPeriodRolling : Page
	{
		// Token: 0x06000147 RID: 327 RVA: 0x00005FC4 File Offset: 0x000041C4
		protected void Page_Load(object sender, EventArgs e)
		{
			if (!UserData.GetUserData().LoggedOn)
			{
				throw new ScadaException(CommonPhrases.NoRights);
			}
			WebUtils.HideAlert(this.pnlErrMsg);
			if (!base.IsPostBack)
			{
				Translator.TranslatePage(this.Page, "Scada.Web.Plugins.ChartPro.WFrmChartPeriodRolling");
				int paramAsInt = WebUtils.GetParamAsInt(base.Request.QueryString, "period", 0);
				this.txtPeriod.Text = paramAsInt.ToString();
			}
		}

		// Token: 0x06000148 RID: 328 RVA: 0x00006034 File Offset: 0x00004234
		protected void btnSubmit_Click(object sender, EventArgs e)
		{
			int num;
			if (!int.TryParse(this.txtPeriod.Text, out num))
			{
				WebUtils.ShowAlert(this.pnlErrMsg, CommonPhrases.IntegerRequired);
				return;
			}
			if (num > 46080)
			{
				WebUtils.ShowAlert(this.pnlErrMsg, string.Format(WebPhrases.DayPeriodTooLong, 32));
				return;
			}
			base.ClientScript.RegisterStartupScript(base.GetType(), "Startup", string.Format("closeRollingPeriodModal({0});", num), true);
		}

		// Token: 0x040000BB RID: 187
		protected HtmlForm frmChartPeriodRolling;

		// Token: 0x040000BC RID: 188
		protected Button btnSubmit;

		// Token: 0x040000BD RID: 189
		protected Panel pnlErrMsg;

		// Token: 0x040000BE RID: 190
		protected Label lblErrMsg;

		// Token: 0x040000BF RID: 191
		protected Label lblPeriod;

		// Token: 0x040000C0 RID: 192
		protected TextBox txtPeriod;

		// Token: 0x040000C1 RID: 193
		protected Label lblPeriodUnit;
	}
}
