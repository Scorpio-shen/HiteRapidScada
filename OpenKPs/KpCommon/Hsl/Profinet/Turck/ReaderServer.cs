using HslCommunication.BasicFramework;
using HslCommunication.Core;
using HslCommunication.Core.IMessage;
using HslCommunication.Core.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace HslCommunication.Profinet.Turck
{
	/// <summary>
	/// 图尔克reader协议的虚拟服务器
	/// </summary>
	public class ReaderServer : NetworkDataServerBase
	{
		#region Constructor

		/// <summary>
		/// 实例化一个默认的图尔克虚拟服务器
		/// </summary>
		public ReaderServer( )
		{
			WordLength    = 2;
			ByteTransform = new ReverseBytesTransform( );

			buffer        = new SoftBuffer( DataPoolLength );
		}

		#endregion

		#region Public Porperties
		
		/// <summary>
		/// 获取或设置每个block占用的字节数信息
		/// </summary>
		public int BytesOfBlock { get => bytesOfBlock; set => bytesOfBlock = value; }


		#endregion

		#region NetServer Override

		/// <inheritdoc/>
		protected override INetMessage GetNewNetMessage( ) => new TurckReaderMessage( );

		/// <inheritdoc/>
		protected override OperateResult<byte[]> ReadFromCoreServer( AppSession session, byte[] receive )
		{
			if (!ReaderNet.CheckCRC( receive, receive.Length - 2 )) return OperateResult.CreateSuccessResult( ReaderNet.PackReaderCommand( new byte[] { receive[3], 0x83, 0x00, 0x01, 0x00 } ) );

			byte[] back = null;
			if (receive[3] == 0x68)
			{
				Send( session.WorkSocket, ReaderNet.PackReaderCommand( new byte[] { 0x68, 0x89 } ) );
				back = ReadByMessage( receive );     // 读取数据
			}
			else if (receive[3] == 0x69)
			{
				Send( session.WorkSocket, ReaderNet.PackReaderCommand( new byte[] { 0x69, 0x89 } ) );
				back = WriteByMessage( receive );    // 写入数据
			}
			else if (receive[3] == 0x70)
			{
				Send( session.WorkSocket, ReaderNet.PackReaderCommand( new byte[] { 0x70, 0x89 } ) );
				back = ReaderNet.PackReaderCommand( new byte[] { 0x70, 0x8A, 0x71, 0x26, 0xD0, 0xE5, 0xD7, 0x01, 0x08, 0xE0, 0x00, 0x00, 0xF9, (byte)(bytesOfBlock - 1), 0x74 } );
			}
			else
			{
				return new OperateResult<byte[]>( StringResources.Language.NotSupportedFunction );
			}
			return OperateResult.CreateSuccessResult( back );
		}

		private byte[] ReadByMessage( byte[] receive )
		{
			byte[] read = buffer.GetBytes( receive[5] * bytesOfBlock, (receive[6] + 1) * bytesOfBlock );
			return ReaderNet.PackReaderCommand( SoftBasic.SpliceArray( new byte[] { receive[3], 0x9A }, read ) );
		}

		private byte[] WriteByMessage( byte[] receive )
		{
			if (!EnableWrite) return ReaderNet.PackReaderCommand( new byte[] { receive[3], 0x83, 0x12, 0x00, 0x00 } );
			if ((receive[6] + 1) * bytesOfBlock != receive.Length - 9) return ReaderNet.PackReaderCommand( new byte[] { receive[3], 0x83, 0x01, 0x00, 0x00 } );

			buffer.SetBytes( receive.SelectMiddle( 7, receive.Length - 9 ), receive[5] * bytesOfBlock );
			return ReaderNet.PackReaderCommand( new byte[] { receive[3], 0x8A } );
		}

		#endregion

		#region Private Member

		private int bytesOfBlock = 8;
		private SoftBuffer buffer;                     // 寄存器的数据池
		private const int DataPoolLength = 65536;      // 数据的长度

		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString( ) => $"ReaderServer[{Port}]";

		#endregion

	}
}
