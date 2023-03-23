using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if !NET35 && !NET20
using System.Threading.Tasks;
#endif

namespace HslCommunication.Instrument.IEC.Helper
{
	/// <summary>
	/// IEC协议的辅助类信息
	/// </summary>
	public class IECHelper
	{
		/// <summary>
		/// 将IEC104的报文打包成完整的IEC104标准的协议报文
		/// </summary>
		/// <param name="controlField1">控制域1</param>
		/// <param name="controlField2">控制域2</param>
		/// <param name="controlField3">控制域3</param>
		/// <param name="controlField4">控制域4</param>
		/// <param name="asdu">ASDU报文，包含类型标识，可变结构限定词，传送原因，应用服务器数据单元公共地址，信息体</param>
		/// <returns>完整的报文消息</returns>
		public static byte[] PackIEC104Message( byte controlField1, byte controlField2, byte controlField3, byte controlField4, byte[] asdu )
		{
			byte[] content = new byte[6 + (asdu == null ? 0 : asdu.Length)];
			content[0] = 0x68;
			content[1] = (byte)(content.Length - 2);
			content[2] = controlField1;
			content[3] = controlField2;
			content[4] = controlField3;
			content[5] = controlField4;
			if (asdu != null && asdu.Length > 0) asdu.CopyTo( content, 6 );
			return content;
		}

		private static byte[] PackIEC104Message( ushort controlField1, ushort controlField2, byte[] asdu )
		{
			return PackIEC104Message(
				BitConverter.GetBytes( controlField1 )[0],
				BitConverter.GetBytes( controlField1 )[1],
				BitConverter.GetBytes( controlField2 )[0],
				BitConverter.GetBytes( controlField2 )[1],
				asdu
				);
		}

		/// <summary>
		/// 根据给定的时间，获取绝对时标的报文数据信息
		/// </summary>
		/// <param name="dateTime">时间信息</param>
		/// <param name="valid">时标是否有效</param>
		/// <returns>可用于发送的绝对时标的报文</returns>
		public static byte[] GetAbsoluteTimeScale( DateTime dateTime, bool valid )
		{
			byte[] buffer = new byte[7];
			buffer[0] = BitConverter.GetBytes( dateTime.Millisecond + dateTime.Second * 1000 )[0];
			buffer[1] = BitConverter.GetBytes( dateTime.Millisecond + dateTime.Second * 1000 )[1];
			buffer[2] = BitConverter.GetBytes( dateTime.Minute )[0];
			if (!valid) buffer[2] = (byte)(buffer[2] | 0x80);
			buffer[3] = BitConverter.GetBytes( dateTime.Hour )[0];
			int week = 1;
			switch (dateTime.DayOfWeek)
			{
				case DayOfWeek.Monday:    week = 1; break;
				case DayOfWeek.Tuesday:   week = 2; break;
				case DayOfWeek.Wednesday: week = 3; break;
				case DayOfWeek.Thursday:  week = 4; break;
				case DayOfWeek.Friday:    week = 5; break;
				case DayOfWeek.Saturday:  week = 6; break;
				case DayOfWeek.Sunday:    week = 7; break;
			}
			buffer[4] = BitConverter.GetBytes( dateTime.Day + week * 32 )[0];
			buffer[5] = BitConverter.GetBytes( dateTime.Month )[0];
			buffer[6] = BitConverter.GetBytes( dateTime.Year - 2000 )[0];
			return buffer;
		}

		/// <summary>
		/// 根据给定的绝对时标的原始内容，解析出实际的时间信息。
		/// </summary>
		/// <param name="source">原始字节</param>
		/// <param name="index">数据的偏移索引</param>
		/// <returns>时间信息</returns>
		public static DateTime PraseTimeFromAbsoluteTimeScale( byte[] source, int index )
		{
			int year   = (source[index + 6] & 0x7F) + 2000;
			int month  = source[index + 5] & 0x0F;
			int day    = source[index + 4] & 0x1F;
			int hour   = source[index + 3] & 0x1F;
			int minute = source[index + 2] & 0x3F;
			int second = BitConverter.ToUInt16( source, index + 0 );

			return new DateTime( year, month, day, hour, minute, second / 1000, second % 1000 );
		}


		/// <summary>
		/// 构建一个S帧协议的内容，需要传入接收需要信息
		/// </summary>
		/// <param name="receiveID">接收序号信息</param>
		/// <returns>S帧协议的报文信息</returns>
		public static byte[] BuildFrameSMessage( int receiveID )
		{
			receiveID *= 2;
			return PackIEC104Message( 0x01, (ushort)receiveID, null );
		}

		/// <summary>
		/// 构建一个U帧消息的报文信息，传入功能码，STARTDT: 0x07, STOPDT: 0x13; TESTFR: 0x43
		/// </summary>
		/// <param name="controlField">控制码信息</param>
		/// <returns>U帧的报文信息</returns>
		public static byte[] BuildFrameUMessage( byte controlField )
		{
			return PackIEC104Message( controlField, 0x00, 0x00, 0x00, null );
		}

		/// <summary>
		/// 构建一个I帧消息的报文信息，传入相关的参数信息，返回完整的104消息报文
		/// </summary>
		/// <param name="sendID">发送的序列号</param>
		/// <param name="receiveID">接收的序列号</param>
		/// <param name="typeId">类型标识</param>
		/// <param name="variableStructureQualifier">可变结构限定词</param>
		/// <param name="reason">传送原因</param>
		/// <param name="address">应用服务数据单元公共地址</param>
		/// <param name="body">信息体，最大243个字节的长度</param>
		/// <returns>用于发送的104报文信息</returns>
		public static byte[] BuildFrameIMessage( int sendID, int receiveID, byte typeId, byte variableStructureQualifier, ushort reason, ushort address, byte[] body )
		{
			sendID    *= 2;
			receiveID *= 2;
			byte[] asdu = new byte[6 + (body == null ? 0 : body.Length)];
			asdu[0] = typeId;
			asdu[1] = variableStructureQualifier;
			asdu[2] = BitConverter.GetBytes( reason )[0];
			asdu[3] = BitConverter.GetBytes( reason )[1];
			asdu[4] = BitConverter.GetBytes( address )[0];
			asdu[5] = BitConverter.GetBytes( address )[1];
			if (body?.Length > 0) body.CopyTo( asdu, 6 );
			return PackIEC104Message( (ushort)sendID, (ushort)receiveID, asdu );
		}

		/// <summary>
		/// U帧协议里，启动的功能
		/// </summary>
		public const byte IEC104ControlStartDT = 0x07;
		/// <summary>
		/// U帧协议里，停止的功能
		/// </summary>
		public const byte IEC104ControlStopDT = 0x13;
		/// <summary>
		/// U帧协议里，测试的功能，主站和子站均可发出
		/// </summary>
		public const byte IEC104ControlTestFR = 0x43;


	}
}
