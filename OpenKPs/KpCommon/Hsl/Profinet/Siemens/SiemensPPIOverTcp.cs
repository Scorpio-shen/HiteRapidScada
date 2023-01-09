using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HslCommunication.Core;
using HslCommunication.Core.Address;
using HslCommunication.Core.Net;
using HslCommunication.Reflection;
using System.Net.Sockets;
using HslCommunication.Core.IMessage;
using System.IO;
#if !NET35 && !NET20
using System.Threading.Tasks;
#endif

namespace HslCommunication.Profinet.Siemens
{
	/// <inheritdoc cref="SiemensPPI"/>
	public class SiemensPPIOverTcp : NetworkDeviceBase
	{
		#region Constructor

		/// <inheritdoc cref="SiemensPPI()"/>
		public SiemensPPIOverTcp( )
		{
			this.WordLength         = 2;
			this.ByteTransform      = new ReverseBytesTransform( );
			this.communicationLock  = new object( );
		}

		/// <summary>
		/// 使用指定的ip地址和端口号来实例化对象<br />
		/// Instantiate the object with the specified IP address and port number
		/// </summary>
		/// <param name="ipAddress">Ip地址信息</param>
		/// <param name="port">端口号信息</param>
		public SiemensPPIOverTcp( string ipAddress, int port ) : this( )
		{
			this.IpAddress         = ipAddress;
			this.Port              = port;
		}

		/// <inheritdoc/>
		protected override OperateResult<byte[]> ReceiveByMessage( Socket socket, int timeOut, INetMessage netMessage, Action<long, long> reportProgress = null )
		{
			MemoryStream ms = new MemoryStream( );
			DateTime startTime = DateTime.Now;        // 加入超时，防止无限循环接收

			while (true)
			{
				OperateResult<byte[]> receive = base.ReceiveByMessage( socket, timeOut, netMessage, reportProgress );
				if (!receive.IsSuccess) return receive;

				ms.Write( receive.Content );          // 数据写入缓存，然后判断是否完整
				if (Helper.SiemensPPIHelper.CheckReceiveDataComplete( ms )) return OperateResult.CreateSuccessResult( ms.ToArray( ) );

				// 一直不完整的话，引发超时
				if (ReceiveTimeOut > 0 && (DateTime.Now - startTime).TotalMilliseconds > ReceiveTimeOut)
					return new OperateResult<byte[]>( StringResources.Language.ReceiveDataTimeout + ReceiveTimeOut );
			}
		}

#if !NET35 && !NET20

		/// <inheritdoc/>
		protected override async Task<OperateResult<byte[]>> ReceiveByMessageAsync( Socket socket, int timeOut, INetMessage netMessage, Action<long, long> reportProgress = null )
		{
			MemoryStream ms = new MemoryStream( );
			DateTime startTime = DateTime.Now;        // 加入超时，防止无限循环接收

			while (true)
			{
				OperateResult<byte[]> receive = await base.ReceiveByMessageAsync( socket, timeOut, netMessage, reportProgress );
				if (!receive.IsSuccess) return receive;

				ms.Write( receive.Content );          // 数据写入缓存，然后判断是否完整
				if (Helper.SiemensPPIHelper.CheckReceiveDataComplete( ms )) return OperateResult.CreateSuccessResult( ms.ToArray( ) );

				// 一直不完整的话，引发超时
				if (ReceiveTimeOut > 0 && (DateTime.Now - startTime).TotalMilliseconds > ReceiveTimeOut)
					return new OperateResult<byte[]>( StringResources.Language.ReceiveDataTimeout + ReceiveTimeOut );
			}
		}

#endif
		#endregion

		#region Public Properties

		/// <inheritdoc cref="SiemensPPI.Station"/>
		public byte Station { get => station; set => station = value; }

		#endregion

		#region Read Write Override

		/// <inheritdoc cref="SiemensPPI.Read(string, ushort)"/>
		[HslMqttApi( "ReadByteArray", "" )]
		public override OperateResult<byte[]> Read( string address, ushort length ) => Helper.SiemensPPIHelper.Read( this, address, length, this.Station, this.communicationLock );

		/// <inheritdoc cref="Helper.SiemensPPIHelper.ReadBool(IReadWriteDevice, string, byte, object)"/>
		[HslMqttApi( "ReadBool", "" )]
		public override OperateResult<bool> ReadBool( string address ) => Helper.SiemensPPIHelper.ReadBool( this, address, this.Station, this.communicationLock );

		/// <inheritdoc cref="Helper.SiemensPPIHelper.ReadBool(IReadWriteDevice, string, byte, object)"/>
		[HslMqttApi( "ReadBoolArray", "" )]
		public override OperateResult<bool[]> ReadBool( string address, ushort length ) => Helper.SiemensPPIHelper.ReadBool( this, address, length, this.Station, this.communicationLock );

		/// <inheritdoc cref="SiemensPPI.Write(string, byte[])"/>
		[HslMqttApi( "WriteByteArray", "" )]
		public override OperateResult Write( string address, byte[] value ) => Helper.SiemensPPIHelper.Write( this, address, value, this.Station, this.communicationLock );

		/// <inheritdoc cref="SiemensPPI.Write(string, bool[])"/>
		[HslMqttApi( "WriteBoolArray", "" )]
		public override OperateResult Write( string address, bool[] value ) => Helper.SiemensPPIHelper.Write( this, address, value, this.Station, this.communicationLock );

		#endregion

		#region Byte Read Write

		/// <inheritdoc cref="SiemensPPI.ReadByte(string)"/>
		[HslMqttApi( "ReadByte", "" )]
		public OperateResult<byte> ReadByte( string address ) => ByteTransformHelper.GetResultFromArray( Read( address, 1 ) );

		/// <inheritdoc cref="SiemensPPI.Write(string, byte)"/>
		[HslMqttApi( "WriteByte", "" )]
		public OperateResult Write( string address, byte value ) => Write( address, new byte[] { value } );

		#endregion

		#region Async Byte Read Write
#if !NET35 && !NET20
		/// <inheritdoc/>
		public async override Task<OperateResult<bool>> ReadBoolAsync( string address ) => await Task.Run( ( ) => ReadBool( address ) );

		/// <inheritdoc cref="ReadByte(string)"/>
		public async Task<OperateResult<byte>> ReadByteAsync( string address ) => ByteTransformHelper.GetResultFromArray( await ReadAsync( address, 1 ) );

		/// <inheritdoc cref="Write(string, byte)"/>
		public async Task<OperateResult> WriteAsync( string address, byte value ) => await WriteAsync( address, new byte[] { value } );
#endif
		#endregion

		//protected override OperateResult<byte[]> ReceiveByMessage( Socket socket, int timeOut, INetMessage netMessage, Action<long, long> reportProgress = null )
		//{
		//	return base.ReceiveByMessage( socket, timeOut, netMessage, reportProgress );
		//}

		#region Start Stop

		/// <inheritdoc cref="SiemensPPI.Start"/>
		[HslMqttApi]
		public OperateResult Start( string parameter = "" ) => Helper.SiemensPPIHelper.Start( this, parameter, this.Station, this.communicationLock );

		/// <inheritdoc cref="SiemensPPI.Stop"/>
		[HslMqttApi]
		public OperateResult Stop( string parameter = "" ) => Helper.SiemensPPIHelper.Stop( this, parameter, this.Station, this.communicationLock );

		/// <inheritdoc cref="Helper.SiemensPPIHelper.ReadPlcType"/>
		[HslMqttApi]
		public OperateResult<string> ReadPlcType( string parameter = "" ) => Helper.SiemensPPIHelper.ReadPlcType( this, parameter, this.Station, this.communicationLock );

		#endregion

		#region Async Start Stop
#if !NET35 && !NET20
		/// <inheritdoc cref="SiemensPPI.Start"/>
		public async Task<OperateResult> StartAsync( string parameter = "" ) => await Task.Run( ( ) => Start( parameter ) );

		/// <inheritdoc cref="SiemensPPI.Stop"/>
		public async Task<OperateResult> StopAsync( string parameter = "" ) => await Task.Run( ( ) => Stop( parameter ) );

		/// <inheritdoc cref="Helper.SiemensPPIHelper.ReadPlcType"/>
		public async Task<OperateResult<string>> ReadPlcTypeAsync( string parameter = "" ) => await Task.Run( ( ) => Helper.SiemensPPIHelper.ReadPlcType( this, parameter, this.Station, this.communicationLock ) );
#endif
		#endregion

		#region Private Member

		private byte station = 0x02;            // PLC的站号信息
		private object communicationLock;       // 通讯锁

		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString( ) => $"SiemensPPIOverTcp[{IpAddress}:{Port}]";

		#endregion

	}
}
