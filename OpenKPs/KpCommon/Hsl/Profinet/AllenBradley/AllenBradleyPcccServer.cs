using HslCommunication.BasicFramework;
using HslCommunication.Core;
using HslCommunication.Core.IMessage;
using HslCommunication.Core.Net;
using HslCommunication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using HslCommunication.Reflection;
using HslCommunication.Core.Address;
#if !NET35 && !NET20
using System.Threading.Tasks;
#endif

namespace HslCommunication.Profinet.AllenBradley
{
	/// <summary>
	/// 虚拟的PCCC服务器，模拟的AB 1400通信
	/// </summary>
	public class AllenBradleyPcccServer : NetworkDataServerBase
	{
		#region Constructor

		/// <summary>
		/// 实例化一个默认的对象
		/// </summary>
		public AllenBradleyPcccServer( )
		{
			this.ByteTransform = new RegularByteTransform( );
			this.WordLength    = 2;
			this.aBuffer       = new SoftBuffer( DataPoolLength );
			this.bBuffer       = new SoftBuffer( DataPoolLength );
			this.nBuffer       = new SoftBuffer( DataPoolLength );
			this.fBuffer       = new SoftBuffer( DataPoolLength );
			this.sBuffer       = new SoftBuffer( DataPoolLength );
			this.iBuffer       = new SoftBuffer( DataPoolLength );
			this.oBuffer       = new SoftBuffer( DataPoolLength );
		}

		#endregion

		#region Read Write Support

		/// <inheritdoc/>
		[HslMqttApi( "ReadByteArray", "" )]
		public override OperateResult<byte[]> Read( string address, ushort length )
		{
			OperateResult<AllenBradleySLCAddress> analysis = AllenBradleySLCAddress.ParseFrom( address );
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( analysis );

			switch (analysis.Content.DataCode)
			{
				case 0x8E: return OperateResult.CreateSuccessResult( aBuffer.GetBytes( analysis.Content.AddressStart, length ) );
				case 0x85: return OperateResult.CreateSuccessResult( bBuffer.GetBytes( analysis.Content.AddressStart, length ) );
				case 0x89: return OperateResult.CreateSuccessResult( nBuffer.GetBytes( analysis.Content.AddressStart, length ) );
				case 0x8A: return OperateResult.CreateSuccessResult( fBuffer.GetBytes( analysis.Content.AddressStart, length ) );
				case 0x84: return OperateResult.CreateSuccessResult( sBuffer.GetBytes( analysis.Content.AddressStart, length ) );
				case 0x83: return OperateResult.CreateSuccessResult( iBuffer.GetBytes( analysis.Content.AddressStart, length ) );
				case 0x82: return OperateResult.CreateSuccessResult( oBuffer.GetBytes( analysis.Content.AddressStart, length ) );
				default: return new OperateResult<byte[]>( StringResources.Language.NotSupportedDataType );
			}
		}

		/// <inheritdoc/>
		[HslMqttApi( "WriteByteArray", "" )]
		public override OperateResult Write( string address, byte[] value )
		{
			OperateResult<AllenBradleySLCAddress> analysis = AllenBradleySLCAddress.ParseFrom( address );
			if (!analysis.IsSuccess) return analysis;

			switch (analysis.Content.DataCode)
			{
				case 0x8E: aBuffer.SetBytes( value, analysis.Content.AddressStart ); return OperateResult.CreateSuccessResult( );
				case 0x85: bBuffer.SetBytes( value, analysis.Content.AddressStart ); return OperateResult.CreateSuccessResult( );
				case 0x89: nBuffer.SetBytes( value, analysis.Content.AddressStart ); return OperateResult.CreateSuccessResult( );
				case 0x8A: fBuffer.SetBytes( value, analysis.Content.AddressStart ); return OperateResult.CreateSuccessResult( );
				case 0x84: sBuffer.SetBytes( value, analysis.Content.AddressStart ); return OperateResult.CreateSuccessResult( );
				case 0x83: iBuffer.SetBytes( value, analysis.Content.AddressStart ); return OperateResult.CreateSuccessResult( );
				case 0x82: oBuffer.SetBytes( value, analysis.Content.AddressStart ); return OperateResult.CreateSuccessResult( );
				default: return new OperateResult( StringResources.Language.NotSupportedDataType );
			}
		}

		#endregion

		#region NetServer Override

		/// <inheritdoc/>
		protected override INetMessage GetNewNetMessage( ) => new AllenBradleyMessage( );

		/// <inheritdoc/>
		protected override void ThreadPoolLoginAfterClientCheck( Socket socket, System.Net.IPEndPoint endPoint )
		{
			// 接收2次的握手协议
			AllenBradleyMessage message = new AllenBradleyMessage( );
			OperateResult<byte[]> read1 = ReceiveByMessage( socket, 5000, message );
			if (!read1.IsSuccess) return;

			string context = read1.Content.SelectMiddle( 12, 8 ).ToHexString( );
			LogNet?.WriteDebug( "Reg1: " + read1.Content.ToHexString( ' ' ) );

			OperateResult send1 = Send( socket, AllenBradleyHelper.PackRequestHeader( 0x65, sessionID, new byte[] { 0x01, 0x00, 0x00, 0x00 } ) );
			if (!send1.IsSuccess) return;

			OperateResult<byte[]> read2 = ReceiveByMessage( socket, 5000, message );
			if (!read2.IsSuccess) return;

			LogNet?.WriteDebug( "Reg2: " + read2.Content.ToHexString( ) );

			OperateResult send2 = Send( socket, AllenBradleyHelper.PackRequestHeader( 0x6f, sessionID, 
				@"00 00 00 00 00 04 02 00 00 00 00 00 b2 00 1e 00 d4 00 00 00 cc 31 59 a2 e8 a3 14 00 27 04 09 10 0b 46 a5 c1 01 40 20 00 01 40 20 00 00 00".ToHexBytes( ) ) ) ;
			if (!send2.IsSuccess) return;

			// 开始接收数据信息
			base.ThreadPoolLoginAfterClientCheck( socket, endPoint );
		}

		/// <inheritdoc/>
		protected override OperateResult<byte[]> ReadFromCoreServer( AppSession session, byte[] receive )
		{
			byte[] back = null;
			if (receive[0] == 0x6f)
			{
				back = AllenBradleyHelper.PackRequestHeader( 0x6f, sessionID,
						AllenBradleyHelper.PackCommandSpecificData(
						AllenBradleyHelper.PackCommandSingleService( null, 0x00 ),
						AllenBradleyHelper.PackCommandSingleService( "ce 00 00 00 27 04 09 10 0b 46 a5 c1 00 00".ToHexBytes( ), 0xB2 ) ) );
			}
			else if (receive[0] == 0x66)
			{
				back = AllenBradleyHelper.PackRequestHeader( 0x6f, sessionID, null );
			}
			else
			{
				//back = "700025004A230F80000000000000000000000000000000000000000000000200A10004000296B933B10011000700CB0000000709100B46A5C14F100600".ToHexBytes( );
				back = ReadWriteCommand( receive.RemoveBegin( 59 ) );
			}
			return OperateResult.CreateSuccessResult( back );
		}

		private byte[] GetResponse( int status, byte[] data )
		{
			byte[] back = AllenBradleyHelper.PackRequestHeader( 0x70, sessionID, 
				AllenBradleyHelper.PackCommandSpecificData( 
					AllenBradleyHelper.PackCommandSingleService( "e8 a3 14 00".ToHexBytes( ), 0xA1 ),
					AllenBradleyHelper.PackCommandSingleService( SoftBasic.SpliceArray( "09 00 cb 00 00 00 07 09 10 0b 46 a5 c1 4f 00 08 00".ToHexBytes( ), data ), 0xB1 ) ) );
			this.ByteTransform.TransByte( status ).CopyTo( back, 8 );
			return back;
		}

		private int GetDynamicLengthData( byte[] fccc, ref int offset )
		{
			int data = fccc[offset++];
			if (data == 0xFF)
			{
				data = BitConverter.ToUInt16( fccc, offset );
				offset += 2;
			}
			return data;
		}

		private byte[] ReadWriteCommand( byte[] fccc )
		{
			int length           = fccc[5];
			int offset           = 6;
			int fileNumber       = GetDynamicLengthData( fccc, ref offset );
			byte fileType        = fccc[offset++];
			int elementNumber    = GetDynamicLengthData( fccc, ref offset );
			int subElementNumber = GetDynamicLengthData( fccc, ref offset );

			if (fccc[4] == 0xA2)
			{
				// 读
				switch (fileType)
				{
					case 0x8E: return GetResponse( 0x00, aBuffer.GetBytes( elementNumber, length ) );
					case 0x85: return GetResponse( 0x00, bBuffer.GetBytes( elementNumber, length ) );
					case 0x89: return GetResponse( 0x00, nBuffer.GetBytes( elementNumber, length ) );
					case 0x8A: return GetResponse( 0x00, fBuffer.GetBytes( elementNumber, length ) );
					case 0x84: return GetResponse( 0x00, sBuffer.GetBytes( elementNumber, length ) );
					case 0x83: return GetResponse( 0x00, iBuffer.GetBytes( elementNumber, length ) );
					case 0x82: return GetResponse( 0x00, oBuffer.GetBytes( elementNumber, length ) );
					default: return GetResponse( 0x01, null );
				}
			}
			else if (fccc[4] == 0xAA)
			{
				// 写
				byte[] data = fccc.RemoveBegin( offset );
				switch (fileType)
				{
					case 0x8E: aBuffer.SetBytes( data, elementNumber ); return GetResponse( 0x00, null );
					case 0x85: bBuffer.SetBytes( data, elementNumber ); return GetResponse( 0x00, null );
					case 0x89: nBuffer.SetBytes( data, elementNumber ); return GetResponse( 0x00, null );
					case 0x8A: fBuffer.SetBytes( data, elementNumber ); return GetResponse( 0x00, null );
					case 0x84: sBuffer.SetBytes( data, elementNumber ); return GetResponse( 0x00, null );
					case 0x83: iBuffer.SetBytes( data, elementNumber ); return GetResponse( 0x00, null );
					case 0x82: oBuffer.SetBytes( data, elementNumber ); return GetResponse( 0x00, null );
					default: return GetResponse( 0x01, null );
				}
			}
			else if (fccc[4] == 0xAB)
			{
				// 掩码写入
				SoftBuffer softBuffer = null;
				switch (fileType)
				{
					case 0x8E: softBuffer = aBuffer; break;
					case 0x85: softBuffer = bBuffer; break;
					case 0x89: softBuffer = nBuffer; break;
					case 0x8A: softBuffer = fBuffer; break;
					case 0x84: softBuffer = sBuffer; break;
					case 0x83: softBuffer = iBuffer; break;
					case 0x82: softBuffer = oBuffer; break;
					default: return GetResponse( 0x01, null );
				}
				int a = BitConverter.ToUInt16( fccc, offset );
				int b = BitConverter.ToUInt16( fccc, offset + 2 );
				int c = softBuffer.GetUInt16( elementNumber );
				ushort d = (ushort)(c & ~a | b);
				softBuffer.SetValue( d, elementNumber );
				return GetResponse( 0x00, null );
			}
			else
			{
				return GetResponse( 0x01, null );
			}
		}


		#endregion

		#region Private Member

		private SoftBuffer aBuffer;                      // A寄存器的数据池
		private SoftBuffer bBuffer;                      // B寄存器的数据池
		private SoftBuffer nBuffer;                      // N寄存器的数据池
		private SoftBuffer fBuffer;                      // F寄存器的数据池
		private SoftBuffer sBuffer;                      // S寄存器的数据池
		private SoftBuffer iBuffer;                      // I寄存器的数据池
		private SoftBuffer oBuffer;                      // O寄存器的数据池
		private uint sessionID = 0xC50359A2;             // 当前的会话的ID信息
		private const int DataPoolLength = 65536;        // 数据的长度

		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString( ) => $"AllenBradleyPcccServer[{Port}]";


		// 6f004000a25903c5000000005f7079636f6d6d5f00000000000000000a00020000000000b20030005402200624010a0500000000e8a31400270409100b46a5c10700000001402000f44301402000f443a303010020022401

		// 70002d00a25903c5000000005f7079636f6d6d5f00000000000000000a000200a1000400cc3159a2b100190003004b02206724010709100b46a5c10f000200a204088a0000       70002900a25903c5000000005f7079636f6d6d5f000000000000000000000200a1000400e8a31400b10015000300cb0000000709100b46a5c14f00020000007040
		// 70002d00a25903c5000000005f7079636f6d6d5f00000000000000000a000200a1000400cc3159a2b100190005004b02206724010709100b46a5c10f000400a204088a0100       70002900a25903c5000000005f7079636f6d6d5f000000000000000000000200a1000400e8a31400b10015000500cb0000000709100b46a5c14f00040000005040
		// 70002d00a25903c5000000005f7079636f6d6d5f00000000000000000a000200a1000400cc3159a2b100190007004b02206724010709100b46a5c10f000600a204088a0200       70002900a25903c5000000005f7079636f6d6d5f000000000000000000000200a1000400e8a31400b10015000700cb0000000709100b46a5c14f0006000000003f
		// 70002d00a25903c5000000005f7079636f6d6d5f00000000000000000a000200a1000400cc3159a2b100190009004b02206724010709100b46a5c10f000800a204088a0500       70002900a25903c5000000005f7079636f6d6d5f000000000000000000000200a1000400e8a31400b10015000900cb0000000709100b46a5c14f0008004a3a6e40
		// 70002d00a25903c5000000005f7079636f6d6d5f00000000000000000a000200a1000400cc3159a2b10019000b004b02206724010709100b46a5c10f000a00a2020b890000       70002700a25903c5000000005f7079636f6d6d5f000000000000000000000200a1000400e8a31400b10013000b00cb0000000709100b46a5c14f000a0082aa
		// 70002d00a25903c5000000005f7079636f6d6d5f00000000000000000a000200a1000400cc3159a2b10019000d004b02206724010709100b46a5c10f000c00a2020b890000       70002700a25903c5000000005f7079636f6d6d5f000000000000000000000200a1000400e8a31400b10013000d00cb0000000709100b46a5c14f000c0082aa
		// 70002d00a25903c5000000005f7079636f6d6d5f00000000000000000a000200a1000400cc3159a2b10019000f004b02206724010709100b46a5c10f000e00a2020b890000       70002700a25903c5000000005f7079636f6d6d5f000000000000000000000200a1000400e8a31400b10013000f00cb0000000709100b46a5c14f000e0082aa
		// 70002d00a25903c5000000005f7079636f6d6d5f00000000000000000a000200a1000400cc3159a2b100190011004b02206724010709100b46a5c10f001000a2020b890000       70002700a25903c5000000005f7079636f6d6d5f000000000000000000000200a1000400e8a31400b10013001100cb0000000709100b46a5c14f00100082aa
		// 70002d00a25903c5000000005f7079636f6d6d5f00000000000000000a000200a1000400cc3159a2b100190013004b02206724010709100b46a5c10f001200a2020b890000       70002700a25903c5000000005f7079636f6d6d5f000000000000000000000200a1000400e8a31400b10013001300cb0000000709100b46a5c14f00120082aa
		// 70002d00a25903c5000000005f7079636f6d6d5f00000000000000000a000200a1000400cc3159a2b100190015004b02206724010709100b46a5c10f001400a2020b890000       70002700a25903c5000000005f7079636f6d6d5f000000000000000000000200a1000400e8a31400b10013001500cb0000000709100b46a5c14f00140082aa
		// 70002d00a25903c5000000005f7079636f6d6d5f00000000000000000a000200a1000400cc3159a2b100190017004b02206724010709100b46a5c10f001600a2020b890000       70002700a25903c5000000005f7079636f6d6d5f000000000000000000000200a1000400e8a31400b10013001700cb0000000709100b46a5c14f00160082aa
		// 70002d00a25903c5000000005f7079636f6d6d5f00000000000000000a000200a1000400cc3159a2b100190019004b02206724010709100b46a5c10f001800a2020b890000       70002700a25903c5000000005f7079636f6d6d5f000000000000000000000200a1000400e8a31400b10013001900cb0000000709100b46a5c14f00180082aa
		//                                                                                                                                                  700025004A230F80000000000000000000000000000000000000000000000200A10004000296B933B10011000700CB0000000709100B46A5C14F100600


		#endregion
	}
}
