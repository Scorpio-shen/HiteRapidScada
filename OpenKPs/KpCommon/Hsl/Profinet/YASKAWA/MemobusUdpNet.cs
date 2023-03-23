using HslCommunication.BasicFramework;
using HslCommunication.Core.IMessage;
using HslCommunication.Core;
using HslCommunication.Core.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if !NET20 && !NET35
using System.Threading.Tasks;
#endif

namespace HslCommunication.Profinet.YASKAWA
{
	/// <inheritdoc cref="MemobusTcpNet"/>
	public class MemobusUdpNet : NetworkUdpDeviceBase, Helper.IMemobus
	{
		#region Constructor

		/// <summary>
		/// 实例化一个Memobus-Tcp协议的客户端对象<br />
		/// Instantiate a client object of the Memobus-Tcp protocol
		/// </summary>
		public MemobusUdpNet( )
		{
			this.softIncrementCount = new SoftIncrementCount( byte.MaxValue );
			this.WordLength = 1;
			this.ByteTransform = new ReverseWordTransform( );
			this.ByteTransform.DataFormat = DataFormat.DCBA;
		}

		/// <summary>
		/// 指定服务器地址，端口号，客户端自己的站号来初始化<br />
		/// Specify the server address, port number, and client's own station number to initialize
		/// </summary>
		/// <param name="ipAddress">服务器的Ip地址</param>
		/// <param name="port">服务器的端口号</param>
		public MemobusUdpNet( string ipAddress, int port = 502 ) : this( )
		{
			this.IpAddress = ipAddress;
			this.Port = port;
		}

		/// <inheritdoc/>
		public override byte[] PackCommandWithHeader( byte[] command ) => Helper.MemobusHelper.PackCommandWithHeader( command, this.softIncrementCount.GetCurrentValue( ) );

		/// <inheritdoc/>
		public override OperateResult<byte[]> UnpackResponseContent( byte[] send, byte[] response ) => Helper.MemobusHelper.UnpackResponseContent( send, response );

		#endregion

		#region Public Properties

		/// <inheritdoc cref="Helper.IMemobus.CpuTo"/>
		public byte CpuTo
		{
			get => this.cpuTo;
			set => this.cpuTo = value;
		}

		/// <inheritdoc cref="Helper.IMemobus.CpuFrom"/>
		public byte CpuFrom
		{
			get => this.cpuFrom;
			set => this.cpuFrom = value;
		}

		#endregion

		#region Read Write

		/// <inheritdoc cref="Helper.MemobusHelper.ReadBool(Helper.IMemobus, string, ushort)"/>
		public override OperateResult<bool[]> ReadBool( string address, ushort length ) => Helper.MemobusHelper.ReadBool( this, address, length );

		/// <inheritdoc cref="Helper.MemobusHelper.Write(Helper.IMemobus, string, bool)"/>
		public override OperateResult Write( string address, bool value ) => Helper.MemobusHelper.Write( this, address, value );

		/// <inheritdoc cref="Helper.MemobusHelper.Write(Helper.IMemobus, string, bool[])"/>
		public override OperateResult Write( string address, bool[] value ) => Helper.MemobusHelper.Write( this, address, value );

		/// <inheritdoc cref="Helper.MemobusHelper.Read(Helper.IMemobus, string, ushort)"/>
		public override OperateResult<byte[]> Read( string address, ushort length ) => Helper.MemobusHelper.Read( this, address, length );

		/// <inheritdoc cref="Helper.MemobusHelper.Write(Helper.IMemobus, string, byte[])"/>
		public override OperateResult Write( string address, byte[] value ) => Helper.MemobusHelper.Write( this, address, value );

		/// <inheritdoc cref="Helper.MemobusHelper.Write(Helper.IMemobus, string, short, Func{string, short, OperateResult})"/>
		public override OperateResult Write( string address, short value ) => Helper.MemobusHelper.Write( this, address, value, base.Write );

		/// <inheritdoc cref="Helper.MemobusHelper.Write(Helper.IMemobus, string, ushort, Func{string, ushort, OperateResult})"/>
		public override OperateResult Write( string address, ushort value ) => Helper.MemobusHelper.Write( this, address, value, base.Write );

#if !NET20 && !NET35

		/// <inheritdoc cref="ReadBool(string, ushort)"/>
		public async override Task<OperateResult<bool[]>> ReadBoolAsync( string address, ushort length ) => await Helper.MemobusHelper.ReadBoolAsync( this, address, length );

		/// <inheritdoc cref="Write(string, bool)"/>
		public async override Task<OperateResult> WriteAsync( string address, bool value ) => await Helper.MemobusHelper.WriteAsync( this, address, value );

		/// <inheritdoc cref="Write(string, bool[])"/>
		public async override Task<OperateResult> WriteAsync( string address, bool[] value ) => await Helper.MemobusHelper.WriteAsync( this, address, value );

		/// <inheritdoc cref="Read(string, ushort)"/>
		public async override Task<OperateResult<byte[]>> ReadAsync( string address, ushort length ) => await Helper.MemobusHelper.ReadAsync( this, address, length );

		/// <inheritdoc cref="Write(string, byte[])"/>
		public async override Task<OperateResult> WriteAsync( string address, byte[] value ) => await Helper.MemobusHelper.WriteAsync( this, address, value );

		/// <inheritdoc cref="Write(string, short)"/>
		public async override Task<OperateResult> WriteAsync( string address, short value ) => await Helper.MemobusHelper.WriteAsync( this, address, value, base.WriteAsync );

		/// <inheritdoc cref="Write(string, ushort)"/>
		public async override Task<OperateResult> WriteAsync( string address, ushort value ) => await Helper.MemobusHelper.WriteAsync( this, address, value, base.WriteAsync );
#endif
		#endregion

		#region Random Read Write

		/// <inheritdoc cref="Helper.MemobusHelper.ReadRandom(Helper.IMemobus, ushort[])"/>
		public OperateResult<byte[]> ReadRandom( ushort[] address ) => Helper.MemobusHelper.ReadRandom( this, address );

		/// <inheritdoc cref="Helper.MemobusHelper.WriteRandom(Helper.IMemobus, ushort[], byte[])"/>
		public OperateResult WriteRandom( ushort[] address, byte[] value ) => Helper.MemobusHelper.WriteRandom( this, address, value );

#if !NET20 && !NET35
		/// <inheritdoc cref="Helper.MemobusHelper.ReadRandom(Helper.IMemobus, ushort[])"/>
		public async Task<OperateResult<byte[]>> ReadRandomAsync( ushort[] address ) => await Helper.MemobusHelper.ReadRandomAsync( this, address );

		/// <inheritdoc cref="Helper.MemobusHelper.WriteRandom(Helper.IMemobus, ushort[], byte[])"/>
		public async Task<OperateResult> WriteRandomAsync( ushort[] address, byte[] value ) => await Helper.MemobusHelper.WriteRandomAsync( this, address, value );
#endif
		#endregion

		#region Private Member

		private byte cpuTo = 0x02;
		private byte cpuFrom = 0x01;
		private readonly SoftIncrementCount softIncrementCount;              // 自增消息的对象

		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString( ) => $"MemobusUdpNet[{IpAddress}:{Port}]";

		#endregion
	}
}
