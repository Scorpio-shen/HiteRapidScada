using HslCommunication.Instrument.DLT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HslCommunication.Core.IMessage
{
	/// <summary>
	/// DLT698的协议消息文本
	/// </summary>
	public class DLT698Message : NetMessageBase, INetMessage
	{
		/// <inheritdoc cref="INetMessage.ProtocolHeadBytesLength"/>
		public int ProtocolHeadBytesLength => 8;

		/// <inheritdoc cref="INetMessage.GetContentLengthByHeadBytes"/>
		public int GetContentLengthByHeadBytes( )
		{
			return BitConverter.ToUInt16( HeadBytes, 1 ) + 2 - 8;
		}

		/// <inheritdoc/>
		public override int PependedUselesByteLength( byte[] headByte ) => HslCommunication.Instrument.DLT.Helper.DLT645Helper.FindHeadCode68H( headByte );
	}
}
