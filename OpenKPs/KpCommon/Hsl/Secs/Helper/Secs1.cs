using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HslCommunication.BasicFramework;
using HslCommunication.Serial;

namespace HslCommunication.Secs.Helper
{
	/// <summary>
	/// Secs-1的协议信息
	/// </summary>
	public class Secs1
	{
		/// <summary>
		/// 根据传入的参数信息，构建完整的SECS消息报文列表
		/// </summary>
		/// <param name="deviceID">装置识别码</param>
		/// <param name="streamNo">主功能码</param>
		/// <param name="functionNo">子功能码</param>
		/// <param name="blockNo">数据块号</param>
		/// <param name="messageID">消息序号</param>
		/// <param name="data">真实数据消息</param>
		/// <param name="wBit">是否必须回复讯息</param>
		/// <returns>完整的报文信息</returns>
		public static List<byte[]> BuildSecsOneMessage( ushort deviceID, byte streamNo, byte functionNo, ushort blockNo, uint messageID, byte[] data, bool wBit )
		{
			List<byte[]> list = new List<byte[]>( );
			List<byte[]> dataList = data.Length <= 244 ? SoftBasic.ArraySplitByLength( data, 244 ) : SoftBasic.ArraySplitByLength( data, 224 );

			for (int i = 0; i < dataList.Count; i++)
			{
				byte[] buffer = new byte[13 + dataList[i].Length];
				buffer[ 0] = (byte)(10 + dataList[i].Length);         // Length
				buffer[ 1] = BitConverter.GetBytes( deviceID )[1];    // Upper DeviceID
				buffer[ 2] = BitConverter.GetBytes( deviceID )[0];    // Lower DeviceID
				buffer[ 3] = wBit ? (byte)(streamNo | 0x80) : streamNo;
				buffer[ 4] = functionNo;
				buffer[ 5] = (i == dataList.Count - 1) ? (byte)(BitConverter.GetBytes( blockNo )[1] | 0x80) : BitConverter.GetBytes( blockNo )[1];
				buffer[ 6] = BitConverter.GetBytes( blockNo )[0];
				buffer[ 7] = BitConverter.GetBytes( messageID )[3];
				buffer[ 8] = BitConverter.GetBytes( messageID )[2];
				buffer[ 9] = BitConverter.GetBytes( messageID )[1];
				buffer[10] = BitConverter.GetBytes( messageID )[0];
				dataList[i].CopyTo( buffer, 11 );

				int sum = SoftLRC.CalculateAcc( buffer, 1, 2 );
				buffer[buffer.Length - 2] = BitConverter.GetBytes( sum )[1];
				buffer[buffer.Length - 1] = BitConverter.GetBytes( sum )[0];
				list.Add( buffer );
			}
			return list;
		}

		/// <summary>
		/// 根据传入的参数信息，构建完整的SECS/HSMS消息报文列表
		/// </summary>
		/// <param name="deviceID">装置识别码</param>
		/// <param name="streamNo">主功能码</param>
		/// <param name="functionNo">子功能码</param>
		/// <param name="blockNo">数据块号</param>
		/// <param name="messageID">消息序号</param>
		/// <param name="data">真实数据消息</param>
		/// <param name="wBit">是否必须回复讯息</param>
		/// <returns>完整的报文信息</returns>
		public static byte[] BuildHSMSMessage( ushort deviceID, byte streamNo, byte functionNo, ushort blockNo, uint messageID, byte[] data, bool wBit )
		{
			if (data == null) data = new byte[0];

			byte[] buffer = new byte[14 + data.Length];
			buffer[ 0] = BitConverter.GetBytes( buffer.Length - 4 )[3];         // Length
			buffer[ 1] = BitConverter.GetBytes( buffer.Length - 4 )[2];         // Length
			buffer[ 2] = BitConverter.GetBytes( buffer.Length - 4 )[1];         // Length
			buffer[ 3] = BitConverter.GetBytes( buffer.Length - 4 )[0];         // Length
			buffer[ 4] = BitConverter.GetBytes( deviceID )[1];                  // Upper DeviceID
			buffer[ 5] = BitConverter.GetBytes( deviceID )[0];                  // Lower DeviceID
			buffer[ 6] = wBit ? (byte)(streamNo | 0x80) : streamNo;
			buffer[ 7] = functionNo;
			buffer[ 8] = (byte)(BitConverter.GetBytes( blockNo )[1] ); // | 0x80
			buffer[ 9] = BitConverter.GetBytes( blockNo )[0];
			buffer[10] = BitConverter.GetBytes( messageID )[3];
			buffer[11] = BitConverter.GetBytes( messageID )[2];
			buffer[12] = BitConverter.GetBytes( messageID )[1];
			buffer[13] = BitConverter.GetBytes( messageID )[0];
			data.CopyTo( buffer, 14 );

			return buffer;
		}


	}
}
