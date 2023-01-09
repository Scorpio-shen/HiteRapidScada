using HslCommunication.BasicFramework;
using HslCommunication.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if !NET20 && !NET35
using System.Threading.Tasks;
#endif

namespace HslCommunication.Profinet.Melsec.Helper
{
	/// <summary>
	/// 三菱编程口协议的辅助方法，定义了如何读写bool数据，以及读写原始字节的数据。<br />
	/// The auxiliary method of Mitsubishi programming port protocol defines how to read and write bool data and read and write raw byte data.
	/// </summary>
	public class MelsecFxSerialHelper
	{

		#region Static Method Helper

		/// <summary>
		/// 根据指定的地址及长度信息从三菱PLC中读取原始的字节数据，根据PLC中实际定义的规则，可以解析出任何类的数据信息<br />
		/// Read the original byte data from the Mitsubishi PLC according to the specified address and length information. 
		/// According to the rules actually defined in the PLC, any type of data information can be parsed
		/// </summary>
		/// <param name="plc">PLC通信对象</param>
		/// <param name="address">读取地址，，支持的类型参考文档说明</param>
		/// <param name="length">读取的数据长度</param>
		/// <param name="isNewVersion">是否是新版的串口访问类</param>
		/// <returns>带成功标志的结果数据对象</returns>
		/// <example>
		/// 假设起始地址为D100，D100存储了温度，100.6℃值为1006，D101存储了压力，1.23Mpa值为123，D102，D103存储了产量计数，读取如下：
		/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\MelsecFxSerial.cs" region="ReadExample2" title="Read示例" />
		/// 以下是读取不同类型数据的示例
		/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\MelsecFxSerial.cs" region="ReadExample1" title="Read示例" />
		/// </example>
		public static OperateResult<byte[]> Read( IReadWriteDevice plc, string address, ushort length, bool isNewVersion )
		{
			// 获取指令
			OperateResult<List<byte[]>> command = BuildReadWordCommand( address, length, isNewVersion );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( command );

			List<byte> array = new List<byte>( );
			for (int i = 0; i < command.Content.Count; i++)
			{
				// 核心交互
				OperateResult<byte[]> read = plc.ReadFromCoreServer( command.Content[i] );
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( read );

				// 反馈检查
				OperateResult ackResult = CheckPlcReadResponse( read.Content );
				if (!ackResult.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( ackResult );

				// 数据提炼
				var extra = ExtractActualData( read.Content );
				if (!extra.IsSuccess) return extra;

				array.AddRange( extra.Content );
			}
			return OperateResult.CreateSuccessResult( array.ToArray( ) );
		}

		/// <summary>
		/// 从三菱PLC中批量读取位软元件，返回读取结果，该读取地址最好从0，16，32...等开始读取，这样可以读取比较长的数据数组<br />
		/// Read bit devices in batches from Mitsubishi PLC and return the read results. 
		/// The read address should preferably be read from 0, 16, 32... etc., so that a relatively long data array can be read
		/// </summary>
		/// <param name="plc">PLC的通信对象</param>
		/// <param name="address">起始地址</param>
		/// <param name="length">读取的长度</param>
		/// <param name="isNewVersion">是否是新版的串口访问类</param>
		/// <returns>带成功标志的结果数据对象</returns>
		/// <example>
		///  <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\MelsecFxSerial.cs" region="ReadBool" title="Bool类型示例" />
		/// </example>
		public static OperateResult<bool[]> ReadBool( IReadWriteDevice plc, string address, ushort length, bool isNewVersion )
		{
			//获取指令
			OperateResult<List<byte[]>, int> command = BuildReadBoolCommand( address, length, isNewVersion );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( command );

			List<byte> array = new List<byte>( );
			for (int i = 0; i < command.Content1.Count; i++)
			{
				// 核心交互
				OperateResult<byte[]> read = plc.ReadFromCoreServer( command.Content1[i] );
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( read );

				// 反馈检查
				OperateResult ackResult = CheckPlcReadResponse( read.Content );
				if (!ackResult.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( ackResult );

				var extra = ExtractActualData( read.Content );
				if (!extra.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( extra );

				array.AddRange( extra.Content );
			}
			// 提取真实的数据
			return OperateResult.CreateSuccessResult( array.ToArray( ).ToBoolArray( ).SelectMiddle( command.Content2, length ) );
		}

		/// <summary>
		/// 根据指定的地址向PLC写入数据，数据格式为原始的字节类型<br />
		/// Write data to the PLC according to the specified address, the data format is the original byte type
		/// </summary>
		/// <param name="plc">PLC通信对象</param>
		/// <param name="address">初始地址，支持的类型参考文档说明</param>
		/// <param name="value">原始的字节数据</param>
		/// <param name="isNewVersion">是否是新版的串口访问类</param>
		/// <example>
		/// 假设起始地址为D100，D100存储了温度，100.6℃值为1006，D101存储了压力，1.23Mpa值为123，D102，D103存储了产量计数，写入如下：
		/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\MelsecFxSerial.cs" region="WriteExample2" title="Write示例" />
		/// 以下是读取不同类型数据的示例
		/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\MelsecFxSerial.cs" region="WriteExample1" title="Write示例" />
		/// </example>
		/// <returns>是否写入成功的结果对象</returns>
		public static OperateResult Write( IReadWriteDevice plc, string address, byte[] value, bool isNewVersion )
		{
			// 获取写入
			OperateResult<byte[]> command = BuildWriteWordCommand( address, value, isNewVersion );
			if (!command.IsSuccess) return command;

			// 核心交互
			OperateResult<byte[]> read = plc.ReadFromCoreServer( command.Content );
			if (!read.IsSuccess) return read;

			// 结果验证
			return CheckPlcWriteResponse( read.Content );
		}

		/// <summary>
		/// 强制写入位数据的通断，支持的类型参考文档说明<br />
		/// The on-off of the forced write bit data, please refer to the document description for the supported types
		/// </summary>
		/// <param name="plc">PLC通信对象</param>
		/// <param name="address">地址信息</param>
		/// <param name="value">是否为通</param>
		/// <returns>是否写入成功的结果对象</returns>
		public static OperateResult Write( IReadWriteDevice plc, string address, bool value )
		{
			// 先获取指令
			OperateResult<byte[]> command = BuildWriteBoolPacket( address, value );
			if (!command.IsSuccess) return command;

			// 和串口进行核心的数据交互
			OperateResult<byte[]> read = plc.ReadFromCoreServer( command.Content );
			if (!read.IsSuccess) return read;

			// 检查结果是否正确
			return CheckPlcWriteResponse( read.Content );
		}

		/// <summary>
		/// 激活PLC的接收状态，需要再和PLC交互之前进行调用，之后就需要再调用了。<br />
		/// </summary>
		/// <param name="plc">PLC通信对象</param>
		/// <returns>是否激活成功</returns>
		public static OperateResult ActivePlc( IReadWriteDevice plc )
		{
			OperateResult<byte[]> read1 = plc.ReadFromCoreServer( new byte[] { 0x05 } );
			if (!read1.IsSuccess) return read1;
			if (read1.Content[0] != 0x06) return new OperateResult( "Send 0x05, Check Receive 0x06 failed" );

			OperateResult<byte[]> read2 = plc.ReadFromCoreServer( new byte[] { 0x02, 0x30, 0x30, 0x45, 0x30, 0x32, 0x30, 0x32, 0x03, 0x36, 0x43 } );
			if (!read2.IsSuccess) return read2;

			return plc.ReadFromCoreServer( new byte[] { 0x02, 0x30, 0x30, 0x45, 0x30, 0x32, 0x30, 0x32, 0x03, 0x36, 0x43 } );
		}

#if !NET20 && !NET35
		/// <inheritdoc cref="Read(IReadWriteDevice, string, ushort, bool)"/>
		public static async Task<OperateResult<byte[]>> ReadAsync( IReadWriteDevice plc, string address, ushort length, bool isNewVersion )
		{
			// 获取指令
			OperateResult<List<byte[]>> command = BuildReadWordCommand( address, length, isNewVersion );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( command );

			List<byte> array = new List<byte>( );
			for (int i = 0; i < command.Content.Count; i++)
			{
				// 核心交互
				OperateResult<byte[]> read = await plc.ReadFromCoreServerAsync( command.Content[i] );
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( read );

				// 反馈检查
				OperateResult ackResult = CheckPlcReadResponse( read.Content );
				if (!ackResult.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( ackResult );

				// 数据提炼
				var extra = ExtractActualData( read.Content );
				if (!extra.IsSuccess) return extra;

				array.AddRange( extra.Content );
			}
			return OperateResult.CreateSuccessResult( array.ToArray( ) );
		}

		/// <inheritdoc cref="ReadBool(IReadWriteDevice, string, ushort, bool)"/>
		public static async Task<OperateResult<bool[]>> ReadBoolAsync( IReadWriteDevice plc, string address, ushort length, bool isNewVersion )
		{
			//获取指令
			OperateResult<List<byte[]>, int> command = BuildReadBoolCommand( address, length, isNewVersion );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( command );

			List<byte> array = new List<byte>( );
			for (int i = 0; i < command.Content1.Count; i++)
			{
				// 核心交互
				OperateResult<byte[]> read = await plc.ReadFromCoreServerAsync( command.Content1[i] );
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( read );

				// 反馈检查
				OperateResult ackResult = CheckPlcReadResponse( read.Content );
				if (!ackResult.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( ackResult );

				var extra = ExtractActualData( read.Content );
				if (!extra.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( extra );

				array.AddRange( extra.Content );
			}
			// 提取真实的数据
			return OperateResult.CreateSuccessResult( array.ToArray( ).ToBoolArray( ).SelectMiddle( command.Content2, length ) );
		}

		/// <inheritdoc cref="Write(IReadWriteDevice, string, byte[], bool)"/>
		public static async Task<OperateResult> WriteAsync( IReadWriteDevice plc, string address, byte[] value, bool isNewVersion )
		{
			// 获取写入
			OperateResult<byte[]> command = BuildWriteWordCommand( address, value, isNewVersion );
			if (!command.IsSuccess) return command;

			// 核心交互
			OperateResult<byte[]> read = await plc.ReadFromCoreServerAsync( command.Content );
			if (!read.IsSuccess) return read;

			// 结果验证
			return CheckPlcWriteResponse( read.Content );
		}
		/// <inheritdoc cref="Write(IReadWriteDevice, string, bool)"/>
		public static async Task<OperateResult> WriteAsync( IReadWriteDevice plc, string address, bool value )
		{
			// 先获取指令
			OperateResult<byte[]> command = BuildWriteBoolPacket( address, value );
			if (!command.IsSuccess) return command;

			// 和串口进行核心的数据交互
			OperateResult<byte[]> read = await plc.ReadFromCoreServerAsync( command.Content );
			if (!read.IsSuccess) return read;

			// 检查结果是否正确
			return CheckPlcWriteResponse( read.Content );
		}

		/// <inheritdoc cref="ActivePlc(IReadWriteDevice)"/>
		public async static Task<OperateResult> ActivePlcAsync( IReadWriteDevice plc )
		{
			OperateResult<byte[]> read1 = await plc.ReadFromCoreServerAsync( new byte[] { 0x05 } );
			if (!read1.IsSuccess) return read1;
			if (read1.Content[0] != 0x06) return new OperateResult( "Send 0x05, Check Receive 0x06 failed" );

			OperateResult<byte[]> read2 = await plc.ReadFromCoreServerAsync( new byte[] { 0x02, 0x30, 0x30, 0x45, 0x30, 0x32, 0x30, 0x32, 0x03, 0x36, 0x43 } );
			if (!read2.IsSuccess) return read2;

			return plc.ReadFromCoreServer( new byte[] { 0x02, 0x30, 0x30, 0x45, 0x30, 0x32, 0x30, 0x32, 0x03, 0x36, 0x43 } );
		}
#endif
		/// <summary>
		/// 检查PLC返回的读取数据是否是正常的
		/// </summary>
		/// <param name="ack">Plc反馈的数据信息</param>
		/// <returns>检查结果</returns>
		public static OperateResult CheckPlcReadResponse( byte[] ack )
		{
			if (ack.Length == 0) return new OperateResult( StringResources.Language.MelsecFxReceiveZero );
			if (ack[0] == 0x15) return new OperateResult( StringResources.Language.MelsecFxAckNagative + " Actual: " + SoftBasic.ByteToHexString( ack, ' ' ) );
			if (ack[0] != 0x02) return new OperateResult( StringResources.Language.MelsecFxAckWrong + ack[0] + " Actual: " + SoftBasic.ByteToHexString( ack, ' ' ) );

			try
			{
				if (!MelsecHelper.CheckCRC( ack )) return new OperateResult( StringResources.Language.MelsecFxCrcCheckFailed + " Actual: " + SoftBasic.ByteToHexString( ack, ' ' ) );
			}
			catch (Exception ex)
			{
				return new OperateResult( StringResources.Language.MelsecFxCrcCheckFailed + ex.Message + Environment.NewLine + "Actual: " + SoftBasic.ByteToHexString( ack, ' ' ) );
			}

			return OperateResult.CreateSuccessResult( );
		}

		/// <summary>
		/// 检查PLC返回的写入的数据是否是正常的
		/// </summary>
		/// <param name="ack">Plc反馈的数据信息</param>
		/// <returns>检查结果</returns>
		public static OperateResult CheckPlcWriteResponse( byte[] ack )
		{
			if (ack.Length == 0) return new OperateResult( StringResources.Language.MelsecFxReceiveZero );
			if (ack[0] == 0x15) return new OperateResult( StringResources.Language.MelsecFxAckNagative + " Actual: " + SoftBasic.ByteToHexString( ack, ' ' ) );
			if (ack[0] != 0x06) return new OperateResult( StringResources.Language.MelsecFxAckWrong + ack[0] + " Actual: " + SoftBasic.ByteToHexString( ack, ' ' ) );

			return OperateResult.CreateSuccessResult( );
		}

		/// <summary>
		/// 生成位写入的数据报文信息，该报文可直接用于发送串口给PLC
		/// </summary>
		/// <param name="address">地址信息，每个地址存在一定的范围，需要谨慎传入数据。举例：M10,S10,X5,Y10,C10,T10</param>
		/// <param name="value"><c>True</c>或是<c>False</c></param>
		/// <returns>带报文信息的结果对象</returns>
		public static OperateResult<byte[]> BuildWriteBoolPacket( string address, bool value )
		{
			var analysis = FxAnalysisAddress( address );
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( analysis );

			// 此处使用的是一个特殊的写入方式，
			// 二次运算起始地址偏移量，根据类型的不同，地址的计算方式不同
			ushort startAddress = analysis.Content2;
			if (analysis.Content1 == MelsecMcDataType.M)
			{
				if (startAddress >= 8000) startAddress = (ushort)(startAddress - 8000 + 0x0F00);
				else startAddress = (ushort)(startAddress + 0x0800);
			}
			else if (analysis.Content1 == MelsecMcDataType.S)  startAddress = (ushort)(startAddress + 0x0000);
			else if (analysis.Content1 == MelsecMcDataType.X)  startAddress = (ushort)(startAddress + 0x0400);
			else if (analysis.Content1 == MelsecMcDataType.Y)  startAddress = (ushort)(startAddress + 0x0500);
			else if (analysis.Content1 == MelsecMcDataType.CS) startAddress = (ushort)(startAddress + 0x01C0);
			else if (analysis.Content1 == MelsecMcDataType.CC) startAddress = (ushort)(startAddress + 0x03C0);
			else if (analysis.Content1 == MelsecMcDataType.CN) startAddress = (ushort)(startAddress + 0x0E00);
			else if (analysis.Content1 == MelsecMcDataType.TS) startAddress = (ushort)(startAddress + 0x00C0);
			else if (analysis.Content1 == MelsecMcDataType.TC) startAddress = (ushort)(startAddress + 0x02C0);
			else if (analysis.Content1 == MelsecMcDataType.TN) startAddress = (ushort)(startAddress + 0x0600);
			else return new OperateResult<byte[]>( StringResources.Language.MelsecCurrentTypeNotSupportedBitOperate );


			byte[] _PLCCommand = new byte[9];
			_PLCCommand[0] = 0x02;                                                       // STX
			_PLCCommand[1] = value ? (byte)0x37 : (byte)0x38;                            // 强制ON 或是 OFF
			_PLCCommand[2] = SoftBasic.BuildAsciiBytesFrom( startAddress )[2];           // 偏移地址
			_PLCCommand[3] = SoftBasic.BuildAsciiBytesFrom( startAddress )[3];
			_PLCCommand[4] = SoftBasic.BuildAsciiBytesFrom( startAddress )[0];
			_PLCCommand[5] = SoftBasic.BuildAsciiBytesFrom( startAddress )[1];
			_PLCCommand[6] = 0x03;                                                       // ETX
			MelsecHelper.FxCalculateCRC( _PLCCommand ).CopyTo( _PLCCommand, 7 );         // CRC

			return OperateResult.CreateSuccessResult( _PLCCommand );
		}

		/// <summary>
		/// 根据类型地址长度确认需要读取的指令头
		/// </summary>
		/// <param name="address">起始地址</param>
		/// <param name="length">长度</param>
		/// <param name="isNewVersion">是否是新版的串口访问类</param>
		/// <returns>带有成功标志的指令数据</returns>
		public static OperateResult<List<byte[]>> BuildReadWordCommand( string address, ushort length, bool isNewVersion )
		{
			var addressResult = FxCalculateWordStartAddress( address, isNewVersion );
			if (!addressResult.IsSuccess) return OperateResult.CreateFailedResult<List<byte[]>>( addressResult );

			length = (ushort)(length * 2);
			ushort startAddress = addressResult.Content;
			int[] splits = SoftBasic.SplitIntegerToArray( length, 254 );
			List<byte[]> array = new List<byte[]>( );

			for (int i = 0; i < splits.Length; i++)
			{
				if (isNewVersion)
				{
					byte[] _PLCCommand = new byte[13];
					_PLCCommand[0] = AsciiControl.STX;                                      //报头
					_PLCCommand[1] = 0x45;                                                  //指令形式
					_PLCCommand[2] = 0x30;                                                  //Read
					_PLCCommand[3] = 0x30;                                                  //Read
					_PLCCommand[4] = SoftBasic.BuildAsciiBytesFrom( startAddress )[0];      //起始地址
					_PLCCommand[5] = SoftBasic.BuildAsciiBytesFrom( startAddress )[1];      //起始地址
					_PLCCommand[6] = SoftBasic.BuildAsciiBytesFrom( startAddress )[2];      //起始地址
					_PLCCommand[7] = SoftBasic.BuildAsciiBytesFrom( startAddress )[3];      //起始地址
					_PLCCommand[8] = SoftBasic.BuildAsciiBytesFrom( (byte)splits[i] )[0];   // 读取长度
					_PLCCommand[9] = SoftBasic.BuildAsciiBytesFrom( (byte)splits[i] )[1];
					_PLCCommand[10] = AsciiControl.ETX;
					MelsecHelper.FxCalculateCRC( _PLCCommand ).CopyTo( _PLCCommand, 11 );   // CRC


					array.Add( _PLCCommand );
					startAddress = (ushort)( startAddress + splits[i]);
				}
				else
				{
					byte[] _PLCCommand = new byte[11];
					_PLCCommand[0] = AsciiControl.STX;                                        // STX
					_PLCCommand[1] = 0x30;                                                    // Read
					_PLCCommand[2] = SoftBasic.BuildAsciiBytesFrom( startAddress )[0];        // 偏移地址
					_PLCCommand[3] = SoftBasic.BuildAsciiBytesFrom( startAddress )[1];
					_PLCCommand[4] = SoftBasic.BuildAsciiBytesFrom( startAddress )[2];
					_PLCCommand[5] = SoftBasic.BuildAsciiBytesFrom( startAddress )[3];
					_PLCCommand[6] = SoftBasic.BuildAsciiBytesFrom( (byte)splits[i] )[0];     // 读取长度
					_PLCCommand[7] = SoftBasic.BuildAsciiBytesFrom( (byte)splits[i] )[1];
					_PLCCommand[8] = AsciiControl.ETX;                                        // ETX
					MelsecHelper.FxCalculateCRC( _PLCCommand ).CopyTo( _PLCCommand, 9 );      // CRC

					array.Add( _PLCCommand );
					startAddress = (ushort)(startAddress + splits[i]);
				}
			}

			return OperateResult.CreateSuccessResult( array );
		}

		/// <summary>
		/// 根据类型地址长度确认需要读取的指令头
		/// </summary>
		/// <param name="address">起始地址</param>
		/// <param name="length">bool数组长度</param>
		/// <param name="isNewVersion">是否是新版的串口访问类</param>
		/// <returns>带有成功标志的指令数据</returns>
		public static OperateResult<List<byte[]>, int> BuildReadBoolCommand( string address, ushort length, bool isNewVersion )
		{
			var addressResult = FxCalculateBoolStartAddress( address, isNewVersion );
			if (!addressResult.IsSuccess) return OperateResult.CreateFailedResult<List<byte[]>, int>( addressResult );

			// 计算下实际需要读取的数据长度
			ushort length2 = (ushort)HslHelper.CalculateOccupyLength( addressResult.Content2, length );
			ushort startAddress = addressResult.Content1;

			int[] splits = SoftBasic.SplitIntegerToArray( length2, 254 );
			List<byte[]> array = new List<byte[]>( );

			for (int i = 0; i < splits.Length; i++)
			{
				if (isNewVersion)
				{
					byte[] _PLCCommand = new byte[13];
					_PLCCommand[0] = AsciiControl.STX;                                      //报头
					_PLCCommand[1] = 0x45;                                                  //指令形式
					_PLCCommand[2] = 0x30;                                                  //Read
					_PLCCommand[3] = 0x30;                                                  //Read
					_PLCCommand[4] = SoftBasic.BuildAsciiBytesFrom( startAddress )[0];      //起始地址
					_PLCCommand[5] = SoftBasic.BuildAsciiBytesFrom( startAddress )[1];      //起始地址
					_PLCCommand[6] = SoftBasic.BuildAsciiBytesFrom( startAddress )[2];      //起始地址
					_PLCCommand[7] = SoftBasic.BuildAsciiBytesFrom( startAddress )[3];      //起始地址
					_PLCCommand[8] = SoftBasic.BuildAsciiBytesFrom( (byte)splits[i] )[0];   // 读取长度
					_PLCCommand[9] = SoftBasic.BuildAsciiBytesFrom( (byte)splits[i] )[1];
					_PLCCommand[10] = AsciiControl.ETX;
					MelsecHelper.FxCalculateCRC( _PLCCommand ).CopyTo( _PLCCommand, 11 );   // CRC

					array.Add( _PLCCommand );
				}
				else
				{
					byte[] _PLCCommand = new byte[11];
					_PLCCommand[0] = AsciiControl.STX;                                        // STX
					_PLCCommand[1] = 0x30;                                                    // Read
					_PLCCommand[2] = SoftBasic.BuildAsciiBytesFrom( startAddress )[0];        // 偏移地址
					_PLCCommand[3] = SoftBasic.BuildAsciiBytesFrom( startAddress )[1];
					_PLCCommand[4] = SoftBasic.BuildAsciiBytesFrom( startAddress )[2];
					_PLCCommand[5] = SoftBasic.BuildAsciiBytesFrom( startAddress )[3];
					_PLCCommand[6] = SoftBasic.BuildAsciiBytesFrom( (byte)splits[i] )[0];       // 读取长度
					_PLCCommand[7] = SoftBasic.BuildAsciiBytesFrom( (byte)splits[i] )[1];
					_PLCCommand[8] = AsciiControl.ETX;                                        // ETX
					MelsecHelper.FxCalculateCRC( _PLCCommand ).CopyTo( _PLCCommand, 9 );      // CRC

					array.Add( _PLCCommand );
				}
				startAddress = (ushort)(startAddress + splits[i]);
			}
			return OperateResult.CreateSuccessResult( array, (int)addressResult.Content3 );                // Return
		}

		/// <summary>
		/// 根据类型地址以及需要写入的数据来生成指令头
		/// </summary>
		/// <param name="address">起始地址</param>
		/// <param name="value">实际的数据信息</param>
		/// <param name="isNewVersion">是否是新版的串口访问类</param>
		/// <returns>带有成功标志的指令数据</returns>
		public static OperateResult<byte[]> BuildWriteWordCommand( string address, byte[] value, bool isNewVersion )
		{
			var addressResult = FxCalculateWordStartAddress( address, isNewVersion );
			if (!addressResult.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( addressResult );

			// 字节数据转换成ASCII格式
			if (value != null) value = SoftBasic.BuildAsciiBytesFrom( value );

			ushort startAddress = addressResult.Content;

			if (isNewVersion)
			{
				byte[] _PLCCommand = new byte[13 + value.Length];
				_PLCCommand[0] = AsciiControl.STX;                                                        //报头
				_PLCCommand[1] = 0x45;                                                                    //指令形式
				_PLCCommand[2] = 0x31;                                                                    //Write
				_PLCCommand[3] = 0x30;                                                                    //Write
				_PLCCommand[4] = SoftBasic.BuildAsciiBytesFrom( startAddress )[0];                        // Offect Address
				_PLCCommand[5] = SoftBasic.BuildAsciiBytesFrom( startAddress )[1];
				_PLCCommand[6] = SoftBasic.BuildAsciiBytesFrom( startAddress )[2];
				_PLCCommand[7] = SoftBasic.BuildAsciiBytesFrom( startAddress )[3];
				_PLCCommand[8] = SoftBasic.BuildAsciiBytesFrom( (byte)(value.Length / 2) )[0];            // Read Length
				_PLCCommand[9] = SoftBasic.BuildAsciiBytesFrom( (byte)(value.Length / 2) )[1];
				Array.Copy( value, 0, _PLCCommand, 10, value.Length );
				_PLCCommand[_PLCCommand.Length - 3] = AsciiControl.ETX;                                   // ETX
				MelsecHelper.FxCalculateCRC( _PLCCommand ).CopyTo( _PLCCommand, _PLCCommand.Length - 2 ); // CRC

				return OperateResult.CreateSuccessResult( _PLCCommand );
			}
			else
			{
				byte[] _PLCCommand = new byte[11 + value.Length];
				_PLCCommand[0] = AsciiControl.STX;              //报头
				_PLCCommand[1] = 0x31;                          //Write
				_PLCCommand[2] = SoftBasic.BuildAsciiBytesFrom( startAddress )[0];                        // Offect Address
				_PLCCommand[3] = SoftBasic.BuildAsciiBytesFrom( startAddress )[1];
				_PLCCommand[4] = SoftBasic.BuildAsciiBytesFrom( startAddress )[2];
				_PLCCommand[5] = SoftBasic.BuildAsciiBytesFrom( startAddress )[3];
				_PLCCommand[6] = SoftBasic.BuildAsciiBytesFrom( (byte)(value.Length / 2) )[0];            // Read Length
				_PLCCommand[7] = SoftBasic.BuildAsciiBytesFrom( (byte)(value.Length / 2) )[1];
				Array.Copy( value, 0, _PLCCommand, 8, value.Length );
				_PLCCommand[_PLCCommand.Length - 3] = AsciiControl.ETX;                                   // ETX
				MelsecHelper.FxCalculateCRC( _PLCCommand ).CopyTo( _PLCCommand, _PLCCommand.Length - 2 ); // CRC

				return OperateResult.CreateSuccessResult( _PLCCommand );
			}
		}


		/// <summary>
		/// 从PLC反馈的数据进行提炼操作
		/// </summary>
		/// <param name="response">PLC反馈的真实数据</param>
		/// <returns>数据提炼后的真实数据</returns>
		public static OperateResult<byte[]> ExtractActualData( byte[] response )
		{
			try
			{
				byte[] data = new byte[(response.Length - 4) / 2];
				for (int i = 0; i < data.Length; i++)
				{
					byte[] buffer = new byte[2];
					buffer[0] = response[i * 2 + 1];
					buffer[1] = response[i * 2 + 2];

					data[i] = Convert.ToByte( Encoding.ASCII.GetString( buffer ), 16 );
				}

				return OperateResult.CreateSuccessResult( data );
			}
			catch (Exception ex)
			{
				return new OperateResult<byte[]>( )
				{
					Message = "Extract Msg：" + ex.Message + Environment.NewLine +
					"Data: " + BasicFramework.SoftBasic.ByteToHexString( response )
				};
			}
		}


		/// <summary>
		/// 从PLC反馈的数据进行提炼bool数组操作
		/// </summary>
		/// <param name="response">PLC反馈的真实数据</param>
		/// <param name="start">起始提取的点信息</param>
		/// <param name="length">bool数组的长度</param>
		/// <returns>数据提炼后的真实数据</returns>
		public static OperateResult<bool[]> ExtractActualBoolData( byte[] response, int start, int length )
		{
			OperateResult<byte[]> extraResult = ExtractActualData( response );
			if (!extraResult.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( extraResult );

			// 转化bool数组
			try
			{
				bool[] data = new bool[length];
				bool[] array = SoftBasic.ByteToBoolArray( extraResult.Content, extraResult.Content.Length * 8 );
				for (int i = 0; i < length; i++)
				{
					data[i] = array[i + start];
				}

				return OperateResult.CreateSuccessResult( data );
			}
			catch (Exception ex)
			{
				return new OperateResult<bool[]>( )
				{
					Message = "Extract Msg：" + ex.Message + Environment.NewLine +
					"Data: " + SoftBasic.ByteToHexString( response )
				};
			}
		}

		/// <summary>
		/// 解析数据地址成不同的三菱地址类型
		/// </summary>
		/// <param name="address">数据地址</param>
		/// <returns>地址结果对象</returns>
		public static OperateResult<MelsecMcDataType, ushort> FxAnalysisAddress( string address )
		{
			var result = new OperateResult<MelsecMcDataType, ushort>( );
			try
			{
				switch (address[0])
				{
					case 'M':
					case 'm':
						{
							result.Content1 = MelsecMcDataType.M;
							result.Content2 = Convert.ToUInt16( address.Substring( 1 ), MelsecMcDataType.M.FromBase );
							break;
						}
					case 'X':
					case 'x':
						{
							result.Content1 = MelsecMcDataType.X;
							result.Content2 = Convert.ToUInt16( address.Substring( 1 ), 8 );
							break;
						}
					case 'Y':
					case 'y':
						{
							result.Content1 = MelsecMcDataType.Y;
							result.Content2 = Convert.ToUInt16( address.Substring( 1 ), 8 );
							break;
						}
					case 'D':
					case 'd':
						{
							result.Content1 = MelsecMcDataType.D;
							result.Content2 = Convert.ToUInt16( address.Substring( 1 ), MelsecMcDataType.D.FromBase );
							break;
						}
					case 'S':
					case 's':
						{
							result.Content1 = MelsecMcDataType.S;
							result.Content2 = Convert.ToUInt16( address.Substring( 1 ), MelsecMcDataType.S.FromBase );
							break;
						}
					case 'T':
					case 't':
						{
							if (address[1] == 'N' || address[1] == 'n')
							{
								result.Content1 = MelsecMcDataType.TN;
								result.Content2 = Convert.ToUInt16( address.Substring( 2 ), MelsecMcDataType.TN.FromBase );
								break;
							}
							else if (address[1] == 'S' || address[1] == 's')
							{
								result.Content1 = MelsecMcDataType.TS;
								result.Content2 = Convert.ToUInt16( address.Substring( 2 ), MelsecMcDataType.TS.FromBase );
								break;
							}
							else if (address[1] == 'C' || address[1] == 'c')
							{
								result.Content1 = MelsecMcDataType.TC;
								result.Content2 = Convert.ToUInt16( address.Substring( 2 ), MelsecMcDataType.TC.FromBase );
								break;
							}
							else
							{
								throw new Exception( StringResources.Language.NotSupportedDataType );
							}
						}
					case 'C':
					case 'c':
						{
							if (address[1] == 'N' || address[1] == 'n')
							{
								result.Content1 = MelsecMcDataType.CN;
								result.Content2 = Convert.ToUInt16( address.Substring( 2 ), MelsecMcDataType.CN.FromBase );
								break;
							}
							else if (address[1] == 'S' || address[1] == 's')
							{
								result.Content1 = MelsecMcDataType.CS;
								result.Content2 = Convert.ToUInt16( address.Substring( 2 ), MelsecMcDataType.CS.FromBase );
								break;
							}
							else if (address[1] == 'C' || address[1] == 'c')
							{
								result.Content1 = MelsecMcDataType.CC;
								result.Content2 = Convert.ToUInt16( address.Substring( 2 ), MelsecMcDataType.CC.FromBase );
								break;
							}
							else
							{
								throw new Exception( StringResources.Language.NotSupportedDataType );
							}
						}
					default: throw new Exception( StringResources.Language.NotSupportedDataType );
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
		/// 返回读取的地址及长度信息
		/// </summary>
		/// <param name="address">读取的地址信息</param>
		/// <param name="isNewVersion">是否是新版的串口访问类</param>
		/// <returns>带起始地址的结果对象</returns>
		private static OperateResult<ushort> FxCalculateWordStartAddress( string address, bool isNewVersion )
		{
			// 初步解析，失败就返回
			var analysis = FxAnalysisAddress( address );
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<ushort>( analysis );

			// 二次解析
			ushort startAddress = analysis.Content2;
			if (analysis.Content1 == MelsecMcDataType.D)
			{
				if (startAddress >= 8000) startAddress = (ushort)((startAddress - 8000) * 2 + (isNewVersion ? 0x8000 : 0x0E00));
				else startAddress = isNewVersion ? (ushort)(startAddress * 2 + 0x4000) : (ushort)(startAddress * 2 + 0x1000);
			}
			else if (analysis.Content1 == MelsecMcDataType.CN)
			{
				if (startAddress >= 200) startAddress = (ushort)((startAddress - 200) * 4 + 0x0C00);
				else startAddress = (ushort)(startAddress * 2 + 0x0A00);
			}
			else if (analysis.Content1 == MelsecMcDataType.TN)
			{
				if (isNewVersion)
				{
					startAddress = (ushort)(startAddress * 2 + 0x1000);
				}
				else
				{
					startAddress = (ushort)(startAddress * 2 + 0x0800);
				}
			}
			else return new OperateResult<ushort>( StringResources.Language.MelsecCurrentTypeNotSupportedWordOperate );

			return OperateResult.CreateSuccessResult( startAddress );
		}

		/// <summary>
		/// 返回读取的实际的字节地址，相对位置，以及当前的位偏置信息
		/// </summary><param name="address">读取的地址信息</param>
		/// <param name="isNewVersion">是否是新版的串口访问类</param>
		/// <returns>带起始地址的结果对象</returns>
		private static OperateResult<ushort, ushort, ushort> FxCalculateBoolStartAddress( string address, bool isNewVersion )
		{
			// 初步解析
			var analysis = FxAnalysisAddress( address );
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<ushort, ushort, ushort>( analysis );

			// 二次解析
			ushort startAddress = analysis.Content2;
			if (analysis.Content1 == MelsecMcDataType.M)
			{
				if (isNewVersion)
				{
					if (startAddress >= 8000) startAddress = (ushort)((startAddress - 8000) / 8 + 0x8C00);
					else startAddress = (ushort)(startAddress / 8 + 0x8800);
				}
				else
				{
					if (startAddress >= 8000) startAddress = (ushort)((startAddress - 8000) / 8 + 0x01E0);
					else startAddress = (ushort)(startAddress / 8 + 0x0100);
				}
			}
			else if (analysis.Content1 == MelsecMcDataType.X) startAddress  = (ushort)(startAddress / 8 + (isNewVersion ? 0x8CA0 : 0x0080));
			else if (analysis.Content1 == MelsecMcDataType.Y) startAddress  = (ushort)(startAddress / 8 + (isNewVersion ? 0x8BC0 : 0x00A0));
			else if (analysis.Content1 == MelsecMcDataType.S) startAddress  = (ushort)(startAddress / 8 + (isNewVersion ? 0x8CE0 : 0x0000));
			else if (analysis.Content1 == MelsecMcDataType.CS) startAddress = (ushort)(startAddress / 8 + (isNewVersion ? 0x9340 : 0x01C0));
			else if (analysis.Content1 == MelsecMcDataType.CC) startAddress = (ushort)(startAddress / 8 + (isNewVersion ? 0x92E0 : 0x03C0));
			else if (analysis.Content1 == MelsecMcDataType.TS) startAddress = (ushort)(startAddress / 8 + (isNewVersion ? 0x9360 : 0x00C0));
			else if (analysis.Content1 == MelsecMcDataType.TC) startAddress = (ushort)(startAddress / 8 + (isNewVersion ? 0x9300 : 0x02C0));
			else return new OperateResult<ushort, ushort, ushort>( StringResources.Language.MelsecCurrentTypeNotSupportedBitOperate );

			return OperateResult.CreateSuccessResult( startAddress, analysis.Content2, (ushort)(analysis.Content2 % 8) );
		}

		#endregion
	}
}
