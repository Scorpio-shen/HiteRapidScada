using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;

namespace HslCommunication.Core.Pipe
{
	/// <summary>
	/// 串口的管道类对象，可以在不同的串口类中使用一个串口的通道信息<br />
	/// The pipe class object of the serial port can use the channel information of a serial port in different serial port classes
	/// </summary>
	public class PipeSerial : PipeBase, IDisposable
	{
		/// <summary>
		/// 实例化一个默认的对象
		/// </summary>
		public PipeSerial( )
		{
			this.serialPort = new SerialPort( );
		}

		/// <summary>
		/// 初始化串口信息，波特率，数据位，停止位，奇偶校验需要全部自己来指定<br />
		/// Start serial port information, baud rate, data bit, stop bit, parity all need to be specified
		/// </summary>
		/// <param name="portName">端口号信息，例如"COM3"</param>
		/// <param name="baudRate">波特率</param>
		/// <param name="dataBits">数据位</param>
		/// <param name="stopBits">停止位</param>
		/// <param name="parity">奇偶校验</param>
		public void SerialPortInni( string portName, int baudRate, int dataBits, StopBits stopBits, Parity parity )
		{
			if (this.serialPort.IsOpen) return;
			this.serialPort.PortName = portName;    // 串口
			this.serialPort.BaudRate = baudRate;    // 波特率
			this.serialPort.DataBits = dataBits;    // 数据位
			this.serialPort.StopBits = stopBits;    // 停止位
			this.serialPort.Parity   = parity;      // 奇偶校验
		}

		/// <summary>
		/// 根据自定义初始化方法进行初始化串口信息<br />
		/// Initialize the serial port information according to the custom initialization method
		/// </summary>
		/// <param name="initi">初始化的委托方法</param>
		public void SerialPortInni( Action<SerialPort> initi )
		{
			if (this.serialPort.IsOpen) return;
			this.serialPort.PortName = "COM1";    // 串口
			initi.Invoke( this.serialPort );
		}

		/// <summary>
		/// 打开一个新的串行端口连接<br />
		/// Open a new serial port connection
		/// </summary>
		public OperateResult Open( )
		{
			try
			{
				if (!this.serialPort.IsOpen) this.serialPort.Open( );
				return OperateResult.CreateSuccessResult( );
			}
			catch (Exception ex)
			{
				return new OperateResult( ex.Message );
			}
		}

		/// <summary>
		/// 获取一个值，指示串口是否处于打开状态<br />
		/// Gets a value indicating whether the serial port is open
		/// </summary>
		/// <returns>是或否</returns>
		public bool IsOpen( ) => this.serialPort.IsOpen;

		/// <summary>
		/// 关闭当前的串口连接<br />
		/// Close the current serial connection
		/// </summary>
		public OperateResult Close( Func<SerialPort, OperateResult> extraOnClose )
		{
			if (this.serialPort.IsOpen)
			{
				// 先执行关闭之间的额外操作信息，如果有的话
				if (extraOnClose != null)
				{
					OperateResult op = extraOnClose.Invoke( this.serialPort );
					if (!op.IsSuccess) return op;
				}

				try
				{
					this.serialPort.Close( );
				}
				catch (Exception ex)
				{
					return new OperateResult( ex.Message );
				}
			}
			return OperateResult.CreateSuccessResult( );
		}

		/// <summary>
		/// 获取或设置一个值，该值指示在串行通信中是否启用请求发送 (RTS) 信号。<br />
		/// Gets or sets a value indicating whether the request sending (RTS) signal is enabled in serial communication.
		/// </summary>
		public bool RtsEnable
		{
			get => this.serialPort.RtsEnable;
			set => this.serialPort.RtsEnable = value;
		}

		/// <inheritdoc cref="IDisposable.Dispose"/>
		public override void Dispose( )
		{
			base.Dispose( );
			this.serialPort?.Dispose( );
		}

		/// <summary>
		/// 获取当前的串口对象信息<br />
		/// Get current serial port object information
		/// </summary>
		/// <returns>串口对象</returns>
		public SerialPort GetPipe( ) => this.serialPort;

		/// <inheritdoc/>
		public override string ToString( ) => $"PipeSerial[{serialPort.PortName},{serialPort.BaudRate},{serialPort.DataBits},{serialPort.StopBits},{serialPort.Parity}]";

		private SerialPort serialPort;                           // 共享的串口通道信息

	}
}
