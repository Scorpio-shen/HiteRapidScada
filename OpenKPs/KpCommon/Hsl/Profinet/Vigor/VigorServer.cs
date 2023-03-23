using HslCommunication.BasicFramework;
using HslCommunication.Core;
using HslCommunication.Core.IMessage;
using HslCommunication.Core.Net;
using HslCommunication.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
#if !NET20 && !NET35
using System.Threading.Tasks;
#endif

namespace HslCommunication.Profinet.Vigor
{
	/// <summary>
	/// 丰炜的虚拟PLC，模拟了VS系列的通信，可以和对应的客户端进行数据读写测试，位地址支持 X,Y,M,S，字地址支持 D,R,SD
	/// </summary>
	public class VigorServer : NetworkDataServerBase
	{
		#region Constructor

		/// <summary>
		/// 实例化一个丰炜PLC的网口和串口服务器，支持数据读写操作
		/// </summary>
		public VigorServer( )
		{
			// 四个数据池初始化，线圈，输入线圈，寄存器，只读寄存器
			xBuffer  = new SoftBuffer( DataPoolLength );
			yBuffer  = new SoftBuffer( DataPoolLength );
			mBuffer  = new SoftBuffer( DataPoolLength );
			sBuffer  = new SoftBuffer( DataPoolLength );
			dBuffer  = new SoftBuffer( DataPoolLength * 2 );
			rBuffer  = new SoftBuffer( DataPoolLength * 2 );
			sdBuffer = new SoftBuffer( DataPoolLength * 2 );

			ByteTransform = new RegularByteTransform( );
			WordLength = 1;
		}

		#endregion

		#region Public Members

		/// <inheritdoc cref="VigorSerial.Station"/>
		public int Station
		{
			get { return station; }
			set { station = value; }
		}

		#endregion

		#region Data Persistence

		/// <inheritdoc/>
		protected override byte[] SaveToBytes( )
		{
			byte[] buffer = new byte[DataPoolLength * 10];
			xBuffer. GetBytes( ).CopyTo( buffer, DataPoolLength * 0 );
			yBuffer. GetBytes( ).CopyTo( buffer, DataPoolLength * 1 );
			mBuffer. GetBytes( ).CopyTo( buffer, DataPoolLength * 2 );
			sBuffer. GetBytes( ).CopyTo( buffer, DataPoolLength * 3 );

			dBuffer. GetBytes( ).CopyTo( buffer, DataPoolLength * 4 );
			rBuffer. GetBytes( ).CopyTo( buffer, DataPoolLength * 6 );
			sdBuffer.GetBytes( ).CopyTo( buffer, DataPoolLength * 8 );
			return buffer;
		}

		/// <inheritdoc/>
		protected override void LoadFromBytes( byte[] content )
		{
			if (content.Length < DataPoolLength * 10) throw new Exception( "File is not correct" );
			xBuffer. SetBytes( content, 0 * DataPoolLength, DataPoolLength );
			yBuffer. SetBytes( content, 1 * DataPoolLength, DataPoolLength );
			mBuffer. SetBytes( content, 2 * DataPoolLength, DataPoolLength );
			sBuffer. SetBytes( content, 3 * DataPoolLength, DataPoolLength );
			dBuffer. SetBytes( content, 4 * DataPoolLength, DataPoolLength * 2 );
			rBuffer. SetBytes( content, 6 * DataPoolLength, DataPoolLength * 2 );
			sdBuffer.SetBytes( content, 8 * DataPoolLength, DataPoolLength * 2 );
		}

		#endregion

		#region NetworkDataServerBase Override

		/// <inheritdoc cref="VigorSerial.Read(string, ushort)"/>
		[HslMqttApi( "ReadByteArray", "" )]
		public override OperateResult<byte[]> Read( string address, ushort length )
		{
			var result = new OperateResult<byte[]>( );
			try
			{
				if (address.StartsWith( "SD" ) || address.StartsWith( "sd" ))
					return OperateResult.CreateSuccessResult( sdBuffer.GetBytes( Convert.ToInt32( address.Substring( 2 ) ) * 2, length * 2 ) );
				else if (address.StartsWith( "D" ) || address.StartsWith( "d" ))
					return OperateResult.CreateSuccessResult( dBuffer.GetBytes( Convert.ToInt32( address.Substring( 1 ) ) * 2, length * 2 ) );
				else if (address.StartsWith( "R" ) || address.StartsWith( "r" ))
					return OperateResult.CreateSuccessResult( rBuffer.GetBytes( Convert.ToInt32( address.Substring( 1 ) ) * 2, length * 2 ) );
				else
					throw new Exception( StringResources.Language.NotSupportedDataType );
			}
			catch (Exception ex)
			{
				result.Message = ex.Message;
				return result;
			}
		}

		/// <inheritdoc cref="VigorSerial.Write(string, byte[])"/>
		[HslMqttApi( "WriteByteArray", "" )]
		public override OperateResult Write( string address, byte[] value )
		{
			var result = new OperateResult<byte[]>( );
			try
			{
				if (address.StartsWith( "SD" ) || address.StartsWith( "sd" ))
				{
					sdBuffer.SetBytes( value, Convert.ToInt32( address.Substring( 2 ) ) * 2 ); return OperateResult.CreateSuccessResult( );
				}
				else if (address.StartsWith( "D" ) || address.StartsWith( "d" ) )
				{
					dBuffer.SetBytes( value, Convert.ToInt32( address.Substring( 1 ) ) * 2 ); return OperateResult.CreateSuccessResult( );
				}
				else if (address.StartsWith( "R" ) || address.StartsWith( "r" ))
				{
					rBuffer.SetBytes( value, Convert.ToInt32( address.Substring( 1 ) ) * 2 ); return OperateResult.CreateSuccessResult( );
				}
				else
					throw new Exception( StringResources.Language.NotSupportedDataType );
			}
			catch (Exception ex)
			{
				result.Message = ex.Message;
				return result;
			}
		}

		/// <inheritdoc cref="VigorSerial.ReadBool(string, ushort)"/>
		[HslMqttApi( "ReadBoolArray", "" )]
		public override OperateResult<bool[]> ReadBool( string address, ushort length )
		{
			try
			{
				int bitIndex = Convert.ToInt32( address.Substring( 1 ) );
				switch (address[0])
				{
					case 'X':
					case 'x': return OperateResult.CreateSuccessResult( xBuffer.GetBool( bitIndex, length ) );
					case 'Y':
					case 'y': return OperateResult.CreateSuccessResult( yBuffer.GetBool( bitIndex, length ) );
					case 'M':
					case 'm': return OperateResult.CreateSuccessResult( mBuffer.GetBool( bitIndex, length ) );
					case 'S':
					case 's': return OperateResult.CreateSuccessResult( sBuffer.GetBool( bitIndex, length ) );
					default: throw new Exception( StringResources.Language.NotSupportedDataType );
				}
			}
			catch (Exception ex)
			{
				return new OperateResult<bool[]>( ex.Message );
			}
		}

		/// <inheritdoc cref="NetworkDeviceBase.Write(string, bool[])"/>
		[HslMqttApi( "WriteBoolArray", "" )]
		public override OperateResult Write( string address, bool[] value )
		{
			try
			{
				int bitIndex = Convert.ToInt32( address.Substring( 1 ) );
				switch (address[0])
				{
					case 'X':
					case 'x': xBuffer.SetBool( value, bitIndex ); return OperateResult.CreateSuccessResult( );
					case 'Y':
					case 'y': yBuffer.SetBool( value, bitIndex ); return OperateResult.CreateSuccessResult( );
					case 'M':
					case 'm': mBuffer.SetBool( value, bitIndex ); return OperateResult.CreateSuccessResult( );
					case 'S':
					case 's': sBuffer.SetBool( value, bitIndex ); return OperateResult.CreateSuccessResult( );
					default: throw new Exception( StringResources.Language.NotSupportedDataType );
				}
			}
			catch (Exception ex)
			{
				return new OperateResult<bool[]>( ex.Message );
			}
		}

		#endregion

		#region NetServer Override

		/// <inheritdoc/>
		protected override OperateResult<byte[]> ReceiveByMessage( Socket socket, int timeOut, INetMessage netMessage, Action<long, long> reportProgress = null )
		{
			return ReceiveVigorMessage( socket, 2000 );
		}
#if !NET20 && !NET35
		/// <inheritdoc/>
		protected async override Task<OperateResult<byte[]>> ReceiveByMessageAsync( Socket socket, int timeOut, INetMessage netMessage, Action<long, long> reportProgress = null )
		{
			return await ReceiveVigorMessageAsync( socket, 2000 );
		}
#endif
		/// <inheritdoc/>
		protected override OperateResult<byte[]> ReadFromCoreServer( AppSession session, byte[] receive )
		{
			if (receive[0] != AsciiControl.DLE || receive[1] != AsciiControl.STX) return new OperateResult<byte[]>( "start message must be 0x10, 0x02" );

			if (receive[2] != station)
				return new OperateResult<byte[]>( $"Station not match , Except: {station:X2} , Actual: {receive[2]}" );

			return OperateResult.CreateSuccessResult( ReadFromVigorCore( Helper.VigorVsHelper.UnPackCommand( receive ) ) );
		}

		#endregion

		#region Core Read

		private byte[] CreateResponseBack( byte[] request, byte err, byte[] data )
		{
			if(data == null) data = new byte[0];
			byte[] command = new byte[4 + data.Length];
			command[0] = request[2];
			command[1] = BitConverter.GetBytes( 1 + data.Length )[0];
			command[2] = BitConverter.GetBytes( 1 + data.Length )[1];
			command[3] = err;
			if (data.Length > 0) data.CopyTo( command, 4 );
			return Helper.VigorVsHelper.PackCommand( command, 0x06 );
		}


		private byte[] ReadFromVigorCore( byte[] receive )
		{
			if (receive.Length < 16) return null;
			if      (receive[5] == 0x20) return ReadWordByCommand( receive );
			else if (receive[5] == 0x21) return ReadBoolByCommand( receive );
			else if (receive[5] == 0x28) return WriteWordByCommand( receive );
			else if (receive[5] == 0x29) return WriteBoolByCommand( receive );
			return CreateResponseBack( receive, 0x31, null );
		}

		private byte[] ReadWordByCommand( byte[] command )
		{
			int address = Convert.ToInt32( command.SelectMiddle( 7, 3 ).Reverse( ).ToArray( ).ToHexString( ) );
			int length = ByteTransform.TransUInt16( command, 10 );

			switch (command[6])
			{
				case 0xA0: return CreateResponseBack( command, 0x00, dBuffer.GetBytes( address * 2, length * 2 ) );
				case 0xA1: return CreateResponseBack( command, 0x00, sdBuffer.GetBytes( address * 2, length * 2 ) );
				case 0xA2: return CreateResponseBack( command, 0x00, rBuffer.GetBytes( address * 2, length * 2 ) );
				default: return CreateResponseBack( command, 0x31, null );
			}
		}
		private byte[] ReadBoolByCommand( byte[] command )
		{
			string address = command.SelectMiddle( 7, 3 ).Reverse( ).ToArray( ).ToHexString( );
			int length = ByteTransform.TransUInt16( command, 10 );

			switch (command[6])
			{
				case 0x90: return CreateResponseBack( command, 0x00, xBuffer.GetBool( Convert.ToInt32( address, 8 ), length ).ToByteArray( ) );
				case 0x91: return CreateResponseBack( command, 0x00, yBuffer.GetBool( Convert.ToInt32( address, 8 ), length ).ToByteArray( ) );
				case 0x92: return CreateResponseBack( command, 0x00, mBuffer.GetBool( Convert.ToInt32( address ), length ).ToByteArray( ) );
				case 0x93: return CreateResponseBack( command, 0x00, sBuffer.GetBool( Convert.ToInt32( address ), length ).ToByteArray( ) );
				default: return CreateResponseBack( command, 0x31, null );
			}
		}

		private byte[] WriteWordByCommand( byte[] command )
		{
			// 先判断是否有写入的权利，没有的话，直接返回写入异常
			if (!this.EnableWrite) return CreateResponseBack( command, 0x31, null );

			int address = Convert.ToInt32( command.SelectMiddle( 7, 3 ).Reverse( ).ToArray( ).ToHexString( ) );
			int byteLength = ByteTransform.TransUInt16( command, 3 ) - 7;
			byte[] data = command.SelectMiddle( 12, byteLength );

			switch (command[6])
			{
				case 0xA0: dBuffer.SetBytes( data, address * 2 ); return CreateResponseBack( command, 0x00, null );
				case 0xA1: sdBuffer.SetBytes( data, address * 2 ); return CreateResponseBack( command, 0x00, null );
				case 0xA2: rBuffer.SetBytes( data, address * 2 ); return CreateResponseBack( command, 0x00, null );
				default: return CreateResponseBack( command, 0x31, null );
			}
		}

		private byte[] WriteBoolByCommand( byte[] command )
		{
			// 先判断是否有写入的权利，没有的话，直接返回写入异常
			if (!this.EnableWrite) return CreateResponseBack( command, 0x31, null );

			string address = command.SelectMiddle( 7, 3 ).Reverse( ).ToArray( ).ToHexString( );
			int byteLength = ByteTransform.TransUInt16( command, 3 ) - 7;
			int bitLength = ByteTransform.TransUInt16( command, 10 );
			bool[] data = command.SelectMiddle( 12, byteLength ).ToBoolArray( ).SelectBegin( bitLength );

			switch (command[6])
			{
				case 0x90: xBuffer.SetBool( data, Convert.ToInt32( address, 8 ) ); return CreateResponseBack( command, 0x00, null );
				case 0x91: yBuffer.SetBool( data, Convert.ToInt32( address, 8 ) ); return CreateResponseBack( command, 0x00, null );
				case 0x92: mBuffer.SetBool( data, Convert.ToInt32( address ) ); return CreateResponseBack( command, 0x00, null );
				case 0x93: sBuffer.SetBool( data, Convert.ToInt32( address ) ); return CreateResponseBack( command, 0x00, null );
				default: return CreateResponseBack( command, 0x31, null );
			}
		}

		#endregion

		#region Serial Support

		/// <inheritdoc/>
		protected override bool CheckSerialReceiveDataComplete( byte[] buffer, int receivedLength )
		{
			return Helper.VigorVsHelper.CheckReceiveDataComplete( buffer, receivedLength );
		}

		#endregion

		#region IDisposable Support

		/// <inheritdoc/>
		protected override void Dispose( bool disposing )
		{
			if (disposing)
			{
				xBuffer.Dispose( );
				yBuffer.Dispose( );
				mBuffer.Dispose( );
				sBuffer.Dispose( );
				dBuffer.Dispose( );
				rBuffer.Dispose( );
				sdBuffer.Dispose( );
			}
			base.Dispose( disposing );
		}

		#endregion

		#region Private Member

		private SoftBuffer xBuffer;       // 输入继电器的数据池
		private SoftBuffer yBuffer;       // 输出继电器的数据池
		private SoftBuffer mBuffer;       // 中间继电器的数据池
		private SoftBuffer sBuffer;       // 状态继电器的数据池
		private SoftBuffer dBuffer;       // 数据寄存器的数据池
		private SoftBuffer rBuffer;       // 文件寄存器的数据池
		private SoftBuffer sdBuffer;      // 扩展寄存器的数据池

		private const int DataPoolLength = 65536;     // 数据的长度
		private int station = 0;                      // 服务器的站号数据，对于tcp无效，对于串口来说，如果小于0，则忽略站号信息

		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString( ) => $"VigorServer[{Port}]";

		#endregion

	}
}
