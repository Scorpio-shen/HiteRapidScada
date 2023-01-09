using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HslCommunication.Core.IMessage
{
	/// <summary>
	/// 异形消息对象，用于异形客户端的注册包接收以及验证使用
	/// </summary>
	public class AlienMessage : NetMessageBase, INetMessage
	{
		/// <inheritdoc cref="INetMessage.ProtocolHeadBytesLength"/>
		public int ProtocolHeadBytesLength => 5;

		/// <inheritdoc cref="INetMessage.CheckHeadBytesLegal(byte[])"/>
		public override bool CheckHeadBytesLegal( byte[] token )
		{
			if (HeadBytes == null) return false;

			if (HeadBytes[0] == 0x48 &&
				HeadBytes[1] == 0x73 &&
				HeadBytes[2] == 0x6E)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <inheritdoc cref="INetMessage.GetContentLengthByHeadBytes"/>
		public int GetContentLengthByHeadBytes( ) => HeadBytes[3] * 256 + HeadBytes[4];

	}
}
