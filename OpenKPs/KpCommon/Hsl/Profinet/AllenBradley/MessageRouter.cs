using HslCommunication.BasicFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HslCommunication.Profinet.AllenBradley
{
	/// <summary>
	/// 自定义的消息路由类，可以实现CIP协议自定义的路由消息<br />
	/// A custom message routing class that can implement custom routing messages of the CIP protocol
	/// </summary>
	public class MessageRouter
	{
		/// <summary>
		/// 实例化一个默认的实例对象<br />
		/// instantiate a default instance object
		/// </summary>
		public MessageRouter( )
		{
			_router[0] = 0x01;
			new byte[] { 0x0f, 0x02, 0x12, 0x01 }.CopyTo(_router, 1);
			_router[5] = 0x0c;
		}

		/// <summary>
		/// 指定路由来实例化一个对象，使用字符串的表示方式<br />
		/// Specify the route to instantiate an object, using the string representation
		/// </summary>
		/// <remarks>
		/// 路有消息支持两种格式，格式1：1.15.2.18.1.12   格式2： 1.1.2.130.133.139.61.1.0<br />
		/// There are two formats for the channel message, format 1: 1.15.2.18.1.12 format 2: 1.1.2.130.133.139.61.1.0
		/// </remarks>
		/// <param name="router">路由信息</param>
		public MessageRouter( string router )
		{
			string[] splits = router.Split( new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries );
			if (splits.Length <= 6)
			{
				if (splits.Length > 0) _router[0] = byte.Parse( splits[0] );
				if (splits.Length > 1) _router[1] = byte.Parse( splits[1] );
				if (splits.Length > 2) _router[2] = byte.Parse( splits[2] );
				if (splits.Length > 3) _router[3] = byte.Parse( splits[3] );
				if (splits.Length > 4) _router[4] = byte.Parse( splits[4] );
				if (splits.Length > 5) _router[5] = byte.Parse( splits[5] );
			}
			else if (splits.Length == 9)
			{
				string ip = splits[3] + "." + splits[4] + "." + splits[5] + "." + splits[6];
				_router = new byte[6 + ip.Length];
				_router[0] = byte.Parse( splits[0] );
				_router[1] = byte.Parse( splits[1] );
				_router[2] = (byte)(0x10 + byte.Parse( splits[2] ));
				_router[3] = (byte)ip.Length;
				Encoding.ASCII.GetBytes( ip ).CopyTo( _router, 4 );
				_router[_router.Length - 2] = byte.Parse( splits[7] );
				_router[_router.Length - 1] = byte.Parse( splits[8] );
			}
		}

		/// <summary>
		/// 使用完全自定义的消息路由来初始化数据<br />
		/// Use fully custom message routing to initialize data
		/// </summary>
		/// <param name="router">完全自定义的路由消息</param>
		public MessageRouter( byte[] router )
		{
			_router = router;
		}

		/// <summary>
		/// 获取路由信息
		/// </summary>
		/// <returns>路由消息的字节信息</returns>
		public byte[] GetRouter( ) => _router;

		/// <summary>
		/// 获取用于发送的CIP路由报文信息<br />
		/// Get information about CIP routing packets for sending
		/// </summary>
		/// <returns>路由信息</returns>
		public byte[] GetRouterCIP( )
		{
			byte[] router = this.GetRouter( );
			if (router.Length % 2 == 1) router = SoftBasic.SpliceArray( router, new byte[] { 0x00 } );  // 奇数长度的情况补0操作
			byte[] routerCip = new byte[46 + router.Length];
			"54022006240105f70200 00800100fe8002001b05 28a7fd03020000008084 1e00f44380841e00f443 a305".ToHexBytes( ).CopyTo( routerCip, 0 );
			router.CopyTo( routerCip, 42 );
			"20022401".ToHexBytes( ).CopyTo( routerCip, 42 + router.Length );
			routerCip[41] = (byte)(router.Length / 2);
			return routerCip;
		}

		/// <summary>
		/// 背板信息
		/// </summary>
		public byte Backplane { get => _router[0]; set => _router[0] = value; }

		/// <summary>
		/// 槽号信息
		/// </summary>
		public byte Slot { get => _router[5]; set => _router[5] = value; }


		private byte[] _router = new byte[6];
	}
}
