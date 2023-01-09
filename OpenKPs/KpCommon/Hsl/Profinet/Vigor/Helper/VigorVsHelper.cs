using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HslCommunication.Core.Address;
using System.IO;
using System.Net.Sockets;
using HslCommunication.BasicFramework;
#if !NET35 && !NET20
using System.Threading.Tasks;
#endif

namespace HslCommunication.Profinet.Vigor.Helper
{
	/// <summary>
	/// 丰炜PLC的辅助类对象
	/// </summary>
	public class VigorVsHelper
	{
		internal static byte[] PackCommand( byte[] command, byte code = 0x02 )
		{
			if (command == null) command = new byte[0];
			MemoryStream ms = new MemoryStream( );
			ms.WriteByte( 0x10 );
			ms.WriteByte( code );
			int sum = 0;
			for (int i = 0; i < command.Length; i++)
			{
				sum += command[i];
				ms.WriteByte( command[i] );
				if (command[i] == 0x10) ms.WriteByte( command[i] ); 
			}
			ms.WriteByte( 0x10 );
			ms.WriteByte( 0x03 );
			byte[] crc = Encoding.ASCII.GetBytes( (sum % 256).ToString( "X2" ) );
			ms.WriteByte( crc[0] );
			ms.WriteByte( crc[1] );
			return ms.ToArray();
		}

		internal static byte[] UnPackCommand( byte[] command )
		{
			if (command == null) command = new byte[0];
			MemoryStream ms = new MemoryStream( );

			for (int i = 0; i < command.Length; i++)
			{
				ms.WriteByte( command[i] );
				if (command[i] == 0x10)
				{
					if ( i + 1 < command.Length && command[i + 1] == 0x10)
					{
						i++;
					}
				}
			}
			return ms.ToArray( );
		}

		internal static bool CheckReceiveDataComplete( byte[] buffer, int length )
		{
			int index = 0;
			if (length < 10) return false;

			for (int i = 0; i < length; i++)
			{
				if (buffer[i] == 0x10 && (i + 1 < length))
				{
					if (buffer[i + 1] == 0x10) i++;
					else if (buffer[i + 1] == 0x03)
					{
						index = i;
						break;
					}
				}
			}
			if (index == length - 4) return true;
			return false;
		}

		private static byte[] GetBytesAddress( int address )
		{
			string addressStart = address.ToString( "D6" );
			if (addressStart.Length > 6) addressStart = addressStart.Substring( 6 );

			return addressStart.ToHexBytes( );
		}

		/// <summary>
		/// 构建读取的报文命令，对于字地址，单次最多读取64字节，支持D,SD,R,T,C的数据读取，对于位地址，最多读取1024位，支持X,Y,M,SM,S,TS(定时器触点),TC（定时器线圈）,CS(计数器触点),CC（计数器线圈）<br />
		/// Construct a read message command. For word addresses, up to 64 bytes can be read at a time, and data reading of D, SD, R, T, and C is supported. For bit addresses, 
		/// up to 1024 bits are read, and X, Y are supported. , M, SM, S, TS (timer contact), TC (timer coil), CS (counter contact), CC (counter coil)
		/// </summary>
		/// <param name="station">站号信息</param>
		/// <param name="address">PLC的数据地址</param>
		/// <param name="length">读取的长度</param>
		/// <param name="isBool">是否进行位读取</param>
		/// <returns>完整的读取的报文信息</returns>
		public static OperateResult<List<byte[]>> BuildReadCommand( byte station, string address, ushort length, bool isBool )
		{
			OperateResult<VigorAddress> analysis = VigorAddress.ParseFrom( address, length, isBool );
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<List<byte[]>>( analysis );

			int[] splits = SoftBasic.SplitIntegerToArray( length, isBool ? 1024 : analysis.Content.DataCode == 0xAD ? 32 : 64 );

			List<byte[]> result = new List<byte[]>( );
			for (int i = 0; i < splits.Length; i++)
			{
				byte[] add = GetBytesAddress( analysis.Content.AddressStart );
				byte[] buffer = new byte[10];
				buffer[0] = station;
				buffer[1] = 0x07;
				buffer[2] = 0x00;
				buffer[3] = isBool ? (byte)0x21 : (byte)0x20;
				buffer[4] = analysis.Content.DataCode;
				buffer[5] = add[2];
				buffer[6] = add[1];
				buffer[7] = add[0];
				buffer[8] = BitConverter.GetBytes( splits[i] )[0];
				buffer[9] = BitConverter.GetBytes( splits[i] )[1];

				result.Add( PackCommand( buffer ) );
				analysis.Content.AddressStart += splits[i];
			}
			return OperateResult.CreateSuccessResult( result );
		}

		/// <summary>
		/// 构建以字单位写入的报文，单次最多写入64个word，地址支持 D,SD,R,T,C，对于C200~C255,是属于32位的计数器<br />
		/// Construct a message written in word units, and write up to 64 words in a single time. The address supports D, SD, R, T, C. For C200~C255, it is a 32-bit counter
		/// </summary>
		/// <param name="station">站号信息</param>
		/// <param name="address">PLC的地址</param>
		/// <param name="value">写入的原始数据</param>
		/// <returns>写入命令的完整报文</returns>
		public static OperateResult<byte[]> BuildWriteWordCommand( byte station, string address, byte[] value )
		{
			OperateResult<VigorAddress> analysis = VigorAddress.ParseFrom( address, 1, false );
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( analysis );

			byte[] add = GetBytesAddress( analysis.Content.AddressStart );

			byte[] buffer = new byte[10 + value.Length];
			buffer[0] = station;
			buffer[1] = BitConverter.GetBytes( 7 + value.Length )[0];
			buffer[2] = BitConverter.GetBytes( 7 + value.Length )[1];
			buffer[3] = 0x28;
			buffer[4] = analysis.Content.DataCode;
			buffer[5] = add[2];
			buffer[6] = add[1];
			buffer[7] = add[0];
			if ( analysis.Content.DataCode == 0xAD)
			{
				buffer[8] = BitConverter.GetBytes( value.Length / 4 )[0];
				buffer[9] = BitConverter.GetBytes( value.Length / 4 )[1];
			}
			else
			{
				buffer[8] = BitConverter.GetBytes( value.Length / 2 )[0];
				buffer[9] = BitConverter.GetBytes( value.Length / 2 )[1];
			}
			value.CopyTo(buffer, 10);

			return OperateResult.CreateSuccessResult( PackCommand( buffer ) );
		}

		/// <summary>
		/// 构建以位单位写入的报文，单次最多写入1024bit，支持X,Y,M,SM,S,TS(定时器触点),TC（定时器线圈）,CS(计数器触点),CC（计数器线圈）
		/// </summary>
		/// <param name="station">站号信息</param>
		/// <param name="address">PLC的地址</param>
		/// <param name="value">等待写入的bool数组</param>
		/// <returns>写入位数据的完整报文信息</returns>
		public static OperateResult<byte[]> BuildWriteBoolCommand( byte station, string address, bool[] value )
		{
			OperateResult<VigorAddress> analysis = VigorAddress.ParseFrom( address, 1, true );
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( analysis );

			byte[] add = GetBytesAddress( analysis.Content.AddressStart );
			byte[] value_byte = value.ToByteArray( );

			byte[] buffer = new byte[10 + value_byte.Length];
			buffer[0] = station;
			buffer[1] = BitConverter.GetBytes( 7 + value_byte.Length )[0];
			buffer[2] = BitConverter.GetBytes( 7 + value_byte.Length )[1];
			buffer[3] = 0x29;
			buffer[4] = analysis.Content.DataCode;
			buffer[5] = add[2];
			buffer[6] = add[1];
			buffer[7] = add[0];
			buffer[8] = BitConverter.GetBytes( value.Length )[0];
			buffer[9] = BitConverter.GetBytes( value.Length )[1];
			value_byte.CopyTo( buffer, 10 );

			return OperateResult.CreateSuccessResult( PackCommand( buffer ) );
		}

		/// <summary>
		/// 检查从PLC返回的报文是否正确，以及提取出正确的结果数据
		/// </summary>
		/// <param name="response">PLC返回的报文</param>
		/// <returns>提取的结果数据内容</returns>
		public static OperateResult<byte[]> CheckResponseContent( byte[] response )
		{
			response = UnPackCommand( response );
			if (response.Length < 6) return new OperateResult<byte[]>( StringResources.Language.ReceiveDataLengthTooShort + " Source: " + response.ToHexString( ' ' ) );
			if (response[5] != 0x00) return new OperateResult<byte[]>( response[5], GetErrorText( response[5] ) + " Source: " + response.ToHexString( ' ' ) );

			int len = BitConverter.ToUInt16( response, 3 );
			if (len + 9 == response.Length)
			{
				if (len == 1) return OperateResult.CreateSuccessResult( new byte[0] );
				return OperateResult.CreateSuccessResult( response.SelectMiddle( 6, len - 1 ) );
			}
			return new OperateResult<byte[]>( response[5], "Length check failed, Source: " + response.ToHexString( ' ' ) );
		}

		internal static string GetErrorText( byte status )
		{
			switch( status)
			{
				case 0x02: return StringResources.Language.Vigor02;
				case 0x04: return StringResources.Language.Vigor04;
				case 0x06: return StringResources.Language.Vigor06;
				case 0x08: return StringResources.Language.Vigor08;
				case 0x31: return StringResources.Language.Vigor31;
				default: return StringResources.Language.UnknownError;
			}
		}

	}
}
