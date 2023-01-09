using HslCommunication.BasicFramework;
using HslCommunication.Core.IMessage;
using HslCommunication.Profinet.AllenBradley;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
#if !NET35 && !NET20
using System.Threading.Tasks;
#endif

namespace HslCommunication.Core.Net
{
	/// <summary>
	/// 基于连接的CIP协议的基类
	/// </summary>
	public class NetworkConnectedCip : NetworkDeviceBase
	{
		/// <summary>
		/// 实例化一个默认的对象
		/// </summary>
		public NetworkConnectedCip( )
		{

		}

		/// <inheritdoc/>
		protected override INetMessage GetNewNetMessage( ) => new AllenBradleyMessage( );

		/// <inheritdoc/>
		public override byte[] PackCommandWithHeader( byte[] command )
		{
			return AllenBradleyHelper.PackRequestHeader( 0x70, SessionHandle, AllenBradleyHelper.PackCommandSpecificData(
				GetOTConnectionIdService( ), command ) );
		}


		#region Double Mode Override


		/// <inheritdoc/>
		protected override OperateResult InitializationOnConnect( Socket socket )
		{
			// Registering Session Information
			OperateResult<byte[]> read1 = ReadFromCoreServer( socket, AllenBradleyHelper.RegisterSessionHandle( BitConverter.GetBytes( Interlocked.Increment( ref context ) ) ), hasResponseData: true, usePackAndUnpack: false );
			if (!read1.IsSuccess) return read1;

			// Check the returned status
			OperateResult check = AllenBradleyHelper.CheckResponse( read1.Content );
			if (!check.IsSuccess) return check;

			// Extract session ID
			uint sessionHandle = ByteTransform.TransUInt32( read1.Content, 4 );

			// Open forward 10 times
			for (int i = 0; i < 10; i++)
			{
				long id = Interlocked.Increment( ref openForwardId );
				// Large Forward Open(Message Router)

				ushort tick = i < 7 ? (ushort)i : (ushort)HslHelper.HslRandom.Next( 7, 200 );
				OperateResult<byte[]> read2 = ReadFromCoreServer( socket, AllenBradleyHelper.PackRequestHeader( 0x6f, sessionHandle,
					GetLargeForwardOpen( tick ), ByteTransform.TransByte( id ) ), hasResponseData: true, usePackAndUnpack: false );
				if (!read2.IsSuccess) return read2;

				try
				{
					if (read2.Content.Length >= 46 && read2.Content[42] != 0x00)
					{
						ushort err = ByteTransform.TransUInt16( read2.Content, 44 );
						if (err == 0x100 && i < 9) continue;

						if      (err == 0x100) return new OperateResult( "Connection in use or duplicate Forward Open" );
						else if (err == 0x113) return new OperateResult( "Extended Status: Out of connections (0x0113)" );
						return new OperateResult( "Forward Open failed, Code: " + ByteTransform.TransUInt16( read2.Content, 44 ) );
					}
					else
					{
						// Extract Connection ID
						OTConnectionId = ByteTransform.TransUInt32( read2.Content, 44 );
						break;
					}
				}
				catch (Exception ex)
				{
					return new OperateResult( ex.Message + Environment.NewLine + "Source: " + read2.Content.ToHexString( ' ' ) );
				}
			}
			// Reset Message Id
			incrementCount.ResetCurrentValue( );
			SessionHandle = sessionHandle;

			return OperateResult.CreateSuccessResult( );
		}

		/// <inheritdoc/>
		protected override OperateResult ExtraOnDisconnect( Socket socket )
		{
			if (socket == null) return OperateResult.CreateSuccessResult( );

			// Forward Close(Message Router)
			byte[] forwardClose = GetLargeForwardClose( );
			if (forwardClose != null)
			{
				OperateResult<byte[]> close = ReadFromCoreServer( socket, AllenBradleyHelper.PackRequestHeader( 0x6f, SessionHandle, forwardClose ), hasResponseData: true, usePackAndUnpack: false );
				if (!close.IsSuccess) return close;
			}

			// Unregister session Information
			OperateResult<byte[]> read = ReadFromCoreServer( socket, AllenBradleyHelper.UnRegisterSessionHandle( SessionHandle ), hasResponseData: true, usePackAndUnpack: false );
			if (!read.IsSuccess) return read;

			return OperateResult.CreateSuccessResult( );
		}

#if !NET35 && !NET20

		/// <inheritdoc/>
		protected override async Task<OperateResult> InitializationOnConnectAsync( Socket socket )
		{
			// Registering Session Information
			OperateResult<byte[]> read1 = await ReadFromCoreServerAsync( socket, AllenBradleyHelper.RegisterSessionHandle( BitConverter.GetBytes( Interlocked.Increment( ref context ) ) ), hasResponseData: true, usePackAndUnpack: false );
			if (!read1.IsSuccess) return read1;

			// Check the returned status
			OperateResult check = AllenBradleyHelper.CheckResponse( read1.Content );
			if (!check.IsSuccess) return check;

			// Extract session ID
			uint sessionHandle = ByteTransform.TransUInt32( read1.Content, 4 );

			// Open forward 5 times
			for (int i = 0; i < 10; i++)
			{
				long id = Interlocked.Increment( ref openForwardId );
				// Large Forward Open(Message Router)

				ushort tick = i < 7 ? (ushort)i : (ushort)HslHelper.HslRandom.Next( 7, 200 );
				OperateResult<byte[]> read2 = await ReadFromCoreServerAsync( socket, AllenBradleyHelper.PackRequestHeader( 0x6f, sessionHandle,
					GetLargeForwardOpen( tick ), ByteTransform.TransByte( id ) ), hasResponseData: true, usePackAndUnpack: false );
				if (!read2.IsSuccess) return read2;

				try
				{
					if (read2.Content.Length >= 46 && read2.Content[42] != 0x00)
					{
						ushort err = ByteTransform.TransUInt16( read2.Content, 44 );
						if (err == 0x100 && i < 9) continue;

						if (err == 0x100) return new OperateResult( "Connection in use or duplicate Forward Open" );
						else if (err == 0x113) return new OperateResult( "Extended Status: Out of connections (0x0113)" );
						return new OperateResult( "Forward Open failed, Code: " + ByteTransform.TransUInt16( read2.Content, 44 ) );
					}
					else
					{
						// Extract Connection ID
						OTConnectionId = ByteTransform.TransUInt32( read2.Content, 44 );
						break;
					}
				}
				catch (Exception ex)
				{
					return new OperateResult( ex.Message + Environment.NewLine + "Source: " + read2.Content.ToHexString( ' ' ) );
				}
			}
			// Reset Message Id
			incrementCount.ResetCurrentValue( );
			SessionHandle = sessionHandle;

			return OperateResult.CreateSuccessResult( );
		}

		/// <inheritdoc/>
		protected override async Task<OperateResult> ExtraOnDisconnectAsync( Socket socket )
		{
			if (socket == null) return OperateResult.CreateSuccessResult( );

			// Forward Close(Message Router)
			byte[] forwardClose = GetLargeForwardClose( );
			if (forwardClose != null)
			{
				OperateResult<byte[]> close = await ReadFromCoreServerAsync( socket, AllenBradleyHelper.PackRequestHeader( 0x6f, SessionHandle, forwardClose ), hasResponseData: true, usePackAndUnpack: false );
				if (!close.IsSuccess) return close;
			}

			// Unregister session Information
			OperateResult<byte[]> read = await ReadFromCoreServerAsync( socket, AllenBradleyHelper.UnRegisterSessionHandle( SessionHandle ), hasResponseData: true, usePackAndUnpack: false );
			if (!read.IsSuccess) return read;

			return OperateResult.CreateSuccessResult( );
		}

#endif
		#endregion

		#region Public Properties

		/// <inheritdoc cref="AllenBradleyNet.SessionHandle"/>
		public uint SessionHandle { get; protected set; }

		/// <summary>
		/// O -> T Network Connection ID
		/// </summary>
		protected uint OTConnectionId = 0;

		/// <summary>
		/// T -> O Network Connection ID
		/// </summary>
		protected uint TOConnectionId = 0;

		#endregion

		#region Protect Method

		/// <summary>
		/// 将多个的CIP命令打包成一个服务的命令
		/// </summary>
		/// <param name="cip">CIP命令列表</param>
		/// <returns>服务命令</returns>
		protected byte[] PackCommandService( params byte[][] cip )
		{
			MemoryStream ms = new MemoryStream( );
			// type id   0xB2:UnConnected Data Item  0xB1:Connected Data Item  0xA1:Connect Address Item
			ms.WriteByte( 0xB1 );
			ms.WriteByte( 0x00 );
			ms.WriteByte( 0x00 );     // 后续数据的长度
			ms.WriteByte( 0x00 );

			long messageId = incrementCount.GetCurrentValue( );
			ms.WriteByte( BitConverter.GetBytes( messageId )[0] );     // CIP Sequence Count 一个累加的CIP序号
			ms.WriteByte( BitConverter.GetBytes( messageId )[1] );

			if (cip.Length == 1)
			{
				ms.Write( cip[0], 0, cip[0].Length );
			}
			else
			{
				ms.Write( new byte[] { 0x0A, 0x02, 0x20, 0x02, 0x24, 0x01 }, 0, 6 );
				ms.WriteByte( BitConverter.GetBytes( cip.Length )[0] );
				ms.WriteByte( BitConverter.GetBytes( cip.Length )[1] );
				int offset = 2 + cip.Length * 2;
				for (int i = 0; i < cip.Length; i++)
				{
					ms.WriteByte( BitConverter.GetBytes( offset )[0] );     // 各个数据的长度
					ms.WriteByte( BitConverter.GetBytes( offset )[1] );
					offset += cip[i].Length;
				}
				for (int i = 0; i < cip.Length; i++)
				{
					ms.Write( cip[i], 0, cip[i].Length );     // 写入欧姆龙CIP的具体内容
				}
			}

			byte[] data = ms.ToArray( );
			ms.Dispose( );
			BitConverter.GetBytes( (ushort)(data.Length - 4) ).CopyTo( data, 2 );
			return data;
		}

		/// <summary>
		/// 获取数据通信的前置打开命令，不同的PLC的信息不一样。
		/// </summary>
		/// <param name="connectionID">连接的ID信息</param>
		/// <returns>原始命令数据</returns>
		protected virtual byte[] GetLargeForwardOpen( ushort connectionID )
		{
			return @"
00 00 00 00 00 00 02 00 00 00 00 00 b2 00 34 00
5b 02 20 06 24 01 0e 9c 02 00 00 80 01 00 fe 80
02 00 1b 05 30 a7 2b 03 02 00 00 00 80 84 1e 00
cc 07 00 42 80 84 1e 00 cc 07 00 42 a3 03 20 02
24 01 2c 01".ToHexBytes( );
		}

		/// <summary>
		/// 获取数据通信的后置关闭命令，不同的PLC的信息不一样。
		/// </summary>
		/// <returns>原始命令数据</returns>
		protected virtual byte[] GetLargeForwardClose( )
		{
			return null;
		}

		#endregion

		#region Private Method

		private byte[] GetOTConnectionIdService( )
		{
			byte[] buffer = new byte[8];
			buffer[0] = 0xA1;  // Connected Address Item
			buffer[1] = 0x00;
			buffer[2] = 0x04;  // Length
			buffer[3] = 0x00;
			ByteTransform.TransByte( OTConnectionId ).CopyTo( buffer, 4 );
			return buffer;
		}

		#endregion

		#region Private

		private SoftIncrementCount incrementCount = new SoftIncrementCount( 65535, 3, 2 );
		private long openForwardId = 0x100;
		private long context = 0;

		#endregion

		#region Static Helper

		/// <summary>
		/// 从PLC反馈的数据解析出真实的数据内容，结果内容分别是原始字节数据，数据类型代码，是否有很多的数据<br />
		/// The real data content is parsed from the data fed back by the PLC. The result content is the original byte data, 
		/// the data type code, and whether there is a lot of data.
		/// </summary>
		/// <param name="response">PLC的反馈数据</param>
		/// <param name="isRead">是否是返回的操作</param>
		/// <returns>带有结果标识的最终数据</returns>
		public static OperateResult<byte[], ushort, bool> ExtractActualData( byte[] response, bool isRead )
		{
			List<byte> data = new List<byte>( );

			int offset = 42;
			bool hasMoreData = false;
			ushort dataType = 0;
			ushort count = BitConverter.ToUInt16( response, offset );    // 剩余总字节长度，在剩余的字节里，有可能是一项数据，也有可能是多项
			if (BitConverter.ToInt32( response, 46 ) == 0x8A)
			{
				// 多项数据
				offset = 50;
				int dataCount = BitConverter.ToUInt16( response, offset );
				for (int i = 0; i < dataCount; i++)
				{
					int offsetStart = BitConverter.ToUInt16( response, offset + 2 + i * 2 ) + offset;
					int offsetEnd = (i == dataCount - 1) ? response.Length : (BitConverter.ToUInt16( response, (offset + 4 + i * 2) ) + offset);
					ushort err = BitConverter.ToUInt16( response, offsetStart + 2 );
					switch (err)
					{
						case 0x04: return new OperateResult<byte[], ushort, bool>( ) { ErrorCode = err, Message = StringResources.Language.AllenBradley04 };
						case 0x05: return new OperateResult<byte[], ushort, bool>( ) { ErrorCode = err, Message = StringResources.Language.AllenBradley05 };
						case 0x06:
							{
								// 06的错误码通常是数据长度太多了
								// CC是符号返回，D2是符号片段返回， D5是列表数据
								if (response[offset + 2] == 0xD2 || response[offset + 2] == 0xCC)
									return new OperateResult<byte[], ushort, bool>( ) { ErrorCode = err, Message = StringResources.Language.AllenBradley06 };
								break;
							}
						case 0x0A: return new OperateResult<byte[], ushort, bool>( ) { ErrorCode = err, Message = StringResources.Language.AllenBradley0A };
						case 0x13: return new OperateResult<byte[], ushort, bool>( ) { ErrorCode = err, Message = StringResources.Language.AllenBradley13 };
						case 0x1C: return new OperateResult<byte[], ushort, bool>( ) { ErrorCode = err, Message = StringResources.Language.AllenBradley1C };
						case 0x1E: return new OperateResult<byte[], ushort, bool>( ) { ErrorCode = err, Message = StringResources.Language.AllenBradley1E };
						case 0x26: return new OperateResult<byte[], ushort, bool>( ) { ErrorCode = err, Message = StringResources.Language.AllenBradley26 };
						case 0x00: break;
						default: return new OperateResult<byte[], ushort, bool>( ) { ErrorCode = err, Message = StringResources.Language.UnknownError };
					}

					if (isRead)
					{
						for (int j = offsetStart + 6; j < offsetEnd; j++)
						{
							data.Add( response[j] );
						}
					}
				}
			}
			else
			{
				byte err = response[offset + 6];
				switch (err)
				{
					case 0x04: return new OperateResult<byte[], ushort, bool>( ) { ErrorCode = err, Message = StringResources.Language.AllenBradley04 };
					case 0x05: return new OperateResult<byte[], ushort, bool>( ) { ErrorCode = err, Message = StringResources.Language.AllenBradley05 };
					case 0x06: hasMoreData = true; break;
					case 0x0A: return new OperateResult<byte[], ushort, bool>( ) { ErrorCode = err, Message = StringResources.Language.AllenBradley0A };
					case 0x13: return new OperateResult<byte[], ushort, bool>( ) { ErrorCode = err, Message = StringResources.Language.AllenBradley13 };
					case 0x1C: return new OperateResult<byte[], ushort, bool>( ) { ErrorCode = err, Message = StringResources.Language.AllenBradley1C };
					case 0x1E: return new OperateResult<byte[], ushort, bool>( ) { ErrorCode = err, Message = StringResources.Language.AllenBradley1E };
					case 0x26: return new OperateResult<byte[], ushort, bool>( ) { ErrorCode = err, Message = StringResources.Language.AllenBradley26 };
					case 0x00: break;
					default: return new OperateResult<byte[], ushort, bool>( ) { ErrorCode = err, Message = StringResources.Language.UnknownError };
				}

				if (response[offset + 4] == 0xCD || response[offset + 4] == 0xD3) return OperateResult.CreateSuccessResult( data.ToArray( ), dataType, hasMoreData );

				if (response[offset + 4] == 0xCC || response[offset + 4] == 0xD2)
				{
					for (int i = offset + 10; i < offset + 2 + count; i++)
					{
						data.Add( response[i] );
					}
					dataType = BitConverter.ToUInt16( response, offset + 8 );
				}
				else if (response[offset + 4] == 0xD5)
				{
					for (int i = offset + 8; i < offset + 2 + count; i++)
					{
						data.Add( response[i] );
					}
				}
				else if (response[offset + 4] == 0xCB)
				{
					// PCCC的格式返回
					if (response[58] != 0x00) return new OperateResult<byte[], ushort, bool>( response[58], AllenBradleyDF1Serial.GetExtStatusDescription( response[58] ) + Environment.NewLine +
						"Source: " + response.RemoveBegin( 57 ).ToHexString( ' ' ) );
					if (!isRead) return OperateResult.CreateSuccessResult( data.ToArray( ), dataType, hasMoreData );
					return OperateResult.CreateSuccessResult( response.RemoveBegin( 61 ), dataType, hasMoreData );
				}
			}

			return OperateResult.CreateSuccessResult( data.ToArray( ), dataType, hasMoreData );
		}

		#endregion

	}
}
