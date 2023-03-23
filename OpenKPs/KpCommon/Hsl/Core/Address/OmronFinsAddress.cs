using HslCommunication.Profinet.Omron;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HslCommunication.Core.Address
{
	/// <summary>
	/// 欧姆龙的Fins协议的地址类对象
	/// </summary>
	public class OmronFinsAddress : DeviceAddressDataBase
	{
		/// <summary>
		/// 进行位操作的指令
		/// </summary>
		public byte BitCode { get; set; }

		/// <summary>
		/// 进行字操作的指令
		/// </summary>
		public byte WordCode { get; set; }

		/// <summary>
		/// 从指定的地址信息解析成真正的设备地址信息
		/// </summary>
		/// <param name="address">地址信息</param>
		/// <param name="length">数据长度</param>
		public override void Parse( string address, ushort length )
		{
			OperateResult<OmronFinsAddress> addressData = ParseFrom( address, length );
			if (addressData.IsSuccess)
			{
				AddressStart = addressData.Content.AddressStart;
				Length       = addressData.Content.Length;
				BitCode      = addressData.Content.BitCode;
				WordCode     = addressData.Content.WordCode;
			}
		}

		#region Static Method

		/// <summary>
		/// 从实际的欧姆龙的地址里面解析出地址对象<br />
		/// Resolve the address object from the actual Omron address
		/// </summary>
		/// <param name="address">欧姆龙的地址数据信息</param>
		/// <returns>是否成功的结果对象</returns>
		public static OperateResult<OmronFinsAddress> ParseFrom( string address )
		{
			return ParseFrom( address, 0 );
		}

		private static int CalculateBitIndex( string address )
		{
			string[] splits = address.SplitDot( );
			int addr = ushort.Parse( splits[0] ) * 16;
			// 包含位的情况，例如 D100.F
			if (splits.Length > 1) addr += HslHelper.CalculateBitStartIndex( splits[1] );
			return addr;
		}

		/// <summary>
		/// 从实际的欧姆龙的地址里面解析出地址对象<br />
		/// Resolve the address object from the actual Omron address
		/// </summary>
		/// <param name="address">欧姆龙的地址数据信息</param>
		/// <param name="length">读取的数据长度</param>
		/// <returns>是否成功的结果对象</returns>
		public static OperateResult<OmronFinsAddress> ParseFrom( string address, ushort length )
		{
			OmronFinsAddress addressData = new OmronFinsAddress( );
			try
			{
				addressData.Length = length;
				if (address.StartsWith( "DR" ) || address.StartsWith( "dr" ))
				{
					addressData.WordCode     = 0xBC;
					addressData.AddressStart = CalculateBitIndex( address.Substring( 2 ) ) + 0x200 * 16;
				}
				else if (address.StartsWith( "IR" ) || address.StartsWith( "ir" ))
				{
					addressData.WordCode     = 0xDC;
					addressData.AddressStart = CalculateBitIndex( address.Substring( 2 ) ) + 0x100 * 16;
				}
				else if (address.StartsWith( "DM" ) || address.StartsWith( "dm" ))
				{
					// DM区数据
					addressData.BitCode      = OmronFinsDataType.DM.BitCode;
					addressData.WordCode     = OmronFinsDataType.DM.WordCode;
					addressData.AddressStart = CalculateBitIndex( address.Substring( 2 ) );
				}
				else if (address.StartsWith( "TIM" ) || address.StartsWith( "tim" ))
				{
					addressData.BitCode      = OmronFinsDataType.TIM.BitCode;
					addressData.WordCode     = OmronFinsDataType.TIM.WordCode;
					addressData.AddressStart = CalculateBitIndex( address.Substring( 3 ) );
				}
				else if (address.StartsWith( "CNT" ) || address.StartsWith( "cnt" ))
				{
					addressData.BitCode      = OmronFinsDataType.TIM.BitCode;
					addressData.WordCode     = OmronFinsDataType.TIM.WordCode;
					addressData.AddressStart = CalculateBitIndex( address.Substring( 3 ) ) + 0x8000 * 16;
				}
				else if (address.StartsWith( "CIO" ) || address.StartsWith( "cio" ))
				{
					addressData.BitCode      = OmronFinsDataType.CIO.BitCode;
					addressData.WordCode     = OmronFinsDataType.CIO.WordCode;
					addressData.AddressStart = CalculateBitIndex( address.Substring( 3 ) );
				}
				else if (address.StartsWith( "WR" ) || address.StartsWith( "wr" ))
				{
					addressData.BitCode      = OmronFinsDataType.WR.BitCode;
					addressData.WordCode     = OmronFinsDataType.WR.WordCode;
					addressData.AddressStart = CalculateBitIndex( address.Substring( 2 ) );
				}
				else if (address.StartsWith( "HR" ) || address.StartsWith( "hr" ))
				{
					addressData.BitCode      = OmronFinsDataType.HR.BitCode;
					addressData.WordCode     = OmronFinsDataType.HR.WordCode;
					addressData.AddressStart = CalculateBitIndex( address.Substring( 2 ) );
				}
				else if (address.StartsWith( "AR" ) || address.StartsWith( "ar" ))
				{
					addressData.BitCode      = OmronFinsDataType.AR.BitCode;
					addressData.WordCode     = OmronFinsDataType.AR.WordCode;
					addressData.AddressStart = CalculateBitIndex( address.Substring( 2 ) );
				}
				else if (address.StartsWith( "EM" ) || address.StartsWith( "em" ) || address.StartsWith( "E" ) || address.StartsWith( "e" ))
				{
					// E区，比较复杂，需要专门的计算
					string[] splits = address.SplitDot( );
					int block = Convert.ToInt32( splits[0].Substring( (address[1] == 'M' || address[1] == 'm') ? 2 : 1 ), 16 );
					if (block < 16)
					{
						addressData.BitCode  = (byte)(0x20 + block);
						addressData.WordCode = (byte)(0xA0 + block);
					}
					else
					{
						addressData.BitCode  = (byte)(0xE0 + block - 16);
						addressData.WordCode = (byte)(0x60 + block - 16);
					}
					addressData.AddressStart = CalculateBitIndex( address.Substring( address.IndexOf( '.' ) + 1 ) );
				}
				else if (address.StartsWith( "D" ) || address.StartsWith( "d" ))
				{
					// DM区数据
					addressData.BitCode      = OmronFinsDataType.DM.BitCode;
					addressData.WordCode     = OmronFinsDataType.DM.WordCode;
					addressData.AddressStart = CalculateBitIndex( address.Substring( 1 ) );
				}
				else if (address.StartsWith( "C" ) || address.StartsWith( "c" ))
				{
					addressData.BitCode      = OmronFinsDataType.CIO.BitCode;
					addressData.WordCode     = OmronFinsDataType.CIO.WordCode;
					addressData.AddressStart = CalculateBitIndex( address.Substring( 1 ) );
				}
				else if (address.StartsWith( "W" ) || address.StartsWith( "w" ))
				{
					addressData.BitCode      = OmronFinsDataType.WR.BitCode;
					addressData.WordCode     = OmronFinsDataType.WR.WordCode;
					addressData.AddressStart = CalculateBitIndex( address.Substring( 1 ) );
				}
				else if (address.StartsWith( "H" ) || address.StartsWith( "h" ))
				{
					addressData.BitCode      = OmronFinsDataType.HR.BitCode;
					addressData.WordCode     = OmronFinsDataType.HR.WordCode;
					addressData.AddressStart = CalculateBitIndex( address.Substring( 1 ) );
				}
				else if (address.StartsWith( "A" ) || address.StartsWith( "a" ))
				{
					addressData.BitCode      = OmronFinsDataType.AR.BitCode;
					addressData.WordCode     = OmronFinsDataType.AR.WordCode;
					addressData.AddressStart = CalculateBitIndex( address.Substring( 1 ) );
				}
				else
				{
					throw new Exception( StringResources.Language.NotSupportedDataType );
				}
			}
			catch (Exception ex)
			{
				return new OperateResult<OmronFinsAddress>( ex.Message );
			}

			return OperateResult.CreateSuccessResult( addressData );
		}

		#endregion
	}
}
