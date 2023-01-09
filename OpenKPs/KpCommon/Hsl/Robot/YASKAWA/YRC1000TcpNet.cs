using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using HslCommunication.Core;
using HslCommunication.Core.IMessage;
using HslCommunication.Core.Net;
using HslCommunication.Reflection;
using System.Text.RegularExpressions;
using HslCommunication.BasicFramework;
#if !NET35 && !NET20
using System.Threading.Tasks;
#endif

namespace HslCommunication.Robot.YASKAWA
{
	/// <summary>
	/// 安川机器人的Ethernet 服务器功能对应的客户端通讯类<br />
	/// Yaskawa robot's Ethernet server features a communication class
	/// </summary>
	/// <remarks>
	/// 要想成功的通信，有两个至关重要的前提。<br />
	/// 1. 开启以太网服务器，[系统]-[设定]-[选项功能]-[网络功能设定]启用网络功能。<br />
	/// 2. 开启远程的命令，[输入输出]-[模拟输入]-[远程命令选择] 激活远程命令
	/// </remarks>
	public class YRC1000TcpNet : NetworkDoubleBase, IRobotNet
	{
		#region Constructor

		/// <summary>
		/// 指定机器人的ip地址及端口号来实例化对象<br />
		/// Specify the robot's IP address and port number to instantiate the object
		/// </summary>
		/// <param name="ipAddress">Ip地址</param>
		/// <param name="port">端口号</param>
		public YRC1000TcpNet( string ipAddress, int port )
		{
			IpAddress     = ipAddress;
			Port          = port;
			ByteTransform = new ReverseWordTransform( );
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// 获取或设置当前的机器人类型，默认为 <see cref="YRCType.YRC1000"/>
		/// Get or set the current robot type, the default is <see cref="YRCType.YRC1000"/>
		/// </summary>
		public YRCType Type { get; set; } = YRCType.YRC1000;

		#endregion

		#region IRobot Interface

		/// <inheritdoc cref="IRobotNet.Read(string)"/>
		[HslMqttApi( ApiTopic = "ReadRobotByte", Description = "Read the robot's original byte data information according to the address" )]
		public OperateResult<byte[]> Read( string address )
		{
			OperateResult<string> read = ReadString( address );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( read );

			return OperateResult.CreateSuccessResult( Encoding.ASCII.GetBytes( read.Content ) );
		}

		/// <inheritdoc cref="IRobotNet.ReadString(string)"/>
		[HslMqttApi( ApiTopic = "ReadRobotString", Description = "Read the string data information of the robot based on the address" )]
		public OperateResult<string> ReadString( string address )
		{
			if (address.IndexOf( '.' ) >= 0 || address.IndexOf( ':' ) >= 0 || address.IndexOf( ';' ) >= 0)
			{
				string[] commands = address.Split( new char[] { '.', ':', ';' } );
				return ReadByCommand( commands[0], commands[1] );
			}
			else
			{
				return ReadByCommand( address, null );
			}
		}

		/// <inheritdoc cref="IRobotNet.Write(string, byte[])"/>
		[HslMqttApi( ApiTopic = "WriteRobotByte", Description = "According to the address, to write the device related bytes data" )]
		public OperateResult Write( string address, byte[] value ) => Write( address, Encoding.ASCII.GetString( value ) );

		/// <inheritdoc cref="IRobotNet.Write(string, string)"/>
		[HslMqttApi( ApiTopic = "WriteRobotString", Description = "According to the address, to write the device related string data" )]
		public OperateResult Write( string address, string value ) => ReadByCommand( address, value );

		#endregion

		#region Async IRobot Interface
#if !NET35 && !NET20
		/// <inheritdoc cref="IRobotNet.ReadAsync(string)"/>
		public async Task<OperateResult<byte[]>> ReadAsync( string address )
		{
			OperateResult<string> read = await ReadStringAsync( address );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( read );

			return OperateResult.CreateSuccessResult( Encoding.ASCII.GetBytes( read.Content ) );
		}

		/// <inheritdoc cref="IRobotNet.ReadStringAsync(string)"/>
		public async Task<OperateResult<string>> ReadStringAsync( string address )
		{
			if (address.IndexOf( '.' ) >= 0 || address.IndexOf( ':' ) >= 0 || address.IndexOf( ';' ) >= 0)
			{
				string[] commands = address.Split( new char[] { '.', ':', ';' } );
				return await ReadByCommandAsync( commands[0], commands[1] );
			}
			else
			{
				return await ReadByCommandAsync( address, null );
			}
		}

		/// <inheritdoc cref="IRobotNet.WriteAsync(string, byte[])"/>
		public async Task<OperateResult> WriteAsync( string address, byte[] value ) => await WriteAsync( address, Encoding.ASCII.GetString( value ) );

		/// <inheritdoc cref="IRobotNet.WriteAsync(string, string)"/>
		public async Task<OperateResult> WriteAsync( string address, string value ) => await ReadByCommandAsync( address, value );
#endif
		#endregion

		#region Initialization Override

		/// <summary>
		/// before read data , the connection should be Initialized
		/// </summary>
		/// <param name="socket">connected socket</param>
		/// <returns>whether is the Initialization is success.</returns>
		protected override OperateResult InitializationOnConnect( Socket socket )
		{
			var read = ReadFromCoreServer( socket, "CONNECT Robot_access KeepAlive:-1\r\n" );
			if (!read.IsSuccess) return read;

			if (read.Content == "OK:YR Information Server(Ver) Keep-Alive:-1.\r\n") return OperateResult.CreateSuccessResult();

			// 检查命令是否返回成功的状态
			if (!read.Content.StartsWith("OK:")) return new OperateResult(read.Content);

			// 不是长连接模式
			isPersistentConn = false;
			return OperateResult.CreateSuccessResult( );
		}
#if !NET35 && !NET20
		/// <summary>
		/// before read data , the connection should be Initialized
		/// </summary>
		/// <param name="socket">connected socket</param>
		/// <returns>whether is the Initialization is success.</returns>
		protected override async Task<OperateResult> InitializationOnConnectAsync( Socket socket )
		{
			var read = await ReadFromCoreServerAsync( socket, "CONNECT Robot_access KeepAlive:-1\r\n" );
			if (!read.IsSuccess) return read;

			if (read.Content == "OK:YR Information Server(Ver) Keep-Alive:-1.\r\n") return OperateResult.CreateSuccessResult( );

			// 检查命令是否返回成功的状态
			if (!read.Content.StartsWith( "OK:" )) return new OperateResult( read.Content );

			// 不是长连接模式
			isPersistentConn = false;
			return OperateResult.CreateSuccessResult( );
		}
#endif
		#endregion

		#region Override Read

		/// <inheritdoc/>
		protected override INetMessage GetNewNetMessage( ) => new SpecifiedCharacterMessage( (byte)'\r', (byte)'\n' );

		/// <summary>
		/// Read string value from socket
		/// </summary>
		/// <param name="socket">connected socket</param>
		/// <param name="send">string value</param>
		/// <returns>received string value with is successfully</returns>
		protected OperateResult<string> ReadFromCoreServer( Socket socket, string send )
		{
			var read = ReadFromCoreServer( socket, Encoding.Default.GetBytes( send ) );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<string>( read );

			return OperateResult.CreateSuccessResult( Encoding.Default.GetString( read.Content ) );
		}

		/// <summary>
		/// 根据指令来读取设备的信息，如果命令数据为空，则传入null即可，注意，所有的命令不带换行符<br />
		/// Read the device information according to the instructions. If the command data is empty, pass in null. Note that all commands do not have a newline character
		/// </summary>
		/// <remarks>
		/// 此处举几个例子<br />
		/// "RALARM", NULL    错误报警代码读取。<br />
		/// "RPOSJ", NULL     关节坐标系的坐标位置读取。<br />
		/// "RJSEQ", NULL     读取当前的程序名，行编号，步编号。<br />
		/// "SAVEV", "7,000" 读取变量数据。字符串变量
		/// </remarks>
		/// <param name="command">命令的内容</param>
		/// <param name="commandData">命令数据内容</param>
		/// <returns>最终的结果内容，需要对IsSuccess进行验证</returns>
		[HslMqttApi( Description = "Read the device information according to the instructions. If the command data is empty, pass in null. Note that all commands do not have a newline character" )]
		public OperateResult<string> ReadByCommand( string command, string commandData )
		{
			this.pipeSocket.PipeLockEnter( );

			// 获取有用的网络通道，如果没有，就建立新的连接
			OperateResult<Socket> resultSocket = GetAvailableSocket( );
			if (!resultSocket.IsSuccess)
			{
				this.pipeSocket.IsSocketError = true;
				AlienSession?.Offline( );
				this.pipeSocket.PipeLockLeave( );
				return OperateResult.CreateFailedResult<string>( resultSocket );
			}

			// 先发送命令
			string sendCommand = string.IsNullOrEmpty( commandData ) ? $"HOSTCTRL_REQUEST {command} 0\r\n" : $"HOSTCTRL_REQUEST {command} {commandData.Length+1}\r\n";
			OperateResult<string> readCommand = ReadFromCoreServer( resultSocket.Content, sendCommand );
			if (!readCommand.IsSuccess)
			{
				this.pipeSocket.IsSocketError = true;
				AlienSession?.Offline( );
				this.pipeSocket.PipeLockLeave( );
				return OperateResult.CreateFailedResult<string>( readCommand );
			}

			// 检查命令是否返回成功的状态
			if (!readCommand.Content.StartsWith( "OK:" ))
			{
				if (!isPersistentConn) resultSocket.Content?.Close( );
				this.pipeSocket.PipeLockLeave( );
				return new OperateResult<string>( readCommand.Content.Remove( readCommand.Content.Length - 2 ) );
			}

			// 在必要的情况下发送命令数据
			if(!string.IsNullOrEmpty( commandData ))
			{
				byte[] send2 = Encoding.ASCII.GetBytes( $"{commandData}\r" );
				LogNet?.WriteDebug( ToString( ), StringResources.Language.Send + " : " + BasicFramework.SoftBasic.ByteToHexString( send2, ' ' ) );

				OperateResult sendResult2 = Send( resultSocket.Content, send2 );
				if (!sendResult2.IsSuccess)
				{
					resultSocket.Content?.Close( );
					this.pipeSocket.IsSocketError = true;
					AlienSession?.Offline( );
					this.pipeSocket.PipeLockLeave( );
					return OperateResult.CreateFailedResult<string>( sendResult2 );
				}
			}

			// 接收数据信息，先接收到\r为止，再根据实际情况决定是否接收\r
			OperateResult<byte[]> resultReceive2 =  ReceiveCommandLineFromSocket( resultSocket.Content, (byte)'\r', ReceiveTimeOut);
			if (!resultReceive2.IsSuccess)
			{
				this.pipeSocket.IsSocketError = true;
				AlienSession?.Offline( );
				this.pipeSocket.PipeLockLeave( );
				return OperateResult.CreateFailedResult<string>( resultReceive2 );
			}

			string commandDataReturn = Encoding.ASCII.GetString( resultReceive2.Content );
			if(string.IsNullOrEmpty( commandDataReturn ))
			{
				if (!isPersistentConn) resultSocket.Content?.Close( );
				this.pipeSocket.PipeLockLeave( );

				return new OperateResult<string>( "Return is Null" );
			}
			else if (commandDataReturn.StartsWith( "ERROR:" ))
			{
				if (!isPersistentConn) resultSocket.Content?.Close( );
				this.pipeSocket.PipeLockLeave( );
				Receive( resultSocket.Content, 1 );

				return Helper.YRCHelper.ExtraErrorMessage( commandDataReturn );
			}
			else if (commandDataReturn.StartsWith( "0000\r" ))
			{
				if (!isPersistentConn) resultSocket.Content?.Close( );
				Receive( resultSocket.Content, 1 );

				this.pipeSocket.PipeLockLeave( );
				return OperateResult.CreateSuccessResult( "0000" );
			}
			else
			{
				if (!isPersistentConn) resultSocket.Content?.Close( );

				this.pipeSocket.PipeLockLeave( );
				return OperateResult.CreateSuccessResult( commandDataReturn.Remove( commandDataReturn.Length - 1 ) );
			}
		}

		#endregion

		#region Async Override Read
#if !NET35 && !NET20

		/// <inheritdoc cref="ReadFromCoreServer(Socket, string)"/>
		protected async Task<OperateResult<string>> ReadFromCoreServerAsync( Socket socket, string send )
		{
			var read = await ReadFromCoreServerAsync( socket, Encoding.Default.GetBytes( send ) );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<string>( read );

			return OperateResult.CreateSuccessResult( Encoding.Default.GetString( read.Content ) );
		}

		/// <inheritdoc cref="ReadByCommand(string, string)"/>
		public async Task<OperateResult<string>> ReadByCommandAsync( string command, string commandData )
		{
			await Task.Run( new Action( ( ) => this.pipeSocket.PipeLockEnter( ) ) );

			// 获取有用的网络通道，如果没有，就建立新的连接
			OperateResult<Socket> resultSocket = await GetAvailableSocketAsync( );
			if (!resultSocket.IsSuccess)
			{
				this.pipeSocket.IsSocketError = true;
				AlienSession?.Offline( );
				this.pipeSocket.PipeLockLeave( );
				return OperateResult.CreateFailedResult<string>( resultSocket );
			}

			// 先发送命令
			string sendCommand = string.IsNullOrEmpty( commandData ) ? $"HOSTCTRL_REQUEST {command} 0\r\n" : $"HOSTCTRL_REQUEST {command} {commandData.Length + 1}\r\n";
			OperateResult<string> readCommand = await ReadFromCoreServerAsync( resultSocket.Content, sendCommand );
			if (!readCommand.IsSuccess)
			{
				this.pipeSocket.IsSocketError = true;
				AlienSession?.Offline( );
				this.pipeSocket.PipeLockLeave( );
				return OperateResult.CreateFailedResult<string>( readCommand );
			}

			// 检查命令是否返回成功的状态
			if (!readCommand.Content.StartsWith( "OK:" ))
			{
				if (!isPersistentConn) resultSocket.Content?.Close( );
				this.pipeSocket.PipeLockLeave( );
				return new OperateResult<string>( readCommand.Content.Remove( readCommand.Content.Length - 2 ) );
			}

			// 在必要的情况下发送命令数据
			if (!string.IsNullOrEmpty( commandData ))
			{
				byte[] send2 = Encoding.ASCII.GetBytes( $"{commandData}\r" );
				LogNet?.WriteDebug( ToString( ), StringResources.Language.Send + " : " + BasicFramework.SoftBasic.ByteToHexString( send2, ' ' ) );

				OperateResult sendResult2 = await SendAsync( resultSocket.Content, send2 );
				if (!sendResult2.IsSuccess)
				{
					resultSocket.Content?.Close( );
					this.pipeSocket.IsSocketError = true;
					AlienSession?.Offline( );
					this.pipeSocket.PipeLockLeave( );
					return OperateResult.CreateFailedResult<string>( sendResult2 );
				}
			}

			// 接收数据信息，先接收到\r为止，再根据实际情况决定是否接收\r
			OperateResult<byte[]> resultReceive2 = await ReceiveCommandLineFromSocketAsync( resultSocket.Content, (byte)'\r', ReceiveTimeOut );
			if (!resultReceive2.IsSuccess)
			{
				this.pipeSocket.IsSocketError = true;
				AlienSession?.Offline( );
				this.pipeSocket.PipeLockLeave( );
				return OperateResult.CreateFailedResult<string>( resultReceive2 );
			}

			string commandDataReturn = Encoding.ASCII.GetString( resultReceive2.Content );
			if (string.IsNullOrEmpty( commandDataReturn ))
			{
				if (!isPersistentConn) resultSocket.Content?.Close( );
				this.pipeSocket.PipeLockLeave( );

				return new OperateResult<string>( "Return is Null" );
			}
			else if (commandDataReturn.StartsWith( "ERROR:" ))
			{
				if (!isPersistentConn) resultSocket.Content?.Close( );
				this.pipeSocket.PipeLockLeave( );
				await ReceiveAsync( resultSocket.Content, 1 );

				return Helper.YRCHelper.ExtraErrorMessage( commandDataReturn );
			}
			else if (commandDataReturn.StartsWith( "0000\r" ))
			{
				if (!isPersistentConn) resultSocket.Content?.Close( );
				await ReceiveAsync( resultSocket.Content, 1 );

				this.pipeSocket.PipeLockLeave( );
				return OperateResult.CreateSuccessResult( "0000" );
			}
			else
			{
				if (!isPersistentConn) resultSocket.Content?.Close( );
				this.pipeSocket.PipeLockLeave( );

				return OperateResult.CreateSuccessResult( commandDataReturn.Remove( commandDataReturn.Length - 1 ) );
			}
		}
#endif
		#endregion

		#region Public Method

		#region Read Status

		/// <summary>
		/// 读取机器人的报警信息<br />
		/// Read the alarm information of the robot
		/// </summary>
		/// <returns>原始的报警信息</returns>
		[HslMqttApi( Description = "Read the alarm information of the robot" )]
		public OperateResult<string> ReadALARM( ) => ReadByCommand( "RALARM", null );

		/// <summary>
		/// 关节坐标系的坐标位置读取。<br />
		/// Read the coordinate data information of the robot
		/// </summary>
		/// <returns>原始的报警信息</returns>
		[HslMqttApi( Description = "Read the coordinate data information of the robot" )]
		public OperateResult<string> ReadPOSJ( ) => ReadByCommand( "RPOSJ", null );

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
			OperateResult<string> read = ReadByCommand( "RPOSC", $"{coordinate},{(hasExteralAxis ? "1" : "0")}" );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<YRCRobotData>( read );

			return OperateResult.CreateSuccessResult( new YRCRobotData( Type, read.Content ) );
		}

		/// <summary>
		/// 模式状态，循环状态，动作状态，报警错误状态，伺服状态的读取。<br />
		/// Reading of mode status, cycle status, action status, alarm error status, and servo status.
		/// </summary>
		/// <remarks>
		/// [0]: 单步
		/// [1]: 1循环
		/// [2]: 自动连续
		/// [3]: 运行中
		/// [4]: 运转中
		/// [5]: 示教
		/// [6]: 在线
		/// [7]: 命令模式
		/// [9]: 示教编程器HOLD中
		/// [10]: 外部HOLD中
		/// [11]: 命令HOLD中
		/// [12]: 发生警报
		/// [13]: 发生错误
		/// [14]: 伺服ON
		/// </remarks>
		/// <returns>状态信息</returns>
		[HslMqttApi( Description = "模式状态，循环状态，动作状态，报警错误状态，伺服状态的读取。" )]
		public OperateResult<bool[]> ReadStats( )
		{
			OperateResult<string> read = ReadByCommand( "RSTATS", null );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( read );

			return OperateResult.CreateSuccessResult( read.Content.ToStringArray<byte>( ).ToBoolArray( ) );
		}

		/// <summary>
		/// 读取当前的程序名，行编号，步编号。<br />
		/// Read the current program name, line number, and step number.
		/// </summary>
		/// <returns>读取结果</returns>
		[HslMqttApi( Description = "读取当前的程序名，行编号，步编号。" )]
		public OperateResult<string> ReadJSeq( ) => ReadByCommand( "RJSEQ", null );

		/// <summary>
		/// 读取指定用户的坐标数据。<br />
		/// Read the coordinate data of the specified user.
		/// </summary>
		/// <param name="frame">用户坐标编号，1-64</param>
		/// <returns>坐标值</returns>
		[HslMqttApi( Description = "读取指定用户的坐标数据。" )]
		public OperateResult<string> ReadUFrame( int frame ) => ReadByCommand( "RUFRAME", $"{frame}" );

		/// <summary>
		/// 读取机器人的字节型变量的数据，需要传入变量的编号<br />
		/// To read the data of the byte variable of the robot, the number of the variable needs to be passed in
		/// </summary>
		/// <param name="variableAdderss">变量的编号</param>
		/// <returns>包含是否成功的结果数据</returns>
		[HslMqttApi( Description = "读取机器人的字节型变量的数据，需要传入变量的编号。" )]
		public OperateResult<string> ReadByteVariable( string variableAdderss ) => ReadByCommand( "SAVEV", $"0,{variableAdderss}" );

		/// <summary>
		/// 读取机器人的整型变量的数据，需要传入变量的编号<br />
		/// To read the data of the integer variable of the robot, the number of the variable needs to be passed in
		/// </summary>
		/// <param name="variableAdderss">变量的编号</param>
		/// <returns>包含是否成功的结果数据</returns>
		[HslMqttApi( Description = "读取机器人的整型变量的数据，需要传入变量的编号" )]
		public OperateResult<string> ReadIntegerVariable( string variableAdderss ) => ReadByCommand( "SAVEV", $"1,{variableAdderss}" );

		/// <summary>
		/// 读取机器人的双精度整型变量的数据，需要传入变量的编号<br />
		/// To read the data of the double integer variable of the robot, the number of the variable needs to be passed in
		/// </summary>
		/// <param name="variableAdderss">变量的编号</param>
		/// <returns>包含是否成功的结果数据</returns>
		[HslMqttApi( Description = "读取机器人的双精度整型变量的数据，需要传入变量的编号" )]
		public OperateResult<string> ReadDoubleIntegerVariable( string variableAdderss ) => ReadByCommand( "SAVEV", $"2,{variableAdderss}" );

		/// <summary>
		/// 读取机器人的实数变量的数据，需要传入变量的编号<br />
		/// To read the data of the real variable of the robot, the number of the variable needs to be passed in
		/// </summary>
		/// <param name="variableAdderss">变量的编号</param>
		/// <returns>包含是否成功的结果数据</returns>
		[HslMqttApi( Description = "读取机器人的实数变量的数据，需要传入变量的编号" )]
		public OperateResult<string> ReadRealVariable( string variableAdderss ) => ReadByCommand( "SAVEV", $"3,{variableAdderss}" );

		/// <summary>
		/// 读取机器人的字符串变量的数据，需要传入变量的编号<br />
		/// To read the data of the string variable of the robot, the number of the variable needs to be passed in
		/// </summary>
		/// <param name="variableAdderss">变量的编号</param>
		/// <returns>包含是否成功的结果数据</returns>
		[HslMqttApi( Description = "读取机器人的字符串变量的数据，需要传入变量的编号" )]
		public OperateResult<string> ReadStringVariable( string variableAdderss ) => ReadByCommand( "SAVEV", $"7,{variableAdderss}" );

		#endregion

		#region System Control

		/// <summary>
		/// 进行HOLD 的 ON/OFF 操作，状态参数 False: OFF操作，True: ON操作<br />
		/// Perform HOLD ON operation, False: OFF，True: ON
		/// </summary>
		/// <param name="status">状态参数 False: OFF操作，True: ON操作</param>
		/// <returns>是否成功的HOLD操作</returns>
		[HslMqttApi( Description = "进行HOLD 的 ON/OFF 操作，状态参数 False: OFF，True: ON" )]
		public OperateResult Hold( bool status ) => ReadByCommand( "HOLD", status ? "1" : "0" );

		/// <summary>
		/// 对机械手的报警进行复位<br />
		/// Reset the alarm of the manipulator
		/// </summary>
		/// <remarks>
		/// 传输报警仅可在示教编程器上进行复位。
		/// </remarks>
		/// <returns>是否复位成功</returns>
		[HslMqttApi( Description = "对机械手的报警进行复位" )]
		public OperateResult Reset( ) => ReadByCommand( "RESET", null );

		/// <summary>
		/// 进行错误取消<br />
		/// Make an error cancellation
		/// </summary>
		/// <returns>是否取消成功</returns>
		[HslMqttApi( Description = "进行错误取消" )]
		public OperateResult Cancel( ) => ReadByCommand( "CANCEL", null );

		/// <summary>
		/// 选择模式。模式编号为1:示教模式，2:再现模式<br />
		/// Choose a mode. The mode number is 1: teaching mode, 2: reproduction mode
		/// </summary>
		/// <param name="number">模式编号为1:示教模式，2:再现模式</param>
		/// <remarks>
		/// MODE 命令，是在「操作条件」 画面中获得外部模式切换的许可后可以使用。
		/// </remarks>
		/// <returns>模式是否选择成功</returns>
		[HslMqttApi( Description = "选择模式。模式编号为1:示教模式，2:再现模式" )]
		public OperateResult Mode( int number ) => ReadByCommand( "MODE", number.ToString( ) );

		/// <summary>
		/// 选择循环。循环编号 1:步骤，2:1循环，3:连续自动<br />
		/// Choose loop. Cycle number 1: step, 2:1 cycle, 3: continuous automatic
		/// </summary>
		/// <param name="number">循环编号 1:步骤，2:1循环，3:连续自动</param>
		/// <returns>循环是否选择成功</returns>
		[HslMqttApi( Description = "选择循环。循环编号 1:步骤，2:1循环，3:连续自动" )]
		public OperateResult Cycle( int number ) => ReadByCommand( "CYCLE", number.ToString( ) );

		/// <summary>
		/// 进行伺服电源的ON/OFF操作，状态参数 False: OFF，True: ON<br />
		/// Carry out the ON/OFF operation of the servo power, the status parameter False: OFF，True: ON
		/// </summary>
		/// <param name="status">状态参数 False: OFF，True: ON</param>
		/// <remarks>
		/// 通过此命令伺服ON的时候，请连接机器人专用端子台（ MTX） 的外部伺服ON（ EXSVON）信号的29和 30 。</remarks>
		/// <returns>是否伺服电源是否成功</returns>
		[HslMqttApi( Description = "进行伺服电源的ON/OFF操作，状态参数 False: OFF，True: ON" )]
		public OperateResult Svon( bool status ) => ReadByCommand( "SVON", status ? "1" : "0" );

		/// <summary>
		/// 设定示教编程器和 I/O的操作信号的联锁。 状态参数 False: OFF，True: ON<br />
		/// Set the interlock between the programming pendant and the operation signal of I/O. Status parameter False: OFF，True: ON
		/// </summary>
		/// <param name="status">状态参数 False: OFF，True: ON</param>
		/// <remarks>
		/// 联锁为ON时，仅可执行以下操作。
		/// <list type="number">
		/// <item>示教编程器的非常停止</item>
		/// <item>Ｉ /O 的模式切换， 外部启动， 外部伺服ON，循环切换， I/O 禁止、 PP/PANEL 禁止、 主程序调出以外的输入信号</item>
		/// </list>
		/// 示教编程器在编辑中或者通过其他的功能访问文件时，不能使用HLOCK.
		/// </remarks>
		/// <returns>是否设定成功</returns>
		[HslMqttApi( Description = "设定示教编程器和 I/O的操作信号的联锁。 状态参数 False: OFF，True: ON" )]
		public OperateResult HLock( bool status ) =>  ReadByCommand( "HLOCK", status ? "1" : "0" );

		/// <summary>
		/// 接受消息数据时， 在YRC1000的示教编程器的远程画面下显示消息若。若不是远程画面时，强制切换到远程画面。显示MDSP命令的消息。<br />
		/// When receiving message data, a message is displayed on the remote screen of the YRC1000 programming pendant. 
		/// If it is not a remote screen, it is forced to switch to the remote screen. Display the message of the MDSP command.
		/// </summary>
		/// <param name="message">显示信息（最大 30byte 字符串）</param>
		/// <returns>是否显示成功</returns>
		[HslMqttApi( Description = "接受消息数据时， 在YRC1000的示教编程器的远程画面下显示消息若。若不是远程画面时，强制切换到远程画面。显示MDSP命令的消息。" )]
		public OperateResult MSDP( string message ) => ReadByCommand( "MDSP", message );

		/// <summary>
		/// 开始程序。操作时指定程序名时，此程序能附带对应主程序，则从该程序的开头开始执行。如果没有指定，则从前行开始执行<br />
		/// Start the program. When the program name is specified during operation, the program can be accompanied by the corresponding main program, 
		/// and the execution starts from the beginning of the program. If not specified, execute from the previous line
		/// </summary>
		/// <param name="programName">开始动作程序名称，可以省略</param>
		/// <returns>是否启动成功</returns>
		[HslMqttApi( Description = "开始程序。操作时指定程序名时，此程序能附带对应主程序，则从该程序的开头开始执行。如果没有指定，则从前行开始执行" )]
		public OperateResult Start( string programName = null ) => ReadByCommand( "START", programName );

		/// <summary>
		/// 删除指定的程序。指定「*」 时， 删除当前登录的所有程序。指定「 删除程序名称」 时，仅删除指定的程序。<br />
		/// Delete the specified program. When "*" is specified, all currently registered programs will be deleted. 
		/// When "delete program name" is specified, only the specified program will be deleted.
		/// </summary>
		/// <param name="programName">删除的程序名称，如果设置为「*」时，删除当前登录的所有程序。</param>
		/// <returns>是否删除成功</returns>
		[HslMqttApi( Description = "删除指定的程序。指定「*」 时， 删除当前登录的所有程序。指定「 删除程序名称」 时，仅删除指定的程序。" )]
		public OperateResult Delete( string programName = null ) => ReadByCommand( "DELETE", programName );

		/// <summary>
		/// 指定的程序设定为主程序。设定主程序的同时执行程序也被设定。<br />
		/// The specified program is set as the main program. The execution program is also set when the main program is set.
		/// </summary>
		/// <param name="programName">设定的程序名称</param>
		/// <returns>是否设定成功</returns>
		[HslMqttApi( Description = "指定的程序设定为主程序。设定主程序的同时执行程序也被设定。" )]
		public OperateResult SetMJ( string programName = null ) => ReadByCommand( "SETMJ", programName );

		/// <summary>
		/// 设定执行程序的名称和行编号。<br />
		/// Set the name and line number of the executed program.
		/// </summary>
		/// <param name="programName">设定程序名称</param>
		/// <param name="line">设定行编号（ 0 ～ 9999）</param>
		/// <returns>是否设定成功</returns>
		[HslMqttApi( Description = "设定执行程序的名称和行编号。" )]
		public OperateResult JSeq( string programName, int line ) => ReadByCommand( "JSEQ", $"{programName},{line}" );

		/// <summary>
		/// 向指定的坐标系位置进行关节动作。其中没有外部轴的系统， 7-12外部轴的值设定为「0」<br />
		/// Perform joint motions to the specified coordinate system position.
		/// where there is no external axis system, the value of 7-12 external axis is set to "0"
		/// </summary>
		/// <param name="robotData">机器的的数据信息</param>
		/// <remarks>
		/// 其中形态数据由6个bool数组组成，每个bool含义参考参数说明，0表示 <c>False</c>，1表示 <c>True</c>
		/// </remarks>
		/// <returns>是否动作成功</returns>
		public OperateResult MoveJ( YRCRobotData robotData ) => ReadByCommand( "MOVJ", robotData.ToWriteString( Type ) );

		#endregion

		#region IO ReadWrite

		/// <summary>
		/// 读取I/O 信号。 I/O 数据是每8个点输出，所以读出接点数是8的倍数。<br />
		/// Read I/O signal. I/O data is output every 8 points, so the number of read contacts is a multiple of 8.
		/// </summary>
		/// <param name="address">读出开始点编号</param>
		/// <param name="length">读出的接点数</param>
		/// <returns>读取的结果点位信息</returns>
		[HslMqttApi( Description = "读取I/O 信号。 I/O 数据是每8个点输出，所以读出接点数是8的倍数。" )]
		public OperateResult<bool[]> IORead( int address, int length )
		{
			OperateResult<string> read = ReadByCommand( "IOREAD", $"{address},{length}" );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( read );

			byte[] buffer = read.Content.ToStringArray<byte>( );
			return OperateResult.CreateSuccessResult( buffer.ToBoolArray( ) );
		}

		/// <summary>
		/// 写入I/O信号状态，写入接点数请指定8的倍数。IO 信号的网络写入仅可是（ #27010 ～ #29567）。<br />
		/// To write I/O signal status, please specify a multiple of 8 for the number of write contacts. 
		/// The network write of IO signal is only available (#27010 to #29567).
		/// </summary>
		/// <param name="address">写入开始接点编号</param>
		/// <param name="value">写入的bool值，写入接点数请指定8的倍数。</param>
		/// <returns>是否写入成功</returns>
		[HslMqttApi( Description = "写入I/O信号状态，写入接点数请指定8的倍数。IO 信号的网络写入仅可是（ #27010 ～ #29567）。" )]
		public OperateResult IOWrite( int address, bool[] value )
		{
			if (value == null || value.Length % 8 != 0) return new OperateResult( $"Parameter [{nameof( value )}] can't be null or length must be 8 *N" );
			byte[] buffer = value.ToByteArray( );

			StringBuilder sb = new StringBuilder( $"{address},{value.Length}" );
			for (int i = 0; i < buffer.Length; i++)
			{
				sb.Append( "," );
				sb.Append( buffer[i].ToString( ) );
			}
			return ReadByCommand( "IOWRITE", sb.ToString( ) );
		}

		#endregion

		#endregion

		#region Async Public Method
#if !NET35 && !NET20
		/// <inheritdoc cref="ReadALARM"/>
		public async Task<OperateResult<string>> ReadALARMAsync( ) => await ReadByCommandAsync( "RALARM", null );

		/// <inheritdoc cref="ReadPOSJ"/>
		public async Task<OperateResult<string>> ReadPOSJAsync( ) => await ReadByCommandAsync( "RPOSJ", null );

		/// <inheritdoc cref="ReadPOSC(int, bool)"/>
		public async Task<OperateResult<YRCRobotData>> ReadPOSCAsync( int coordinate, bool hasExteralAxis )
		{
			OperateResult<string> read = await ReadByCommandAsync( "RPOSC", $"{coordinate},{(hasExteralAxis ? "1" : "0")}" );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<YRCRobotData>( read );

			return OperateResult.CreateSuccessResult( new YRCRobotData( Type, read.Content ) );
		}

		/// <inheritdoc cref="ReadStats"/>
		public async Task<OperateResult<bool[]>> ReadStatsAsync( )
		{
			OperateResult<string> read = await ReadByCommandAsync( "RSTATS", null );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( read );

			return OperateResult.CreateSuccessResult( read.Content.ToStringArray<byte>( ).ToBoolArray( ) );
		}

		/// <inheritdoc cref="ReadJSeq"/>
		public async Task<OperateResult<string>> ReadJSeqAsync( ) => await ReadByCommandAsync( "RJSEQ", null );

		/// <inheritdoc cref="ReadUFrame(int)"/>
		public async Task<OperateResult<string>> ReadUFrameAsync( int frame ) => await ReadByCommandAsync( "RUFRAME", $"{frame}" );

		/// <inheritdoc cref="ReadByteVariable(string)"/>
		public async Task<OperateResult<string>> ReadByteVariableAsync( string variableAdderss ) => await ReadByCommandAsync( "SAVEV", $"0,{variableAdderss}" );

		/// <inheritdoc cref="ReadIntegerVariable(string)"/>
		public async Task<OperateResult<string>> ReadIntegerVariableAsync( string variableAdderss ) => await ReadByCommandAsync( "SAVEV", $"1,{variableAdderss}" );

		/// <inheritdoc cref="ReadDoubleIntegerVariable(string)"/>
		public async Task<OperateResult<string>> ReadDoubleIntegerVariableAsync( string variableAdderss ) => await ReadByCommandAsync( "SAVEV", $"2,{variableAdderss}" );

		/// <inheritdoc cref="ReadRealVariable(string)"/>
		public async Task<OperateResult<string>> ReadRealVariableAsync( string variableAdderss ) => await ReadByCommandAsync( "SAVEV", $"3,{variableAdderss}" );

		/// <inheritdoc cref="ReadStringVariable(string)"/>
		public async Task<OperateResult<string>> ReadStringVariableAsync( string variableAdderss ) => await ReadByCommandAsync( "SAVEV", $"7,{variableAdderss}" );

		/// <inheritdoc cref="Hold(bool)"/>
		public async Task<OperateResult> HoldAsync( bool status ) => await ReadByCommandAsync( "HOLD", status ? "1" : "0" );

		/// <inheritdoc cref="Reset"/>
		public async Task<OperateResult> ResetAsync( ) => await ReadByCommandAsync( "RESET", null );

		/// <inheritdoc cref="Cancel"/>
		public async Task<OperateResult> CancelAsync( ) => await ReadByCommandAsync( "CANCEL", null );

		/// <inheritdoc cref="Mode(int)"/>
		public async Task<OperateResult> ModeAsync( int number ) => await ReadByCommandAsync( "MODE", number.ToString( ) );

		/// <inheritdoc cref="Cycle(int)"/>
		public async Task<OperateResult> CycleAsync( int number ) => await ReadByCommandAsync( "CYCLE", number.ToString( ) );

		/// <inheritdoc cref="Svon(bool)"/>
		public async Task<OperateResult> SvonAsync( bool status ) => await ReadByCommandAsync( "SVON", status ? "1" : "0" );

		/// <inheritdoc cref="HLock(bool)"/>
		public async Task<OperateResult> HLockAsync( bool status ) => await ReadByCommandAsync( "HLOCK", status ? "1" : "0" );

		/// <inheritdoc cref="MSDP(string)"/>
		public async Task<OperateResult> MSDPAsync( string message ) => await ReadByCommandAsync( "MDSP", message );

		/// <inheritdoc cref="Start(string)"/>
		public async Task<OperateResult> StartAsync( string programName = null ) => await ReadByCommandAsync( "START", programName );

		/// <inheritdoc cref="Delete(string)"/>
		public async Task<OperateResult> DeleteAsync( string programName = null ) => await ReadByCommandAsync( "DELETE", programName );

		/// <inheritdoc cref="SetMJ(string)"/>
		public async Task<OperateResult> SetMJAsync( string programName = null ) => await ReadByCommandAsync( "SETMJ", programName );

		/// <inheritdoc cref="JSeq(string, int)"/>
		public async Task<OperateResult> JSeqAsync( string programName, int line ) => await ReadByCommandAsync( "JSEQ", $"{programName},{line}" );

		/// <inheritdoc cref="MoveJ(YRCRobotData)"/>
		public async Task<OperateResult> MoveJAsync( YRCRobotData robotData ) => await ReadByCommandAsync( "MOVJ", robotData.ToWriteString( Type ) );

		/// <inheritdoc cref="IORead(int, int)"/>
		public async Task<OperateResult<bool[]>> IOReadAsync( int address, int length )
		{
			OperateResult<string> read = await ReadByCommandAsync( "IOREAD", $"{address},{length}" );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( read );

			byte[] buffer = read.Content.ToStringArray<byte>( );
			return OperateResult.CreateSuccessResult( buffer.ToBoolArray( ) );
		}

		/// <inheritdoc cref="IOWrite(int, bool[])"/>
		public async Task<OperateResult> IOWriteAsync( int address, bool[] value )
		{
			if (value == null || value.Length % 8 != 0) return new OperateResult( $"Parameter [{nameof( value )}] can't be null or length must be 8 *N" );
			byte[] buffer = value.ToByteArray( );

			StringBuilder sb = new StringBuilder( $"{address},{value.Length}" );
			for (int i = 0; i < buffer.Length; i++)
			{
				sb.Append( "," );
				sb.Append( buffer[i].ToString( ) );
			}
			return await ReadByCommandAsync( "IOWRITE", sb.ToString( ) );
		}

#endif
		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString( ) => $"YRC1000TcpNet Robot[{IpAddress}:{Port}]";

		#endregion
	}
}
