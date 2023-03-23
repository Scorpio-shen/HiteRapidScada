using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HslCommunication.Core.IMessage
{

	/// <summary>
	/// 埃夫特机器人的消息对象
	/// </summary>
	public class EFORTMessage : NetMessageBase, INetMessage
	{
		/// <inheritdoc cref="INetMessage.ProtocolHeadBytesLength"/>
		public int ProtocolHeadBytesLength => 18;

		/// <inheritdoc cref="INetMessage.GetContentLengthByHeadBytes"/>
		public int GetContentLengthByHeadBytes( )
		{
			int length = BitConverter.ToInt16( HeadBytes, 16 ) - 18;
			if (length < 0) length = 0;
			return length;
		}

	}
}
