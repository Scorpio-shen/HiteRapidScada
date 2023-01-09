using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HslCommunication.Core.IMessage
{
	/// <summary>
	/// 用于欧姆龙通信的Fins协议的消息解析规则
	/// </summary>
	public class FinsMessage : NetMessageBase, INetMessage
	{
		/// <inheritdoc cref="INetMessage.ProtocolHeadBytesLength"/>
		public int ProtocolHeadBytesLength => 16;

		/// <inheritdoc cref="INetMessage.GetContentLengthByHeadBytes"/>
		public int GetContentLengthByHeadBytes( )
		{
			byte[] buffer = new byte[4];
			buffer[0] = HeadBytes[7];
			buffer[1] = HeadBytes[6];
			buffer[2] = HeadBytes[5];
			buffer[3] = HeadBytes[4];

			int length = BitConverter.ToInt32( buffer, 0 );
			if (length > 10000) length = 10000;
			if (length < 0) length = 0;
			return length - 8;
		}

		/// <inheritdoc cref="INetMessage.CheckHeadBytesLegal(byte[])"/>
		public override bool CheckHeadBytesLegal( byte[] token )
		{
			if (HeadBytes == null) return false;

			if (HeadBytes[0] == 0x46 && HeadBytes[1] == 0x49 && HeadBytes[2] == 0x4E && HeadBytes[3] == 0x53)
				return true;
			else
				return false;
		}

		/// <inheritdoc cref="INetMessage.PependedUselesByteLength(byte[])"/>
		public override int PependedUselesByteLength( byte[] headByte )
		{
			if (headByte == null) return 0;
			for (int i = 0; i < headByte.Length - 3; i++)
			{
				if (headByte[i + 0] == 0x46 &&
					headByte[i + 1] == 0x49 &&
					headByte[i + 2] == 0x4E &&
					headByte[i + 3] == 0x53)
					return i;
			}
			return base.PependedUselesByteLength( headByte );
		}

	}
}
