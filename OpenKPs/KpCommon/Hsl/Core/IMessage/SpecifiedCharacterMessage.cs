using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HslCommunication.Core.IMessage
{
	/// <summary>
	/// 专门用于接收指定字符结尾的网络消息
	/// </summary>
	public class SpecifiedCharacterMessage : NetMessageBase, INetMessage
	{
		#region Constructor

		/// <summary>
		/// 使用固定的一个字符结尾作为当前的报文接收条件，来实例化一个对象<br />
		/// Instantiate an object using a fixed end of one character as the current message reception condition
		/// </summary>
		/// <param name="endCode">结尾的字符</param>
		public SpecifiedCharacterMessage( byte endCode )
		{
			byte[] buffer = new byte[4];
			buffer[3] = (byte)(buffer[3] | 0x80);        // 最高位设置为1
			buffer[3] = (byte)(buffer[3] | 0x01);        // 表示只有一个字节结尾
			buffer[1] = endCode;

			protocolHeadBytesLength = BitConverter.ToInt32( buffer, 0 );
		}

		/// <summary>
		/// 使用固定的两个个字符结尾作为当前的报文接收条件，来实例化一个对象<br />
		/// Instantiate an object using a fixed two-character end as the current message reception condition
		/// </summary>
		/// <param name="endCode1">第一个结尾的字符</param>
		/// <param name="endCode2">第二个结尾的字符</param>
		public SpecifiedCharacterMessage( byte endCode1, byte endCode2 )
		{
			byte[] buffer = new byte[4];
			buffer[3] = (byte)(buffer[3] | 0x80);        // 最高位设置为1
			buffer[3] = (byte)(buffer[3] | 0x02);        // 表示有两个字节结尾
			buffer[1] = endCode1;
			buffer[0] = endCode2;

			protocolHeadBytesLength = BitConverter.ToInt32( buffer, 0 );
		}

		#endregion

		/// <summary>
		/// 获取或设置在结束字符之后剩余的固定字节长度，有些则还包含两个字节的校验码，这时该值就需要设置为2。<br />
		/// Gets or sets the remaining fixed byte length after the end character, and some also contain a two-byte check code. In this case, the value needs to be set to 2.
		/// </summary>
		public byte EndLength
		{
			get => BitConverter.GetBytes( protocolHeadBytesLength )[2];
			set 
			{
				byte[] buffer = BitConverter.GetBytes( protocolHeadBytesLength );
				buffer[2] = value;
				protocolHeadBytesLength = BitConverter.ToInt32( buffer, 0 );
			}
		}

		/// <inheritdoc cref="INetMessage.ProtocolHeadBytesLength"/>
		public int ProtocolHeadBytesLength => protocolHeadBytesLength;

		/// <inheritdoc cref="INetMessage.GetContentLengthByHeadBytes"/>
		public int GetContentLengthByHeadBytes( ) => 0;

		/// <inheritdoc cref="INetMessage.CheckHeadBytesLegal(byte[])"/>
		public override bool CheckHeadBytesLegal( byte[] token )
		{
			return true;
		}

		#region Private Member

		private int protocolHeadBytesLength = -1;

		#endregion

	}
}
