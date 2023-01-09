using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HslCommunication.Core.Address
{
	/// <summary>
	/// 丰炜PLC的地址类对象
	/// </summary>
	public class VigorAddress : DeviceAddressDataBase
	{
		/// <summary>
		/// 获取或设置等待读取的数据的代码<br />
		/// Get or set the code of the data waiting to be read
		/// </summary>
		public byte DataCode { get; set; }

		/// <inheritdoc/>
		public override string ToString( )
		{
			return AddressStart.ToString( );
		}

		#region Static Method

		/// <summary>
		/// 从实际的丰炜PLC的地址里面解析出地址对象<br />
		/// Resolve the address object from the actual Vigor address
		/// </summary>
		/// <param name="address">丰炜的地址数据信息</param>
		/// <param name="length">读取的数据长度</param>
		/// <param name="isBit">是否是对位进行访问的</param>
		/// <returns>是否成功的结果对象</returns>
		public static OperateResult<VigorAddress> ParseFrom( string address, ushort length, bool isBit )
		{
			VigorAddress addressData = new VigorAddress( );
			try
			{
				addressData.Length = length;
				if (isBit)
				{
					if      (address.StartsWith( "SM" ) || address.StartsWith( "sm" )) { addressData.DataCode = 0x94; addressData.AddressStart = Convert.ToInt32( address.Substring( 2 ) ); }
					else if (address.StartsWith( "TS" ) || address.StartsWith( "ts" )) { addressData.DataCode = 0x99; addressData.AddressStart = Convert.ToInt32( address.Substring( 2 ) ); }   // 触点
					else if (address.StartsWith( "TC" ) || address.StartsWith( "tc" )) { addressData.DataCode = 0x98; addressData.AddressStart = Convert.ToInt32( address.Substring( 2 ) ); }   // 线圈
					else if (address.StartsWith( "CS" ) || address.StartsWith( "cs" )) { addressData.DataCode = 0x9D; addressData.AddressStart = Convert.ToInt32( address.Substring( 2 ) ); }   // 触点
					else if (address.StartsWith( "CC" ) || address.StartsWith( "cc" )) { addressData.DataCode = 0x9C; addressData.AddressStart = Convert.ToInt32( address.Substring( 2 ) ); }   // 线圈
					else if (address.StartsWith( "X" )  || address.StartsWith( "x" ))  { addressData.DataCode = 0x90; addressData.AddressStart = Convert.ToInt32( address.Substring( 1 ) ); }
					else if (address.StartsWith( "Y" )  || address.StartsWith( "y" ))  { addressData.DataCode = 0x91; addressData.AddressStart = Convert.ToInt32( address.Substring( 1 ) ); }
					else if (address.StartsWith( "M" )  || address.StartsWith( "m" )) 
					{
						addressData.AddressStart = Convert.ToInt32( address.Substring( 1 ) );
						if (addressData.AddressStart >= 9000)
						{ 
							addressData.AddressStart = 0;
							addressData.DataCode = 0x94;
						}
						else
						{
							addressData.DataCode = 0x92;
						}
					}
					else if (address.StartsWith( "S" )  || address.StartsWith( "s" ))  { addressData.DataCode = 0x93; addressData.AddressStart = Convert.ToInt32( address.Substring( 1 ) ); }
					else return new OperateResult<VigorAddress>( StringResources.Language.NotSupportedDataType );
				}
				else
				{
					if (address.StartsWith( "SD" ) || address.StartsWith( "sd" )) { addressData.DataCode = 0xA1; addressData.AddressStart = Convert.ToInt32( address.Substring( 2 ) ); }
					else if (address.StartsWith( "D" ) || address.StartsWith( "d" ))
					{
						addressData.AddressStart = Convert.ToInt32( address.Substring( 1 ) );
						if (addressData.AddressStart >= 9000)
						{
							addressData.DataCode = 0xA1;
							addressData.AddressStart = addressData.AddressStart - 9000;
						}
						else
						{
							addressData.DataCode = 0xA0;
						}
					}
					else if (address.StartsWith( "R" ) || address.StartsWith( "r" )) { addressData.DataCode = 0xA2; addressData.AddressStart = Convert.ToInt32( address.Substring( 1 ) ); }
					else if (address.StartsWith( "T" ) || address.StartsWith( "t" )) { addressData.DataCode = 0xA8; addressData.AddressStart = Convert.ToInt32( address.Substring( 1 ) ); }
					else if (address.StartsWith( "C" ) || address.StartsWith( "c" ))
					{
						addressData.AddressStart = Convert.ToInt32( address.Substring( 1 ) );
						if (addressData.AddressStart >= 200)
						{
							addressData.DataCode = 0xAD;
						}
						else
						{
							addressData.DataCode = 0xAC;
						}
					}
					else return new OperateResult<VigorAddress>( StringResources.Language.NotSupportedDataType );
				}
			}
			catch (Exception ex)
			{
				return new OperateResult<VigorAddress>( StringResources.Language.AddressFormatWrong + ex.Message );
			}

			return OperateResult.CreateSuccessResult( addressData );
		}

		#endregion
	}
}
