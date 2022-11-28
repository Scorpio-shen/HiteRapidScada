using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Scada.Server.Modules.Properties
{
	// Token: 0x02000009 RID: 9
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
	[DebuggerNonUserCode]
	[CompilerGenerated]
	internal class Resources
	{
		// Token: 0x0600002C RID: 44 RVA: 0x000020FF File Offset: 0x000002FF
		internal Resources()
		{
		}

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x0600002D RID: 45 RVA: 0x000032AC File Offset: 0x000014AC
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static ResourceManager ResourceManager
		{
			get
			{
				if (Resources.resourceMan == null)
				{
					Resources.resourceMan = new ResourceManager("Scada.Server.Modules.Properties.Resources", typeof(Resources).Assembly);
				}
				return Resources.resourceMan;
			}
		}

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x0600002E RID: 46 RVA: 0x000032D8 File Offset: 0x000014D8
		// (set) Token: 0x0600002F RID: 47 RVA: 0x000032DF File Offset: 0x000014DF
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static CultureInfo Culture
		{
			get
			{
				return Resources.resourceCulture;
			}
			set
			{
				Resources.resourceCulture = value;
			}
		}

		// Token: 0x0400002E RID: 46
		private static ResourceManager resourceMan;

		// Token: 0x0400002F RID: 47
		private static CultureInfo resourceCulture;
	}
}
