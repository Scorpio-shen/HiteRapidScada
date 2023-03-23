using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HslCommunication.Core;
using HslCommunication.BasicFramework;
using HslCommunication.Serial;
using HslCommunication;

namespace HslCommunication.Secs
{
	/// <summary>
	/// 串口类相关的Secs
	/// </summary>
	public class SecsGemSerial : SerialDeviceBase
	{
		#region Constructor

		/// <summary>
		/// 实例化一个默认的对象
		/// </summary>
		public SecsGemSerial( )
		{
			this.incrementCount = new SoftIncrementCount( uint.MaxValue );
			this.ByteTransform  = new ReverseBytesTransform( );
			this.WordLength     = 2;
		}

		#endregion

		#region Public Method

		/// <summary>
		/// 执行SECS命令
		/// </summary>
		/// <param name="command">命令信息</param>
		/// <returns>是否成功的结果</returns>
		public OperateResult<byte[]> ExecuteCommand( byte[] command )
		{
			// 先发送一个ENQ告诉设备方需要传送数据了
			OperateResult<byte[]> readEnq = ReadFromCoreServer( new byte[] { AsciiControl.ENQ } );
			if (!readEnq.IsSuccess) return readEnq;

			if (readEnq.Content[0] != AsciiControl.EOT) return new OperateResult<byte[]>( $"Send Enq to device, but receive [{readEnq.Content[0]}], need receive [EOT]" );
			while (true)
			{
				OperateResult<byte[]> readCmd = ReadFromCoreServer( command );
				if (!readCmd.IsSuccess) return readCmd;


			}
			
		}

		#endregion

		#region Private Member

		private SoftIncrementCount incrementCount;           // 增量的ID信息

		#endregion
	}
}
