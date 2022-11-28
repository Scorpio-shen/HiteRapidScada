using System;

namespace Utils.KeyGen
{
	// Token: 0x02000007 RID: 7
	public interface IAppReg
	{
		// Token: 0x17000008 RID: 8
		// (get) Token: 0x06000018 RID: 24
		string ProdCode { get; }

		// Token: 0x17000009 RID: 9
		// (get) Token: 0x06000019 RID: 25
		string ProdName { get; }

		// Token: 0x1700000A RID: 10
		// (get) Token: 0x0600001A RID: 26
		string KeyFileName { get; }
	}
}
