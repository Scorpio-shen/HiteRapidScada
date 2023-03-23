using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
#if !NET35 && !NET20
using System.Threading.Tasks;
#endif

namespace HslCommunication.Robot.YASKAWA.Helper
{
	/// <summary>
	/// 安川机器人的高速以太网的辅助方法
	/// </summary>
	public class YRCHighEthernetHelper
	{
		/// <summary>
		/// 构建完整的读取指令
		/// </summary>
		/// <param name="handle">处理分区，1:机器人控制 2:文件控制</param>
		/// <param name="requestID">请求ID， 客户端每次命令输出的时请增量</param>
		/// <param name="command">命令编号，相当于CIP通信的CLASS</param>
		/// <param name="dataAddress">数据队列编号，相当于CIP通信的Instance</param>
		/// <param name="dataAttribute">单元编号，相当于CIP通信协议的Attribute</param>
		/// <param name="dataHandle">处理请求，定义数据的请方法</param>
		/// <param name="dataPart">数据部分的内容</param>
		/// <returns>构建结果</returns>
		public static byte[] BuildCommand( byte handle, byte requestID, ushort command, ushort dataAddress, byte dataAttribute, byte dataHandle, byte[] dataPart )
		{
			MemoryStream ms = new MemoryStream( );
			ms.Write( Encoding.ASCII.GetBytes( "YERC" ) );                      // 识别子
			ms.Write( new byte[] { 0x20, 0x00, 0x00, 0x00 } );                  // 数据部大小
			ms.Write( new byte[] { 0x03, handle, 0x00, requestID } );           // 预约1/处理分区
			ms.Write( new byte[] { 0x00, 0x00, 0x00, 0x00 } );                  // 数据块编号
			ms.Write( Encoding.ASCII.GetBytes( "99999999" ) );                  // 预约2
			ms.Write( BitConverter.GetBytes( command ) );                       // 命令编号
			ms.Write( BitConverter.GetBytes( dataAddress ) );                   // 数据排列编号
			ms.Write( new byte[] { dataAttribute, dataHandle, 0x00, 0x00 } );   // 单元编号，处理特性，PADDING

			if (dataPart != null) ms.Write( dataPart );                         // 数据部
			byte[] buffer = ms.ToArray( );
			buffer[6] = BitConverter.GetBytes( buffer.Length - 32 )[0];
			buffer[7] = BitConverter.GetBytes( buffer.Length - 32 )[1];
			return buffer;
		}

		/// <summary>
		/// 检查当前的机器人反馈的数据是否正确
		/// </summary>
		/// <param name="response">从机器人反馈的数据</param>
		/// <returns>是否检查正确</returns>
		public static OperateResult CheckResponseContent( byte[] response )
		{
			if (response[25] != 0x00)
			{
				byte status = response[25];
				int affix = 0;
				if (status == 0x1f)   // 查看附加状态的参数
				{
					if (response[26] == 0x01) affix = response[28];
					else affix = BitConverter.ToUInt16( response, 28 );
				}

				return new OperateResult( status, GetErrorText( status, affix ) );
			}
			return OperateResult.CreateSuccessResult( );
		}

		/// <summary>
		/// 根据状态信息及附加状态信息来获取错误的文本描述信息
		/// </summary>
		/// <param name="status">状态信息</param>
		/// <param name="affix">附加状态信息</param>
		/// <returns>错误的文本描述信息</returns>
		public static string GetErrorText( byte status, int affix )
		{
			switch (status)
			{
				case 0x08: return "未定义被要求的命令。";
				case 0x09: return "检出无效的数据单元编号。";
				case 0x28: return "请求的数据排列编号不存在指定的命令。";
				case 0x1f:
					{
						switch (affix)
						{
							case 0x1010: return "命令异常";
							case 0x1011: return "命令操作数异常";
							case 0x1012: return "命令操作值超出范围";
							case 0x1013: return "命令操作长异常";
							case 0x1020: return "设备的文件数太多。";
							case 0x2010: return "机器人动作中";
							case 0x2020: return "示教编程器HOLD停止中";
							case 0x2030: return "再线盒HOLD停止中";
							case 0x2040: return "外部HOLD中";
							case 0x2050: return "命令HOLD中";
							case 0x2060: return "发生错误报警中";
							case 0x2070: return "伺服ON中";
							case 0x2080: return "模式不同";
							case 0x2090: return "访问其他功能的文件中";
							case 0x2100: return "没有命令模式设定";
							case 0x2110: return "此数据不能访问";
							case 0x2120: return "此数据不能读取";
							case 0x2130: return "编辑中";
							case 0x2150: return "执行坐标变换功能中";
							case 0x3010: return "请接通伺服电源";
							case 0x3040: return "请确认原点位置";
							case 0x3050: return "请进行位置确认";
							case 0x3070: return "无法生成现在值";
							case 0x3220: return "收到锁定面板模式／循环禁止信号";
							case 0x3230: return "面板锁定 收到禁止启动信号";
							case 0x3350: return "没有示教用户坐标";
							case 0x3360: return "用户坐标文件被损坏";
							case 0x3370: return "控制轴组不同";
							case 0x3380: return "基座轴数据不同";
							case 0x3390: return "不可变换相对JOB ";
							case 0x3400: return "禁止调用主程序 （参数）";
							case 0x3410: return "禁止调用主程序 （动作中灯亮）";
							case 0x3420: return "禁止调用主程序 （示教锁定）";
							case 0x3430: return "未定义机器人间的校准";
							case 0x3450: return "不能接通伺服电源";
							case 0x3460: return "不能设定坐标系";
							case 0x4010: return "内存容量不足 （程序登录内存）";
							case 0x4012: return "内存容量不足 （ 变位机数据登录内存）";
							case 0x4020: return "禁止编辑程序";
							case 0x4030: return "存在相同名称的程序";
							case 0x4040: return "没有指定的程序";
							case 0x4060: return "请设定执行的程序";
							case 0x4120: return "位置数据被损坏";
							case 0x4130: return "位置数据部存在";
							case 0x4140: return "位置变量类型不同";
							case 0x4150: return "不是主程序的程序 END命令";
							case 0x4170: return "命令数据被损坏";
							case 0x4190: return "程序名中存在不合适的字符";
							case 0x4200: return "标签名中存在不合适的字符";
							case 0x4230: return "本系统中存在不能使用的命令";
							case 0x4420: return "转换的程序没有步骤";
							case 0x4430: return "此程序已全被转换";
							case 0x4480: return "请示教用户坐标";
							case 0x4490: return "相对JOB／ 独立控制功能未被许可";
							case 0x5110: return "语法错误 （命令的语法）";
							case 0x5120: return "变位机数据异常";
							case 0x5130: return "缺少NOP或者 END命令";
							case 0x5170: return "格式错误（违背写法）";
							case 0x5180: return "数据数不恰当";
							case 0x5200: return "超出数据范围";
							case 0x5310: return "语法错误 （ 命令以外）";
							case 0x5340: return "模拟命令指定错误";
							case 0x5370: return "存在条件数据记录错误";
							case 0x5390: return "存在程序数据记录错误";
							case 0x5430: return "系统数据不一致";
							case 0x5480: return "焊接机类型不一致";
							case 0x6010: return "机器人或工装轴动作中";
							case 0x6020: return "指定设备容量不足";
							case 0x6030: return "指定设备无法访问";
							case 0x6040: return "预想外自动备份要求";
							case 0x6050: return "CMOS 大小在RAM 区域超出";
							case 0x6060: return "电源接通时，无法确保内存";
							case 0x6070: return "备份文件信息访问异常";
							case 0x6080: return "备份文件排序（删除）失败";
							case 0x6090: return "备份文件排序（重命名）失败";
							case 0x6100: return "驱动名称超出规定值";
							case 0x6110: return "设备不同";
							case 0x6120: return "系统错误";
							case 0x6130: return "不可设定自动备份";
							case 0x6140: return "自动备份中不可手动备份";
							case 0xA000: return "未定义命令";
							case 0xA001: return "数据排列编号（ Instance） 异常";
							case 0xA002: return "单元编号（ Attribute） 异常";
							case 0xA100: return "响应数据部大小( 硬件限制值) 异常";
							case 0xA102: return "响应数据部大小(软件限制值)异常";
							case 0xB001: return "未定义位置变量";
							case 0xB002: return "禁止使用数据";
							case 0xB003: return "请求数据大小异常";
							case 0xB004: return "数据范围以外";
							case 0xB005: return "未设定数据";
							case 0xB006: return "未登录指定的用途";
							case 0xB007: return "未登录指定的机种";
							case 0xB008: return "控制轴组设定异常";
							case 0xB009: return "速度设定异常";
							case 0xB00A: return "未设定动作速度";
							case 0xB00B: return "动作坐标系设定异常";
							case 0xB00C: return "形态设定异常";
							case 0xB00D: return "工具编号设定异常";
							case 0xB00E: return "用户编号设定异常";
							default: return StringResources.Language.UnknownError;
						}
					}
				default: return StringResources.Language.UnknownError;
			}
		}
	}
}
