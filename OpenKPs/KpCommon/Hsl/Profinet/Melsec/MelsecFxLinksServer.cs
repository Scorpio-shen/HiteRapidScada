using HslCommunication.BasicFramework;
using HslCommunication.Core.Net;
using HslCommunication.Profinet.Melsec.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using HslCommunication.Core;
using System.Threading.Tasks;
using HslCommunication.Core.IMessage;
using HslCommunication.Serial;
using HslCommunication.Reflection;
using HslCommunication.Core.Address;

namespace HslCommunication.Profinet.Melsec
{
	/// <summary>
	/// 三菱的虚拟的FxLinks服务器
	/// </summary>
	public class MelsecFxLinksServer : NetworkDataServerBase
	{
		#region Constructor

		/// <summary>
		/// 实例化一个虚拟的FxLinks服务器
		/// </summary>
		public MelsecFxLinksServer( )
		{
			this.WordLength         = 1;
			this.ByteTransform      = new ReverseWordTransform( );
			this.ByteTransform.DataFormat = DataFormat.CDAB;
			this.LogMsgFormatBinary = false;

			this.xBuffer = new SoftBuffer( DataPoolLength * 2 );
			this.yBuffer = new SoftBuffer( DataPoolLength * 2 );
			this.mBuffer = new SoftBuffer( DataPoolLength * 2 );
			this.sBuffer = new SoftBuffer( DataPoolLength * 2 );
			this.dBuffer = new SoftBuffer( DataPoolLength * 2 );
			this.rBuffer = new SoftBuffer( DataPoolLength * 2 );

		}

		#endregion

		#region Public Properties

		/// <inheritdoc cref="IReadWriteA3C.Station"/>
		public byte Station { get; set; }

		/// <inheritdoc cref="IReadWriteA3C.SumCheck"/>
		public bool SumCheck { get; set; } = true;

		/// <inheritdoc cref="IReadWriteA3C.Format"/>
		public int Format { get; set; } = 1;

		#endregion

		#region NetworkDataServerBase Override

		/// <inheritdoc/>
		[HslMqttApi( "ReadByteArray", "" )]
		public override OperateResult<byte[]> Read( string address, ushort length )
		{
			OperateResult<MelsecFxLinksAddress> addressAnalysis = MelsecFxLinksAddress.ParseFrom( address );
			if (!addressAnalysis.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( addressAnalysis );

			// 分析地址
			if      (addressAnalysis.Content.TypeCode == "X") return OperateResult.CreateSuccessResult( xBuffer.GetBool( addressAnalysis.Content.AddressStart, length * 16 ).ToByteArray( ) );
			else if (addressAnalysis.Content.TypeCode == "Y") return OperateResult.CreateSuccessResult( yBuffer.GetBool( addressAnalysis.Content.AddressStart, length * 16 ).ToByteArray( ) );
			else if (addressAnalysis.Content.TypeCode == "M") return OperateResult.CreateSuccessResult( mBuffer.GetBool( addressAnalysis.Content.AddressStart, length * 16 ).ToByteArray( ) );
			else if (addressAnalysis.Content.TypeCode == "S") return OperateResult.CreateSuccessResult( sBuffer.GetBool( addressAnalysis.Content.AddressStart, length * 16 ).ToByteArray( ) );
			else if (addressAnalysis.Content.TypeCode == "D") return OperateResult.CreateSuccessResult( dBuffer.GetBytes( addressAnalysis.Content.AddressStart * 2, length * 2 ) );
			else if (addressAnalysis.Content.TypeCode == "R") return OperateResult.CreateSuccessResult( rBuffer.GetBytes( addressAnalysis.Content.AddressStart * 2, length * 2 ) );
			else return new OperateResult<byte[]>( StringResources.Language.NotSupportedDataType );
		}

		/// <inheritdoc/>
		[HslMqttApi( "WriteByteArray", "" )]
		public override OperateResult Write( string address, byte[] value )
		{
			// 分析地址
			OperateResult<MelsecFxLinksAddress> addressAnalysis = MelsecFxLinksAddress.ParseFrom( address );
			if (!addressAnalysis.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( addressAnalysis );

			// 分析地址
			if      (addressAnalysis.Content.TypeCode == "X") xBuffer.SetBool( value.ToBoolArray( ), addressAnalysis.Content.AddressStart );
			else if (addressAnalysis.Content.TypeCode == "Y") yBuffer.SetBool( value.ToBoolArray( ), addressAnalysis.Content.AddressStart );
			else if (addressAnalysis.Content.TypeCode == "M") mBuffer.SetBool( value.ToBoolArray( ), addressAnalysis.Content.AddressStart );
			else if (addressAnalysis.Content.TypeCode == "S") sBuffer.SetBool( value.ToBoolArray( ), addressAnalysis.Content.AddressStart );
			else if (addressAnalysis.Content.TypeCode == "D") dBuffer.SetBytes( value, addressAnalysis.Content.AddressStart * 2 );
			else if (addressAnalysis.Content.TypeCode == "R") rBuffer.SetBytes( value, addressAnalysis.Content.AddressStart * 2 );
			else return new OperateResult<byte[]>( StringResources.Language.NotSupportedDataType );

			return OperateResult.CreateSuccessResult( );
		}

		#endregion

		#region Bool Read Write Operate

		/// <inheritdoc/>
		[HslMqttApi( "ReadBoolArray", "" )]
		public override OperateResult<bool[]> ReadBool( string address, ushort length )
		{
			OperateResult<MelsecFxLinksAddress> addressAnalysis = MelsecFxLinksAddress.ParseFrom( address );
			if (!addressAnalysis.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( addressAnalysis );

			// 分析地址
			if      (addressAnalysis.Content.TypeCode == "X") return OperateResult.CreateSuccessResult( xBuffer.GetBool( addressAnalysis.Content.AddressStart, length ) );
			else if (addressAnalysis.Content.TypeCode == "Y") return OperateResult.CreateSuccessResult( yBuffer.GetBool( addressAnalysis.Content.AddressStart, length ) );
			else if (addressAnalysis.Content.TypeCode == "M") return OperateResult.CreateSuccessResult( mBuffer.GetBool( addressAnalysis.Content.AddressStart, length ) );
			else if (addressAnalysis.Content.TypeCode == "S") return OperateResult.CreateSuccessResult( sBuffer.GetBool( addressAnalysis.Content.AddressStart, length ) );
			else return new OperateResult<bool[]>( StringResources.Language.NotSupportedDataType );
		}

		/// <inheritdoc/>
		[HslMqttApi( "WriteBoolArray", "" )]
		public override OperateResult Write( string address, bool[] value )
		{
			// 分析地址
			OperateResult<MelsecFxLinksAddress> addressAnalysis = MelsecFxLinksAddress.ParseFrom( address );
			if (!addressAnalysis.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( addressAnalysis );

			// 分析地址
			if      (addressAnalysis.Content.TypeCode == "X") xBuffer.SetBool( value, addressAnalysis.Content.AddressStart );
			else if (addressAnalysis.Content.TypeCode == "Y") yBuffer.SetBool( value, addressAnalysis.Content.AddressStart );
			else if (addressAnalysis.Content.TypeCode == "M") mBuffer.SetBool( value, addressAnalysis.Content.AddressStart );
			else if (addressAnalysis.Content.TypeCode == "S") sBuffer.SetBool( value, addressAnalysis.Content.AddressStart );
			else return new OperateResult<byte[]>( StringResources.Language.NotSupportedDataType );

			return OperateResult.CreateSuccessResult( );
		}

		#endregion

		#region NetServer Override

		/// <inheritdoc/>
		protected override INetMessage GetNewNetMessage( ) => null;

		/// <inheritdoc/>
		protected override OperateResult<byte[]> ReadFromCoreServer( AppSession session, byte[] receive )
		{
			OperateResult<byte[]> extra = ExtraMcCore( receive, this.Format );
			if (!extra.IsSuccess)
			{
				if (extra.ErrorCode < 256 && extra.ErrorCode > 0) return OperateResult.CreateSuccessResult( PackCommand( (byte)extra.ErrorCode, null, this.Format ) );
				return extra;
			}
			else
			{
				string cmd = Encoding.ASCII.GetString( extra.Content, 0, 2 );
				if      (cmd == "BR")                return ReadBoolByCommand(  extra.Content );     // 位单位成批读出
				else if (cmd == "WR" || cmd == "QR") return ReadWordByCommand(  extra.Content );     // 字单位成批读出
				else if (cmd == "BW")                return WriteBoolByCommand( extra.Content );     // 位单位成批写入
				else if (cmd == "WW" || cmd == "QW") return WriteWordByCommand( extra.Content );     // 字单位成批写入
				else if (cmd == "RR") return OperateResult.CreateSuccessResult( PackCommand( 0x00, null, this.Format ) ); // 远程启动
				else if (cmd == "RS") return OperateResult.CreateSuccessResult( PackCommand( 0x00, null, this.Format ) ); // 远程停止
				else if (cmd == "PC") return OperateResult.CreateSuccessResult( PackCommand( 0x00, Encoding.ASCII.GetBytes( "F3" ), this.Format ) ); // PLC型号

				return OperateResult.CreateSuccessResult( PackCommand( 0x06, null, this.Format ) );  // 不支持的功能
			}
		}

		#endregion

		private OperateResult<byte[]> ExtraMcCore( byte[] command, int format )
		{
			byte station = Convert.ToByte( Encoding.ASCII.GetString( command, 1, 2 ), 16 );
			if (this.Station != station) return new OperateResult<byte[]>( $"Station Not Match, need: {this.Station}  but: {station}" );
			// 格式信息及和校验检测
			if (format == 1)
			{
				if (command[0] != AsciiControl.ENQ) return new OperateResult<byte[]>( "First Byte Must Start with ENQ(0x05)" );
				if (SumCheck)
				{
					if (!SoftLRC.CalculateAccAndCheck( command, 1, 2 )) return new OperateResult<byte[]>( 0x02, "Sum Check Failed!" );
					return OperateResult.CreateSuccessResult( command.SelectMiddle( 5, command.Length - 7 ) );
				}
				else
				{
					return OperateResult.CreateSuccessResult( command.SelectMiddle( 5, command.Length - 5 ) );
				}
			}
			else if (format == 4)
			{
				if (command[command.Length - 1] == AsciiControl.LF && command[command.Length - 2] == AsciiControl.CR)
					return ExtraMcCore( command.RemoveLast( 2 ), 1 );
				return new OperateResult<byte[]>( "In format 4 case, last two char must be CR(0x0d) and LF(0x0a)" );
			}
			return OperateResult.CreateSuccessResult( command );
		}

		private int GetAddressOctOrTen( byte address )
		{
			return (address == 'X' || address == 'Y') ? 8 : 10;
		}
		private SoftBuffer GetAddressBuffer( byte address )
		{
			if (address == 'X') return xBuffer;
			if (address == 'Y') return yBuffer;
			if (address == 'M') return mBuffer;
			if (address == 'S') return sBuffer;
			if (address == 'D') return dBuffer;
			if (address == 'R') return rBuffer;
			return null;
		}

		private OperateResult<byte[]> ReadBoolByCommand( byte[] command )
		{
			if (command[3] == 'D' || command[3] == 'R') return OperateResult.CreateSuccessResult( PackCommand( 0x06, null, this.Format ) );

			int index  = Convert.ToInt32( Encoding.ASCII.GetString( command, 4, 4 ), GetAddressOctOrTen( command[3] ) );
			int length = Convert.ToInt32( Encoding.ASCII.GetString( command, 8, 2 ), 16 );
			if (length == 0) length = 256;   // 00表示256长度
			SoftBuffer softBuffer = GetAddressBuffer( command[3] );
			if ( softBuffer == null ) return OperateResult.CreateSuccessResult( PackCommand( 0x06, null, this.Format ) );

			return OperateResult.CreateSuccessResult( PackCommand( 0x00, softBuffer.GetBool( index, length ).Select( m => m ? (byte)0x31 : (byte)0x30 ).ToArray( ), this.Format ) );
		}

		private OperateResult<byte[]> WriteBoolByCommand( byte[] command )
		{
			if (command[3] == 'D' || command[3] == 'R') return OperateResult.CreateSuccessResult( PackCommand( 0x06, null, this.Format ) );

			int index             = Convert.ToInt32( Encoding.ASCII.GetString( command, 4, 4 ), GetAddressOctOrTen( command[3] ) );
			int length            = Convert.ToInt32( Encoding.ASCII.GetString( command, 8, 2 ), 16 );
			bool[] value          = command.SelectMiddle( 10, length ).Select( m => m == 0x31 ).ToArray( );
			SoftBuffer softBuffer = GetAddressBuffer( command[3] );
			if (softBuffer == null) return OperateResult.CreateSuccessResult( PackCommand( 0x06, null, this.Format ) );

			softBuffer.SetBool( value, index );
			return OperateResult.CreateSuccessResult( PackCommand( 0x00, null, this.Format ) );
		}

		private OperateResult<byte[]> ReadWordByCommand( byte[] command )
		{
			if (command[3] == 'X' || command[3] == 'Y' || command[3] == 'M' || command[3] == 'S')
			{
				int index  = 0;
				int length = 0;

				if (command[0] == 'Q')
				{
					index  = Convert.ToInt32( Encoding.ASCII.GetString( command, 4,  6 ), GetAddressOctOrTen( command[3] ) );
					length = Convert.ToInt32( Encoding.ASCII.GetString( command, 10, 2 ), 16 );
				}
				else
				{
					index  = Convert.ToInt32( Encoding.ASCII.GetString( command, 4, 4 ), GetAddressOctOrTen( command[3] ) );
					length = Convert.ToInt32( Encoding.ASCII.GetString( command, 8, 2 ), 16 );
				}
				SoftBuffer softBuffer = GetAddressBuffer( command[3] );
				return OperateResult.CreateSuccessResult( PackCommand( 0x00, Encoding.ASCII.GetBytes( softBuffer.GetBool( index, length * 16 ).ToByteArray( ).ToHexString( ) ), this.Format ) );
			}
			else if (command[3] == 'D' || command[3] == 'R')
			{
				int index  = 0;
				int length = 0;
				if (command[0] == 'Q')
				{
					index  = Convert.ToInt32( Encoding.ASCII.GetString( command, 4,  6 ) );
					length = Convert.ToInt32( Encoding.ASCII.GetString( command, 10, 2 ), 16 );
				}
				else
				{
					index  = Convert.ToInt32( Encoding.ASCII.GetString( command, 4, 4 ) );
					length = Convert.ToInt32( Encoding.ASCII.GetString( command, 8, 2 ), 16 );
				}
				SoftBuffer softBuffer = GetAddressBuffer( command[3] );
				return OperateResult.CreateSuccessResult( PackCommand( 0x00, Encoding.ASCII.GetBytes( softBuffer.GetBytes( index * 2, length * 2 ).ToHexString( ) ), this.Format ) );
			}
			else
				return OperateResult.CreateSuccessResult( PackCommand( 0x06, null, this.Format ) );
		}

		private OperateResult<byte[]> WriteWordByCommand( byte[] command )
		{
			if (command[3] == 'X' || command[3] == 'Y' || command[3] == 'M' || command[3] == 'S')
			{
				int index  = 0;
				int length = 0;
				bool[] value;
				if (command[0] == 'Q')
				{
					index    = Convert.ToInt32( Encoding.ASCII.GetString( command, 4,  6 ), GetAddressOctOrTen( command[3] ) );
					length   = Convert.ToInt32( Encoding.ASCII.GetString( command, 10, 2 ), 16 );
					value    = Encoding.ASCII.GetString( command, 12, length * 4 ).ToHexBytes( ).ToBoolArray( );
				}
				else
				{
					index    = Convert.ToInt32( Encoding.ASCII.GetString( command, 4, 4 ), GetAddressOctOrTen( command[3] ) );
					length   = Convert.ToInt32( Encoding.ASCII.GetString( command, 8, 2 ), 16 );
					value    = Encoding.ASCII.GetString( command, 10, length * 4 ).ToHexBytes( ).ToBoolArray( );
				}
				SoftBuffer softBuffer = GetAddressBuffer( command[3] );
				softBuffer.SetBool( value, index );
				return OperateResult.CreateSuccessResult( PackCommand( 0x00, null, this.Format ) );
			}
			else if (command[3] == 'D' || command[3] == 'R')
			{
				int index  = 0;
				int length = 0;
				byte[] value;
				if (command[0] == 'Q')
				{
					index  = Convert.ToInt32( Encoding.ASCII.GetString( command, 4, 6 ) );
					length = Convert.ToInt32( Encoding.ASCII.GetString( command, 10, 2 ), 16 );
					value  = Encoding.ASCII.GetString( command, 12, length * 4 ).ToHexBytes( );
				}
				else
				{
					index  = Convert.ToInt32( Encoding.ASCII.GetString( command, 4, 4 ) );
					length = Convert.ToInt32( Encoding.ASCII.GetString( command, 8, 2 ), 16 );
					value  = Encoding.ASCII.GetString( command, 10, length * 4 ).ToHexBytes( );
				}
				SoftBuffer softBuffer = GetAddressBuffer( command[3] );
				softBuffer.SetBytes( value, index * 2 );
				return OperateResult.CreateSuccessResult( PackCommand( 0x00, null, this.Format ) );
			}
			else
				return OperateResult.CreateSuccessResult( PackCommand( 0x06, null, this.Format ) );
		}

		/// <inheritdoc/>
		protected byte[] PackCommand( byte status, byte[] data, int format )
		{
			if (data == null) data = new byte[0];
			if (data.Length == 0)
			{
				// 写入操作
				if (format == 1)
				{
					if (status == 0)
					{
						byte[] buffer = Encoding.ASCII.GetBytes( "\u0006F9FF" );
						SoftBasic.BuildAsciiBytesFrom( Station ).CopyTo( buffer, 1 );
						return buffer;
					}
					else
					{
						byte[] buffer = Encoding.ASCII.GetBytes( "\u001500FF00" );
						SoftBasic.BuildAsciiBytesFrom( Station ).CopyTo( buffer, 1 );
						SoftBasic.BuildAsciiBytesFrom( status ).CopyTo( buffer, 5 );
						return buffer;
					}
				}
				else if (format == 4)
				{
					byte[] buffer = PackCommand( status, data, 1 );
					return SoftBasic.SpliceArray( buffer, new byte[] { AsciiControl.CR, AsciiControl.LF } );
				}
				return null;
			}
			else
			{
				// 读取操作
				if (format == 1)
				{
					if (status != 0)
					{
						byte[] buffer = Encoding.ASCII.GetBytes( "\u001500FF00" );
						SoftBasic.BuildAsciiBytesFrom( Station ).CopyTo( buffer, 1 );
						SoftBasic.BuildAsciiBytesFrom( status ).CopyTo( buffer, 5 );
						return buffer;
					}
					else
					{
						byte[] buffer = new byte[(SumCheck ? 8 : 6) + data.Length];
						buffer[0] = AsciiControl.STX;
						SoftBasic.BuildAsciiBytesFrom( Station ).CopyTo( buffer, 1 );
						Encoding.ASCII.GetBytes( "FF" ).CopyTo( buffer, 3 );
						data.CopyTo( buffer, 5 );
						buffer[buffer.Length - (SumCheck ? 3 : 1)] = AsciiControl.ETX;
						if (SumCheck) Serial.SoftLRC.CalculateAccAndFill( buffer, 1, 2 );

						return buffer;
					}
				}
				else if (format == 4)
				{
					byte[] buffer = PackCommand( status, data, 1 );
					return SoftBasic.SpliceArray( buffer, new byte[] { AsciiControl.CR, AsciiControl.LF } );
				}
				return null;
			}
		}

		#region Private Member

		private SoftBuffer xBuffer;                    // x寄存器的数据池
		private SoftBuffer yBuffer;                    // y寄存器的数据池
		private SoftBuffer mBuffer;                    // m寄存器的数据池
		private SoftBuffer sBuffer;                    // s寄存器的数据池
		private SoftBuffer dBuffer;                    // d寄存器的数据池
		private SoftBuffer rBuffer;                    // r文件寄存器的数据池

		private const int DataPoolLength = 65536;      // 数据的长度

		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString( ) => $"MelsecFxLinksServer[{Port}]";

		#endregion
	}
}
