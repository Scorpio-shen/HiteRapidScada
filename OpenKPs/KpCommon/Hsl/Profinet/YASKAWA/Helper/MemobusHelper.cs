using HslCommunication.BasicFramework;
using HslCommunication.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if !NET20 && !NET35
using System.Threading.Tasks;
#endif

namespace HslCommunication.Profinet.YASKAWA.Helper
{
	/// <summary>
	/// Memobus的辅助类对象
	/// </summary>
	public class MemobusHelper
	{
		#region Method Helper

		internal static byte[] PackCommandWithHeader( byte[] command, long id )
		{
			byte[] buffer = new byte[12 + command.Length];        // 添加12字节的218报头
			buffer[0] = 0x11;                                     // 0x11:Memobus指令  0x12:通用信息  0x19:Memobus响应
			buffer[1] = (byte)id;
			buffer[2] = 0x00;                                     // 设定发送目标的通道编号(共享存储器的通道编号) 通过MP系列以外的设备存取时为00H
			buffer[3] = 0x00;                                     // 发送源的通道编号(共享存储器的通道编号)。通过MP系列以外的设备存取时为00H。
			buffer[6] = BitConverter.GetBytes( buffer.Length )[0];
			buffer[7] = BitConverter.GetBytes( buffer.Length )[1];
			command.CopyTo( buffer, 12 );
			return buffer;
		}

		internal static string GetErrorText( byte err )
		{
			switch (err)
			{
				case 0x01: return StringResources.Language.Memobus01;
				case 0x02: return StringResources.Language.Memobus02;
				case 0x03: return StringResources.Language.Memobus03;
				case 0x40: return StringResources.Language.Memobus40;
				case 0x41: return StringResources.Language.Memobus41;
				case 0x42: return StringResources.Language.Memobus42;
				default: return StringResources.Language.UnknownError;
			}
		}

		internal static OperateResult<byte[]> UnpackResponseContent( byte[] send, byte[] response )
		{
			if (send.Length > 15 && response.Length > 15)
			{
				if (send[15] + 0x80 == response[15] && response.Length >= 18)
				{
					return new OperateResult<byte[]>( response[17], GetErrorText( response[17] ) + " Source: " + response.ToHexString( ' ' ) );
				}
				if (send[15] != response[15])
				{
					return new OperateResult<byte[]>( response[15], "Send SFC not same as back SFC:" + response.ToHexString( ) );
				}
			}

			return OperateResult.CreateSuccessResult( response.RemoveBegin( 12 ) );
		}

		private static void SetByteHead( byte[] buffer, byte mfc, byte sfc, byte cpuTo, byte cpuFrom )
		{
			buffer[0] = BitConverter.GetBytes( buffer.Length - 2 )[0];
			buffer[1] = BitConverter.GetBytes( buffer.Length - 2 )[1];
			buffer[2] = mfc;
			buffer[3] = sfc;
			buffer[4] = (byte)((cpuTo << 4) + cpuFrom);
		}

		/// <summary>
		/// 构建读取的命令报文，支持功能码 01,02,03,04,09,0A
		/// </summary>
		/// <param name="mfc">主功能码</param>
		/// <param name="sfc">子功能码</param>
		/// <param name="cpuTo">目标的CPU编号</param>
		/// <param name="cpuFrom">发送源CPU编号</param>
		/// <param name="address">起始地址</param>
		/// <param name="length">读取地址长度</param>
		/// <returns>结果报文信息</returns>
		internal static OperateResult<byte[]> BuildReadCommand( byte mfc, byte sfc, byte cpuTo, byte cpuFrom, ushort address, ushort length )
		{
			if (sfc == 0x01 || sfc == 0x02 || sfc == 0x03 || sfc == 0x04)
			{
				byte[] buffer = new byte[9];
				SetByteHead( buffer, mfc, sfc, cpuTo, cpuFrom );

				buffer[5] = BitConverter.GetBytes( address )[1];
				buffer[6] = BitConverter.GetBytes( address )[0];
				buffer[7] = BitConverter.GetBytes( length )[1];
				buffer[8] = BitConverter.GetBytes( length )[0];
				return OperateResult.CreateSuccessResult( buffer );
			}
			else if (sfc == 0x09 || sfc == 0x0A)
			{
				byte[] buffer = new byte[10];
				SetByteHead( buffer, mfc, sfc, cpuTo, cpuFrom );

				buffer[6] = BitConverter.GetBytes( address )[0];
				buffer[7] = BitConverter.GetBytes( address )[1];
				buffer[8] = BitConverter.GetBytes( length )[0];
				buffer[9] = BitConverter.GetBytes( length )[1];
				return OperateResult.CreateSuccessResult( buffer );
			}
			else
				return new OperateResult<byte[]>( $"SFC:{sfc} {StringResources.Language.NotSupportedFunction}" );
		}

		internal static OperateResult<byte[]> BuildReadRandomCommand( byte mfc, byte sfc, byte cpuTo, byte cpuFrom, ushort[] address )
		{
			byte[] buffer = new byte[8 + address.Length * 2];
			SetByteHead( buffer, mfc, sfc, cpuTo, cpuFrom );

			buffer[6] = BitConverter.GetBytes( address.Length )[0];
			buffer[7] = BitConverter.GetBytes( address.Length )[1];
			for (int i = 0; i < address.Length; i++)
			{
				buffer[8 + i * 2 + 0] = BitConverter.GetBytes( address[i] )[0];
				buffer[8 + i * 2 + 1] = BitConverter.GetBytes( address[i] )[1];
			}
			return OperateResult.CreateSuccessResult( buffer );
		}

		/// <summary>
		/// 构建写入单一的线圈的状态变更的报文
		/// </summary>
		/// <param name="mfc">主功能码</param>
		/// <param name="sfc">子功能码</param>
		/// <param name="cpuTo">目标的CPU编号</param>
		/// <param name="cpuFrom">发送源CPU编号</param>
		/// <param name="address">起始地址</param>
		/// <param name="value">写入的通断值信息</param>
		/// <returns>写入的报文</returns>
		internal static OperateResult<byte[]> BuildWriteCommand( byte mfc, byte sfc, byte cpuTo, byte cpuFrom, ushort address, bool value )
		{
			byte[] buffer = new byte[9];
			SetByteHead( buffer, mfc, sfc, cpuTo, cpuFrom );
			buffer[5] = BitConverter.GetBytes( address )[1];
			buffer[6] = BitConverter.GetBytes( address )[0];
			buffer[7] = (byte)(value ? 0xFF : 0x00);
			buffer[8] = 0x00;
			return OperateResult.CreateSuccessResult( buffer );
		}

		internal static OperateResult<byte[]> BuildWriteCommand( byte mfc, byte sfc, byte cpuTo, byte cpuFrom, ushort address, bool[] value )
		{
			byte[] data = SoftBasic.BoolArrayToByte( value );
			byte[] buffer = new byte[9 + data.Length];
			SetByteHead( buffer, mfc, sfc, cpuTo, cpuFrom );
			buffer[5] = BitConverter.GetBytes( address )[1];
			buffer[6] = BitConverter.GetBytes( address )[0];
			buffer[7] = BitConverter.GetBytes( value.Length )[1];
			buffer[8] = BitConverter.GetBytes( value.Length )[0];
			data.CopyTo( buffer, 9 );
			return OperateResult.CreateSuccessResult( buffer );
		}

		internal static OperateResult<byte[]> BuildWriteCommand( byte mfc, byte sfc, byte cpuTo, byte cpuFrom, ushort address, short value )
		{
			byte[] buffer = new byte[9];
			SetByteHead( buffer, mfc, sfc, cpuTo, cpuFrom );
			buffer[5] = BitConverter.GetBytes( address )[1];
			buffer[6] = BitConverter.GetBytes( address )[0];
			buffer[7] = BitConverter.GetBytes( value )[1];
			buffer[8] = BitConverter.GetBytes( value )[0];
			return OperateResult.CreateSuccessResult( buffer );
		}

		internal static OperateResult<byte[]> BuildWriteCommand( byte mfc, byte sfc, byte cpuTo, byte cpuFrom, ushort address, ushort value )
		{
			byte[] buffer = new byte[9];
			SetByteHead( buffer, mfc, sfc, cpuTo, cpuFrom );
			buffer[5] = BitConverter.GetBytes( address )[1];
			buffer[6] = BitConverter.GetBytes( address )[0];
			buffer[7] = BitConverter.GetBytes( value )[1];
			buffer[8] = BitConverter.GetBytes( value )[0];
			return OperateResult.CreateSuccessResult( buffer );
		}

		internal static OperateResult<byte[]> BuildWriteCommand( byte mfc, byte sfc, byte cpuTo, byte cpuFrom, ushort address, byte[] value )
		{
			if (sfc == 0x0B)
			{
				byte[] buffer = new byte[10 + value.Length];
				SetByteHead( buffer, mfc, sfc, cpuTo, cpuFrom );
				buffer[6] = BitConverter.GetBytes( address )[0];
				buffer[7] = BitConverter.GetBytes( address )[1];
				buffer[8] = BitConverter.GetBytes( value.Length / 2 )[0];
				buffer[9] = BitConverter.GetBytes( value.Length / 2 )[1];
				SoftBasic.BytesReverseByWord( value ).CopyTo( buffer, 10 );
				return OperateResult.CreateSuccessResult( buffer );
			}
			else if (sfc == 0x10)
			{
				byte[] buffer = new byte[9 + value.Length];
				SetByteHead( buffer, mfc, sfc, cpuTo, cpuFrom );
				buffer[5] = BitConverter.GetBytes( address )[1];
				buffer[6] = BitConverter.GetBytes( address )[0];
				buffer[7] = BitConverter.GetBytes( value.Length / 2 )[1];
				buffer[8] = BitConverter.GetBytes( value.Length / 2 )[0];
				value.CopyTo( buffer, 9 );
				return OperateResult.CreateSuccessResult( buffer );
			}
			else
				return new OperateResult<byte[]>( $"SFC:{sfc} {StringResources.Language.NotSupportedFunction}" );
		}

		internal static OperateResult<byte[]> BuildWriteRandomCommand( byte mfc, byte sfc, byte cpuTo, byte cpuFrom, ushort[] address, byte[] value )
		{
			if (value.Length != address.Length * 2) return new OperateResult<byte[]>( "value.Length must be twice as much as address.Length" );

			byte[] buffer = new byte[8 + address.Length * 4];
			SetByteHead( buffer, mfc, sfc, cpuTo, cpuFrom );

			buffer[6] = BitConverter.GetBytes( address.Length )[0];
			buffer[7] = BitConverter.GetBytes( address.Length )[1];
			for (int i = 0; i < address.Length; i++)
			{
				buffer[8 + i * 4 + 0] = BitConverter.GetBytes( address[i] )[0];
				buffer[8 + i * 4 + 1] = BitConverter.GetBytes( address[i] )[1];
				buffer[8 + i * 4 + 2] = value[i * 2 + 1];                             // 这里的数据直接在赋值的时候就给颠倒了
				buffer[8 + i * 4 + 3] = value[i * 2 + 0];
			}
			return OperateResult.CreateSuccessResult( buffer );
		}

		#endregion

		#region Read Write

		/// <inheritdoc cref="IReadWriteNet.ReadBool(string, ushort)"/>
		/// <remarks>
		/// 默认使用功能码01，读取线圈操作，如果需要指定读取输入线圈，地址需要携带额外的参数，例如 x=2;100<br />
		/// The function code 01 is used by default to read the coil operation. If you need to specify the read input coil, the address needs to carry additional parameters, such as x=2;100
		/// </remarks>
		public static OperateResult<bool[]> ReadBool( IMemobus memobus, string address, ushort length )
		{
			byte mfc = (byte)HslHelper.ExtractParameter( ref address, "mfc", 0x20 );
			byte sfc = (byte)HslHelper.ExtractParameter( ref address, "x", 0x01 );

			OperateResult<byte[]> command = BuildReadCommand( mfc, sfc, memobus.CpuTo, memobus.CpuFrom, ushort.Parse( address ), length );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( command );

			OperateResult<byte[]> read = memobus.ReadFromCoreServer( command.Content );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( read );

			return OperateResult.CreateSuccessResult( read.Content.RemoveBegin( 5 ).ToBoolArray( ).SelectBegin( length ) );
		}

		/// <inheritdoc cref="IReadWriteNet.Write(string, bool)"/>
		/// <remarks>
		/// 单一线圈的状态变更，使用的主功能码为0x20, 子功能码为0x05<br />
		/// The status of a single coil is changed, the main function code used is 0x20, and the sub function code is 0x05
		/// </remarks>
		public static OperateResult Write( IMemobus memobus, string address, bool value )
		{
			byte mfc = (byte)HslHelper.ExtractParameter( ref address, "mfc", 0x20 );
			byte sfc = (byte)HslHelper.ExtractParameter( ref address, "x", 0x05 );

			OperateResult<byte[]> command = BuildWriteCommand( mfc, sfc, memobus.CpuTo, memobus.CpuFrom, ushort.Parse( address ), value );
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> read = memobus.ReadFromCoreServer( command.Content );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( read );

			return OperateResult.CreateSuccessResult( );
		}

		/// <inheritdoc cref="IReadWriteNet.Write(string, bool[])"/>
		/// <remarks>
		/// 多个线圈的状态更改，默认使用的是 0x0f 子功能码。<br />
		/// The status of multiple coils is changed, and the sub-function code 0x0f is used by default.
		/// </remarks>
		public static OperateResult Write( IMemobus memobus, string address, bool[] value )
		{
			byte mfc = (byte)HslHelper.ExtractParameter( ref address, "mfc", 0x20 );
			byte sfc = (byte)HslHelper.ExtractParameter( ref address, "x", 0x0F );

			OperateResult<byte[]> command = BuildWriteCommand( mfc, sfc, memobus.CpuTo, memobus.CpuFrom, ushort.Parse( address ), value );
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> read = memobus.ReadFromCoreServer( command.Content );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( read );

			return OperateResult.CreateSuccessResult( );
		}

		/// <inheritdoc cref="IReadWriteNet.Read(string, ushort)"/>
		/// <remarks>
		/// 地址默认使用功能码03，如果需要指定其他的功能码地址，需要手动指定功能码，例如：x=4;100, x=9;100, x=10;100, 当然也可以写成 x=0x0A;100<br />
		/// The address uses function code 03 by default. If you need to specify other function code addresses, 
		/// you need to manually specify the function code, for example: x=4;100, x=9;100, x=10;100, of course, it can also be written as x=0x0A; 100
		/// </remarks>
		public static OperateResult<byte[]> Read( IMemobus memobus, string address, ushort length )
		{
			byte mfc = (byte)HslHelper.ExtractParameter( ref address, "mfc", 0x20 );
			byte sfc = (byte)HslHelper.ExtractParameter( ref address, "x", 0x03 );

			OperateResult<byte[]> command = BuildReadCommand( mfc, sfc, memobus.CpuTo, memobus.CpuFrom, ushort.Parse( address ), length );
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> read = memobus.ReadFromCoreServer( command.Content );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( read );

			if (read.Content[3] == 0x03 || read.Content[3] == 0x04)
				return OperateResult.CreateSuccessResult( read.Content.RemoveBegin( 5 ) );
			else if (read.Content[3] == 0x09 || read.Content[3] == 0x0A)
				return OperateResult.CreateSuccessResult( SoftBasic.BytesReverseByWord( read.Content.RemoveBegin( 8 ) ) );
			else
				return OperateResult.CreateSuccessResult( read.Content.RemoveBegin( 5 ) );
		}

		/// <inheritdoc cref="IReadWriteNet.Write(string, byte[])"/>
		/// <remarks>
		/// 连续的寄存器写入操作，默认功能码是0x10，如果需要写入扩展的寄存器，使用 x=0xA;100 或是 x=10;100 即可。<br />
		/// For continuous register write operation, the default function code is 0x10. If you need to write an extended register, use x=0xA;100 or x=10;100.
		/// </remarks>
		public static OperateResult Write( IMemobus memobus, string address, byte[] value )
		{
			byte mfc = (byte)HslHelper.ExtractParameter( ref address, "mfc", 0x20 );
			byte sfc = (byte)HslHelper.ExtractParameter( ref address, "x", 0x10 );
			if (sfc == 0x03) sfc = 0x10;
			if (sfc == 0x09) sfc = 0x0B;

			OperateResult<byte[]> command = BuildWriteCommand( mfc, sfc, memobus.CpuTo, memobus.CpuFrom, ushort.Parse( address ), value );
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> read = memobus.ReadFromCoreServer( command.Content );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( read );

			return OperateResult.CreateSuccessResult( );
		}

		/// <inheritdoc cref="IReadWriteNet.Write(string, short)"/>
		/// <remarks>
		/// 单一保持寄存器的值变更，使用的主功能码为0x20, 默认子功能码为0x06，也可以写入扩展的保持型寄存器，子功能码为0x0B<br />
		/// The value of a single hold register is changed, using a primary function code of 0x20 and a default subfunction code of 0x06, or an extended holding register with a subfunction code of 0x0B
		/// </remarks>
		public static OperateResult Write( IMemobus memobus, string address, short value, Func<string, short, OperateResult> writeShort )
		{
			byte mfc = (byte)HslHelper.ExtractParameter( ref address, "mfc", 0x20 );
			byte sfc = (byte)HslHelper.ExtractParameter( ref address, "x", 0x06 );
			if (sfc == 0x0B || sfc == 0x09) return writeShort.Invoke( $"x={sfc};{address}", value );
			if (sfc == 0x03) sfc = 0x06;

			OperateResult<byte[]> command = BuildWriteCommand( mfc, sfc, memobus.CpuTo, memobus.CpuFrom, ushort.Parse( address ), value );
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> read = memobus.ReadFromCoreServer( command.Content );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( read );

			return OperateResult.CreateSuccessResult( );
		}

		/// <inheritdoc cref="IReadWriteNet.Write(string, ushort)"/>
		/// <remarks>
		/// 单一保持寄存器的值变更，使用的主功能码为0x20, 默认子功能码为0x06<br />
		/// The value of a single hold register changes, using a primary function code of 0x20 and a default subfunction code of 0x06
		/// </remarks>
		public static OperateResult Write( IMemobus memobus, string address, ushort value, Func<string, ushort, OperateResult> writeUShort )
		{
			byte mfc = (byte)HslHelper.ExtractParameter( ref address, "mfc", 0x20 );
			byte sfc = (byte)HslHelper.ExtractParameter( ref address, "x", 0x06 );
			if (sfc == 0x0B || sfc == 0x09) return writeUShort.Invoke( $"x={sfc};{address}", value );
			if (sfc == 0x03) sfc = 0x06;

			OperateResult<byte[]> command = BuildWriteCommand( mfc, sfc, memobus.CpuTo, memobus.CpuFrom, ushort.Parse( address ), value );
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> read = memobus.ReadFromCoreServer( command.Content );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( read );

			return OperateResult.CreateSuccessResult( );
		}

		/// <summary>
		/// 随机读取扩展的保持寄存器的内容，也即读取不连续地址的字数据，可以指定多个地址，然后一次性读取所有的数据，然后解析出实际的数据<br />
		/// Randomly read the contents of the extended hold register, that is, read word data of discontinuous addresses, 
		/// you can specify multiple addresses, then read all the data at once, and then parse out the actual data
		/// </summary>
		/// <remarks>
		/// 注意，本方法只能针对扩展的保持寄存器进行读取
		/// </remarks>
		/// <param name="memobus">PLC通信对象</param>
		/// <param name="address">地址信息</param>
		/// <returns>读取的原始字节结果信息</returns>
		public static OperateResult<byte[]> ReadRandom( IMemobus memobus, ushort[] address )
		{
			OperateResult<byte[]> command = BuildReadRandomCommand( 0x20, 0x0D, memobus.CpuTo, memobus.CpuFrom, address );
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> read = memobus.ReadFromCoreServer( command.Content );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( read );

			return OperateResult.CreateSuccessResult( SoftBasic.BytesReverseByWord( read.Content.RemoveBegin( 8 ) ) );
		}

		/// <summary>
		/// 随机写入扩展的保持寄存器的内容，也即写入不连续的地址的字数据，字节数组的长度必须为地址数组长度的两倍，才能正确写入。<br />
		/// Write the contents of the extended hold registers randomly, that is, write word data for discontinuous addresses,
		/// and the byte array must be twice the length of the address array to be written correctly.
		/// </summary>
		/// <remarks>
		/// 注意，本方法只能针对扩展的保持寄存器进行读取
		/// </remarks>
		/// <param name="memobus">PLC通信对象</param>
		/// <param name="address">地址信息</param>
		/// <param name="value">数据信息</param>
		/// <returns>是否写入成功的结果对象</returns>
		public static OperateResult WriteRandom( IMemobus memobus, ushort[] address, byte[] value )
		{
			OperateResult<byte[]> command = BuildWriteRandomCommand( 0x20, 0x0E, memobus.CpuTo, memobus.CpuFrom, address, value );
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> read = memobus.ReadFromCoreServer( command.Content );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( read );

			return OperateResult.CreateSuccessResult( );
		}

#if !NET20 && !NET35

		/// <inheritdoc cref="ReadBool(IMemobus, string, ushort)"/>
		public async static Task<OperateResult<bool[]>> ReadBoolAsync( IMemobus memobus, string address, ushort length )
		{
			byte mfc = (byte)HslHelper.ExtractParameter( ref address, "mfc", 0x20 );
			byte sfc = (byte)HslHelper.ExtractParameter( ref address, "x", 0x01 );

			OperateResult<byte[]> command = BuildReadCommand( mfc, sfc, memobus.CpuTo, memobus.CpuFrom, ushort.Parse( address ), length );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( command );

			OperateResult<byte[]> read = await memobus.ReadFromCoreServerAsync( command.Content );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( read );

			return OperateResult.CreateSuccessResult( read.Content.RemoveBegin( 5 ).ToBoolArray( ).SelectBegin( length ) );
		}

		/// <inheritdoc cref="Write(IMemobus, string, bool)"/>
		public async static Task<OperateResult> WriteAsync( IMemobus memobus, string address, bool value )
		{
			byte mfc = (byte)HslHelper.ExtractParameter( ref address, "mfc", 0x20 );
			byte sfc = (byte)HslHelper.ExtractParameter( ref address, "x", 0x05 );

			OperateResult<byte[]> command = BuildWriteCommand( mfc, sfc, memobus.CpuTo, memobus.CpuFrom, ushort.Parse( address ), value );
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> read = await memobus.ReadFromCoreServerAsync( command.Content );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( read );

			return OperateResult.CreateSuccessResult( );
		}

		/// <inheritdoc cref="Write(IMemobus, string, bool[])"/>
		public async static Task<OperateResult> WriteAsync( IMemobus memobus, string address, bool[] value )
		{
			byte mfc = (byte)HslHelper.ExtractParameter( ref address, "mfc", 0x20 );
			byte sfc = (byte)HslHelper.ExtractParameter( ref address, "x", 0x0F );

			OperateResult<byte[]> command = BuildWriteCommand( mfc, sfc, memobus.CpuTo, memobus.CpuFrom, ushort.Parse( address ), value );
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> read = await memobus.ReadFromCoreServerAsync( command.Content );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( read );

			return OperateResult.CreateSuccessResult( );
		}

		/// <inheritdoc cref="Read(IMemobus, string, ushort)"/>
		public async static Task<OperateResult<byte[]>> ReadAsync( IMemobus memobus, string address, ushort length )
		{
			byte mfc = (byte)HslHelper.ExtractParameter( ref address, "mfc", 0x20 );
			byte sfc = (byte)HslHelper.ExtractParameter( ref address, "x", 0x03 );

			OperateResult<byte[]> command = BuildReadCommand( mfc, sfc, memobus.CpuTo, memobus.CpuFrom, ushort.Parse( address ), length );
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> read = await memobus.ReadFromCoreServerAsync( command.Content );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( read );

			if (read.Content[3] == 0x03 || read.Content[3] == 0x04)
				return OperateResult.CreateSuccessResult( read.Content.RemoveBegin( 5 ) );
			else if (read.Content[3] == 0x09 || read.Content[3] == 0x0A)
				return OperateResult.CreateSuccessResult( SoftBasic.BytesReverseByWord( read.Content.RemoveBegin( 8 ) ) );
			else
				return OperateResult.CreateSuccessResult( read.Content.RemoveBegin( 5 ) );
		}

		/// <inheritdoc cref="Write(IMemobus, string, byte[])"/>
		public async static Task<OperateResult> WriteAsync( IMemobus memobus, string address, byte[] value )
		{
			byte mfc = (byte)HslHelper.ExtractParameter( ref address, "mfc", 0x20 );
			byte sfc = (byte)HslHelper.ExtractParameter( ref address, "x", 0x10 );

			if (sfc == 0x03) sfc = 0x10;
			if (sfc == 0x09) sfc = 0x0B;

			OperateResult<byte[]> command = BuildWriteCommand( mfc, sfc, memobus.CpuTo, memobus.CpuFrom, ushort.Parse( address ), value );
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> read = await memobus.ReadFromCoreServerAsync( command.Content );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( read );

			return OperateResult.CreateSuccessResult( );
		}

		/// <inheritdoc cref="Write(IMemobus, string, short, Func{string, short, OperateResult})"/>
		public async static Task<OperateResult> WriteAsync( IMemobus memobus, string address, short value, Func<string, short, Task<OperateResult>> writeShort )
		{
			byte mfc = (byte)HslHelper.ExtractParameter( ref address, "mfc", 0x20 );
			byte sfc = (byte)HslHelper.ExtractParameter( ref address, "x", 0x06 );

			if (sfc == 0x0B || sfc == 0x09) return await writeShort.Invoke( $"x={sfc};{address}", value );
			if (sfc == 0x03) sfc = 0x06;

			OperateResult<byte[]> command = BuildWriteCommand( mfc, sfc, memobus.CpuTo, memobus.CpuFrom, ushort.Parse( address ), value );
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> read = await memobus.ReadFromCoreServerAsync( command.Content );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( read );

			return OperateResult.CreateSuccessResult( );
		}

		/// <inheritdoc cref="Write(IMemobus, string, ushort, Func{string, ushort, OperateResult})"/>
		public async static Task<OperateResult> WriteAsync( IMemobus memobus, string address, ushort value, Func<string, ushort, Task<OperateResult>> writeUShort )
		{
			byte mfc = (byte)HslHelper.ExtractParameter( ref address, "mfc", 0x20 );
			byte sfc = (byte)HslHelper.ExtractParameter( ref address, "x", 0x06 );

			if (sfc == 0x0B || sfc == 0x09) return await writeUShort.Invoke( $"x={sfc};{address}", value );
			if (sfc == 0x03) sfc = 0x06;

			OperateResult<byte[]> command = BuildWriteCommand( mfc, sfc, memobus.CpuTo, memobus.CpuFrom, ushort.Parse( address ), value );
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> read = await memobus.ReadFromCoreServerAsync( command.Content );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( read );

			return OperateResult.CreateSuccessResult( );
		}

		/// <inheritdoc cref="ReadRandom(IMemobus, ushort[])"/>
		public static async Task<OperateResult<byte[]>> ReadRandomAsync( IMemobus memobus, ushort[] address )
		{
			OperateResult<byte[]> command = BuildReadRandomCommand( 0x20, 0x0D, memobus.CpuTo, memobus.CpuFrom, address );
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> read = await memobus.ReadFromCoreServerAsync( command.Content );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( read );

			return OperateResult.CreateSuccessResult( SoftBasic.BytesReverseByWord( read.Content.RemoveBegin( 8 ) ) );
		}

		/// <inheritdoc cref="WriteRandom(IMemobus, ushort[], byte[])"/>
		public static async Task<OperateResult> WriteRandomAsync( IMemobus memobus, ushort[] address, byte[] value )
		{
			OperateResult<byte[]> command = BuildWriteRandomCommand( 0x20, 0x0E, memobus.CpuTo, memobus.CpuFrom, address, value );
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> read = await memobus.ReadFromCoreServerAsync( command.Content );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( read );

			return OperateResult.CreateSuccessResult( );
		}
#endif
		#endregion
	}
}
