using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HslCommunication.Core.IMessage
{
	/// <summary>
	/// OpenProtocol协议的消息
	/// </summary>
	public class OpenProtocolMessage : NetMessageBase, INetMessage
	{
		/// <inheritdoc cref="INetMessage.ProtocolHeadBytesLength"/>
		public int ProtocolHeadBytesLength => 4;

		/// <inheritdoc cref="INetMessage.GetContentLengthByHeadBytes"/>
		public int GetContentLengthByHeadBytes( )
		{
			try
			{
				if (HeadBytes?.Length >= 4)
				{
					int length = Convert.ToInt32( Encoding.ASCII.GetString( HeadBytes, 0, 4 ) ) - 4 + 1;

					return length < 0 ? 0 : length;
				}
				else
					return 0;
			}
			catch
			{
				return 16 + 1;
			}
		}
	}
}
