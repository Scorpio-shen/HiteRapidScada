using HslCommunication.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HslCommunication.Instrument.CJT.Helper
{
	/// <summary>
	/// CJT188设备的接口
	/// </summary>
	public interface ICjt188 : IReadWriteDevice
	{
		/// <summary>
		/// 仪表的类型
		/// </summary>
		byte InstrumentType { get; set; }

		/// <inheritdoc cref="DLT.Helper.IDlt645.Station"/>
		string Station { get; set; }

		/// <summary>
		/// 获取或设置是否在每一次的报文通信时，增加"FE FE"的命令头<br />
		/// Get or set whether to add the command header of "FE FE" in each message communication
		/// </summary>
		bool EnableCodeFE { get; set; }
	}
}
