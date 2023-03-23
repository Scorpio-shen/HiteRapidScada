using HslCommunication.BasicFramework;
using HslCommunication.Core;
using HslCommunication.Core.IMessage;
using HslCommunication.Core.Net;
using HslCommunication.Reflection;
using HslCommunication.ModBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if !NET35 && !NET20
using System.Threading.Tasks;
#endif

namespace HslCommunication.Profinet.XINJE
{
	/// <summary>
	/// 信捷内部TCP的虚拟服务器类
	/// </summary>
	public class XinJEServer : ModbusTcpServer
	{
		#region Constructor

		/// <summary>
		/// 实例化一个默认参数的mc协议的服务器<br />
		/// Instantiate a mc protocol server with default parameters
		/// </summary>
		public XinJEServer( )
		{
			// 共计使用了如下的数据池
			mBuffer  = new SoftBuffer( DataPoolLength );
			smBuffer = new SoftBuffer( DataPoolLength );
			dBuffer  = new SoftBuffer( 500_000 * 2 );
			sdBuffer = new SoftBuffer( DataPoolLength * 2 );
			hdBuffer = new SoftBuffer( DataPoolLength * 2 );

			this.WordLength = 1;
			this.ByteTransform = new ReverseWordTransform( );
			this.ByteTransform.DataFormat = DataFormat.CDAB;
		}

		#endregion

		#region NetworkDataServerBase Override

		/// <inheritdoc/>
		[HslMqttApi( "ReadByteArray", "" )]
		public override OperateResult<byte[]> Read( string address, ushort length )
		{
			return base.Read( address, length );
		}

		/// <inheritdoc/>
		[HslMqttApi( "WriteByteArray", "" )]
		public override OperateResult Write( string address, byte[] value )
		{
			return base.Write(address, value);
		}

		#endregion

		#region Bool Read Write Operate

		/// <inheritdoc/>
		[HslMqttApi( "ReadBoolArray", "" )]
		public override OperateResult<bool[]> ReadBool( string address, ushort length )
		{
			return base.ReadBool( address, length );
		}

		/// <inheritdoc/>
		[HslMqttApi( "WriteBoolArray", "" )]
		public override OperateResult Write( string address, bool[] value )
		{
			return base.Write( address, value );
		}

		#endregion

		#region NetServer Override

		/// <inheritdoc/>
		protected override OperateResult<byte[]> ReadFromCoreServer( AppSession session, byte[] receive )
		{
			// 检查站号
			if (receive[7] == 0x20 || receive[7] == 0x1E)
			{
				if (receive[6] != this.Station) return OperateResult.CreateSuccessResult( PackCommand( receive, 1, null ) );

				return OperateResult.CreateSuccessResult( ReadByCommand( receive ) );
			}
			else if (receive[7] == 0x21 || receive[7] == 0x1F)
			{
				if (receive[6] != this.Station) return OperateResult.CreateSuccessResult( PackCommand( receive, 1, null ) );

				return OperateResult.CreateSuccessResult( WriteByMessage( receive ) );
			}
			else return base.ReadFromCoreServer( session, receive );
		}

		/// <summary>
		/// 将状态码，数据打包成一个完成的回复报文信息
		/// </summary>
		/// <param name="command">原始的命令数据</param>
		/// <param name="status">状态信息</param>
		/// <param name="data">数据</param>
		/// <returns>状态信息</returns>
		private byte[] PackCommand( byte[] command, ushort status, byte[] data )
		{
			if (data == null)
			{
				byte[] buffer = command.SelectBegin( 14 );
				buffer[4] = 0x00;
				buffer[5] = 0x08;
				if (status == 0) return buffer;
				buffer[7] = (byte)(buffer[7] + 0x80 + status);
				return buffer;
			}
			else
			{
				byte[] buffer = new byte[9 + data.Length];
				Array.Copy( command, 0, buffer, 0, 8 );
				buffer[4] = 0x00;
				buffer[5] = (byte)(buffer.Length - 6);
				buffer[8] = (byte)data.Length;
				data.CopyTo( buffer, 9 );
				return buffer;
			}
		}

		private byte[] ReadByCommand( byte[] command )
		{
			ushort length = ByteTransform.TransUInt16( command, 12 );
			int startIndex = (command[9] * 65536 + command[10] * 256 + command[11]);
			byte type = command[8];

			if (command[7] == 0x20)
			{
				// 字操作
				if (length > 125) return PackCommand( command, 1, null );
				if      (type == 0x80) return PackCommand( command, 0, dBuffer. GetBytes( startIndex * 2, length * 2 ) );
				else if (type == 0x83) return PackCommand( command, 0, sdBuffer.GetBytes( startIndex * 2, length * 2 ) );
				else if (type == 0x88) return PackCommand( command, 0, hdBuffer.GetBytes( startIndex * 2, length * 2 ) );
				else return PackCommand( command, 1, null );
			}
			else if (command[7] == 0x1E)
			{
				// 位操作
				if (length > 125 * 16) return PackCommand( command, 1, null );
				if      (type == 0x03) return PackCommand( command, 0, mBuffer. GetBool( startIndex, length ).ToByteArray( ) );
				else if (type == 0x0D) return PackCommand( command, 0, smBuffer.GetBool( startIndex, length ).ToByteArray( ) );
				else return PackCommand( command, 1, null );
			}
			return PackCommand( command, 1, null );
		}

		private byte[] WriteByMessage( byte[] command )
		{
			// 先判断是否有写入的权利，没有的话，直接返回写入异常
			if (!this.EnableWrite) return PackCommand( command, 1, null );

			ushort length = ByteTransform.TransUInt16( command, 12 );
			int startIndex = (command[9] * 65536 + command[10] * 256 + command[11]);
			byte type = command[8];

			if (command[7] == 0x21)
			{
				byte[] buffer = command.SelectMiddle( 15, command[14] );
				// 字操作
				if      (type == 0x80) { dBuffer.SetBytes(  buffer, startIndex * 2 ); return PackCommand( command, 0, null ); }
				else if (type == 0x83) { sdBuffer.SetBytes( buffer, startIndex * 2 ); return PackCommand( command, 0, null ); }
				else if (type == 0x88) { hdBuffer.SetBytes( buffer, startIndex * 2 ); return PackCommand( command, 0, null ); }
				else return PackCommand( command, 1, null );
			}
			else if (command[7] == 0x1F)
			{
				bool[] buffer = command.SelectMiddle( 15, command[14] ).ToBoolArray( ).SelectBegin( length );
				// 位操作
				if      (type == 0x03) { mBuffer.SetBool( buffer, startIndex ); return PackCommand( command, 0, null ); }
				else if (type == 0x0D) { smBuffer.SetBool( buffer, startIndex ); return PackCommand( command, 0, null ); }
				else return PackCommand( command, 1, null );
			}
			return PackCommand( command, 1, null );
		}

		#endregion

		#region Data Save Load Override

		///// <inheritdoc/>
		//protected override void LoadFromBytes( byte[] content )
		//{
		//	if (content.Length < DataPoolLength * 10) throw new Exception( "File is not correct" );

		//	mBuffer.SetBytes(  content, DataPoolLength * 0, 0, DataPoolLength );
		//	xBuffer.SetBytes(  content, DataPoolLength * 1, 0, DataPoolLength );
		//	yBuffer.SetBytes(  content, DataPoolLength * 2, 0, DataPoolLength );
		//	smBuffer.SetBytes( content, DataPoolLength * 3, 0, DataPoolLength );
		//	dBuffer.SetBytes(  content, DataPoolLength * 4, 0, DataPoolLength * 2 );
		//	sdBuffer.SetBytes( content, DataPoolLength * 6, 0, DataPoolLength * 2 );
		//	hdBuffer.SetBytes( content, DataPoolLength * 8, 0, DataPoolLength * 2 );
		//}

		///// <inheritdoc/>
		//[HslMqttApi]
		//protected override byte[] SaveToBytes( )
		//{
		//	byte[] buffer = new byte[DataPoolLength * 10];
		//	Array.Copy( mBuffer.GetBytes( ),  0, buffer, DataPoolLength * 0, DataPoolLength );
		//	Array.Copy( xBuffer.GetBytes( ),  0, buffer, DataPoolLength * 1, DataPoolLength );
		//	Array.Copy( yBuffer.GetBytes( ),  0, buffer, DataPoolLength * 2, DataPoolLength );
		//	Array.Copy( smBuffer.GetBytes( ), 0, buffer, DataPoolLength * 3, DataPoolLength );
		//	Array.Copy( dBuffer.GetBytes( ),  0, buffer, DataPoolLength * 4, DataPoolLength * 2 );
		//	Array.Copy( sdBuffer.GetBytes( ), 0, buffer, DataPoolLength * 6, DataPoolLength * 2 );
		//	Array.Copy( hdBuffer.GetBytes( ), 0, buffer, DataPoolLength * 8, DataPoolLength * 2 );
		//	return buffer;
		//}

		#endregion

		#region IDisposable Support

		///// <summary>
		///// 释放当前的对象
		///// </summary>
		///// <param name="disposing">是否托管对象</param>
		//protected override void Dispose( bool disposing )
		//{
		//	if (disposing)
		//	{
		//		xBuffer?.Dispose( );
		//		yBuffer?.Dispose( );
		//		mBuffer?.Dispose( );
		//		smBuffer?.Dispose( );
		//		dBuffer?.Dispose( );
		//		sdBuffer?.Dispose( );
		//		hdBuffer?.Dispose( );
		//	}
		//	base.Dispose( disposing );
		//}

		#endregion

		#region Private Member

		private SoftBuffer mBuffer;                    // m寄存器的数据池
		private SoftBuffer smBuffer;                   // sm寄存器的数据池
		private SoftBuffer dBuffer;                    // d寄存器的数据池
		private SoftBuffer sdBuffer;                   // sd寄存器的数据池
		private SoftBuffer hdBuffer;                   // hd继电器的数据池

		private const int DataPoolLength = 65536;      // 数据的长度

		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString( ) => $"XinJEServer[{Port}]";

		#endregion

	}
}
