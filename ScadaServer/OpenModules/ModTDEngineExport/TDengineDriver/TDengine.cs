using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace TDengineDriver
{

	internal class TDengine
	{
		[DllImport("taos", CallingConvention = CallingConvention.Cdecl, EntryPoint = "taos_init")]
		public static extern void Init();

		[DllImport("taos", CallingConvention = CallingConvention.Cdecl, EntryPoint = "taos_cleanup")]
		public static extern void Cleanup();

		[DllImport("taos", CallingConvention = CallingConvention.Cdecl, EntryPoint = "taos_options")]
		public static extern void Options(int option, string value);

		[DllImport("taos", CallingConvention = CallingConvention.Cdecl, EntryPoint = "taos_connect")]
		public static extern IntPtr Connect(string ip, string user, string password, string db, short port);

		[DllImport("taos", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr taos_errstr(IntPtr res);

		public static string Error(IntPtr res)
		{
			return Marshal.PtrToStringAnsi(TDengine.taos_errstr(res));
		}

		[DllImport("taos", CallingConvention = CallingConvention.Cdecl, EntryPoint = "taos_errno")]
		public static extern int ErrorNo(IntPtr res);


		[DllImport("taos", CallingConvention = CallingConvention.Cdecl, EntryPoint = "taos_query")]
		public static extern IntPtr Query(IntPtr conn, string sqlstr);

		[DllImport("taos", CallingConvention = CallingConvention.Cdecl, EntryPoint = "taos_affected_rows")]
		public static extern int AffectRows(IntPtr res);

		[DllImport("taos", CallingConvention = CallingConvention.Cdecl, EntryPoint = "taos_field_count")]
		public static extern int FieldCount(IntPtr res);

		[DllImport("taos", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr taos_fetch_fields(IntPtr res);

		public static List<TDengineMeta> FetchFields(IntPtr res)
		{
			List<TDengineMeta> list = new List<TDengineMeta>();
			if (res == IntPtr.Zero)
			{
				return list;
			}
			int num = TDengine.FieldCount(res);
			IntPtr pointer = TDengine.taos_fetch_fields(res);
			for (int i = 0; i < num; i++)
			{
				int offset = i * 68;
				list.Add(new TDengineMeta
				{
					name = Marshal.PtrToStringAnsi(pointer + offset),
					type = Marshal.ReadByte(pointer + offset + 65),
					size = Marshal.ReadInt16(pointer + offset + 66)
				});
			}
			return list;
		}

		[DllImport("taos", CallingConvention = CallingConvention.Cdecl, EntryPoint = "taos_fetch_row")]
		public static extern IntPtr FetchRows(IntPtr res);

		[DllImport("taos", CallingConvention = CallingConvention.Cdecl, EntryPoint = "taos_free_result")]
		public static extern IntPtr FreeResult(IntPtr res);

		[DllImport("taos", CallingConvention = CallingConvention.Cdecl, EntryPoint = "taos_close")]
		public static extern int Close(IntPtr taos);

		[DllImport("taos", CallingConvention = CallingConvention.Cdecl, EntryPoint = "taos_result_precision")]
		public static extern int ResultPrecision(IntPtr taos);

		public const int TSDB_CODE_SUCCESS = 0;
	}
}
