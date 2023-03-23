using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HslCommunication.Core.Address
{
	/// <summary>
	/// 基恩士上位链路协议的地址类对象
	/// </summary>
	public class KeyenceNanoAddress : DeviceAddressDataBase
	{
		#region Constructor

		/// <summary>
		/// 实例化一个默认的对象
		/// </summary>
		public KeyenceNanoAddress( )
		{

		}

		/// <summary>
		/// 通过指定的参数来实例化对象
		/// </summary>
		/// <param name="dataCode">数据类型</param>
		/// <param name="address">偏移地址</param>
		/// <param name="splits">切割但愿长度</param>
		public KeyenceNanoAddress( string dataCode, int address, int splits )
		{
			this.DataCode     = dataCode;
			this.AddressStart = address;
			this.SplitLength  = splits;
		}

		#endregion


		/// <summary>
		/// 获取或设置等待读取的数据的代码<br />
		/// Get or set the code of the data waiting to be read
		/// </summary>
		public string DataCode { get; set; }

		/// <summary>
		/// 获取或设置读取的时候切割的数据长度信息
		/// </summary>
		public int SplitLength { get; set; }

		/// <summary>
		/// 获取地址的字符串表示方式
		/// </summary>
		/// <returns>字符串信息</returns>
		public string GetAddressStartFormat( )
		{
			switch (this.DataCode)
			{
				case "":
				case "CR":
				case "MR":
				case "LR": return AddressStart >= 16 ? $"{AddressStart / 16}{AddressStart % 16:D2}" : $"{AddressStart % 16}";
				case "B":
				case "VB":
				case "W": return AddressStart.ToString( "X" );
				default: return AddressStart.ToString( );
			}
		}

		/// <summary>
		/// 从指定的地址信息解析成真正的设备地址信息
		/// </summary>
		/// <param name="address">地址信息</param>
		/// <param name="length">数据长度</param>
		public override void Parse( string address, ushort length )
		{
			OperateResult<KeyenceNanoAddress> analysis = ParseFrom( address, length );
			if(analysis.IsSuccess)
			{
				AddressStart = analysis.Content.AddressStart;
				DataCode     = analysis.Content.DataCode;
				SplitLength  = analysis.Content.SplitLength;
			}
		}

		/// <summary>
		/// 位地址转换方法，101等同于10.1等同于10*16+1=161<br />
		/// Bit address conversion method, 101 is equivalent to 10.1 is equivalent to 10 * 16 + 1 = 161
		/// </summary>
		/// <param name="address">地址信息</param>
		/// <returns>实际的位地址信息</returns>
		private static int CalculateAddress( string address )
		{
			int add = 0;
			if (address.IndexOf( "." ) < 0)
			{
				if (address.Length <= 2)
					add = Convert.ToInt32( address );
				else
					add = Convert.ToInt32( address.Substring( 0, address.Length - 2 ) ) * 16 + Convert.ToInt32( address.Substring( address.Length - 2 ) );
			}
			else
			{
				add = Convert.ToInt32( address.Substring( 0, address.IndexOf( "." ) ) ) * 16;
				string bit = address.Substring( address.IndexOf( "." ) + 1 );
				add += HslHelper.CalculateBitStartIndex( bit );
			}
			return add;
		}

		/// <summary>
		/// 解析出一个基恩士上位链路协议的地址信息
		/// </summary>
		/// <param name="address">字符串地址</param>
		/// <param name="length">长度信息</param>
		/// <returns>成功地址</returns>
		public static OperateResult<KeyenceNanoAddress> ParseFrom( string address, ushort length )
		{
			try
			{
				if (address.StartsWith( "CTH" ) || address.StartsWith( "cth" ))      return OperateResult.CreateSuccessResult( new KeyenceNanoAddress( "CTH", Convert.ToInt32( address.Substring( 3 ) ), 2 ) );
				else if (address.StartsWith( "CTC" ) || address.StartsWith( "ctc" )) return OperateResult.CreateSuccessResult( new KeyenceNanoAddress( "CTC", Convert.ToInt32( address.Substring( 3 ) ), 4 ) );
				else if (address.StartsWith( "CR" ) || address.StartsWith( "cr" ))   return OperateResult.CreateSuccessResult( new KeyenceNanoAddress( "CR",  CalculateAddress( address.Substring( 2 ) ), 256 ) );
				else if (address.StartsWith( "MR" ) || address.StartsWith( "mr" ))   return OperateResult.CreateSuccessResult( new KeyenceNanoAddress( "MR",  CalculateAddress( address.Substring( 2 ) ), 256 ) );
				else if (address.StartsWith( "LR" ) || address.StartsWith( "lr" ))   return OperateResult.CreateSuccessResult( new KeyenceNanoAddress( "LR",  CalculateAddress( address.Substring( 2 ) ), 256 ) );
				else if (address.StartsWith( "DM" ) || address.StartsWith( "dm" ))   return OperateResult.CreateSuccessResult( new KeyenceNanoAddress( "DM",  Convert.ToInt32( address.Substring( 2 ) ), 256 ) );
				else if (address.StartsWith( "CM" ) || address.StartsWith( "cm" ))   return OperateResult.CreateSuccessResult( new KeyenceNanoAddress( "CM",  Convert.ToInt32( address.Substring( 2 ) ), 256 ) );
				else if (address.StartsWith( "TM" ) || address.StartsWith( "tm" ))   return OperateResult.CreateSuccessResult( new KeyenceNanoAddress( "TM",  Convert.ToInt32( address.Substring( 2 ) ), 256 ) );
				else if (address.StartsWith( "VM" ) || address.StartsWith( "vm" ))   return OperateResult.CreateSuccessResult( new KeyenceNanoAddress( "VM",  Convert.ToInt32( address.Substring( 2 ) ), 256 ) );
				else if (address.StartsWith( "VB" ) || address.StartsWith( "vb" ))   return OperateResult.CreateSuccessResult( new KeyenceNanoAddress( "VB",  Convert.ToInt32( address.Substring( 2 ), 16 ), 256 ) );
				else if (address.StartsWith( "EM" ) || address.StartsWith( "em" ))   return OperateResult.CreateSuccessResult( new KeyenceNanoAddress( "EM",  Convert.ToInt32( address.Substring( 2 ) ), 256 ) );
				else if (address.StartsWith( "FM" ) || address.StartsWith( "fm" ))   return OperateResult.CreateSuccessResult( new KeyenceNanoAddress( "FM",  Convert.ToInt32( address.Substring( 2 ) ), 256 ) );
				else if (address.StartsWith( "ZF" ) || address.StartsWith( "zf" ))   return OperateResult.CreateSuccessResult( new KeyenceNanoAddress( "ZF",  Convert.ToInt32( address.Substring( 2 ) ), 256 ) );
				else if (address.StartsWith( "AT" ) || address.StartsWith( "at" ))   return OperateResult.CreateSuccessResult( new KeyenceNanoAddress( "AT",  Convert.ToInt32( address.Substring( 2 ) ), 8 ) );
				else if (address.StartsWith( "TS" ) || address.StartsWith( "ts" ))   return OperateResult.CreateSuccessResult( new KeyenceNanoAddress( "TS",  Convert.ToInt32( address.Substring( 2 ) ), 64 ) );
				else if (address.StartsWith( "TC" ) || address.StartsWith( "tc" ))   return OperateResult.CreateSuccessResult( new KeyenceNanoAddress( "TC",  Convert.ToInt32( address.Substring( 2 ) ), 64 ) );
				else if (address.StartsWith( "CC" ) || address.StartsWith( "cc" ))   return OperateResult.CreateSuccessResult( new KeyenceNanoAddress( "CC",  Convert.ToInt32( address.Substring( 2 ) ), 64 ) );
				else if (address.StartsWith( "CS" ) || address.StartsWith( "cs" ))   return OperateResult.CreateSuccessResult( new KeyenceNanoAddress( "CS",  Convert.ToInt32( address.Substring( 2 ) ), 64 ) );
				else if (address.StartsWith( "W" ) || address.StartsWith( "w" ))     return OperateResult.CreateSuccessResult( new KeyenceNanoAddress( "W",   Convert.ToInt32( address.Substring( 1 ), 16 ), 256 ) );
				else if (address.StartsWith( "Z" ) || address.StartsWith( "z" ))     return OperateResult.CreateSuccessResult( new KeyenceNanoAddress( "Z",   Convert.ToInt32( address.Substring( 1 ) ), 12 ) );
				else if (address.StartsWith( "R" ) || address.StartsWith( "r" ))     return OperateResult.CreateSuccessResult( new KeyenceNanoAddress( "",    CalculateAddress( address.Substring( 1 ) ), 256 ) );
				else if (address.StartsWith( "B" ) || address.StartsWith( "b" ))     return OperateResult.CreateSuccessResult( new KeyenceNanoAddress( "B",   Convert.ToInt32( address.Substring( 1 ), 16 ), 256 ) );
				else if (address.StartsWith( "T" ) || address.StartsWith( "t" ))     return OperateResult.CreateSuccessResult( new KeyenceNanoAddress( "T",   Convert.ToInt32( address.Substring( 1 ) ), 64 ) );
				else if (address.StartsWith( "C" ) || address.StartsWith( "c" ))     return OperateResult.CreateSuccessResult( new KeyenceNanoAddress( "C",   Convert.ToInt32( address.Substring( 1 ) ) , 64) );
				else
					throw new Exception( StringResources.Language.NotSupportedDataType );
			}
			catch (Exception ex)
			{
				return new OperateResult<KeyenceNanoAddress>( ex.Message );
			}
		}
	}
}
