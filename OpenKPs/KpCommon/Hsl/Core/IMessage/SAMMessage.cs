using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HslCommunication.Core.IMessage
{
	/// <summary>
	/// SAM身份证通信协议的消息
	/// </summary>
	public class SAMMessage : NetMessageBase, INetMessage
	{
		/// <inheritdoc cref="INetMessage.ProtocolHeadBytesLength"/>
		public int ProtocolHeadBytesLength => 7;

		/// <inheritdoc cref="INetMessage.CheckHeadBytesLegal(byte[])"/>
		public override bool CheckHeadBytesLegal( byte[] token )
		{
			if (HeadBytes == null) return false;

			return HeadBytes[0] == 0xAA && HeadBytes[1] == 0xAA && HeadBytes[2] == 0xAA && HeadBytes[3] == 0x96 && HeadBytes[4] == 0x69;
		}

		/// <inheritdoc cref="GetContentLengthByHeadBytes"/>
		public int GetContentLengthByHeadBytes( )
		{
			if (HeadBytes?.Length >= 7)
				return HeadBytes[5] * 256 + HeadBytes[6];
			else
				return 0;
		}

	}
}
