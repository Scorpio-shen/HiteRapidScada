using HslCommunication.BasicFramework;
using HslCommunication.Core;
using HslCommunication.Core.IMessage;
using HslCommunication.Core.Net;
using HslCommunication.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using HslCommunication.Serial;
using HslCommunication.Core.Address;

namespace HslCommunication.Profinet.FATEK
{
	/// <summary>
	/// 永宏编程口协议的虚拟PLC，可以用来和<see cref="FatekProgram"/>及<see cref="FatekProgramOverTcp"/>类做通信测试，支持简单数据的读写操作。
	/// </summary>
	public class FatekProgramServer : NetworkDataServerBase
	{
		#region Constructor

		/// <summary>
		/// 实例化一个基于Programe协议的虚拟的永宏PLC对象，可以用来和<see cref="FatekProgram"/>进行通信测试。
		/// </summary>
		public FatekProgramServer( )
		{
			xBuffer   = new SoftBuffer( DataPoolLength );
			yBuffer   = new SoftBuffer( DataPoolLength );
			mBuffer   = new SoftBuffer( DataPoolLength );
			sBuffer   = new SoftBuffer( DataPoolLength );
			tBuffer   = new SoftBuffer( DataPoolLength );
			cBuffer   = new SoftBuffer( DataPoolLength );
			tmrBuffer = new SoftBuffer( DataPoolLength * 2 );
			ctrBuffer = new SoftBuffer( DataPoolLength * 2 );
			hrBuffer  = new SoftBuffer( DataPoolLength * 2 );
			drBuffer  = new SoftBuffer( DataPoolLength * 2 );


			ByteTransform      = new ReverseWordTransform( );
			ByteTransform.DataFormat = DataFormat.CDAB;
			LogMsgFormatBinary = false;
			WordLength         = 1;
		}

		#endregion

		#region Public Properties

		/// <inheritdoc cref="FatekProgram.Station"/>
		public byte Station
		{
			get => this.station;
			set => this.station = value;
		}

		#endregion

		#region Data Persistence

		/// <inheritdoc/>
		protected override byte[] SaveToBytes( )
		{
			byte[] buffer = new byte[DataPoolLength * 14];
			xBuffer.  GetBytes( ).CopyTo( buffer, DataPoolLength * 0 );
			yBuffer.  GetBytes( ).CopyTo( buffer, DataPoolLength * 1 );
			mBuffer.  GetBytes( ).CopyTo( buffer, DataPoolLength * 2 );
			sBuffer.  GetBytes( ).CopyTo( buffer, DataPoolLength * 3 );
			tBuffer.  GetBytes( ).CopyTo( buffer, DataPoolLength * 4 );
			cBuffer.  GetBytes( ).CopyTo( buffer, DataPoolLength * 5 );
			tmrBuffer.GetBytes( ).CopyTo( buffer, DataPoolLength * 6 );
			ctrBuffer.GetBytes( ).CopyTo( buffer, DataPoolLength * 8 );
			hrBuffer. GetBytes( ).CopyTo( buffer, DataPoolLength * 10 );
			drBuffer. GetBytes( ).CopyTo( buffer, DataPoolLength * 12 );
			return buffer;
		}

		/// <inheritdoc/>
		protected override void LoadFromBytes( byte[] content )
		{
			if (content.Length < DataPoolLength * 14) throw new Exception( "File is not correct" );
			xBuffer.  SetBytes( content, 0  * DataPoolLength, DataPoolLength * 1 );
			yBuffer.  SetBytes( content, 1  * DataPoolLength, DataPoolLength * 1 );
			mBuffer.  SetBytes( content, 2  * DataPoolLength, DataPoolLength * 1 );
			sBuffer.  SetBytes( content, 3  * DataPoolLength, DataPoolLength * 1 );
			tBuffer.  SetBytes( content, 4  * DataPoolLength, DataPoolLength * 1 );
			cBuffer.  SetBytes( content, 5  * DataPoolLength, DataPoolLength * 1 );
			tmrBuffer.SetBytes( content, 6  * DataPoolLength, DataPoolLength * 2 );
			ctrBuffer.SetBytes( content, 8  * DataPoolLength, DataPoolLength * 2 );
			hrBuffer. SetBytes( content, 10 * DataPoolLength, DataPoolLength * 2 );
			drBuffer. SetBytes( content, 12 * DataPoolLength, DataPoolLength * 2 );
		}

		#endregion

		#region NetworkDataServerBase Override

		/// <inheritdoc/>
		[HslMqttApi( "ReadByteArray", "" )]
		public override OperateResult<byte[]> Read( string address, ushort length )
		{
			OperateResult<FatekProgramAddress> addressAnalysis = FatekProgramAddress.ParseFrom( address, length );
			if (!addressAnalysis.IsSuccess) return addressAnalysis.ConvertFailed<byte[]>( );

			if      (addressAnalysis.Content.DataCode == "D")  return OperateResult.CreateSuccessResult( drBuffer.GetBytes( addressAnalysis.Content.AddressStart * 2, length * 2 ) );
			else if (addressAnalysis.Content.DataCode == "R")  return OperateResult.CreateSuccessResult( hrBuffer.GetBytes( addressAnalysis.Content.AddressStart * 2, length * 2 ) );
			else if (addressAnalysis.Content.DataCode == "RT") return OperateResult.CreateSuccessResult( tmrBuffer.GetBytes( addressAnalysis.Content.AddressStart * 2, length * 2 ) );
			else if (addressAnalysis.Content.DataCode == "CT") return OperateResult.CreateSuccessResult( ctrBuffer.GetBytes( addressAnalysis.Content.AddressStart * 2, length * 2 ) );
			else if (addressAnalysis.Content.DataCode == "X")  return OperateResult.CreateSuccessResult( xBuffer.GetBool( addressAnalysis.Content.AddressStart, length * 16 ).ToByteArray( ) );
			else if (addressAnalysis.Content.DataCode == "Y")  return OperateResult.CreateSuccessResult( yBuffer.GetBool( addressAnalysis.Content.AddressStart, length * 16 ).ToByteArray( ) );
			else if (addressAnalysis.Content.DataCode == "M")  return OperateResult.CreateSuccessResult( mBuffer.GetBool( addressAnalysis.Content.AddressStart, length * 16 ).ToByteArray( ) );
			else if (addressAnalysis.Content.DataCode == "S")  return OperateResult.CreateSuccessResult( sBuffer.GetBool( addressAnalysis.Content.AddressStart, length * 16 ).ToByteArray( ) );
			else if (addressAnalysis.Content.DataCode == "T")  return OperateResult.CreateSuccessResult( tBuffer.GetBool( addressAnalysis.Content.AddressStart, length * 16 ).ToByteArray( ) );
			else if (addressAnalysis.Content.DataCode == "C")  return OperateResult.CreateSuccessResult( cBuffer.GetBool( addressAnalysis.Content.AddressStart, length * 16 ).ToByteArray( ) );
			else return new OperateResult<byte[]>( StringResources.Language.NotSupportedDataType );
		}

		/// <inheritdoc/>
		[HslMqttApi( "WriteByteArray", "" )]
		public override OperateResult Write( string address, byte[] value )
		{
			OperateResult<FatekProgramAddress> addressAnalysis = FatekProgramAddress.ParseFrom( address, 0 );
			if (!addressAnalysis.IsSuccess) return addressAnalysis.ConvertFailed<byte[]>( );

			if      (addressAnalysis.Content.DataCode == "D")  drBuffer.SetBytes( value, addressAnalysis.Content.AddressStart * 2 );
			else if (addressAnalysis.Content.DataCode == "R")  hrBuffer.SetBytes( value, addressAnalysis.Content.AddressStart * 2 );
			else if (addressAnalysis.Content.DataCode == "RT") tmrBuffer.SetBytes( value, addressAnalysis.Content.AddressStart * 2 );
			else if (addressAnalysis.Content.DataCode == "CT") ctrBuffer.SetBytes( value, addressAnalysis.Content.AddressStart * 2 );
			else if (addressAnalysis.Content.DataCode == "X")  xBuffer.SetBool( value.ToBoolArray( ), addressAnalysis.Content.AddressStart );
			else if (addressAnalysis.Content.DataCode == "Y")  yBuffer.SetBool( value.ToBoolArray( ), addressAnalysis.Content.AddressStart );
			else if (addressAnalysis.Content.DataCode == "M")  mBuffer.SetBool( value.ToBoolArray( ), addressAnalysis.Content.AddressStart );
			else if (addressAnalysis.Content.DataCode == "S")  sBuffer.SetBool( value.ToBoolArray( ), addressAnalysis.Content.AddressStart );
			else if (addressAnalysis.Content.DataCode == "T")  tBuffer.SetBool( value.ToBoolArray( ), addressAnalysis.Content.AddressStart );
			else if (addressAnalysis.Content.DataCode == "C")  cBuffer.SetBool( value.ToBoolArray( ), addressAnalysis.Content.AddressStart );
			else return new OperateResult<byte[]>( StringResources.Language.NotSupportedDataType );

			return OperateResult.CreateSuccessResult( );
		}

		/// <inheritdoc cref="IReadWriteNet.ReadBool(string, ushort)"/>
		[HslMqttApi( "ReadBoolArray", "" )]
		public override OperateResult<bool[]> ReadBool( string address, ushort length )
		{
			OperateResult<FatekProgramAddress> addressAnalysis = FatekProgramAddress.ParseFrom( address, length );
			if (!addressAnalysis.IsSuccess) return addressAnalysis.ConvertFailed<bool[]>( );

			if      (addressAnalysis.Content.DataCode == "X") return OperateResult.CreateSuccessResult( xBuffer.GetBool( addressAnalysis.Content.AddressStart, length ) );
			else if (addressAnalysis.Content.DataCode == "Y") return OperateResult.CreateSuccessResult( yBuffer.GetBool( addressAnalysis.Content.AddressStart, length ) );
			else if (addressAnalysis.Content.DataCode == "M") return OperateResult.CreateSuccessResult( mBuffer.GetBool( addressAnalysis.Content.AddressStart, length ) );
			else if (addressAnalysis.Content.DataCode == "S") return OperateResult.CreateSuccessResult( sBuffer.GetBool( addressAnalysis.Content.AddressStart, length ) );
			else if (addressAnalysis.Content.DataCode == "T") return OperateResult.CreateSuccessResult( tBuffer.GetBool( addressAnalysis.Content.AddressStart, length ) );
			else if (addressAnalysis.Content.DataCode == "C") return OperateResult.CreateSuccessResult( cBuffer.GetBool( addressAnalysis.Content.AddressStart, length ) );
			return new OperateResult<bool[]>( StringResources.Language.NotSupportedDataType );
		}

		/// <inheritdoc cref="IReadWriteNet.Write(string, bool[])"/>
		[HslMqttApi( "WriteBoolArray", "" )]
		public override OperateResult Write( string address, bool[] value )
		{
			OperateResult<FatekProgramAddress> addressAnalysis = FatekProgramAddress.ParseFrom( address, 0 );
			if (!addressAnalysis.IsSuccess) return addressAnalysis.ConvertFailed<byte[]>( );

			if      (addressAnalysis.Content.DataCode == "X") xBuffer.SetBool( value, addressAnalysis.Content.AddressStart );
			else if (addressAnalysis.Content.DataCode == "Y") yBuffer.SetBool( value, addressAnalysis.Content.AddressStart );
			else if (addressAnalysis.Content.DataCode == "M") mBuffer.SetBool( value, addressAnalysis.Content.AddressStart );
			else if (addressAnalysis.Content.DataCode == "S") sBuffer.SetBool( value, addressAnalysis.Content.AddressStart );
			else if (addressAnalysis.Content.DataCode == "T") tBuffer.SetBool( value, addressAnalysis.Content.AddressStart );
			else if (addressAnalysis.Content.DataCode == "C") cBuffer.SetBool( value, addressAnalysis.Content.AddressStart );
			else return new OperateResult<byte[]>( StringResources.Language.NotSupportedDataType );

			return OperateResult.CreateSuccessResult( );
		}

		#endregion

		#region NetServer Override

		/// <inheritdoc/>
		protected override INetMessage GetNewNetMessage( ) => new SpecifiedCharacterMessage( AsciiControl.ETX );

		/// <inheritdoc/>
		protected override bool CheckSerialReceiveDataComplete( byte[] buffer, int receivedLength )
		{
			if (receivedLength < 5) return false;
			return buffer[receivedLength - 1] == AsciiControl.ETX;
		}

		/// <inheritdoc/>
		protected override OperateResult<byte[]> ReadFromCoreServer( AppSession session, byte[] receive )
		{
			byte stat = Convert.ToByte( Encoding.ASCII.GetString( receive, 1, 2 ), 16 );
			if (stat != this.Station) return new OperateResult<byte[]>( $"Station is not match, need [{station}] but actual is [{stat}]" );

			string cmd = Encoding.ASCII.GetString( receive, 3, 2 );
			if (cmd == "44") return OperateResult.CreateSuccessResult( ReadBoolByMessage( receive ) );
			if (cmd == "45") return OperateResult.CreateSuccessResult( WriteBoolByMessage( receive ) );
			if (cmd == "46") return OperateResult.CreateSuccessResult( ReadWordByMessage( receive ) );
			if (cmd == "47") return OperateResult.CreateSuccessResult( WriteWordByMessage( receive ) );
			if (cmd == "41")
			{
				run = receive[5] == 0x31;
				return OperateResult.CreateSuccessResult( PackResponseBack( receive, (byte)'0', null ) );  // 启动或是停止
			}
			if (cmd == "40")
			{
				return OperateResult.CreateSuccessResult( PackResponseBack( receive, (byte)'0', 
					Encoding.ASCII.GetBytes( new byte[] { run ? (byte) 0x01 : (byte)0x00, 0x00, 0x00 }.ToHexString( ) )) );  // 系统的状态
			}

			return OperateResult.CreateSuccessResult( PackResponseBack( receive, (byte)'4', null ) );
		}

		#endregion

		#region Core Read

		private byte[] PackResponseBack( byte[] receive, byte err, byte[] value )
		{
			if (value == null ) value = new byte[0];

			byte[] buffer = new byte[9 + value.Length ];
			buffer[0] = AsciiControl.STX;
			buffer[1] = receive[1];
			buffer[2] = receive[2];
			buffer[3] = receive[3];
			buffer[4] = receive[4];
			buffer[5] = err;
			value.CopyTo( buffer, 6 );
			SoftLRC.CalculateAccAndFill( buffer, 0, 3 );
			buffer[buffer.Length - 1] = AsciiControl.ETX;
			return buffer;
		}

		private SoftBuffer GetBoolBuffer(char code )
		{
			switch (code)
			{
				case 'X': return xBuffer;
				case 'Y': return yBuffer;
				case 'M': return mBuffer;
				case 'S': return sBuffer;
				case 'T': return tBuffer;
				case 'C': return cBuffer;
				default: return null;
			}
		}

		private SoftBuffer GetWordBuffer(byte[] receive, out int address )
		{
			if (Encoding.ASCII.GetString( receive, 7, 2 ) == "RT")
			{
				address = Convert.ToInt32( Encoding.ASCII.GetString( receive, 9, 4 ) );
				return tmrBuffer;
			}
			else if (Encoding.ASCII.GetString( receive, 7, 2 ) == "RC")
			{
				address = Convert.ToInt32( Encoding.ASCII.GetString( receive, 9, 4 ) );
				return ctrBuffer;
			}
			else if (Encoding.ASCII.GetString( receive, 7, 1 ) == "D")
			{
				address = Convert.ToInt32( Encoding.ASCII.GetString( receive, 8, 5 ) );
				return drBuffer;
			}
			else if (Encoding.ASCII.GetString( receive, 7, 1 ) == "R")
			{
				address = Convert.ToInt32( Encoding.ASCII.GetString( receive, 8, 5 ) );
				return hrBuffer;
			}
			else
			{
				address = 0;
				return null;
			}
		}

		private byte[] ReadBoolByMessage( byte[] receive )
		{
			int length = Convert.ToInt32( Encoding.ASCII.GetString( receive, 5, 2 ), 16 );
			if (length == 0) length = 256;

			int address = Convert.ToInt32( Encoding.ASCII.GetString( receive, 8, 4 ), 10 );
			SoftBuffer softBuffer = GetBoolBuffer( (char)receive[7] );
			if (softBuffer == null) return PackResponseBack( receive, (byte)'4', null );

			return PackResponseBack( receive, (byte)'0', softBuffer.GetBool( address, length ).Select( m => m ? (byte)0x31 : (byte)0x30 ).ToArray( ) );
		}

		private byte[] WriteBoolByMessage( byte[] receive )
		{
			int length = Convert.ToInt32( Encoding.ASCII.GetString( receive, 5, 2 ), 16 );
			if (length == 0) length = 256;

			int address = Convert.ToInt32( Encoding.ASCII.GetString( receive, 8, 4 ), 10 );
			SoftBuffer softBuffer = GetBoolBuffer( (char)receive[7] );
			if (softBuffer == null) return PackResponseBack( receive, (byte)'4', null );

			bool[] value = receive.SelectMiddle( 12, length ).Select( m => m == 0x31 ? true : false ).ToArray( );
			softBuffer.SetBool( value, address );
			return PackResponseBack( receive, (byte)'0', null );
		}

		private byte[] ReadWordByMessage( byte[] receive )
		{
			int length = Convert.ToInt32( Encoding.ASCII.GetString( receive, 5, 2 ), 16 );
			if (length > 0x40) return PackResponseBack( receive, (byte)'2', null );

			SoftBuffer softBuffer = GetWordBuffer( receive, out int address );
			if (softBuffer == null) return PackResponseBack( receive, (byte)'4', null );

			return PackResponseBack( receive, (byte)'0', Encoding.ASCII.GetBytes( softBuffer.GetBytes( address * 2, length * 2 ).ToHexString( ) ) );
		}

		private byte[] WriteWordByMessage( byte[] receive )
		{
			int length = Convert.ToInt32( Encoding.ASCII.GetString( receive, 5, 2 ), 16 );
			if (length > 0x40) return PackResponseBack( receive, (byte)'2', null );

			SoftBuffer softBuffer = GetWordBuffer( receive, out int address );
			if (softBuffer == null) return PackResponseBack( receive, (byte)'4', null );

			byte[] hex = Encoding.ASCII.GetString( receive, 13, length * 4 ).ToHexBytes( );
			softBuffer.SetBytes( hex, address * 2 );
			return PackResponseBack( receive, (byte)'0', null );
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
				tBuffer.Dispose( );
				cBuffer.Dispose( );
				tmrBuffer.Dispose( );
				ctrBuffer.Dispose( );
				hrBuffer.Dispose( );
				drBuffer.Dispose( );
			}
			base.Dispose( disposing );
		}

		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString( ) => $"FatekProgramServer[{Port}]";

		#endregion

		#region Private Member

		private SoftBuffer xBuffer;        // X输入接点
		private SoftBuffer yBuffer;        // Y输出继电器
		private SoftBuffer mBuffer;        // M内部继电器
		private SoftBuffer sBuffer;        // S步进继电器
		private SoftBuffer tBuffer;        // T定时器接点
		private SoftBuffer cBuffer;        // C计数器接点
		private SoftBuffer tmrBuffer;      // 定时器缓存器
		private SoftBuffer ctrBuffer;      // 计数器缓存器
		private SoftBuffer hrBuffer;       // H资料缓存器
		private SoftBuffer drBuffer;       // D资料缓存器

		private byte station = 0x01;       // 站号信息
		private const int DataPoolLength = 65536;     // 数据的长度
		private bool run = false;

		#endregion
	}
}
