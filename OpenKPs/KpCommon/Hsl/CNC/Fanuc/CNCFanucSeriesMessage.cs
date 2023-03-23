using HslCommunication.Core.IMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HslCommunication.CNC.Fanuc
{
	/// <summary>
	/// Fanuc床子的消息对象
	/// </summary>
	public class CNCFanucSeriesMessage : NetMessageBase, INetMessage
	{
		/// <inheritdoc/>
		public int ProtocolHeadBytesLength => 10;


		/// <inheritdoc/>
		public int GetContentLengthByHeadBytes( )
		{
			return HeadBytes[8] * 256 + HeadBytes[9];
		}


		/// <inheritdoc/>
		public override string ToString( ) => $"CNCFanucSeriesMessage";
	}
}
