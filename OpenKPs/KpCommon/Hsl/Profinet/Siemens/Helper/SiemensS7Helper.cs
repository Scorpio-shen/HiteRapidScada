using HslCommunication.BasicFramework;
using HslCommunication.Core;
using HslCommunication.Core.Address;
using HslCommunication.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if !NET35 && !NET20
using System.Threading.Tasks;
#endif

namespace HslCommunication.Profinet.Siemens.Helper
{
	internal class SiemensS7Helper
	{
		/// <summary>
		/// 读取BOOL时，根据S7协议的返回报文，正确提取出实际的数据内容
		/// </summary>
		/// <param name="content">PLC返回的原始字节信息</param>
		/// <returns>解析之后的结果对象</returns>
		internal static OperateResult<byte[]> AnalysisReadBit( byte[] content )
		{
			int receiveCount = 1;
			if (content.Length >= 21 && content[20] == 1)
			{
				byte[] buffer = new byte[receiveCount];
				if (22 < content.Length)
				{
					if (content[21] == 0xFF &&
						content[22] == 0x03)
					{
						buffer[0] = content[25];
					}
					else if (content[21] == 0x05 &&
							 content[22] == 0x00)
					{
						return new OperateResult<byte[]>( content[21], StringResources.Language.SiemensReadLengthOverPlcAssign );
					}
					else if (content[21] == 0x06 &&
							 content[22] == 0x00)
					{
						return new OperateResult<byte[]>( content[21], StringResources.Language.SiemensError0006 );
					}
					else if (content[21] == 0x0A &&
							 content[22] == 0x00)
					{
						return new OperateResult<byte[]>( content[21], StringResources.Language.SiemensError000A );
					}
					else
					{
						return new OperateResult<byte[]>( content[21], StringResources.Language.UnknownError + " Source: " + content.ToHexString( ' ' ) );
					}
				}

				return OperateResult.CreateSuccessResult( buffer );
			}
			else
			{
				return new OperateResult<byte[]>( StringResources.Language.SiemensDataLengthCheckFailed );
			}
		}

		/// <summary>
		/// 读取字数据时，根据S7协议返回的报文，解析出实际的原始字节数组信息
		/// </summary>
		/// <param name="content">PLC返回的原始字节数组</param>
		/// <returns>实际的结果数据对象</returns>
		internal static OperateResult<byte[]> AnalysisReadByte( byte[] content )
		{
			List<byte> list = new List<byte>( );
			if (content.Length >= 21)
			{
				for (int i = 21; i < content.Length - 1; i++)
				{
					if (content[i] == 0xFF && content[i + 1] == 0x04)
					{
						int count = (content[i + 2] * 256 + content[i + 3]) / 8;
						list.AddRange( content.SelectMiddle( i + 4, count ) );
						i += count + 3;
					}
					else if (content[i] == 0xFF && content[i + 1] == 0x09)
					{
						int count = content[i + 2] * 256 + content[i + 3];
						if (count % 3 == 0)
						{
							for (int j = 0; j < count / 3; j++)
							{
								list.AddRange( content.SelectMiddle( i + 5 + 3 * j, 2 ) );
							}
						}
						else
						{
							for (int j = 0; j < count / 5; j++)
							{
								list.AddRange( content.SelectMiddle( i + 7 + 5 * j, 2 ) );
							}
						}
						i += count + 4;
					}
					else if (content[i] == 0x05 && content[i + 1] == 0x00) return new OperateResult<byte[]>( content[i], StringResources.Language.SiemensReadLengthOverPlcAssign );
					else if (content[i] == 0x06 && content[i + 1] == 0x00) return new OperateResult<byte[]>( content[i], StringResources.Language.SiemensError0006 );
					else if (content[i] == 0x0A && content[i + 1] == 0x00) return new OperateResult<byte[]>( content[i], StringResources.Language.SiemensError000A );
				}
				return OperateResult.CreateSuccessResult( list.ToArray( ) );
			}
			else
			{
				return new OperateResult<byte[]>( StringResources.Language.SiemensDataLengthCheckFailed + " Msg: " + SoftBasic.ByteToHexString( content, ' ' ) );
			}
		}


		/// <summary>
		/// 读取西门子的地址的字符串信息，这个信息是和西门子绑定在一起，长度随西门子的信息动态变化的<br />
		/// Read the Siemens address string information. This information is bound to Siemens and its length changes dynamically with the Siemens information
		/// </summary>
		/// <remarks>
		/// 如果指定编码，一般<see cref="Encoding.ASCII"/>即可，中文需要 Encoding.GetEncoding("gb2312")
		/// </remarks>
		/// <param name="plc">PLC的通信对象</param>
		/// <param name="currentPlc">PLC的系列信息</param>
		/// <param name="address">数据地址，具体的格式需要参照类的说明文档</param>
		/// <param name="encoding">自定的编码信息，一般<see cref="Encoding.ASCII"/>即可，中文需要 Encoding.GetEncoding("gb2312")</param>
		/// <returns>带有是否成功的字符串结果类对象</returns>
		public static OperateResult<string> ReadString( IReadWriteNet plc, SiemensPLCS currentPlc, string address, Encoding encoding )
		{
			if (currentPlc != SiemensPLCS.S200Smart)
			{
				var read = plc.Read( address, 2 );
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<string>( read );

				if (read.Content[0] == 0 || read.Content[0] == 255) return new OperateResult<string>( "Value in plc is not string type" );    // max string length can't be zero

				var readString = plc.Read( address, (ushort)(2 + read.Content[1]) );
				if (!readString.IsSuccess) return OperateResult.CreateFailedResult<string>( readString );

				return OperateResult.CreateSuccessResult( encoding.GetString( readString.Content, 2, readString.Content.Length - 2 ) );
			}
			else
			{
				var read = plc.Read( address, 1 );
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<string>( read );

				var readString = plc.Read( address, (ushort)(1 + read.Content[0]) );
				if (!readString.IsSuccess) return OperateResult.CreateFailedResult<string>( readString );

				return OperateResult.CreateSuccessResult( encoding.GetString( readString.Content, 1, readString.Content.Length - 1 ) );
			}
		}

		/// <summary>
		/// 将指定的字符串写入到西门子PLC里面去，将自动添加字符串长度信息，方便PLC识别字符串的内容。<br />
		/// Write the specified string into Siemens PLC, and the string length information will be automatically added, which is convenient for PLC to identify the content of the string.
		/// </summary>
		/// <param name="plc">PLC的通信对象</param>
		/// <param name="currentPlc">PLC的系列信息</param>
		/// <param name="address">数据地址，具体的格式需要参照类的说明文档</param>
		/// <param name="value">写入的字符串值</param>
		/// <param name="encoding">编码信息</param>
		/// <returns>是否写入成功</returns>
		public static OperateResult Write( IReadWriteNet plc, SiemensPLCS currentPlc, string address, string value, Encoding encoding )
		{
			if (value == null) value = string.Empty;

			byte[] buffer = encoding.GetBytes( value );
			if (encoding == Encoding.Unicode) buffer = SoftBasic.BytesReverseByWord( buffer );

			if (currentPlc != SiemensPLCS.S200Smart)
			{
				// need read one time
				OperateResult<byte[]> readLength = plc.Read( address, 2 );
				if (!readLength.IsSuccess) return readLength;

				if (readLength.Content[0] == 255) return new OperateResult<string>( "Value in plc is not string type" );
				if (readLength.Content[0] == 0) readLength.Content[0] = 254; // allow to create new string
				if (value.Length > readLength.Content[0]) return new OperateResult<string>( "String length is too long than plc defined" );

				return plc.Write( address, SoftBasic.SpliceArray( new byte[] { readLength.Content[0], (byte)buffer.Length }, buffer ) );
			}
			else
			{
				return plc.Write( address, SoftBasic.SpliceArray( new byte[] { (byte)buffer.Length }, buffer ) );
			}
		}

		/// <summary>
		/// 读取西门子的地址的字符串信息，这个信息是和西门子绑定在一起，长度随西门子的信息动态变化的<br />
		/// Read the Siemens address string information. This information is bound to Siemens and its length changes dynamically with the Siemens information
		/// </summary>
		/// <param name="plc">PLC的通信对象</param>
		/// <param name="currentPlc">PLC的系列信息</param>
		/// <param name="address">数据地址，具体的格式需要参照类的说明文档</param>
		/// <returns>带有是否成功的字符串结果类对象</returns>
		public static OperateResult<string> ReadWString( IReadWriteNet plc, SiemensPLCS currentPlc, string address )
		{
			if (currentPlc != SiemensPLCS.S200Smart)
			{
				var read = plc.Read( address, 4 ); // 2
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<string>( read );

				var readString = plc.Read( address, (ushort)(4 + (read.Content[2] * 256 + read.Content[3]) * 2) );
				if (!readString.IsSuccess) return OperateResult.CreateFailedResult<string>( readString );

				return OperateResult.CreateSuccessResult( Encoding.Unicode.GetString( SoftBasic.BytesReverseByWord( readString.Content.RemoveBegin( 4 ) ) ) );
			}
			else
			{
				var read = plc.Read( address, 1 );
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<string>( read );

				var readString = plc.Read( address, (ushort)(1 + read.Content[0] * 2) );
				if (!readString.IsSuccess) return OperateResult.CreateFailedResult<string>( readString );

				return OperateResult.CreateSuccessResult( Encoding.Unicode.GetString( readString.Content, 1, readString.Content.Length - 1 ) );
			}
		}

		/// <summary>
		/// 使用双字节编码的方式，将字符串以 Unicode 编码写入到PLC的地址里，可以使用中文。<br />
		/// Use the double-byte encoding method to write the character string to the address of the PLC in Unicode encoding. Chinese can be used.
		/// </summary>
		/// <param name="plc">PLC的通信对象</param>
		/// <param name="currentPlc">PLC的系列信息</param>
		/// <param name="address">起始地址，格式为I100，M100，Q100，DB20.100 -> Starting address, formatted as I100,mM100,Q100,DB20.100</param>
		/// <param name="value">字符串的值</param>
		/// <returns>是否写入成功的结果对象</returns>
		public static OperateResult WriteWString( IReadWriteNet plc, SiemensPLCS currentPlc, string address, string value )
		{
			//await WriteAsync( address, value, Encoding.Unicode );
			if (currentPlc != SiemensPLCS.S200Smart)
			{
				if (value == null) value = string.Empty;
				byte[] buffer = Encoding.Unicode.GetBytes( value );
				buffer = SoftBasic.BytesReverseByWord( buffer );

				// need read one time
				OperateResult<byte[]> readLength = plc.Read( address, 4 );
				if (!readLength.IsSuccess) return readLength;

				int defineLength = readLength.Content[0] * 256 + readLength.Content[1];
				if (value.Length > defineLength) return new OperateResult<string>( "String length is too long than plc defined" );

				byte[] write = new byte[buffer.Length + 4];
				write[0] = readLength.Content[0];
				write[1] = readLength.Content[1];
				write[2] = BitConverter.GetBytes( value.Length )[1];
				write[3] = BitConverter.GetBytes( value.Length )[0];
				buffer.CopyTo( write, 4 );
				return plc.Write( address, write );
			}
			else
			{
				return plc.Write( address, value, Encoding.Unicode );
			}
		}

#if !NET20 && !NET35
		/// <inheritdoc cref="ReadString(IReadWriteNet, SiemensPLCS, string, Encoding)"/>
		public static async Task<OperateResult<string>> ReadStringAsync( IReadWriteNet plc, SiemensPLCS currentPlc, string address, Encoding encoding )
		{
			if (currentPlc != SiemensPLCS.S200Smart)
			{
				var read = await plc.ReadAsync( address, 2 );
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<string>( read );

				if (read.Content[0] == 0 || read.Content[0] == 255) return new OperateResult<string>( "Value in plc is not string type" );    // max string length can't be zero

				var readString = await plc.ReadAsync( address, (ushort)(2 + read.Content[1]) );
				if (!readString.IsSuccess) return OperateResult.CreateFailedResult<string>( readString );

				return OperateResult.CreateSuccessResult( encoding.GetString( readString.Content, 2, readString.Content.Length - 2 ) );
			}
			else
			{
				var read = await plc.ReadAsync( address, 1 );
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<string>( read );

				var readString = await plc.ReadAsync( address, (ushort)(1 + read.Content[0]) );
				if (!readString.IsSuccess) return OperateResult.CreateFailedResult<string>( readString );

				return OperateResult.CreateSuccessResult( encoding.GetString( readString.Content, 1, readString.Content.Length - 1 ) );
			}
		}

		/// <inheritdoc cref="Write(IReadWriteNet, SiemensPLCS, string, string, Encoding)"/>
		public static async Task<OperateResult> WriteAsync( IReadWriteNet plc, SiemensPLCS currentPlc, string address, string value, Encoding encoding )
		{
			if (value == null) value = string.Empty;

			byte[] buffer = encoding.GetBytes( value );
			if (encoding == Encoding.Unicode) buffer = SoftBasic.BytesReverseByWord( buffer );

			if (currentPlc != SiemensPLCS.S200Smart)
			{
				// need read one time
				OperateResult<byte[]> readLength = await plc.ReadAsync( address, 2 );
				if (!readLength.IsSuccess) return readLength;

				if (readLength.Content[0] == 255) return new OperateResult<string>( "Value in plc is not string type" );
				if (readLength.Content[0] == 0) readLength.Content[0] = 254; // allow to create new string
				if (value.Length > readLength.Content[0]) return new OperateResult<string>( "String length is too long than plc defined" );

				return await plc.WriteAsync( address, SoftBasic.SpliceArray( new byte[] { readLength.Content[0], (byte)buffer.Length }, buffer ) );
			}
			else
			{
				return await plc.WriteAsync( address, SoftBasic.SpliceArray( new byte[] { (byte)buffer.Length }, buffer ) );
			}
		}

		/// <inheritdoc cref="ReadWString(IReadWriteNet, SiemensPLCS, string)"/>
		public static async Task<OperateResult<string>> ReadWStringAsync( IReadWriteNet plc, SiemensPLCS currentPlc, string address )
		{
			if (currentPlc != SiemensPLCS.S200Smart)
			{
				var read = await plc.ReadAsync( address, 4 );                                  // 2
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<string>( read );

				var readString = await plc.ReadAsync( address, (ushort)(4 + (read.Content[2] * 256 + read.Content[3]) * 2) );
				if (!readString.IsSuccess) return OperateResult.CreateFailedResult<string>( readString );

				return OperateResult.CreateSuccessResult( Encoding.Unicode.GetString( SoftBasic.BytesReverseByWord( readString.Content.RemoveBegin( 4 ) ) ) );
			}
			else
			{
				var read = await plc.ReadAsync( address, 1 );
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<string>( read );

				var readString = await plc.ReadAsync( address, (ushort)(1 + read.Content[0] * 2) );
				if (!readString.IsSuccess) return OperateResult.CreateFailedResult<string>( readString );

				return OperateResult.CreateSuccessResult( Encoding.Unicode.GetString( readString.Content, 1, readString.Content.Length - 1 ) );
			}
		}

		/// <inheritdoc cref="WriteWString(IReadWriteNet, SiemensPLCS, string, string)"/>
		public static async Task<OperateResult> WriteWStringAsync( IReadWriteNet plc, SiemensPLCS currentPlc, string address, string value )
		{
			//await WriteAsync( address, value, Encoding.Unicode );
			if (currentPlc != SiemensPLCS.S200Smart)
			{
				if (value == null) value = string.Empty;
				byte[] buffer = Encoding.Unicode.GetBytes( value );
				buffer = SoftBasic.BytesReverseByWord( buffer );

				// need read one time
				OperateResult<byte[]> readLength = await plc.ReadAsync( address, 4 );
				if (!readLength.IsSuccess) return readLength;

				int defineLength = readLength.Content[0] * 256 + readLength.Content[1];
				if (value.Length > defineLength) return new OperateResult<string>( "String length is too long than plc defined" );

				byte[] write = new byte[buffer.Length + 4];
				write[0] = readLength.Content[0];
				write[1] = readLength.Content[1];
				write[2] = BitConverter.GetBytes( value.Length )[1];
				write[3] = BitConverter.GetBytes( value.Length )[0];
				buffer.CopyTo( write, 4 );
				return await plc.WriteAsync( address, write );
			}
			else
			{
				return await plc.WriteAsync( address, value, Encoding.Unicode );
			}
		}
#endif

	}
}
