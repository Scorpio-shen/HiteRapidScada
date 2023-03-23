using HslCommunication.Core;
using HslCommunication.Core.IMessage;
using HslCommunication.Core.Net;
using HslCommunication.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.IO;
#if !NET35 && !NET20
using System.Threading.Tasks;
#endif

namespace HslCommunication.Profinet.Vigor
{
	/// <summary>
	/// 丰炜通信协议的网口透传版本，支持VS系列，地址支持携带站号，例如 s=2;D100, 字地址支持 D,SD,R,T,C(C200-C255是32位寄存器), 位地址支持X,Y,M,SM,S,TS(定时器触点),TC（定时器线圈）,CS(计数器触点),CC（计数器线圈)<br />
	/// The network port transparent transmission version of Fengwei communication protocol supports VS series, and the address supports carrying station number, 
	/// such as s=2;D100, word address supports D, SD, R, T, C (C200-C255 are 32-bit registers), Bit address supports X, Y, M, SM, S, TS (timer contact), 
	/// TC (timer coil), CS (counter contact), CC (counter coil)
	/// </summary>
	/// <remarks>
	/// 暂时不支持对字寄存器(D,R)进行读写位操作，感谢随时关注库的更新日志
	/// </remarks>
	public class VigorSerialOverTcp : NetworkDeviceBase
	{
		#region Constructor

		/// <summary>
		/// 实例化默认的构造方法<br />
		/// Instantiate the default constructor
		/// </summary>
		public VigorSerialOverTcp( )
		{
			this.WordLength = 1;
			this.ByteTransform = new RegularByteTransform( );
		}

		/// <summary>
		/// 使用指定的ip地址和端口来实例化一个对象<br />
		/// Instantiate an object with the specified IP address and port
		/// </summary>
		/// <param name="ipAddress">设备的Ip地址</param>
		/// <param name="port">设备的端口号</param>
		public VigorSerialOverTcp( string ipAddress, int port ) : this( )
		{
			this.IpAddress = ipAddress;
			this.Port      = port;
		}

		/// <inheritdoc/>
		protected override OperateResult<byte[]> ReceiveByMessage( Socket socket, int timeOut, INetMessage netMessage, Action<long, long> reportProgress = null )
		{
			return ReceiveVigorMessage( socket, timeOut );
		}
#if !NET35 && !NET20
		/// <inheritdoc/>
		protected async override Task<OperateResult<byte[]>> ReceiveByMessageAsync( Socket socket, int timeOut, INetMessage netMessage, Action<long, long> reportProgress = null )
		{
			return await ReceiveVigorMessageAsync( socket, timeOut );
		}

#endif
		#endregion

		#region Public Properties

		/// <inheritdoc cref="VigorSerial.Station"/>
		public byte Station { get; set; }

		#endregion

		#region ReadWrite

		/// <inheritdoc cref="Helper.VigorHelper.Read(IReadWriteDevice, byte, string, ushort)"/>
		[HslMqttApi( "ReadByteArray", "" )]
		public override OperateResult<byte[]> Read( string address, ushort length ) => Helper.VigorHelper.Read( this, this.Station, address, length );

		/// <inheritdoc cref="Helper.VigorHelper.Write(IReadWriteDevice, byte, string, byte[])"/>
		[HslMqttApi( "WriteByteArray", "" )]
		public override OperateResult Write( string address, byte[] value ) => Helper.VigorHelper.Write( this, this.Station, address, value );

		/// <inheritdoc cref="Helper.VigorHelper.ReadBool(IReadWriteDevice, byte, string, ushort)"/>
		[HslMqttApi( "ReadBoolArray", "" )]
		public override OperateResult<bool[]> ReadBool( string address, ushort length ) => Helper.VigorHelper.ReadBool( this, this.Station, address, length );

		/// <inheritdoc cref="Helper.VigorHelper.Write(IReadWriteDevice, byte, string, bool[])"/>
		[HslMqttApi( "WriteBoolArray", "" )]
		public override OperateResult Write( string address, bool[] value ) => Helper.VigorHelper.Write( this, this.Station, address, value );
#if !NET35 && !NET20
		/// <inheritdoc cref="Helper.VigorHelper.Read(IReadWriteDevice, byte, string, ushort)"/>
		public async override Task<OperateResult<byte[]>> ReadAsync( string address, ushort length ) => await Helper.VigorHelper.ReadAsync( this, this.Station, address, length );

		/// <inheritdoc cref="Helper.VigorHelper.Write(IReadWriteDevice, byte, string, byte[])"/>
		public async override Task<OperateResult> WriteAsync( string address, byte[] value ) => await Helper.VigorHelper.WriteAsync( this, this.Station, address, value );

		/// <inheritdoc cref="Helper.VigorHelper.ReadBool(IReadWriteDevice, byte, string, ushort)"/>
		public async override Task<OperateResult<bool[]>> ReadBoolAsync( string address, ushort length ) => await Helper.VigorHelper.ReadBoolAsync( this, this.Station, address, length );

		/// <inheritdoc cref="Helper.VigorHelper.Write(IReadWriteDevice, byte, string, bool[])"/>
		public async override Task<OperateResult> WriteAsync( string address, bool[] value ) => await Helper.VigorHelper.WriteAsync( this, this.Station, address, value );
#endif
		#endregion


		#region Object Override

		/// <inheritdoc/>
		public override string ToString( ) => $"VigorSerialOverTcp[{IpAddress}:{Port}]";

		#endregion
	}
}
