using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HslCommunication.BasicFramework;
using HslCommunication.Core;
using HslCommunication.ModBus;
using HslCommunication.Core.Address;
using System.Text.RegularExpressions;
#if !NET35 && !NET20
using System.Threading.Tasks;
#endif

namespace HslCommunication.Profinet.XINJE
{
	/// <summary>
	/// 信捷PLC的相关辅助类
	/// </summary>
	public class XinJEHelper
	{
		private static int CalculateXinJEStartAddress( string address )
		{
			if (address.IndexOf( '.' ) < 0)
				return Convert.ToInt32( address, 8 );
			else
			{
				string[] splits = address.Split( new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries );
				return Convert.ToInt32( splits[0], 8 ) * 8 + int.Parse( splits[1] );
			}
		}

		/// <summary>
		/// 根据信捷PLC的地址，解析出转换后的modbus协议信息
		/// </summary>
		/// <param name="series">PLC的系列信息</param>
		/// <param name="address">汇川plc的地址信息</param>
		/// <param name="modbusCode">原始的对应的modbus信息</param>
		/// <returns>还原后的modbus地址</returns>
		public static OperateResult<string> PraseXinJEAddress( XinJESeries series, string address, byte modbusCode )
		{
			string station = string.Empty;
			OperateResult<int> stationPara = HslHelper.ExtractParameter( ref address, "s" );
			if (stationPara.IsSuccess) station = $"s={stationPara.Content};";

			string function = string.Empty;
			OperateResult<int> functionPara = HslHelper.ExtractParameter( ref address, "x" );
			if (functionPara.IsSuccess) function = $"x={functionPara.Content};";


			if (series == XinJESeries.XC)
			{
				try
				{
					if (Regex.IsMatch( address, "^X[0-9]+", RegexOptions.IgnoreCase ) ||
						Regex.IsMatch( address, "^Y[0-9]+", RegexOptions.IgnoreCase ) ||
						Regex.IsMatch( address, "^M[0-9]+", RegexOptions.IgnoreCase ) ||
						Regex.IsMatch( address, "^S[0-9]+", RegexOptions.IgnoreCase ))
					{
						if (modbusCode == ModbusInfo.ReadRegister)
						{
							modbusCode = ModbusInfo.ReadCoil;
							function = "x=1;";
						}
					}
				}
				catch
				{

				}
				return PraseXinJEXCAddress( station + function, address, modbusCode );
			}
			else
			{
				try
				{
					if (Regex.IsMatch( address, "^X[0-9]+",   RegexOptions.IgnoreCase) ||
						Regex.IsMatch( address, "^Y[0-9]+",   RegexOptions.IgnoreCase ) ||
						Regex.IsMatch( address, "^M[0-9]+",   RegexOptions.IgnoreCase ) ||
						Regex.IsMatch( address, "^S[0-9]+",   RegexOptions.IgnoreCase) ||
						Regex.IsMatch( address, "^SEM[0-9]+", RegexOptions.IgnoreCase ) ||
						Regex.IsMatch( address, "^HSC[0-9]+", RegexOptions.IgnoreCase ) ||
						Regex.IsMatch( address, "^SM[0-9]+",  RegexOptions.IgnoreCase ) ||
						Regex.IsMatch( address, "^ET[0-9]+",  RegexOptions.IgnoreCase ) ||
						Regex.IsMatch( address, "^HM[0-9]+",  RegexOptions.IgnoreCase ) ||
						Regex.IsMatch( address, "^HS[0-9]+",  RegexOptions.IgnoreCase ) ||
						Regex.IsMatch( address, "^HT[0-9]+",  RegexOptions.IgnoreCase ) ||
						Regex.IsMatch( address, "^HC[0-9]+",  RegexOptions.IgnoreCase ))
					{
						if (modbusCode == ModbusInfo.ReadRegister)
						{
							modbusCode = ModbusInfo.ReadCoil;
							function = "x=1;";
						}
					}
				}
				catch
				{

				}
				return PraseXinJEXD1XD2XD3XL1XL3Address( station + function, address, modbusCode );
			}
		}

		/// <summary>
		/// 根据信捷PLC的地址，解析出转换后的modbus协议信息，适用XC系列
		/// </summary>
		/// <param name="station">站号的特殊指定信息，可以为空</param>
		/// <param name="address">信捷plc的地址信息</param>
		/// <param name="modbusCode">原始的对应的modbus信息</param>
		/// <returns>还原后的modbus地址</returns>
		public static OperateResult<string> PraseXinJEXCAddress( string station, string address, byte modbusCode )
		{
			try
			{
				if (modbusCode == ModbusInfo.ReadCoil || modbusCode == ModbusInfo.ReadDiscrete || modbusCode == ModbusInfo.WriteCoil || modbusCode == ModbusInfo.WriteOneCoil)
				{
					if (address.StartsWith( "X" ) || address.StartsWith( "x" ))
						return OperateResult.CreateSuccessResult( station + (CalculateXinJEStartAddress( address.Substring( 1 ) ) + 0x4000).ToString( ) );
					else if (address.StartsWith( "Y" ) || address.StartsWith( "y" ))
						return OperateResult.CreateSuccessResult( station + (CalculateXinJEStartAddress( address.Substring( 1 ) ) + 0x4800).ToString( ) );
					else if (address.StartsWith( "S" ) || address.StartsWith( "s" ))
						return OperateResult.CreateSuccessResult( station + (Convert.ToInt32( address.Substring( 1 ) ) + 0x5000).ToString( ) );
					else if (address.StartsWith( "T" ) || address.StartsWith( "t" ))
						return OperateResult.CreateSuccessResult( station + (Convert.ToInt32( address.Substring( 1 ) ) + 0x6400).ToString( ) );
					else if (address.StartsWith( "C" ) || address.StartsWith( "c" ))
						return OperateResult.CreateSuccessResult( station + (Convert.ToInt32( address.Substring( 1 ) ) + 0x6C00).ToString( ) );
					else if (address.StartsWith( "M" ) || address.StartsWith( "m" ))
					{
						int add = Convert.ToInt32( address.Substring( 1 ) );
						if (add >= 8000)
							return OperateResult.CreateSuccessResult( station + (add - 8000 + 0x6000).ToString( ) );
						else
							return OperateResult.CreateSuccessResult( station + add.ToString( ) );
					}
				}
				else
				{
					if (address.StartsWith( "D" ) || address.StartsWith( "d" ))
					{
						int add = Convert.ToInt32( address.Substring( 1 ) );
						if (add >= 8000)
							return OperateResult.CreateSuccessResult( station + (add - 8000 + 0x4000).ToString( ) );
						else
							return OperateResult.CreateSuccessResult( station + add.ToString( ) );
					}
					else if (address.StartsWith( "F" ) || address.StartsWith( "f" ))
					{
						int add = Convert.ToInt32( address.Substring( 1 ) );
						if (add >= 8000)
							return OperateResult.CreateSuccessResult( station + (add - 8000 + 0x6800).ToString( ) );
						else
							return OperateResult.CreateSuccessResult( station + (add + 0x4800).ToString( ) );
					}
					else if (address.StartsWith( "E" ) || address.StartsWith( "e" ))
						return OperateResult.CreateSuccessResult( station + (Convert.ToInt32( address.Substring( 1 ) ) + 0x7000).ToString( ) );
					else if (address.StartsWith( "T" ) || address.StartsWith( "t" ))
						return OperateResult.CreateSuccessResult( station + (Convert.ToInt32( address.Substring( 1 ) ) + 0x3000).ToString( ) );
					else if (address.StartsWith( "C" ) || address.StartsWith( "c" ))
						return OperateResult.CreateSuccessResult( station + (Convert.ToInt32( address.Substring( 1 ) ) + 0x3800).ToString( ) );
				}

				return new OperateResult<string>( StringResources.Language.NotSupportedDataType );
			}
			catch (Exception ex)
			{
				return new OperateResult<string>( ex.Message );
			}
		}

		/// <summary>
		/// 解析信捷的XD1,XD2,XD3,XL1,XL3系列的PLC的Modbus地址和内部软元件的对照
		/// </summary>
		/// <remarks>适用 XD1、XD2、XD3、XL1、XL3、XD5、XDM、XDC、XD5E、XDME、XL5、XL5E、XLME, XDH 只是支持的地址范围不一样而已</remarks>
		/// <param name="station">站号的特殊指定信息，可以为空</param>
		/// <param name="address">PLC内部的软元件的地址</param>
		/// <param name="modbusCode">默认的Modbus功能码</param>
		/// <returns>解析后的Modbus地址</returns>
		public static OperateResult<string> PraseXinJEXD1XD2XD3XL1XL3Address( string station, string address, byte modbusCode )
		{
			try
			{
				if (modbusCode == ModbusInfo.ReadCoil || modbusCode == ModbusInfo.ReadDiscrete || modbusCode == ModbusInfo.WriteCoil || modbusCode == ModbusInfo.WriteOneCoil)
				{
					if (address.StartsWith( "X" ) || address.StartsWith( "x" ))
					{
						int start = CalculateXinJEStartAddress( address.Substring( 1 ) );
						if (start < 0x1000) return OperateResult.CreateSuccessResult( station + (start - 0x0000 + 0x5000).ToString( ) ); // X0 - X77
						if (start < 0x2000) return OperateResult.CreateSuccessResult( station + (start - 0x1000 + 0x5100).ToString( ) ); // X10000 - X11177  10个模块
						if (start < 0x3000) return OperateResult.CreateSuccessResult( station + (start - 0x2000 + 0x58D0).ToString( ) ); // X20000 - X20177  2个模块
						return OperateResult.CreateSuccessResult( station + (start - 0x3000 + 0x5BF0).ToString( ) ); // #1 ED
					}
					else if (address.StartsWith( "Y" ) || address.StartsWith( "y" ))
					{
						int start = CalculateXinJEStartAddress( address.Substring( 1 ) );
						if (start < 0x1000) return OperateResult.CreateSuccessResult( station + (start - 0x0000 + 0x6000).ToString( ) ); // Y0 - Y77
						if (start < 0x2000) return OperateResult.CreateSuccessResult( station + (start - 0x1000 + 0x6100).ToString( ) ); // Y10000 - Y11177  10个模块
						if (start < 0x3000) return OperateResult.CreateSuccessResult( station + (start - 0x2000 + 0x68D0).ToString( ) ); // Y20000 - Y20177  2个模块
						return OperateResult.CreateSuccessResult( station + (start - 0x3000 + 0x6BF0).ToString( ) ); // #1 ED
					}
					else if (address.StartsWith( "SEM" ) || address.StartsWith( "sem" ))
						return OperateResult.CreateSuccessResult( station + (Convert.ToInt32( address.Substring( 3 ) ) + 0xC080).ToString( ) );
					else if (address.StartsWith( "HSC" ) || address.StartsWith( "hsc" ))
						return OperateResult.CreateSuccessResult( station + (Convert.ToInt32( address.Substring( 3 ) ) + 0xE900).ToString( ) );
					else if (address.StartsWith( "SM" ) || address.StartsWith( "sm" ))
						return OperateResult.CreateSuccessResult( station + (Convert.ToInt32( address.Substring( 2 ) ) + 0x9000).ToString( ) );
					else if (address.StartsWith( "ET" ) || address.StartsWith( "et" ))
						return OperateResult.CreateSuccessResult( station + (Convert.ToInt32( address.Substring( 2 ) ) + 0xC000).ToString( ) );
					else if (address.StartsWith( "HM" ) || address.StartsWith( "hm" ))
						return OperateResult.CreateSuccessResult( station + (Convert.ToInt32( address.Substring( 2 ) ) + 0xC100).ToString( ) );
					else if (address.StartsWith( "HS" ) || address.StartsWith( "hs" ))
						return OperateResult.CreateSuccessResult( station + (Convert.ToInt32( address.Substring( 2 ) ) + 0xD900).ToString( ) );
					else if (address.StartsWith( "HT" ) || address.StartsWith( "ht" ))
						return OperateResult.CreateSuccessResult( station + (Convert.ToInt32( address.Substring( 2 ) ) + 0xE100).ToString( ) );
					else if (address.StartsWith( "HC" ) || address.StartsWith( "hc" ))
						return OperateResult.CreateSuccessResult( station + (Convert.ToInt32( address.Substring( 2 ) ) + 0xE500).ToString( ) );
					else if (address.StartsWith( "S" ) || address.StartsWith( "s" ))
						return OperateResult.CreateSuccessResult( station + (Convert.ToInt32( address.Substring( 1 ) ) + 0x7000).ToString( ) );
					else if (address.StartsWith( "T" ) || address.StartsWith( "t" ))
						return OperateResult.CreateSuccessResult( station + (Convert.ToInt32( address.Substring( 1 ) ) + 0xA000).ToString( ) );
					else if (address.StartsWith( "C" ) || address.StartsWith( "c" ))
						return OperateResult.CreateSuccessResult( station + (Convert.ToInt32( address.Substring( 1 ) ) + 0xB000).ToString( ) );
					else if (address.StartsWith( "M" ) || address.StartsWith( "m" ))
						return OperateResult.CreateSuccessResult( station + (Convert.ToInt32( address.Substring( 1 ) ) + 0x0000).ToString( ) );
				}
				else
				{
					if (address.StartsWith( "ID" ) || address.StartsWith( "id" ))
					{
						int start = Convert.ToInt32( address.Substring( 2 ) );
						if (start < 10000) return OperateResult.CreateSuccessResult( station + (start + 0x5000).ToString( ) ); // ID0 - ID99
						if (start < 20000) return OperateResult.CreateSuccessResult( station + (start - 10000 + 0x5100).ToString( ) ); // ID10000 - ID10999
						if (start < 30000) return OperateResult.CreateSuccessResult( station + (start - 20000 + 0x58D0).ToString( ) ); // ID20000 - ID20199
						return OperateResult.CreateSuccessResult( station + (start - 30000 + 0x5BF0).ToString( ) ); // ID30000 - ID30099
					}
					else if (address.StartsWith( "QD" ) || address.StartsWith( "qd" ))
					{
						int start = Convert.ToInt32( address.Substring( 2 ) );
						if (start < 10000) return OperateResult.CreateSuccessResult( station + (start + 0x6000).ToString( ) ); // QD0 - QD99
						if (start < 20000) return OperateResult.CreateSuccessResult( station + (start - 10000 + 0x6100).ToString( ) ); // QD10000 - QD10999
						if (start < 30000) return OperateResult.CreateSuccessResult( station + (start - 20000 + 0x68D0).ToString( ) ); // QD20000 - QD20199
						return OperateResult.CreateSuccessResult( station + (start - 30000 + 0x6BF0).ToString( ) ); // QD30000 - QD30099
					}
					else if (address.StartsWith( "HSCD" ) || address.StartsWith( "hscd" ))
						return OperateResult.CreateSuccessResult( station + (Convert.ToInt32( address.Substring( 4 ) ) + 0xC480).ToString( ) );
					else if (address.StartsWith( "ETD" ) || address.StartsWith( "etd" ))
						return OperateResult.CreateSuccessResult( station + (Convert.ToInt32( address.Substring( 3 ) ) + 0xA000).ToString( ) );
					else if (address.StartsWith( "HSD" ) || address.StartsWith( "hsd" ))
						return OperateResult.CreateSuccessResult( station + (Convert.ToInt32( address.Substring( 3 ) ) + 0xB880).ToString( ) );
					else if (address.StartsWith( "HTD" ) || address.StartsWith( "htd" ))
						return OperateResult.CreateSuccessResult( station + (Convert.ToInt32( address.Substring( 3 ) ) + 0xBC80).ToString( ) );
					else if (address.StartsWith( "HCD" ) || address.StartsWith( "hcd" ))
						return OperateResult.CreateSuccessResult( station + (Convert.ToInt32( address.Substring( 3 ) ) + 0xC080).ToString( ) );
					else if (address.StartsWith( "SFD" ) || address.StartsWith( "sfd" ))
						return OperateResult.CreateSuccessResult( station + (Convert.ToInt32( address.Substring( 3 ) ) + 0xE4C0).ToString( ) );
					else if (address.StartsWith( "SD" ) || address.StartsWith( "sd" ))
						return OperateResult.CreateSuccessResult( station + (Convert.ToInt32( address.Substring( 2 ) ) + 0x7000).ToString( ) );
					else if (address.StartsWith( "TD" ) || address.StartsWith( "td" ))
						return OperateResult.CreateSuccessResult( station + (Convert.ToInt32( address.Substring( 2 ) ) + 0x8000).ToString( ) );
					else if (address.StartsWith( "CD" ) || address.StartsWith( "cd" ))
						return OperateResult.CreateSuccessResult( station + (Convert.ToInt32( address.Substring( 2 ) ) + 0x9000).ToString( ) );
					else if (address.StartsWith( "HD" ) || address.StartsWith( "hd" ))
						return OperateResult.CreateSuccessResult( station + (Convert.ToInt32( address.Substring( 2 ) ) + 0xA080).ToString( ) );
					else if (address.StartsWith( "FD" ) || address.StartsWith( "fd" ))
						return OperateResult.CreateSuccessResult( station + (Convert.ToInt32( address.Substring( 2 ) ) + 0xC4C0).ToString( ) );
					else if (address.StartsWith( "FS" ) || address.StartsWith( "fs" ))
						return OperateResult.CreateSuccessResult( station + (Convert.ToInt32( address.Substring( 2 ) ) + 0xF4C0).ToString( ) );
					else if (address.StartsWith( "D" ) || address.StartsWith( "d" ))
						return OperateResult.CreateSuccessResult( station + (Convert.ToInt32( address.Substring( 1 ) ) + 0x0000).ToString( ) );
				}

				return new OperateResult<string>( StringResources.Language.NotSupportedDataType );
			}
			catch (Exception ex)
			{
				return new OperateResult<string>( ex.Message );
			}
		}



		#region Static Helper

		//internal static OperateResult<byte[]> ExtractActualData( byte[] core )
		//{
		//	if (
		//		core[1] == ModbusInfo.ReadCoil ||
		//		core[1] == ModbusInfo.ReadDiscrete ||
		//		core[1] == ModbusInfo.ReadRegister ||
		//		core[1] == ModbusInfo.ReadInputRegister ||
		//		core[1] == ModbusInfo.WriteOneCoil ||
		//		core[1] == ModbusInfo.WriteCoil ||
		//		core[1] == ModbusInfo.WriteOneRegister ||
		//		core[1] == ModbusInfo.WriteRegister ||
		//		core[1] == ModbusInfo.WriteMaskRegister)
		//		return ModbusInfo.ExtractActualData( core );
		//	else if (
		//		core[1] == 0x1E ||
		//		core[1] == 0x1F ||
		//		core[1] == 0x20 ||
		//		core[1] == 0x21)
		//	{
		//		if (core[1] > 0x80) return new OperateResult<byte[]>( core[1], StringResources.Language.UnknownError + core.ToHexString( ' ' ) );
		//		if (core.Length > 9) return OperateResult.CreateSuccessResult( core.RemoveBegin( 9 ) );
		//		return OperateResult.CreateSuccessResult( new byte[0] );
		//	}
		//	else
		//		return new OperateResult<byte[]>( $"Function not identify: [{core[1]}] source: " + core.ToHexString( ' ' ) );
		//}

		internal static OperateResult<List<byte[]>> BuildReadCommand( byte station, string address, ushort length, bool isBit )
		{
			OperateResult<XinJEAddress> read = XinJEAddress.ParseFrom( address, station );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<List<byte[]>>( read );

			return BuildReadCommand( read.Content, length, isBit );
		}

		internal static OperateResult<List<byte[]>> BuildReadCommand( XinJEAddress address, ushort length, bool isBit )
		{
			List<byte[]> array = new List<byte[]>( );
			int[] splits = SoftBasic.SplitIntegerToArray( length, isBit ? 120 * 16 : 120 );
			for (int i = 0; i < splits.Length; i++)
			{
				byte[] command = new byte[8];
				command[0] = address.Station;
				command[1] = isBit ? (byte)0x1E : (byte)0x20;
				command[2] = address.DataCode;
				command[3] = BitConverter.GetBytes( address.AddressStart )[2];
				command[4] = BitConverter.GetBytes( address.AddressStart )[1];
				command[5] = BitConverter.GetBytes( address.AddressStart )[0];
				command[6] = BitConverter.GetBytes( splits[i] )[1];
				command[7] = BitConverter.GetBytes( splits[i] )[0];

				address.AddressStart += splits[i];
				array.Add( command );
			}
			return OperateResult.CreateSuccessResult( array );
		}

		internal static OperateResult<byte[]> BuildWriteWordCommand( byte station, string address, byte[] value )
		{
			OperateResult<XinJEAddress> read = XinJEAddress.ParseFrom( address, station );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( read );

			return BuildWriteWordCommand( read.Content, value );
		}

		internal static OperateResult<byte[]> BuildWriteWordCommand( XinJEAddress address, byte[] value )
		{
			byte[] command = new byte[9 + value.Length];
			command[0] = address.Station;
			command[1] = 0x21;
			command[2] = address.DataCode;
			command[3] = BitConverter.GetBytes( address.AddressStart )[2];
			command[4] = BitConverter.GetBytes( address.AddressStart )[1];
			command[5] = BitConverter.GetBytes( address.AddressStart )[0];
			command[6] = BitConverter.GetBytes( value.Length / 2 )[1];
			command[7] = BitConverter.GetBytes( value.Length / 2 )[0];
			command[8] = (byte)value.Length;
			value.CopyTo( command, 9 );
			return OperateResult.CreateSuccessResult( command );
		}

		internal static OperateResult<byte[]> BuildWriteBoolCommand( byte station, string address, bool[] value )
		{
			OperateResult<XinJEAddress> read = XinJEAddress.ParseFrom( address, station );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( read );

			return BuildWriteBoolCommand( read.Content, value );
		}

		internal static OperateResult<byte[]> BuildWriteBoolCommand( XinJEAddress address, bool[] value )
		{
			byte[] buffer = value.ToByteArray( );
			byte[] command = new byte[9 + buffer.Length];
			command[0] = address.Station;
			command[1] = 0x1F;
			command[2] =address.DataCode;
			command[3] = BitConverter.GetBytes( address.AddressStart )[2];
			command[4] = BitConverter.GetBytes( address.AddressStart )[1];
			command[5] = BitConverter.GetBytes( address.AddressStart )[0];
			command[6] = BitConverter.GetBytes( value.Length )[1];
			command[7] = BitConverter.GetBytes( value.Length )[0];
			command[8] = (byte)buffer.Length;
			buffer.CopyTo( command, 9 );
			return OperateResult.CreateSuccessResult( command );
		}

		#endregion

		#region Read Write Helper

		internal static OperateResult<byte[]> Read( IModbus modbus, string address, ushort length, Func<string, ushort, OperateResult<byte[]>> funcRead )
		{
			OperateResult<XinJEAddress> analysis = XinJEAddress.ParseFrom( address, length, modbus.Station );
			if (!analysis.IsSuccess) return funcRead.Invoke( address, length );

			XinJEAddress xinJE = analysis.Content;
			if (xinJE.AddressStart + length <= xinJE.CriticalAddress) return funcRead.Invoke( address, length );

			List<byte> result = new List<byte>( );
			if (xinJE.AddressStart < xinJE.CriticalAddress)
			{
				OperateResult<byte[]> read = funcRead.Invoke( address, (ushort)(xinJE.CriticalAddress - xinJE.AddressStart) );
				if (!read.IsSuccess) return read;

				result.AddRange( read.Content );
				length = (ushort)(length - (xinJE.CriticalAddress - xinJE.AddressStart));
				xinJE.AddressStart = xinJE.CriticalAddress;
			}

			OperateResult<List<byte[]>> command = XinJEHelper.BuildReadCommand( xinJE, length, false );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( command );

			OperateResult<byte[]> readAgain = modbus.ReadFromCoreServer( command.Content );
			if (!readAgain.IsSuccess) return readAgain;

			result.AddRange( readAgain.Content );
			return OperateResult.CreateSuccessResult( result.ToArray( ) );
		}

		internal static OperateResult Write( IModbus modbus, string address, byte[] value, Func<string, byte[], OperateResult> funcWrite )
		{
			OperateResult<XinJEAddress> analysis = XinJEAddress.ParseFrom( address, modbus.Station );
			if (!analysis.IsSuccess) return funcWrite.Invoke( address, value );

			XinJEAddress xinJE = analysis.Content;
			if ((xinJE.AddressStart + value.Length / 2) <= xinJE.CriticalAddress) return funcWrite.Invoke( address, value );

			if (xinJE.AddressStart < xinJE.CriticalAddress)
			{
				OperateResult write = funcWrite.Invoke( address, value.SelectBegin( (xinJE.CriticalAddress - xinJE.AddressStart) * 2 ) );
				if (!write.IsSuccess) return write;

				value = value.RemoveBegin( (xinJE.CriticalAddress - xinJE.AddressStart) * 2 );
				xinJE.AddressStart = xinJE.CriticalAddress;
			}

			OperateResult<byte[]> command = XinJEHelper.BuildWriteWordCommand( xinJE, value );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( command );

			return modbus.ReadFromCoreServer( command.Content );
		}

		internal static OperateResult Write( IModbus modbus, string address, short value, Func<string, short, OperateResult> funcWrite )
		{
			OperateResult<XinJEAddress> analysis = XinJEAddress.ParseFrom( address, modbus.Station );
			if (!analysis.IsSuccess) return funcWrite.Invoke( address, value );

			XinJEAddress xinJE = analysis.Content;
			if (xinJE.AddressStart < xinJE.CriticalAddress)
				return funcWrite.Invoke( address, value );
			else
				return modbus.Write( address, modbus.ByteTransform.TransByte( value ) );
		}

		internal static OperateResult Write( IModbus modbus, string address, ushort value, Func<string, ushort, OperateResult> funcWrite )
		{
			OperateResult<XinJEAddress> analysis = XinJEAddress.ParseFrom( address, modbus.Station );
			if (!analysis.IsSuccess) return funcWrite.Invoke( address, value );

			XinJEAddress xinJE = analysis.Content;
			if (xinJE.AddressStart < xinJE.CriticalAddress)
				return funcWrite.Invoke( address, value );
			else
				return modbus.Write( address, modbus.ByteTransform.TransByte( value ) );
		}

		internal static OperateResult<bool[]> ReadBool( IModbus modbus, string address, ushort length, Func<string, ushort, OperateResult<bool[]>> funcRead )
		{
			OperateResult<XinJEAddress> analysis = XinJEAddress.ParseFrom( address, length, modbus.Station );
			if (!analysis.IsSuccess) return funcRead.Invoke( address, length );

			XinJEAddress xinJE = analysis.Content;
			if (xinJE.AddressStart + length <= xinJE.CriticalAddress) return funcRead.Invoke( address, length );

			List<bool> result = new List<bool>( );
			if (xinJE.AddressStart < xinJE.CriticalAddress)
			{
				OperateResult<bool[]> read = funcRead.Invoke( address, (ushort)(xinJE.CriticalAddress - xinJE.AddressStart) );
				if (!read.IsSuccess) return read;

				result.AddRange( read.Content );
				length = (ushort)(length - (xinJE.CriticalAddress - xinJE.AddressStart));
				xinJE.AddressStart = xinJE.CriticalAddress;
			}

			OperateResult<List<byte[]>> command = XinJEHelper.BuildReadCommand( xinJE, length, true );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( command );

			OperateResult<byte[]> readAgain = modbus.ReadFromCoreServer( command.Content );
			if (!readAgain.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( readAgain );

			result.AddRange( readAgain.Content.ToBoolArray( ).SelectBegin( length ) );
			return OperateResult.CreateSuccessResult( result.ToArray( ) );
		}

		internal static OperateResult Write( IModbus modbus, string address, bool[] values, Func<string, bool[], OperateResult> funcWrite )
		{
			OperateResult<XinJEAddress> analysis = XinJEAddress.ParseFrom( address, modbus.Station );
			if (!analysis.IsSuccess) return funcWrite.Invoke( address, values );

			XinJEAddress xinJE = analysis.Content;
			if ((xinJE.AddressStart + values.Length) <= xinJE.CriticalAddress) return funcWrite.Invoke( address, values );

			if (xinJE.AddressStart < xinJE.CriticalAddress)
			{
				OperateResult write = funcWrite.Invoke( address, values.SelectBegin( xinJE.CriticalAddress - xinJE.AddressStart ) );
				if (!write.IsSuccess) return write;

				values = values.RemoveBegin( xinJE.CriticalAddress - xinJE.AddressStart );
				xinJE.AddressStart = xinJE.CriticalAddress;
			}

			OperateResult<byte[]> command = XinJEHelper.BuildWriteBoolCommand( xinJE, values );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( command );

			return modbus.ReadFromCoreServer( command.Content );
		}

		internal static OperateResult Write( IModbus modbus, string address, bool value, Func<string, bool, OperateResult> funcWrite )
		{
			OperateResult<XinJEAddress> analysis = XinJEAddress.ParseFrom( address, modbus.Station );
			if (!analysis.IsSuccess) return funcWrite.Invoke( address, value );

			XinJEAddress xinJE = analysis.Content;
			if (xinJE.AddressStart < xinJE.CriticalAddress)
				return funcWrite.Invoke( address, value );
			else
				return modbus.Write( address, new bool[] { value } );
		}

#if !NET35 && !NET20
		internal async static Task<OperateResult<byte[]>> ReadAsync( IModbus modbus, string address, ushort length, Func<string, ushort, Task<OperateResult<byte[]>>> funcRead )
		{
			OperateResult<XinJEAddress> analysis = XinJEAddress.ParseFrom( address, modbus.Station );
			if (!analysis.IsSuccess) return await funcRead.Invoke( address, length );

			XinJEAddress xinJE = analysis.Content;
			if (xinJE.AddressStart + length <= xinJE.CriticalAddress) return await funcRead.Invoke( address, length );

			List<byte> result = new List<byte>( );
			if (xinJE.AddressStart < xinJE.CriticalAddress)
			{
				OperateResult<byte[]> read = await funcRead.Invoke( address, (ushort)(xinJE.CriticalAddress - xinJE.AddressStart) );
				if (!read.IsSuccess) return read;

				result.AddRange( read.Content );
				length = (ushort)(length - (xinJE.CriticalAddress - xinJE.AddressStart));
				xinJE.AddressStart = xinJE.CriticalAddress;
			}

			OperateResult<List<byte[]>> command = XinJEHelper.BuildReadCommand( analysis.Content, length, false );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( command );

			OperateResult<byte[]> readAgain = await modbus.ReadFromCoreServerAsync( command.Content );
			if (!readAgain.IsSuccess) return readAgain;

			result.AddRange( readAgain.Content );
			return OperateResult.CreateSuccessResult( result.ToArray( ) );
		}

		internal async static Task<OperateResult> WriteAsync( IModbus modbus, string address, byte[] value, Func<string, byte[], Task<OperateResult>> funcWrite )
		{
			OperateResult<XinJEAddress> analysis = XinJEAddress.ParseFrom( address, modbus.Station );
			if (!analysis.IsSuccess) return await funcWrite.Invoke( address, value );

			XinJEAddress xinJE = analysis.Content;
			if ((xinJE.AddressStart + value.Length / 2) <= xinJE.CriticalAddress) return await funcWrite.Invoke( address, value );

			if (xinJE.AddressStart < xinJE.CriticalAddress)
			{
				OperateResult write = await funcWrite.Invoke( address, value.SelectBegin( (xinJE.CriticalAddress - xinJE.AddressStart) * 2 ) );
				if (!write.IsSuccess) return write;

				value = value.RemoveBegin( (xinJE.CriticalAddress - xinJE.AddressStart) * 2 );
				xinJE.AddressStart = xinJE.CriticalAddress;
			}

			OperateResult<byte[]> command = XinJEHelper.BuildWriteWordCommand( xinJE, value );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( command );

			return await modbus.ReadFromCoreServerAsync( command.Content );
		}

		internal static async Task<OperateResult> WriteAsync( IModbus modbus, string address, short value, Func<string, short, Task<OperateResult>> funcWrite )
		{
			OperateResult<XinJEAddress> analysis = XinJEAddress.ParseFrom( address, modbus.Station );
			if (!analysis.IsSuccess) return await funcWrite.Invoke( address, value );

			XinJEAddress xinJE = analysis.Content;
			if (xinJE.AddressStart < xinJE.CriticalAddress)
				return await funcWrite.Invoke( address, value );
			else
				return await modbus.WriteAsync( address, modbus.ByteTransform.TransByte( value ) );
		}

		internal static async Task<OperateResult> WriteAsync( IModbus modbus, string address, ushort value, Func<string, ushort, Task<OperateResult>> funcWrite )
		{
			OperateResult<XinJEAddress> analysis = XinJEAddress.ParseFrom( address, modbus.Station );
			if (!analysis.IsSuccess) return await funcWrite.Invoke( address, value );

			XinJEAddress xinJE = analysis.Content;
			if (xinJE.AddressStart < xinJE.CriticalAddress)
				return await funcWrite.Invoke( address, value );
			else
				return await modbus.WriteAsync( address, modbus.ByteTransform.TransByte( value ) );
		}

		internal async static Task<OperateResult<bool[]>> ReadBoolAsync( IModbus modbus, string address, ushort length, Func<string, ushort, Task<OperateResult<bool[]>>> funcRead )
		{
			OperateResult<XinJEAddress> analysis = XinJEAddress.ParseFrom( address, length, modbus.Station );
			if (!analysis.IsSuccess) return await funcRead.Invoke( address, length );

			XinJEAddress xinJE = analysis.Content;
			if (xinJE.AddressStart + length <= xinJE.CriticalAddress) return await funcRead.Invoke( address, length );

			List<bool> result = new List<bool>( );
			if (xinJE.AddressStart < xinJE.CriticalAddress)
			{
				OperateResult<bool[]> read = await funcRead.Invoke( address, (ushort)(xinJE.CriticalAddress - xinJE.AddressStart) );
				if (!read.IsSuccess) return read;

				result.AddRange( read.Content );
				length = (ushort)(length - (xinJE.CriticalAddress - xinJE.AddressStart));
				xinJE.AddressStart = xinJE.CriticalAddress;
			}

			OperateResult<List<byte[]>> command = XinJEHelper.BuildReadCommand( xinJE, length, true );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( command );

			OperateResult<byte[]> readAgain = await modbus.ReadFromCoreServerAsync( command.Content );
			if (!readAgain.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( readAgain );

			result.AddRange( readAgain.Content.ToBoolArray( ).SelectBegin( length ) );
			return OperateResult.CreateSuccessResult( result.ToArray( ) );
		}

		internal async static Task<OperateResult> WriteAsync( IModbus modbus, string address, bool[] values, Func<string, bool[], Task<OperateResult>> funcWrite )
		{
			OperateResult<XinJEAddress> analysis = XinJEAddress.ParseFrom( address, modbus.Station );
			if (!analysis.IsSuccess) return await funcWrite.Invoke( address, values );

			XinJEAddress xinJE = analysis.Content;
			if ((xinJE.AddressStart + values.Length) <= xinJE.CriticalAddress) return await funcWrite.Invoke( address, values );

			if (xinJE.AddressStart < xinJE.CriticalAddress)
			{
				OperateResult write = await funcWrite.Invoke( address, values.SelectBegin( xinJE.CriticalAddress - xinJE.AddressStart ) );
				if (!write.IsSuccess) return write;

				values = values.RemoveBegin( xinJE.CriticalAddress - xinJE.AddressStart );
				xinJE.AddressStart = xinJE.CriticalAddress;
			}

			OperateResult<byte[]> command = XinJEHelper.BuildWriteBoolCommand( xinJE, values );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( command );

			return await modbus.ReadFromCoreServerAsync( command.Content );
		}

		internal async static Task<OperateResult> WriteAsync( IModbus modbus, string address, bool value, Func<string, bool, Task<OperateResult>> funcWrite )
		{
			OperateResult<XinJEAddress> analysis = XinJEAddress.ParseFrom( address, modbus.Station );
			if (!analysis.IsSuccess) return await funcWrite.Invoke( address, value );

			XinJEAddress xinJE = analysis.Content;
			if (xinJE.AddressStart < xinJE.CriticalAddress)
				return await funcWrite.Invoke( address, value );
			else
				return await modbus.WriteAsync( address, new bool[] { value } );
		}
#endif
		#endregion

	}
}
