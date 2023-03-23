using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if !NET35 && !NET20
using System.Threading.Tasks;
#endif

namespace HslCommunication.Instrument.DLT
{
	/// <summary>
	/// 基本的控制码信息
	/// </summary>
	public class DLTControl
	{
		/// <summary>
		/// 保留
		/// </summary>
		public const byte DLT2007_Retain = 0;

		/// <summary>
		/// 广播
		/// </summary>
		public const byte DLT2007_Broadcast = 0x08;

		/// <summary>
		/// 读数据
		/// </summary>
		public const byte DLT2007_ReadData = 0x11;

		/// <summary>
		/// 读后续数据
		/// </summary>
		public const byte DLT2007_ReadFollowData = 0x12;

		/// <summary>
		/// 读通信地址
		/// </summary>
		public const byte DLT2007_ReadAddress = 0x13;

		/// <summary>
		/// 写数据
		/// </summary>
		public const byte DLT2007_WriteData = 0x14;

		/// <summary>
		/// 写通信地址
		/// </summary>
		public const byte DLT2007_WriteAddress = 0x15;

		/// <summary>
		/// 冻结命令
		/// </summary>
		public const byte DLT2007_FreezeCommand = 0x16;

		/// <summary>
		/// 更改通信速率
		/// </summary>
		public const byte DLT2007_ChangeBaudRate = 0x17;

		/// <summary>
		/// 修改密码
		/// </summary>
		public const byte DLT2007_ChangePassword = 0x18;

		/// <summary>
		/// 最大需求量清零
		/// </summary>
		public const byte DLT2007_ClearMaxQuantityDemanded = 0x19;

		/// <summary>
		/// 电表清零
		/// </summary>
		public const byte DLT2007_ElectricityReset = 0x1A;

		/// <summary>
		/// 事件清零
		/// </summary>
		public const byte DLT2007_EventReset = 0x1B;

		/// <summary>
		/// 跳合闸、报警、保电
		/// </summary>
		public const byte DLT2007_ClosingAlarmPowerpProtection = 0x1C;

		/// <summary>
		/// 多功能端子输出控制命令
		/// </summary>
		public const byte DLT2007_MultiFunctionTerminalOutputControlCommand = 0x1D;

		/// <summary>
		/// 安全认证命令
		/// </summary>
		public const byte DLT2007_SecurityAuthenticationCommand = 0x03;


		/// <summary>
		/// 保留
		/// </summary>
		public const byte DLT1997_Retain = 0;

		/// <summary>
		/// 读数据
		/// </summary>
		public const byte DLT1997_ReadData = 0x01;
		/// <summary>
		/// 读后续数据
		/// </summary>
		public const byte DLT1997_ReadFollowData = 0x02;
		/// <summary>
		/// 重读数据
		/// </summary>
		public const byte DLT1997_ReadAgain = 0x03;
		/// <summary>
		/// 写数据
		/// </summary>
		public const byte DLT1997_WriteData = 0x04;
		/// <summary>
		/// 广播校时
		/// </summary>
		public const byte DLT1997_Broadcast = 0x08;
		/// <summary>
		/// 写设备地址
		/// </summary>
		public const byte DLT1997_WriteAddress = 0x0A;
		/// <summary>
		/// 更改通信速率
		/// </summary>
		public const byte DLT1997_ChangeBaudRate = 0x0C;
		/// <summary>
		/// 修改密码
		/// </summary>
		public const byte DLT1997_ChangePassword = 0x0F;
		/// <summary>
		/// 最大需量清零
		/// </summary>
		public const byte DLT1997_ClearMaxQuantityDemanded = 0x10;

	}
}
