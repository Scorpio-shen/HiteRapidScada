﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HslCommunication.Core.Net;
using HslCommunication.Core.IMessage;
using HslCommunication.Core;
using HslCommunication;
using HslCommunication.Reflection;
#if !NET35 && !NET20
using System.Threading.Tasks;
#endif

namespace HslCommunication.Profinet.Freedom
{
	/// <summary>
	/// 基于TCP/IP协议的自由协议，需要在地址里传入报文信息，也可以传入数据偏移信息，<see cref="NetworkDoubleBase.ByteTransform"/>默认为<see cref="RegularByteTransform"/>
	/// </summary>
	/// <example>
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\FreedomExample.cs" region="Sample1" title="实例化" />
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\FreedomExample.cs" region="Sample2" title="连接及读取" />
	/// </example>
	public class FreedomTcpNet : NetworkDeviceBase
	{
		#region Constrcutor

		/// <summary>
		/// 实例化一个默认的对象
		/// </summary>
		public FreedomTcpNet( )
		{
			this.ByteTransform = new RegularByteTransform( );
		}

		/// <summary>
		/// 指定IP地址及端口号来实例化自由的TCP协议
		/// </summary>
		/// <param name="ipAddress">Ip地址</param>
		/// <param name="port">端口</param>
		public FreedomTcpNet(string ipAddress, int port ) : this( )
		{
			this.IpAddress     = ipAddress;
			this.Port          = port;
		}

		/// <inheritdoc/>
		protected override INetMessage GetNewNetMessage( ) => NetMessage;

		#endregion

		#region Function

		/// <summary>
		/// 一个对返回数据合法检查的委托实例，默认为空，不进行合法性检查，可以实例化实现任意的报文检测，返回是否合法结果。<br />
		/// A delegate instance that checks the legality of the returned data. It is empty by default and does not perform legality checking. 
		/// It can be instantiated to implement any packet inspection and return whether the result is legal or not.
		/// </summary>
		/// <remarks>
		/// 例如返回的第一个字节为0表示正常报文，否则是异常返回，可以简写：CheckResponseStatus = ( send, receive ) => receive[2] == 0 ? OperateResult.CreateSuccessResult( ) : new OperateResult( receive[2], "error" );<br />
		/// For example, if the first byte returned is 0, it means a normal message, otherwise it is an abnormal return, 
		/// which can be abbreviated as: CheckResponseStatus = ( send, receive ) => receive[2] == 0 ? OperateResult.CreateSuccessResult( ) : new OperateResult( receive[2], "error" );
		/// </remarks>
		public Func<byte[], byte[], OperateResult> CheckResponseStatus { get; set; }

		/// <summary>
		/// 如果当前的报文使用了固定报文头加剩余报文长度来描述完整报文的情况下，可以自定义实例化报文消息对象，可以更快更完整的接收全部报文的数据。<br />
		/// If the current message uses a fixed header and the remaining message length to describe the complete message, 
		/// you can customize the instantiated message object to receive the data of all messages faster and more completely.
		/// </summary>
		/// <remarks>
		/// 例如当前的报文是modbustcp协议的话，NetMessage = new HslCommunication.Core.IMessage.ModbusTcpMessage( );<br />
		/// For example, if the current message is modbustcp protocol, NetMessage = new HslCommunication.Core.IMessage.ModbusTcpMessage( );
		/// </remarks>
		public INetMessage NetMessage { get; set; }

		#endregion

		#region Read Write

		/// <inheritdoc/>
		/// <remarks>
		/// length没有任何意义，需要传入原始的字节报文，例如：stx=9;00 00 00 00 00 06 01 03 00 64 00 01，stx得值用于获取数据移除的前置报文头，可以不填写<br />
		/// length has no meaning, you need to pass in the original byte message, for example: stx=9;00 00 00 00 00 06 01 03 00 64 00 01, 
		/// the value of stx is used to obtain the preamble header for data removal, can be left blank
		/// </remarks>
		[HslMqttApi( "ReadByteArray", "特殊的地址格式，需要采用解析包起始地址的报文，例如 modbus 协议为 stx=9;00 00 00 00 00 06 01 03 00 64 00 01" )]
		public override OperateResult<byte[]> Read( string address, ushort length )
		{
			int startIndex = HslHelper.ExtractParameter( ref address, "stx", 0x00 );
			byte[] send = address.ToHexBytes( );

			OperateResult<byte[]> read = ReadFromCoreServer( send );
			if (!read.IsSuccess) return read;

			if (CheckResponseStatus != null)
			{
				OperateResult check = CheckResponseStatus.Invoke( send, read.Content );
				if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( check );
			}

			if (startIndex >= read.Content.Length) return new OperateResult<byte[]>( StringResources.Language.ReceiveDataLengthTooShort );
			return OperateResult.CreateSuccessResult( read.Content.RemoveBegin( startIndex ) );
		}

		/// <inheritdoc/>
		public override OperateResult Write( string address, byte[] value ) => Read( address, 0 );

		#endregion

		#region Read Write Async
#if !NET35 && !NET20
		/// <inheritdoc cref="Read(string, ushort)"/>
		public async override Task<OperateResult<byte[]>> ReadAsync( string address, ushort length )
		{
			int startIndex = HslHelper.ExtractParameter( ref address, "stx", 0x00 );
			byte[] send = address.ToHexBytes( );

			OperateResult<byte[]> read = await ReadFromCoreServerAsync( send );
			if (!read.IsSuccess) return read;

			if (CheckResponseStatus != null)
			{
				OperateResult check = CheckResponseStatus.Invoke( send, read.Content );
				if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( check );
			}

			if (startIndex >= read.Content.Length) return new OperateResult<byte[]>( StringResources.Language.ReceiveDataLengthTooShort );
			return OperateResult.CreateSuccessResult( read.Content.RemoveBegin( startIndex ) );
		}

		/// <inheritdoc/>
		public async override Task<OperateResult> WriteAsync( string address, byte[] value ) => await ReadAsync( address, 0 );
#endif
		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString( ) => $"FreedomTcpNet<{ByteTransform.GetType( )}>[{IpAddress}:{Port}]";

		#endregion
	}
}
