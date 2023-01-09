using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HslCommunication.Core.IMessage
{
	/// <summary>
	/// Kuka机器人的 KRC4 控制器中的服务器KUKAVARPROXY
	/// </summary>
	public class KukaVarProxyMessage : NetMessageBase, INetMessage
	{
		/// <inheritdoc cref="INetMessage.ProtocolHeadBytesLength"/>
		public int ProtocolHeadBytesLength => 4;

		/// <inheritdoc cref="INetMessage.GetContentLengthByHeadBytes"/>
		public int GetContentLengthByHeadBytes()
		{
			if (HeadBytes?.Length >= 4)
				return HeadBytes[2] * 256 + HeadBytes[3];
			else
				return 0;
		}

		/// <inheritdoc cref="INetMessage.GetHeadBytesIdentity"/>
		public override int GetHeadBytesIdentity()
		{
			if (HeadBytes?.Length >= 4)
				return HeadBytes[0] * 256 + HeadBytes[1];
			else
				return 0;
		}

	}
}
