using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HslCommunication.ModBus;
using HslCommunication;
using HslCommunication.BasicFramework;
#if !NET20 && !NET35
using System.Threading.Tasks;
#endif

namespace HslCommunication.Robot.Estun
{
	/// <summary>
	/// 一个埃斯顿的机器人的通信类，底层使用的是ModbusTCP协议，支持读取简单机器人数据，并且支持对机器人进行一些操作。<br />
	/// A communication class of Estun's robot, the bottom layer uses the ModbusTCP protocol, supports reading simple robot data, and supports some operations on the robot.
	/// </summary>
	public class EstunTcpNet : ModbusTcpNet
	{
		#region Constructor

		/// <summary>
		/// 实例化一个Modbus-Tcp协议的客户端对象<br />
		/// Instantiate a client object of the Modbus-Tcp protocol
		/// </summary>
		public EstunTcpNet( )
		{
			this.timer = new System.Threading.Timer( ThreadTimerTick, null, 3000, 10_000 );
			this.ByteTransform.DataFormat = Core.DataFormat.CDAB;
		}

		/// <summary>
		/// 指定服务器地址，端口号，客户端自己的站号来初始化<br />
		/// Specify the server address, port number, and client's own station number to initialize
		/// </summary>
		/// <param name="ipAddress">服务器的Ip地址</param>
		/// <param name="port">服务器的端口号</param>
		/// <param name="station">客户端自身的站号</param>
		public EstunTcpNet( string ipAddress, int port = 502, byte station = 0x01 ) : this( )
		{
			this.IpAddress = ipAddress;
			this.Port      = port;
		}

		private void ThreadTimerTick( object obj )
		{
			OperateResult<ushort> read = ReadUInt16( "0" );
			if (read.IsSuccess)
			{

			}
		}

		#endregion

		/// <summary>
		/// 读取埃斯顿的机器人的数据
		/// </summary>
		/// <returns>机器人数据</returns>
		public OperateResult<EstunData> ReadRobotData( )
		{
			OperateResult<byte[]> read = Read( "0", 100 );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<EstunData>( read );

			return OperateResult.CreateSuccessResult( new EstunData( read.Content, this.ByteTransform ) );
		}

		private OperateResult ExecuteCommand( short command )
		{
			// QQ:305758752 有执行命令的方法
			// 1. 确保40100及40052的地址是0
			OperateResult<short> check40100 = ReadInt16( "99" );
			if (!check40100.IsSuccess) return check40100;

			OperateResult<short> check40052 = ReadInt16( "51" );
			if (!check40052.IsSuccess) return check40052;

			if (check40100.Content != 0) return new OperateResult( "Step1: check 40100 value 0 failed, actual is " + check40100.Content );
			if (check40052.Content != 0) return new OperateResult( "Step1: check 40052 value 0 failed, actual is " + check40052.Content );

			// 打开下发权限
			OperateResult openAuthority = Write( "99", (short)0x11 );
			if (!openAuthority.IsSuccess) return new OperateResult( "Step2: write 40100 0x11 failed, " + openAuthority.Message );

			// 等待40019变为0x801
			int tick = 0;
			while (true)
			{
				OperateResult<short> status = ReadInt16( "18" );
				if (!status.IsSuccess) return new OperateResult( "Step3: read 40019 failed, " + status.Message );

				if (status.Content == 0x801) break;
				tick++;
				if (tick >= 20) return new OperateResult( "Step3: wait 40019 0x801 timeout, timeout is 2s" );
				System.Threading.Thread.Sleep( 100 );
			}

			OperateResult writeCmd = Write( "51", command );
			if (!writeCmd.IsSuccess) return new OperateResult( "Step4: write cmd to 40052 failed, " + writeCmd.Message );

			System.Threading.Thread.Sleep( 100 );

			// 等待40019改变对应的状态，暂时没有进行额外的判断
			OperateResult<short> cmdCheck = ReadInt16( "18" );
			if (!cmdCheck.IsSuccess) return new OperateResult( "Step5: read cmd status failed, " + cmdCheck.Message );

			// 清空40100和40052
			OperateResult clear40100 = Write( "99", (short)0 );
			if (!clear40100.IsSuccess) return new OperateResult( "Step6: clear 40100 failed, " + clear40100.Message );

			OperateResult clear40052 = Write( "51", (short)0 );
			if (!clear40052.IsSuccess) return new OperateResult( "Step6: clear 40052 failed, " + clear40052.Message );

			return OperateResult.CreateSuccessResult( );
		}

		/// <summary>
		/// 机器人程序启动
		/// </summary>
		/// <returns>是否启动成功</returns>
		public OperateResult RobotStartPrograme( ) => ExecuteCommand( 0x04 );

		/// <summary>
		/// 机器人程序停止
		/// </summary>
		/// <returns>是否停止成功</returns>
		public OperateResult RobotStopPrograme( ) => ExecuteCommand( 0x08 );

		/// <summary>
		/// 机器人的错误进行复位
		/// </summary>
		/// <returns>是否重置了错误</returns>
		public OperateResult RobotResetError( ) => ExecuteCommand( 0x10 );

		/// <summary>
		/// 机器人重新装载程序名
		/// </summary>
		/// <param name="projectName">程序的名称</param>
		/// <returns>是否装载成功</returns>
		public OperateResult RobotLoadProject( string projectName )
		{
			byte[] name = SoftBasic.ArrayExpandToLength( Encoding.ASCII.GetBytes( projectName ), 20 );
			OperateResult write = Write( "53", name );
			if (!write.IsSuccess) return write;

			return ExecuteCommand( 0x80 );
		}

		/// <summary>
		/// 机器人卸载程序名
		/// </summary>
		/// <returns>是否卸载成功</returns>
		public OperateResult RobotUnregisterProject() => ExecuteCommand( 0x100 );

		/// <summary>
		/// 机器人设置全局速度值
		/// </summary>
		/// <param name="value">全局速度值</param>
		/// <returns>是否设置成功</returns>
		public OperateResult RobotSetGlobalSpeedValue( short value )
		{
			OperateResult write = Write( "52", value );
			if (!write.IsSuccess) return write;

			return ExecuteCommand( 0x200 );
		}

		/// <summary>
		/// 重置机器人的命令状态
		/// </summary>
		/// <returns>是否操作成功</returns>
		public OperateResult RobotCommandStatusRestart( ) => ExecuteCommand( 0x400 );

		#region Private Member

		private System.Threading.Timer timer;                    // 确保心跳

		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString( ) => $"EstunTcpNet[{IpAddress}:{Port}]";

		#endregion
	}
}
