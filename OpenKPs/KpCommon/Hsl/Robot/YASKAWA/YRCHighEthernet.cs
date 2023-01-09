using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HslCommunication.BasicFramework;
using HslCommunication.Core.Net;
using HslCommunication;
using HslCommunication.Core;
using System.IO;
using HslCommunication.Reflection;
#if !NET35 && !NET20
using System.Threading.Tasks;
#endif

namespace HslCommunication.Robot.YASKAWA
{
	/// <summary>
	/// 安川机器人的通信类，基于高速以太网的通信，基于UDP协议实现，默认端口10040，支持读写一些数据地址
	/// </summary>
	public class YRCHighEthernet : NetworkUdpBase
	{
		/// <summary>
		/// 实例化一个默认的对象
		/// </summary>
		public YRCHighEthernet( )
		{

		}

		/// <summary>
		/// 使用指定的IP地址和端口号信息来实例化一个对象
		/// </summary>
		/// <param name="ipAddress">IP地址</param>
		/// <param name="port">端口号信息</param>
		public YRCHighEthernet( string ipAddress, int port = 10040 )
		{
			IpAddress = ipAddress;
			Port      = port;
		}


		/// <summary>
		/// 使用自定义的命令来读取机器人指定的数据信息，每个命令返回的数据格式互不相同，需要根据手册来自定义解析的。<br />
		/// Use custom commands to read the data information specified by the robot. 
		/// The data format returned by each command is different from each other, 
		/// and you need to customize the analysis according to the manual.
		/// </summary>
		/// <param name="command">命令编号，相当于CIP 通信协议的Class</param>
		/// <param name="dataAddress">数据队列编号，相当于CIP 通信协议的Instance</param>
		/// <param name="dataAttribute">单元编号，相当于CIP 通信协议的Attribute</param>
		/// <param name="dataHandle">处理(请求), 定义数据请求方法。</param>
		/// <param name="dataPart">附加数据信息</param>
		/// <returns>从机器人返回的设备数据，如果是写入状态，则 Content 为 NULL</returns>
		public OperateResult<byte[]> ReadCommand( ushort command, ushort dataAddress, byte dataAttribute, byte dataHandle, byte[] dataPart )
		{
			byte[] build = Helper.YRCHighEthernetHelper.BuildCommand( handle, (byte)incrementCount.GetCurrentValue( ),
				command, dataAddress, dataAttribute, dataHandle, dataPart );

			OperateResult<byte[]> read = ReadFromCoreServer( build );
			if (!read.IsSuccess) return read;

			OperateResult check = Helper.YRCHighEthernetHelper.CheckResponseContent( read.Content );
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( check );

			if (read.Content.Length > 32) return OperateResult.CreateSuccessResult( read.Content.RemoveBegin( 32 ) );
			return OperateResult.CreateSuccessResult( new byte[0] );
		}

		/// <summary>
		/// 读取机器人的最新的报警列表信息，最多为四个报警
		/// </summary>
		/// <returns>报警列表信息</returns>
		public OperateResult<YRCAlarmItem[]> ReadAlarms( )
		{
			YRCAlarmItem[] alarmItems = new YRCAlarmItem[4];
			for (int i = 0; i < alarmItems.Length; i++)
			{
				OperateResult<byte[]> read = ReadCommand( 0x70, (ushort)(i + 1), 0, 0x01, null );
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<YRCAlarmItem[]>( read );

				if (read.Content.Length > 0)
					alarmItems[i] = new YRCAlarmItem( this.byteTransform, read.Content, this.encoding );

			}
			return OperateResult.CreateSuccessResult( alarmItems );
		}

		/// <summary>
		/// 读取机器人的指定的报警信息，需要指定报警类型，及报警数量，其中length为1-100之间。
		/// </summary>
		/// <param name="alarmType">报警类型，1-100:重故障; 1001-1100: 轻故障; 2001-2100: 用户报警(系统); 3001-3100: 用户报警(用户); 4001-4100:在线报警</param>
		/// <param name="length">读取的报警的个数</param>
		/// <returns>报警列表信息</returns>
		public OperateResult<YRCAlarmItem[]> ReadHistoryAlarms( ushort alarmType, short length )
		{
			YRCAlarmItem[] alarmItems = new YRCAlarmItem[length];
			for (int i = 0; i < alarmItems.Length; i++)
			{
				OperateResult<byte[]> read = ReadCommand( 0x71, alarmType, 0, 0x01, null );
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<YRCAlarmItem[]>( read );

				if (read.Content.Length > 0)
					alarmItems[i] = new YRCAlarmItem( this.byteTransform, read.Content, this.encoding );

			}
			return OperateResult.CreateSuccessResult( alarmItems );
		}

		/// <inheritdoc cref="YRC1000TcpNet.ReadStats"/>
		public OperateResult<bool[]> ReadStats( ) => ByteTransformHelper.GetSuccessResultFromOther( ReadCommand( 0x72, 1, 0, 0x01, null ),
			m => new byte[] { (byte)this.byteTransform.TransInt32( m, 0 ), (byte)this.byteTransform.TransInt32( m, 4 ) }.ToBoolArray( ) );

		/// <summary>
		/// 读取当前的机器人的程序名称，行编号，步骤编号，速度超出值。需要指定当前的任务号，默认为1，表示主任务<br />
		/// Read the current robot's program name, line number, step number, and speed exceeding value. 
		/// Need to specify the current task number, the default is 1, which means the main task
		/// </summary>
		/// <param name="task">任务标识，1:主任务; 2-16分别表示子任务1-子任务15</param>
		/// <returns>读取的任务的结果信息</returns>
		public OperateResult<string[]> ReadJSeq( ushort task = 1 )
		{
			OperateResult<byte[]> read = ReadCommand( 0x73, task, 0, 0x01, null );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<string[]>( read );

			string[] result = new string[4];
			result[0] = encoding.GetString( read.Content, 0, 32 );
			result[1] = byteTransform.TransInt32( read.Content, 32 ).ToString( );
			result[2] = byteTransform.TransInt32( read.Content, 36 ).ToString( );
			result[3] = byteTransform.TransInt32( read.Content, 40 ).ToString( );
			return OperateResult.CreateSuccessResult( result );
		}

		/// <summary>
		/// 读取机器人的姿态信息，包括X,Y,Z,Rx,Ry,Rz,如果是七轴机器人，还包括Re
		/// </summary>
		/// <returns>姿态信息</returns>
		public OperateResult<string[]> ReadPose( )
		{
			OperateResult<byte[]> read = ReadCommand( 0x75, 101, 0, 0x01, null );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<string[]>( read );

			string[] result = new string[read.Content.Length / 4 - 5];
			for (int i = 0; i < result.Length; i++)
			{
				result[i] = byteTransform.TransInt32( read.Content, 20 + i * 4 ).ToString( );
			}
			return OperateResult.CreateSuccessResult( result );
		}

		/// <summary>
		/// 读取力矩数据功能
		/// </summary>
		/// <returns>力矩信息</returns>
		public OperateResult<string[]> ReadTorqueData( )
		{
			OperateResult<byte[]> read = ReadCommand( 0x77, 21, 0, 0x01, null );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<string[]>( read );

			string[] result = new string[read.Content.Length / 4];
			for (int i = 0; i < result.Length; i++)
			{
				result[i] = byteTransform.TransInt32( read.Content, i * 4 ).ToString( );
			}
			return OperateResult.CreateSuccessResult( result );
		}

		/// <inheritdoc cref="ReadIO(ushort, int)"/>
		public OperateResult<byte> ReadIO( ushort address )
		{
			OperateResult<byte[]> read = ReadCommand( 0x78, address, 1, 0x0E, null );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte>( read );

			return OperateResult.CreateSuccessResult( read.Content[0] );
		}

		/// <summary>
		/// 写入IO的数据，只可写入网络输入信号，也即地址是 2701~2956
		/// </summary>
		/// <param name="address">网络输入信号，也即地址是 2701~2956</param>
		/// <param name="value">表示8个bool的字节数据</param>
		/// <returns>是否写入成功</returns>
		public OperateResult WriteIO( ushort address, byte value )
		{
			return ReadCommand( 0x78, address, 1, 0x10, new byte[] { value } );
		}

		/// <summary>
		/// 读取IO数据，需要指定IO的地址。
		/// </summary>
		/// <remarks>
		/// io地址如下：<br />
		/// 1~512: 机器人通用输入命令；1001~1512：机器人通用输出命令；2001~2512：外部输入信号；2701~2956：网络输入信号；
		/// 3001~3512：外部输出信号；3701~3956：网络输出信号；4001~4256：机器人专用输入信号；5001~5512：机器人专用输出信号；
		/// 6001~6064：接口面板输入信号；7001~7999：辅助继电器信号；8001~8512：机器人控制状态信号；8701~8720：模拟输入信号；
		/// </remarks>
		/// <param name="address">信号地址，详细参见注释</param>
		/// <param name="length">读取的数据长度信息</param>
		/// <returns>bool值</returns>
		public OperateResult<byte[]> ReadIO( ushort address, int length )
		{
			OperateResult<byte[]> read = ReadCommand( 0x300, address, 0, 0x33, this.byteTransform.TransByte( length ) );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( read );

			int count = this.byteTransform.TransInt32( read.Content, 0 );
			return OperateResult.CreateSuccessResult( read.Content.SelectMiddle( 4, count ) );
		}

		/// <summary>
		/// 写入多个IO数据的命令，写入的字节长度需要是2的倍数
		/// </summary>
		/// <param name="address">网络输入信号，也即地址是 2701~2956</param>
		/// <param name="value">连续的字数据</param>
		/// <returns>是否写入成功</returns>
		public OperateResult WriteIO( ushort address, byte[] value )
		{
			return ReadCommand( 0x300, address, 0, 0x34, value );
		}

		#region Register ReadWrite

		/// <summary>
		/// 读取寄存器的数据，地址范围 0 ~ 999
		/// </summary>
		/// <param name="address">地址索引</param>
		/// <returns>读取结果数据</returns>
		public OperateResult<ushort> ReadRegisterVariable( ushort address ) => ByteTransformHelper.GetSuccessResultFromOther( ReadCommand( 0x79, address, 1, 0x0E, null ), m => this.byteTransform.TransUInt16( m, 0 ) );

		/// <summary>
		/// 将数据写入到寄存器，支持写入的地址范围为 0 ~ 599
		/// </summary>
		/// <param name="address">地址索引</param>
		/// <param name="value">等待写入的值</param>
		/// <returns>是否写入成功</returns>
		public OperateResult WriteRegisterVariable( ushort address, ushort value ) => ReadCommand( 0x79, address, 1, 0x10, this.byteTransform.TransByte( value ) );

		/// <summary>
		/// 批量读取多个寄存器的数据，地址范围 0 ~ 999，指定读取的数据长度，最大不超过237 个
		/// </summary>
		/// <param name="address">地址索引</param>
		/// <param name="length">读取的数据长度，最大不超过237 个</param>
		/// <returns>读取结果内容</returns>
		public OperateResult<ushort[]> ReadRegisterVariable( ushort address, int length ) => ByteTransformHelper.GetSuccessResultFromOther(
			ReadCommand( 0x301, address, 0, 0x33, this.byteTransform.TransByte( length ) ), m => this.byteTransform.TransUInt16( m, 0, length ) );

		/// <summary>
		/// 写入多个数据到寄存器，地址范围 0 ~ 999，指定读取的数据长度，最大不超过237 个
		/// </summary>
		/// <param name="address">地址索引</param>
		/// <param name="value">等待写入的数据，最大不超过237 个长度</param>
		/// <returns>是否写入成功</returns>
		public OperateResult WriteRegisterVariable( ushort address, ushort[] value ) => ReadCommand( 0x301, address, 0, 0x34, this.byteTransform.TransByte( value ) );

		#endregion

		#region Byte ReadWrite

		/// <summary>
		/// 读取字节型变量的数据，标准地址范围为 0 ~ 99
		/// </summary>
		/// <param name="address">标准地址范围为 0 ~ 99</param>
		/// <returns>读取的结果对象</returns>
		public OperateResult<byte> ReadByteVariable( ushort address ) => ByteTransformHelper.GetResultFromArray( ReadCommand( 0x7A, address, 1, 0x0E, null ) );

		/// <summary>
		/// 将数据写入到字节型变量的地址里去，标准地址范围为 0 ~ 99
		/// </summary>
		/// <param name="address">标准地址范围为 0 ~ 99</param>
		/// <param name="value">等待写入的值</param>
		/// <returns>是否写入成功</returns>
		public OperateResult WriteByteVariable( ushort address, byte value ) => ReadCommand( 0x7A, address, 1, 0x10, new byte[] { value } );

		/// <summary>
		/// 读取多个的字节型变量的数据，读取的最大个数为 474 个。
		/// </summary>
		/// <param name="address">标准地址范围为 0 ~ 99</param>
		/// <param name="length">读取的数据个数，读取的最大个数为 474 个</param>
		/// <returns>结果数据内容</returns>
		public OperateResult<byte[]> ReadByteVariable( ushort address, int length ) => ReadCommand( 0x302, address, 0, 0x33, this.byteTransform.TransByte( length ) );

		/// <summary>
		/// 将多个字节的变量的数据写入到指定的地址，最大个数为 474 个，仅可指定2的倍数
		/// </summary>
		/// <param name="address">标准地址范围为 0 ~ 99</param>
		/// <param name="vaule">写入的值，最大个数为 474 个，仅可指定2的倍数</param>
		/// <returns>是否写入成功</returns>
		public OperateResult WriteByteVariable( ushort address, byte[] vaule ) => ReadCommand( 0x302, address, 0, 0x34, vaule );

		#endregion

		#region Integer ReadWrite

		/// <summary>
		/// 读取单个的整型变量数据，地址范围：0 ～ 99（ 标准设定时）
		/// </summary>
		/// <param name="address">0 ～ 99（ 标准设定时）</param>
		/// <returns>读取结果对象</returns>
		public OperateResult<short> ReadIntegerVariable( ushort address ) => ByteTransformHelper.GetSuccessResultFromOther( ReadCommand( 0x7B, address, 1, 0x0E, null ), m => this.byteTransform.TransInt16( m, 0 ) );

		/// <summary>
		/// 将单个的数据写入到整型变量去，地址范围：0 ～ 99（ 标准设定时）
		/// </summary>
		/// <param name="address">地址范围：0 ～ 99（ 标准设定时）</param>
		/// <param name="value">等待写入的值</param>
		/// <returns>是否写入成功</returns>
		public OperateResult WriteIntegerVariable( ushort address, short value ) => ReadCommand( 0x7B, address, 1, 0x10, this.byteTransform.TransByte( value ) );

		/// <summary>
		/// 读取多个的整型变量数据，地址范围：0 ～ 99（ 标准设定时）
		/// </summary>
		/// <param name="address">地址范围：0 ～ 99（ 标准设定时）</param>
		/// <param name="length">读取的个数</param>
		/// <returns>读取结果对象</returns>
		public OperateResult<short[]> ReadIntegerVariable( ushort address, int length ) => ByteTransformHelper.GetSuccessResultFromOther( 
			ReadCommand( 0x303, address, 0, 0x33, this.byteTransform.TransByte( length ) ), m => this.byteTransform.TransInt16( m, 0, length ) );

		/// <summary>
		/// 写入多个的整型变量数据到机器人，地址范围：0 ～ 99（ 标准设定时）
		/// </summary>
		/// <param name="address">地址范围：0 ～ 99（ 标准设定时）</param>
		/// <param name="value">等待写入的数据信息</param>
		/// <returns>是否写入成功</returns>
		public OperateResult WriteIntegerVariable( ushort address, short[] value ) => ReadCommand( 0x303, address, 0, 0x34, this.byteTransform.TransByte( value ) );

		#endregion

		#region DoubleInteger ReadWrite

		/// <summary>
		/// 读取单个的双精度整型变量数据，地址范围：0 ～ 99（ 标准设定时）
		/// </summary>
		/// <param name="address">0 ～ 99（ 标准设定时）</param>
		/// <returns>读取结果对象</returns>
		public OperateResult<int> ReadDoubleIntegerVariable( ushort address ) => ByteTransformHelper.GetSuccessResultFromOther( ReadCommand( 0x7C, address, 1, 0x0E, null ), m => this.byteTransform.TransInt32( m, 0 ) );

		/// <summary>
		/// 将单个的数据写入到双精度整型变量去，地址范围：0 ～ 99（ 标准设定时）
		/// </summary>
		/// <param name="address">地址范围：0 ～ 99（ 标准设定时）</param>
		/// <param name="value">等待写入的值</param>
		/// <returns>是否写入成功</returns>
		public OperateResult WriteDoubleIntegerVariable( ushort address, int value ) => ReadCommand( 0x7C, address, 1, 0x10, this.byteTransform.TransByte( value ) );

		/// <summary>
		/// 读取多个的双精度整型变量数据，地址范围：0 ～ 99（ 标准设定时）
		/// </summary>
		/// <param name="address">地址范围：0 ～ 99（ 标准设定时）</param>
		/// <param name="length">读取的个数，最大118个</param>
		/// <returns>读取结果对象</returns>
		public OperateResult<int[]> ReadDoubleIntegerVariable( ushort address, int length ) => ByteTransformHelper.GetSuccessResultFromOther(
			ReadCommand( 0x304, address, 0, 0x33, this.byteTransform.TransByte( length ) ), m => this.byteTransform.TransInt32( m, 0, length ) );

		/// <summary>
		/// 写入多个的双精度整型变量数据到机器人，地址范围：0 ～ 99（ 标准设定时）
		/// </summary>
		/// <param name="address">地址范围：0 ～ 99（ 标准设定时）</param>
		/// <param name="value">等待写入的数据信息，最大118个数据</param>
		/// <returns>是否写入成功</returns>
		public OperateResult WriteDoubleIntegerVariable( ushort address, int[] value ) => ReadCommand( 0x304, address, 0, 0x34, this.byteTransform.TransByte( value ) );

		#endregion

		#region Real ReadWrite

		/// <summary>
		/// 读取单个的实数型变量数据，地址范围：0 ～ 99（ 标准设定时）
		/// </summary>
		/// <param name="address">地址范围：0 ～ 99（ 标准设定时）</param>
		/// <returns>读取结果内容</returns>
		public OperateResult<float> ReadRealVariable( ushort address ) => ByteTransformHelper.GetSuccessResultFromOther( ReadCommand( 0x7D, address, 1, 0x0E, null ), m => this.byteTransform.TransSingle( m, 0 ) );

		/// <summary>
		/// 将单个的数据写入到实数型变量去，地址范围：0 ～ 99（ 标准设定时）
		/// </summary>
		/// <param name="address">地址范围：0 ～ 99（ 标准设定时）</param>
		/// <param name="value">写入的值</param>
		/// <returns>是否写入成功</returns>
		public OperateResult WriteRealVariable( ushort address, float value ) => ReadCommand( 0x7D, address, 1, 0x10, this.byteTransform.TransByte( value ) );

		/// <summary>
		/// 读取多个的实数型变量数据，地址范围：0 ～ 99（ 标准设定时）
		/// </summary>
		/// <param name="address">地址范围：0 ～ 99（ 标准设定时）</param>
		/// <param name="length">读取的个数，最大118个</param>
		/// <returns>读取的结果对象</returns>
		public OperateResult<float[]> ReadRealVariable( ushort address, int length ) => ByteTransformHelper.GetSuccessResultFromOther(
			ReadCommand( 0x305, address, 0, 0x33, this.byteTransform.TransByte( length ) ), m => this.byteTransform.TransSingle( m, 0, length ) );

		/// <summary>
		/// 写入多个的实数型的变量数据到机器人，地址范围：0 ～ 99（ 标准设定时）
		/// </summary>
		/// <param name="address">地址范围：0 ～ 99（ 标准设定时）</param>
		/// <param name="value">等待写入的数据信息，最大118个数据</param>
		/// <returns>是否写入成功</returns>
		public OperateResult WriteRealVariable( ushort address, float[] value ) => ReadCommand( 0x305, address, 0, 0x34, this.byteTransform.TransByte( value ) );

		#endregion

		#region String ReadWrite

		/// <summary>
		/// 读取单个的字符串变量数据，地址范围：0 ～ 99（ 标准设定时）
		/// </summary>
		/// <param name="address">地址范围：0 ～ 99（ 标准设定时）</param>
		/// <returns>读取的结果对象</returns>
		public OperateResult<string> ReadStringVariable( ushort address ) => ByteTransformHelper.GetSuccessResultFromOther( ReadCommand( 0x7E, address, 1, 0x0E, null ), m => this.byteTransform.TransString( m, this.encoding ) );

		/// <summary>
		/// 写入单个的字符串变量数据，地址范围：0 ～ 99（ 标准设定时）
		/// </summary>
		/// <param name="address">地址范围：0 ～ 99（ 标准设定时）</param>
		/// <param name="value">写入的字符串数据</param>
		/// <returns>是否写入成功</returns>
		public OperateResult WriteStringVariable( ushort address, string value ) => ReadCommand( 0x7E, address, 1, 0x10, SoftBasic.ArrayExpandToLength( this.encoding.GetBytes( value ), 16 ) );

		/// <summary>
		/// 读取多个的字符串变量数据，地址范围：0 ～ 99（ 标准设定时）
		/// </summary>
		/// <param name="address">地址范围：0 ～ 99（ 标准设定时）</param>
		/// <param name="length">读取的字符串个数，最大个数为 29 </param>
		/// <returns>读取的结果对象</returns>
		public OperateResult<string[]> ReadStringVariable( ushort address, int length )
		{
			OperateResult<byte[]> read = ReadCommand( 0x306, address, 0, 0x33, this.byteTransform.TransByte( length ) );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<string[]>( read );

			string[] result = new string[ length ];
			for ( int i = 0; i < length; i++)
			{
				result[i] = this.encoding.GetString( read.Content, i * 16, 16 );
			}
			return OperateResult.CreateSuccessResult( result );
		}

		/// <summary>
		/// 写入多个的字符串变量数据到机器人，地址范围：0 ～ 99（ 标准设定时）
		/// </summary>
		/// <param name="address">地址范围：0 ～ 99（ 标准设定时）</param>
		/// <param name="value">等待写入的字符串数组，最大数组长度为 29 </param>
		/// <returns>是否写入成功</returns>
		public OperateResult WriteStringVariable( ushort address, string[] value )
		{
			byte[] buffer = new byte[ value.Length * 16 ];
			for (int i = 0; i < value.Length; i++)
			{
				this.encoding.GetBytes( value[i] ).CopyTo( buffer, i * 16 );
			}
			return ReadCommand( 0x306, address, 0, 0x34, buffer );
		}

		#endregion

		#region System Control

		/// <inheritdoc cref="YRC1000TcpNet.Hold(bool)"/>
		[HslMqttApi( Description = "进行HOLD 的 ON/OFF 操作，状态参数 False: OFF，True: ON" )]
		public OperateResult Hold( bool status ) => ReadCommand( 0x83, 1, 1, 0x10, status ? this.byteTransform.TransByte( 1 ) : this.byteTransform.TransByte( 2 ) );

		/// <inheritdoc cref="YRC1000TcpNet.Reset"/>
		[HslMqttApi( Description = "对机械手的报警进行复位" )]
		public OperateResult Reset( ) => ReadCommand( 0x82, 1, 1, 0x10, this.byteTransform.TransByte( 1 ) );

		/// <inheritdoc cref="YRC1000TcpNet.Cancel"/>
		[HslMqttApi( Description = "进行错误取消" )]
		public OperateResult Cancel( ) => ReadCommand( 0x82, 2, 1, 0x10, this.byteTransform.TransByte( 1 ) );

		/// <inheritdoc cref="YRC1000TcpNet.Svon(bool)"/>
		[HslMqttApi( Description = "进行伺服电源的ON/OFF操作，状态参数 False: OFF，True: ON" )]
		public OperateResult Svon( bool status ) => ReadCommand( 0x83, 2, 1, 0x10, status ? this.byteTransform.TransByte( 1 ) : this.byteTransform.TransByte( 2 ) );

		/// <inheritdoc cref="YRC1000TcpNet.HLock(bool)"/>
		[HslMqttApi( Description = "设定示教编程器和 I/O的操作信号的联锁。 状态参数 False: OFF，True: ON" )]
		public OperateResult HLock( bool status ) => ReadCommand( 0x83, 3, 1, 0x10, status ? this.byteTransform.TransByte( 1 ) : this.byteTransform.TransByte( 2 ) );

		/// <inheritdoc cref="YRC1000TcpNet.Cycle(int)"/>
		[HslMqttApi( Description = "选择循环。循环编号 1:步骤，2:1循环，3:连续自动" )]
		public OperateResult Cycle( int number ) => ReadCommand( 0x84, 2, 1, 0x10, this.byteTransform.TransByte( number ) );

		/// <inheritdoc cref="YRC1000TcpNet.MSDP(string)"/>
		[HslMqttApi( Description = "接受消息数据时， 在YRC1000的示教编程器的远程画面下显示消息若。若不是远程画面时，强制切换到远程画面。显示MDSP命令的消息。" )]
		public OperateResult MSDP( string message ) => ReadCommand( 0x85, 1, 1, 0x10, string.IsNullOrEmpty( message ) ? new byte[0] : this.encoding.GetBytes( message ) );

		/// <inheritdoc cref="YRC1000TcpNet.Start(string)"/>
		[HslMqttApi( Description = "开始程序。操作时指定程序名时，此程序能附带对应主程序，则从该程序的开头开始执行。如果没有指定，则从前行开始执行" )]
		public OperateResult Start( ) => ReadCommand( 0x86, 1, 1, 0x10, this.byteTransform.TransByte( 1 ) );

		/// <summary>
		/// 读取机器人的诗句信息，根据地址来获取不同的时间，地址如下：<br />
		/// 1: 控制电源的接通时间<br />
		/// 10: 伺服电源接通时间(TOTAL)<br />
		/// 11~18: 伺服电源接通时间(R1~R8)<br />
		/// 21~44: 伺服电源接通时间(S1~S24)<br />
		/// 110: 再线时间（TOTAL）<br />
		/// 111~118: 再线时间（R1~R8）<br />
		/// 121~144: 再线时间 (S1~S24)<br />
		/// 210: 移动时间（TOTAL）<br />
		/// 211~218: 移动时间（R1~R8）<br />
		/// 221~244: 移动时间（S1~S24）<br />
		/// 301~308: 作业时间（用途1~用途8）
		/// </summary>
		/// <param name="address">时间的地址信息，具体参照方法的注释</param>
		/// <returns>读取的时间信息</returns>
		public OperateResult<DateTime> ReadManagementTime( ushort address )
		{
			OperateResult<byte[]> read = ReadCommand( 0x88, address, 1, 0x0E, null );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<DateTime>( read );

			return OperateResult.CreateSuccessResult( Convert.ToDateTime( Encoding.ASCII.GetString( read.Content, 0, 16 ) ) );
		}

		/// <summary>
		/// 读取系统的参数信息，其中系统种类参数：<br />
		/// 11~18:机种信息R1~R8; <br />
		/// 21~44:机种信息S1~S24; <br />
		/// 101~108: 用途信息(用途1~用途8); <br />
		/// 返回数据信息为数组，分别为 [0]:系统软件版本；[1]:机种名称/用途名称；[2]:参数版本
		/// </summary>
		/// <param name="system">统种类参数：11~18:机种信息R1~R8; 21~44:机种信息S1~S24; 101~108: 用途信息(用途1~用途8);</param>
		/// <returns>返回数据信息为数组，分别为 [0]:系统软件版本；[1]:机种名称/用途名称；[2]:参数版本</returns>
		public OperateResult<string[]> ReadSystemInfo( ushort system )
		{
			OperateResult<byte[]> read = ReadCommand( 0x89, system, 0x00, 0x01, null );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<string[]>( read );

			string[] result = new string[3];
			result[0] = encoding.GetString( read.Content, 0, 24 );
			result[1] = encoding.GetString( read.Content, 24, 16 );
			result[2] = encoding.GetString( read.Content, 40, 8 );
			return OperateResult.CreateSuccessResult( result );
		}

		/// <inheritdoc cref="YRC1000TcpNet.JSeq(string, int)"/>
		[HslMqttApi( Description = "设定执行程序的名称和行编号。" )]
		public OperateResult JSeq( string programName, int line )
		{
			byte[] buffer = new byte[36];
			this.encoding.GetBytes( programName ).CopyTo( buffer, 0 );
			this.byteTransform.TransByte( line ).CopyTo( buffer, 32 );
			return ReadCommand( 0x84, 2, 1, 0x10, buffer );
		}

		/// <summary>
		/// 指定坐标系的当前值读取。并且可以指定外部轴的有无。<br />
		/// The current value of the specified coordinate system is read. And you can specify the presence or absence of an external axis.
		/// </summary>
		/// <param name="coordinate">指定读取坐标 0:基座坐标，1:机器人坐标，2-65分别表示用户坐标1-64</param>
		/// <param name="hasExteralAxis">外部轴的有/无</param>
		/// <returns>坐标系当前值</returns>
		[HslMqttApi( Description = "指定坐标系的当前值读取。并且可以指定外部轴的有无。" )]
		public OperateResult<YRCRobotData> ReadPOSC( int coordinate, bool hasExteralAxis )
		{
			return new OperateResult<YRCRobotData>( );
			//OperateResult<string> read = ReadByCommand( "RPOSC", $"{coordinate},{(hasExteralAxis ? "1" : "0")}" );
			//if (!read.IsSuccess) return OperateResult.CreateFailedResult<YRCRobotData>( read );

			//return OperateResult.CreateSuccessResult( new YRCRobotData( Type, read.Content ) );
		}


		///// <summary>
		///// 向指定的坐标系位置进行关节动作。其中没有外部轴的系统， 7-12外部轴的值设定为「0」<br />
		///// Perform joint motions to the specified coordinate system position.
		///// where there is no external axis system, the value of 7-12 external axis is set to "0"
		///// </summary>
		///// <param name="robotData">机器的的数据信息</param>
		///// <remarks>
		///// 其中形态数据由6个bool数组组成，每个bool含义参考参数说明，0表示 <c>False</c>，1表示 <c>True</c>
		///// </remarks>
		///// <returns>是否动作成功</returns>
		////public OperateResult MoveJ( YRCRobotData robotData ) => ReadByCommand( "MOVJ", robotData.ToWriteString( Type ) );

		#endregion




		private IByteTransform byteTransform = new RegularByteTransform( );
		private SoftIncrementCount incrementCount = new SoftIncrementCount( byte.MaxValue );
		private byte handle = 0x01;            // 处理分区，1：机器人控制  2：文件控制
		private Encoding encoding = Encoding.ASCII;
	}
}
