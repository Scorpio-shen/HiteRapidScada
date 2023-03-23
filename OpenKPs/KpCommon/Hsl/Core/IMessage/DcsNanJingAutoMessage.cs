using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HslCommunication.Core.IMessage
{
	/// <summary>
	/// 南京自动化研究所推出的DCS设备的消息类
	/// </summary>
	public class DcsNanJingAutoMessage : NetMessageBase, INetMessage
	{
		/// <inheritdoc cref="INetMessage.ProtocolHeadBytesLength"/>
		public int ProtocolHeadBytesLength => 6;

		/// <inheritdoc cref="INetMessage.GetContentLengthByHeadBytes"/>
		public int GetContentLengthByHeadBytes( )
		{
			if (HeadBytes?.Length >= ProtocolHeadBytesLength)
			{
				return HeadBytes[4] * 256 + HeadBytes[5];
			}
			else
				return 0;
		}

		/// <inheritdoc cref="INetMessage.GetHeadBytesIdentity"/>
		public override int GetHeadBytesIdentity( ) => HeadBytes[0] * 256 + HeadBytes[1];

	}
}
