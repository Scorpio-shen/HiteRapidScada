using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HslCommunication.Core.IMessage
{
	/// <summary>
	/// 倍福的ADS协议的信息
	/// </summary>
	public class AdsNetMessage : NetMessageBase, INetMessage
	{
		/// <inheritdoc cref="INetMessage.ProtocolHeadBytesLength"/>
		public int ProtocolHeadBytesLength => 6;

		/// <inheritdoc cref="INetMessage.GetContentLengthByHeadBytes"/>
		public int GetContentLengthByHeadBytes( )
		{
			if (HeadBytes?.Length >= 6)
			{
				int length = BitConverter.ToInt32( HeadBytes, 2 );
				if (length > 10000) length = 10000;
				if (length < 0) length = 0;
				return length;
			}
			else
				return 0;
		}
	}
}
