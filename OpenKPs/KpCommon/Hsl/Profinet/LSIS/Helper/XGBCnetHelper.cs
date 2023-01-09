using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HslCommunication.BasicFramework;
using HslCommunication.Core;
using System.IO;
#if !NET20 && !NET35
using System.Threading.Tasks;
#endif

namespace HslCommunication.Profinet.LSIS.Helper
{
	/// <summary>
	/// Cnet的辅助类
	/// </summary>
	public class XGBCnetHelper
	{
		/// <summary>
		/// 根据错误号，获取到真实的错误描述信息<br />
		/// According to the error number, get the real error description information
		/// </summary>
		/// <param name="err">错误号</param>
		/// <returns>真实的错误描述信息</returns>
		public static string GetErrorText( int err )
		{
			switch (err)
			{
				case 0x0003: return StringResources.Language.LsisCnet0003;
				case 0x0004: return StringResources.Language.LsisCnet0004;
				case 0x0007: return StringResources.Language.LsisCnet0007;
				case 0x0011: return StringResources.Language.LsisCnet0011;
				case 0x0090: return StringResources.Language.LsisCnet0090;
				case 0x0190: return StringResources.Language.LsisCnet0190;
				case 0x0290: return StringResources.Language.LsisCnet0290;
				case 0x1132: return StringResources.Language.LsisCnet1132;
				case 0x1232: return StringResources.Language.LsisCnet1232;
				case 0x1234: return StringResources.Language.LsisCnet1234;
				case 0x1332: return StringResources.Language.LsisCnet1332;
				case 0x1432: return StringResources.Language.LsisCnet1432;
				case 0x7132: return StringResources.Language.LsisCnet7132;
				default: return StringResources.Language.UnknownError;
			}
		}

		/// <inheritdoc cref="HslCommunication.Core.Net.NetworkDoubleBase.UnpackResponseContent(byte[], byte[])"/>
		public static OperateResult<byte[]> UnpackResponseContent( byte[] send, byte[] response )
		{
			try
			{
				if (response[0] == AsciiControl.ACK)
				{
					// ACK
					if (response[3] == 0x57 || response[3] == 0x77)             // write
						return OperateResult.CreateSuccessResult( response );
					string cmd = Encoding.ASCII.GetString( response, 4, 2 );
					if (cmd == "SS")
					{
						int blocks = Convert.ToInt32( Encoding.ASCII.GetString( response, 6, 2 ), 16 );
						int byteIndex = 8;
						List<byte> array = new List<byte>( );

						for (int i = 0; i < blocks; i++)
						{
							int count = Convert.ToInt32( Encoding.ASCII.GetString( response, byteIndex, 2 ), 16 );
							array.AddRange( Encoding.ASCII.GetString( response, byteIndex + 2, count * 2 ).ToHexBytes( ) );
							byteIndex += 2 + count * 2;
						}
						return OperateResult.CreateSuccessResult( array.ToArray( ) );
					}
					else if (cmd == "SB")
					{
						int count = Convert.ToInt32( Encoding.ASCII.GetString( response, 8, 2 ), 16 );
						byte[] buffer = Encoding.ASCII.GetString( response, 10, count * 2 ).ToHexBytes( );
						return OperateResult.CreateSuccessResult( buffer );
					}
					else
					{
						return new OperateResult<byte[]>( 1, "Command Wrong:" + cmd + Environment.NewLine + "Source: " + response.ToHexString( ) );
					}
				}
				else if (response[0] == AsciiControl.NAK)
				{
					// NAK
					int err = Convert.ToInt32( Encoding.ASCII.GetString( response, 6, 4 ), 16 );
					return new OperateResult<byte[]>( err, GetErrorText( err ) );
				}
				else
				{
					return new OperateResult<byte[]>( response[0], "Source: " + SoftBasic.GetAsciiStringRender( response ) );
				}
			}
			catch (Exception ex)
			{
				return new OperateResult<byte[]>( 1, "Wrong:" + ex.Message + Environment.NewLine + "Source: " + response.ToHexString( ) );
			}
		}

		private const string CnetTypes = "PMLKFTCDSQINUZR";

		/// <summary>
		/// 从输入的地址里解析出真实的可以直接放到协议的地址信息，如果是X的地址，自动转换带小数点的表示方式到位地址，如果是其他类型地址，则一律统一转化为字节为单位的地址<br />
		/// The real address information that can be directly put into the protocol is parsed from the input address. If it is the address of X, 
		/// it will automatically convert the representation with a decimal point to the address. If it is an address of other types, it will be uniformly converted into a unit of bytes. address
		/// </summary>
		/// <param name="address">输入的起始偏移地址</param>
		/// <param name="transBit">是否转换为bool地址</param>
		/// <returns>analysis result</returns>
		public static OperateResult<string> AnalysisAddress( string address, bool transBit = false )
		{
			// Bit: P,M,L,K,F,T
			// Continus P,M,L,K,F,T,C,D,S
			StringBuilder sb = new StringBuilder( );
			try
			{
				if (!CnetTypes.Contains( address[0] )) return new OperateResult<string>( StringResources.Language.NotSupportedDataType );
				sb.Append( "%" );
				sb.Append( address[0] );

				if (address[1] == 'X')
				{
					sb.Append( "X" );
					if (transBit & address.IndexOf( '.' ) > 0)
					{
						int bitIndex = HslHelper.GetBitIndexInformation( ref address );
						sb.Append( address.Substring( 2 ) );
						sb.Append( bitIndex.ToString( "X1" ) );
					}
					else
					{
						sb.Append( address.Substring( 2 ) );
					}
				}
				else
				{
					sb.Append( transBit ? "X" : "B" );
					int index = 0;
					int bitIndex = 0;
					if (transBit & address.IndexOf( '.' ) > 0) bitIndex = HslHelper.GetBitIndexInformation( ref address );

					if      (address[1] == 'B') index = Convert.ToInt32( address.Substring( 2 ) );
					else if (address[1] == 'W') index = Convert.ToInt32( address.Substring( 2 ) ) * 2;
					else if (address[1] == 'D') index = Convert.ToInt32( address.Substring( 2 ) ) * 4;
					else if (address[1] == 'L') index = Convert.ToInt32( address.Substring( 2 ) ) * 8;
					else index = Convert.ToInt32( address.Substring( 1 ) );

					if (transBit)
					{
						sb.Append( index / 2 );
						sb.Append( bitIndex.ToString( "X1" ) );
					}
					else
					{
						sb.Append( index );
					}
				}
			}
			catch (Exception ex)
			{
				return new OperateResult<string>( ex.Message );
			}

			return OperateResult.CreateSuccessResult( sb.ToString( ) );
		}

		/// <summary>
		/// 往现有的命令数据中增加BCC的内容
		/// </summary>
		/// <param name="command">现有的命令</param>
		private static void AddBccTail( List<byte> command )
		{
			int sum = 0;
			for (int i = 0; i < command.Count; i++)
			{
				sum += command[i];
			}
			command.AddRange( SoftBasic.BuildAsciiBytesFrom( (byte)sum ) );
		}

		/// <summary>
		/// reading address  Type of ReadByte
		/// </summary>
		/// <param name="station">plc station</param>
		/// <param name="address">address, for example: M100, D100, DW100</param>
		/// <param name="length">read length</param>
		/// <returns>command bytes</returns>
		public static OperateResult<byte[]> BuildReadByteCommand( byte station, string address, ushort length )
		{
			var analysisResult = AnalysisAddress( address );
			if (!analysisResult.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( analysisResult );

			List<byte> command = new List<byte>( );
			command.Add( AsciiControl.ENQ );    // ENQ
			command.AddRange( SoftBasic.BuildAsciiBytesFrom( station ) );
			command.Add( 0x72 );                // command r
			command.Add( 0x53 );                // command type: SB
			command.Add( 0x42 );
			command.AddRange( SoftBasic.BuildAsciiBytesFrom( (byte)analysisResult.Content.Length ) );
			command.AddRange( Encoding.ASCII.GetBytes( analysisResult.Content ) );
			command.AddRange( SoftBasic.BuildAsciiBytesFrom( (byte)length ) );
			command.Add( AsciiControl.EOT );    // EOT
			AddBccTail( command );              // BCC

			return OperateResult.CreateSuccessResult( command.ToArray( ) );
		}

		/// <inheritdoc cref="BuildReadIndividualCommand(byte, string[])"/>
		public static OperateResult<byte[]> BuildReadIndividualCommand( byte station, string address ) => BuildReadIndividualCommand( station, new string[] { address } );

		/// <summary>
		/// Multi reading address Type of Read Individual
		/// </summary>
		/// <param name="station">plc station</param>
		/// <param name="addresses">address, for example: MX100, PX100</param>
		/// <returns></returns>
		public static OperateResult<byte[]> BuildReadIndividualCommand( byte station, string[] addresses )
		{
			List<byte> command = new List<byte>( );
			command.Add( AsciiControl.ENQ );    // ENQ
			command.AddRange( SoftBasic.BuildAsciiBytesFrom( station ) );
			command.Add( 0x72 );                // command r
			command.Add( 0x53 );                // command type: SS
			command.Add( 0x53 );
			command.AddRange( SoftBasic.BuildAsciiBytesFrom( (byte)addresses.Length ) );    // Number of blocks

			if(addresses.Length > 1)
			{
				foreach (var address in addresses)
				{
					string add = address.StartsWith( "%" ) ? address : "%" + address;

					command.AddRange( SoftBasic.BuildAsciiBytesFrom( (byte)add.Length ) );
					command.AddRange( Encoding.ASCII.GetBytes( add ) );
				}
			}
			else
			{
				foreach (var address in addresses)
				{
					var analysisResult = AnalysisAddress( address );
					if (!analysisResult.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( analysisResult );

					command.AddRange( SoftBasic.BuildAsciiBytesFrom( (byte)analysisResult.Content.Length ) );
					command.AddRange( Encoding.ASCII.GetBytes( analysisResult.Content ) );
				}
			}

			command.Add( AsciiControl.EOT );    // EOT
			AddBccTail( command );              // BCC

			return OperateResult.CreateSuccessResult( command.ToArray( ) );
		}

		/// <summary>
		/// build read command. 
		/// </summary>
		/// <param name="station">station</param>
		/// <param name="address">start address</param>
		/// <param name="length">address length</param>
		/// <returns> command</returns>
		public static OperateResult<byte[]> BuildReadCommand( byte station, string address, ushort length )
		{
			var DataTypeResult = XGBFastEnet.GetDataTypeToAddress( address );
			if (!DataTypeResult.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( DataTypeResult );

			switch (DataTypeResult.Content)
			{
				case "Bit": return BuildReadIndividualCommand( station, address );
				case "Word":
				case "DWord":
				case "LWord":
				case "Continuous": return BuildReadByteCommand( station, address, length );
				default: return new OperateResult<byte[]>( StringResources.Language.NotSupportedDataType );
			}
		}

		/// <summary>
		/// write data to address  Type of ReadByte
		/// </summary>
		/// <param name="station">plc station</param>
		/// <param name="address">address, for example: M100, D100, DW100</param>
		/// <param name="value">source value</param>
		/// <returns>command bytes</returns>
		public static OperateResult<byte[]> BuildWriteByteCommand( byte station, string address, byte[] value )
		{
			var analysisResult = AnalysisAddress( address );
			if (!analysisResult.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( analysisResult );
			List<byte> command = new List<byte>( );
			command.Add( AsciiControl.ENQ );    // ENQ
			command.AddRange( SoftBasic.BuildAsciiBytesFrom( station ) );
			command.Add( 0x77 );                // command w
			command.Add( 0x53 );                // command type: S
			command.Add( 0x42 );                // command type: B
			command.AddRange( SoftBasic.BuildAsciiBytesFrom( (byte)analysisResult.Content.Length ) );
			command.AddRange( Encoding.ASCII.GetBytes( analysisResult.Content ) );
			command.AddRange( SoftBasic.BuildAsciiBytesFrom( (byte)value.Length ) );
			command.AddRange( SoftBasic.BytesToAsciiBytes( value ) );
			command.Add( AsciiControl.EOT );    // EOT
			AddBccTail( command );              // BCC

			return OperateResult.CreateSuccessResult( command.ToArray( ) );
		}
		/// <summary>
		/// write data to address  Type of One
		/// </summary>
		/// <param name="station">plc station</param>
		/// <param name="address">address, for example: M100, D100, DW100</param>
		/// <param name="value">source value</param>
		/// <returns>command bytes</returns>
		public static OperateResult<byte[]> BuildWriteOneCommand( byte station, string address, byte[] value )
		{
			var analysisResult = AnalysisAddress( address );
			if (!analysisResult.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( analysisResult );

			List<byte> command = new List<byte>( );
			command.Add( AsciiControl.ENQ );    // ENQ
			command.AddRange( SoftBasic.BuildAsciiBytesFrom( station ) );
			command.Add( 0x77 );                // command w
			command.Add( 0x53 );                // command type: S
			command.Add( 0x53 );                // command type: S
			command.Add( 0x30 );                // Number of blocks
			command.Add( 0x31 );
			command.AddRange( SoftBasic.BuildAsciiBytesFrom( (byte)analysisResult.Content.Length ) );
			command.AddRange( Encoding.ASCII.GetBytes( analysisResult.Content ) );
			command.AddRange( SoftBasic.BytesToAsciiBytes( value ) );
			command.Add( AsciiControl.EOT );    // EOT
			AddBccTail( command );              // BCC

			return OperateResult.CreateSuccessResult( command.ToArray( ) );
		}

		/// <summary>
		/// write data to address  Type of ReadByte
		/// </summary>
		/// <param name="station">plc station</param>
		/// <param name="address">address, for example: M100, D100, DW100</param>
		/// <param name="value">source value</param>
		/// <returns>command bytes</returns>
		public static OperateResult<byte[]> BuildWriteCommand( byte station, string address, byte[] value )
		{
			var DataTypeResult = XGBFastEnet.GetDataTypeToAddress( address );
			if (!DataTypeResult.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( DataTypeResult );

			switch (DataTypeResult.Content)
			{
				case "Bit": return BuildWriteOneCommand( station, address, value );
				case "Word":
				case "DWord":
				case "LWord":
				case "Continuous": return BuildWriteByteCommand( station, address, value );
				default: return new OperateResult<byte[]>( StringResources.Language.NotSupportedDataType );
			}
		}

		/// <summary>
		/// 从PLC的指定地址读取原始的字节数据信息，地址示例：MB100, MW100, MD100, 如果输入了M100等同于MB100<br />
		/// Read the original byte data information from the designated address of the PLC. 
		/// Examples of addresses: MB100, MW100, MD100, if the input M100 is equivalent to MB100
		/// </summary>
		/// <remarks>
		/// 地址类型支持 P,M,L,K,F,T,C,D,R,I,Q,W, 支持携带站号的形式，例如 s=2;MW100
		/// </remarks>
		/// <param name="plc">PLC通信对象</param>
		/// <param name="station">站号信息</param>
		/// <param name="address">PLC的地址信息，例如 M100, MB100, MW100, MD100</param>
		/// <param name="length">读取的长度信息</param>
		/// <returns>返回是否读取成功的结果对象</returns>
		public static OperateResult<byte[]> Read( IReadWriteDevice plc, int station, string address, ushort length )
		{
			byte stat = (byte)HslHelper.ExtractParameter( ref address, "s", station );

			OperateResult<byte[]> command = BuildReadCommand( stat, address, length );
			if (!command.IsSuccess) return command;

			return plc.ReadFromCoreServer( command.Content );
		}

		/// <summary>
		/// 从PLC设备读取多个地址的数据信息，返回连续的字节数组，需要按照实际情况进行按顺序解析。<br />
		/// Read the data information of multiple addresses from the PLC device and return a continuous byte array, which needs to be parsed in order according to the actual situation.
		/// </summary>
		/// <remarks>按照每16个地址长度进行自动的切割，支持任意的多的长度地址</remarks>
		/// <param name="plc">PLC通信对象</param>
		/// <param name="station">站号信息</param>
		/// <param name="address">PLC的地址信息，例如 M100, MB100, MW100, MD100</param>
		/// <returns>结果对象数据</returns>
		public static OperateResult<byte[]> Read( IReadWriteDevice plc, int station, string[] address )
		{
			List<string[]> list = SoftBasic.ArraySplitByLength( address, 16 );
			List<byte> result = new List<byte>( 32 );

			for (int i = 0; i < list.Count; i++)
			{
				OperateResult<byte[]> command = BuildReadIndividualCommand( (byte)station, list[i] );
				if (!command.IsSuccess) return command;

				OperateResult<byte[]> read = plc.ReadFromCoreServer( command.Content );
				if (!read.IsSuccess) return read;

				result.AddRange( read.Content );
			}
			return OperateResult.CreateSuccessResult( result.ToArray( ) );
		}

		/// <summary>
		/// 将原始数据写入到PLC的指定的地址里，地址示例：MB100, MW100, MD100, 如果输入了M100等同于MB100<br />
		/// Write the original data to the designated address of the PLC. 
		/// Examples of addresses: MB100, MW100, MD100, if input M100 is equivalent to MB100
		/// </summary>
		/// <param name="plc">PLC通信对象</param>
		/// <param name="station">站号信息</param>
		/// <param name="address">PLC的地址信息，例如 M100, MB100, MW100, MD100</param>
		/// <param name="value">等待写入的原始数据内容</param>
		/// <returns>是否写入成功</returns>
		public static OperateResult Write( IReadWriteDevice plc, int station, string address, byte[] value )
		{
			byte stat = (byte)HslHelper.ExtractParameter( ref address, "s", station );

			OperateResult<byte[]> command = BuildWriteCommand( stat, address, value );
			if (!command.IsSuccess) return command;

			return plc.ReadFromCoreServer( command.Content );
		}

		/// <summary>
		/// 从PLC的指定地址读取原始的位数据信息，地址示例：MX100, MX10A<br />
		/// Read the original bool data information from the designated address of the PLC. 
		/// Examples of addresses: MX100, MX10A
		/// </summary>
		/// <remarks>
		/// 地址类型支持 P,M,L,K,F,T,C,D,R,I,Q,W, 支持携带站号的形式，例如 s=2;MX100
		/// </remarks>
		/// <param name="plc">PLC通信对象</param>
		/// <param name="station">站号信息</param>
		/// <param name="address">PLC的地址信息，例如 MX100, MX10A</param>
		/// <returns>返回是否读取成功的结果对象</returns>
		public static OperateResult<bool> ReadBool( IReadWriteDevice plc, int station, string address )
		{
			byte stat = (byte)HslHelper.ExtractParameter( ref address, "s", station );
			int bitIndex = HslHelper.GetBitIndexInformation( ref address );

			OperateResult<byte[]> command = BuildReadIndividualCommand( stat, address );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<bool>( command );

			OperateResult<byte[]> read = plc.ReadFromCoreServer( command.Content );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool>( read );

			return OperateResult.CreateSuccessResult( read.Content.ToBoolArray( )[bitIndex] );
		}

		/// <summary>
		/// 从PLC的指定地址读取原始的位数据信息，地址示例：MB100.0, MW100.0<br />
		/// Read the original bool data information from the designated address of the PLC. 
		/// Examples of addresses: MB100.0, MW100.0
		/// </summary>
		/// <remarks>
		/// 地址类型支持 P,M,L,K,F,T,C,D,R,I,Q,W, 支持携带站号的形式，例如 s=2;MB100.0
		/// </remarks>
		/// <param name="plc">PLC通信对象</param>
		/// <param name="station">站号信息</param>
		/// <param name="address">PLC的地址信息，例如 MB100.0, MW100.0</param>
		/// <param name="length">读取的长度信息</param>
		/// <returns>返回是否读取成功的结果对象</returns>
		public static OperateResult<bool[]> ReadBool( IReadWriteDevice plc, int station, string address, ushort length )
		{
			int bitIndex = HslHelper.GetBitIndexInformation( ref address );

			int byteLength = HslHelper.CalculateOccupyLength( bitIndex, length );
			OperateResult<byte[]> read = Read( plc, station, address, (ushort)byteLength );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( read );

			return OperateResult.CreateSuccessResult( read.Content.ToBoolArray( ).SelectMiddle( bitIndex, length ) );
		}

		/// <summary>
		/// 将bool数据写入到PLC的指定的地址里，地址示例：MX100, MX10A<br />
		/// Write the bool data to the designated address of the PLC. Examples of addresses: MX100, MX10A
		/// </summary>
		/// <remarks>
		/// 地址类型支持 P,M,L,K,F,T,C,D,R,I,Q,W, 支持携带站号的形式，例如 s=2;MX100
		/// </remarks>
		/// <param name="plc">PLC通信对象</param>
		/// <param name="station">站号信息</param>
		/// <param name="address">PLC的地址信息，例如 MX100, MX10A</param>
		/// <param name="value">bool值信息</param>
		/// <returns>返回是否读取成功的结果对象</returns>
		public static OperateResult Write( IReadWriteDevice plc, int station, string address, bool value )
		{
			byte stat = (byte)HslHelper.ExtractParameter( ref address, "s", station );
			OperateResult<string> analysis = AnalysisAddress( address, true );
			if (!analysis.IsSuccess) return analysis;

			OperateResult<byte[]> command = BuildWriteOneCommand( stat, analysis.Content.Substring( 1 ), new byte[] { (byte)(value ? 0x01 : 0x00) } );
			if (!command.IsSuccess) return command;

			return plc.ReadFromCoreServer( command.Content );
		}

#if !NET20 && !NET35
		/// <inheritdoc cref="Read(IReadWriteDevice, int, string, ushort)"/>
		public static async Task<OperateResult<byte[]>> ReadAsync( IReadWriteDevice plc, int station, string address, ushort length )
		{
			byte stat = (byte)HslHelper.ExtractParameter( ref address, "s", station );

			OperateResult<byte[]> command = BuildReadCommand( stat, address, length );
			if (!command.IsSuccess) return command;

			return await plc.ReadFromCoreServerAsync( command.Content );
		}

		/// <inheritdoc cref="Read(IReadWriteDevice, int, string[])"/>
		public async static Task<OperateResult<byte[]>> ReadAsync( IReadWriteDevice plc, int station, string[] address )
		{
			List<string[]> list = SoftBasic.ArraySplitByLength( address, 16 );
			List<byte> result = new List<byte>( 32 );

			for (int i = 0; i < list.Count; i++)
			{
				OperateResult<byte[]> command = BuildReadIndividualCommand( (byte)station, list[i] );
				if (!command.IsSuccess) return command;

				OperateResult<byte[]> read = await plc.ReadFromCoreServerAsync( command.Content );
				if (!read.IsSuccess) return read;

				result.AddRange( read.Content );
			}
			return OperateResult.CreateSuccessResult( result.ToArray( ) );
		}

		/// <inheritdoc cref="Write(IReadWriteDevice, int, string, byte[])"/>
		public static async Task<OperateResult> WriteAsync( IReadWriteDevice plc, int station, string address, byte[] value )
		{
			byte stat = (byte)HslHelper.ExtractParameter( ref address, "s", station );

			OperateResult<byte[]> command = BuildWriteCommand( stat, address, value );
			if (!command.IsSuccess) return command;

			return await plc.ReadFromCoreServerAsync( command.Content );
		}

		/// <inheritdoc cref="ReadBool(IReadWriteDevice, int, string)"/>
		public async static Task<OperateResult<bool>> ReadBoolAsync( IReadWriteDevice plc, int station, string address )
		{
			byte stat = (byte)HslHelper.ExtractParameter( ref address, "s", station );
			int bitIndex = HslHelper.GetBitIndexInformation( ref address );

			OperateResult<byte[]> command = BuildReadIndividualCommand( stat, address );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<bool>( command );

			OperateResult<byte[]> read = await plc.ReadFromCoreServerAsync( command.Content );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool>( read );

			return OperateResult.CreateSuccessResult( read.Content.ToBoolArray( )[bitIndex] );
		}

		/// <inheritdoc cref="ReadBool(IReadWriteDevice, int, string, ushort)"/>
		public async static Task<OperateResult<bool[]>> ReadBoolAsync( IReadWriteDevice plc, int station, string address, ushort length )
		{
			int bitIndex = HslHelper.GetBitIndexInformation( ref address );

			int byteLength = HslHelper.CalculateOccupyLength( bitIndex, length );
			OperateResult<byte[]> read = await ReadAsync( plc, station, address, (ushort)byteLength );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( read );

			return OperateResult.CreateSuccessResult( read.Content.ToBoolArray( ).SelectMiddle( bitIndex, length ) );
		}

		/// <inheritdoc cref="Write(IReadWriteDevice, int, string, bool)"/>
		public async static Task<OperateResult> WriteAsync( IReadWriteDevice plc, int station, string address, bool value )
		{
			byte stat = (byte)HslHelper.ExtractParameter( ref address, "s", station );
			OperateResult<string> analysis = AnalysisAddress( address, true );
			if (!analysis.IsSuccess) return analysis;

			OperateResult<byte[]> command = BuildWriteOneCommand( stat, analysis.Content.Substring( 1 ), new byte[] { (byte)(value ? 0x01 : 0x00) } );
			if (!command.IsSuccess) return command;

			return await plc.ReadFromCoreServerAsync( command.Content );
		}
#endif
	}
}
