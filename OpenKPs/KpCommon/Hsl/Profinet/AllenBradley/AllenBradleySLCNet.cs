using HslCommunication.Core;
using HslCommunication.Core.IMessage;
using HslCommunication.Core.Net;
using HslCommunication.Profinet.Panasonic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using HslCommunication.Reflection;
using System.Text;
using HslCommunication.Core.Address;
#if !NET20 && !NET35
using System.Threading.Tasks;
#endif

namespace HslCommunication.Profinet.AllenBradley
{
	/// <summary>
	/// AllenBradley品牌的PLC，针对SLC系列的通信的实现，测试PLC为1747。<br />
	/// AllenBradley brand PLC, for the realization of SLC series communication, the test PLC is 1747.
	/// </summary>
	/// <remarks>
	/// 地址支持的举例：A9:0, N9:0, B9:0, F9:0, S:0, ST1:0, C9:0, I:0/10, O:0/1, R9:0, T9:0, L9:0
	/// </remarks>
	/// <example>
	/// 地址格式如下：
	/// <list type="table">
	///   <listheader>
	///     <term>地址代号</term>
	///     <term>字操作</term>
	///     <term>位操作</term>
	///     <term>备注</term>
	///   </listheader>
	///   <item>
	///     <term>A</term>
	///     <term>A9:0</term>
	///     <term>A9:0/1 或 A9:0.1</term>
	///     <term>ASCII</term>
	///   </item>
	///   <item>
	///     <term>B</term>
	///     <term>B9:0</term>
	///     <term>B9:0/1 或 B9:0.1</term>
	///     <term>Bit</term>
	///   </item>
	///   <item>
	///     <term>N</term>
	///     <term>N9:0</term>
	///     <term>N9:0/1 或 N9:0.1</term>
	///     <term>Integer</term>
	///   </item>
	///   <item>
	///     <term>F</term>
	///     <term>F9:0</term>
	///     <term>F9:0/1 或 F9:0.1</term>
	///     <term>Floating point</term>
	///   </item>
	///   <item>
	///     <term>S</term>
	///     <term>S:0</term>
	///     <term>S:0/1 或 S:0.1</term>
	///     <term>Status  S:0 等同于 S2:0</term>
	///   </item>
	///   <item>
	///     <term>ST</term>
	///     <term>ST1:0</term>
	///     <term></term>
	///     <term>String</term>
	///   </item>
	///   <item>
	///     <term>C</term>
	///     <term>C9:0</term>
	///     <term>C9:0/1 或 C9:0.1</term>
	///     <term>Counter</term>
	///   </item>
	///   <item>
	///     <term>I</term>
	///     <term>I:0</term>
	///     <term>I:0/1 或 I9:0.1</term>
	///     <term>Input</term>
	///   </item>
	///   <item>
	///     <term>O</term>
	///     <term>O:0</term>
	///     <term>O:0/1 或 O9:0.1</term>
	///     <term>Output</term>
	///   </item>
	///   <item>
	///     <term>R</term>
	///     <term>R9:0</term>
	///     <term>R9:0/1 或 R9:0.1</term>
	///     <term>Control</term>
	///   </item>
	///   <item>
	///     <term>T</term>
	///     <term>T9:0</term>
	///     <term>T9:0/1 或 T9:0.1</term>
	///     <term>Timer</term>
	///   </item>
	///   <item>
	///     <term>L</term>
	///     <term>L9:0</term>
	///     <term>L9:0/1 或 L9:0.1</term>
	///     <term>long integer</term>
	///   </item>
	/// </list>
	/// 感谢 seedee 的测试支持。
	/// </example>
	public class AllenBradleySLCNet : NetworkDeviceBase
	{
		#region Constructor

		/// <summary>
		/// Instantiate a communication object for a Allenbradley PLC protocol
		/// </summary>
		public AllenBradleySLCNet( )
		{
			WordLength    = 2;
			ByteTransform = new RegularByteTransform( );
		}

		/// <summary>
		/// Instantiate a communication object for a Allenbradley PLC protocol
		/// </summary>
		/// <param name="ipAddress">PLC IpAddress</param>
		/// <param name="port">PLC Port</param>
		public AllenBradleySLCNet( string ipAddress, int port = 44818 ) : this( )
		{
			IpAddress = ipAddress;
			Port      = port;
		}

		/// <inheritdoc/>
		protected override INetMessage GetNewNetMessage( ) => new AllenBradleySLCMessage( );

		#endregion

		#region Public Properties

		/// <summary>
		/// The current session handle, which is determined by the PLC when communicating with the PLC handshake
		/// </summary>
		public uint SessionHandle { get; protected set; }

		#endregion

		#region Double Mode Override

		/// <inheritdoc/>
		protected override OperateResult InitializationOnConnect( Socket socket )
		{
			// Registering Session Information
			OperateResult<byte[]> read = ReadFromCoreServer( socket,
				"01 01 00 00 00 00 00 00 00 00 00 00 00 04 00 05 00 00 00 00 00 00 00 00 00 00 00 00".ToHexBytes( ) );
			if (!read.IsSuccess) return read;

			// Check the returned status

			// Extract session ID
			SessionHandle = ByteTransform.TransUInt32( read.Content, 4 );

			return OperateResult.CreateSuccessResult( );
		}

		#endregion

		#region Async Double Mode Override
#if !NET35 && !NET20
		/// <inheritdoc/>
		protected override async Task<OperateResult> InitializationOnConnectAsync( Socket socket )
		{
			// Registering Session Information
			OperateResult<byte[]> read = await ReadFromCoreServerAsync( socket,
				"01 01 00 00 00 00 00 00 00 00 00 00 00 04 00 05 00 00 00 00 00 00 00 00 00 00 00 00".ToHexBytes( ) );
			if (!read.IsSuccess) return read;

			// Check the returned status

			// Extract session ID
			SessionHandle = ByteTransform.TransUInt32( read.Content, 4 );

			return OperateResult.CreateSuccessResult( );
		}
#endif
		#endregion

		#region Override Read Write

		/// <summary>
		/// Read data information, data length for read array length information
		/// </summary>
		/// <param name="address">Address format of the node</param>
		/// <param name="length">In the case of arrays, the length of the array </param>
		/// <returns>Result data with result object </returns>
		[HslMqttApi( "ReadByteArray", "" )]
		public override OperateResult<byte[]> Read( string address, ushort length )
		{
			OperateResult<byte[]> command = BuildReadCommand( address, length );
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> read = ReadFromCoreServer( PackCommand( command.Content ) );
			if (!read.IsSuccess) return read;

			OperateResult<byte[]> extra = ExtraActualContent( read.Content );
			if (!extra.IsSuccess) return extra;

			return OperateResult.CreateSuccessResult( extra.Content );
		}

		/// <inheritdoc/>
		[HslMqttApi( "WriteByteArray", "" )]
		public override OperateResult Write( string address, byte[] value )
		{
			OperateResult<byte[]> command = BuildWriteCommand( address, value );
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> read = ReadFromCoreServer( PackCommand( command.Content ) );
			if (!read.IsSuccess) return read;

			OperateResult<byte[]> extra = ExtraActualContent( read.Content );
			if (!extra.IsSuccess) return extra;

			return OperateResult.CreateSuccessResult( extra.Content );
		}

		#endregion

		#region Override Read Write Bool

		/// <inheritdoc/>
		[HslMqttApi( "ReadBool", "" )]
		public override OperateResult<bool> ReadBool( string address )
		{
			address = AnalysisBitIndex( address, out int bitIndex );

			OperateResult<byte[]> read = Read( address, 1 );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool>( read );

			return OperateResult.CreateSuccessResult( read.Content.ToBoolArray( )[bitIndex] );
		}

		/// <inheritdoc/>
		[HslMqttApi( "WriteBool", "" )]
		public override OperateResult Write( string address, bool value )
		{
			OperateResult<byte[]> command = BuildWriteCommand( address, value );
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> read = ReadFromCoreServer( PackCommand( command.Content ) );
			if (!read.IsSuccess) return read;

			OperateResult<byte[]> extra = ExtraActualContent( read.Content );
			if (!extra.IsSuccess) return extra;

			return OperateResult.CreateSuccessResult( extra.Content );
		}

		#endregion

		#region Override Read Write Async
#if !NET20 && !NET35
		/// <inheritdoc cref="Read(string, ushort)"/>
		public override async Task<OperateResult<byte[]>> ReadAsync( string address, ushort length )
		{
			OperateResult<byte[]> command = BuildReadCommand( address, length );
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> read = await ReadFromCoreServerAsync( PackCommand( command.Content ) );
			if (!read.IsSuccess) return read;

			OperateResult<byte[]> extra = ExtraActualContent( read.Content );
			if (!extra.IsSuccess) return extra;

			return OperateResult.CreateSuccessResult( extra.Content );
		}

		/// <inheritdoc/>
		public override async Task<OperateResult> WriteAsync( string address, byte[] value )
		{
			OperateResult<byte[]> command = BuildWriteCommand( address, value );
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> read = await ReadFromCoreServerAsync( PackCommand( command.Content ) );
			if (!read.IsSuccess) return read;

			OperateResult<byte[]> extra = ExtraActualContent( read.Content );
			if (!extra.IsSuccess) return extra;

			return OperateResult.CreateSuccessResult( extra.Content );
		}
#endif
		#endregion

		#region Override Read Write Bool Async
#if !NET20 && !NET35
		/// <inheritdoc/>
		public override async Task<OperateResult<bool>> ReadBoolAsync( string address )
		{
			address = AnalysisBitIndex( address, out int bitIndex );

			OperateResult<byte[]> read = await ReadAsync( address, 1 );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool>( read );

			return OperateResult.CreateSuccessResult( read.Content.ToBoolArray( )[bitIndex] );
		}
		/// <inheritdoc/>
		public override async Task<OperateResult> WriteAsync( string address, bool value )
		{
			OperateResult<byte[]> command = BuildWriteCommand( address, value );
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> read = await ReadFromCoreServerAsync( PackCommand( command.Content ) );
			if (!read.IsSuccess) return read;

			OperateResult<byte[]> extra = ExtraActualContent( read.Content );
			if (!extra.IsSuccess) return extra;

			return OperateResult.CreateSuccessResult( extra.Content );
		}
#endif
		#endregion

		#region Build Command

		private byte[] PackCommand( byte[] coreCmd )
		{
			byte[] cmd = new byte[28 + coreCmd.Length];
			cmd[0] = 0x01;
			cmd[1] = 0x07;
			cmd[2] = (byte)(coreCmd.Length / 256);
			cmd[3] = (byte)(coreCmd.Length % 256);
			BitConverter.GetBytes( SessionHandle ).CopyTo( cmd, 4 );
			coreCmd.CopyTo( cmd, 28 );
			return cmd;
		}

		/// <summary>
		/// 分析地址数据信息里的位索引的信息
		/// </summary>
		/// <param name="address">数据地址</param>
		/// <param name="bitIndex">位索引</param>
		/// <returns>地址信息</returns>
		public static string AnalysisBitIndex(string address, out int bitIndex )
		{
			bitIndex = 0;
			int index = address.IndexOf( '/' );
			if(index < 0) index = address.IndexOf( '.' );

			if (index > 0)
			{
				bitIndex = int.Parse( address.Substring( index + 1 ) );
				address = address.Substring( 0, index );
			}
			return address;
		}

		/// <summary>
		/// 构建读取的指令信息
		/// </summary>
		/// <param name="address">地址信息，举例：A9:0</param>
		/// <param name="length">读取的长度信息</param>
		/// <returns>是否成功</returns>
		public static OperateResult<byte[]> BuildReadCommand( string address, ushort length )
		{
			var analysis = AllenBradleySLCAddress.ParseFrom( address );
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( analysis );

			if (length < 2) length = 2;
			if (analysis.Content.DataCode == 0x8E)
			{
				analysis.Content.AddressStart /= 2;
			}

			byte[] command = new byte[14];
			command[0] = 0x00;
			command[1] = 0x05;
			command[2] = 0x00;
			command[3] = 0x00;
			command[4] = 0x0F;
			command[5] = 0x00;
			command[6] = 0x00;                                                 // ID信息
			command[7] = 0x01;
			command[8] = 0xA2;
			command[9] = (byte)length;                                         // 读取字节数
			command[10] = (byte)analysis.Content.DbBlock;                      // 数据区块号
			command[11] = analysis.Content.DataCode;                           // 数据类型号
			BitConverter.GetBytes( (ushort)analysis.Content.AddressStart ).CopyTo( command, 12 );  // 起始地址
			return OperateResult.CreateSuccessResult( command );
		}

		/// <summary>
		/// 构建写入的报文内容，变成实际的数据
		/// </summary>
		/// <param name="address">地址信息</param>
		/// <param name="value">数据值</param>
		/// <returns>是否成功的结果对象</returns>
		public static OperateResult<byte[]> BuildWriteCommand(string address, byte[] value )
		{
			var analysis = AllenBradleySLCAddress.ParseFrom( address );
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( analysis );

			if (analysis.Content.DataCode == 0x8E)
			{
				analysis.Content.AddressStart /= 2;
			}

			byte[] command = new byte[18 + value.Length];
			command[0] = 0x00;
			command[1] = 0x05;
			command[2] = 0x00;
			command[3] = 0x00;
			command[4] = 0x0F;
			command[5] = 0x00;
			command[6] = 0x00;                                                 // ID信息
			command[7] = 0x01;
			command[8] = 0xAB;
			command[9] = 0xFF;
			command[10] = BitConverter.GetBytes( value.Length )[0];             // 写入的字节数
			command[11] = BitConverter.GetBytes( value.Length )[1];             //
			command[12] = (byte)analysis.Content.DbBlock;                       // 数据区块号
			command[13] = analysis.Content.DataCode;                            // 数据类型号
			BitConverter.GetBytes( (ushort)analysis.Content.AddressStart ).CopyTo( command, 14 );   // 起始地址
			command[16] = 0xFF;
			command[17] = 0xFF;
			value.CopyTo( command, 18 );
			return OperateResult.CreateSuccessResult( command );
		}

		/// <summary>
		/// 构建写入的报文内容，变成实际的数据
		/// </summary>
		/// <param name="address">地址信息</param>
		/// <param name="value">数据值</param>
		/// <returns>是否成功的结果对象</returns>
		public static OperateResult<byte[]> BuildWriteCommand( string address, bool value )
		{
			address = AnalysisBitIndex( address, out int bitIndex );

			var analysis = AllenBradleySLCAddress.ParseFrom( address );
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( analysis );

			if (analysis.Content.DataCode == 0x8E)
			{
				analysis.Content.AddressStart /= 2;
			}

			int bitPow = 0x01 << bitIndex;

			byte[] command = new byte[20];
			command[0] = 0x00;
			command[1] = 0x05;
			command[2] = 0x00;
			command[3] = 0x00;
			command[4] = 0x0F;
			command[5] = 0x00;
			command[6] = 0x00;                                                         // ID信息
			command[7] = 0x01;
			command[8] = 0xAB;
			command[9] = 0xFF;
			command[10] = 0x02;                                                        // 写入的字节数
			command[11] = 0x00;
			command[12] = (byte)analysis.Content.DbBlock;                              // 数据区块号
			command[13] = analysis.Content.DataCode;                                   // 数据类型号
			BitConverter.GetBytes( (ushort)analysis.Content.AddressStart ).CopyTo( command, 14 );   // 起始地址
			command[16] = BitConverter.GetBytes( bitPow )[0];
			command[17] = BitConverter.GetBytes( bitPow )[1];
			if (value)
			{
				command[18] = BitConverter.GetBytes( bitPow )[0];
				command[19] = BitConverter.GetBytes( bitPow )[1];
			}
			return OperateResult.CreateSuccessResult( command );
		}

		/// <summary>
		/// 解析当前的实际报文内容，变成数据内容
		/// </summary>
		/// <param name="content">报文内容</param>
		/// <returns>是否成功</returns>
		public static OperateResult<byte[]> ExtraActualContent( byte[] content )
		{
			if (content.Length < 36)
			{
				return new OperateResult<byte[]>( StringResources.Language.ReceiveDataLengthTooShort + content.ToHexString( ' ' ) );
			}
			else
			{
				return OperateResult.CreateSuccessResult( content.RemoveBegin( 36 ) );
			}
		}

		#endregion

	}
}
