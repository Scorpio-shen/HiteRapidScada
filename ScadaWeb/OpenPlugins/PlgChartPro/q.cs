using System;
using System.Web;
using Utils;
using Utils.KeyGen;

// Token: 0x02000019 RID: 25
internal static class q
{
	// Token: 0x06000126 RID: 294 RVA: 0x000055C0 File Offset: 0x000037C0
#pragma warning disable CS0102 // 类型“q”已经包含“a”的定义
	public static bool a(IAppReg A_0, string A_1, ILog A_2, bool A_3 = true, bool A_4 = true, string A_5 = "")
#pragma warning restore CS0102 // 类型“q”已经包含“a”的定义
	{
		HttpContext httpContext = HttpContext.Current;
		WebRegPublic.a(httpContext);
		string name = A_0.ProdCode + "_WebReg.RegOK";
		if (httpContext.Session[name] is q.a)
		{
			return true;
		}
		m.e = httpContext.Request.PhysicalApplicationPath;
		string text;
		bool flag = p.a(A_1, A_1, A_0, false, out text);
		if (A_2 != null)
		{
			A_2.WriteAction(text);
		}
		if (flag)
		{
			httpContext.Session[name] = new q.a();
			return flag;
		}
		if (A_3 || (A_4 && string.IsNullOrEmpty(A_5)))
		{
			throw new Exception(n.c());
		}
		if (A_4)
		{
			httpContext.Response.Redirect(A_5);
		}
		return flag;
	}

	// Token: 0x06000127 RID: 295 RVA: 0x00005665 File Offset: 0x00003865
#pragma warning disable CS0102 // 类型“q”已经包含“a”的定义
	public static bool a(IAppReg A_0, string A_1, ILog A_2, out string A_3)
#pragma warning restore CS0102 // 类型“q”已经包含“a”的定义
	{
		if (q.a(A_0, A_1, A_2, false, false, ""))
		{
			A_3 = "";
			return true;
		}
		A_3 = string.Format(n.b(), A_0.ProdName);
		return false;
	}

	// Token: 0x0200001A RID: 26
	private class a
	{
	}
}
