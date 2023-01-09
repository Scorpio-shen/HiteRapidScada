using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HslCommunication.Instrument.IEC
{
	/// <summary>
	/// IEC104的消息事件
	/// </summary>
	public class IEC104MessageEventArgs : EventArgs
	{
		/// <summary>
		/// 指定asdu消息进行实例化一个对象
		/// </summary>
		/// <param name="asdu">asdu报文</param>
		public IEC104MessageEventArgs( byte[] asdu )
		{
			this.ASDU                = asdu;
			if ( asdu != null )
			{
				this.TypeID              = asdu[0];
				this.IsAddressContinuous = (asdu[1] & 0x80) == 0x80;
				this.InfoObjectCount     = asdu[1] & 0x7f;
				this.TransmissionReason  = asdu[2] & 0x3f;
				this.StationAddress      = BitConverter.ToUInt16( asdu, 4 );
				this.Body                = asdu.RemoveBegin( 6 );
			}
		}

		/// <summary>
		/// 获取或设置当前的asdu信息
		/// </summary>
		public byte[] ASDU { get; }

		/// <summary>
		/// 类型标识
		/// </summary>
		public byte TypeID { get; set; }

		/// <summary>
		/// 地址是否连续
		/// </summary>
		public bool IsAddressContinuous { get; set; }

		/// <summary>
		/// 信息对象个数
		/// </summary>
		public int InfoObjectCount { get; set; }

		/// <summary>
		/// 传送原因
		/// </summary>
		public int TransmissionReason { get; set; }

		/// <summary>
		/// 站地址
		/// </summary>
		public int StationAddress { get; set; }

		/// <summary>
		/// 信息体
		/// </summary>
		public byte[] Body { get; set; }
	}
}
