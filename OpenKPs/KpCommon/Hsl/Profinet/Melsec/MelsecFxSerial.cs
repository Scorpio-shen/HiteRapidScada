using HslCommunication.Core;
using HslCommunication.Serial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HslCommunication.BasicFramework;
using HslCommunication.Reflection;
using System.IO.Ports;
#if !NET35 && !NET20
using System.Threading.Tasks;
#endif

namespace HslCommunication.Profinet.Melsec
{
	/// <summary>
	/// 三菱的串口通信的对象，适用于读取FX系列的串口数据，支持的类型参考文档说明<br />
	/// Mitsubishi's serial communication object is suitable for reading serial data of the FX series. Refer to the documentation for the supported types.
	/// </summary>
	/// <remarks>
	/// 一般老旧的型号，例如FX2N之类的，需要将<see cref="IsNewVersion"/>设置为<c>False</c>，如果是FX3U新的型号，则需要将<see cref="IsNewVersion"/>设置为<c>True</c>
	/// </remarks>
	/// <example>
	/// <inheritdoc cref="MelsecFxSerialOverTcp" path="remarks"/>
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\MelsecFxSerial.cs" region="Usage" title="简单的使用" />
	/// </example>
	public class MelsecFxSerial : SerialDeviceBase
	{
		#region Constructor

		/// <summary>
		/// 实例化一个默认的对象
		/// </summary>
		public MelsecFxSerial( )
		{
			this.ByteTransform                         = new RegularByteTransform( );
			this.WordLength                            = 1;
			this.IsNewVersion                          = true;
			this.ByteTransform.IsStringReverseByteWord = true;
		}

		/// <inheritdoc/>
		public override OperateResult Open( )
		{
			int baudRate = this.pipeSerial.GetPipe( ).BaudRate;
			if (AutoChangeBaudRate && baudRate != 9600)
			{
				//sp.Close( );
				this.pipeSerial.GetPipe( ).BaudRate = 9600;
				OperateResult open = this.pipeSerial.Open( );
				if (!open.IsSuccess) return open;

				for (int i = 0; i < 3; i++)
				{
					OperateResult<byte[]> read1 = ReadFromCoreServer( this.pipeSerial.GetPipe( ), new byte[] { 0x05 } );
					if (!read1.IsSuccess) return read1;

					if (read1.Content.Length == 1 && read1.Content[0] == 0x06) break;
					if (i == 2) return new OperateResult( "check 0x06 back before send data failed!" );
				}

				// 发送更改波特率的办法
				byte[] changeBaudRate = 
					baudRate == 19200  ? new byte[] { 0x02, 0x41, 0x31, 0x03, 0x37, 0x35 } :
					baudRate == 38400  ? new byte[] { 0x02, 0x41, 0x32, 0x03, 0x37, 0x36 } :
					baudRate == 57600  ? new byte[] { 0x02, 0x41, 0x33, 0x03, 0x37, 0x37 } :
					baudRate == 115200 ? new byte[] { 0x02, 0x41, 0x35, 0x03, 0x37, 0x39 } : new byte[] { 0x02, 0x41, 0x35, 0x03, 0x37, 0x39 };

				OperateResult<byte[]> read2 = ReadFromCoreServer( this.pipeSerial.GetPipe( ), changeBaudRate );
				if (!read2.IsSuccess) return read2;
				if (read2.Content[0] != 0x06) return new OperateResult( "check 0x06 back after send data failed!" );

				this.pipeSerial.Close( null );
				this.pipeSerial.GetPipe( ).BaudRate = baudRate;
			}

			return base.Open( );
		}

		/// <inheritdoc/>
		protected override OperateResult InitializationOnOpen( SerialPort sp )
		{
			if (AutoChangeBaudRate)
				return ReadFromCoreServer( sp, new byte[] { 0x05 } );
			else
				return base.InitializationOnOpen( sp );
		}

		#endregion

		/// <inheritdoc cref="MelsecFxSerialOverTcp.IsNewVersion"/>
		public bool IsNewVersion { get; set; }

		/// <summary>
		/// 获取或设置是否动态修改PLC的波特率，如果为 <c>True</c>，那么如果本对象设置了波特率 115200，就会自动修改PLC的波特率到 115200，因为三菱PLC再重启后都会使用默认的波特率9600 <br/>
		/// Get or set whether to dynamically modify the baud rate of the PLC. If it is <c>True</c>, then if the baud rate of this object is set to 115200, 
		/// the baud rate of the PLC will be automatically modified to 115200, because the Mitsubishi PLC is not After restart, the default baud rate of 9600 will be used
		/// </summary>
		public bool AutoChangeBaudRate { get; set; } = false;

		#region Read Write Byte

		/// <inheritdoc cref="MelsecFxSerialOverTcp.Read(string, ushort)"/>
		[HslMqttApi( "ReadByteArray", "" )]
		public override OperateResult<byte[]> Read( string address, ushort length ) => 
			Helper.MelsecFxSerialHelper.Read( this, address, length, IsNewVersion );

		/// <inheritdoc cref="MelsecFxSerialOverTcp.Write(string, byte[])"/>
		[HslMqttApi( "WriteByteArray", "" )]
		public override OperateResult Write( string address, byte[] value ) => 
			Helper.MelsecFxSerialHelper.Write( this, address, value, IsNewVersion );

		#endregion

		#region Read Write Bool

		/// <inheritdoc cref="MelsecFxSerialOverTcp.ReadBool(string, ushort)"/>
		[HslMqttApi( "ReadBoolArray", "" )]
		public override OperateResult<bool[]> ReadBool( string address, ushort length ) => 
			Helper.MelsecFxSerialHelper.ReadBool( this, address, length, IsNewVersion );

		/// <inheritdoc cref="MelsecFxSerialOverTcp.Write(string, bool)"/>
		[HslMqttApi( "WriteBool", "" )]
		public override OperateResult Write( string address, bool value ) => 
			Helper.MelsecFxSerialHelper.Write( this, address, value );

		#endregion

		#region Active

		/// <inheritdoc cref="Helper.MelsecFxSerialHelper.ActivePlc(IReadWriteDevice)"/>
		[HslMqttApi( )]
		public OperateResult ActivePlc( ) => Helper.MelsecFxSerialHelper.ActivePlc( this );
#if !NET20 && !NET35
		/// <inheritdoc cref="Helper.MelsecFxSerialHelper.ActivePlc(IReadWriteDevice)"/>
		public async Task<OperateResult> ActivePlcAsync( ) => await Helper.MelsecFxSerialHelper.ActivePlcAsync( this );
#endif

		#endregion

		#region Async Read Write Bool
#if !NET35 && !NET20
		/// <inheritdoc/>
		public override async Task<OperateResult> WriteAsync( string address, bool value ) => await Task.Run( ( ) => Write( address, value ) );
#endif
		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString( ) => $"MelsecFxSerial[{PortName}:{this.pipeSerial.GetPipe().BaudRate}]";

		#endregion
	}
}
