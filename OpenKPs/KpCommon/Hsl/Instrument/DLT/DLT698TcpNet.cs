using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
#if !NET35 && !NET20
using System.Threading.Tasks;
#endif

namespace HslCommunication.Instrument.DLT
{
	/// <summary>
	/// 698.45协议的TCP通信类(不是串口透传通信)，面向对象的用电信息数据交换协议，使用明文的通信方式。支持读取功率，总功，电压，电流，频率，功率因数等数据。<br />
	/// The TCP communication class of the 698.45 protocol (not the serial port transparent transmission communication), the object-oriented power consumption information data exchange protocol, 
	/// uses the clear text communication method. Support reading power, total power, voltage, current, frequency, power factor and other data.
	/// </summary>
	/// <remarks>
	/// <inheritdoc cref="DLT698" path="remarks"/>
	/// </remarks>
	/// <example>
	/// <inheritdoc cref="DLT698" path="example"/>
	/// </example>
	public class DLT698TcpNet : DLT698OverTcp
	{
		#region Constructor

		/// <summary>
		/// 指定地址域，密码，操作者代码来实例化一个对象，密码及操作者代码在写入操作的时候进行验证<br />
		/// Specify the address field, password, and operator code to instantiate an object, and the password and operator code are validated during write operations, 
		/// which address field is a 12-character BCD code, for example: 149100007290
		/// </summary>
		/// <param name="station">设备的地址信息，通常是一个12字符的BCD码</param>
		public DLT698TcpNet( string station = "AAAAAAAAAAAA" ) : base( station )
		{
		}

		/// <inheritdoc cref="DLT698OverTcp.DLT698OverTcp(string, int, string)"/>
		public DLT698TcpNet( string ipAddress, int port, string station = "AAAAAAAAAAAA" ) : base( ipAddress, port, station )
		{

		}

		#endregion

		/// <inheritdoc/>
		protected override OperateResult InitializationOnConnect( Socket socket )
		{
			OperateResult<byte[]> read1 = ReadFromCoreServer( socket, DLT698.BuildEntireCommand( 0x81, this.Station, 0x00, DLT698TcpNet.CreateLoginApdu( ) ).Content );
			if (!read1.IsSuccess) return read1;

			OperateResult<byte[]> read2 = ReadFromCoreServer( socket, DLT698.BuildEntireCommand( 0x81, this.Station, 0x00, DLT698TcpNet.CreateConnectApdu( ) ).Content );
			if (!read2.IsSuccess) return read2;

			return base.InitializationOnConnect( socket );
		}
#if !NET35 && !NET20
		/// <inheritdoc/>
		protected override async Task<OperateResult> InitializationOnConnectAsync( Socket socket )
		{
			OperateResult<byte[]> read1 = await ReadFromCoreServerAsync( socket, DLT698.BuildEntireCommand( 0x81, this.Station, 0x00, DLT698TcpNet.CreateLoginApdu( ) ).Content );
			if (!read1.IsSuccess) return read1;

			OperateResult<byte[]> read2 = await ReadFromCoreServerAsync( socket, DLT698.BuildEntireCommand( 0x81, this.Station, 0x00, DLT698TcpNet.CreateConnectApdu( ) ).Content );
			if (!read2.IsSuccess) return read2;

			return await base.InitializationOnConnectAsync( socket );
		}
#endif

		/// <inheritdoc/>
		public override string ToString( ) => $"DLT698TcpNet[{IpAddress}:{Port}]";


		#region Static Helper

		private static byte GetDayOfWeek( DayOfWeek dayOfWeek )
		{
			switch (dayOfWeek)
			{
				case DayOfWeek.Monday: return 0x01;
				case DayOfWeek.Tuesday: return 0x02;
				case DayOfWeek.Wednesday: return 0x03;
				case DayOfWeek.Thursday: return 0x04;
				case DayOfWeek.Friday: return 0x05;
				case DayOfWeek.Saturday: return 0x06;
				default: return 0x07;
			}
		}

		internal static byte[] CreateLoginApdu( byte services = 0x01, byte piid = 0x00, byte type = 0x00 )
		{
			byte[] apdu = new byte[15];
			apdu[0] = services;               // LINK-Request
			apdu[1] = piid;                   // PIID-ACD
			apdu[2] = type;                   // 请求类型，建立连接 0
			apdu[3] = 0x00;                   // 心跳周期 180秒
			apdu[4] = 0x84;                   // 
			DateTime dateTime = DateTime.Now;
			apdu[5] = BitConverter.GetBytes( dateTime.Year )[1];   // 年
			apdu[6] = BitConverter.GetBytes( dateTime.Year )[0];
			apdu[7] = BitConverter.GetBytes( dateTime.Month )[0];  // 月
			apdu[8] = BitConverter.GetBytes( dateTime.Day )[0];    // 日
			apdu[9] = GetDayOfWeek( dateTime.DayOfWeek );          // 星期
			apdu[10] = (byte)dateTime.Hour;
			apdu[11] = (byte)dateTime.Minute;
			apdu[12] = (byte)dateTime.Second;
			apdu[13] = BitConverter.GetBytes( dateTime.Millisecond )[1];  // 毫秒
			apdu[14] = BitConverter.GetBytes( dateTime.Millisecond )[0];  // 毫秒
			return apdu;
		}

		internal static byte[] CreateConnectApdu( )
		{
			return "02 00 00 10 FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF 04 00 04 00 01 04 00 00 00 00 64 00 00".ToHexBytes( );
		}

		#endregion
	}
}
