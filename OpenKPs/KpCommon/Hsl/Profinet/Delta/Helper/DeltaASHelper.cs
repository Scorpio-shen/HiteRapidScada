using HslCommunication.Core;
using HslCommunication.ModBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if !NET20 && !NET35
using System.Threading.Tasks;
#endif

namespace HslCommunication.Profinet.Delta.Helper
{
	/// <summary>
	/// 台达AS300的辅助帮助类信息
	/// </summary>
	public class DeltaASHelper
	{
		private static int ParseDeltaBitAddress( string address )
		{
			int bitIndex = address.IndexOf( '.' );
			if (bitIndex > 0)
				return Convert.ToInt32( address.Substring( 0, bitIndex ) ) * 16 + HslHelper.CalculateBitStartIndex( address.Substring( bitIndex + 1 ) );
			else
				return Convert.ToInt32( address ) * 16;
		}

		/// <summary>
		/// 根据台达AS300的PLC的地址，解析出转换后的modbus协议信息，适用AS300系列，当前的地址仍然支持站号指定，例如s=2;D100<br />
		/// According to the PLC address of Delta AS300, the converted modbus protocol information is parsed, 
		/// and it is applicable to AS300 series. The current address still supports station number designation, for example, s=2;D100
		/// </summary>
		/// <param name="address">台达plc的地址信息</param>
		/// <param name="modbusCode">原始的对应的modbus信息</param>
		/// <returns>还原后的modbus地址</returns>
		public static OperateResult<string> ParseDeltaASAddress( string address, byte modbusCode )
		{
			try
			{
				string station = string.Empty;
				OperateResult<int> stationPara = HslHelper.ExtractParameter( ref address, "s" );
				if (stationPara.IsSuccess) station = $"s={stationPara.Content};";

				if (modbusCode == ModbusInfo.ReadCoil || modbusCode == ModbusInfo.WriteCoil || modbusCode == ModbusInfo.WriteOneCoil)
				{
					if( address.StartsWith( "SM" ) || address.StartsWith( "sm" ) )
						return OperateResult.CreateSuccessResult( station + (Convert.ToInt32( address.Substring( 2 ) ) + 0x4000).ToString( ) );
					else if( address.StartsWith( "HC" ) || address.StartsWith( "hc" ))
						return OperateResult.CreateSuccessResult( station + (Convert.ToInt32( address.Substring( 2 ) ) + 0xFC00).ToString( ) );
					else if (address.StartsWith( "S" ) || address.StartsWith( "s" ))
						return OperateResult.CreateSuccessResult( station + (Convert.ToInt32( address.Substring( 1 ) ) + 0x5000 ).ToString( ) );
					else if (address.StartsWith( "X" ) || address.StartsWith( "x" ))
						return OperateResult.CreateSuccessResult( station + "x=2;" + (ParseDeltaBitAddress( address.Substring( 1 ) ) + 0x6000).ToString( ) );
					else if (address.StartsWith( "Y" ) || address.StartsWith( "y" ))
						return OperateResult.CreateSuccessResult( station + (ParseDeltaBitAddress( address.Substring( 1 ) ) + 0xA000).ToString( ) );
					else if (address.StartsWith( "T" ) || address.StartsWith( "t" ))
						return OperateResult.CreateSuccessResult( station + (Convert.ToInt32( address.Substring( 1 ) ) + 0xE000).ToString( ) );
					else if (address.StartsWith( "C" ) || address.StartsWith( "c" ))
						return OperateResult.CreateSuccessResult( station + (Convert.ToInt32( address.Substring( 1 ) ) + 0xF000).ToString( ) );
					else if (address.StartsWith( "M" ) || address.StartsWith( "m" ))
						return OperateResult.CreateSuccessResult( station + (Convert.ToInt32( address.Substring( 1 ) ) + 0x0000).ToString( ) );
					else if (address.StartsWith( "D" ) && address.Contains( "." )) return OperateResult.CreateSuccessResult( station + address );
				}
				else
				{
					if (address.StartsWith( "SR" ) || address.StartsWith( "sr" ))
						return OperateResult.CreateSuccessResult( station + (Convert.ToInt32( address.Substring( 2 ) ) + 0xC000).ToString( ) );
					else if (address.StartsWith( "HC" ) || address.StartsWith( "hc" ))
						return OperateResult.CreateSuccessResult( station + (Convert.ToInt32( address.Substring( 2 ) ) + 0xFC00).ToString( ) );
					else if (address.StartsWith( "D" ) || address.StartsWith( "d" ))
						return OperateResult.CreateSuccessResult( station + (Convert.ToInt32( address.Substring( 1 ) ) + 0x0000).ToString( ) );
					else if (address.StartsWith( "X" ) || address.StartsWith( "x" ))
						return OperateResult.CreateSuccessResult( station + "x=4;" + (Convert.ToInt32( address.Substring( 1 ) ) + 0x8000).ToString( ) );
					else if (address.StartsWith( "Y" ) || address.StartsWith( "y" ))
						return OperateResult.CreateSuccessResult( station + (Convert.ToInt32( address.Substring( 1 ) ) + 0xA000).ToString( ) );
					else if (address.StartsWith( "C" ) || address.StartsWith( "c" ))
						return OperateResult.CreateSuccessResult( station + (Convert.ToInt32( address.Substring( 1 ) ) + 0xF000).ToString( ) );
					else if (address.StartsWith( "T" ) || address.StartsWith( "t" ))
						return OperateResult.CreateSuccessResult( station + (Convert.ToInt32( address.Substring( 1 ) ) + 0xE000).ToString( ) );
					else if (address.StartsWith( "E" ) || address.StartsWith( "e" ))
						return OperateResult.CreateSuccessResult( station + (Convert.ToInt32( address.Substring( 1 ) ) + 0xFE00).ToString( ) );
				}

				return new OperateResult<string>( StringResources.Language.NotSupportedDataType );
			}
			catch (Exception ex)
			{
				return new OperateResult<string>( ex.Message );
			}
		}
	}
}
