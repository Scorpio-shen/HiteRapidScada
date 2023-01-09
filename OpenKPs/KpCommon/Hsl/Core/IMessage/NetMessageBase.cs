using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HslCommunication.Core.IMessage
{
	/// <summary>
	/// 消息类的基类
	/// </summary>
	public class NetMessageBase
	{
		/// <inheritdoc cref="INetMessage.HeadBytes"/>
		public byte[] HeadBytes { get; set; }

		/// <inheritdoc cref="INetMessage.ContentBytes"/>
		public byte[] ContentBytes { get; set; }

		/// <inheritdoc cref="INetMessage.SendBytes"/>
		public byte[] SendBytes { get; set; }

		/// <inheritdoc cref="INetMessage.PependedUselesByteLength"/>
		public virtual int PependedUselesByteLength( byte[] headByte ) => 0;

		/// <inheritdoc cref="INetMessage.GetHeadBytesIdentity"/>
		public virtual int GetHeadBytesIdentity( ) => 0;

		/// <inheritdoc cref="CheckHeadBytesLegal(byte[])"/>
		public virtual bool CheckHeadBytesLegal( byte[] token )
		{
			if (HeadBytes == null) return false;
			return true;
		}
	}
}
