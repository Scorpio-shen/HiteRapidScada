using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HslCommunication.Core.IMessage
{

	/// <summary>
	/// 西门子S7协议的消息解析规则
	/// </summary>
	public class S7Message : NetMessageBase, INetMessage
	{
		/// <inheritdoc cref="INetMessage.ProtocolHeadBytesLength"/>
		public int ProtocolHeadBytesLength => 4;

		/// <inheritdoc cref="INetMessage.CheckHeadBytesLegal(byte[])"/>
		public override bool CheckHeadBytesLegal(byte[] token)
		{
			if (HeadBytes == null) return false;

			if (HeadBytes[0] == 0x03 && HeadBytes[1] == 0x00)
				return true;
			else
				return false;
		}

		/// <inheritdoc cref="INetMessage.GetContentLengthByHeadBytes"/>
		public int GetContentLengthByHeadBytes( )
		{
			if (HeadBytes?.Length >= 4)
			{
				int length = HeadBytes[2] * 256 + HeadBytes[3] - 4;
				if (length < 0) length = 0;
				return length;
			}
			else
				return 0;
		}

	}
}
