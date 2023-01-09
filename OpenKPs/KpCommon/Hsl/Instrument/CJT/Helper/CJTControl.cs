using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HslCommunication.Instrument.CJT.Helper
{
	/// <summary>
	/// 控制码信息
	/// </summary>
	public class CJTControl
	{
		/// <summary>
		/// 读数据
		/// </summary>
		public const byte ReadData = 0x01;

		/// <summary>
		/// 写数据
		/// </summary>
		public const byte WriteData = 0x04;

		/// <summary>
		/// 读地址
		/// </summary>
		public const byte ReadAddress = 0x03;

		/// <summary>
		/// 写地址
		/// </summary>
		public const byte WriteAddress = 0x15;

	}
}
