using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Scada.UI;
using Scada.Web.Plugins.Chart;

namespace Scada.Web.Plugins.ChartPro
{
	// Token: 0x02000021 RID: 33
	public class WFrmChartCnls : Page
	{
		// Token: 0x0600012C RID: 300 RVA: 0x000056BC File Offset: 0x000038BC
		private void a(int[] A_0, int[] A_1)
		{
			this.c = new List<CnlViewPair>();
			this.selCnlSet = new HashSet<int>();
			int i = 0;
			int num = Math.Min(A_0.Length, A_1.Length);
			while (i < num)
			{
				int num2 = A_0[i];
				int num3 = A_1[i];
				CnlViewPair cnlViewPair = new CnlViewPair(num2, num3);
				cnlViewPair.FillInfo(this.a.DataAccess.GetCnlProps(num2), this.b.UserViews);
				this.c.Add(cnlViewPair);
				this.selCnlSet.Add(num2);
				i++;
			}
		}

		// Token: 0x0600012D RID: 301 RVA: 0x00005748 File Offset: 0x00003948
		private void b()
		{
			this.repSelCnls.DataSource = this.c;
			this.repSelCnls.DataBind();
			this.btnSubmit.Enabled = (this.c.Count > 0);
			this.pnlPerfWarn.Visible = (this.c.Count > 10);
			this.lblNoSelCnls.Visible = !this.btnSubmit.Enabled;
		}

		// Token: 0x0600012E RID: 302 RVA: 0x000057C0 File Offset: 0x000039C0
		private void a()
		{
			int num;
			int.TryParse(this.ddlView.SelectedValue, out num);
			List<CnlViewPair> cnlViewPairsByView = ChartUtils.GetCnlViewPairsByView(num, this.a.DataAccess, this.b.UserViews);
			this.repCnlsByView.DataSource = cnlViewPairsByView;
			this.repCnlsByView.DataBind();
			this.lblUnableLoadView.Visible = (cnlViewPairsByView == null);
			this.lblNoCnlsByView.Visible = (cnlViewPairsByView != null && cnlViewPairsByView.Count == 0);
		}

		// Token: 0x0600012F RID: 303 RVA: 0x00005840 File Offset: 0x00003A40
		protected void Page_Load(object sender, EventArgs e)
		{
			this.a = AppData.GetAppData();
			this.b = UserData.GetUserData();
			if (!this.b.LoggedOn)
			{
				throw new ScadaException(CommonPhrases.NoRights);
			}
			if (base.IsPostBack)
			{
				this.c = (List<CnlViewPair>)this.ViewState["SelCnls"];
				this.selCnlSet = ChartUtils.GetCnlSet(this.c);
				return;
			}
			Translator.TranslatePage(this.Page, "Scada.Web.Plugins.ChartPro.WFrmChartCnls");
			this.lblPerfWarn.Text = ChartPhrases.PerfWarning;
			int[] paramAsIntArray = WebUtils.GetParamAsIntArray(base.Request.QueryString, "cnlNums");
			int[] paramAsIntArray2 = WebUtils.GetParamAsIntArray(base.Request.QueryString, "viewIDs");
			this.a(paramAsIntArray, paramAsIntArray2);
			this.ViewState.Add("SelCnls", this.c);
			this.b();
		}

		// Token: 0x06000130 RID: 304 RVA: 0x00005924 File Offset: 0x00003B24
		protected void btnSubmit_Click(object sender, EventArgs e)
		{
			string arg;
			string arg2;
			ChartUtils.GetSelection(this.c, ref arg, ref arg2);
			base.ClientScript.RegisterStartupScript(base.GetType(), "CloseModalScript", string.Format("closeModal('{0}', '{1}');", arg, arg2), true);
		}

		// Token: 0x06000131 RID: 305 RVA: 0x00005964 File Offset: 0x00003B64
		protected void btnAddCnls_Click(object sender, EventArgs e)
		{
			if (this.ddlView.Items.Count == 0)
			{
				int num = (this.c.Count > 0) ? this.c[this.c.Count - 1].ViewID : 0;
				ChartUtils.FillViewList(this.ddlView, num, this.b.UserViews);
				this.a();
			}
			this.mvCnls.SetActiveView(this.viewAddCnls);
			ChartUtils.AddUpdateModalHeightScript(this);
		}

		// Token: 0x06000132 RID: 306 RVA: 0x000059E8 File Offset: 0x00003BE8
		protected void repSelCnls_ItemCommand(object source, RepeaterCommandEventArgs e)
		{
			if (e.CommandName == "RemoveCnl")
			{
				int num = int.Parse((string)e.CommandArgument);
				if (num < this.c.Count)
				{
					this.c.RemoveAt(num);
					this.ViewState.Add("SelCnls", this.c);
					this.b();
					ChartUtils.AddUpdateModalHeightScript(this);
				}
			}
		}

		// Token: 0x06000133 RID: 307 RVA: 0x00005A55 File Offset: 0x00003C55
		protected void lbtnBackToCnls_Click(object sender, EventArgs e)
		{
			this.mvCnls.SetActiveView(this.viewSelCnls);
			this.b();
			ChartUtils.AddUpdateModalHeightScript(this);
		}

		// Token: 0x06000134 RID: 308 RVA: 0x00005A74 File Offset: 0x00003C74
		protected void ddlView_SelectedIndexChanged(object sender, EventArgs e)
		{
			this.a();
			ChartUtils.AddUpdateModalHeightScript(this);
		}

		// Token: 0x06000135 RID: 309 RVA: 0x00005A84 File Offset: 0x00003C84
		protected void repCnlsByView_ItemCommand(object source, RepeaterCommandEventArgs e)
		{
			if (e.CommandName == "AddCnl")
			{
				int num = int.Parse((string)e.CommandArgument);
				if (!this.selCnlSet.Contains(num))
				{
					int num2 = int.Parse(this.ddlView.SelectedValue);
					CnlViewPair cnlViewPair = new CnlViewPair(num, num2);
					cnlViewPair.FillInfo(this.a.DataAccess.GetCnlProps(num), this.b.UserViews);
					this.c.Add(cnlViewPair);
					this.ViewState.Add("SelCnls", this.c);
					this.btnSubmit.Enabled = true;
					this.pnlPerfWarn.Visible = (this.c.Count > 10);
				}
				((Label)e.Item.FindControl("lblCnlAdded")).Visible = true;
			}
		}

		// Token: 0x0400008C RID: 140
#pragma warning disable CS0102 // 类型“WFrmChartCnls”已经包含“a”的定义
#pragma warning disable CS0246 // 未能找到类型或命名空间名“AppData”(是否缺少 using 指令或程序集引用?)
		private AppData a;
#pragma warning restore CS0246 // 未能找到类型或命名空间名“AppData”(是否缺少 using 指令或程序集引用?)
#pragma warning restore CS0102 // 类型“WFrmChartCnls”已经包含“a”的定义

		// Token: 0x0400008D RID: 141
#pragma warning disable CS0102 // 类型“WFrmChartCnls”已经包含“b”的定义
#pragma warning disable CS0246 // 未能找到类型或命名空间名“UserData”(是否缺少 using 指令或程序集引用?)
		private UserData b;
#pragma warning restore CS0246 // 未能找到类型或命名空间名“UserData”(是否缺少 using 指令或程序集引用?)
#pragma warning restore CS0102 // 类型“WFrmChartCnls”已经包含“b”的定义

		// Token: 0x0400008E RID: 142
		private List<CnlViewPair> c;

		// Token: 0x0400008F RID: 143
		protected HashSet<int> selCnlSet;

		// Token: 0x04000090 RID: 144
		protected HtmlForm frmChartCnls;

		// Token: 0x04000091 RID: 145
		protected Button btnSubmit;

		// Token: 0x04000092 RID: 146
		protected Panel pnlPerfWarn;

		// Token: 0x04000093 RID: 147
		protected Label lblPerfWarn;

		// Token: 0x04000094 RID: 148
		protected MultiView mvCnls;

		// Token: 0x04000095 RID: 149
		protected View viewSelCnls;

		// Token: 0x04000096 RID: 150
		protected Button btnAddCnls;

		// Token: 0x04000097 RID: 151
		protected Label lblSelCnls;

		// Token: 0x04000098 RID: 152
		protected Panel pnlSelCnls;

		// Token: 0x04000099 RID: 153
		protected Repeater repSelCnls;

		// Token: 0x0400009A RID: 154
		protected Label lblNoSelCnls;

		// Token: 0x0400009B RID: 155
		protected View viewAddCnls;

		// Token: 0x0400009C RID: 156
		protected LinkButton lbtnBackToCnls;

		// Token: 0x0400009D RID: 157
		protected DropDownList ddlView;

		// Token: 0x0400009E RID: 158
		protected Panel pnlCnlsByView;

		// Token: 0x0400009F RID: 159
		protected Repeater repCnlsByView;

		// Token: 0x040000A0 RID: 160
		protected Label lblLoading;

		// Token: 0x040000A1 RID: 161
		protected Label lblUnableLoadView;

		// Token: 0x040000A2 RID: 162
		protected Label lblNoCnlsByView;
	}
}
