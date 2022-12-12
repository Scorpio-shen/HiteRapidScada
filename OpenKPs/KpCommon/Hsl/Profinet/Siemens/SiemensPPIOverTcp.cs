using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HslCommunication.Core;
using HslCommunication.Core.Address;
using HslCommunication.Core.Net;
using HslCommunication.Reflection;
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
			this.SleepTime          = 20;
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

		#endregion

		#region Public Properties

		/// <inheritdoc cref="SiemensPPI.Station"/>
		public byte Station { get => station; set => station = value; }

		#endregion

		#region Read Write Override

		/// <inheritdoc cref="SiemensPPI.Read(string, ushort)"/>
		[HslMqttApi( "ReadByteArray", "" )]
		public override OperateResult<byte[]> Read( string address, ushort length )
		{
			byte stat = (byte)HslHelper.ExtractParameter( ref address, "s", this.Station );

			// 解析指令
			OperateResult<byte[]> command = BuildReadCommand( stat, address, length, false );
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

		/// <inheritdoc cref="SiemensPPI.ReadBool(string, ushort)"/>
		[HslMqttApi( "ReadBoolArray", "" )]
		public override OperateResult<bool[]> ReadBool( string address, ushort length )
		{
			byte stat = (byte)HslHelper.ExtractParameter( ref address, "s", this.Station );

			// 解析指令
			OperateResult<byte[]> command = BuildReadCommand( stat, address, length, true );
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

		/// <inheritdoc cref="SiemensPPI.Write(string, byte[])"/>
		[HslMqttApi( "WriteByteArray", "" )]
		public override OperateResult Write( string address, byte[] value )
		{
			byte stat = (byte)HslHelper.ExtractParameter( ref address, "s", this.Station );

			// 解析指令
			OperateResult<byte[]> command = BuildWriteCommand( stat, address, value );
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

		/// <inheritdoc cref="SiemensPPI.Write(string, bool[])"/>
		[HslMqttApi( "WriteBoolArray", "" )]
		public override OperateResult Write( string address, bool[] value )
		{
			byte stat = (byte)HslHelper.ExtractParameter( ref address, "s", this.Station );

			// 解析指令
			OperateResult<byte[]> command = BuildWriteCommand( stat, address, value );
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

		#region Async Read Write Override
#if !NET35 && !NET20
		/// <inheritdoc cref="Read(string, ushort)"/>
		public override async Task<OperateResult<byte[]>> ReadAsync( string address, ushort length )
		{
			byte stat = (byte)HslHelper.ExtractParameter( ref address, "s", this.Station );

			// 解析指令
			OperateResult<byte[]> command = BuildReadCommand( stat, address, length, false );
			if (!command.IsSuccess) return command;

			// 第一次数据交互
			OperateResult<byte[]> read1 = await ReadFromCoreServerAsync( command.Content );
			if (!read1.IsSuccess) return read1;

			// 验证
			if (read1.Content[0] != 0xE5) return new OperateResult<byte[]>( "PLC Receive Check Failed:" + BasicFramework.SoftBasic.ByteToHexString( read1.Content, ' ' ) );

			// 第二次数据交互
			OperateResult<byte[]> read2 = await ReadFromCoreServerAsync( SiemensPPIOverTcp.GetExecuteConfirm( stat ) );
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

		/// <inheritdoc cref="ReadBool(string, ushort)"/>
		public override async Task<OperateResult<bool[]>> ReadBoolAsync( string address, ushort length )
		{
			byte stat = (byte)HslHelper.ExtractParameter( ref address, "s", this.Station );

			// 解析指令
			OperateResult<byte[]> command = BuildReadCommand( stat, address, length, true );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( command );

			// 第一次数据交互
			OperateResult<byte[]> read1 = await ReadFromCoreServerAsync( command.Content );
			if (!read1.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( read1 );

			// 验证
			if (read1.Content[0] != 0xE5) return new OperateResult<bool[]>( "PLC Receive Check Failed:" + BasicFramework.SoftBasic.ByteToHexString( read1.Content, ' ' ) );

			// 第二次数据交互
			OperateResult<byte[]> read2 = await ReadFromCoreServerAsync( SiemensPPIOverTcp.GetExecuteConfirm( stat ) );
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

		/// <inheritdoc cref="Write(string, byte[])"/>
		public override async Task<OperateResult> WriteAsync( string address, byte[] value )
		{
			byte stat = (byte)HslHelper.ExtractParameter( ref address, "s", this.Station );

			// 解析指令
			OperateResult<byte[]> command = BuildWriteCommand( stat, address, value );
			if (!command.IsSuccess) return command;

			// 第一次数据交互
			OperateResult<byte[]> read1 = await ReadFromCoreServerAsync( command.Content );
			if (!read1.IsSuccess) return read1;

			// 验证
			if (read1.Content[0] != 0xE5) return new OperateResult<byte[]>( "PLC Receive Check Failed:" + read1.Content[0] );

			// 第二次数据交互
			OperateResult<byte[]> read2 = await ReadFromCoreServerAsync( SiemensPPIOverTcp.GetExecuteConfirm( stat ) );
			if (!read2.IsSuccess) return read2;

			// 错误码判断
			OperateResult check = SiemensPPIOverTcp.CheckResponse( read2.Content );
			if (!check.IsSuccess) return check;

			// 数据提取
			return OperateResult.CreateSuccessResult( );
		}

		/// <inheritdoc cref="Write(string, bool[])"/>
		public override async Task<OperateResult> WriteAsync( string address, bool[] value )
		{
			byte stat = (byte)HslHelper.ExtractParameter( ref address, "s", this.Station );

			// 解析指令
			OperateResult<byte[]> command = BuildWriteCommand( stat, address, value );
			if (!command.IsSuccess) return command;

			// 第一次数据交互
			OperateResult<byte[]> read1 = await ReadFromCoreServerAsync( command.Content );
			if (!read1.IsSuccess) return read1;

			// 验证
			if (read1.Content[0] != 0xE5) return new OperateResult<byte[]>( "PLC Receive Check Failed:" + read1.Content[0] );

			// 第二次数据交互
			OperateResult<byte[]> read2 = await ReadFromCoreServerAsync( SiemensPPIOverTcp.GetExecuteConfirm( stat ) );
			if (!read2.IsSuccess) return read2;

			// 错误码判断
			OperateResult check = SiemensPPIOverTcp.CheckResponse( read2.Content );
			if (!check.IsSuccess) return check;

			// 数据提取
			return OperateResult.CreateSuccessResult( );
		}
#endif
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
		/// <inheritdoc cref="ReadByte(string)"/>
		public async Task<OperateResult<byte>> ReadByteAsync( string address ) => ByteTransformHelper.GetResultFromArray( await ReadAsync( address, 1 ) );

		/// <inheritdoc cref="Write(string, byte)"/>
		public async Task<OperateResult> WriteAsync( string address, byte value ) => await WriteAsync( address, new byte[] { value } );
#endif
		#endregion

		#region Start Stop

		/// <inheritdoc cref="SiemensPPI.Start"/>
		[HslMqttApi]
		public OperateResult Start( string parameter = "" )
		{
			byte stat = (byte)HslHelper.ExtractParameter( ref parameter, "s", this.Station );

			byte[] cmd = new byte[] { 0x68, 0x21, 0x21, 0x68, station, 0x00, 0x6C, 0x32, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x14, 0x00, 0x00, 0x28, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFD, 0x00, 0x00, 0x09, 0x50, 0x5F, 0x50, 0x52, 0x4F, 0x47, 0x52, 0x41, 0x4D, 0xAA, 0x16 };

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

		/// <inheritdoc cref="SiemensPPI.Stop"/>
		[HslMqttApi]
		public OperateResult Stop( string parameter = "" )
		{
			byte stat = (byte)HslHelper.ExtractParameter( ref parameter, "s", this.Station );

			byte[] cmd = new byte[] { 0x68, 0x1D, 0x1D, 0x68, station, 0x00, 0x6C, 0x32, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 0x29, 0x00, 0x00, 0x00, 0x00, 0x00, 0x09, 0x50, 0x5F, 0x50, 0x52, 0x4F, 0x47, 0x52, 0x41, 0x4D, 0xAA, 0x16 };

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

		#region Async Start Stop
#if !NET35 && !NET20
		/// <inheritdoc cref="SiemensPPI.Start"/>
		public async Task<OperateResult> StartAsync( string parameter = "" )
		{
			byte stat = (byte)HslHelper.ExtractParameter( ref parameter, "s", this.Station );

			byte[] cmd = new byte[] { 0x68, 0x21, 0x21, 0x68, station, 0x00, 0x6C, 0x32, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x14, 0x00, 0x00, 0x28, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFD, 0x00, 0x00, 0x09, 0x50, 0x5F, 0x50, 0x52, 0x4F, 0x47, 0x52, 0x41, 0x4D, 0xAA, 0x16 };
			
			// 第一次数据交互
			OperateResult<byte[]> read1 = await ReadFromCoreServerAsync( cmd );
			if (!read1.IsSuccess) return read1;

			// 验证
			if (read1.Content[0] != 0xE5) return new OperateResult<byte[]>( "PLC Receive Check Failed:" + read1.Content[0] );

			// 第二次数据交互
			OperateResult<byte[]> read2 = await ReadFromCoreServerAsync( SiemensPPIOverTcp.GetExecuteConfirm( stat ) );
			if (!read2.IsSuccess) return read2;

			// 数据提取
			return OperateResult.CreateSuccessResult( );
		}

		/// <inheritdoc cref="SiemensPPI.Stop"/>
		public async Task<OperateResult> StopAsync( string parameter = "" )
		{
			byte stat = (byte)HslHelper.ExtractParameter( ref parameter, "s", this.Station );

			byte[] cmd = new byte[] { 0x68, 0x1D, 0x1D, 0x68, station, 0x00, 0x6C, 0x32, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 0x29, 0x00, 0x00, 0x00, 0x00, 0x00, 0x09, 0x50, 0x5F, 0x50, 0x52, 0x4F, 0x47, 0x52, 0x41, 0x4D, 0xAA, 0x16 };
			// 第一次数据交互
			OperateResult<byte[]> read1 = await ReadFromCoreServerAsync( cmd );
			if (!read1.IsSuccess) return read1;

			// 验证
			if (read1.Content[0] != 0xE5) return new OperateResult<byte[]>( "PLC Receive Check Failed:" + read1.Content[0] );

			// 第二次数据交互
			OperateResult<byte[]> read2 = await ReadFromCoreServerAsync( SiemensPPIOverTcp.GetExecuteConfirm( stat ) );
			if (!read2.IsSuccess) return read2;

			// 数据提取
			return OperateResult.CreateSuccessResult( );
		}
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

		#region Static Helper

		/// <summary>
		/// 解析数据地址，解析出地址类型，起始地址，DB块的地址<br />
		/// Parse data address, parse out address type, start address, db block address
		/// </summary>
		/// <param name="address">起始地址，例如M100，I0，Q0，V100 ->
		/// Start address, such as M100,I0,Q0,V100</param>
		/// <returns>解析数据地址，解析出地址类型，起始地址，DB块的地址 ->
		/// Parse data address, parse out address type, start address, db block address</returns>
		public static OperateResult<byte, int, ushort> AnalysisAddress( string address )
		{
			var result = new OperateResult<byte, int, ushort>( );
			try
			{
				result.Content3 = 0;
				if (address.Substring( 0, 2 ) == "AI")
				{
					result.Content1 = 0x06;
					result.Content2 = S7AddressData.CalculateAddressStarted( address.Substring( 2 ) );
				}
				else if (address.Substring( 0, 2 ) == "AQ")
				{
					result.Content1 = 0x07;
					result.Content2 = S7AddressData.CalculateAddressStarted( address.Substring( 2 ) );
				}
				else if (address[0] == 'T')
				{
					result.Content1 = 0x1F;
					result.Content2 = S7AddressData.CalculateAddressStarted( address.Substring( 1 ) );
				}
				else if (address[0] == 'C')
				{
					result.Content1 = 0x1E;
					result.Content2 = S7AddressData.CalculateAddressStarted( address.Substring( 1 ) );
				}
				else if (address.Substring( 0, 2 ) == "SM")
				{
					result.Content1 = 0x05;
					result.Content2 = S7AddressData.CalculateAddressStarted( address.Substring( 2 ) );
				}
				else if (address[0] == 'S')
				{
					result.Content1 = 0x04;
					result.Content2 = S7AddressData.CalculateAddressStarted( address.Substring( 1 ) );
				}
				else if (address[0] == 'I')
				{
					result.Content1 = 0x81;
					result.Content2 = S7AddressData.CalculateAddressStarted( address.Substring( 1 ) );
				}
				else if (address[0] == 'Q')
				{
					result.Content1 = 0x82;
					result.Content2 = S7AddressData.CalculateAddressStarted( address.Substring( 1 ) );
				}
				else if (address[0] == 'M')
				{
					result.Content1 = 0x83;
					result.Content2 = S7AddressData.CalculateAddressStarted( address.Substring( 1 ) );
				}
				else if (address[0] == 'D' || address.Substring( 0, 2 ) == "DB")
				{
					result.Content1 = 0x84;
					string[] adds = address.Split( '.' );
					if (address[1] == 'B')
					{
						result.Content3 = Convert.ToUInt16( adds[0].Substring( 2 ) );
					}
					else
					{
						result.Content3 = Convert.ToUInt16( adds[0].Substring( 1 ) );
					}

					result.Content2 = S7AddressData.CalculateAddressStarted( address.Substring( address.IndexOf( '.' ) + 1 ) );
				}
				else if (address[0] == 'V')
				{
					result.Content1 = 0x84;
					result.Content3 = 1;
					result.Content2 = S7AddressData.CalculateAddressStarted( address.Substring( 1 ) );
				}
				else
				{
					result.Message = StringResources.Language.NotSupportedDataType;
					result.Content1 = 0;
					result.Content2 = 0;
					result.Content3 = 0;
					return result;
				}
			}
			catch (Exception ex)
			{
				result.Message = ex.Message;
				return result;
			}

			result.IsSuccess = true;
			return result;
		}

		/// <summary>
		/// 生成一个读取字数据指令头的通用方法<br />
		/// A general method for generating a command header to read a Word data
		/// </summary>
		/// <param name="station">设备的站号信息 -> Station number information for the device</param>
		/// <param name="address">起始地址，例如M100，I0，Q0，V100 ->
		/// Start address, such as M100,I0,Q0,V100</param>
		/// <param name="length">读取数据长度 -> Read Data length</param>
		/// <param name="isBit">是否为位读取</param>
		/// <returns>包含结果对象的报文 -> Message containing the result object</returns>
		public static OperateResult<byte[]> BuildReadCommand( byte station, string address, ushort length, bool isBit )
		{
			OperateResult<byte, int, ushort> analysis = AnalysisAddress( address );
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( analysis );

			byte[] _PLCCommand = new byte[33];
			_PLCCommand[0] = 0x68;
			_PLCCommand[1] = BitConverter.GetBytes( _PLCCommand.Length - 6 )[0];
			_PLCCommand[2] = BitConverter.GetBytes( _PLCCommand.Length - 6 )[0];
			_PLCCommand[3] = 0x68;
			_PLCCommand[4] = station;
			_PLCCommand[5] = 0x00;
			_PLCCommand[6] = 0x6C;
			_PLCCommand[7] = 0x32;
			_PLCCommand[8] = 0x01;
			_PLCCommand[9] = 0x00;
			_PLCCommand[10] = 0x00;
			_PLCCommand[11] = 0x00;
			_PLCCommand[12] = 0x00;
			_PLCCommand[13] = 0x00;
			_PLCCommand[14] = 0x0E;
			_PLCCommand[15] = 0x00;
			_PLCCommand[16] = 0x00;
			_PLCCommand[17] = 0x04;
			_PLCCommand[18] = 0x01;
			_PLCCommand[19] = 0x12;
			_PLCCommand[20] = 0x0A;
			_PLCCommand[21] = 0x10;

			_PLCCommand[22] = isBit ? (byte)0x01 : (byte)0x02;
			_PLCCommand[23] = 0x00;
			_PLCCommand[24] = BitConverter.GetBytes( length )[0];
			_PLCCommand[25] = BitConverter.GetBytes( length )[1];
			_PLCCommand[26] = (byte)analysis.Content3;
			_PLCCommand[27] = analysis.Content1;
			_PLCCommand[28] = BitConverter.GetBytes( analysis.Content2 )[2];
			_PLCCommand[29] = BitConverter.GetBytes( analysis.Content2 )[1];
			_PLCCommand[30] = BitConverter.GetBytes( analysis.Content2 )[0];

			int count = 0;
			for (int i = 4; i < 31; i++)
			{
				count += _PLCCommand[i];
			}
			_PLCCommand[31] = BitConverter.GetBytes( count )[0];
			_PLCCommand[32] = 0x16;

			return OperateResult.CreateSuccessResult( _PLCCommand );
		}

		/// <summary>
		/// 生成一个写入PLC数据信息的报文内容
		/// </summary>
		/// <param name="station">PLC的站号</param>
		/// <param name="address">地址</param>
		/// <param name="values">数据值</param>
		/// <returns>是否写入成功</returns>
		public static OperateResult<byte[]> BuildWriteCommand( byte station, string address, byte[] values )
		{
			OperateResult<byte, int, ushort> analysis = AnalysisAddress( address );
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( analysis );

			int length = values.Length;
			// 68 21 21 68 02 00 6C 32 01 00 00 00 00 00 0E 00 00 04 01 12 0A 10
			byte[] _PLCCommand = new byte[37 + values.Length];
			_PLCCommand[0] = 0x68;
			_PLCCommand[1] = BitConverter.GetBytes( _PLCCommand.Length - 6 )[0];
			_PLCCommand[2] = BitConverter.GetBytes( _PLCCommand.Length - 6 )[0];
			_PLCCommand[3] = 0x68;
			_PLCCommand[4] = station;
			_PLCCommand[5] = 0x00;
			_PLCCommand[6] = 0x7C;
			_PLCCommand[7] = 0x32;
			_PLCCommand[8] = 0x01;
			_PLCCommand[9] = 0x00;
			_PLCCommand[10] = 0x00;
			_PLCCommand[11] = 0x00;
			_PLCCommand[12] = 0x00;
			_PLCCommand[13] = 0x00;
			_PLCCommand[14] = 0x0E;
			_PLCCommand[15] = 0x00;
			_PLCCommand[16] = (byte)(values.Length + 4);
			_PLCCommand[17] = 0x05;
			_PLCCommand[18] = 0x01;
			_PLCCommand[19] = 0x12;
			_PLCCommand[20] = 0x0A;
			_PLCCommand[21] = 0x10;

			_PLCCommand[22] = 0x02;
			_PLCCommand[23] = 0x00;
			_PLCCommand[24] = BitConverter.GetBytes( length )[0];
			_PLCCommand[25] = BitConverter.GetBytes( length )[1];
			_PLCCommand[26] = (byte)analysis.Content3;
			_PLCCommand[27] = analysis.Content1;
			_PLCCommand[28] = BitConverter.GetBytes( analysis.Content2 )[2];
			_PLCCommand[29] = BitConverter.GetBytes( analysis.Content2 )[1];
			_PLCCommand[30] = BitConverter.GetBytes( analysis.Content2 )[0];

			_PLCCommand[31] = 0x00;
			_PLCCommand[32] = 0x04;
			_PLCCommand[33] = BitConverter.GetBytes( length * 8 )[1];
			_PLCCommand[34] = BitConverter.GetBytes( length * 8 )[0];


			values.CopyTo( _PLCCommand, 35 );

			int count = 0;
			for (int i = 4; i < _PLCCommand.Length - 2; i++)
			{
				count += _PLCCommand[i];
			}
			_PLCCommand[_PLCCommand.Length - 2] = BitConverter.GetBytes( count )[0];
			_PLCCommand[_PLCCommand.Length - 1] = 0x16;


			return OperateResult.CreateSuccessResult( _PLCCommand );
		}

		/// <summary>
		/// 根据错误信息，获取到文本信息
		/// </summary>
		/// <param name="code">状态</param>
		/// <returns>消息文本</returns>
		public static string GetMsgFromStatus( byte code )
		{
			switch (code)
			{
				case 0xFF: return "No error";
				case 0x01: return "Hardware fault";
				case 0x03: return "Illegal object access";
				case 0x05: return "Invalid address(incorrent variable address)";
				case 0x06: return "Data type is not supported";
				case 0x0A: return "Object does not exist or length error";
				default: return StringResources.Language.UnknownError;
			}
		}

		/// <summary>
		/// 根据错误信息，获取到文本信息
		/// </summary>
		/// <param name="errorClass">错误类型</param>
		/// <param name="errorCode">错误代码</param>
		/// <returns>错误信息</returns>
		public static string GetMsgFromStatus( byte errorClass, byte errorCode )
		{
			if (errorClass == 0x80 && errorCode == 0x01)
			{
				return "Switch in wrong position for requested operation";
			}
			else if (errorClass == 0x81 && errorCode == 0x04)
			{
				return "Miscellaneous structure error in command.  Command is not supportedby CPU";
			}
			else if (errorClass == 0x84 && errorCode == 0x04)
			{
				return "CPU is busy processing an upload or download CPU cannot process command because of system fault condition";
			}
			else if (errorClass == 0x85 && errorCode == 0x00)
			{
				return "Length fields are not correct or do not agree with the amount of data received";
			}
			else if (errorClass == 0xD2)
			{
				return "Error in upload or download command";
			}
			else if (errorClass == 0xD6)
			{
				return "Protection error(password)";
			}
			else if (errorClass == 0xDC && errorCode == 0x01)
			{
				return "Error in time-of-day clock data";
			}
			else
			{
				return StringResources.Language.UnknownError;
			}
		}

		/// <summary>
		/// 创建写入PLC的bool类型数据报文指令
		/// </summary>
		/// <param name="station">PLC的站号信息</param>
		/// <param name="address">地址信息</param>
		/// <param name="values">bool[]数据值</param>
		/// <returns>带有成功标识的结果对象</returns>
		public static OperateResult<byte[]> BuildWriteCommand( byte station, string address, bool[] values )
		{
			OperateResult<byte, int, ushort> analysis = AnalysisAddress( address );
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( analysis );

			byte[] bytesValue = BasicFramework.SoftBasic.BoolArrayToByte( values );
			// 68 21 21 68 02 00 6C 32 01 00 00 00 00 00 0E 00 00 04 01 12 0A 10
			byte[] _PLCCommand = new byte[37 + bytesValue.Length];
			_PLCCommand[0] = 0x68;
			_PLCCommand[1] = BitConverter.GetBytes( _PLCCommand.Length - 6 )[0];
			_PLCCommand[2] = BitConverter.GetBytes( _PLCCommand.Length - 6 )[0];
			_PLCCommand[3] = 0x68;
			_PLCCommand[4] = station;
			_PLCCommand[5] = 0x00;
			_PLCCommand[6] = 0x7C;
			_PLCCommand[7] = 0x32;
			_PLCCommand[8] = 0x01;
			_PLCCommand[9] = 0x00;
			_PLCCommand[10] = 0x00;
			_PLCCommand[11] = 0x00;
			_PLCCommand[12] = 0x00;
			_PLCCommand[13] = 0x00;
			_PLCCommand[14] = 0x0E;
			_PLCCommand[15] = 0x00;
			_PLCCommand[16] = 0x05;
			_PLCCommand[17] = 0x05;
			_PLCCommand[18] = 0x01;
			_PLCCommand[19] = 0x12;
			_PLCCommand[20] = 0x0A;
			_PLCCommand[21] = 0x10;

			_PLCCommand[22] = 0x01;
			_PLCCommand[23] = 0x00;
			_PLCCommand[24] = BitConverter.GetBytes( values.Length )[0];
			_PLCCommand[25] = BitConverter.GetBytes( values.Length )[1];
			_PLCCommand[26] = (byte)analysis.Content3;
			_PLCCommand[27] = analysis.Content1;
			_PLCCommand[28] = BitConverter.GetBytes( analysis.Content2 )[2];
			_PLCCommand[29] = BitConverter.GetBytes( analysis.Content2 )[1];
			_PLCCommand[30] = BitConverter.GetBytes( analysis.Content2 )[0];

			_PLCCommand[31] = 0x00;
			_PLCCommand[32] = 0x03;
			_PLCCommand[33] = BitConverter.GetBytes( values.Length )[1];
			_PLCCommand[34] = BitConverter.GetBytes( values.Length )[0];


			bytesValue.CopyTo( _PLCCommand, 35 );

			int count = 0;
			for (int i = 4; i < _PLCCommand.Length - 2; i++)
			{
				count += _PLCCommand[i];
			}
			_PLCCommand[_PLCCommand.Length - 2] = BitConverter.GetBytes( count )[0];
			_PLCCommand[_PLCCommand.Length - 1] = 0x16;


			return OperateResult.CreateSuccessResult( _PLCCommand );
		}

		/// <summary>
		/// 检查西门子PLC的返回的数据和合法性，对反馈的数据进行初步的校验
		/// </summary>
		/// <param name="content">服务器返回的原始的数据内容</param>
		/// <returns>是否校验成功</returns>
		public static OperateResult CheckResponse( byte[] content )
		{
			if (content.Length < 21) return new OperateResult( 10000, "Failed, data too short:" + BasicFramework.SoftBasic.ByteToHexString( content, ' ' ) );
			if (content[17] != 0x00 || content[18] != 0x00) return new OperateResult( content[19], SiemensPPIOverTcp.GetMsgFromStatus( content[18], content[19] ) );
			if (content[21] != 0xFF) return new OperateResult( content[21], SiemensPPIOverTcp.GetMsgFromStatus( content[21] ) );
			return OperateResult.CreateSuccessResult( );
		}

		/// <summary>
		/// 根据站号信息获取命令二次确认的报文信息
		/// </summary>
		/// <param name="station">站号信息</param>
		/// <returns>二次命令确认的报文</returns>
		public static byte[] GetExecuteConfirm( byte station )
		{
			byte[] buffer = new byte[] { 0x10, 0x02, 0x00, 0x5C, 0x5E, 0x16 };
			buffer[1] = station;

			int count = 0;
			for (int i = 1; i < 4; i++)
			{
				count += buffer[i];
			}
			buffer[4] = (byte)count;
			return buffer;
		}


		#endregion
	}
}
