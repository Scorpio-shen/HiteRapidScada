using HslCommunication.BasicFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HslCommunication;
using HslCommunication.Reflection;
using HslCommunication.Core.Net;
using System.Net.Sockets;
using System.IO.Ports;
using HslCommunication.Core.IMessage;
using HslCommunication.Core;
#if !NET20 && !NET35
using System.Threading.Tasks;
#endif

namespace HslCommunication.Profinet.Panasonic
{
	/// <summary>
	/// <b>[商业授权]</b> 松下Mewtocol协议的虚拟服务器，支持串口和网口的操作<br />
	/// <b>[Authorization]</b> Panasonic Mewtocol protocol virtual server, supports serial and network port operations
	/// </summary>
	/// <remarks>
	/// 地址的地址分为线圈型和整型，线圈支持X,Y,R,L, 字单位的整型支持 X,Y,R,L,D,LD,F<br />
	/// The address of the address is divided into coil type and integer type, the coil supports X, Y, R, L, and the integer type of word unit supports X, Y, R, L, D, LD, F
	/// </remarks>
	public class PanasonicMewtocolServer : NetworkDataServerBase
	{
		#region Constructor

		/// <summary>
		/// 实例化一个默认的对象
		/// </summary>
		public PanasonicMewtocolServer( )
		{
			rBuffer  = new SoftBuffer( DataPoolLength * 2 );
			dtBuffer = new SoftBuffer( DataPoolLength * 2 );
			ldBuffer = new SoftBuffer( DataPoolLength * 2 );
			flBuffer = new SoftBuffer( DataPoolLength * 2 );
			xBuffer  = new SoftBuffer( DataPoolLength * 2 );
			lBuffer  = new SoftBuffer( DataPoolLength * 2 );
			yBuffer  = new SoftBuffer( DataPoolLength * 2 );

			ByteTransform            = new RegularByteTransform( );
			ByteTransform.DataFormat = DataFormat.DCBA;
			LogMsgFormatBinary       = false;
		}

		#endregion

		#region Public Members

		/// <inheritdoc cref="PanasonicMewtocol.Station"/>
		public byte Station
		{
			get { return station; }
			set { station = value; }
		}

		#endregion

		#region Data Persistence

		/// <inheritdoc/>
		protected override byte[] SaveToBytes( )
		{
			byte[] buffer = new byte[DataPoolLength * 2 * 7];
			Array.Copy( rBuffer  .GetBytes( ),  0, buffer, DataPoolLength * 0,  DataPoolLength * 2 );
			Array.Copy( dtBuffer .GetBytes( ), 0, buffer, DataPoolLength * 2,  DataPoolLength * 2 );
			Array.Copy( ldBuffer .GetBytes( ), 0, buffer, DataPoolLength * 4,  DataPoolLength * 2 );
			Array.Copy( flBuffer .GetBytes( ), 0, buffer, DataPoolLength * 6,  DataPoolLength * 2 );
			Array.Copy( xBuffer  .GetBytes( ), 0, buffer, DataPoolLength * 8,  DataPoolLength * 2 );
			Array.Copy( lBuffer  .GetBytes( ), 0, buffer, DataPoolLength * 10, DataPoolLength * 2 );
			Array.Copy( yBuffer  .GetBytes( ), 0, buffer, DataPoolLength * 12, DataPoolLength * 2 );
			return buffer;
		}

		/// <inheritdoc/>
		protected override void LoadFromBytes( byte[] content )
		{
			if (content.Length < DataPoolLength * 2 * 7) throw new Exception( "File is not correct" );
			rBuffer  .SetBytes( content, DataPoolLength * 0,  0, DataPoolLength * 2 );
			dtBuffer .SetBytes( content, DataPoolLength * 2,  0, DataPoolLength * 2 );
			ldBuffer .SetBytes( content, DataPoolLength * 4,  0, DataPoolLength * 2 );
			flBuffer .SetBytes( content, DataPoolLength * 6,  0, DataPoolLength * 2 );
			xBuffer  .SetBytes( content, DataPoolLength * 8,  0, DataPoolLength * 2 );
			lBuffer  .SetBytes( content, DataPoolLength * 10, 0, DataPoolLength * 2 );
			yBuffer  .SetBytes( content, DataPoolLength * 12, 0, DataPoolLength * 2 );
		}

		#endregion

		#region NetworkDataServerBase Override

		/// <inheritdoc cref="PanasonicMewtocol.Read(string, ushort)"/>
		/// <remarks>
		/// 在服务器端的功能实现里，暂时不支持C,T数据的访问。<br />
		/// In the server-side function implementation, access to C and T data is temporarily not supported.
		/// </remarks>
		[HslMqttApi( "ReadByteArray", "" )]
		public override OperateResult<byte[]> Read( string address, ushort length )
		{
			// 解析地址
			OperateResult<string, int> analysis = PanasonicHelper.AnalysisAddress( address );
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( analysis );

			if      (analysis.Content1 == "D")  return OperateResult.CreateSuccessResult( dtBuffer.GetBytes( analysis.Content2 * 2, length * 2 ) );
			else if (analysis.Content1 == "LD") return OperateResult.CreateSuccessResult( ldBuffer.GetBytes( analysis.Content2 * 2, length * 2 ) );
			else if (analysis.Content1 == "F")  return OperateResult.CreateSuccessResult( flBuffer.GetBytes( analysis.Content2 * 2, length * 2 ) );
			else if (analysis.Content1 == "X")  return OperateResult.CreateSuccessResult( xBuffer.GetBool(   analysis.Content2, length * 16 ).ToByteArray( ) );
			else if (analysis.Content1 == "Y")  return OperateResult.CreateSuccessResult( yBuffer.GetBool(   analysis.Content2, length * 16 ).ToByteArray( ) );
			else if (analysis.Content1 == "R")  return OperateResult.CreateSuccessResult( rBuffer.GetBool(   analysis.Content2, length * 16 ).ToByteArray( ) );
			else if (analysis.Content1 == "L")  return OperateResult.CreateSuccessResult( lBuffer.GetBool(   analysis.Content2, length * 16 ).ToByteArray( ) );
			else return new OperateResult<byte[]>( StringResources.Language.NotSupportedDataType );
		}

		/// <inheritdoc cref="PanasonicMewtocol.Write(string, byte[])"/>
		/// <remarks>
		/// 在服务器端的功能实现里，暂时不支持C,T数据的访问。<br />
		/// In the server-side function implementation, access to C and T data is temporarily not supported.
		/// </remarks>
		[HslMqttApi( "WriteByteArray", "" )]
		public override OperateResult Write( string address, byte[] value )
		{
			// 解析地址
			OperateResult<string, int> analysis = PanasonicHelper.AnalysisAddress( address );
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( analysis );

			if      (analysis.Content1 == "D")  dtBuffer.SetBytes( value, analysis.Content2 * 2 );
			else if (analysis.Content1 == "LD") ldBuffer.SetBytes( value, analysis.Content2 * 2 );
			else if (analysis.Content1 == "F")  flBuffer.SetBytes( value, analysis.Content2 * 2 );
			else if (analysis.Content1 == "X")  xBuffer. SetBool( value.ToBoolArray( ), analysis.Content2 );
			else if (analysis.Content1 == "Y")  yBuffer. SetBool( value.ToBoolArray( ), analysis.Content2 );
			else if (analysis.Content1 == "R")  rBuffer. SetBool( value.ToBoolArray( ), analysis.Content2 );
			else if (analysis.Content1 == "L")  lBuffer. SetBool( value.ToBoolArray( ), analysis.Content2 );
			else return new OperateResult<byte[]>( StringResources.Language.NotSupportedDataType );

			return OperateResult.CreateSuccessResult( );
		}

		/// <inheritdoc cref="PanasonicMewtocol.ReadBool(string, ushort)"/>
		/// <remarks>
		/// 在服务器端的功能实现里，长度支持任意的长度信息。<br />
		/// In the server-side function implementation, the length supports arbitrary length information.
		/// </remarks>
		[HslMqttApi( "ReadBoolArray", "" )]
		public override OperateResult<bool[]> ReadBool( string address, ushort length )
		{
			// 解析地址
			OperateResult<string, int> analysis = PanasonicHelper.AnalysisAddress( address );
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( analysis );

			if      (analysis.Content1 == "X")  return OperateResult.CreateSuccessResult( xBuffer.GetBool( analysis.Content2, length ) );
			else if (analysis.Content1 == "Y")  return OperateResult.CreateSuccessResult( yBuffer.GetBool( analysis.Content2, length ) );
			else if (analysis.Content1 == "R")  return OperateResult.CreateSuccessResult( rBuffer.GetBool( analysis.Content2, length ) );
			else if (analysis.Content1 == "L")  return OperateResult.CreateSuccessResult( lBuffer.GetBool( analysis.Content2, length ) );
			else return new OperateResult<bool[]>( StringResources.Language.NotSupportedDataType );
		}

		/// <inheritdoc cref="PanasonicMewtocol.Write(string, bool[])"/>
		[HslMqttApi( "WriteBoolArray", "" )]
		public override OperateResult Write( string address, bool[] value )
		{
			// 解析地址
			OperateResult<string, int> analysis = PanasonicHelper.AnalysisAddress( address );
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( analysis );
			
			if      (analysis.Content1 == "X")  xBuffer.SetBool( value, analysis.Content2 );
			else if (analysis.Content1 == "Y")  yBuffer.SetBool( value, analysis.Content2 );
			else if (analysis.Content1 == "R")  rBuffer.SetBool( value, analysis.Content2 );
			else if (analysis.Content1 == "L")  lBuffer.SetBool( value, analysis.Content2 );
			else return new OperateResult<bool[]>( StringResources.Language.NotSupportedDataType );

			return OperateResult.CreateSuccessResult( );
		}

		#endregion

		#region NetServer Override

		/// <inheritdoc/>
		protected override INetMessage GetNewNetMessage( ) => new SpecifiedCharacterMessage( AsciiControl.CR );

		/// <inheritdoc/>
		protected override OperateResult<byte[]> ReadFromCoreServer( AppSession session, byte[] receive )
		{
			if (receive.Length < 5) return new OperateResult<byte[]>( $"Uknown Data：{receive.ToHexString( ' ' )}" );
			
			return PanasonicHelper.PackPanasonicCommand( station, ReadFromCommand( receive ), receive[0] == '<' );
		}

		#endregion

		#region Function Process Center

		/// <summary>
		/// 创建一个失败的返回消息，指定错误码即可，会自动计算出来BCC校验和
		/// </summary>
		/// <param name="code">错误码</param>
		/// <returns>原始字节报文，用于反馈消息</returns>
		protected string CreateFailedResponse( byte code ) => "!" + code.ToString( "D2" );

		/// <summary>
		/// 根据命令来获取相关的数据内容
		/// </summary>
		/// <param name="cmd">原始的命令码</param>
		/// <returns>返回的数据信息</returns>
		public virtual string ReadFromCommand( byte[] cmd )
		{
			try
			{
				string strCommand = Encoding.ASCII.GetString( cmd );
				if (strCommand[0] != '%' && strCommand[0] != '<') return CreateFailedResponse( 41 );
				byte stat = Convert.ToByte( strCommand.Substring( 1, 2 ), 16 );
				bool useExpandedHeader = strCommand[0] == '<';
				if (stat != station)
				{
					LogNet?.WriteError( ToString( ), $"Station not match, need:{station}, but now: {stat}" );
					return CreateFailedResponse( 50 );
				}
				if (strCommand[3] != '#') return CreateFailedResponse( 41 );
				if (strCommand.Substring( 4, 3 ) == "RCS")
				{
					// 读取单个的bool的值
					int bitIndex = Convert.ToInt32( strCommand.Substring( 8, 3 ) ) * 16 + Convert.ToInt32( strCommand.Substring( 11, 1 ), 16 );
					if      (strCommand[7] == 'R') return "$RC" + (rBuffer.GetBool( bitIndex ) ? "1" : "0");
					else if (strCommand[7] == 'X') return "$RC" + (xBuffer.GetBool( bitIndex ) ? "1" : "0");
					else if (strCommand[7] == 'Y') return "$RC" + (yBuffer.GetBool( bitIndex ) ? "1" : "0");
					else if (strCommand[7] == 'L') return "$RC" + (lBuffer.GetBool( bitIndex ) ? "1" : "0");
					else return CreateFailedResponse( 42 );
				}
				else if (strCommand.Substring( 4, 3 ) == "WCS")
				{
					// 写入单个的bool的值
					int bitIndex = Convert.ToInt32( strCommand.Substring( 8, 3 ) ) * 16 + Convert.ToInt32( strCommand.Substring( 11, 1 ), 16 );
					if      (strCommand[7] == 'R') { rBuffer.SetBool( strCommand[12] == '1', bitIndex ); return "$WC"; }
					else if (strCommand[7] == 'X') { xBuffer.SetBool( strCommand[12] == '1', bitIndex ); return "$WC"; }
					else if (strCommand[7] == 'Y') { yBuffer.SetBool( strCommand[12] == '1', bitIndex ); return "$WC"; }
					else if (strCommand[7] == 'L') { lBuffer.SetBool( strCommand[12] == '1', bitIndex ); return "$WC"; }
					else return CreateFailedResponse( 42 );
				}
				else if (strCommand.Substring( 4, 3 ) == "RCP")
				{
					// 读取多触点的状态
					int number = strCommand[7] - '0';
					if (number > 8) return CreateFailedResponse( 42 );
					StringBuilder stringBuilder = new StringBuilder( );
					for (int i = 0; i < number; i++)
					{
						int bitIndex = Convert.ToInt32( strCommand.Substring( 9 + 5 * i, 3 ) ) * 16 + Convert.ToInt32( strCommand.Substring( 12 + 5 * i, 1 ), 16 );
						if      (strCommand[8 + 5 * i] == 'R') stringBuilder.Append( rBuffer.GetBool( bitIndex ) ? "1" : "0");
						else if (strCommand[8 + 5 * i] == 'X') stringBuilder.Append( xBuffer.GetBool( bitIndex ) ? "1" : "0");
						else if (strCommand[8 + 5 * i] == 'Y') stringBuilder.Append( yBuffer.GetBool( bitIndex ) ? "1" : "0");
						else if (strCommand[8 + 5 * i] == 'L') stringBuilder.Append( lBuffer.GetBool( bitIndex ) ? "1" : "0");
					}
					return "$RC" + stringBuilder.ToString( );
				}
				else if (strCommand.Substring( 4, 3 ) == "WCP")
				{
					// 写入多触点的状态
					int number = strCommand[7] - '0';
					if (number > 8) return CreateFailedResponse( 42 );
					for (int i = 0; i < number; i++)
					{
						int bitIndex = Convert.ToInt32( strCommand.Substring( 9 + 6 * i, 3 ) ) * 16 + Convert.ToInt32( strCommand.Substring( 12 + 6 * i, 1 ), 16 );
						if      (strCommand[8 + 6 * i] == 'R') rBuffer.SetBool( strCommand[13 + 6 * i] == '1', bitIndex );
						else if (strCommand[8 + 6 * i] == 'X') xBuffer.SetBool( strCommand[13 + 6 * i] == '1', bitIndex );
						else if (strCommand[8 + 6 * i] == 'Y') yBuffer.SetBool( strCommand[13 + 6 * i] == '1', bitIndex );
						else if (strCommand[8 + 6 * i] == 'L') lBuffer.SetBool( strCommand[13 + 6 * i] == '1', bitIndex );
					}
					return "$WC";
				}
				if (strCommand.Substring( 4, 3 ) == "RCC")
				{
					// 字单位读取线圈
					int addressStart = Convert.ToInt32( strCommand.Substring( 8, 4 ) );
					int addressEnd   = Convert.ToInt32( strCommand.Substring( 12, 4 ) );
					int length       = addressEnd - addressStart + 1;
					if (length > (useExpandedHeader ? 509 : 27)) return CreateFailedResponse( 42 );

					if      (strCommand[7] == 'R') return "$RC" + rBuffer.GetBytes( addressStart * 2, length * 2 ).ToHexString( );
					else if (strCommand[7] == 'X') return "$RC" + xBuffer.GetBytes( addressStart * 2, length * 2 ).ToHexString( );
					else if (strCommand[7] == 'Y') return "$RC" + yBuffer.GetBytes( addressStart * 2, length * 2 ).ToHexString( );
					else if (strCommand[7] == 'L') return "$RC" + lBuffer.GetBytes( addressStart * 2, length * 2 ).ToHexString( );
					else return CreateFailedResponse( 42 );
				}
				else if (strCommand.Substring( 4, 3 ) == "WCC")
				{
					// 字单位写入线圈的值
					int addressStart = Convert.ToInt32( strCommand.Substring( 8, 4 ) );
					int addressEnd   = Convert.ToInt32( strCommand.Substring( 12, 4 ) );
					int length       = addressEnd - addressStart + 1;
					byte[] buffer    = strCommand.Substring( 16, length * 4 ).ToHexBytes( );
					if (buffer.Length > (useExpandedHeader ? 2048 - 20 : 118 - 20)) return CreateFailedResponse( 42 );

					if      (strCommand[7] == 'R') { rBuffer.SetBytes( buffer, addressStart * 2 ); return "$WC"; }
					else if (strCommand[7] == 'X') { xBuffer.SetBytes( buffer, addressStart * 2 ); return "$WC"; }
					else if (strCommand[7] == 'Y') { yBuffer.SetBytes( buffer, addressStart * 2 ); return "$WC"; }
					else if (strCommand[7] == 'L') { lBuffer.SetBytes( buffer, addressStart * 2 ); return "$WC"; }
					else return CreateFailedResponse( 42 );
				}

				if (strCommand.Substring( 4, 2 ) == "RD")
				{
					// 读取数据寄存器，支持D（DT）,L（LD）,F（FL）
					int addressStart = Convert.ToInt32( strCommand.Substring( 7,  5 ) );
					int addressEnd   = Convert.ToInt32( strCommand.Substring( 12, 5 ) );
					int length       = addressEnd - addressStart + 1;
					if (length > (useExpandedHeader ? 509 : 27)) return CreateFailedResponse( 42 );

					if      (strCommand[6] == 'D') return "$RD" + dtBuffer.GetBytes( addressStart * 2, length * 2 ).ToHexString( );
					else if (strCommand[6] == 'L') return "$RD" + ldBuffer.GetBytes( addressStart * 2, length * 2 ).ToHexString( );
					else if (strCommand[6] == 'F') return "$RD" + flBuffer.GetBytes( addressStart * 2, length * 2 ).ToHexString( );
					else return CreateFailedResponse( 42 );
				}
				else if (strCommand.Substring( 4, 2 ) == "WD")
				{
					// 写入数据寄存器值
					int addressStart = Convert.ToInt32( strCommand.Substring( 7,  5 ) );
					int addressEnd   = Convert.ToInt32( strCommand.Substring( 12, 5 ) );
					int length       = addressEnd - addressStart + 1;
					byte[] buffer    = strCommand.Substring( 17, length * 4 ).ToHexBytes( );
					if (buffer.Length > (useExpandedHeader ? 2048 - 20 : 118 - 20)) return CreateFailedResponse( 42 );

					if      (strCommand[6] == 'D') { dtBuffer.SetBytes( buffer, addressStart * 2 ); return "$WD"; }
					else if (strCommand[6] == 'L') { ldBuffer.SetBytes( buffer, addressStart * 2 ); return "$WD"; }
					else if (strCommand[6] == 'F') { flBuffer.SetBytes( buffer, addressStart * 2 ); return "$WD"; }
					else return CreateFailedResponse( 42 );
				}
				return CreateFailedResponse( 41 );
			}
			catch( Exception ex )
			{
				LogNet?.WriteException( ToString( ), ex );
				return CreateFailedResponse( 41 );
			}
		}

		#endregion

		#region Serial Support

		/// <inheritdoc/>
		protected override bool CheckSerialReceiveDataComplete( byte[] buffer, int receivedLength )
		{
			if (receivedLength > 5) return buffer[receivedLength - 1] == 0x0D;
			return base.CheckSerialReceiveDataComplete( buffer, receivedLength );
		}

		#endregion

		#region IDisposable Support

		/// <inheritdoc/>
		protected override void Dispose( bool disposing )
		{
			if (disposing)
			{
				xBuffer?. Dispose( );
				rBuffer?. Dispose( );
				dtBuffer?.Dispose( );
				ldBuffer?.Dispose( );
				flBuffer?.Dispose( );
				yBuffer?. Dispose( );
				lBuffer?. Dispose( );
			}
			base.Dispose( disposing );
		}

		#endregion

		#region Private Member

		private SoftBuffer xBuffer;                // 输入线圈的数据池
		private SoftBuffer rBuffer;                // 线圈的数据池
		private SoftBuffer dtBuffer;               // DT数据寄存器
		private SoftBuffer ldBuffer;               // LD链接寄存器
		private SoftBuffer flBuffer;               // FL文件寄存器
		private SoftBuffer yBuffer;                // F寄存器数据池
		private SoftBuffer lBuffer;                // L寄存器数据池

		private const int DataPoolLength = 65536;     // 数据的长度
		private byte station = 1;                     // 服务器的站号数据

		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString( ) => $"PanasonicMewtocolServer[{Port}]";

		#endregion

	}
}
