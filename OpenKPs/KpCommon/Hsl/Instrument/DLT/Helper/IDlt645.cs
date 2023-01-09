using HslCommunication.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HslCommunication.Instrument.DLT.Helper
{
	/// <summary>
	/// DLT645的接口实现
	/// </summary>
	public interface IDlt645 : IReadWriteDevice
	{
		/// <summary>
		/// 获取或设置当前的地址域信息，是一个12个字符的BCD码，例如：149100007290<br />
		/// Get or set the current address domain information, which is a 12-character BCD code, for example: 149100007290
		/// </summary>
		string Station { get; set; }

		/// <summary>
		/// 获取或设置是否在每一次的报文通信时，增加"FE FE FE FE"的命令头<br />
		/// Get or set whether to add the command header of "FE FE FE FE" in each message communication
		/// </summary>
		bool EnableCodeFE { get; set; }

		/// <summary>
		/// 获取当前的DLT645的类型信息<br />
		/// Gets the type information of the current DLT645
		/// </summary>
		DLT645Type DLTType { get; }

		/// <inheritdoc cref="Core.Net.NetworkDoubleBase.ReadFromCoreServer(System.Net.Sockets.Socket, byte[], bool, bool)"/>
		OperateResult<byte[]> ReadFromCoreServer( byte[] send, bool hasResponseData, bool usePackAndUnpack = true );

	}
}
