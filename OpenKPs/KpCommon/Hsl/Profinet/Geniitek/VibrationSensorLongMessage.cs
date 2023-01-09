using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HslCommunication.Core.IMessage;

namespace HslCommunication.Profinet.Geniitek
{
	/// <summary>
	/// 完整的数据报文信息
	/// </summary>
	public class VibrationSensorLongMessage : NetMessageBase, INetMessage
	{
		/// <inheritdoc cref="INetMessage.ProtocolHeadBytesLength"/>
		public int ProtocolHeadBytesLength => 12;

		/// <inheritdoc cref="INetMessage.CheckHeadBytesLegal(byte[])"/>
		public override bool CheckHeadBytesLegal( byte[] token )
		{
			if (HeadBytes == null) return false;

			if (HeadBytes[0] == 0xAA && HeadBytes[1] == 0x55 && HeadBytes[2] == 0x7F)
				return true;
			else
				return false;
		}

		/// <inheritdoc cref="INetMessage.GetContentLengthByHeadBytes"/>
		public int GetContentLengthByHeadBytes( )
		{
			return HeadBytes[10] * 256 + HeadBytes[11] + 4;
		}

	}
}
