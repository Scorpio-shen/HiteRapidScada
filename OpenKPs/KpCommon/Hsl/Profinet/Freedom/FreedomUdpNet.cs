﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if !NET35 && !NET20
using System.Threading.Tasks;
#endif
using HslCommunication.Core;
using HslCommunication.Core.Net;
using HslCommunication.Reflection;

namespace HslCommunication.Profinet.Freedom
{
	/// <summary>
	/// 基于UDP/IP协议的自由协议，需要在地址里传入报文信息，也可以传入数据偏移信息，<see cref="NetworkUdpDeviceBase.ByteTransform"/>默认为<see cref="RegularByteTransform"/>
	/// </summary>
	/// <example>
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\FreedomExample.cs" region="Sample3" title="实例化" />
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\FreedomExample.cs" region="Sample4" title="读取" />
	/// </example>
	public class FreedomUdpNet : NetworkUdpDeviceBase
	{
		#region Constrcutor

		/// <summary>
		/// 实例化一个默认的对象
		/// </summary>
		public FreedomUdpNet( )
		{
			this.ByteTransform = new RegularByteTransform( );
		}

		/// <summary>
		/// 指定IP地址及端口号来实例化自由的TCP协议
		/// </summary>
		/// <param name="ipAddress">Ip地址</param>
		/// <param name="port">端口</param>
		public FreedomUdpNet( string ipAddress, int port )
		{
			this.IpAddress     = ipAddress;
			this.Port          = port;
			this.ByteTransform = new RegularByteTransform( );
		}

		#endregion

		#region Function

		/// <inheritdoc cref="FreedomTcpNet.CheckResponseStatus"/>
		public Func<byte[], byte[], OperateResult> CheckResponseStatus { get; set; }

		#endregion

		#region Read Write

		/// <inheritdoc cref="FreedomTcpNet.Read(string, ushort)"/>
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

		#region Object Override

		/// <inheritdoc/>
		public override string ToString( ) => $"FreedomUdpNet<{ByteTransform.GetType( )}>[{IpAddress}:{Port}]";

		#endregion
	}
}
