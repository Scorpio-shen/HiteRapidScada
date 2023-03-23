using HslCommunication.Instrument.DLT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HslCommunication.Core.IMessage
{
	/// <summary>
	/// DLT 645协议的串口透传的消息类
	/// </summary>
	public class DLT645Message : NetMessageBase,      INetMessage
	{
		/// <inheritdoc cref="INetMessage.ProtocolHeadBytesLength"/>
		public int ProtocolHeadBytesLength => 10;

		/// <inheritdoc cref="INetMessage.GetContentLengthByHeadBytes"/>
		public int GetContentLengthByHeadBytes( ) => HeadBytes[9] + 2;

		/// <inheritdoc/>
		public override int PependedUselesByteLength( byte[] headByte )
		{
			return HslCommunication.Instrument.DLT.Helper.DLT645Helper.FindHeadCode68H( headByte );
		}
	}
}
