using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HslCommunication.Core
{
	/// <summary>
	/// 控制型的ascii资源信息<br />
	/// Controlled ascii resource information
	/// </summary>
	public class AsciiControl
	{
		/// <summary>
		/// 空字符
		/// </summary>
		public const byte NUL = 0x00;

		/// <summary>
		/// 标题开始<br />
		/// start of headling
		/// </summary>
		public const byte SOH = 0x01;

		/// <summary>
		/// 正文开始<br />
		/// start of text
		/// </summary>
		public const byte STX = 0x02;

		/// <summary>
		/// 正文结束<br />
		/// end of text
		/// </summary>
		public const byte ETX = 0x03;

		/// <summary>
		/// 传输结束<br />
		/// end of transmission
		/// </summary>
		public const byte EOT = 0x04;

		/// <summary>
		/// 请求<br />
		/// enquiry
		/// </summary>
		public const byte ENQ = 0x05;

		/// <summary>
		/// 接到通知<br />
		/// acknowledge
		/// </summary>
		public const byte ACK = 0x06;

		/// <summary>
		/// 响铃<br />
		/// bell
		/// </summary>
		public const byte BEL = 0x07;

		/// <summary>
		/// 退格<br />
		/// backspace
		/// </summary>
		public const byte BS = 0x08;

		/// <summary>
		/// 水平制表符<br />
		/// horizontal tab
		/// </summary>
		public const byte HT = 0x09;

		/// <summary>
		/// 换行符<br />
		/// NL line feed, new line
		/// </summary>
		public const byte LF = 0x0a;

		/// <summary>
		/// 垂直制表符<br />
		/// vertical tab
		/// </summary>
		public const byte VT = 0x0b;

		/// <summary>
		/// 换页键<br />
		/// NP form feed, new page
		/// </summary>
		public const byte FF = 0x0c;

		/// <summary>
		/// 回车键<br />
		/// carriage return
		/// </summary>
		public const byte CR = 0x0d;

		/// <summary>
		/// 不用切换<br />
		/// shift out
		/// </summary>
		public const byte SO = 0x0e;

		/// <summary>
		/// 启用切换<br />
		/// shift in
		/// </summary>
		public const byte SI = 0x0f;

		/// <summary>
		/// 数据链路定义<br />
		/// data link escape
		/// </summary>
		public const byte DLE = 0x10;

		/// <summary>
		/// 设备控制1<br />
		/// device control 1
		/// </summary>
		public const byte DC1 = 0x11;

		/// <summary>
		/// 设备控制2<br />
		/// device control 2
		/// </summary>
		public const byte DC2 = 0x12;

		/// <summary>
		/// 设备控制3<br />
		/// device control 3
		/// </summary>
		public const byte DC3 = 0x13;

		/// <summary>
		/// 设备控制4<br />
		/// device control 4
		/// </summary>
		public const byte DC4 = 0x14;

		/// <summary>
		/// 拒绝接收<br />
		/// negative acknowledge
		/// </summary>
		public const byte NAK = 0x15;

		/// <summary>
		/// 同步空闲<br />
		/// synchronous idle
		/// </summary>
		public const byte SYN = 0x16;

		/// <summary>
		/// 传输块结束<br />
		/// end of trans. block
		/// </summary>
		public const byte ETB = 0x17;

		/// <summary>
		/// 取消<br />
		/// cancel
		/// </summary>
		public const byte CAN = 0x18;

		/// <summary>
		/// 介质中断<br />
		/// end of medium
		/// </summary>
		public const byte EM = 0x19;

		/// <summary>
		/// 替补<br/>
		/// substitute
		/// </summary>
		public const byte SUB = 0x1a;

		/// <summary>
		/// 溢出<br />
		/// escape
		/// </summary>
		public const byte ESC = 0x1b;

		/// <summary>
		/// 文件分隔符<br />
		/// file separator
		/// </summary>
		public const byte FS = 0x1c;

		/// <summary>
		/// 分组符<br />
		/// group separator
		/// </summary>
		public const byte GS = 0x1d;

		/// <summary>
		/// 记录分离符<br />
		/// record separator
		/// </summary>
		public const byte RS = 0x1e;

		/// <summary>
		/// 单元分隔符<br />
		/// unit separator
		/// </summary>
		public const byte US = 0x1f;
	}
}
