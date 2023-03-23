using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HslCommunication;
using HslCommunication.Core;
using HslCommunication.Core.IMessage;

namespace HslCommunication.Secs.Message
{
	/// <summary>
	/// Hsms协议的消息定义
	/// </summary>
	public class SecsHsmsMessage : NetMessageBase, INetMessage
	{
		/// <inheritdoc cref="INetMessage.ProtocolHeadBytesLength"/>
		public int ProtocolHeadBytesLength => 4;

		/// <inheritdoc cref="INetMessage.GetContentLengthByHeadBytes"/>
		public int GetContentLengthByHeadBytes( )
		{
			byte[] buffer = new byte[4];
			buffer[0] = HeadBytes[3];
			buffer[1] = HeadBytes[2];
			buffer[2] = HeadBytes[1];
			buffer[3] = HeadBytes[0];
			return BitConverter.ToInt32( buffer, 0 );
		}

		/// <inheritdoc/>
		public override bool CheckHeadBytesLegal( byte[] token ) => true;
	}
}
