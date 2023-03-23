using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HslCommunication.Core.IMessage
{
	/// <summary>
	/// CJT188的协议信息
	/// </summary>
	public class CJT188Message : NetMessageBase, INetMessage
	{
		/// <inheritdoc cref="INetMessage.ProtocolHeadBytesLength"/>
		public int ProtocolHeadBytesLength => 11;

		/// <inheritdoc cref="INetMessage.GetContentLengthByHeadBytes"/>
		public int GetContentLengthByHeadBytes( ) => HeadBytes[10] + 2;

		/// <inheritdoc/>
		public override int PependedUselesByteLength( byte[] headByte )
		{
			return HslCommunication.Instrument.DLT.Helper.DLT645Helper.FindHeadCode68H( headByte );
		}
	}
}
