using HslCommunication.BasicFramework;
using HslCommunication.Core;
using HslCommunication.Core.IMessage;
using HslCommunication.Core.Net;
using HslCommunication.Profinet.Omron;
using HslCommunication.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
#if !NET35 && !NET20
using System.Threading.Tasks;
#endif

namespace HslCommunication.Profinet.AllenBradley
{
	/// <summary>
	/// 在CIP协议里，使用PCCC命令进行访问设备的原始数据文件的通信方法，
	/// </summary>
	/// <remarks>
	/// <inheritdoc cref="AllenBradleySLCNet" path="remarks"/>
	/// </remarks>
	/// <example>
	/// <inheritdoc cref="AllenBradleySLCNet" path="example"/>
	/// </example>
	public class AllenBradleyPcccNet : NetworkConnectedCip
	{
		#region Constructor

		/// <summary>
		/// 实例化一个默认的对象
		/// </summary>
		public AllenBradleyPcccNet( )
		{
			WordLength    = 2;
			ByteTransform = new RegularByteTransform( );
		}

		/// <summary>
		/// 根据指定的IP及端口来实例化这个连接对象
		/// </summary>
		/// <param name="ipAddress">PLC的Ip地址</param>
		/// <param name="port">PLC的端口号信息</param>
		public AllenBradleyPcccNet( string ipAddress, int port = 44818 ) : this( )
		{
			IpAddress = ipAddress;
			Port      = port;
		}

		/// <inheritdoc/>
		protected override byte[] GetLargeForwardOpen( ushort connectionID )
		{
			TOConnectionId = (uint)HslHelper.HslRandom.Next( );
			byte[] buffer =  @"
00 00 00 00 0a 00 02 00 00 00 00 00 b2 00 30 00
54 02 20 06 24 01 0a 05 00 00 00 00 e8 a3 14 00
27 04 09 10 0b 46 a5 c1 07 00 00 00 01 40 20 00
f4 43 01 40 20 00 f4 43 a3 03 01 00 20 02 24 01".ToHexBytes( );
			BitConverter.GetBytes( AllenBradleyHelper.OriginatorVendorID ).CopyTo( buffer, 34 );
			BitConverter.GetBytes( AllenBradleyHelper.OriginatorSerialNumber ).CopyTo( buffer, 36 );
			BitConverter.GetBytes( TOConnectionId ).CopyTo( buffer, 28 );
			return buffer;
		}

		/// <inheritdoc/>
		protected override byte[] GetLargeForwardClose( )
		{
			return @"
00 00 00 00 0a 00 02 00 00 00 00 00 b2 00 18 00
4e 02 20 06 24 01 0a 05 27 04 09 10 0b 46 a5 c1
03 00 01 00 20 02 24 01".ToHexBytes( );
		}

		#endregion

		#region Read Write Byte

		/// <inheritdoc/>
		/// <remarks>
		/// 读取PLC的原始数据信息，地址示例：N7:0
		/// </remarks>
		[HslMqttApi( "ReadByteArray", "" )]
		public override OperateResult<byte[]> Read( string address, ushort length )
		{
			// 指令生成 -> Instruction Generation
			OperateResult<byte[]> command = AllenBradleyHelper.PackExecutePCCCRead( (int)incrementCount.GetCurrentValue( ), address, length );
			if (!command.IsSuccess) return command;

			// 核心交互 -> Core Interactions
			OperateResult<byte[]> read = ReadFromCoreServer( PackCommandService( command.Content ) );
			if (!read.IsSuccess) return read;

			// 检查反馈 -> Check Feedback
			OperateResult check = AllenBradleyHelper.CheckResponse( read.Content );
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( check );

			// 提取数据 -> Extracting data
			OperateResult<byte[], ushort, bool> extra = ExtractActualData( read.Content, true );
			if (!extra.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( extra );

			return OperateResult.CreateSuccessResult( extra.Content1 );
		}

		/// <inheritdoc/>
		/// <remarks>
		/// 写入PLC的原始数据信息，地址示例：N7:0
		/// </remarks>
		[HslMqttApi( "WriteByteArray", "" )]
		public override OperateResult Write( string address, byte[] value )
		{
			// 指令生成 -> Instruction Generation
			OperateResult<byte[]> command = AllenBradleyHelper.PackExecutePCCCWrite( (int)incrementCount.GetCurrentValue( ), address, value );
			if (!command.IsSuccess) return command;

			// 核心交互 -> Core Interactions
			OperateResult<byte[]> read = ReadFromCoreServer( PackCommandService( command.Content ) );
			if (!read.IsSuccess) return read;

			// 检查反馈 -> Check Feedback
			OperateResult check = AllenBradleyHelper.CheckResponse( read.Content );
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( check );

			// 提取数据 -> Extracting data
			OperateResult<byte[], ushort, bool> extra = ExtractActualData( read.Content, true );
			if (!extra.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( extra );

			return OperateResult.CreateSuccessResult( );
		}

		/// <inheritdoc/>
		[HslMqttApi( "ReadBool", "" )]
		public override OperateResult<bool> ReadBool( string address )
		{
			address = AllenBradleySLCNet.AnalysisBitIndex( address, out int bitIndex );

			OperateResult<byte[]> read = Read( address, (ushort)(bitIndex / 16 * 2 + 2 ));
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool>( read );

			return OperateResult.CreateSuccessResult( read.Content.ToBoolArray( )[bitIndex] );
		}

		/// <inheritdoc cref="IReadWriteNet.Write(string, bool)"/>
		[HslMqttApi( "WriteBool", "" )]
		public override OperateResult Write( string address, bool value )
		{
			address = AllenBradleySLCNet.AnalysisBitIndex( address, out int bitIndex );

			// 指令生成 -> Instruction Generation
			OperateResult<byte[]> command = AllenBradleyHelper.PackExecutePCCCWrite( (int)incrementCount.GetCurrentValue( ), address, bitIndex, value );
			if (!command.IsSuccess) return command;

			// 核心交互 -> Core Interactions
			OperateResult<byte[]> read = ReadFromCoreServer( PackCommandService( command.Content ) );
			if (!read.IsSuccess) return read;

			// 检查反馈 -> Check Feedback
			OperateResult check = AllenBradleyHelper.CheckResponse( read.Content );
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( check );

			// 提取数据 -> Extracting data
			OperateResult<byte[], ushort, bool> extra = ExtractActualData( read.Content, true );
			if (!extra.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( extra );

			return OperateResult.CreateSuccessResult( );
		}

		/// <inheritdoc cref="Read(string, ushort)"/>
		public OperateResult<string> ReadString( string address ) => ReadString( address, 0, Encoding.ASCII );

		/// <inheritdoc/>
		/// <remarks>
		/// 读取PLC的地址信息，如果输入了 ST 的地址，例如 ST10:2, 当长度指定为 0 的时候，这时候就是动态的读取PLC来获取实际的字符串长度。<br />
		/// Read the PLC address information, if the ST address is entered, such as ST10:2, when the length is specified as 0, then the PLC is dynamically read to obtain the actual string length.
		/// </remarks>
		public override OperateResult<string> ReadString( string address, ushort length, Encoding encoding )
		{
			if ( !string.IsNullOrEmpty( address ) && address.StartsWith("ST") )
			{
				if ( length <= 0)
				{
					OperateResult<byte[]> read = Read( address, 2 );
					if (!read.IsSuccess) return OperateResult.CreateFailedResult<string>( read );

					int len = ByteTransform.TransUInt16( read.Content, 0 );

					read = Read( address, (ushort)(2 + (len % 2 != 0 ? len + 1 : len)) );
					if (!read.IsSuccess) return OperateResult.CreateFailedResult<string>( read );

					return OperateResult.CreateSuccessResult( encoding.GetString( SoftBasic.BytesReverseByWord( read.Content ), 2, len ) );
				}
				else
				{
					OperateResult<byte[]> read = Read( address, (ushort)(length % 2 != 0 ? length + 3 : length + 2) );
					if (!read.IsSuccess) return OperateResult.CreateFailedResult<string>( read );

					int len = ByteTransform.TransUInt16( read.Content, 0 );
					if (len + 2 > read.Content.Length) len = read.Content.Length - 2;

					return OperateResult.CreateSuccessResult( encoding.GetString( SoftBasic.BytesReverseByWord( read.Content ), 2, len ) );
				}
			}
			else
			{
				return base.ReadString( address, length, encoding );
			}
		}

		/// <inheritdoc/>
		public override OperateResult Write( string address, string value, Encoding encoding )
		{
			if (!string.IsNullOrEmpty( address ) && address.StartsWith( "ST" ))
			{
				byte[] temp = ByteTransform.TransByte( value, encoding );
				int len = temp.Length;
				temp = SoftBasic.ArrayExpandToLengthEven( temp );

				return Write( address, SoftBasic.SpliceArray( new byte[] { BitConverter.GetBytes( len )[0], BitConverter.GetBytes( len )[1] }, SoftBasic.BytesReverseByWord( temp ) ) );
			}
			else
			{
				return base.Write( address, value, encoding );
			}
		}

#if !NET35 && !NET20

		/// <inheritdoc cref="Read(string, ushort)"/>
		public override async Task<OperateResult<byte[]>> ReadAsync( string address, ushort length )
		{
			// 指令生成 -> Instruction Generation
			OperateResult<byte[]> command = AllenBradleyHelper.PackExecutePCCCRead( (int)incrementCount.GetCurrentValue( ), address, length );
			if (!command.IsSuccess) return command;

			// 核心交互 -> Core Interactions
			OperateResult<byte[]> read = await ReadFromCoreServerAsync( PackCommandService( command.Content ) );
			if (!read.IsSuccess) return read;

			// 检查反馈 -> Check Feedback
			OperateResult check = AllenBradleyHelper.CheckResponse( read.Content );
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( check );

			// 提取数据 -> Extracting data
			OperateResult<byte[], ushort, bool> extra = ExtractActualData( read.Content, true );
			if (!extra.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( extra );

			return OperateResult.CreateSuccessResult( extra.Content1 );
		}

		/// <inheritdoc cref="Write(string, byte[])"/>
		public override async Task<OperateResult> WriteAsync( string address, byte[] value )
		{
			// 指令生成 -> Instruction Generation
			OperateResult<byte[]> command = AllenBradleyHelper.PackExecutePCCCWrite( (int)incrementCount.GetCurrentValue( ), address, value );
			if (!command.IsSuccess) return command;

			// 核心交互 -> Core Interactions
			OperateResult<byte[]> read = await ReadFromCoreServerAsync( PackCommandService( command.Content ) );
			if (!read.IsSuccess) return read;

			// 检查反馈 -> Check Feedback
			OperateResult check = AllenBradleyHelper.CheckResponse( read.Content );
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( check );

			// 提取数据 -> Extracting data
			OperateResult<byte[], ushort, bool> extra = ExtractActualData( read.Content, true );
			if (!extra.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( extra );

			return OperateResult.CreateSuccessResult( );
		}

		/// <inheritdoc cref="ReadBool(string)"/>
		public override async Task<OperateResult<bool>> ReadBoolAsync( string address )
		{
			address = AllenBradleySLCNet.AnalysisBitIndex( address, out int bitIndex );

			OperateResult<byte[]> read = await ReadAsync( address, (ushort)(bitIndex / 16 * 2 + 2) );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool>( read );

			return OperateResult.CreateSuccessResult( read.Content.ToBoolArray( )[bitIndex] );
		}


		/// <inheritdoc cref="IReadWriteNet.Write(string, bool)"/>
		public async override Task<OperateResult> WriteAsync( string address, bool value )
		{
			address = AllenBradleySLCNet.AnalysisBitIndex( address, out int bitIndex );

			// 指令生成 -> Instruction Generation
			OperateResult<byte[]> command = AllenBradleyHelper.PackExecutePCCCWrite( (int)incrementCount.GetCurrentValue( ), address, bitIndex, value );
			if (!command.IsSuccess) return command;

			// 核心交互 -> Core Interactions
			OperateResult<byte[]> read = await ReadFromCoreServerAsync( PackCommandService( command.Content ) );
			if (!read.IsSuccess) return read;

			// 检查反馈 -> Check Feedback
			OperateResult check = AllenBradleyHelper.CheckResponse( read.Content );
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( check );

			// 提取数据 -> Extracting data
			OperateResult<byte[], ushort, bool> extra = ExtractActualData( read.Content, true );
			if (!extra.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( extra );

			return OperateResult.CreateSuccessResult( );
		}


		/// <inheritdoc cref="Read(string, ushort)"/>
		public async Task<OperateResult<string>> ReadStringAsync( string address ) => await ReadStringAsync( address, 0, Encoding.ASCII );

		/// <inheritdoc cref="ReadString(string, ushort, Encoding)"/>
		public async override Task<OperateResult<string>> ReadStringAsync( string address, ushort length, Encoding encoding )
		{
			if (!string.IsNullOrEmpty( address ) && address.StartsWith( "ST" ))
			{
				if (length <= 0)
				{
					OperateResult<byte[]> read = await ReadAsync( address, 2 );
					if (!read.IsSuccess) return OperateResult.CreateFailedResult<string>( read );

					int len = ByteTransform.TransUInt16( read.Content, 0 );

					read = await ReadAsync( address, (ushort)(2 + (len % 2 != 0 ? len + 1 : len)) );
					if (!read.IsSuccess) return OperateResult.CreateFailedResult<string>( read );

					return OperateResult.CreateSuccessResult( encoding.GetString( SoftBasic.BytesReverseByWord( read.Content ), 2, len ) );
				}
				else
				{
					OperateResult<byte[]> read = await ReadAsync( address, (ushort)(length % 2 != 0 ? length + 3 : length + 2) );
					if (!read.IsSuccess) return OperateResult.CreateFailedResult<string>( read );

					int len = ByteTransform.TransUInt16( read.Content, 0 );
					if (len + 2 > read.Content.Length) len = read.Content.Length - 2;

					return OperateResult.CreateSuccessResult( encoding.GetString( SoftBasic.BytesReverseByWord( read.Content ), 2, len ) );
				}
			}
			else
			{
				return base.ReadString( address, length, encoding );
			}
		}

		/// <inheritdoc/>
		public async override Task<OperateResult> WriteAsync( string address, string value, Encoding encoding )
		{
			if (!string.IsNullOrEmpty( address ) && address.StartsWith( "ST" ))
			{
				byte[] temp = ByteTransform.TransByte( value, encoding );
				int len = temp.Length;
				temp = SoftBasic.ArrayExpandToLengthEven( temp );

				return await WriteAsync( address, SoftBasic.SpliceArray( new byte[] { BitConverter.GetBytes( len )[0], BitConverter.GetBytes( len )[1] }, SoftBasic.BytesReverseByWord( temp ) ) );
			}
			else
			{
				return await base.WriteAsync( address, value, encoding );
			}
		}
#endif
		#endregion

		#region Read Write Byte

		/// <inheritdoc cref="Read(string, ushort)"/>
		[HslMqttApi( "ReadByte", "" )]
		public OperateResult<byte> ReadByte( string address ) => ByteTransformHelper.GetResultFromArray( Read( address, 1 ) );

		/// <inheritdoc cref="Write(string, byte)"/>
		[HslMqttApi( "WriteByte", "" )]
		public OperateResult Write( string address, byte value ) => Write( address, new byte[] { value } );

		#endregion

		#region Async Read Write Byte
#if !NET35 && !NET20
		/// <inheritdoc cref="ReadByte(string)"/>
		public async Task<OperateResult<byte>> ReadByteAsync( string address ) => ByteTransformHelper.GetResultFromArray( await ReadAsync( address, 1 ) );

		/// <inheritdoc cref="Write(string, byte)"/>
		public async Task<OperateResult> WriteAsync( string address, byte value ) => await WriteAsync( address, new byte[] { value } );
#endif
		#endregion

		#region Private Method

		#endregion

		#region Private Member

		private SoftIncrementCount incrementCount = new SoftIncrementCount( 65535, 2, 2 );

		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString( ) => $"AllenBradleyPcccNet[{IpAddress}:{Port}]";

		#endregion

	}
}
