using System;
using System.Web;

namespace Utils.KeyGen
{
	// Token: 0x0200002B RID: 43
	public sealed class WebRegPublic
	{
		// Token: 0x06000168 RID: 360 RVA: 0x00006A64 File Offset: 0x00004C64
		private WebRegPublic()
		{
		}

		// Token: 0x06000169 RID: 361 RVA: 0x00006A6C File Offset: 0x00004C6C
		private WebRegPublic(IAppReg A_0, string A_1)
		{
			if (A_0 == null)
			{
				throw new ArgumentNullException("appReg");
			}
			if (A_1 == null)
			{
				throw new ArgumentNullException("configDir");
			}
			this.a = A_0;
			this.b = A_1;
		}

		// Token: 0x17000014 RID: 20
		// (get) Token: 0x0600016A RID: 362 RVA: 0x00006A9E File Offset: 0x00004C9E
		public string ProdName
		{
			get
			{
				return this.a.ProdName;
			}
		}

		// Token: 0x0600016B RID: 363 RVA: 0x00006AAC File Offset: 0x00004CAC
		public void GetInfo(out string regKey, out string compCode, out string statusStr, out string statusMsg)
		{
			HttpContext httpContext = HttpContext.Current;
			WebRegPublic.a(httpContext);
			m.e = httpContext.Request.PhysicalApplicationPath;
			statusStr = p.a(this.b, this.b, this.a, false, out regKey, out compCode, out statusMsg).ToString();
		}

		// Token: 0x0600016C RID: 364 RVA: 0x00006AFF File Offset: 0x00004CFF
		public bool SaveKey(string regKey, out string errMsg)
		{
			return p.a(this.b, this.a, regKey, out errMsg);
		}

		// Token: 0x0600016D RID: 365 RVA: 0x00006B14 File Offset: 0x00004D14
		internal static void a(HttpContext A_0)
		{
			if (A_0 == null)
			{
				throw new ArgumentNullException("httpContext", "HTTP context or its properties are undefined.");
			}
			if (A_0.Session == null)
			{
				throw new ArgumentNullException("httpContext.Session", "HTTP context or its properties are undefined.");
			}
			if (A_0.Request == null)
			{
				throw new ArgumentNullException("httpContext.Request", "HTTP context or its properties are undefined.");
			}
		}

		// Token: 0x0600016E RID: 366 RVA: 0x00006B64 File Offset: 0x00004D64
		public static void AddToSession(IAppReg appReg, string configDir)
		{
			HttpContext httpContext = HttpContext.Current;
			WebRegPublic.a(httpContext);
			WebRegPublic value = new WebRegPublic(appReg, configDir);
			httpContext.Session[appReg.ProdCode + "_WebRegPublic"] = value;
		}

		// Token: 0x040000E9 RID: 233
#pragma warning disable CS0102 // 类型“WebRegPublic”已经包含“a”的定义
		private IAppReg a;
#pragma warning restore CS0102 // 类型“WebRegPublic”已经包含“a”的定义

		// Token: 0x040000EA RID: 234
		private string b;
	}
}
