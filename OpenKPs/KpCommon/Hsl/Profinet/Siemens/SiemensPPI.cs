using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HslCommunication.Serial;
using HslCommunication.Core;
using HslCommunication.Reflection;

namespace HslCommunication.Profinet.Siemens
{
	/// <summary>
	/// 西门子的PPI协议，适用于s7-200plc，注意，由于本类库的每次通讯分成2次操作，内部增加了一个同步锁，所以单次通信时间比较久，另外，地址支持携带站号，例如：s=2;M100<br />
	/// Siemens' PPI protocol is suitable for s7-200plc. Note that since each communication of this class library is divided into two operations, 
	/// and a synchronization lock is added inside, the single communication time is relatively long. In addition, 
	/// the address supports carrying the station number, for example : S=2;M100
	/// </summary>
	/// <remarks>
	/// 适用于西门子200的通信，非常感谢 合肥-加劲 的测试，让本类库圆满完成。注意：M地址范围有限 0-31地址<br />
	/// 在本类的<see cref="SiemensPPIOverTcp"/>实现类里，如果使用了Async的异步方法，没有增加同步锁，多线程调用可能会引发数据错乱的情况。<br />
	/// In the <see cref="SiemensPPIOverTcp"/> implementation class of this class, if the asynchronous method of Async is used, 
	/// the synchronization lock is not added, and multi-threaded calls may cause data disorder.
	/// </remarks>
	public class SiemensPPI : SerialDeviceBase
	{
		#region Constructor

		/// <summary>
		/// 实例化一个西门子的PPI协议对象<br />
		/// Instantiate a Siemens PPI protocol object
		/// </summary>
		public SiemensPPI( )
		{
			this.ByteTransform     = new ReverseBytesTransform( );
			this.WordLength        = 2;
			this.communicationLock = new object( );
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// 西门子PLC的站号信息<br />
		/// Siemens PLC station number information
		/// </summary>
		public byte Station { get => station; set => station = value; }

		#endregion

		#region Read Write Override

		/// <summary>
		/// 从西门子的PLC中读取数据信息，地址为"M100","AI100","I0","Q0","V100","S100"等<br />
		/// Read data information from Siemens PLC with addresses "M100", "AI100", "I0", "Q0", "V100", "S100", etc.
		/// </summary>
		/// <param name="address">西门子的地址数据信息</param>
		/// <param name="length">数据长度</param>
		/// <returns>带返回结果的结果对象</returns>
		[HslMqttApi( "ReadByteArray", "" )]
		public override OperateResult<byte[]> Read( string address, ushort length )
		{
			byte stat = (byte)HslHelper.ExtractParameter( ref address, "s", this.Station );

			// 解析指令
			OperateResult<byte[]> command = SiemensPPIOverTcp.BuildReadCommand( stat, address, length, false );
			if (!command.IsSuccess) return command;

			lock (communicationLock)
			{
				// 第一次数据交互
				OperateResult<byte[]> read1 = ReadFromCoreServer( command.Content );
				if (!read1.IsSuccess) return read1;

				// 验证
				if (read1.Content[0] != 0xE5) return new OperateResult<byte[]>( "PLC Receive Check Failed:" + BasicFramework.SoftBasic.ByteToHexString( read1.Content, ' ' ) );

				// 第二次数据交互
				OperateResult<byte[]> read2 = ReadFromCoreServer( SiemensPPIOverTcp.GetExecuteConfirm( stat ) );
				if (!read2.IsSuccess) return read2;

				// 错误码判断
				OperateResult check = SiemensPPIOverTcp.CheckResponse( read2.Content );
				if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( check );

				// 数据提取
				byte[] buffer = new byte[length];
				if (read2.Content[21] == 0xFF && read2.Content[22] == 0x04)
				{
					Array.Copy( read2.Content, 25, buffer, 0, length );
				}
				return OperateResult.CreateSuccessResult( buffer );
			}
		}

		/// <summary>
		/// 从西门子的PLC中读取bool数据信息，地址为"M100.0","AI100.1","I0.3","Q0.6","V100.4","S100"等<br />
		/// Read bool data information from Siemens PLC, the addresses are "M100.0", "AI100.1", "I0.3", "Q0.6", "V100.4", "S100", etc.
		/// </summary>
		/// <param name="address">西门子的地址数据信息</param>
		/// <param name="length">数据长度</param>
		/// <returns>带返回结果的结果对象</returns>
		[HslMqttApi( "ReadBoolArray", "" )]
		public override OperateResult<bool[]> ReadBool( string address, ushort length )
		{
			byte stat = (byte)HslHelper.ExtractParameter( ref address, "s", this.Station );

			// 解析指令
			OperateResult<byte[]> command = SiemensPPIOverTcp.BuildReadCommand( stat, address, length, true );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( command );

			lock (communicationLock)
			{
				// 第一次数据交互
				OperateResult<byte[]> read1 = ReadFromCoreServer( command.Content );
				if (!read1.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( read1 );

				// 验证
				if (read1.Content[0] != 0xE5) return new OperateResult<bool[]>( "PLC Receive Check Failed:" + BasicFramework.SoftBasic.ByteToHexString( read1.Content, ' ' ) );

				// 第二次数据交互
				OperateResult<byte[]> read2 = ReadFromCoreServer( SiemensPPIOverTcp.GetExecuteConfirm( stat ) );
				if (!read2.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( read2 );

				// 错误码判断
				OperateResult check = SiemensPPIOverTcp.CheckResponse( read2.Content );
				if (!check.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( check );

				// 数据提取
				byte[] buffer = new byte[read2.Content.Length - 27];
				if (read2.Content[21] == 0xFF && read2.Content[22] == 0x03)
				{
					Array.Copy( read2.Content, 25, buffer, 0, buffer.Length );
				}

				return OperateResult.CreateSuccessResult( BasicFramework.SoftBasic.ByteToBoolArray( buffer, length ) );
			}
		}

		/// <summary>
		/// 将字节数据写入到西门子PLC中，地址为"M100.0","AI100.1","I0.3","Q0.6","V100.4","S100"等<br />
		/// Write byte data to Siemens PLC with addresses "M100.0", "AI100.1", "I0.3", "Q0.6", "V100.4", "S100", etc.
		/// </summary>
		/// <param name="address">西门子的地址数据信息</param>
		/// <param name="value">数据长度</param>
		/// <returns>带返回结果的结果对象</returns>
		[HslMqttApi( "WriteByteArray", "" )]
		public override OperateResult Write( string address, byte[] value )
		{
			byte stat = (byte)HslHelper.ExtractParameter( ref address, "s", this.Station );

			// 解析指令
			OperateResult<byte[]> command = SiemensPPIOverTcp.BuildWriteCommand( stat, address, value );
			if (!command.IsSuccess) return command;

			lock (communicationLock)
			{
				// 第一次数据交互
				OperateResult<byte[]> read1 = ReadFromCoreServer( command.Content );
				if (!read1.IsSuccess) return read1;

				// 验证
				if (read1.Content[0] != 0xE5) return new OperateResult<byte[]>( "PLC Receive Check Failed:" + read1.Content[0] );

				// 第二次数据交互
				OperateResult<byte[]> read2 = ReadFromCoreServer( SiemensPPIOverTcp.GetExecuteConfirm( stat ) );
				if (!read2.IsSuccess) return read2;

				// 错误码判断
				OperateResult check = SiemensPPIOverTcp.CheckResponse( read2.Content );
				if (!check.IsSuccess) return check;

				// 数据提取
				return OperateResult.CreateSuccessResult( );
			}
		}

		/// <summary>
		/// 将bool数据写入到西门子PLC中，地址为"M100.0","AI100.1","I0.3","Q0.6","V100.4","S100"等<br />
		/// Write the bool data to Siemens PLC with the addresses "M100.0", "AI100.1", "I0.3", "Q0.6", "V100.4", "S100", etc.
		/// </summary>
		/// <param name="address">西门子的地址数据信息</param>
		/// <param name="value">数据长度</param>
		/// <returns>带返回结果的结果对象</returns>
		[HslMqttApi( "WriteBoolArray", "" )]
		public override OperateResult Write(string address, bool[] value )
		{
			byte stat = (byte)HslHelper.ExtractParameter( ref address, "s", this.Station );

			// 解析指令
			OperateResult<byte[]> command = SiemensPPIOverTcp.BuildWriteCommand( stat, address, value );
			if (!command.IsSuccess) return command;

			lock (communicationLock)
			{
				// 第一次数据交互
				OperateResult<byte[]> read1 = ReadFromCoreServer( command.Content );
				if (!read1.IsSuccess) return read1;

				// 验证
				if (read1.Content[0] != 0xE5) return new OperateResult<byte[]>( "PLC Receive Check Failed:" + read1.Content[0] );

				// 第二次数据交互
				OperateResult<byte[]> read2 = ReadFromCoreServer( SiemensPPIOverTcp.GetExecuteConfirm( stat ) );
				if (!read2.IsSuccess) return read2;

				// 错误码判断
				OperateResult check = SiemensPPIOverTcp.CheckResponse( read2.Content );
				if (!check.IsSuccess) return check;

				// 数据提取
				return OperateResult.CreateSuccessResult( );
			}
		}

		#endregion

		#region Byte Read Write

		/// <summary>
		/// 从西门子的PLC中读取byte数据信息，地址为"M100","AI100","I0","Q0","V100","S100"等，详细请参照API文档<br />
		/// Read byte data information from Siemens PLC. The addresses are "M100", "AI100", "I0", "Q0", "V100", "S100", etc. Please refer to the API documentation for details.
		/// </summary>
		/// <param name="address">西门子的地址数据信息</param>
		/// <returns>带返回结果的结果对象</returns>
		[HslMqttApi( "ReadByte", "" )]
		public OperateResult<byte> ReadByte( string address ) => ByteTransformHelper.GetResultFromArray( Read( address, 1 ) );

		/// <summary>
		/// 向西门子的PLC中读取byte数据，地址为"M100","AI100","I0","Q0","V100","S100"等，详细请参照API文档<br />
		/// Read byte data from Siemens PLC with addresses "M100", "AI100", "I0", "Q0", "V100", "S100", etc. For details, please refer to the API documentation
		/// </summary>
		/// <param name="address">西门子的地址数据信息</param>
		/// <param name="value">数据长度</param>
		/// <returns>带返回结果的结果对象</returns>
		[HslMqttApi( "Write", "" )]
		public OperateResult Write(string address, byte value ) => Write( address, new byte[] { value } );

		#endregion

		#region Start Stop

		/// <summary>
		/// 启动西门子PLC为RUN模式，参数信息可以携带站号信息 "s=2;", 注意，分号是必须的。<br />
		/// Start Siemens PLC in RUN mode, parameter information can carry station number information "s=2;", note that the semicolon is required.
		/// </summary>
		/// <param name="parameter">额外的参数信息，例如可以携带站号信息 "s=2;", 注意，分号是必须的。</param>
		/// <returns>是否启动成功</returns>
		[HslMqttApi]
		public OperateResult Start( string parameter = "" )
		{
			byte stat = (byte)HslHelper.ExtractParameter( ref parameter, "s", this.Station );

			byte[] cmd = new byte[] { 0x68, 0x21, 0x21, 0x68, stat, 0x00, 0x6C, 0x32, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x14, 0x00, 0x00, 0x28, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFD, 0x00, 0x00, 0x09, 0x50, 0x5F, 0x50, 0x52, 0x4F, 0x47, 0x52, 0x41, 0x4D, 0xAA, 0x16 };

			lock (communicationLock)
			{
				// 第一次数据交互
				OperateResult<byte[]> read1 = ReadFromCoreServer( cmd );
				if (!read1.IsSuccess) return read1;

				// 验证
				if (read1.Content[0] != 0xE5) return new OperateResult<byte[]>( "PLC Receive Check Failed:" + read1.Content[0] );

				// 第二次数据交互
				OperateResult<byte[]> read2 = ReadFromCoreServer( SiemensPPIOverTcp.GetExecuteConfirm( stat ) );
				if (!read2.IsSuccess) return read2;

				// 数据提取
				return OperateResult.CreateSuccessResult( );
			}
		}

		/// <summary>
		/// 停止西门子PLC，切换为Stop模式，参数信息可以携带站号信息 "s=2;", 注意，分号是必须的。<br />
		/// Stop Siemens PLC and switch to Stop mode, parameter information can carry station number information "s=2;", note that the semicolon is required.
		/// </summary>
		/// <param name="parameter">额外的参数信息，例如可以携带站号信息 "s=2;", 注意，分号是必须的。</param>
		/// <returns>是否停止成功</returns>
		[HslMqttApi]
		public OperateResult Stop( string parameter = "" )
		{
			byte stat = (byte)HslHelper.ExtractParameter( ref parameter, "s", this.Station );

			byte[] cmd = new byte[] { 0x68, 0x1D, 0x1D, 0x68, stat, 0x00, 0x6C, 0x32, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 0x29, 0x00, 0x00, 0x00, 0x00, 0x00, 0x09, 0x50, 0x5F, 0x50, 0x52, 0x4F, 0x47, 0x52, 0x41, 0x4D, 0xAA, 0x16 };

			lock (communicationLock)
			{
				// 第一次数据交互
				OperateResult<byte[]> read1 = ReadFromCoreServer( cmd );
				if (!read1.IsSuccess) return read1;

				// 验证
				if (read1.Content[0] != 0xE5) return new OperateResult<byte[]>( "PLC Receive Check Failed:" + read1.Content[0] );

				// 第二次数据交互
				OperateResult<byte[]> read2 = ReadFromCoreServer( SiemensPPIOverTcp.GetExecuteConfirm( stat ) );
				if (!read2.IsSuccess) return read2;

				// 数据提取
				return OperateResult.CreateSuccessResult( );
			}
		}

		#endregion

		#region Private Member

		private byte station = 0x02;            // PLC的站号信息
		private object communicationLock;       // 通讯锁

		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString( ) => $"SiemensPPI[{PortName}:{BaudRate}]";

		#endregion
	}
}
