using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HslCommunication.Core.IMessage
{
	/// <summary>
	/// LSIS的PLC的FastEnet的消息定义
	/// </summary>
	public class LsisFastEnetMessage : NetMessageBase, INetMessage
	{
		/// <inheritdoc cref="INetMessage.ProtocolHeadBytesLength"/>
		public int ProtocolHeadBytesLength => 20;

		/// <inheritdoc cref="INetMessage.CheckHeadBytesLegal(byte[])"/>
		public override bool CheckHeadBytesLegal( byte[] token )
		{
			if (HeadBytes == null) return false;

			return HeadBytes[0] == 0x4C;
		}

		/// <inheritdoc cref="INetMessage.GetContentLengthByHeadBytes"/>
		public int GetContentLengthByHeadBytes( )
		{
			if (HeadBytes?.Length >= 20)
				return BitConverter.ToUInt16( HeadBytes, 16 );
			else
				return 0;
		}

		/// <inheritdoc cref="INetMessage.GetHeadBytesIdentity"/>
		public override int GetHeadBytesIdentity( ) => BitConverter.ToUInt16( HeadBytes, 14 );

	}
}
