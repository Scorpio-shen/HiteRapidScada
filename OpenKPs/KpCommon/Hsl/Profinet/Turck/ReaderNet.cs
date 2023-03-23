using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HslCommunication.Core;
using HslCommunication.Core.IMessage;
using HslCommunication.Core.Net;
using HslCommunication.Reflection;
using System.Net.Sockets;
using HslCommunication.BasicFramework;
#if !NET35 && !NET20
using System.Threading.Tasks;
#endif

namespace HslCommunication.Profinet.Turck
{
	/// <summary>
	/// Reader协议的实现
	/// </summary>
	public class ReaderNet : NetworkDeviceBase
	{
		#region Constrcutor

		/// <summary>
		/// 实例化默认的构造方法<br />
		/// Instantiate the default constructor
		/// </summary>
		public ReaderNet( )
		{
			this.WordLength    = 2;
			this.ByteTransform = new RegularByteTransform( );
		}

		/// <summary>
		/// 使用指定的ip地址和端口来实例化一个对象<br />
		/// Instantiate an object with the specified IP address and port
		/// </summary>
		/// <param name="ipAddress">设备的Ip地址</param>
		/// <param name="port">设备的端口号</param>
		public ReaderNet( string ipAddress, int port ) : this( )
		{
			this.IpAddress = ipAddress;
			this.Port      = port;
		}

		/// <inheritdoc/>
		protected override INetMessage GetNewNetMessage( ) => new TurckReaderMessage( );

		/// <inheritdoc/>
		protected override OperateResult InitializationOnConnect( Socket socket )
		{
			successfullyInitialized = false;
			return base.InitializationOnConnect( socket );
		}

		/// <inheritdoc/>
		protected override OperateResult<byte[]> ReceiveByMessage( Socket socket, int timeOut, INetMessage netMessage, Action<long, long> reportProgress = null )
		{
			// 主要是针对ACK
			while(true)
			{
				OperateResult<byte[]> read = base.ReceiveByMessage( socket, timeOut, netMessage, reportProgress );
				if (!read.IsSuccess) return read;

				if (CheckResponseACK( read.Content ))
				{
					LogNet?.WriteDebug( ToString( ), $"ACK: " + read.Content.ToHexString( ' ' ) );
					continue;
				}
				return read;
			}
		}

#if !NET35 && !NET20
		/// <inheritdoc/>
		protected async override Task<OperateResult> InitializationOnConnectAsync( Socket socket )
		{
			successfullyInitialized = false;
			return await base.InitializationOnConnectAsync( socket );
		}
		/// <inheritdoc/>
		protected async override Task<OperateResult<byte[]>> ReceiveByMessageAsync( Socket socket, int timeOut, INetMessage netMessage, Action<long, long> reportProgress = null )
		{
			while (true)
			{
				OperateResult<byte[]> read = await base.ReceiveByMessageAsync( socket, timeOut, netMessage, reportProgress );
				if (!read.IsSuccess) return read;

				if (CheckResponseACK( read.Content ))
				{
					LogNet?.WriteDebug( ToString( ), $"ACK: " + read.Content.ToHexString( ' ' ) );
					continue;
				}
				return read;
			}
		}
#endif
		#endregion

		#region Public Properties

		/// <summary>
		/// 获取设备的唯一的UID信息，本值会在连接上PLC之后自动赋值
		/// </summary>
		public string UID { get; private set; }

		/// <summary>
		/// 获取当前设备的数据块总数量，本值会在连接上PLC之后自动赋值
		/// </summary>
		public byte NumberOfBlock { get; private set; }

		/// <summary>
		/// 获取当前设备的每个数据块拥有的字节数，本值会在连接上PLC之后自动赋值
		/// </summary>
		public byte BytesOfBlock { get; private set; }

		#endregion

		#region Read Write Byte

		/// <summary>
		/// 读取指定地址的byte数据
		/// </summary>
		/// <param name="address">起始地址</param>
		/// <returns>是否读取成功的结果对象 -> Whether to read the successful result object</returns>
		/// <example>参考<see cref="Read(string, ushort)"/>的注释</example>
		[HslMqttApi( "ReadByte", "" )]
		public OperateResult<byte> ReadByte( string address ) => ByteTransformHelper.GetResultFromArray( Read( address, 1 ) );

		/// <summary>
		/// 向设备中写入byte数据，返回值说明<br />
		/// </summary>
		/// <param name="address">起始地址</param>
		/// <param name="value">byte数据 -> Byte data</param>
		/// <returns>是否写入成功的结果对象 -> Whether to write a successful result object</returns>
		[HslMqttApi( "WriteByte", "" )]
		public OperateResult Write( string address, byte value ) => Write( address, new byte[] { value } );
#if !NET35 && !NET20
		/// <inheritdoc cref="ReadByte(string)"/>
		public async Task<OperateResult<byte>> ReadByteAsync( string address ) => ByteTransformHelper.GetResultFromArray( await ReadAsync( address, 1 ) );

		/// <inheritdoc cref="Write(string, byte)"/>
		public async Task<OperateResult> WriteAsync( string address, byte value ) => await WriteAsync( address, new byte[] { value } );
#endif
		#endregion

		#region ReadWrite Method

		private OperateResult<byte[]> CheckResponseContent( byte[] content )
		{
			if (content[1] == 0x0a && content[2] == 0x0a)
			{
				if (content[5] == 0x00 && content[6] == 0x02 && content[7] == 0x00)
					successfullyInitialized = false;
				return new OperateResult<byte[]>( msg: GetErrorText( content[5], content[6], content[7] ) + " Source: " + content.ToHexString( ' ' ) );
			}

			if (content[1] == 0x07 && content[2] == 0x07)
				return OperateResult.CreateSuccessResult( new byte[0] );

			if (content.Length > 7)
				return OperateResult.CreateSuccessResult( content.SelectMiddle( 5, content.Length - 7 ) );

			return new OperateResult<byte[]>( "Error message: " + content.ToHexString( ' ' ) );
		}

		private OperateResult<byte[]> ReadRaw( byte startBlock, byte lengthOfBlock )
		{
			List<byte[]> list = BuildReadCommand( startBlock, lengthOfBlock, BytesOfBlock );
			List<byte> result = new List<byte>( );
			for (int i = 0; i < list.Count; i++)
			{
				OperateResult<byte[]> read = ReadFromCoreServer( list[i] );
				if (!read.IsSuccess) return read;

				OperateResult<byte[]> check = CheckResponseContent( read.Content );
				if (!check.IsSuccess) return check;

				result.AddRange( check.Content );
			}
			return OperateResult.CreateSuccessResult( result.ToArray( ) );
		}

		private OperateResult WriteRaw( byte startBlock, byte lengthOfBlock, byte[] value )
		{
			List<byte[]> list = BuildWriteCommand( startBlock, lengthOfBlock, BytesOfBlock, value );
			for (int i = 0; i < list.Count; i++)
			{
				OperateResult<byte[]> read = ReadFromCoreServer( list[i] );
				if (!read.IsSuccess) return read;

				OperateResult<byte[]> check = CheckResponseContent( read.Content );
				if (!check.IsSuccess) return check;
			}

			return OperateResult.CreateSuccessResult( );
		}

		/// <inheritdoc/>
		[HslMqttApi( "ReadByteArray", "" )]
		public override OperateResult<byte[]> Read( string address, ushort length )
		{
			if (!successfullyInitialized)
			{
				OperateResult<string> ini = ReadRFIDInfo( );
				if (!ini.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( ini );
			}

			OperateResult<ushort> addAnalysis = ParseAddress( address, false );
			if (!addAnalysis.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( addAnalysis );

			CalculateBlockAddress( addAnalysis.Content, length, BytesOfBlock, out byte startBlock, out byte lengthOfBlock );

			OperateResult<byte[]> read = ReadRaw( startBlock, lengthOfBlock );
			if (!read.IsSuccess) return read;

			return OperateResult.CreateSuccessResult( read.Content.SelectMiddle( addAnalysis.Content % BytesOfBlock, length ) );
		}

		/// <inheritdoc/>
		[HslMqttApi( "WriteByteArray", "" )]
		public override OperateResult Write( string address, byte[] value )
		{
			if (!successfullyInitialized)
			{
				OperateResult<string> ini = ReadRFIDInfo( );
				if (!ini.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( ini );
			}

			OperateResult<ushort> addAnalysis = ParseAddress( address, false );
			if (!addAnalysis.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( addAnalysis );

			CalculateBlockAddress( addAnalysis.Content, (ushort)value.Length, BytesOfBlock, out byte startBlock, out byte lengthOfBlock );

			// 先读取数据
			OperateResult<byte[]> readRaw = ReadRaw( startBlock, lengthOfBlock );
			if (!readRaw.IsSuccess) return readRaw;

			// 修改里面的数据信息
			value.CopyTo( readRaw.Content, addAnalysis.Content % BytesOfBlock );

			// 然后重新写入操作
			return WriteRaw( startBlock, lengthOfBlock, readRaw.Content );
		}

		/// <inheritdoc/>
		[HslMqttApi( "ReadBoolArray", "" )]
		public override OperateResult<bool[]> ReadBool( string address, ushort length )
		{
			if (!successfullyInitialized)
			{
				OperateResult<string> ini = ReadRFIDInfo( );
				if (!ini.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( ini );
			}

			OperateResult<ushort> addAnalysis = ParseAddress( address, true );
			if (!addAnalysis.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( addAnalysis );

			ushort byteStart = (ushort)(addAnalysis.Content / 8);
			ushort byteLength = (ushort)((addAnalysis.Content + length - 1) / 8 - byteStart + 1);
			CalculateBlockAddress( byteStart, byteLength, BytesOfBlock, out byte startBlock, out byte lengthOfBlock );

			OperateResult<byte[]> read = ReadRaw( startBlock, lengthOfBlock );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( read );

			return OperateResult.CreateSuccessResult( read.Content.SelectMiddle( byteStart % BytesOfBlock, byteLength ).ToBoolArray( ).SelectMiddle( addAnalysis.Content % 8, length ) );
		}

		/// <inheritdoc/>
		[HslMqttApi( "WriteBoolArray", "" )]
		public override OperateResult Write( string address, bool[] value )
		{
			if (!successfullyInitialized)
			{
				OperateResult<string> ini = ReadRFIDInfo( );
				if (!ini.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( ini );
			}

			OperateResult<ushort> addAnalysis = ParseAddress( address, true );
			if (!addAnalysis.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( addAnalysis );

			ushort byteStart = (ushort)(addAnalysis.Content / 8);
			ushort byteLength = (ushort)((addAnalysis.Content + value.Length - 1) / 8 - byteStart + 1);
			CalculateBlockAddress( byteStart, byteLength, BytesOfBlock, out byte startBlock, out byte lengthOfBlock );

			// 先读取数据
			OperateResult<byte[]> readRaw = ReadRaw( startBlock, lengthOfBlock );
			if (!readRaw.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( readRaw );

			// 修改里面的数据信息
			bool[] boolArray = readRaw.Content.ToBoolArray( );
			value.CopyTo( boolArray, (byteStart % BytesOfBlock) * 8 + addAnalysis.Content % 8 );

			// 然后重新写入操作
			return WriteRaw( startBlock, lengthOfBlock, boolArray.ToByteArray( ) );
		}
#if !NET35 && !NET20

		private async Task<OperateResult<byte[]>> ReadRawAsync( byte startBlock, byte lengthOfBlock )
		{
			List<byte[]> list = BuildReadCommand( startBlock, lengthOfBlock, BytesOfBlock );
			List<byte> result = new List<byte>( );
			for (int i = 0; i < list.Count; i++)
			{
				OperateResult<byte[]> read = await ReadFromCoreServerAsync( list[i] );
				if (!read.IsSuccess) return read;

				OperateResult<byte[]> check = CheckResponseContent( read.Content );
				if (!check.IsSuccess) return check;

				result.AddRange( check.Content );
			}
			return OperateResult.CreateSuccessResult( result.ToArray( ) );
		}

		private async Task<OperateResult> WriteRawAsync( byte startBlock, byte lengthOfBlock, byte[] value )
		{
			List<byte[]> list = BuildWriteCommand( startBlock, lengthOfBlock, BytesOfBlock, value );
			for (int i = 0; i < list.Count; i++)
			{
				OperateResult<byte[]> read = await ReadFromCoreServerAsync( list[i] );
				if (!read.IsSuccess) return read;

				OperateResult<byte[]> check = CheckResponseContent( read.Content );
				if (!check.IsSuccess) return check;
			}

			return OperateResult.CreateSuccessResult( );
		}

		/// <inheritdoc/>
		public override async Task<OperateResult<byte[]>> ReadAsync( string address, ushort length )
		{
			if (!successfullyInitialized)
			{
				OperateResult<string> ini = await ReadRFIDInfoAsync( );
				if (!ini.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( ini );
			}

			OperateResult<ushort> addAnalysis = ParseAddress( address, false );
			if (!addAnalysis.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( addAnalysis );

			CalculateBlockAddress( addAnalysis.Content, length, BytesOfBlock, out byte startBlock, out byte lengthOfBlock );

			OperateResult<byte[]> read = await ReadRawAsync( startBlock, lengthOfBlock );
			if (!read.IsSuccess) return read;

			return OperateResult.CreateSuccessResult( read.Content.SelectMiddle( addAnalysis.Content % BytesOfBlock, length ) );
		}

		/// <inheritdoc/>
		public override async Task<OperateResult> WriteAsync( string address, byte[] value )
		{
			if (!successfullyInitialized)
			{
				OperateResult<string> ini = await ReadRFIDInfoAsync( );
				if (!ini.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( ini );
			}

			OperateResult<ushort> addAnalysis = ParseAddress( address, false );
			if (!addAnalysis.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( addAnalysis );

			CalculateBlockAddress( addAnalysis.Content, (ushort)value.Length, BytesOfBlock, out byte startBlock, out byte lengthOfBlock );

			// 先读取数据
			OperateResult<byte[]> readRaw = await ReadRawAsync( startBlock, lengthOfBlock );
			if (!readRaw.IsSuccess) return readRaw;

			// 修改里面的数据信息
			value.CopyTo( readRaw.Content, addAnalysis.Content % BytesOfBlock );

			// 然后重新写入操作
			return await WriteRawAsync( startBlock, lengthOfBlock, readRaw.Content );
		}

		/// <inheritdoc/>
		public override async Task<OperateResult<bool[]>> ReadBoolAsync( string address, ushort length )
		{
			if (!successfullyInitialized)
			{
				OperateResult<string> ini = await ReadRFIDInfoAsync( );
				if (!ini.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( ini );
			}

			OperateResult<ushort> addAnalysis = ParseAddress( address, true );
			if (!addAnalysis.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( addAnalysis );

			ushort byteStart = (ushort)(addAnalysis.Content / 8);
			ushort byteLength = (ushort)((addAnalysis.Content + length - 1) / 8 - byteStart + 1);
			CalculateBlockAddress( byteStart, byteLength, BytesOfBlock, out byte startBlock, out byte lengthOfBlock );

			OperateResult<byte[]> read = await ReadRawAsync( startBlock, lengthOfBlock );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( read );

			return OperateResult.CreateSuccessResult( read.Content.SelectMiddle( byteStart % BytesOfBlock, byteLength ).ToBoolArray( ).SelectMiddle( addAnalysis.Content % 8, length ) );
		}

		/// <inheritdoc/>
		public override async Task<OperateResult> WriteAsync( string address, bool[] value )
		{
			if (!successfullyInitialized)
			{
				OperateResult<string> ini = await ReadRFIDInfoAsync( );
				if (!ini.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( ini );
			}

			OperateResult<ushort> addAnalysis = ParseAddress( address, true );
			if (!addAnalysis.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( addAnalysis );

			ushort byteStart = (ushort)(addAnalysis.Content / 8);
			ushort byteLength = (ushort)((addAnalysis.Content + value.Length - 1) / 8 - byteStart + 1);
			CalculateBlockAddress( byteStart, byteLength, BytesOfBlock, out byte startBlock, out byte lengthOfBlock );

			// 先读取数据
			OperateResult<byte[]> readRaw = await ReadRawAsync( startBlock, lengthOfBlock );
			if (!readRaw.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( readRaw );

			// 修改里面的数据信息
			bool[] boolArray = readRaw.Content.ToBoolArray( );
			value.CopyTo( boolArray, (byteStart % BytesOfBlock) * 8 + addAnalysis.Content % 8 );

			// 然后重新写入操作
			return await WriteRawAsync( startBlock, lengthOfBlock, boolArray.ToByteArray( ) );
		}
#endif
		#endregion

		#region Public Method

		private OperateResult<string> ExtraUID( byte[] content )
		{
			OperateResult<byte[]> check = CheckResponseContent( content );
			if (check.IsSuccess)
			{
				UID                     = content.SelectMiddle( 5, 8 ).ToHexString( );
				NumberOfBlock           = content[15];
				BytesOfBlock            = (byte)(content[16] + 1);
				successfullyInitialized = true;

				return OperateResult.CreateSuccessResult( UID );
			}
			else
			{
				successfullyInitialized = false;
				return OperateResult.CreateFailedResult<string>( check );
			}
		}

		/// <summary>
		/// 读取载码体信息，并将读取的信息进行初始化
		/// </summary>
		/// <returns>返回UID信息</returns>
		public OperateResult<string> ReadRFIDInfo( ) => ReadFromCoreServer( PackReaderCommand( new byte[] { 0x70, 0x00 } ) ).Then( ExtraUID );
#if !NET35 && !NET20
		/// <inheritdoc cref="ReadRFIDInfo"/>
		public async Task<OperateResult<string>> ReadRFIDInfoAsync( ) => (await ReadFromCoreServerAsync( PackReaderCommand( new byte[] { 0x70, 0x00 } ) )).Then( ExtraUID );
#endif
	#endregion

	#region Private Member

	private bool successfullyInitialized = false;

		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString( ) => $"ReaderNet[{IpAddress}:{Port}]";

		#endregion

		#region Static Helper

		private static string GetErrorText( byte err1, byte err2, byte err3 )
		{
			if (err1 == 0x01) return "Command not supported";
			if (err1 == 0x02) return "Command not correctly detected, e.g. wrong format";
			if (err1 == 0x03) return "Command option not supportde";
			if (err1 == 0x0f) return "Undefined/General error";
			if (err1 == 0x10) return "Requested memory block not available";
			if (err1 == 0x11) return "Requested memory block is already locked";
			if (err1 == 0x12) return "Requested memory block is locked and cannot be written";
			if (err1 == 0x13) return "Writing of requested memory block not successful";
			if (err1 == 0x14) return "Requested memory block could not be locked";
			if (err1 != 0x00) return "Customer specific error codes";
			if (err2 == 0x01) return "CRC_ERR, telegram fault in the tag-response";
			if (err2 == 0x02) return "TimeOut_ERR, no tag-response in the given time";
			if (err2 == 0x04) return "Tag_ERR, tag defect, e.g. multiple crc-faults on the air interface";
			if (err2 == 0x08) return "CHAIN_ERR, Tag has left the air interface before executing all commands";
			if (err2 == 0x10) return "UID_ERR, other UID as expected was detected during addressed mode";
			if (err2 != 0x00) return StringResources.Language.UnknownError;
			if (err3 == 0x01) return "TRANS_ERR, transceiver defect, e.g. Flash-checksum";
			if (err3 == 0x02) return "CMD_ERR, fault during execution of a command";
			if (err3 == 0x04) return "syntax_ERR, telegram content not valid, e.g. requested tag-memory address not available";
			if (err3 == 0x08) return "PS_ERR, power supply too low";
			if (err3 == 0x10) return "CMD_UNKNOWN, unknown command code";
			return StringResources.Language.UnknownError;
		}

		/// <summary>
		/// 将字符串的地址解析出实际的整数地址，如果是位地址，支持使用小数点的形式 例如100.1
		/// </summary>
		/// <param name="address">地址信息</param>
		/// <param name="isBit">是否位地址</param>
		/// <returns>整数地址信息</returns>
		public static OperateResult<ushort> ParseAddress( string address, bool isBit )
		{
			try
			{
				if (!isBit) return OperateResult.CreateSuccessResult( ushort.Parse( address ) );
				else
				{
					if (address.IndexOf('.') < 0)
					{
						return OperateResult.CreateSuccessResult( ushort.Parse( address ) );
					}
					else
					{
						string[] splits = address.Split( new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries );
						return OperateResult.CreateSuccessResult( (ushort)(int.Parse( splits[0] ) * 8 + int.Parse( splits[1] )) );
					}
				}
			}
			catch ( Exception ex)
			{
				return new OperateResult<ushort>( "Address input wrong, reason: " + ex.Message );
			}
		}

		/// <summary>
		/// 检查当前的设备的返回的数据是否是ACK消息
		/// </summary>
		/// <param name="content">设备的数据信息</param>
		/// <returns>是否是ACK内容</returns>
		public static bool CheckResponseACK( byte[] content )
		{
			if (content[1] == 0x07 && content[2] == 0x07)
			{
				if ((content[3] == 0x68) ||                         // 读ACK
					(content[3] == 0x69 && content[4] == 0x89) ||   // 写ACK
					(content[3] == 0x70) ||                         // 读载码体ACK
					(content[3] == 0x69 && content[4] == 0x81))     // 写错误Error ACK
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// 计算缓存数据里的CRC校验信息，并返回CRC计算的结果
		/// </summary>
		/// <param name="data">数据信息</param>
		/// <param name="len">计算的长度信息</param>
		/// <returns>CRC计算结果</returns>
		public static byte[] CalculateCRC( byte[] data, int len )
		{
			int crc = 0xFFFF;
			int poly = 0x8408;
			byte[] buffer = new byte[2];
			for (int i = 0; i < len; i++)
			{
				crc = crc ^ (int)data[i];
				for (int j = 0; j < 8; j++)
				{
					if ((crc & 0x0001) == 1)
					{
						crc = (crc >> 1) ^ poly;
					}
					else
					{
						crc = (crc >> 1);
					}
				}
			}
			crc = ~crc;
			buffer[0] = Convert.ToByte( crc & 0xff );
			buffer[1] = Convert.ToByte( (crc >> 8) & 0xff );
			return buffer;
		}

		/// <summary>
		/// 计算并填充CRC校验到原始数据中去
		/// </summary>
		/// <param name="data">原始的数据信息</param>
		/// <param name="len">计算的长度信息</param>
		public static void CalculateAndFillCRC( byte[] data, int len )
		{
			byte[] crc = CalculateCRC( data, len );
			data[len + 0] = crc[0];
			data[len + 1] = crc[1];
		}

		/// <summary>
		/// 校验当前数据的CRC校验是否正确
		/// </summary>
		/// <param name="data">原始数据信息</param>
		/// <param name="len">长度数据信息</param>
		/// <returns>校验结果</returns>
		public static bool CheckCRC( byte[] data, int len )
		{
			byte[] crc = CalculateCRC( data, len );
			return data[len + 0] == crc[0] && data[len + 1] == crc[1];
		}

		/// <summary>
		/// 将普通的命令打造成图尔克的reader协议完整命令
		/// </summary>
		/// <param name="command">命令信息</param>
		/// <returns>完整的命令包</returns>
		public static byte[] PackReaderCommand( byte[] command )
		{
			byte[] buffer = new byte[5 + command.Length];
			buffer[0] = 0xAA;
			buffer[1] = (byte)buffer.Length;
			buffer[2] = (byte)buffer.Length;
			command.CopyTo( buffer, 3 );

			CalculateAndFillCRC( buffer, 3 + command.Length );
			return buffer;
		}

		/// <summary>
		/// 构建读取的数据块的命令信息，一次最多读取64个字节
		/// </summary>
		/// <param name="startBlock">需要读取的起始 Block。从 0 开始。</param>
		/// <param name="numberBlock">需要读取的 Block 数量。 从 0 开始。</param>
		/// <param name="bytesOfBlock">每个数据块占用的字节数</param>
		/// <returns>完整的命令报文信息</returns>
		private static List<byte[]> BuildReadCommand( byte startBlock, byte numberBlock, byte bytesOfBlock )
		{
			int splitLength = 64 / bytesOfBlock;
			int[] split = SoftBasic.SplitIntegerToArray(numberBlock, splitLength);
			List<byte[]> list = new List<byte[]>( );
			for ( int i = 0; i < split.Length; i++)
			{
				list.Add( PackReaderCommand( new byte[] { 0x68, 0x00, startBlock, (byte)(split[i] - 1) } ) );
				startBlock += (byte)split[i];
			}
			return list;
		}

		/// <summary>
		/// 构建写入数据块的命令信息，一次最多写入64个字节
		/// </summary>
		/// <param name="startBlock">需要读取的起始 Block。从 0 开始。</param>
		/// <param name="numberBlock">需要读取的 Block 数量。 从 0 开始。</param>
		/// <param name="bytesOfBlock">每个数据块占用的字节数</param>
		/// <param name="value">写入的数据</param>
		/// <returns>完整的写入的命令报文信息</returns>
		private static List<byte[]> BuildWriteCommand( byte startBlock, byte numberBlock, byte bytesOfBlock, byte[] value )
		{
			if (value == null) value = new byte[0];
			int splitLength = 64 / bytesOfBlock;
			int[] split = SoftBasic.SplitIntegerToArray( numberBlock, splitLength );
			List<byte[]> list = new List<byte[]>( );

			int index = 0;
			for (int i = 0; i < split.Length; i++)
			{
				byte[] buffer = new byte[4 + split[i] * bytesOfBlock];
				buffer[0] = 0x69;
				buffer[1] = 0x00;
				buffer[2] = startBlock;
				buffer[3] = (byte)(split[i] - 1);
				value.SelectMiddle( index, split[i] * bytesOfBlock ).CopyTo( buffer, 4 );

				startBlock += (byte)split[i];
				index += split[i] * bytesOfBlock;
				list.Add( PackReaderCommand( buffer ) );
			}
			return list;
		}

		private static void CalculateBlockAddress( ushort address, ushort length, byte bytesOfBlock, out byte startBlock, out byte lengthOfBlock )
		{
			startBlock = (byte)(address / bytesOfBlock);
			int endBlock = (byte)((address + length - 1) / bytesOfBlock);
			lengthOfBlock = (byte)(endBlock - startBlock + 1);
		}

		#endregion
	}
}
