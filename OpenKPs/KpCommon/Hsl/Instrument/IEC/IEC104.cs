using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HslCommunication;
using HslCommunication.Core.Net;
using HslCommunication.Core;
using HslCommunication.Core.IMessage;
using System.Net.Sockets;
using HslCommunication.BasicFramework;
using System.Threading;
using HslCommunication.Secs.Types;
#if !NET35 && !NET20
using System.Threading.Tasks;
#endif

namespace HslCommunication.Instrument.IEC
{
	/// <summary>
	/// IEC104规约实现的电力协议
	/// </summary>
	public class IEC104 : NetworkDeviceBase
	{
		#region Constructor

		/// <summary>
		/// 实例化IEC104协议的通讯对象<br />
		/// Instantiate the communication object of the IEC104 protocol
		/// </summary>
		public IEC104( )
		{
			this.WordLength            = 2;
			this.ByteTransform         = new RegularByteTransform( );
			this.sendIncrementCount    = new SoftIncrementCount( short.MaxValue, 0 );
		}

		/// <summary>
		/// 指定ip地址和端口号来实例化一个默认的对象<br />
		/// Specify the IP address and port number to instantiate a default object
		/// </summary>
		/// <param name="ipAddress">IEC104的Ip地址</param>
		/// <param name="port">IEC104的端口, 默认是2404端口</param>
		public IEC104( string ipAddress, int port = 2404 ) : this( )
		{
			this.IpAddress = ipAddress;
			this.Port      = port;
		}

		/// <inheritdoc/>
		protected override INetMessage GetNewNetMessage( ) => new IEC104Message( );

		#endregion

		#region Double Mode Override

		/// <inheritdoc/>
		protected override OperateResult InitializationOnConnect( Socket socket )
		{
			// 激活操作
			OperateResult<byte[]> start = ReadFromCoreServer( socket, Helper.IECHelper.BuildFrameUMessage( Helper.IECHelper.IEC104ControlStartDT ) );
			if (!start.IsSuccess) return start;

			this.UseServerActivePush = true;
			// 发送ID和接收ID，重置0操作
			this.sendIncrementCount.ResetCurrentValue( 0 );
			this.receiveIncrementCount = 0;
			return base.InitializationOnConnect( socket );
		}
		#endregion

		#region Async Double Mode Override
#if !NET35 && !NET20
		/// <inheritdoc/>
		protected override async Task<OperateResult> InitializationOnConnectAsync( Socket socket )
		{
			// 激活操作
			OperateResult<byte[]> start = await ReadFromCoreServerAsync( socket, Helper.IECHelper.BuildFrameUMessage( Helper.IECHelper.IEC104ControlStartDT ) );
			if (!start.IsSuccess) return start;

			this.UseServerActivePush = true;
			// 发送ID和接收ID，重置0操作
			this.sendIncrementCount.ResetCurrentValue( 0 );
			this.receiveIncrementCount = 0;
			return await base.InitializationOnConnectAsync( socket );
		}
#endif
		#endregion

		#region Receive IEC

		/// <inheritdoc/>
		protected override bool DecideWhetherQAMessage(Socket socket, OperateResult<byte[]> receive)
		{
			if (!receive.IsSuccess) return false;

			byte[] response = receive.Content;
			// 解析数据并且操作
			if (response.Length < 6) return false;
			// 判断三种报文体系
			if (response[2] == 0x01 && response[3] == 0x00)
			{
				// S帧消息

			}
			else if ((response[2] & 0x01) == 0x01)
			{
				// U帧消息，如果是测试帧
				byte[] send = Helper.IECHelper.BuildFrameUMessage(0x83);

				LogNet?.WriteDebug(ToString(), StringResources.Language.Send + " : " + (LogMsgFormatBinary ? send.ToHexString(' ') : SoftBasic.GetAsciiStringRender(send)));
				if (response[2] == 0x43) Send(socket, send);
			}
			else
			{
				// I帧消息
				int sendID    = BitConverter.ToUInt16(response, 2) / 2;
				int receiveID = BitConverter.ToUInt16(response, 4) / 2;

				if (this.receiveIncrementCount == sendID)
				{
					this.receiveIncrementCount++;
					// 回发S帧消息
					byte[] send = Helper.IECHelper.BuildFrameSMessage(this.receiveIncrementCount);
					LogNet?.WriteDebug(ToString(), StringResources.Language.Send + " : " + (LogMsgFormatBinary ? send.ToHexString(' ') : SoftBasic.GetAsciiStringRender(send)));

					Send(socket, send);
				}
				else
				{
					// 消息号对不上，应该断开重连

				}
				// 回发确认帧
				if (receiveIncrementCount > short.MaxValue) receiveIncrementCount = 0;

				OnIEC104MessageReceived?.Invoke(this, new IEC104MessageEventArgs(response.RemoveBegin(6)));
			}
			return false;  // base.DecideWhetherQAMessage( socket, response );
		}

		#endregion

		#region Public Method

		/// <summary>
		/// 以I消息的格式发送传入的原始字节数据，传入的消息为asdu信息
		/// </summary>
		/// <param name="asdu">ASDU报文信息</param>
		/// <returns>是否发送成功</returns>
		public OperateResult SendFrameIMessage( byte[] asdu )
		{
			int sendID   = (int)this.sendIncrementCount.GetCurrentValue( );
			int reciveID = this.receiveIncrementCount;
			byte[] send = Helper.IECHelper.BuildFrameIMessage( sendID, reciveID, asdu[0], asdu[1],
				ByteTransform.TransUInt16( asdu, 2 ), ByteTransform.TransUInt16( asdu, 4 ), asdu.RemoveBegin( 6 ) );

			LogNet?.WriteDebug(ToString(), StringResources.Language.Send + " : " + (LogMsgFormatBinary ? send.ToHexString(' ') : SoftBasic.GetAsciiStringRender(send)));

			return Send( pipeSocket.Socket, send );
		}

		/// <summary>
		/// 向设备发送U帧消息的报文信息，传入功能码，STARTDT: 0x07, STOPDT: 0x13; TESTFR: 0x43
		/// </summary>
		/// <param name="controlField">功能码，STARTDT: 0x07, STOPDT: 0x13; TESTFR: 0x43</param>
		/// <returns>是否发送成功</returns>
		public OperateResult SendFrameUMessage(byte controlField)
		{
			byte[] send = Helper.IECHelper.BuildFrameUMessage(controlField);

			LogNet?.WriteDebug(ToString(), StringResources.Language.Send + " : " + (LogMsgFormatBinary ? send.ToHexString(' ') : SoftBasic.GetAsciiStringRender(send)));
			return Send(pipeSocket.Socket, send);
		}

		#endregion

		#region Event Handle

		/// <summary>
		/// 当接收到了IEC104的消息触发的事件
		/// </summary>
		public event EventHandler<IEC104MessageEventArgs> OnIEC104MessageReceived;

		#endregion

		#region Private Member

		private readonly SoftIncrementCount sendIncrementCount;                 // 用于发送的自增消息的对象
		private int receiveIncrementCount = 0;                                  // 用于接收的自增消息的对象

		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString() => $"IEC104[{IpAddress}:{Port}]";

		#endregion

		#region TypeID

		/// <summary>
		/// 类型信息资源
		/// </summary>
		public class TypeID
		{
			/// <summary>
			/// 单点遥信，带品质描述，不带时标
			/// </summary>
			public const byte M_SP_NA_1 = 0x01;

			/// <summary>
			/// 双点遥信，带品质描述，不带时标
			/// </summary>
			public const byte M_DP_NA_1 = 0x03;

			/// <summary>
			/// 步位置信息，带品质描述，不带时标
			/// </summary>
			public const byte M_ST_NA_1 = 0x05;

			/// <summary>
			/// 32比特串，带品质描述，不带时标
			/// </summary>
			public const byte M_BO_NA_1 = 0x07;

			/// <summary>
			/// 归一化遥测值，带品质描述，不带时标
			/// </summary>
			public const byte M_ME_NA_1 = 0x09;

			/// <summary>
			/// 标度化遥测值，带品质描述，不带时标
			/// </summary>
			public const byte M_ME_NB_1 = 11;

			/// <summary>
			/// 短浮点遥测值，带品质描述，不带时标
			/// </summary>
			public const byte M_ME_NC_1 = 13;

			/// <summary>
			/// 累计量，带品质描述，不带时标
			/// </summary>
			public const byte M_IT_NA_1 = 15;
			
			/// <summary>
			/// 成组单点遥信，只带变位标志
			/// </summary>
			public const byte M_PS_NA_1 = 20;

			/// <summary>
			/// 归一化遥测值，不带品质描述，不带时标
			/// </summary>
			public const byte M_ME_ND_1 = 21;

			/// <summary>
			/// 单点遥信，带品质描述，带绝对时标
			/// </summary>
			public const byte M_SP_TB_1 = 30;

			/// <summary>
			/// 双点遥信，带品质描述，带绝对时标
			/// </summary>
			public const byte M_DP_TB_1 = 31;

			/// <summary>
			/// 步位置信息，带品质描述，带绝对时标
			/// </summary>
			public const byte M_ST_TB_1 = 32;

			/// <summary>
			/// 32比特串，带品质描述，带绝对时标
			/// </summary>
			public const byte M_BO_TB_1 = 33;

			/// <summary>
			/// 归一化遥测值，带品质描述，带绝对时标
			/// </summary>
			public const byte M_ME_TD_1 = 34;

			/// <summary>
			/// 标度化遥测值，带品质描述，带绝对时标
			/// </summary>
			public const byte M_ME_TE_1 = 35;

			/// <summary>
			/// 短浮点遥测值，带品质描述，带绝对时标
			/// </summary>
			public const byte M_ME_TF_1 = 36;

			/// <summary>
			/// 累计量，带品质描述，带绝对时标
			/// </summary>
			public const byte M_IT_TB_1 = 37;

			/// <summary>
			/// 单点遥控，一个报文只有一个遥控信息体，不带时标
			/// </summary>
			public const byte C_SC_NA_1 = 45;

			/// <summary>
			/// 双点遥控，一个报文只有一个遥控信息体，不带时标
			/// </summary>
			public const byte C_DC_NA_1 = 46;

			/// <summary>
			/// 升降遥控，一个报文只有一个遥控信息体，不带时标
			/// </summary>
			public const byte C_RC_NA_1 = 47;

			/// <summary>
			/// 归一化设定值，一个报文只有一个设定值，不带时标
			/// </summary>
			public const byte C_SE_NA_1 = 48;

			/// <summary>
			/// 标度化设定值，一个报文只有一个设定值，不带时标
			/// </summary>
			public const byte C_SE_NB_1 = 49;

			/// <summary>
			/// 短浮点设定值，一个报文只有一个设定值，不带时标
			/// </summary>
			public const byte C_SE_NC_1 = 50;

			/// <summary>
			/// 32比特串设定，一个报文只有一个设定值，不带时标
			/// </summary>
			public const byte C_SE_ND_1 = 51;

			/// <summary>
			/// 单点遥控，一个报文只有一个遥控信息体，带时标
			/// </summary>
			public const byte C_SE_TA_1 = 58;

			/// <summary>
			/// 双点遥控，一个报文只有一个遥控信息体，带时标
			/// </summary>
			public const byte C_SE_TB_1 = 59;

			/// <summary>
			/// 升降遥控，一个报文只有一个遥控信息体，带时标
			/// </summary>
			public const byte C_SE_TC_1 = 60;

			/// <summary>
			/// 归一化设定值，一个报文只有一个设定值，带时标
			/// </summary>
			public const byte C_SE_TD_1 = 61;

			/// <summary>
			/// 标度化设定值，一个报文只有一个设定值，带时标
			/// </summary>
			public const byte C_SE_TE_1 = 62;

			/// <summary>
			/// 短浮点设定值，一个报文只有一个设定值，带时标
			/// </summary>
			public const byte C_SE_TF_1 = 63;

			/// <summary>
			/// 32比特串设定，一个报文只有一个设定值，带时标
			/// </summary>
			public const byte C_SE_TG_1 = 64;

			/// <summary>
			/// 归一化设定值，一个报文可以包含多个设定值，不带时标
			/// </summary>
			public const byte C_SE_NE_1 = 136;

			/// <summary>
			/// 初始化结束，报告厂站初始化完成
			/// </summary>
			public const byte M_EI_NA_1 = 70;

			/// <summary>
			/// 总召唤，带不同的限定词可以用于组召唤
			/// </summary>
			public const byte C_IC_NA_1 = 100;

			/// <summary>
			/// 累计量召唤，带不同的限定词可以用于组召唤
			/// </summary>
			public const byte C_CI_NA_1 = 101;

			/// <summary>
			/// 读命令，读取单个的信息对象值
			/// </summary>
			public const byte C_RD_NA_1 = 102;

			/// <summary>
			/// 时钟同步命令，需要通过测量通道延迟加以校正
			/// </summary>
			public const byte C_CS_NA_1 = 103;

			/// <summary>
			/// 复位进程命令，使用前需要双方验证
			/// </summary>
			public const byte C_RS_NA_1 = 105;

			/// <summary>
			/// 带时标的测试命令
			/// </summary>
			public const byte C_TS_TA_1 = 107;
		}

		#endregion
	}

}
