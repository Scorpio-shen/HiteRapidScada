using System;
using System.IO;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

// Token: 0x02000011 RID: 17
internal sealed class m
{
	// Token: 0x060000B2 RID: 178 RVA: 0x00003CFC File Offset: 0x00001EFC
	private string e(string A_0)
	{
		RijndaelManaged rijndaelManaged = null;
		string result;
		try
		{
			rijndaelManaged = new RijndaelManaged();
			rijndaelManaged.Key = m.c;
			rijndaelManaged.IV = m.d;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				using (CryptoStream cryptoStream = new CryptoStream(memoryStream, rijndaelManaged.CreateEncryptor(m.c, m.d), CryptoStreamMode.Write))
				{
					using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
					{
						streamWriter.Write(A_0);
					}
				}
				result = o.a(memoryStream.ToArray());
			}
		}
		finally
		{
			if (rijndaelManaged != null)
			{
				rijndaelManaged.Clear();
			}
		}
		return result;
	}

	// Token: 0x060000B3 RID: 179 RVA: 0x00003DC4 File Offset: 0x00001FC4
	private string d(string A_0)
	{
		byte[] buffer = o.a(A_0);
		RijndaelManaged rijndaelManaged = null;
		string result;
		try
		{
			rijndaelManaged = new RijndaelManaged();
			rijndaelManaged.Key = m.c;
			rijndaelManaged.IV = m.d;
			using (MemoryStream memoryStream = new MemoryStream(buffer))
			{
				using (CryptoStream cryptoStream = new CryptoStream(memoryStream, rijndaelManaged.CreateDecryptor(m.c, m.d), CryptoStreamMode.Read))
				{
					using (StreamReader streamReader = new StreamReader(cryptoStream))
					{
						result = streamReader.ReadToEnd();
					}
				}
			}
		}
		finally
		{
			if (rijndaelManaged != null)
			{
				rijndaelManaged.Clear();
			}
		}
		return result;
	}

	// Token: 0x060000B4 RID: 180 RVA: 0x00003E88 File Offset: 0x00002088
	private string c(string A_0)
	{
		StringBuilder stringBuilder = new StringBuilder("=");
		int i = 0;
		int length = A_0.Length;
		while (i < length)
		{
			if (i + 16 < length)
			{
				stringBuilder.Append(A_0.Substring(i, 16)).Append("-");
			}
			else
			{
				stringBuilder.Append(A_0.Substring(i));
			}
			i += 16;
		}
		stringBuilder.Append("=");
		return stringBuilder.ToString();
	}

	// Token: 0x060000B5 RID: 181 RVA: 0x00003EF8 File Offset: 0x000020F8
	private string b(string A_0)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (A_0 != null)
		{
			foreach (char value in A_0)
			{
				if (char.IsLetterOrDigit(value))
				{
					stringBuilder.Append(value);
				}
			}
		}
		return stringBuilder.ToString();
	}

	// Token: 0x060000B6 RID: 182 RVA: 0x00003F40 File Offset: 0x00002140
#pragma warning disable CS0102 // 类型“m”已经包含“a”的定义
	private string a()
#pragma warning restore CS0102 // 类型“m”已经包含“a”的定义
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
		{
			if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet || networkInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
			{
				stringBuilder.Append(networkInterface.GetPhysicalAddress()).Append("; ");
			}
		}
		return stringBuilder.ToString().TrimEnd(new char[]
		{
			';',
			' '
		});
	}

	// Token: 0x060000B7 RID: 183 RVA: 0x00003FB0 File Offset: 0x000021B0
#pragma warning disable CS0102 // 类型“m”已经包含“a”的定义
	public string a(m.a A_0)
#pragma warning restore CS0102 // 类型“m”已经包含“a”的定义
	{
		if (A_0 == null)
		{
			throw new ArgumentNullException("storedInfo");
		}
		string result;
		try
		{
			string a_ = A_0.ToString();
			a_ = this.e(a_);
			result = this.c(a_);
		}
		catch (Exception ex)
		{
			result = "=00000000-" + Marshal.GetHRForException(ex).ToString("X8") + "=";
		}
		return result;
	}

	// Token: 0x060000B8 RID: 184 RVA: 0x0000401C File Offset: 0x0000221C
	public string b()
	{
		string a_;
		try
		{
			a_ = Environment.MachineName;
		}
		catch
		{
			a_ = "";
		}
		string a_2;
		try
		{
			a_2 = Environment.UserDomainName;
		}
		catch
		{
			a_2 = "";
		}
		string a_3;
		try
		{
			a_3 = this.a();
		}
		catch
		{
			a_3 = "";
		}
		string a_4;
		try
		{
			a_4 = (string.IsNullOrEmpty(m.e) ? Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) : m.e);
		}
		catch
		{
			a_4 = "";
		}
		m.a a = new m.a();
		a.b(a_);
		a.c(a_2);
		a.d(a_3);
		a.e(a_4);
		return this.a(a);
	}

	// Token: 0x060000B9 RID: 185 RVA: 0x000040EC File Offset: 0x000022EC
#pragma warning disable CS0102 // 类型“m”已经包含“a”的定义
	public bool a(string A_0, out m.a A_1, out string A_2)
#pragma warning restore CS0102 // 类型“m”已经包含“a”的定义
	{
		bool result;
		try
		{
			A_0 = (A_0 ?? "");
			if (A_0.StartsWith("=00000000-", StringComparison.Ordinal))
			{
				A_1 = new m.a();
				A_2 = n.v();
				result = false;
			}
			else
			{
				A_0 = this.b(A_0);
				A_0 = this.d(A_0);
				A_1 = m.a.a(A_0);
				A_2 = "";
				result = true;
			}
		}
		catch (Exception ex)
		{
			A_1 = new m.a();
			A_2 = n.u() + ": " + ex.Message;
			result = false;
		}
		return result;
	}

	// Token: 0x060000BA RID: 186 RVA: 0x00004180 File Offset: 0x00002380
#pragma warning disable CS0102 // 类型“m”已经包含“a”的定义
	public bool a(string A_0, out string A_1, out string A_2)
#pragma warning restore CS0102 // 类型“m”已经包含“a”的定义
	{
		bool result;
		try
		{
			if (!File.Exists(A_0))
			{
				throw new FileNotFoundException(string.Format(n.j(), A_0));
			}
			using (FileStream fileStream = new FileStream(A_0, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				using (StreamReader streamReader = new StreamReader(fileStream, Encoding.UTF8))
				{
					A_1 = streamReader.ReadLine();
				}
			}
			A_2 = "";
			result = true;
		}
		catch (Exception ex)
		{
			A_1 = "";
			A_2 = n.i() + ": " + ex.Message;
			result = false;
		}
		return result;
	}

	// Token: 0x060000BB RID: 187 RVA: 0x00004234 File Offset: 0x00002434
#pragma warning disable CS0102 // 类型“m”已经包含“a”的定义
	public bool a(string A_0, string A_1, out string A_2)
#pragma warning restore CS0102 // 类型“m”已经包含“a”的定义
	{
		bool result;
		try
		{
			using (FileStream fileStream = new FileStream(A_0, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
			{
				using (StreamWriter streamWriter = new StreamWriter(fileStream, Encoding.UTF8))
				{
					streamWriter.Write(A_1);
				}
			}
			A_2 = "";
			result = true;
		}
		catch (Exception ex)
		{
			A_2 = n.h() + ": " + ex.Message;
			result = false;
		}
		return result;
	}

	// Token: 0x060000BC RID: 188 RVA: 0x000042C8 File Offset: 0x000024C8
#pragma warning disable CS0102 // 类型“m”已经包含“a”的定义
	public static bool a(string A_0)
#pragma warning restore CS0102 // 类型“m”已经包含“a”的定义
	{
		return string.IsNullOrEmpty(A_0) || A_0.StartsWith("=00000000-", StringComparison.Ordinal);
	}

	// Token: 0x04000049 RID: 73
#pragma warning disable CS0102 // 类型“m”已经包含“a”的定义
	public const string a = "CompCode.txt";
#pragma warning restore CS0102 // 类型“m”已经包含“a”的定义

	// Token: 0x0400004A RID: 74
#pragma warning disable CS0102 // 类型“m”已经包含“b”的定义
	private const string b = "=00000000-";
#pragma warning restore CS0102 // 类型“m”已经包含“b”的定义

	// Token: 0x0400004B RID: 75
#pragma warning disable CS0102 // 类型“m”已经包含“c”的定义
	private static readonly byte[] c = new byte[]
#pragma warning restore CS0102 // 类型“m”已经包含“c”的定义
	{
		215,
		148,
		69,
		223,
		121,
		124,
		205,
		228,
		71,
		117,
		112,
		26,
		152,
		48,
		32,
		181,
		161,
		129,
		154,
		3,
		245,
		134,
		31,
		25,
		66,
		150,
		240,
		136,
		176,
		111,
		197,
		19
	};

	// Token: 0x0400004C RID: 76
#pragma warning disable CS0102 // 类型“m”已经包含“d”的定义
	private static readonly byte[] d = new byte[]
#pragma warning restore CS0102 // 类型“m”已经包含“d”的定义
	{
		202,
		117,
		63,
		34,
		235,
		93,
		67,
		21,
		219,
		38,
		94,
		81,
		130,
		64,
		174,
		194
	};

	// Token: 0x0400004D RID: 77
#pragma warning disable CS0102 // 类型“m”已经包含“e”的定义
	internal static string e = "";
#pragma warning restore CS0102 // 类型“m”已经包含“e”的定义

	// Token: 0x02000012 RID: 18
	public class a
	{
		// Token: 0x060000BF RID: 191 RVA: 0x00004322 File Offset: 0x00002522
		public a()
		{
			this.b("");
			this.c("");
			this.d("");
			this.e("");
		}

		// Token: 0x060000C0 RID: 192 RVA: 0x00004356 File Offset: 0x00002556
		[CompilerGenerated]
#pragma warning disable CS0542 // “a”: 成员名不能与它们的封闭类型相同
		public string a()
#pragma warning restore CS0542 // “a”: 成员名不能与它们的封闭类型相同
		{
			return this.b;
		}

		// Token: 0x060000C1 RID: 193 RVA: 0x0000435E File Offset: 0x0000255E
		[CompilerGenerated]
		public void b(string A_0)
		{
			this.b = A_0;
		}

		// Token: 0x060000C2 RID: 194 RVA: 0x00004367 File Offset: 0x00002567
		[CompilerGenerated]
		public string b()
		{
			return this.c;
		}

		// Token: 0x060000C3 RID: 195 RVA: 0x0000436F File Offset: 0x0000256F
		[CompilerGenerated]
		public void c(string A_0)
		{
			this.c = A_0;
		}

		// Token: 0x060000C4 RID: 196 RVA: 0x00004378 File Offset: 0x00002578
		[CompilerGenerated]
		public string c()
		{
			return this.d;
		}

		// Token: 0x060000C5 RID: 197 RVA: 0x00004380 File Offset: 0x00002580
		[CompilerGenerated]
		public void d(string A_0)
		{
			this.d = A_0;
		}

		// Token: 0x060000C6 RID: 198 RVA: 0x00004389 File Offset: 0x00002589
		[CompilerGenerated]
		public string d()
		{
			return this.e;
		}

		// Token: 0x060000C7 RID: 199 RVA: 0x00004391 File Offset: 0x00002591
		[CompilerGenerated]
		public void e(string A_0)
		{
			this.e = A_0;
		}

		// Token: 0x060000C8 RID: 200 RVA: 0x0000439C File Offset: 0x0000259C
		public override string ToString()
		{
			return new StringBuilder().Append(this.a()).Append('\n').Append(this.b()).Append('\n').Append(this.c()).Append('\n').Append(this.d()).ToString();
		}

		// Token: 0x060000C9 RID: 201 RVA: 0x000043F4 File Offset: 0x000025F4
#pragma warning disable CS0542 // “a”: 成员名不能与它们的封闭类型相同
		public static m.a a(string A_0)
#pragma warning restore CS0542 // “a”: 成员名不能与它们的封闭类型相同
		{
			if (A_0 == null)
			{
				throw new ArgumentNullException("s");
			}
			string[] array = A_0.Split(new char[]
			{
				'\n'
			}, StringSplitOptions.None);
			int num = array.Length;
			m.a a = new m.a();
			a.b((num > 0) ? array[0] : "");
			a.c((num > 1) ? array[1] : "");
			a.d((num > 2) ? array[2] : "");
			a.e((num > 3) ? array[3] : "");
			return a;
		}

		// Token: 0x0400004E RID: 78
#pragma warning disable CS0102 // 类型“m.a”已经包含“a”的定义
#pragma warning disable CS0542 // “a”: 成员名不能与它们的封闭类型相同
		private const char a = '\n';
#pragma warning restore CS0542 // “a”: 成员名不能与它们的封闭类型相同
#pragma warning restore CS0102 // 类型“m.a”已经包含“a”的定义

		// Token: 0x0400004F RID: 79
		[CompilerGenerated]
#pragma warning disable CS0102 // 类型“m.a”已经包含“b”的定义
		private string b;
#pragma warning restore CS0102 // 类型“m.a”已经包含“b”的定义

		// Token: 0x04000050 RID: 80
		[CompilerGenerated]
#pragma warning disable CS0102 // 类型“m.a”已经包含“c”的定义
		private string c;
#pragma warning restore CS0102 // 类型“m.a”已经包含“c”的定义

		// Token: 0x04000051 RID: 81
		[CompilerGenerated]
#pragma warning disable CS0102 // 类型“m.a”已经包含“d”的定义
		private string d;
#pragma warning restore CS0102 // 类型“m.a”已经包含“d”的定义

		// Token: 0x04000052 RID: 82
		[CompilerGenerated]
#pragma warning disable CS0102 // 类型“m.a”已经包含“e”的定义
		private string e;
#pragma warning restore CS0102 // 类型“m.a”已经包含“e”的定义
	}
}
