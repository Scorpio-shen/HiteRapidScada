using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace HslCommunication.Core.IMessage
{
	/// <summary>
	/// 通用电气公司的SRIP协议的消息
	/// </summary>
	public class GeSRTPMessage : NetMessageBase, INetMessage
	{
		/// <inheritdoc cref="INetMessage.ProtocolHeadBytesLength"/>
		public int ProtocolHeadBytesLength => 56;

		/// <inheritdoc cref="INetMessage.GetContentLengthByHeadBytes"/>
		public int GetContentLengthByHeadBytes( ) => HeadBytes[4]+ HeadBytes[5] * 256;

	}
}
