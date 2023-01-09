using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HslCommunication.Core.IMessage
{
	/// <summary>
	/// Memobus协议的消息定义
	/// </summary>
	public class MemobusMessage : NetMessageBase, INetMessage
	{
		/// <inheritdoc cref="INetMessage.ProtocolHeadBytesLength"/>
		public int ProtocolHeadBytesLength => 12;

		/// <inheritdoc cref="INetMessage.GetContentLengthByHeadBytes"/>
		public int GetContentLengthByHeadBytes( )
		{
			if (HeadBytes?.Length >= ProtocolHeadBytesLength)
			{
				int length = BitConverter.ToUInt16( HeadBytes, 6 ) - 12;
				if (length < 0) length = 0;
				return length;
			}
			else
				return 0;
		}
	}
}
