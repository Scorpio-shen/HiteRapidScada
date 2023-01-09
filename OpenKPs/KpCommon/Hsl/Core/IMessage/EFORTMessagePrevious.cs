using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HslCommunication.Core.IMessage
{
	/// <summary>
	/// 旧版的机器人的消息类对象，保留此类为了实现兼容
	/// </summary>
	public class EFORTMessagePrevious : NetMessageBase, INetMessage
	{
		/// <inheritdoc cref="INetMessage.ProtocolHeadBytesLength"/>
		public int ProtocolHeadBytesLength => 17;

		/// <inheritdoc cref="INetMessage.GetContentLengthByHeadBytes"/>
		public int GetContentLengthByHeadBytes( )
		{
			int length = BitConverter.ToInt16( HeadBytes, 15 ) - 17;
			if (length < 0) length = 0;
			return length;
		}
	}
}
