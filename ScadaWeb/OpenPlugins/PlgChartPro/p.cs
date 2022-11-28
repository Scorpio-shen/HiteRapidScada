using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using Utils.KeyGen;

// Token: 0x02000015 RID: 21
internal sealed class p
{
	// Token: 0x06000104 RID: 260 RVA: 0x00004B90 File Offset: 0x00002D90
#pragma warning disable CS0102 // 类型“p”已经包含“b”的定义
	private void b(byte[] A_0)
#pragma warning restore CS0102 // 类型“p”已经包含“b”的定义
	{
		int i = 0;
		int num = A_0.Length;
		while (i < num)
		{
			byte b = A_0[i];
			A_0[i] = (byte)((int)(b & 51) << 2 | (b & 204) >> 2);
			i++;
		}
	}

	// Token: 0x06000105 RID: 261 RVA: 0x00004BC8 File Offset: 0x00002DC8
#pragma warning disable CS0102 // 类型“p”已经包含“a”的定义
	private void a(byte[] A_0, byte[] A_1)
#pragma warning restore CS0102 // 类型“p”已经包含“a”的定义
	{
		int i = 0;
		int num = Math.Min(A_0.Length, A_1.Length);
		while (i < num)
		{
			int num2 = i;
			A_0[num2] ^= A_1[i];
			i++;
		}
	}

	// Token: 0x06000106 RID: 262 RVA: 0x00004BFC File Offset: 0x00002DFC
#pragma warning disable CS0102 // 类型“p”已经包含“a”的定义
	private byte[] a(byte[] A_0)
#pragma warning restore CS0102 // 类型“p”已经包含“a”的定义
	{
		return MD5.Create().ComputeHash(A_0);
	}

	// Token: 0x06000107 RID: 263 RVA: 0x00004C09 File Offset: 0x00002E09
#pragma warning disable CS0102 // 类型“p”已经包含“a”的定义
	private byte[] a(string A_0)
#pragma warning restore CS0102 // 类型“p”已经包含“a”的定义
	{
		return this.a(Encoding.UTF8.GetBytes(A_0));
	}

	// Token: 0x06000108 RID: 264 RVA: 0x00004C1C File Offset: 0x00002E1C
#pragma warning disable CS0102 // 类型“p”已经包含“a”的定义
	private void a(ref string A_0, p.b A_1)
#pragma warning restore CS0102 // 类型“p”已经包含“a”的定义
	{
		p.a a = A_1.d();
		if (a != p.a.f || A_1.g())
		{
			m m = new m();
			m.a a2;
			string text;
			if (m.a(A_0, out a2, out text))
			{
				if (!a.HasFlag(p.a.b))
				{
					a2.b("");
				}
				if (!a.HasFlag(p.a.c))
				{
					a2.c("");
				}
				if (!a.HasFlag(p.a.d))
				{
					a2.d("");
				}
				if (!a.HasFlag(p.a.e))
				{
					a2.e("");
				}
				if (A_1.g())
				{
					string[] array = a2.c().Split(new char[]
					{
						';'
					});
					for (int i = 0; i < array.Length; i++)
					{
						string a_ = array[i].Trim();
						if (A_1.e() == p.b.a(a_))
						{
							a2.d(a_);
							break;
						}
					}
				}
				A_0 = m.a(a2);
			}
		}
	}

	// Token: 0x06000109 RID: 265 RVA: 0x00004D30 File Offset: 0x00002F30
#pragma warning disable CS0102 // 类型“p”已经包含“a”的定义
	public string a(string A_0, string A_1, p.b A_2)
#pragma warning restore CS0102 // 类型“p”已经包含“a”的定义
	{
		string result;
		try
		{
			this.a(ref A_0, A_2);
			byte[] array = A_2.j();
			byte[] second = this.a(A_0 + A_1 + "a22MdzRgRD");
			byte[] a_ = array.Concat(second).ToArray<byte>();
			byte[] array2 = this.a(a_);
			this.a(array, array2);
			this.b(array);
			string str = o.a(array);
			string text = o.a(array2);
			result = str + "-" + text.Insert(16, "-");
		}
		catch (Exception e)
		{
			result = "00000000-" + Marshal.GetHRForException(e).ToString("X8");
		}
		return result;
	}

	// Token: 0x0600010A RID: 266 RVA: 0x00004DE8 File Offset: 0x00002FE8
#pragma warning disable CS0102 // 类型“p”已经包含“a”的定义
	public string a(string A_0, string A_1, DateTime A_2)
#pragma warning restore CS0102 // 类型“p”已经包含“a”的定义
	{
		p.b a_ = new p.b(A_2, p.a.f);
		return this.a(A_0, A_1, a_);
	}

	// Token: 0x0600010B RID: 267 RVA: 0x00004E07 File Offset: 0x00003007
#pragma warning disable CS0102 // 类型“p”已经包含“a”的定义
	public string a(string A_0, string A_1)
#pragma warning restore CS0102 // 类型“p”已经包含“a”的定义
	{
		return this.a(A_0, A_1, p.b.b());
	}

	// Token: 0x0600010C RID: 268 RVA: 0x00004E18 File Offset: 0x00003018
#pragma warning disable CS0102 // 类型“p”已经包含“a”的定义
	public p.KeyStatuses a(string A_0, string A_1, string A_2, DateTime A_3, out string A_4)
#pragma warning restore CS0102 // 类型“p”已经包含“a”的定义
	{
		if (m.a(A_1))
		{
			A_4 = n.o();
			return p.KeyStatuses.Valid;
		}
		if (string.IsNullOrEmpty(A_0))
		{
			A_4 = n.m();
			return p.KeyStatuses.Valid;
		}
		if (A_0.StartsWith("00000000-", StringComparison.CurrentCultureIgnoreCase))
		{
			A_4 = n.l();
			return p.KeyStatuses.Valid;
		}
		p.b b;
		string text;
		if (!this.a(A_0, out b, out text))
		{
			A_4 = n.k();
			return p.KeyStatuses.Valid;
		}
		string text2 = this.a(A_1, A_2, b);
		if (!(A_0 == text2))
		{
			A_4 = n.o();
			return p.KeyStatuses.Valid;
		}
		if (!b.f())
		{
			A_4 = n.q();
			return p.KeyStatuses.Valid;
		}
		if (A_3.Date <= b.c())
		{
			A_4 = string.Format(n.p(), b.c().ToString("d", n.y()));
			return p.KeyStatuses.Valid;
		}
		A_4 = string.Format(n.n(), b.c().ToString("d", n.y()));
		return p.KeyStatuses.Valid;
	}

	// Token: 0x0600010D RID: 269 RVA: 0x00004F0C File Offset: 0x0000310C
#pragma warning disable CS0102 // 类型“p”已经包含“a”的定义
	public bool a(string A_0, out p.b A_1, out string A_2)
#pragma warning restore CS0102 // 类型“p”已经包含“a”的定义
	{
		bool result;
		try
		{
			A_0 = (A_0 ?? "").Replace("-", "").Trim();
			if (A_0.Length < 16)
			{
				throw new FormatException(n.s());
			}
			string a_ = A_0.Substring(0, 16);
			string a_2 = A_0.Substring(16);
			byte[] a_3 = o.a(a_);
			byte[] a_4 = o.a(a_2);
			this.b(a_3);
			this.a(a_3, a_4);
			A_1 = p.b.a(a_3);
			A_2 = "";
			result = true;
		}
		catch (Exception ex)
		{
			A_1 = p.b.a();
			A_2 = n.t() + ": " + ex.Message;
			result = false;
		}
		return result;
	}

	// Token: 0x0600010E RID: 270 RVA: 0x00004FC8 File Offset: 0x000031C8
#pragma warning disable CS0102 // 类型“p”已经包含“a”的定义
	public bool a(string A_0, out string A_1, out string A_2)
#pragma warning restore CS0102 // 类型“p”已经包含“a”的定义
	{
		bool result;
		try
		{
			if (!File.Exists(A_0))
			{
				throw new FileNotFoundException(string.Format(n.j(), A_0));
			}
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(A_0);
			A_1 = ((xmlDocument.DocumentElement.Name == "RegKey") ? xmlDocument.DocumentElement.InnerText.Trim() : "");
			A_2 = "";
			result = true;
		}
		catch (Exception ex)
		{
			A_1 = "";
			A_2 = n.g() + ": " + ex.Message;
			result = false;
		}
		return result;
	}

	// Token: 0x0600010F RID: 271 RVA: 0x0000506C File Offset: 0x0000326C
#pragma warning disable CS0102 // 类型“p”已经包含“a”的定义
	public bool a(string A_0, string A_1, out string A_2)
#pragma warning restore CS0102 // 类型“p”已经包含“a”的定义
	{
		bool result;
		try
		{
			XmlDocument xmlDocument = new XmlDocument();
			XmlDeclaration newChild = xmlDocument.CreateXmlDeclaration("1.0", "utf-8", null);
			xmlDocument.AppendChild(newChild);
			XmlElement xmlElement = xmlDocument.CreateElement("RegKey");
			xmlDocument.AppendChild(xmlElement);
			xmlElement.InnerText = (A_1 ?? "").Trim();
			xmlDocument.Save(A_0);
			A_2 = "";
			result = true;
		}
		catch (Exception ex)
		{
			A_2 = n.f() + ": " + ex.Message;
			result = false;
		}
		return result;
	}

	// Token: 0x06000110 RID: 272 RVA: 0x00005100 File Offset: 0x00003300
#pragma warning disable CS0102 // 类型“p”已经包含“a”的定义
	public static bool a(string A_0, string A_1, IAppReg A_2, bool A_3, out string A_4)
#pragma warning restore CS0102 // 类型“p”已经包含“a”的定义
	{
		if (A_0 == null)
		{
			throw new ArgumentNullException("keyDir");
		}
		if (A_1 == null)
		{
			throw new ArgumentNullException("compCodeDir");
		}
		if (A_2 == null)
		{
			throw new ArgumentNullException("appReg");
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat(n.e(), A_2.ProdName).AppendLine();
		m m = new m();
		string text = m.b();
		p p = new p();
		p.KeyStatuses keyStatuses = p.KeyStatuses.NotValid;
		string a_;
		string value;
		if (p.a(Path.Combine(A_0, A_2.KeyFileName), out a_, out value))
		{
			string value2;
			keyStatuses = p.a(a_, text, A_2.ProdCode, DateTime.Today, out value2);
			stringBuilder.AppendLine(value2);
		}
		else
		{
			stringBuilder.AppendLine(value);
		}
		if (keyStatuses != p.KeyStatuses.Valid)
		{
			stringBuilder.AppendFormat(n.d(), text).AppendLine();
		}
		if (A_3 && !m.a(Path.Combine(A_1, "CompCode.txt"), text, out value))
		{
			stringBuilder.AppendLine(value);
		}
		A_4 = stringBuilder.ToString().TrimEnd(Array.Empty<char>());
		return p.KeyStatuses.ValidStatuses.HasFlag(keyStatuses);
	}

	// Token: 0x06000111 RID: 273 RVA: 0x0000520C File Offset: 0x0000340C
#pragma warning disable CS0102 // 类型“p”已经包含“a”的定义
	public static p.KeyStatuses a(string A_0, string A_1, IAppReg A_2, bool A_3, out string A_4, out string A_5, out string A_6)
#pragma warning restore CS0102 // 类型“p”已经包含“a”的定义
	{
		if (A_0 == null)
		{
			throw new ArgumentNullException("keyDir");
		}
		if (A_1 == null)
		{
			throw new ArgumentNullException("compCodeDir");
		}
		if (A_2 == null)
		{
			throw new ArgumentNullException("appReg");
		}
		m m = new m();
		string text;
		if (A_3)
		{
			m.a(Path.Combine(A_1, "CompCode.txt"), out A_5, out text);
		}
		else
		{
			A_5 = m.b();
		}
		p p = new p();
		p.a(Path.Combine(A_0, A_2.KeyFileName), out A_4, out text);
		return p.a(A_4, A_5, A_2.ProdCode, DateTime.Today, out A_6);
	}

	// Token: 0x06000112 RID: 274 RVA: 0x000052A1 File Offset: 0x000034A1
#pragma warning disable CS0102 // 类型“p”已经包含“a”的定义
	public static p.KeyStatuses a(string A_0, string A_1, IAppReg A_2, out string A_3)
#pragma warning restore CS0102 // 类型“p”已经包含“a”的定义
	{
		return new p().a(A_0, A_1, A_2.ProdCode, DateTime.Today, out A_3);
	}

	// Token: 0x06000113 RID: 275 RVA: 0x000052BB File Offset: 0x000034BB
#pragma warning disable CS0102 // 类型“p”已经包含“a”的定义
	public static bool a(string A_0, IAppReg A_1, string A_2, out string A_3)
#pragma warning restore CS0102 // 类型“p”已经包含“a”的定义
	{
		if (A_0 == null)
		{
			throw new ArgumentNullException("keyDir");
		}
		if (A_1 == null)
		{
			throw new ArgumentNullException("appReg");
		}
		return new p().a(Path.Combine(A_0, A_1.KeyFileName), A_2, out A_3);
	}

	// Token: 0x0400006F RID: 111
#pragma warning disable CS0102 // 类型“p”已经包含“a”的定义
	private const string a = "a22MdzRgRD";
#pragma warning restore CS0102 // 类型“p”已经包含“a”的定义

	// Token: 0x04000070 RID: 112
#pragma warning disable CS0102 // 类型“p”已经包含“b”的定义
	private const string b = "00000000-";
#pragma warning restore CS0102 // 类型“p”已经包含“b”的定义

	// Token: 0x02000016 RID: 22
	[Flags]
	public enum KeyStatuses
	{
		// Token: 0x04000072 RID: 114
		Valid = 1,
		// Token: 0x04000073 RID: 115
		ValidWithWarning = 2,
		// Token: 0x04000074 RID: 116
		NotValid = 4,
		// Token: 0x04000075 RID: 117
		ValidStatuses = 3
	}

	// Token: 0x02000017 RID: 23
	[Flags]
	public enum a : byte
	{
		// Token: 0x04000077 RID: 119
		a = 0,
		// Token: 0x04000078 RID: 120
		b = 1,
		// Token: 0x04000079 RID: 121
		c = 2,
		// Token: 0x0400007A RID: 122
		d = 4,
		// Token: 0x0400007B RID: 123
		e = 8,
		// Token: 0x0400007C RID: 124
		f = 15,
		// Token: 0x0400007D RID: 125
		g = 161
	}

	// Token: 0x02000018 RID: 24
	public class b
	{
		// Token: 0x06000115 RID: 277 RVA: 0x000052F9 File Offset: 0x000034F9
		private b()
		{
			this.a(DateTime.MinValue);
			this.a(p.a.f);
			this.a(45779);
		}

		// Token: 0x06000116 RID: 278 RVA: 0x0000531F File Offset: 0x0000351F
		public b(DateTime A_0, p.a A_1)
		{
			this.a(A_0);
			this.a(A_1);
			this.a(45779);
		}

		// Token: 0x06000117 RID: 279 RVA: 0x00005340 File Offset: 0x00003540
		[CompilerGenerated]
		public DateTime c()
		{
			return this.c;
		}

		// Token: 0x06000118 RID: 280 RVA: 0x00005348 File Offset: 0x00003548
		[CompilerGenerated]
		public void a(DateTime A_0)
		{
			this.c = A_0;
		}

		// Token: 0x06000119 RID: 281 RVA: 0x00005351 File Offset: 0x00003551
		[CompilerGenerated]
		public p.a d()
		{
			return this.d;
		}

		// Token: 0x0600011A RID: 282 RVA: 0x00005359 File Offset: 0x00003559
		[CompilerGenerated]
		public void a(p.a A_0)
		{
			this.d = A_0;
		}

		// Token: 0x0600011B RID: 283 RVA: 0x00005362 File Offset: 0x00003562
		[CompilerGenerated]
		public ushort e()
		{
			return this.e;
		}

		// Token: 0x0600011C RID: 284 RVA: 0x0000536A File Offset: 0x0000356A
		[CompilerGenerated]
		public void a(ushort A_0)
		{
			this.e = A_0;
		}

		// Token: 0x0600011D RID: 285 RVA: 0x00005374 File Offset: 0x00003574
		public bool f()
		{
			return this.c() < DateTime.MaxValue.Date;
		}

		// Token: 0x0600011E RID: 286 RVA: 0x00005399 File Offset: 0x00003599
		public bool g()
		{
			return this.e() != 45779;
		}

		// Token: 0x0600011F RID: 287 RVA: 0x000053AB File Offset: 0x000035AB
		public bool h()
		{
			return !this.f() && this.d() == p.a.f && !this.g();
		}

		// Token: 0x06000120 RID: 288 RVA: 0x000053CA File Offset: 0x000035CA
		public p.b i()
		{
			return new p.b(this.c(), this.d());
		}

		// Token: 0x06000121 RID: 289 RVA: 0x000053E0 File Offset: 0x000035E0
		public byte[] j()
		{
			byte[] array = new byte[]
			{
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				196
			};
			array[0] = (byte)(this.c().Year % 256);
			array[1] = (byte)(this.c().Year / 256);
			array[2] = (byte)this.c().Month;
			array[3] = (byte)this.c().Day;
			array[4] = ((this.d() == p.a.f) ? p.a.g : this.d());
			ushort num = this.d().HasFlag(p.a.d) ? this.e() : 45779;
			array[5] = (byte)(num / 256);
			array[6] = (byte)num;
			return array;
		}

		// Token: 0x06000122 RID: 290 RVA: 0x000054A0 File Offset: 0x000036A0
		public static p.b a(byte[] A_0)
		{
			if (A_0 == null)
			{
				throw new ArgumentNullException("buffer");
			}
			if (A_0.Length != 8)
			{
				throw new FormatException(n.r());
			}
			p.b b = new p.b();
			p.b result;
			try
			{
				b.a(new DateTime((int)A_0[0] + (int)A_0[1] * 256, (int)A_0[2], (int)A_0[3]));
				p.a a = (p.a)A_0[4];
				p.a a2 = a & p.a.f;
				b.a((a == p.a.g || a2 == p.a.a) ? p.a.f : a2);
				b.a(b.d().HasFlag(p.a.d) ? ((ushort)((int)A_0[5] * 256 + (int)A_0[6])) : 45779);
				result = b;
			}
			catch (Exception innerException)
			{
				throw new FormatException(n.r(), innerException);
			}
			return result;
		}

		// Token: 0x06000123 RID: 291 RVA: 0x00005568 File Offset: 0x00003768
#pragma warning disable CS0542 // “b”: 成员名不能与它们的封闭类型相同
		public static p.b b()
#pragma warning restore CS0542 // “b”: 成员名不能与它们的封闭类型相同
		{
			return new p.b(DateTime.MaxValue, p.a.f);
		}

		// Token: 0x06000124 RID: 292 RVA: 0x00005576 File Offset: 0x00003776
		public static p.b a()
		{
			return new p.b();
		}

		// Token: 0x06000125 RID: 293 RVA: 0x00005580 File Offset: 0x00003780
		public static ushort a(string A_0)
		{
			if (string.IsNullOrEmpty(A_0))
			{
				return 45779;
			}
			byte[] bytes = Encoding.ASCII.GetBytes(A_0);
			ushort num = o.a(bytes, 0, bytes.Length);
			if (num != 45779)
			{
				return num;
			}
			return num + 1;
		}

		// Token: 0x0400007E RID: 126
#pragma warning disable CS0102 // 类型“p.b”已经包含“a”的定义
		private const ushort a = 45779;
#pragma warning restore CS0102 // 类型“p.b”已经包含“a”的定义

		// Token: 0x0400007F RID: 127
#pragma warning disable CS0542 // “b”: 成员名不能与它们的封闭类型相同
#pragma warning disable CS0102 // 类型“p.b”已经包含“b”的定义
		public const int b = 8;
#pragma warning restore CS0102 // 类型“p.b”已经包含“b”的定义
#pragma warning restore CS0542 // “b”: 成员名不能与它们的封闭类型相同

		// Token: 0x04000080 RID: 128
		[CompilerGenerated]
#pragma warning disable CS0102 // 类型“p.b”已经包含“c”的定义
		private DateTime c;
#pragma warning restore CS0102 // 类型“p.b”已经包含“c”的定义

		// Token: 0x04000081 RID: 129
		[CompilerGenerated]
#pragma warning disable CS0102 // 类型“p.b”已经包含“d”的定义
		private p.a d;
#pragma warning restore CS0102 // 类型“p.b”已经包含“d”的定义

		// Token: 0x04000082 RID: 130
		[CompilerGenerated]
#pragma warning disable CS0102 // 类型“p.b”已经包含“e”的定义
		private ushort e;
#pragma warning restore CS0102 // 类型“p.b”已经包含“e”的定义
	}
}
