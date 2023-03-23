using HslCommunication.BasicFramework;
using HslCommunication.Core;
using HslCommunication.Core.Address;
using HslCommunication.Core.IMessage;
using HslCommunication.Core.Net;
using HslCommunication.Profinet.YASKAWA.Helper;
using HslCommunication.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if !NET20 && !NET35
using System.Threading.Tasks;
#endif

namespace HslCommunication.Profinet.YASKAWA
{
	/// <summary>
	/// 基于扩展memobus协议的虚拟服务器，支持线圈，输入线圈，保持型寄存器，输入寄存器，保持型寄存器（扩展），输入寄存器（扩展）的读写。对于远程客户端来说，还支持对扩展保持寄存器的随机字读取和写入操作。<br />
	/// A virtual server based on the extended memobus protocol, which supports the reading and writing of coils, input coils, holding registers, input registers, 
	/// holding registers (expansion), input registers (expansion). For remote clients, random word reads and writes to extended hold registers are also supported.
	/// </summary>
	/// <remarks>
	/// 支持的功能码为 01 02 03 04 05 06 08 09 0A 0B 0D 0E 0F 10, 访问方式分为位读写和字读写，位读写：线圈：100; 输入线圈：x=2;100<br />
	/// 字读写时，保持型寄存器：100; 输入寄存器：x=4;100  保持型寄存器（扩展）: x=9;100   输入寄存器（扩展）: x=10;100
	/// </remarks>
	public class MemobusTcpServer : NetworkDataServerBase
	{
		#region Constructor

		/// <summary>
		/// 实例化一个memobus协议的PLC的服务器，支持线圈，输入线圈，保持型寄存器，输入寄存器，保持型寄存器（扩展），输入寄存器（扩展）的读写。
		/// </summary>
		public MemobusTcpServer( )
		{
			// 四个数据池初始化，输入寄存器，输出寄存器，中间寄存器，DB块寄存器
			xBuffer        = new SoftBuffer( DataPoolLength );
			yBuffer        = new SoftBuffer( DataPoolLength );
			inputBuffer    = new SoftBuffer( DataPoolLength * 2 );
			rBuffer        = new SoftBuffer( DataPoolLength * 2 );
			inputExtBuffer = new SoftBuffer( DataPoolLength * 2 );
			rExtBuffer     = new SoftBuffer( DataPoolLength * 2 );

			WordLength = 2;
			ByteTransform  = new ReverseWordTransform( );
			ByteTransform.DataFormat = DataFormat.CDAB;
		}

		#endregion

		#region NetworkDataServerBase Override

		private OperateResult<SoftBuffer> GetDataAreaFromYokogawaAddress( MemobusAddress address, bool isBit )
		{
			if (isBit)
			{
				switch (address.SFC)
				{
					case 0x01:
					case 0x05:
					case 0x0F: return OperateResult.CreateSuccessResult( yBuffer );
					case 0x02: return OperateResult.CreateSuccessResult( xBuffer );
					default: return new OperateResult<SoftBuffer>( StringResources.Language.NotSupportedDataType );
				}
			}
			else
			{
				switch (address.SFC)
				{
					case 0x03:
					case 0x06:
					case 0x10: return OperateResult.CreateSuccessResult( rBuffer );
					case 0x04: return OperateResult.CreateSuccessResult( inputBuffer );
					case 0x09:
					case 0x0B:
					case 0x0D:
					case 0x0E: return OperateResult.CreateSuccessResult( rExtBuffer );
					case 0x0A: return OperateResult.CreateSuccessResult( inputExtBuffer );
					default: return new OperateResult<SoftBuffer>( StringResources.Language.NotSupportedDataType );
				}
			}
		}

		/// <inheritdoc cref="MemobusTcpNet.Read(string, ushort)"/>
		[HslMqttApi( "ReadByteArray", "" )]
		public override OperateResult<byte[]> Read( string address, ushort length )
		{
			OperateResult<MemobusAddress> analysis = MemobusAddress.ParseFrom( address, false );
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( analysis );

			OperateResult<SoftBuffer> buffer = GetDataAreaFromYokogawaAddress( analysis.Content, false );
			if (!buffer.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( buffer );

			return OperateResult.CreateSuccessResult( buffer.Content.GetBytes( analysis.Content.AddressStart * 2, length * 2 ) );
		}

		/// <inheritdoc cref="MemobusTcpNet.Write(string, byte[])"/>
		[HslMqttApi( "WriteByteArray", "" )]
		public override OperateResult Write( string address, byte[] value )
		{
			OperateResult<MemobusAddress> analysis = MemobusAddress.ParseFrom( address, false );
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( analysis );

			OperateResult<SoftBuffer> buffer = GetDataAreaFromYokogawaAddress( analysis.Content, false );
			if (!buffer.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( buffer );

			buffer.Content.SetBytes( value, analysis.Content.AddressStart * 2 );
			return OperateResult.CreateSuccessResult( );
		}

		#endregion

		#region Bool Read Write Operate

		/// <inheritdoc cref="MemobusTcpNet.ReadBool(string, ushort)"/>
		[HslMqttApi( "ReadBoolArray", "" )]
		public override OperateResult<bool[]> ReadBool( string address, ushort length )
		{
			OperateResult<MemobusAddress> analysis = MemobusAddress.ParseFrom( address, true );
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( analysis );

			OperateResult<SoftBuffer> buffer = GetDataAreaFromYokogawaAddress( analysis.Content, true );
			if (!buffer.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( buffer );

			return OperateResult.CreateSuccessResult( buffer.Content.GetBytes( analysis.Content.AddressStart, length ).Select( m => m != 0x00 ).ToArray( ) );
		}

		/// <inheritdoc cref="MemobusTcpNet.Write(string, bool[])"/>
		[HslMqttApi( "WriteBoolArray", "" )]
		public override OperateResult Write( string address, bool[] value )
		{
			OperateResult<MemobusAddress> analysis = MemobusAddress.ParseFrom( address, true );
			if (!analysis.IsSuccess) return analysis;

			OperateResult<SoftBuffer> buffer = GetDataAreaFromYokogawaAddress( analysis.Content, true );
			if (!buffer.IsSuccess) return buffer;

			buffer.Content.SetBytes( value.Select( m => m ? (byte)1 : (byte)0 ).ToArray( ), analysis.Content.AddressStart );
			return OperateResult.CreateSuccessResult( );
		}

		#endregion

		#region NetServer Override

		/// <inheritdoc/>
		protected override INetMessage GetNewNetMessage( ) => new MemobusMessage( );

		/// <inheritdoc/>
		protected override OperateResult<byte[]> ReadFromCoreServer( AppSession session, byte[] receive )
		{
			byte[] back = null;
			if      (receive[15] == 0x01 || receive[15] == 0x02) back = ReadBoolByCommand( receive );
			else if (receive[15] == 0x05 || receive[15] == 0x0F) back = WriteBoolByCommand( receive );
			else if (receive[15] == 0x03 || receive[15] == 0x04 || receive[15] == 0x09 || receive[15] == 0x0A) back = ReadWordByCommand( receive );
			else if (receive[15] == 0x06 || receive[15] == 0x10 || receive[15] == 0x0B) back = WriteWordByCommand( receive );
			else if (receive[15] == 0x0D) back = ReadRandomWordByCommand( receive );
			else if (receive[15] == 0x0E) back = WriteRandomWordByCommand( receive );
			else if (receive[15] == 0x08) back = PackCommandBack( receive, 0x00, null );
			else back = PackCommandBack( receive, 0x01, null );
			return OperateResult.CreateSuccessResult( back );
		}

		private byte[] ReadBoolByCommand( byte[] command )
		{
			int address = ByteTransform.TransUInt16( command, 17 );
			int length  = ByteTransform.TransUInt16( command, 19 );

			// if (length > 256) return PackCommandBack( command, 0x05, null );
			if (address + length > 65535) return PackCommandBack( command, 0x03, null );
			switch (command[15])
			{
				case 0x01: return PackCommandBack( command, 0x00, yBuffer.GetBytes( address, length ).Select( m => m != 0x00 ).ToArray( ).ToByteArray( ) );
				case 0x02: return PackCommandBack( command, 0x00, xBuffer.GetBytes( address, length ).Select( m => m != 0x00 ).ToArray( ).ToByteArray( ) );
				default:   return PackCommandBack( command, 0x01, null );
			}
		}

		private byte[] WriteBoolByCommand( byte[] command )
		{
			// 先判断是否有写入的权利，没有的话，直接返回写入异常
			if (!EnableWrite) return PackCommandBack( command, 0x03, null );
			
			int address = ByteTransform.TransUInt16( command, 17 );
			int length  = ByteTransform.TransUInt16( command, 19 );

			// if (length > 256) return PackCommandBack( command, 0x05, null );
			switch (command[15])
			{
				case 0x05:
					{
						if (command.Length != (12 + 7 + 2)) return PackCommandBack( command, 0x05, null );
						yBuffer.SetBytes( new byte[] { command[19] }, address );
						return PackCommandBack( command, 0x00, null );
					}
				case 0x0f:
					{
						if (address + length > 65535) return PackCommandBack( command, 0x03, null );

						if (command.Length != (12 + 7 + 2 + (length + 7) / 8)) return PackCommandBack( command, 0x05, null );
						bool[] values = command.RemoveBegin( 21 ).ToBoolArray( ).SelectBegin( length );
						yBuffer.SetBytes( values.Select( m => m ? (byte)0xff : (byte)0x00 ).ToArray( ), address );
						return PackCommandBack( command, 0x00, null );
					}
				default: return PackCommandBack( command, 0x01, null );
			}
		}

		private byte[] ReadWordByCommand( byte[] command )
		{
			if (command[15] == 0x03 || command[15] == 0x04)
			{
				int address = ByteTransform.TransUInt16( command, 17 );
				int length  = ByteTransform.TransUInt16( command, 19 );

				// if (length > 64) return PackCommandBack( command, 0x05, null );
				if (address + length > 65535) return PackCommandBack( command, 0x03, null );
				if (command[15] == 0x03) return PackCommandBack( command, 0x00, rBuffer.    GetBytes( address * 2, length * 2 ) );
				if (command[15] == 0x04) return PackCommandBack( command, 0x00, inputBuffer.GetBytes( address * 2, length * 2 ) );
			}
			else
			{
				int address = BitConverter.ToUInt16( command, 18 );
				int length  = BitConverter.ToUInt16( command, 20 );

				// if (length > 64) return PackCommandBack( command, 0x05, null );
				if (address + length > 65535) return PackCommandBack( command, 0x03, null );
				if (command[15] == 0x09) return PackCommandBack( command, 0x00, SoftBasic.BytesReverseByWord( rExtBuffer.    GetBytes( address * 2, length * 2 ) ) );
				if (command[15] == 0x0A) return PackCommandBack( command, 0x00, SoftBasic.BytesReverseByWord( inputExtBuffer.GetBytes( address * 2, length * 2 ) ) );
			}
			return PackCommandBack( command, 0x01, null );
		}

		private byte[] WriteWordByCommand( byte[] command )
		{
			// 先判断是否有写入的权利，没有的话，直接返回写入异常
			if (!EnableWrite) return PackCommandBack( command, 0x03, null );
			
			if (command[15] == 0x06 || command[15] == 0x10)
			{
				int address = ByteTransform.TransUInt16( command, 17 );
				int length  = ByteTransform.TransUInt16( command, 19 );

				//if (length > 64) return PackCommandBack( command, 0x05, null );
				if (command[15] == 0x06)
				{
					if (command.Length != (12 + 7 + 2)) return PackCommandBack( command, 0x03, null );

					rBuffer.SetBytes( command.SelectLast( 2 ), address * 2 ); 
					return PackCommandBack( command, 0x00, null );
				}
				else
				{
					if (address + length > 65535) return PackCommandBack( command, 0x03, null );
					if (command.Length != (12 + 7 + 2 + length * 2)) return PackCommandBack( command, 0x03, null );

					rBuffer.SetBytes( command.RemoveBegin( 21 ), address * 2 );
					return PackCommandBack( command, 0x00, null );
				}
			}
			else if (command[15] == 0x0B)
			{
				int address = BitConverter.ToUInt16( command, 18 );
				int length  = BitConverter.ToUInt16( command, 20 );

				if (address + length > 65535) return PackCommandBack( command, 0x03, null );
				if (command.Length != (12 + 8 + 2 + length * 2)) return PackCommandBack( command, 0x03, null );

				rExtBuffer.SetBytes( SoftBasic.BytesReverseByWord( command.RemoveBegin( 22 ) ), address * 2 );
				return PackCommandBack( command, 0x00, null );
			}
			return PackCommandBack( command, 0x01, null );
		}

		private byte[] ReadRandomWordByCommand( byte[] command )
		{
			int length = BitConverter.ToUInt16( command, 18 );
			if (command.Length != (12 + 6 + 2 + length * 2)) return PackCommandBack( command, 0x03, null );

			// 提取所有的随机读取的字数据信息，注意高低字节翻转
			byte[] buffer = new byte[length * 2];
			for (int i = 0; i < length; i++)
			{
				int address = BitConverter.ToUInt16( command, 20 + i * 2 );

				byte[] bytes = rExtBuffer.GetBytes( address * 2, 2 );
				buffer[i * 2 + 0] = bytes[1];
				buffer[i * 2 + 1] = bytes[0];
			}

			return PackCommandBack( command, 0x00, buffer );
		}

		private byte[] WriteRandomWordByCommand( byte[] command )
		{
			// 先判断是否有写入的权利，没有的话，直接返回写入异常
			if (!EnableWrite) return PackCommandBack( command, 0x03, null );

			int length = BitConverter.ToUInt16( command, 18 );
			if (command.Length != (12 + 6 + 2 + length * 4)) return PackCommandBack( command, 0x03, null );

			for (int i = 0; i < length; i++)
			{
				int address = BitConverter.ToUInt16( command, 20 + i * 4 );

				rExtBuffer.SetValue( command[20 + i * 4 + 2], address * 2 + 1 );
				rExtBuffer.SetValue( command[20 + i * 4 + 3], address * 2 + 0 );
			}

			return PackCommandBack( command, 0x00, null );
		}

		private byte TransByteHighLow( byte value )
		{
			int tmp = value & 0xf0;
			tmp >>= 4;
			return (byte)(((value & 0x0f) << 4) | tmp);
		}

		private byte[] PackCommandBack( byte[] cmds, byte err, byte[] result )
		{
			if (result == null) result = new byte[0];
			if (err > 0)
			{
				byte[] back = new byte[6 + result.Length];
				back[0] = 0x04;                        // Length L
				back[1] = 0x00;                        // Length H
				back[2] = cmds[14];                    // MFC
				back[3] = (byte)(cmds[15] + 0x80);     // SFC
				back[4] = TransByteHighLow(cmds[16]);  // CPU Number
				back[5] = err;                         // Error
				return MemobusHelper.PackCommandWithHeader( back, cmds[1] );
			}
			else
			{
				if (cmds[15] == 0x01 ||
					cmds[15] == 0x02 ||
					cmds[15] == 0x03 ||
					cmds[15] == 0x04)
				{
					byte[] back = new byte[5 + result.Length];
					back[0] = BitConverter.GetBytes( 3 + result.Length )[0];    // Length L
					back[1] = BitConverter.GetBytes( 3 + result.Length )[1];    // Length H
					back[2] = cmds[14];                                         // MFC
					back[3] = cmds[15];                                         // SFC
					back[4] = TransByteHighLow( cmds[16] );                     // CPU Number
					result.CopyTo( back, 5 );
					return MemobusHelper.PackCommandWithHeader( back, cmds[1] );
				}
				else if (
					cmds[15] == 0x05 ||
					cmds[15] == 0x06 ||
					cmds[15] == 0x08)
				{
					byte[] back = cmds.RemoveBegin( 12 );
					back[0] = BitConverter.GetBytes( back.Length - 2 )[0];      // Length L
					back[1] = BitConverter.GetBytes( back.Length - 2 )[1];      // Length H
					back[4] = TransByteHighLow( cmds[16] );                     // CPU Number
					return MemobusHelper.PackCommandWithHeader( back, cmds[1] );
				}
				else if (
					cmds[15] == 0x09 ||
					cmds[15] == 0x0A ||
					cmds[15] == 0x0D)
				{
					byte[] back = new byte[8 + result.Length];
					back[0] = BitConverter.GetBytes( back.Length - 2 )[0];      // Length L
					back[1] = BitConverter.GetBytes( back.Length - 2 )[1];      // Length H
					back[2] = cmds[14];                                         // MFC
					back[3] = cmds[15];                                         // SFC
					back[4] = TransByteHighLow( cmds[16] );                     // CPU Number
					back[6] = BitConverter.GetBytes( result.Length / 2 )[0];    // regist length L
					back[7] = BitConverter.GetBytes( result.Length / 2 )[1];    // regist length H
					result.CopyTo( back, 8 );
					return MemobusHelper.PackCommandWithHeader( back, cmds[1] );
				}
				else if (cmds[15] == 0x0B)
				{
					byte[] back = cmds.SelectMiddle( 12, 10 );
					back[0] = BitConverter.GetBytes( back.Length - 2 )[0];      // Length L
					back[1] = BitConverter.GetBytes( back.Length - 2 )[1];      // Length H
					back[4] = TransByteHighLow( cmds[16] );                     // CPU Number
					return MemobusHelper.PackCommandWithHeader( back, cmds[1] );
				}
				else if (cmds[15] == 0x0E)
				{
					byte[] back = cmds.SelectMiddle( 12, 8 );
					back[0] = BitConverter.GetBytes( back.Length - 2 )[0];      // Length L
					back[1] = BitConverter.GetBytes( back.Length - 2 )[1];      // Length H
					back[4] = TransByteHighLow( cmds[16] );                     // CPU Number
					return MemobusHelper.PackCommandWithHeader( back, cmds[1] );
				}
				else if (cmds[15] == 0x0F ||
					cmds[15] == 0x10)
				{
					byte[] back = cmds.SelectMiddle( 12, 9 );
					back[0] = BitConverter.GetBytes( back.Length - 2 )[0];      // Length L
					back[1] = BitConverter.GetBytes( back.Length - 2 )[1];      // Length H
					back[4] = TransByteHighLow( cmds[16] );                     // CPU Number
					return MemobusHelper.PackCommandWithHeader( back, cmds[1] );
				}
			}
			return PackCommandBack( cmds, 0x03, null );
		}

		#endregion

		#region Data Save Load Override

		/// <inheritdoc/>
		protected override void LoadFromBytes( byte[] content )
		{
			if (content.Length < DataPoolLength * 10) throw new Exception( "File is not correct" );

			xBuffer.SetBytes(        content, DataPoolLength * 0, 0, DataPoolLength );
			yBuffer.SetBytes(        content, DataPoolLength * 1, 0, DataPoolLength );
			inputBuffer.SetBytes(    content, DataPoolLength * 2, 0, DataPoolLength );
			rBuffer.SetBytes(        content, DataPoolLength * 4, 0, DataPoolLength );
			rExtBuffer.SetBytes(     content, DataPoolLength * 6, 0, DataPoolLength );
			inputExtBuffer.SetBytes( content, DataPoolLength * 8, 0, DataPoolLength );
		}

		/// <inheritdoc/>
		protected override byte[] SaveToBytes( )
		{
			byte[] buffer = new byte[DataPoolLength * 10];
			Array.Copy( xBuffer.GetBytes( ),        0, buffer, DataPoolLength * 0, DataPoolLength );
			Array.Copy( yBuffer.GetBytes( ),        0, buffer, DataPoolLength * 1, DataPoolLength );
			Array.Copy( inputBuffer.GetBytes( ),    0, buffer, DataPoolLength * 2, DataPoolLength * 2 );
			Array.Copy( rBuffer.GetBytes( ),        0, buffer, DataPoolLength * 4, DataPoolLength * 2 );
			Array.Copy( rExtBuffer.GetBytes( ),     0, buffer, DataPoolLength * 6, DataPoolLength * 2 );
			Array.Copy( inputExtBuffer.GetBytes( ), 0, buffer, DataPoolLength * 8, DataPoolLength * 2 );

			return buffer;
		}

		#endregion

		#region IDisposable Support

		/// <inheritdoc/>
		protected override void Dispose( bool disposing )
		{
			if (disposing)
			{
				xBuffer?.Dispose( );
				yBuffer?.Dispose( );
				inputBuffer?.Dispose( );
				rBuffer?.Dispose( );
				rExtBuffer?.Dispose( );
				inputExtBuffer?.Dispose( );
			}
			base.Dispose( disposing );
		}

		#endregion

		#region Private Member

		private SoftBuffer xBuffer;                    // 输入继电器的数据池
		private SoftBuffer yBuffer;                    // 输出继电器的数据池
		private SoftBuffer inputBuffer;                // 输入寄存器的数据池
		private SoftBuffer rBuffer;                    // 保持寄存器的数据池
		private SoftBuffer rExtBuffer;                 // 扩展保持寄存器的数据池
		private SoftBuffer inputExtBuffer;             // 扩展输入寄存器的数据池
		private const int DataPoolLength = 65536;      // 数据的长度

		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString( ) => $"MemobusTcpServer[{Port}]";

		#endregion
	}
}
