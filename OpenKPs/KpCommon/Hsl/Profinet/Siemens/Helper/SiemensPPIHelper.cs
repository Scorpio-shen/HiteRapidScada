using HslCommunication.Core;
using HslCommunication.Core.Address;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
#if !NET20&&!NET35
using System.Threading.Tasks;
#endif

namespace HslCommunication.Profinet.Siemens.Helper
{
	/// <summary>
	/// 西门子PPI协议的辅助类对象
	/// </summary>
	public class SiemensPPIHelper
	{
		#region Static Helper

		/// <summary>
		/// 解析数据地址，解析出地址类型，起始地址，DB块的地址<br />
		/// Parse data address, parse out address type, start address, db block address
		/// </summary>
		/// <param name="address">起始地址，例如M100，I0，Q0，V100 ->
		/// Start address, such as M100,I0,Q0,V100</param>
		/// <returns>解析数据地址，解析出地址类型，起始地址，DB块的地址 ->
		/// Parse data address, parse out address type, start address, db block address</returns>
		public static OperateResult<S7AddressData> AnalysisAddress( string address )
		{
			S7AddressData s7AddressData = new S7AddressData( );
			try
			{
				s7AddressData.DbBlock = 0;
				if (address.StartsWith( "SYS" ))
				{
					s7AddressData.DataCode     = 0x03;
					s7AddressData.AddressStart = S7AddressData.CalculateAddressStarted( address.Substring( 3 ) );
				}
				else if (address.StartsWith( "AI"))
				{
					s7AddressData.DataCode     = 0x06;
					s7AddressData.AddressStart = S7AddressData.CalculateAddressStarted( address.Substring( 2 ) );
				}
				else if (address.StartsWith( "AQ" ))
				{
					s7AddressData.DataCode     = 0x07;
					s7AddressData.AddressStart = S7AddressData.CalculateAddressStarted( address.Substring( 2 ) );
				}
				else if (address[0] == 'T')
				{
					s7AddressData.DataCode     = 0x1F;
					s7AddressData.AddressStart = S7AddressData.CalculateAddressStarted( address.Substring( 1 ) );
				}
				else if (address[0] == 'C')
				{
					s7AddressData.DataCode     = 0x1E;
					s7AddressData.AddressStart = S7AddressData.CalculateAddressStarted( address.Substring( 1 ) );
				}
				else if (address.StartsWith( "SM" ))
				{
					s7AddressData.DataCode     = 0x05;
					s7AddressData.AddressStart = S7AddressData.CalculateAddressStarted( address.Substring( 2 ) );
				}
				else if (address[0] == 'S')
				{
					s7AddressData.DataCode     = 0x04;
					s7AddressData.AddressStart = S7AddressData.CalculateAddressStarted( address.Substring( 1 ) );
				}
				else if (address[0] == 'I')
				{
					s7AddressData.DataCode     = 0x81;
					s7AddressData.AddressStart = S7AddressData.CalculateAddressStarted( address.Substring( 1 ) );
				}
				else if (address[0] == 'Q')
				{
					s7AddressData.DataCode     = 0x82;
					s7AddressData.AddressStart = S7AddressData.CalculateAddressStarted( address.Substring( 1 ) );
				}
				else if (address[0] == 'M')
				{
					s7AddressData.DataCode     = 0x83;
					s7AddressData.AddressStart = S7AddressData.CalculateAddressStarted( address.Substring( 1 ) );
				}
				else if (address[0] == 'D' || address.StartsWith( "DB" ))
				{
					s7AddressData.DataCode = 0x84;
					string[] adds = address.Split( '.' );
					if (address[1] == 'B')
					{
						s7AddressData.DbBlock = Convert.ToUInt16( adds[0].Substring( 2 ) );
					}
					else
					{
						s7AddressData.DbBlock = Convert.ToUInt16( adds[0].Substring( 1 ) );
					}

					s7AddressData.AddressStart = S7AddressData.CalculateAddressStarted( address.Substring( address.IndexOf( '.' ) + 1 ) );
				}
				else if (address[0] == 'V')
				{
					s7AddressData.DataCode = 0x84;
					s7AddressData.DbBlock = 1;
					s7AddressData.AddressStart = S7AddressData.CalculateAddressStarted( address.Substring( 1 ) );
				}
				else
				{
					return new OperateResult<S7AddressData>( StringResources.Language.NotSupportedDataType );
				}
			}
			catch (Exception ex)
			{
				return new OperateResult<S7AddressData>( ex.Message );
			}

			return OperateResult.CreateSuccessResult( s7AddressData );
		}

		/// <inheritdoc cref="BuildReadCommand(byte, S7AddressData, ushort, bool)"/>
		public static OperateResult<byte[]> BuildReadCommand( byte station, string address, ushort length, bool isBit )
		{
			OperateResult<S7AddressData> analysis = AnalysisAddress( address );
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( analysis );

			return BuildReadCommand( station, analysis.Content, length, isBit );
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
		public static OperateResult<byte[]> BuildReadCommand( byte station, S7AddressData address, ushort length, bool isBit )
		{
			byte[] _PLCCommand = new byte[33];
			_PLCCommand[0] = 0x68;
			_PLCCommand[ 1] = BitConverter.GetBytes( _PLCCommand.Length - 6 )[0];
			_PLCCommand[ 2] = BitConverter.GetBytes( _PLCCommand.Length - 6 )[0];
			_PLCCommand[ 3] = 0x68;
			_PLCCommand[ 4] = station;
			_PLCCommand[ 5] = 0x00;
			_PLCCommand[ 6] = 0x6C;
			_PLCCommand[ 7] = 0x32;
			_PLCCommand[ 8] = 0x01;
			_PLCCommand[ 9] = 0x00;
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
			_PLCCommand[26] = (byte)address.DbBlock;
			_PLCCommand[27] = address.DataCode;
			_PLCCommand[28] = BitConverter.GetBytes( address.AddressStart )[2];
			_PLCCommand[29] = BitConverter.GetBytes( address.AddressStart )[1];
			_PLCCommand[30] = BitConverter.GetBytes( address.AddressStart )[0];

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
			OperateResult<S7AddressData> analysis = AnalysisAddress( address );
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( analysis );

			int length = values.Length;
			// 68 21 21 68 02 00 6C 32 01 00 00 00 00 00 0E 00 00 04 01 12 0A 10
			byte[] _PLCCommand = new byte[37 + values.Length];
			_PLCCommand[ 0] = 0x68;
			_PLCCommand[ 1] = BitConverter.GetBytes( _PLCCommand.Length - 6 )[0];
			_PLCCommand[ 2] = BitConverter.GetBytes( _PLCCommand.Length - 6 )[0];
			_PLCCommand[ 3] = 0x68;
			_PLCCommand[ 4] = station;
			_PLCCommand[ 5] = 0x00;
			_PLCCommand[ 6] = 0x7C;
			_PLCCommand[ 7] = 0x32;
			_PLCCommand[ 8] = 0x01;
			_PLCCommand[ 9] = 0x00;
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
			_PLCCommand[26] = (byte)analysis.Content.DbBlock;
			_PLCCommand[27] = analysis.Content.DataCode;
			_PLCCommand[28] = BitConverter.GetBytes( analysis.Content.AddressStart )[2];
			_PLCCommand[29] = BitConverter.GetBytes( analysis.Content.AddressStart )[1];
			_PLCCommand[30] = BitConverter.GetBytes( analysis.Content.AddressStart )[0];

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
		/// 根据错误代号信息，获取到指定的文本信息<br />
		/// According to the error code information, get the specified text information
		/// </summary>
		/// <param name="code">错误状态信息</param>
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
			OperateResult<S7AddressData> analysis = AnalysisAddress( address );
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( analysis );

			byte[] bytesValue = BasicFramework.SoftBasic.BoolArrayToByte( values );
			// 68 21 21 68 02 00 6C 32 01 00 00 00 00 00 0E 00 00 04 01 12 0A 10
			byte[] _PLCCommand = new byte[37 + bytesValue.Length];
			_PLCCommand[ 0] = 0x68;
			_PLCCommand[ 1] = BitConverter.GetBytes( _PLCCommand.Length - 6 )[0];
			_PLCCommand[ 2] = BitConverter.GetBytes( _PLCCommand.Length - 6 )[0];
			_PLCCommand[ 3] = 0x68;
			_PLCCommand[ 4] = station;
			_PLCCommand[ 5] = 0x00;
			_PLCCommand[ 6] = 0x7C;
			_PLCCommand[ 7] = 0x32;
			_PLCCommand[ 8] = 0x01;
			_PLCCommand[ 9] = 0x00;
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
			_PLCCommand[26] = (byte)analysis.Content.DbBlock;
			_PLCCommand[27] = analysis.Content.DataCode;
			_PLCCommand[28] = BitConverter.GetBytes( analysis.Content.AddressStart )[2];
			_PLCCommand[29] = BitConverter.GetBytes( analysis.Content.AddressStart )[1];
			_PLCCommand[30] = BitConverter.GetBytes( analysis.Content.AddressStart )[0];

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

		/// <inheritdoc cref="Serial.SerialBase.CheckReceiveDataComplete(MemoryStream)"/>
		public static bool CheckReceiveDataComplete( MemoryStream ms )
		{
			byte[] buffer = ms.ToArray( );
			if (buffer.Length == 1 && buffer[0] == 0xE5) return true;

			if (buffer.Length > 6 && buffer[0] == 0x68 && (buffer[1] + 6 == buffer.Length) && buffer[buffer.Length - 1] == AsciiControl.SYN) return true;
			return false;
		}

		/// <summary>
		/// 检查西门子PLC的返回的数据和合法性，对反馈的数据进行初步的校验
		/// </summary>
		/// <param name="content">服务器返回的原始的数据内容</param>
		/// <returns>是否校验成功</returns>
		public static OperateResult CheckResponse( byte[] content )
		{
			if (content.Length < 21) return new OperateResult( 10000, "Failed, data too short:" + BasicFramework.SoftBasic.ByteToHexString( content, ' ' ) );
			if (content[17] != 0x00 || content[18] != 0x00) return new OperateResult( content[19], GetMsgFromStatus( content[18], content[19] ) );
			if (content[21] != 0xFF) return new OperateResult( content[21], GetMsgFromStatus( content[21] ) );
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

		private static OperateResult<byte[]> Read( IReadWriteDevice plc, S7AddressData address, ushort length, byte station, object communicationLock )
		{
			// 解析指令
			OperateResult<byte[]> command = BuildReadCommand( station, address, length, false );
			if (!command.IsSuccess) return command;

			lock (communicationLock)
			{
				// 第一次数据交互
				OperateResult<byte[]> read1 = plc.ReadFromCoreServer( command.Content );
				if (!read1.IsSuccess) return read1;

				// 验证
				if (read1.Content[0] != 0xE5) return new OperateResult<byte[]>( "PLC Receive Check Failed:" + BasicFramework.SoftBasic.ByteToHexString( read1.Content, ' ' ) );

				// 第二次数据交互
				OperateResult<byte[]> read2 = plc.ReadFromCoreServer( GetExecuteConfirm( station ) );
				if (!read2.IsSuccess) return read2;

				// 错误码判断
				OperateResult check = CheckResponse( read2.Content );
				if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( check );

				// 数据提取
				return SiemensS7Helper.AnalysisReadByte( read2.Content );
			}
		}

		/// <summary>
		/// 从西门子的PLC中读取数据信息，地址为"M100","AI100","I0","Q0","V100","S100"等<br />
		/// Read data information from Siemens PLC with addresses "M100", "AI100", "I0", "Q0", "V100", "S100", etc.
		/// </summary>
		/// <param name="plc">PLC的通信对象</param>
		/// <param name="address">西门子的地址数据信息</param>
		/// <param name="length">数据长度</param>
		/// <param name="station">当前的站号信息</param>
		/// <param name="communicationLock">当前的同通信锁</param>
		/// <returns>带返回结果的结果对象</returns>
		public static OperateResult<byte[]> Read( IReadWriteDevice plc, string address, ushort length, byte station, object communicationLock )
		{
			byte stat = (byte)HslHelper.ExtractParameter( ref address, "s", station );

			OperateResult<S7AddressData> analysis = AnalysisAddress( address );
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( analysis );

			return Read( plc, analysis.Content, length, stat, communicationLock );
		}

		/// <summary>
		/// 从西门子的PLC中读取bool数据信息，地址为"M100.0","AI100.1","I0.3","Q0.6","V100.4","S100"等<br />
		/// Read bool data information from Siemens PLC, the addresses are "M100.0", "AI100.1", "I0.3", "Q0.6", "V100.4", "S100", etc.
		/// </summary>
		/// <param name="plc">PLC的通信对象</param>
		/// <param name="address">西门子的地址数据信息</param>
		/// <param name="station">当前的站号信息</param>
		/// <param name="communicationLock">当前的同通信锁</param>
		/// <returns>带返回结果的结果对象</returns>
		public static OperateResult<bool> ReadBool( IReadWriteDevice plc, string address, byte station, object communicationLock )
		{
			byte stat = (byte)HslHelper.ExtractParameter( ref address, "s", station );

			// 解析指令
			OperateResult<byte[]> command = BuildReadCommand( stat, address, 1, true );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<bool>( command );

			lock (communicationLock)
			{
				// 第一次数据交互
				OperateResult<byte[]> read1 = plc.ReadFromCoreServer( command.Content );
				if (!read1.IsSuccess) return OperateResult.CreateFailedResult<bool>( read1 );

				// 验证
				if (read1.Content[0] != 0xE5) return new OperateResult<bool>( "PLC Receive Check Failed:" + BasicFramework.SoftBasic.ByteToHexString( read1.Content, ' ' ) );

				// 第二次数据交互
				OperateResult<byte[]> read2 = plc.ReadFromCoreServer( GetExecuteConfirm( stat ) );
				if (!read2.IsSuccess) return OperateResult.CreateFailedResult<bool>( read2 );

				// 错误码判断
				OperateResult check = CheckResponse( read2.Content );
				if (!check.IsSuccess) return OperateResult.CreateFailedResult<bool>( check );

				// 数据提取
				OperateResult<byte[]> extra = SiemensS7Helper.AnalysisReadBit( read2.Content );
				if (!extra.IsSuccess) return OperateResult.CreateFailedResult<bool>( extra );

				return OperateResult.CreateSuccessResult( extra.Content.ToBoolArray( )[0] );
			}
		}

		/// <inheritdoc cref="ReadBool(IReadWriteDevice, string, byte, object)"/>
		public static OperateResult<bool[]> ReadBool( IReadWriteDevice plc, string address, ushort length, byte station, object communicationLock )
		{
			byte stat = (byte)HslHelper.ExtractParameter( ref address, "s", station );

			OperateResult<S7AddressData> analysis = AnalysisAddress( address );
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( analysis );

			HslHelper.CalculateStartBitIndexAndLength( analysis.Content.AddressStart, length, out int newStart, out ushort byteLength, out int offset );
			analysis.Content.AddressStart = newStart;
			analysis.Content.Length = byteLength;

			OperateResult<byte[]> read = Read( plc, analysis.Content, analysis.Content.Length, stat, communicationLock );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( read );

			return OperateResult.CreateSuccessResult( read.Content.ToBoolArray( ).SelectMiddle( offset, length ) );
		}

		/// <summary>
		/// 将字节数据写入到西门子PLC中，地址为"M100.0","AI100.1","I0.3","Q0.6","V100.4","S100"等<br />
		/// Write byte data to Siemens PLC with addresses "M100.0", "AI100.1", "I0.3", "Q0.6", "V100.4", "S100", etc.
		/// </summary>
		/// <param name="plc">PLC的通信对象</param>
		/// <param name="address">西门子的地址数据信息</param>
		/// <param name="value">数据长度</param>
		/// <param name="station">当前的站号信息</param>
		/// <param name="communicationLock">当前的同通信锁</param>
		/// <returns>带返回结果的结果对象</returns>
		public static OperateResult Write( IReadWriteDevice plc, string address, byte[] value, byte station, object communicationLock )
		{
			byte stat = (byte)HslHelper.ExtractParameter( ref address, "s", station );

			// 解析指令
			OperateResult<byte[]> command = BuildWriteCommand( stat, address, value );
			if (!command.IsSuccess) return command;

			lock (communicationLock)
			{
				// 第一次数据交互
				OperateResult<byte[]> read1 = plc.ReadFromCoreServer( command.Content );
				if (!read1.IsSuccess) return read1;

				// 验证
				if (read1.Content[0] != 0xE5) return new OperateResult<byte[]>( "PLC Receive Check Failed:" + read1.Content[0] );

				// 第二次数据交互
				OperateResult<byte[]> read2 = plc.ReadFromCoreServer( GetExecuteConfirm( stat ) );
				if (!read2.IsSuccess) return read2;

				// 错误码判断
				OperateResult check = CheckResponse( read2.Content );
				if (!check.IsSuccess) return check;

				// 数据提取
				return OperateResult.CreateSuccessResult( );
			}
		}

		/// <summary>
		/// 将bool数据写入到西门子PLC中，地址为"M100.0","AI100.1","I0.3","Q0.6","V100.4","S100"等<br />
		/// Write the bool data to Siemens PLC with the addresses "M100.0", "AI100.1", "I0.3", "Q0.6", "V100.4", "S100", etc.
		/// </summary>
		/// <param name="plc">PLC的通信对象</param>
		/// <param name="address">西门子的地址数据信息</param>
		/// <param name="value">数据长度</param>
		/// <param name="station">当前的站号信息</param>
		/// <param name="communicationLock">当前的同通信锁</param>
		/// <returns>带返回结果的结果对象</returns>
		public static OperateResult Write( IReadWriteDevice plc, string address, bool[] value, byte station, object communicationLock )
		{
			byte stat = (byte)HslHelper.ExtractParameter( ref address, "s", station );

			// 解析指令
			OperateResult<byte[]> command = BuildWriteCommand( stat, address, value );
			if (!command.IsSuccess) return command;

			lock (communicationLock)
			{
				// 第一次数据交互
				OperateResult<byte[]> read1 = plc.ReadFromCoreServer( command.Content );
				if (!read1.IsSuccess) return read1;

				// 验证
				if (read1.Content[0] != 0xE5) return new OperateResult<byte[]>( "PLC Receive Check Failed:" + read1.Content[0] );

				// 第二次数据交互
				OperateResult<byte[]> read2 = plc.ReadFromCoreServer( GetExecuteConfirm( stat ) );
				if (!read2.IsSuccess) return read2;

				// 错误码判断
				OperateResult check = CheckResponse( read2.Content );
				if (!check.IsSuccess) return check;

				// 数据提取
				return OperateResult.CreateSuccessResult( );
			}
		}

		/// <summary>
		/// 启动西门子PLC为RUN模式，参数信息可以携带站号信息 "s=2;", 注意，分号是必须的。<br />
		/// Start Siemens PLC in RUN mode, parameter information can carry station number information "s=2;", note that the semicolon is required.
		/// </summary>
		/// <param name="plc">PLC的通信对象</param>
		/// <param name="parameter">额外的参数信息，例如可以携带站号信息 "s=2;", 注意，分号是必须的。</param>
		/// <param name="station">当前的站号信息</param>
		/// <param name="communicationLock">当前的同通信锁</param>
		/// <returns>是否启动成功</returns>
		public static OperateResult Start( IReadWriteDevice plc, string parameter, byte station, object communicationLock )
		{
			byte stat = (byte)HslHelper.ExtractParameter( ref parameter, "s", station );

			byte[] cmd = new byte[] { 0x68, 0x21, 0x21, 0x68, station, 0x00, 0x6C, 0x32, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x14, 0x00, 0x00, 0x28, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFD, 0x00, 0x00, 0x09, 0x50, 0x5F, 0x50, 0x52, 0x4F, 0x47, 0x52, 0x41, 0x4D, 0xAA, 0x16 };

			lock (communicationLock)
			{
				// 第一次数据交互
				OperateResult<byte[]> read1 = plc.ReadFromCoreServer( cmd );
				if (!read1.IsSuccess) return read1;

				// 验证
				if (read1.Content[0] != 0xE5) return new OperateResult<byte[]>( "PLC Receive Check Failed:" + read1.Content[0] );

				// 第二次数据交互
				OperateResult<byte[]> read2 = plc.ReadFromCoreServer( GetExecuteConfirm( stat ) );
				if (!read2.IsSuccess) return read2;

				// 数据提取
				return OperateResult.CreateSuccessResult( );
			}
		}

		/// <summary>
		/// 停止西门子PLC，切换为Stop模式，参数信息可以携带站号信息 "s=2;", 注意，分号是必须的。<br />
		/// Stop Siemens PLC and switch to Stop mode, parameter information can carry station number information "s=2;", note that the semicolon is required.
		/// </summary>
		/// <param name="plc">PLC的通信对象</param>
		/// <param name="parameter">额外的参数信息，例如可以携带站号信息 "s=2;", 注意，分号是必须的。</param>
		/// <param name="station">当前的站号信息</param>
		/// <param name="communicationLock">当前的同通信锁</param>
		/// <returns>是否停止成功</returns>
		public static OperateResult Stop( IReadWriteDevice plc, string parameter, byte station, object communicationLock )
		{
			byte stat = (byte)HslHelper.ExtractParameter( ref parameter, "s", station );

			byte[] cmd = new byte[] { 0x68, 0x1D, 0x1D, 0x68, station, 0x00, 0x6C, 0x32, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 0x29, 0x00, 0x00, 0x00, 0x00, 0x00, 0x09, 0x50, 0x5F, 0x50, 0x52, 0x4F, 0x47, 0x52, 0x41, 0x4D, 0xAA, 0x16 };

			lock (communicationLock)
			{
				// 第一次数据交互
				OperateResult<byte[]> read1 = plc.ReadFromCoreServer( cmd );
				if (!read1.IsSuccess) return read1;

				// 验证
				if (read1.Content[0] != 0xE5) return new OperateResult<byte[]>( "PLC Receive Check Failed:" + read1.Content[0] );

				// 第二次数据交互
				OperateResult<byte[]> read2 = plc.ReadFromCoreServer( GetExecuteConfirm( stat ) );
				if (!read2.IsSuccess) return read2;

				// 数据提取
				return OperateResult.CreateSuccessResult( );
			}
		}

		/// <summary>
		/// 读取西门子PLC的型号信息，参数信息可以携带站号信息 "s=2;", 注意，分号是必须的。<br />
		/// Read the model information of Siemens PLC, the parameter information can carry the station number information "s=2;", note that the semicolon is required.
		/// </summary>
		/// <param name="plc">PLC的通信对象</param>
		/// <param name="parameter">额外的参数信息，例如可以携带站号信息 "s=2;", 注意，分号是必须的。</param>
		/// <param name="station">当前的站号信息</param>
		/// <param name="communicationLock">当前的同通信锁</param>
		/// <returns>包含是否成功的结果对象</returns>
		public static OperateResult<string> ReadPlcType( IReadWriteDevice plc, string parameter, byte station, object communicationLock )
		{
			byte stat = (byte)HslHelper.ExtractParameter( ref parameter, "s", station );
			OperateResult<byte[]> command = BuildReadCommand( stat, "SYS0", 20, false );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<string>( command );

			lock (communicationLock)
			{
				// 第一次数据交互
				OperateResult<byte[]> read1 = plc.ReadFromCoreServer( command.Content );
				if (!read1.IsSuccess) return OperateResult.CreateFailedResult<string>( read1 );

				// 验证
				if (read1.Content[0] != 0xE5) return new OperateResult<string>( "PLC Receive Check Failed:" + read1.Content[0] );

				// 第二次数据交互
				OperateResult<byte[]> read2 = plc.ReadFromCoreServer( GetExecuteConfirm( stat ) );
				if (!read2.IsSuccess) return OperateResult.CreateFailedResult<string>( read2 );

				// 数据提取
				byte[] buffer = new byte[20];
				if (read2.Content[21] == 0xFF && read2.Content[22] == 0x04)
				{
					Array.Copy( read2.Content, 25, buffer, 0, 20 );
				}
				return OperateResult.CreateSuccessResult( Encoding.ASCII.GetString( buffer ) );
			}
		}
	}
}
