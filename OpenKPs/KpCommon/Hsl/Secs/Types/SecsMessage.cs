using HslCommunication.Secs.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HslCommunication.Secs.Types
{
	/// <summary>
	/// Secs的消息类对象
	/// </summary>
	public class SecsMessage
	{
		/// <summary>
		/// 实例化一个默认的对象
		/// </summary>
		public SecsMessage( )
		{

		}

		/// <inheritdoc cref="SecsMessage.SecsMessage(byte[], int )"/>
		public SecsMessage( byte[] message ) : this( message, 0 )
		{
		}

		/// <summary>
		/// 通过原始的报文信息来实例化一个默认的对象
		/// </summary>
		/// <param name="message">原始的字节信息</param>
		/// <param name="startIndex">起始的偏移地址</param>
		public SecsMessage( byte[] message, int startIndex )
		{
			byte[] deviceid = new byte[2];
			deviceid[0] = message[startIndex + 1];
			deviceid[1] = (byte)(message[startIndex] & 0x7F);
			DeviceID    = BitConverter.ToUInt16( deviceid, 0 );

			R = (message[startIndex + 0] & 0x80) == 0x80;
			W = (message[startIndex + 2] & 0x80) == 0x80;
			E = (message[startIndex + 4] & 0x80) == 0x80;

			StreamNo      = (byte)(message[startIndex + 2] & 0x7F);
			FunctionNo    = (byte)(message[startIndex + 3] & 0x7F);

			byte[] buffer = new byte[2];
			buffer[0]     = (byte)(message[startIndex + 4] & 0x7F);
			buffer[1]     = message[startIndex + 5];
			BlockNo       = Secs2.SecsTransform.TransInt16( buffer, 0 );
			MessageID     = Secs2.SecsTransform.TransUInt32( message, startIndex + 6 );

			Data          = message.RemoveBegin( startIndex + 10 );
		}

		/// <summary>
		/// 设备的ID信息
		/// </summary>
		public ushort DeviceID { get; set; }

		/// <summary>
		/// R=false, Host → Equipment; R=true, Host ← Equipment
		/// </summary>
		public bool R { get; set; }

		/// <summary>
		/// W=false, 不必回复讯息；W=true, 必须回复讯息
		/// </summary>
		public bool W { get; set; }

		/// <summary>
		/// E=false, 尚有Block; E=true, 此为最后一个Block
		/// </summary>
		public bool E { get; set; }

		/// <summary>
		/// Stream功能码
		/// </summary>
		public byte StreamNo { get; set; }

		/// <summary>
		/// Function功能码
		/// </summary>
		public byte FunctionNo { get; set; }

		/// <summary>
		/// 获取或设置区块号信息
		/// </summary>
		public int BlockNo { get; set; }

		/// <summary>
		/// 获取或设置消息ID信息
		/// </summary>
		public uint MessageID { get; set; }

		/// <summary>
		/// 消息数据对象
		/// </summary>
		public byte[] Data { get; set; }

		/// <summary>
		/// 获取或设置用于字符串解析的编码信息
		/// </summary>
		public Encoding StringEncoding { get; set; } = Encoding.Default;

		/// <summary>
		/// 获取当前消息的所有对象信息
		/// </summary>
		/// <returns>Secs数据对象</returns>
		public SecsValue GetItemValues( ) => Secs2.ExtraToSecsItemValue( Data, StringEncoding );

		/// <summary>
		/// 使用指定的编码获取当前消息的所有对象信息
		/// </summary>
		/// <param name="encoding">自定义的编码信息</param>
		/// <returns>Secs数据对象</returns>
		public SecsValue GetItemValues( Encoding encoding ) => Secs2.ExtraToSecsItemValue( Data, encoding );

		/// <inheritdoc/>
		public override string ToString( )
		{
			SecsValue value = GetItemValues( this.StringEncoding );
			if (StreamNo == 0 && FunctionNo == 0) return $"S{StreamNo}F{FunctionNo}{(W ? "W" : string.Empty)} B{BlockNo}";
			if (value == null)
				return $"S{StreamNo}F{FunctionNo}{(W ? "W" : string.Empty)}";
			else
				return $"S{StreamNo}F{FunctionNo}{(W ? "W" : string.Empty)} {Environment.NewLine}{value}";
		}

	}

	/// <summary>
	/// 扩展类
	/// </summary>
	public static class SecsMessageExtension
	{
		/// <summary>
		/// 获取显示的字符串文本信息
		/// </summary>
		/// <param name="secsMessages">SECS消息类</param>
		/// <returns>字符串信息</returns>
		public static string ToRenderString( this SecsValue[] secsMessages )
		{
			if (secsMessages == null || secsMessages.Length == 0) return string.Empty;
			StringBuilder stringBuilder = new StringBuilder( );
			foreach (var secsMessage in secsMessages)
			{
				stringBuilder.Append( Environment.NewLine );
				stringBuilder.Append( secsMessage.ToString( ) );
			}
			return stringBuilder.ToString( );
		}
	}
}
