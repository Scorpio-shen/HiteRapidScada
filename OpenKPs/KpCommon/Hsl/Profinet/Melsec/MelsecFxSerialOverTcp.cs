using HslCommunication.BasicFramework;
using HslCommunication.Core;
using HslCommunication.Core.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HslCommunication.Reflection;
#if !NET35 && !NET20
using System.Threading.Tasks;
#endif

namespace HslCommunication.Profinet.Melsec
{
	/// <summary>
	/// 三菱串口协议的网络版
	/// </summary>
	/// <remarks>
	/// 字读写地址支持的列表如下：
	/// <list type="table">
	///   <listheader>
	///     <term>地址名称</term>
	///     <term>地址代号</term>
	///     <term>示例</term>
	///     <term>地址范围</term>
	///     <term>地址进制</term>
	///     <term>备注</term>
	///   </listheader>
	///   <item>
	///     <term>数据寄存器</term>
	///     <term>D</term>
	///     <term>D100,D200</term>
	///     <term>D0-D511,D8000-D8255</term>
	///     <term>10</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>定时器的值</term>
	///     <term>TN</term>
	///     <term>TN10,TN20</term>
	///     <term>TN0-TN255</term>
	///     <term>10</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>计数器的值</term>
	///     <term>CN</term>
	///     <term>CN10,CN20</term>
	///     <term>CN0-CN199,CN200-CN255</term>
	///     <term>10</term>
	///     <term></term>
	///   </item>
	/// </list>
	/// 位地址支持的列表如下：
	/// <list type="table">
	///   <listheader>
	///     <term>地址名称</term>
	///     <term>地址代号</term>
	///     <term>示例</term>
	///     <term>地址范围</term>
	///     <term>地址进制</term>
	///     <term>备注</term>
	///   </listheader>
	///   <item>
	///     <term>内部继电器</term>
	///     <term>M</term>
	///     <term>M100,M200</term>
	///     <term>M0-M1023,M8000-M8255</term>
	///     <term>10</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>输入继电器</term>
	///     <term>X</term>
	///     <term>X1,X20</term>
	///     <term>X0-X177</term>
	///     <term>8</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>输出继电器</term>
	///     <term>Y</term>
	///     <term>Y10,Y20</term>
	///     <term>Y0-Y177</term>
	///     <term>8</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>步进继电器</term>
	///     <term>S</term>
	///     <term>S100,S200</term>
	///     <term>S0-S999</term>
	///     <term>10</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>定时器触点</term>
	///     <term>TS</term>
	///     <term>TS10,TS20</term>
	///     <term>TS0-TS255</term>
	///     <term>10</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>定时器线圈</term>
	///     <term>TC</term>
	///     <term>TC10,TC20</term>
	///     <term>TC0-TC255</term>
	///     <term>10</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>计数器触点</term>
	///     <term>CS</term>
	///     <term>CS10,CS20</term>
	///     <term>CS0-CS255</term>
	///     <term>10</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>计数器线圈</term>
	///     <term>CC</term>
	///     <term>CC10,CC20</term>
	///     <term>CC0-CC255</term>
	///     <term>10</term>
	///     <term></term>
	///   </item>
	/// </list>
	/// </remarks>
	/// <example>
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\MelsecFxSerial.cs" region="Usage" title="简单的使用" />
	/// </example>
	public class MelsecFxSerialOverTcp : NetworkDeviceBase
	{
		#region Constructor

		/// <summary>
		/// 实例化网络版的三菱的串口协议的通讯对象<br />
		/// Instantiate the communication object of Mitsubishi's serial protocol on the network
		/// </summary>
		public MelsecFxSerialOverTcp( )
		{
			this.WordLength    = 1;
			this.ByteTransform = new RegularByteTransform( );
			this.IsNewVersion  = true;
			this.ByteTransform.IsStringReverseByteWord = true;
			this.SleepTime     = 20;
		}

		/// <summary>
		/// 指定ip地址及端口号来实例化三菱的串口协议的通讯对象<br />
		/// Specify the IP address and port number to instantiate the communication object of Mitsubishi's serial protocol
		/// </summary>
		/// <param name="ipAddress">Ip地址</param>
		/// <param name="port">端口号</param>
		public MelsecFxSerialOverTcp( string ipAddress, int port ) : this( )
		{
			this.IpAddress     = ipAddress;
			this.Port          = port;
		}

		#endregion

		/// <summary>
		/// 当前的编程口协议是否为新版，默认为新版，如果无法读取，切换旧版再次尝试<br />
		/// Whether the current programming port protocol is the new version, the default is the new version, 
		/// if it cannot be read, switch to the old version and try again
		/// </summary>
		public bool IsNewVersion { get; set; }

		#region Read Write Byte

		/// <inheritdoc/>
		/// <example>
		/// 假设起始地址为D100，D100存储了温度，100.6℃值为1006，D101存储了压力，1.23Mpa值为123，D102，D103存储了产量计数，读取如下：
		/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\MelsecFxSerial.cs" region="ReadExample2" title="Read示例" />
		/// 以下是读取不同类型数据的示例
		/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\MelsecFxSerial.cs" region="ReadExample1" title="Read示例" />
		/// </example>
		[HslMqttApi( "ReadByteArray", "" )]
		public override OperateResult<byte[]> Read( string address, ushort length ) => ReadHelper( address, length, ReadFromCoreServer, IsNewVersion );

		/// <inheritdoc/>
		/// <example>
		/// 假设起始地址为D100，D100存储了温度，100.6℃值为1006，D101存储了压力，1.23Mpa值为123，D102，D103存储了产量计数，写入如下：
		/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\MelsecFxSerial.cs" region="WriteExample2" title="Write示例" />
		/// 以下是读取不同类型数据的示例
		/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\MelsecFxSerial.cs" region="WriteExample1" title="Write示例" />
		/// </example>
		[HslMqttApi( "WriteByteArray", "" )]
		public override OperateResult Write( string address, byte[] value ) => WriteHelper( address, value, ReadFromCoreServer, IsNewVersion );

		#endregion

		#region Async Read Write Byte
#if !NET35 && !NET20
		/// <inheritdoc cref="Read(string, ushort)"/>
		public override async Task<OperateResult<byte[]>> ReadAsync( string address, ushort length )
		{
			// 获取指令
			OperateResult<byte[]> command = BuildReadWordCommand( address, length, IsNewVersion );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( command );

			// 核心交互
			OperateResult<byte[]> read = await ReadFromCoreServerAsync( command.Content );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( read );

			// 反馈检查
			OperateResult ackResult = CheckPlcReadResponse( read.Content );
			if (!ackResult.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( ackResult );

			// 数据提炼
			return ExtractActualData( read.Content );
		}

		/// <inheritdoc cref="Write(string, byte[])"/>
		public override async Task<OperateResult> WriteAsync( string address, byte[] value )
		{
			// 获取写入
			OperateResult<byte[]> command = BuildWriteWordCommand( address, value, IsNewVersion );
			if (!command.IsSuccess) return command;

			// 核心交互
			OperateResult<byte[]> read = await ReadFromCoreServerAsync( command.Content );
			if (!read.IsSuccess) return read;

			// 结果验证
			OperateResult checkResult = CheckPlcWriteResponse( read.Content );
			if (!checkResult.IsSuccess) return checkResult;

			return OperateResult.CreateSuccessResult( );
		}
#endif
		#endregion

		#region Read Write Bool

		/// <summary>
		/// 从三菱PLC中批量读取位软元件，返回读取结果，该读取地址最好从0，16，32...等开始读取，这样可以读取比较长得数据数组<br />
		/// Read bit devices from Mitsubishi PLC in batches and return the read result. The read address should preferably be read from 0, 16, 32, etc., so that you can read the longer data array.
		/// </summary>
		/// <param name="address">起始地址</param>
		/// <param name="length">读取的长度</param>
		/// <returns>带成功标志的结果数据对象</returns>
		/// <example>
		///  <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\MelsecFxSerial.cs" region="ReadBool" title="Bool类型示例" />
		/// </example>
		[HslMqttApi( "ReadBoolArray", "" )]
		public override OperateResult<bool[]> ReadBool( string address, ushort length ) => ReadBoolHelper( address, length, ReadFromCoreServer );

		/// <inheritdoc/>
		[HslMqttApi( "WriteBool", "" )]
		public override OperateResult Write( string address, bool value ) => WriteHelper(address, value, ReadFromCoreServer );

		#endregion

		#region Async Read Write Bool
#if !NET35 && !NET20
		/// <inheritdoc cref="ReadBool(string, ushort)"/>
		public override async Task<OperateResult<bool[]>> ReadBoolAsync( string address, ushort length )
		{
			//获取指令
			OperateResult<byte[], int> command = BuildReadBoolCommand( address, length );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( command );

			// 核心交互
			OperateResult<byte[]> read = await ReadFromCoreServerAsync( command.Content1 );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( read );

			// 反馈检查
			OperateResult ackResult = CheckPlcReadResponse( read.Content );
			if (!ackResult.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( ackResult );

			// 提取真实的数据
			return ExtractActualBoolData( read.Content, command.Content2, length );
		}

		/// <inheritdoc/>
		public override async Task<OperateResult> WriteAsync( string address, bool value )
		{
			OperateResult<byte[]> command = BuildWriteBoolPacket( address, value );
			if (!command.IsSuccess) return command;

			// 和串口进行核心的数据交互
			OperateResult<byte[]> read = await ReadFromCoreServerAsync( command.Content );
			if (!read.IsSuccess) return read;

			// 检查结果是否正确
			OperateResult checkResult = CheckPlcWriteResponse( read.Content );
			if (!checkResult.IsSuccess) return checkResult;

			return OperateResult.CreateSuccessResult( );
		}
#endif
		#endregion

		#region Object Override

		///<inheritdoc/>
		public override string ToString( ) => $"MelsecFxSerialOverTcp[{IpAddress}:{Port}]";

		#endregion

		#region Static Method Helper

		/// <summary>
		/// 从三菱PLC中读取想要的数据，返回读取结果
		/// </summary>
		/// <param name="address">读取地址，，支持的类型参考文档说明</param>
		/// <param name="length">读取的数据长度</param>
		/// <param name="readCore">指定的通道信息</param>
		/// <param name="isNewVersion">是否是新版的串口访问类</param>
		/// <returns>带成功标志的结果数据对象</returns>
		/// <example>
		/// 假设起始地址为D100，D100存储了温度，100.6℃值为1006，D101存储了压力，1.23Mpa值为123，D102，D103存储了产量计数，读取如下：
		/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\MelsecFxSerial.cs" region="ReadExample2" title="Read示例" />
		/// 以下是读取不同类型数据的示例
		/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\MelsecFxSerial.cs" region="ReadExample1" title="Read示例" />
		/// </example>
		public static OperateResult<byte[]> ReadHelper( string address, ushort length, Func<byte[],OperateResult<byte[]>> readCore, bool isNewVersion )
		{
			// 获取指令
			OperateResult<byte[]> command = BuildReadWordCommand( address, length, isNewVersion );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( command );

			// 核心交互
			OperateResult<byte[]> read = readCore( command.Content );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( read );

			// 反馈检查
			OperateResult ackResult = CheckPlcReadResponse( read.Content );
			if (!ackResult.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( ackResult );

			// 数据提炼
			return ExtractActualData( read.Content );
		}

		/// <summary>
		/// 从三菱PLC中批量读取位软元件，返回读取结果，该读取地址最好从0，16，32...等开始读取，这样可以读取比较长得数据数组
		/// </summary>
		/// <param name="address">起始地址</param>
		/// <param name="length">读取的长度</param>
		/// <param name="readCore">指定的通道信息</param>
		/// <returns>带成功标志的结果数据对象</returns>
		/// <example>
		///  <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\MelsecFxSerial.cs" region="ReadBool" title="Bool类型示例" />
		/// </example>
		public static OperateResult<bool[]> ReadBoolHelper( string address, ushort length, Func<byte[], OperateResult<byte[]>> readCore )
		{
			//获取指令
			OperateResult<byte[], int> command = BuildReadBoolCommand( address, length );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( command );

			// 核心交互
			OperateResult<byte[]> read = readCore( command.Content1 );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( read );

			// 反馈检查
			OperateResult ackResult = CheckPlcReadResponse( read.Content );
			if (!ackResult.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( ackResult );

			// 提取真实的数据
			return ExtractActualBoolData( read.Content, command.Content2, length );
		}

		/// <summary>
		/// 向PLC写入数据，数据格式为原始的字节类型
		/// </summary>
		/// <param name="address">初始地址，支持的类型参考文档说明</param>
		/// <param name="value">原始的字节数据</param>
		/// <param name="readCore">指定的通道信息</param>
		/// <param name="isNewVersion">是否是新版的串口访问类</param>
		/// <example>
		/// 假设起始地址为D100，D100存储了温度，100.6℃值为1006，D101存储了压力，1.23Mpa值为123，D102，D103存储了产量计数，写入如下：
		/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\MelsecFxSerial.cs" region="WriteExample2" title="Write示例" />
		/// 以下是读取不同类型数据的示例
		/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\MelsecFxSerial.cs" region="WriteExample1" title="Write示例" />
		/// </example>
		/// <returns>是否写入成功的结果对象</returns>
		public static OperateResult WriteHelper( string address, byte[] value, Func<byte[], OperateResult<byte[]>> readCore, bool isNewVersion )
		{
			// 获取写入
			OperateResult<byte[]> command = BuildWriteWordCommand( address, value, isNewVersion );
			if (!command.IsSuccess) return command;

			// 核心交互
			OperateResult<byte[]> read = readCore( command.Content );
			if (!read.IsSuccess) return read;

			// 结果验证
			OperateResult checkResult = CheckPlcWriteResponse( read.Content );
			if (!checkResult.IsSuccess) return checkResult;

			return OperateResult.CreateSuccessResult( );
		}

		/// <summary>
		/// 强制写入位数据的通断，支持的类型参考文档说明
		/// </summary>
		/// <param name="address">地址信息</param>
		/// <param name="value">是否为通</param>
		/// <param name="readCore">指定的通道信息</param>
		/// <returns>是否写入成功的结果对象</returns>
		public static OperateResult WriteHelper( string address, bool value, Func<byte[], OperateResult<byte[]>> readCore )
		{
			// 先获取指令
			OperateResult<byte[]> command = BuildWriteBoolPacket( address, value );
			if (!command.IsSuccess) return command;

			// 和串口进行核心的数据交互
			OperateResult<byte[]> read = readCore( command.Content );
			if (!read.IsSuccess) return read;

			// 检查结果是否正确
			OperateResult checkResult = CheckPlcWriteResponse( read.Content );
			if (!checkResult.IsSuccess) return checkResult;

			return OperateResult.CreateSuccessResult( );
		}

		/// <summary>
		/// 检查PLC返回的读取数据是否是正常的
		/// </summary>
		/// <param name="ack">Plc反馈的数据信息</param>
		/// <returns>检查结果</returns>
		public static OperateResult CheckPlcReadResponse( byte[] ack )
		{
			if (ack.Length == 0) return new OperateResult( StringResources.Language.MelsecFxReceiveZero );
			if (ack[0] == 0x15)  return new OperateResult( StringResources.Language.MelsecFxAckNagative + " Actual: " + SoftBasic.ByteToHexString( ack, ' ' ) );
			if (ack[0] != 0x02)  return new OperateResult( StringResources.Language.MelsecFxAckWrong + ack[0] + " Actual: " + SoftBasic.ByteToHexString( ack, ' ' ) );

			if (!MelsecHelper.CheckCRC( ack )) return new OperateResult( StringResources.Language.MelsecFxCrcCheckFailed );

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
			if (ack[0] == 0x15)  return new OperateResult( StringResources.Language.MelsecFxAckNagative + " Actual: " + SoftBasic.ByteToHexString( ack, ' ' ) );
			if (ack[0] != 0x06)  return new OperateResult( StringResources.Language.MelsecFxAckWrong + ack[0] + " Actual: " + SoftBasic.ByteToHexString( ack, ' ' ) );

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
		public static OperateResult<byte[]> BuildReadWordCommand( string address, ushort length, bool isNewVersion )
		{
			var addressResult = FxCalculateWordStartAddress( address, isNewVersion );
			if (!addressResult.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( addressResult );

			length = (ushort)(length * 2);
			ushort startAddress = addressResult.Content;

			if (isNewVersion)
			{
				byte[] _PLCCommand = new byte[13];
				_PLCCommand[0] = 0x02;                                                  //报头
				_PLCCommand[1] = 0x45;                                                  //指令形式
				_PLCCommand[2] = 0x30;                                                  //Read
				_PLCCommand[3] = 0x30;                                                  //Read
				_PLCCommand[4] = SoftBasic.BuildAsciiBytesFrom( startAddress )[0];      //起始地址
				_PLCCommand[5] = SoftBasic.BuildAsciiBytesFrom( startAddress )[1];      //起始地址
				_PLCCommand[6] = SoftBasic.BuildAsciiBytesFrom( startAddress )[2];      //起始地址
				_PLCCommand[7] = SoftBasic.BuildAsciiBytesFrom( startAddress )[3];      //起始地址
				_PLCCommand[8] = SoftBasic.BuildAsciiBytesFrom( (byte)length )[0];      // 读取长度
				_PLCCommand[9] = SoftBasic.BuildAsciiBytesFrom( (byte)length )[1];
				_PLCCommand[10] = 0x03;
				MelsecHelper.FxCalculateCRC( _PLCCommand ).CopyTo( _PLCCommand, 11 );   // CRC

				return OperateResult.CreateSuccessResult( _PLCCommand );                // Return
			}
			else
			{
				byte[] _PLCCommand = new byte[11];
				_PLCCommand[0] = 0x02;                                                    // STX
				_PLCCommand[1] = 0x30;                                                    // Read
				_PLCCommand[2] = SoftBasic.BuildAsciiBytesFrom( startAddress )[0];        // 偏移地址
				_PLCCommand[3] = SoftBasic.BuildAsciiBytesFrom( startAddress )[1];
				_PLCCommand[4] = SoftBasic.BuildAsciiBytesFrom( startAddress )[2];
				_PLCCommand[5] = SoftBasic.BuildAsciiBytesFrom( startAddress )[3];
				_PLCCommand[6] = SoftBasic.BuildAsciiBytesFrom( (byte)length )[0];        // 读取长度
				_PLCCommand[7] = SoftBasic.BuildAsciiBytesFrom( (byte)length )[1];
				_PLCCommand[8] = 0x03;                                                    // ETX
				MelsecHelper.FxCalculateCRC( _PLCCommand ).CopyTo( _PLCCommand, 9 );      // CRC

				return OperateResult.CreateSuccessResult( _PLCCommand );                  // Return
			}
		}

		/// <summary>
		/// 根据类型地址长度确认需要读取的指令头
		/// </summary>
		/// <param name="address">起始地址</param>
		/// <param name="length">bool数组长度</param>
		/// <returns>带有成功标志的指令数据</returns>
		public static OperateResult<byte[], int> BuildReadBoolCommand( string address, ushort length )
		{
			var addressResult = FxCalculateBoolStartAddress( address );
			if (!addressResult.IsSuccess) return OperateResult.CreateFailedResult<byte[], int>( addressResult );

			// 计算下实际需要读取的数据长度
			ushort length2 = (ushort)((addressResult.Content2 + length - 1) / 8 - (addressResult.Content2 / 8) + 1);

			ushort startAddress = addressResult.Content1;
			byte[] _PLCCommand = new byte[11];
			_PLCCommand[0] = 0x02;                                                    // STX
			_PLCCommand[1] = 0x30;                                                    // Read
			_PLCCommand[2] = SoftBasic.BuildAsciiBytesFrom( startAddress )[0];      // 偏移地址
			_PLCCommand[3] = SoftBasic.BuildAsciiBytesFrom( startAddress )[1];
			_PLCCommand[4] = SoftBasic.BuildAsciiBytesFrom( startAddress )[2];
			_PLCCommand[5] = SoftBasic.BuildAsciiBytesFrom( startAddress )[3];
			_PLCCommand[6] = SoftBasic.BuildAsciiBytesFrom( (byte)length2 )[0];     // 读取长度
			_PLCCommand[7] = SoftBasic.BuildAsciiBytesFrom( (byte)length2 )[1];
			_PLCCommand[8] = 0x03;                                                    // ETX
			MelsecHelper.FxCalculateCRC( _PLCCommand ).CopyTo( _PLCCommand, 9 );      // CRC

			return OperateResult.CreateSuccessResult( _PLCCommand, (int)addressResult.Content3 );
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
				_PLCCommand[0] = 0x02;  //报头
				_PLCCommand[1] = 0x45;  //指令形式
				_PLCCommand[2] = 0x31;  //Write
				_PLCCommand[3] = 0x30;  //Write
				_PLCCommand[4] = SoftBasic.BuildAsciiBytesFrom( startAddress )[0];                        // Offect Address
				_PLCCommand[5] = SoftBasic.BuildAsciiBytesFrom( startAddress )[1];
				_PLCCommand[6] = SoftBasic.BuildAsciiBytesFrom( startAddress )[2];
				_PLCCommand[7] = SoftBasic.BuildAsciiBytesFrom( startAddress )[3];
				_PLCCommand[8] = SoftBasic.BuildAsciiBytesFrom( (byte)(value.Length / 2) )[0];            // Read Length
				_PLCCommand[9] = SoftBasic.BuildAsciiBytesFrom( (byte)(value.Length / 2) )[1];
				Array.Copy( value, 0, _PLCCommand, 10, value.Length );
				_PLCCommand[_PLCCommand.Length - 3] = 0x03;                                               // ETX
				MelsecHelper.FxCalculateCRC( _PLCCommand ).CopyTo( _PLCCommand, _PLCCommand.Length - 2 ); // CRC

				return OperateResult.CreateSuccessResult( _PLCCommand );
			}
			else
			{
				byte[] _PLCCommand = new byte[11 + value.Length];
				_PLCCommand[0] = 0x02;  //报头
				_PLCCommand[1] = 0x31;  //Write
				_PLCCommand[2] = SoftBasic.BuildAsciiBytesFrom( startAddress )[0];                        // Offect Address
				_PLCCommand[3] = SoftBasic.BuildAsciiBytesFrom( startAddress )[1];
				_PLCCommand[4] = SoftBasic.BuildAsciiBytesFrom( startAddress )[2];
				_PLCCommand[5] = SoftBasic.BuildAsciiBytesFrom( startAddress )[3];
				_PLCCommand[6] = SoftBasic.BuildAsciiBytesFrom( (byte)(value.Length / 2) )[0];            // Read Length
				_PLCCommand[7] = SoftBasic.BuildAsciiBytesFrom( (byte)(value.Length / 2) )[1];
				Array.Copy( value, 0, _PLCCommand, 8, value.Length );
				_PLCCommand[_PLCCommand.Length - 3] = 0x03;                                               // ETX
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
				bool[] array = BasicFramework.SoftBasic.ByteToBoolArray( extraResult.Content, extraResult.Content.Length * 8 );
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
					"Data: " + BasicFramework.SoftBasic.ByteToHexString( response )
				};
			}
		}

		/// <summary>
		/// 解析数据地址成不同的三菱地址类型
		/// </summary>
		/// <param name="address">数据地址</param>
		/// <returns>地址结果对象</returns>
		private static OperateResult<MelsecMcDataType, ushort> FxAnalysisAddress( string address )
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
				if (startAddress >= 8000) startAddress = (ushort)((startAddress - 8000) * 2 + 0x0E00);
				else startAddress = isNewVersion ? (ushort)(startAddress * 2 + 0x4000) : (ushort)(startAddress * 2 + 0x1000);
			}
			else if (analysis.Content1 == MelsecMcDataType.CN)
			{
				if (startAddress >= 200) startAddress = (ushort)((startAddress - 200) * 4 + 0x0C00);
				else startAddress = (ushort)(startAddress * 2 + 0x0A00);
			}
			else if (analysis.Content1 == MelsecMcDataType.TN) startAddress = (ushort)(startAddress * 2 + 0x0800);
			else return new OperateResult<ushort>( StringResources.Language.MelsecCurrentTypeNotSupportedWordOperate );

			return OperateResult.CreateSuccessResult( startAddress );
		}

		/// <summary>
		/// 返回读取的地址及长度信息，以及当前的偏置信息
		/// </summary><param name="address">读取的地址信息</param>
		/// <returns>带起始地址的结果对象</returns>
		private static OperateResult<ushort, ushort, ushort> FxCalculateBoolStartAddress( string address )
		{
			// 初步解析
			var analysis = FxAnalysisAddress( address );
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<ushort, ushort, ushort>( analysis );

			// 二次解析
			ushort startAddress = analysis.Content2;
			if (analysis.Content1 == MelsecMcDataType.M)
			{
				if (startAddress >= 8000) startAddress = (ushort)((startAddress - 8000) / 8 + 0x01E0);
				else startAddress = (ushort)(startAddress / 8 + 0x0100);
			}
			else if (analysis.Content1 == MelsecMcDataType.X)  startAddress = (ushort)(startAddress / 8 + 0x0080);
			else if (analysis.Content1 == MelsecMcDataType.Y)  startAddress = (ushort)(startAddress / 8 + 0x00A0);
			else if (analysis.Content1 == MelsecMcDataType.S)  startAddress = (ushort)(startAddress / 8 + 0x0000);
			else if (analysis.Content1 == MelsecMcDataType.CS) startAddress = (ushort)(startAddress / 8 + 0x01C0);
			else if (analysis.Content1 == MelsecMcDataType.CC) startAddress = (ushort)(startAddress / 8 + 0x03C0);
			else if (analysis.Content1 == MelsecMcDataType.TS) startAddress = (ushort)(startAddress / 8 + 0x00C0);
			else if (analysis.Content1 == MelsecMcDataType.TC) startAddress = (ushort)(startAddress / 8 + 0x02C0);
			else return new OperateResult<ushort, ushort, ushort>( StringResources.Language.MelsecCurrentTypeNotSupportedBitOperate );

			return OperateResult.CreateSuccessResult( startAddress, analysis.Content2, (ushort)(analysis.Content2 % 8) );
		}

		#endregion
	}
}
