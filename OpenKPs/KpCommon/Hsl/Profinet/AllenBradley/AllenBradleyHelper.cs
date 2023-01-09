using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using HslCommunication.Core;
#if !NET20 && !NET35
using System.Threading.Tasks;
#endif

namespace HslCommunication.Profinet.AllenBradley
{
	/// <summary>
	/// AB PLC的辅助类，用来辅助生成基本的指令信息
	/// </summary>
	public class AllenBradleyHelper
	{
		#region Static Service Code
		/// <summary>
		/// CIP命令中PCCC命令相关的信息
		/// </summary>
		public const byte CIP_Execute_PCCC = 0x4B;
		/// <summary>
		/// CIP命令中的读取数据的服务
		/// </summary>
		public const byte CIP_READ_DATA = 0x4C;
		/// <summary>
		/// CIP命令中的写数据的服务
		/// </summary>
		public const int CIP_WRITE_DATA = 0x4D;
		/// <summary>
		/// CIP命令中的读并写的数据服务
		/// </summary>
		public const int CIP_READ_WRITE_DATA = 0x4E;
		/// <summary>
		/// CIP命令中的读片段的数据服务
		/// </summary>
		public const int CIP_READ_FRAGMENT = 0x52;
		/// <summary>
		/// CIP命令中的写片段的数据服务
		/// </summary>
		public const int CIP_WRITE_FRAGMENT = 0x53;
		/// <summary>
		/// CIP指令中读取数据的列表
		/// </summary>
		public const byte CIP_READ_LIST = 0x55;
		/// <summary>
		/// CIP命令中的对数据读取服务
		/// </summary>
		public const int CIP_MULTIREAD_DATA = 0x1000;

		#endregion

		#region DataType Code

		/// <summary>
		/// 日期的格式
		/// </summary>
		public const ushort CIP_Type_DATE = 0x08;

		/// <summary>
		/// 时间的格式
		/// </summary>
		public const ushort CIP_Type_TIME = 0x09;

		/// <summary>
		/// 日期时间格式，最完整的时间格式
		/// </summary>
		public const ushort CIP_Type_TimeAndDate = 0x0A;

		/// <summary>
		/// 一天中的时间格式
		/// </summary>
		public const ushort CIP_Type_TimeOfDate = 0x0B;

		/// <summary>
		/// bool型数据，一个字节长度
		/// </summary>
		public const ushort CIP_Type_Bool = 0xC1;

		/// <summary>
		/// byte型数据，一个字节长度，SINT
		/// </summary>
		public const ushort CIP_Type_Byte = 0xC2;

		/// <summary>
		/// 整型，两个字节长度，INT
		/// </summary>
		public const ushort CIP_Type_Word = 0xC3;

		/// <summary>
		/// 长整型，四个字节长度，DINT
		/// </summary>
		public const ushort CIP_Type_DWord = 0xC4;

		/// <summary>
		/// 特长整型，8个字节，LINT
		/// </summary>
		public const ushort CIP_Type_LInt = 0xC5;

		/// <summary>
		/// Unsigned 8-bit integer, USINT
		/// </summary>
		public const ushort CIP_Type_USInt = 0xC6;

		/// <summary>
		/// Unsigned 16-bit integer, UINT
		/// </summary>
		public const ushort CIP_Type_UInt = 0xC7;

		/// <summary>
		///  Unsigned 32-bit integer, UDINT
		/// </summary>
		public const ushort CIP_Type_UDint = 0xC8;

		/// <summary>
		///  Unsigned 64-bit integer, ULINT
		/// </summary>
		public const ushort CIP_Type_ULint = 0xC9;

		/// <summary>
		/// 实数数据，四个字节长度
		/// </summary>
		public const ushort CIP_Type_Real = 0xCA;

		/// <summary>
		/// 实数数据，八个字节的长度
		/// </summary>
		public const ushort CIP_Type_Double = 0xCB;

		/// <summary>
		/// 结构体数据，不定长度
		/// </summary>
		public const ushort CIP_Type_Struct = 0xCC;

		/// <summary>
		/// 字符串数据内容
		/// </summary>
		public const ushort CIP_Type_String = 0xD0;

		/// <summary>
		///  Bit string, 8 bits, BYTE,
		/// </summary>
		public const ushort CIP_Type_D1 = 0xD1;

		/// <summary>
		/// Bit string, 16-bits, WORD
		/// </summary>
		public const ushort CIP_Type_D2 = 0xD2;

		/// <summary>
		/// Bit string, 32 bits, DWORD
		/// </summary>
		public const ushort CIP_Type_D3 = 0xD3;

		/// <summary>
		/// Bit string, 64 bits LWORD
		/// </summary>
		public const ushort CIP_Type_D4 = 0xD4;

		/// <summary>
		/// 二进制数据内容
		/// </summary>
		public const ushort CIP_Type_BitArray = 0xD3;

		/// <summary>
		/// 连接方的厂商标识
		/// </summary>
		public const ushort OriginatorVendorID = 0x1009;

		/// <summary>
		/// 连接方的序列号
		/// </summary>
		public const uint OriginatorSerialNumber = 0xC1A5460B;

		#endregion

		private static byte[] BuildRequestPathCommand( string address, bool isConnectedAddress = false )
		{
			using (MemoryStream ms = new MemoryStream( ))
			{
				string[] tagNames = address.Split( new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries );

				for (int i = 0; i < tagNames.Length; i++)
				{
					string strIndex = string.Empty;
					int indexFirst = tagNames[i].IndexOf( '[' );
					int indexSecond = tagNames[i].IndexOf( ']' );
					if (indexFirst > 0 && indexSecond > 0 && indexSecond > indexFirst)
					{
						strIndex = tagNames[i].Substring( indexFirst + 1, indexSecond - indexFirst - 1 );
						tagNames[i] = tagNames[i].Substring( 0, indexFirst );
					}

					ms.WriteByte( 0x91 );                      // 固定，字符串标签
					byte[] nameBytes = Encoding.UTF8.GetBytes( tagNames[i] );
					ms.WriteByte( (byte)nameBytes.Length );    // 节点的长度值
					ms.Write( nameBytes, 0, nameBytes.Length );
					if (nameBytes.Length % 2 == 1) ms.WriteByte( 0x00 );

					if (!string.IsNullOrEmpty( strIndex ))
					{
						string[] indexs = strIndex.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries );
						for (int j = 0; j < indexs.Length; j++)
						{
							int index = Convert.ToInt32( indexs[j] );
							if (index < 256 && !isConnectedAddress)
							{
								ms.WriteByte( 0x28 );
								ms.WriteByte( (byte)index );
							}
							else
							{
								ms.WriteByte( 0x29 );
								ms.WriteByte( 0x00 );
								ms.WriteByte( BitConverter.GetBytes( index )[0] );
								ms.WriteByte( BitConverter.GetBytes( index )[1] );
							}
						}
					}
				}

				return ms.ToArray( );
			}
		}

		/// <summary>
		/// 从生成的报文里面反解出实际的数据地址，不支持结构体嵌套，仅支持数据，一维数组，不支持多维数据
		/// </summary>
		/// <param name="pathCommand">地址路径报文</param>
		/// <returns>实际的地址</returns>
		public static string ParseRequestPathCommand( byte[] pathCommand )
		{
			StringBuilder sb = new StringBuilder( );
			for (int i = 0; i < pathCommand.Length; i++)
			{
				if (pathCommand[i] == 0x91)
				{
					string name = Encoding.UTF8.GetString( pathCommand, i + 2, pathCommand[i + 1] ).TrimEnd( '\0' );
					sb.Append( name );
					int length = 2 + name.Length;
					if (name.Length % 2 == 1) length++;
					if (pathCommand.Length > length + i)
						if (pathCommand[i + length] == 0x28)
						{
							sb.Append( $"[{pathCommand[i + length + 1]}]" );
						}
						else if (pathCommand[i + length] == 0x29)
						{
							sb.Append( $"[{BitConverter.ToUInt16( pathCommand, i + length + 2 )}]" );
						}
					sb.Append( "." );
				}
			}
			if (sb[sb.Length - 1] == '.') sb.Remove( sb.Length - 1, 1 );
			return sb.ToString( );
		}

		/// <summary>
		/// 获取枚举PLC数据信息的指令
		/// </summary>
		/// <param name="startInstance">实例的起始地址</param>
		/// <returns>结果数据</returns>
		public static byte[] BuildEnumeratorCommand( uint startInstance )
		{
			byte[] buffer = new byte[16];
			buffer[ 0] = 0x55; // Get_Instance_Attribute_List Service (Request)
			buffer[ 1] = 0x03; // Request Path is 3 words (6 bytes) long
			buffer[ 2] = 0x20; // 8-bit Class ID
			buffer[ 3] = 0x6B; // Symbol Object
			buffer[ 4] = 0x25; // 16-bit Instance ID
			buffer[ 5] = 0x00;
			buffer[ 6] = BitConverter.GetBytes( startInstance )[0];
			buffer[ 7] = BitConverter.GetBytes( startInstance )[1];
			buffer[ 8] = 0x03; // Number of attributes to retrieve
			buffer[ 9] = 0x00;
			buffer[10] = 0x01; // Attribute 1 – Symbol Name
			buffer[11] = 0x00;
			buffer[12] = 0x02; // Attribute 2 – Symbol Type
			buffer[13] = 0x00;
			buffer[14] = 0x08; // Attribute 3 – test
			buffer[15] = 0x00;
			return buffer;
		}

		/// <summary>
		/// 获取枚举PLC的局部变量的数据信息的指令
		/// </summary>
		/// <param name="startInstance">实例的起始地址</param>
		/// <returns>结果命令数据</returns>
		public static byte[] BuildEnumeratorProgrameMainCommand( uint startInstance )
		{
			byte[] buffer = new byte[38];
			buffer[ 0] = 0x55; // Get_Instance_Attribute_List Service (Request)
			buffer[ 1] = 0x0e; // Request Path is 14 words (28 bytes) long
			buffer[ 2] = 0x91; // Logical Segments: Class 0x6B, Instance起始地址 0 x0000(返回的实例ID就是这里的偏移量)
			buffer[ 3] = 0x13;
			Encoding.ASCII.GetBytes( "Program:MainProgram" ).CopyTo( buffer, 4 );
			buffer[23] = 0x00; 
			buffer[24] = 0x20; // 8-bit Class ID
			buffer[25] = 0x6B; // Symbol Object
			buffer[26] = 0x25; // 16-bit Instance ID
			buffer[27] = 0x00;
			buffer[28] = BitConverter.GetBytes( startInstance )[0];
			buffer[29] = BitConverter.GetBytes( startInstance )[1];
			buffer[30] = 0x03; // Number of attributes to retrieve
			buffer[31] = 0x00;
			buffer[32] = 0x01; // Attribute 1 – Symbol Name
			buffer[33] = 0x00;
			buffer[34] = 0x02; // Attribute 2 – Symbol Type
			buffer[35] = 0x00;
			buffer[36] = 0x08; // Attribute 3 – test
			buffer[37] = 0x00;
			return buffer;
		}

		/// <summary>
		/// 获取获得结构体句柄的命令
		/// </summary>
		/// <param name="symbolType">包含地址的信息</param>
		/// <returns>命令数据</returns>
		public static byte[] GetStructHandleCommand( ushort symbolType )
		{
			byte[] buffer = new byte[18];
			symbolType = (ushort)(symbolType & 0x0fff);

			buffer[ 0] = 0x03;                                    // Get Attributes, List Service (Request)
			buffer[ 1] = 0x03;                                    // Request Path is 3 words (6 bytes) long
			buffer[ 2] = 0x20;                                    // 8-bit class ID
			buffer[ 3] = 0x6c;                                    // Template class (0x6C)
			buffer[ 4] = 0x25;                                    // 16-bit Instance ID
			buffer[ 5] = 0x00;
			buffer[ 6] = BitConverter.GetBytes( symbolType )[0];  // 将数据类型当成实例ID使用
			buffer[ 7] = BitConverter.GetBytes( symbolType )[1];
			buffer[ 8] = 0x04;     // Attribute Count
			buffer[ 9] = 0x00;
			buffer[10] = 0x04;     // Attribute List: Attributes 4
			buffer[11] = 0x00;
			buffer[12] = 0x05;     // Attribute List: Attributes 5
			buffer[13] = 0x00;
			buffer[14] = 0x02;     // Attribute List: Attributes 2
			buffer[15] = 0x00;
			buffer[16] = 0x01;     // Attribute List: Attributes 1
			buffer[17] = 0x00;
			return buffer;
		}

		/// <summary>
		/// 获取结构体内部数据结构的方法
		/// </summary>
		/// <param name="symbolType">地址</param>
		/// <param name="structHandle">句柄</param>
		/// <param name="offset">偏移量地址</param>
		/// <returns>指令</returns>
		public static byte[] GetStructItemNameType( ushort symbolType, AbStructHandle structHandle, int offset )
		{
			byte[] buffer = new byte[14];
			symbolType = (ushort)(symbolType & 0x0fff);
			byte[] read_len_buff = BitConverter.GetBytes( structHandle.TemplateObjectDefinitionSize * 4 - 21 ); // 获取读取长度

			buffer[ 0] = CIP_READ_DATA;                             // Read Service (Request)
			buffer[ 1] = 0x03;                                      // Request Path is 3 words (6 bytes) long
			buffer[ 2] = 0x20;                                      // 8-bit class ID
			buffer[ 3] = 0x6c;                                      // Template class (0x6C)
			buffer[ 4] = 0x25;                                      // 16-bit Instance ID
			buffer[ 5] = 0x00;
			buffer[ 6] = BitConverter.GetBytes( symbolType )[0];    // instance id
			buffer[ 7] = BitConverter.GetBytes( symbolType )[1];
			buffer[ 8] = BitConverter.GetBytes( offset )[0];        //偏移量
			buffer[ 9] = BitConverter.GetBytes( offset )[1];
			buffer[10] = BitConverter.GetBytes( offset )[2];
			buffer[11] = BitConverter.GetBytes( offset )[3];
			buffer[12] = read_len_buff[0];                          //读取字节长度
			buffer[13] = read_len_buff[1];
			return buffer;
		}

		/// <inheritdoc cref="PackRequestHeader(ushort, uint, uint, byte[])"/>
		public static byte[] PackRequestHeader( ushort command, uint session, byte[] commandSpecificData, byte[] senderContext = null )
		{
			if ( commandSpecificData == null ) commandSpecificData = new byte[0];
			byte[] buffer = new byte[commandSpecificData.Length + 24];
			Array.Copy( commandSpecificData, 0, buffer, 24, commandSpecificData.Length );
			BitConverter.GetBytes( command ).CopyTo( buffer, 0 );
			BitConverter.GetBytes( session ).CopyTo( buffer, 4 );
			if (senderContext != null) senderContext.CopyTo( buffer, 12 );
			BitConverter.GetBytes( (ushort)commandSpecificData.Length ).CopyTo( buffer, 2 );
			return buffer;
		}

		/// <summary>
		/// 将CommandSpecificData的命令，打包成可发送的数据指令
		/// </summary>
		/// <param name="command">实际的命令暗号</param>
		/// <param name="error">错误号信息</param>
		/// <param name="session">当前会话的id</param>
		/// <param name="commandSpecificData">CommandSpecificData命令</param>
		/// <returns>最终可发送的数据命令</returns>
		public static byte[] PackRequestHeader( ushort command, uint error, uint session, byte[] commandSpecificData )
		{
			byte[] buffer = PackRequestHeader( command, session, commandSpecificData );
			BitConverter.GetBytes( error ).CopyTo( buffer, 8 );
			return buffer;
		}

		private static byte[] PackExecutePCCC( byte[] pccc )
		{
			MemoryStream ms = new MemoryStream( );
			ms.WriteByte( CIP_Execute_PCCC );
			ms.WriteByte( 0x02 );
			ms.WriteByte( 0x20 );  //
			ms.WriteByte( 0x67 );
			ms.WriteByte( 0x24 );  // Requestor ID
			ms.WriteByte( 0x01 );
			ms.WriteByte( 0x07 );  // CIP PCCC Object
			ms.WriteByte( 0x09 );
			ms.WriteByte( 0x10 );
			ms.WriteByte( 0x0B );  // CIP Serial Number
			ms.WriteByte( 0x46 );
			ms.WriteByte( 0xA5 );
			ms.WriteByte( 0xC1 );
			ms.Write( pccc );

			byte[] buffer = ms.ToArray( );
			BitConverter.GetBytes( AllenBradleyHelper.OriginatorVendorID ).CopyTo( buffer, 7 );
			BitConverter.GetBytes( AllenBradleyHelper.OriginatorSerialNumber ).CopyTo( buffer, 9 );
			return buffer;
		}

		/// <summary>
		/// 打包一个PCCC的读取的命令报文
		/// </summary>
		/// <param name="tns">请求序号信息</param>
		/// <param name="address">请求的文件地址，地址示例：N7:1</param>
		/// <param name="length">请求的字节长度</param>
		/// <returns>PCCC的读取报文信息</returns>
		public static OperateResult<byte[]> PackExecutePCCCRead( int tns, string address, ushort length )
		{
			OperateResult<byte[]> command = AllenBradleyDF1Serial.BuildProtectedTypedLogicalReadWithThreeAddressFields( tns, address, length );
			if (!command.IsSuccess) return command;

			return OperateResult.CreateSuccessResult( PackExecutePCCC( command.Content ) );
		}

		/// <summary>
		/// 打包一个PCCC的写入的命令报文
		/// </summary>
		/// <param name="tns">请求序号信息</param>
		/// <param name="address">请求的文件地址，地址示例：N7:1</param>
		/// <param name="value">写入的原始数据信息</param>
		/// <returns>PCCC的写入报文信息</returns>
		public static OperateResult<byte[]> PackExecutePCCCWrite( int tns, string address, byte[] value )
		{
			OperateResult<byte[]> command = AllenBradleyDF1Serial.BuildProtectedTypedLogicalWriteWithThreeAddressFields( tns, address, value );
			if (!command.IsSuccess) return command;

			return OperateResult.CreateSuccessResult( PackExecutePCCC( command.Content ) );
		}

		internal static OperateResult<byte[]> PackExecutePCCCWrite( int tns, string address, int bitIndex, bool value )
		{
			OperateResult<byte[]> command = AllenBradleyDF1Serial.BuildProtectedTypedLogicalMaskWithThreeAddressFields( tns, address, bitIndex, value );
			if (!command.IsSuccess) return command;

			return OperateResult.CreateSuccessResult( PackExecutePCCC( command.Content ) );
		}

		/// <summary>
		/// 打包生成一个请求读取数据的节点信息，CIP指令信息
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="length">指代数组的长度</param>
		/// <param name="isConnectedAddress">是否是连接模式下的地址，默认为false</param>
		/// <returns>CIP的指令信息</returns>
		public static byte[] PackRequsetRead( string address, int length, bool isConnectedAddress = false )
		{
			byte[] buffer = new byte[1024];
			int offset = 0;
			buffer[offset++] = CIP_READ_DATA;
			offset++;

			byte[] requestPath = BuildRequestPathCommand( address, isConnectedAddress );
			requestPath.CopyTo( buffer, offset );
			offset += requestPath.Length;

			buffer[1] = (byte)((offset - 2) / 2);
			buffer[offset++] = BitConverter.GetBytes( length )[0];
			buffer[offset++] = BitConverter.GetBytes( length )[1];

			byte[] data = new byte[offset];
			Array.Copy( buffer, 0, data, 0, offset );
			return data;
		}

		/// <summary>
		/// 打包生成一个请求读取数据片段的节点信息，CIP指令信息
		/// </summary>
		/// <param name="address">节点的名称 -> Tag Name</param>
		/// <param name="startIndex">起始的索引位置，以字节为单位 -> The initial index position, in bytes</param>
		/// <param name="length">读取的数据长度，一次通讯总计490个字节 -> Length of read data, a total of 490 bytes of communication</param>
		/// <returns>CIP的指令信息</returns>
		public static byte[] PackRequestReadSegment( string address, int startIndex, int length )
		{
			byte[] buffer = new byte[1024];
			int offset = 0;
			buffer[offset++] = CIP_READ_FRAGMENT;
			offset++;


			byte[] requestPath = BuildRequestPathCommand( address );
			requestPath.CopyTo( buffer, offset );
			offset += requestPath.Length;

			buffer[1] = (byte)((offset - 2) / 2);
			buffer[offset++] = BitConverter.GetBytes( length )[0];
			buffer[offset++] = BitConverter.GetBytes( length )[1];
			buffer[offset++] = BitConverter.GetBytes( startIndex )[0];
			buffer[offset++] = BitConverter.GetBytes( startIndex )[1];
			buffer[offset++] = BitConverter.GetBytes( startIndex )[2];
			buffer[offset++] = BitConverter.GetBytes( startIndex )[3];

			byte[] data = new byte[offset];
			Array.Copy( buffer, 0, data, 0, offset );
			return data;
		}

		/// <summary>
		/// 根据指定的数据和类型，生成对应的数据
		/// </summary>
		/// <param name="address">地址信息</param>
		/// <param name="typeCode">数据类型</param>
		/// <param name="value">字节值</param>
		/// <param name="length">如果节点为数组，就是数组长度</param>
		/// <param name="isConnectedAddress">是否为连接模式的地址</param>
		/// <returns>CIP的指令信息</returns>
		public static byte[] PackRequestWrite( string address, ushort typeCode, byte[] value, int length = 1, bool isConnectedAddress = false )
		{
			byte[] buffer = new byte[1024];
			int offset = 0;
			buffer[offset++] = CIP_WRITE_DATA;
			offset++;

			byte[] requestPath = BuildRequestPathCommand( address, isConnectedAddress );
			requestPath.CopyTo( buffer, offset );
			offset += requestPath.Length;

			buffer[1] = (byte)((offset - 2) / 2);

			buffer[offset++] = BitConverter.GetBytes( typeCode )[0];     // 数据类型
			buffer[offset++] = BitConverter.GetBytes( typeCode )[1];

			buffer[offset++] = BitConverter.GetBytes( length )[0];       // 固定
			buffer[offset++] = BitConverter.GetBytes( length )[1];

			value.CopyTo( buffer, offset );                              // 数值
			offset += value.Length;

			byte[] data = new byte[offset];
			Array.Copy( buffer, 0, data, 0, offset );
			return data;
		}

		/// <summary>
		/// 分析地址数据信息里的位索引的信息，例如a[10]  返回 a 和 10 索引，如果没有指定索引，就默认为0
		/// </summary>
		/// <param name="address">数据地址</param>
		/// <param name="arrayIndex">位索引</param>
		/// <returns>地址信息</returns>
		public static string AnalysisArrayIndex( string address, out int arrayIndex )
		{
			arrayIndex = 0;
			if (!address.EndsWith( "]" )) return address;

			int index = address.LastIndexOf( '[' );
			if (index < 0) return address;

			address = address.Remove( address.Length - 1 );
			arrayIndex = int.Parse( address.Substring( index + 1 ) );
			address = address.Substring( 0, index );
			return address;
		}

		/// <summary>
		/// 写入Bool数据的基本指令信息
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <returns>报文信息</returns>
		public static byte[] PackRequestWrite( string address, bool value )
		{
			address = AnalysisArrayIndex( address, out int bitIndex );

			address = address + "[" + (bitIndex / 32) + "]";
			int valueOr = 0;
			int valueAnd = -1;
			if (value) valueOr = 1 << bitIndex;
			else valueAnd = ~(1 << bitIndex);

			byte[] buffer = new byte[1024];
			int offset = 0;
			buffer[offset++] = CIP_READ_WRITE_DATA;
			offset++;

			byte[] requestPath = BuildRequestPathCommand( address );
			requestPath.CopyTo( buffer, offset );
			offset += requestPath.Length;

			buffer[1] = (byte)((offset - 2) / 2);

			buffer[offset++] = 0x04;                                     // 掩盖长度
			buffer[offset++] = 0x00;
			BitConverter.GetBytes( valueOr ).CopyTo( buffer, offset );   // 或操作
			offset += 4;
			BitConverter.GetBytes( valueAnd ).CopyTo( buffer, offset );  // 或操作
			offset += 4;

			byte[] data = new byte[offset];
			Array.Copy( buffer, 0, data, 0, offset );
			return data;
		}

		/// <summary>
		/// 将所有的cip指定进行打包操作。
		/// </summary>
		/// <param name="portSlot">PLC所在的面板槽号</param>
		/// <param name="cips">所有的cip打包指令信息</param>
		/// <returns>包含服务的信息</returns>
		public static byte[] PackCommandService( byte[] portSlot, params byte[][] cips )
		{
			MemoryStream ms = new MemoryStream( );
			// type id   0xB2:UnConnected Data Item  0xB1:Connected Data Item  0xA1:Connect Address Item
			ms.WriteByte( 0xB2 );
			ms.WriteByte( 0x00 );
			ms.WriteByte( 0x00 );     // 后续数据的长度
			ms.WriteByte( 0x00 );

			ms.WriteByte( 0x52 );     // 服务
			ms.WriteByte( 0x02 );     // 请求路径大小
			ms.WriteByte( 0x20 );     // 请求路径
			ms.WriteByte( 0x06 );
			ms.WriteByte( 0x24 );
			ms.WriteByte( 0x01 );
			ms.WriteByte( 0x0A );     // 超时时间
			ms.WriteByte( 0xF0 );
			ms.WriteByte( 0x00 );     // CIP指令长度
			ms.WriteByte( 0x00 );

			int count = 0;
			if (cips.Length == 1)
			{
				ms.Write( cips[0], 0, cips[0].Length );
				count += cips[0].Length;
			}
			else
			{
				ms.WriteByte( 0x0A );   // 固定
				ms.WriteByte( 0x02 );
				ms.WriteByte( 0x20 );
				ms.WriteByte( 0x02 );
				ms.WriteByte( 0x24 );
				ms.WriteByte( 0x01 );
				count += 8;

				ms.Write( BitConverter.GetBytes( (ushort)cips.Length ), 0, 2 );  // 写入项数
				ushort offect = (ushort)(0x02 + 2 * cips.Length);
				count += 2 * cips.Length;

				for (int i = 0; i < cips.Length; i++)
				{
					ms.Write( BitConverter.GetBytes( offect ), 0, 2 );
					offect = (ushort)(offect + cips[i].Length);
				}

				for (int i = 0; i < cips.Length; i++)
				{
					ms.Write( cips[i], 0, cips[i].Length );
					count += cips[i].Length;
				}
			}

			if (portSlot != null)
			{
				ms.WriteByte( (byte)((portSlot.Length + 1) / 2) );     // Path Size
				ms.WriteByte( 0x00 );
				ms.Write( portSlot, 0, portSlot.Length );
				if (portSlot.Length % 2 == 1) ms.WriteByte( 0x00 );
			}

			byte[] data = ms.ToArray( );
			BitConverter.GetBytes( (short)count ).CopyTo( data, 12 );
			BitConverter.GetBytes( (short)(data.Length - 4) ).CopyTo( data, 2 );
			return data;
		}

		/// <summary>
		/// 将所有的cip指定进行打包操作。
		/// </summary>
		/// <param name="portSlot">PLC所在的面板槽号</param>
		/// <param name="cips">所有的cip打包指令信息</param>
		/// <returns>包含服务的信息</returns>
		public static byte[] PackCleanCommandService( byte[] portSlot, params byte[][] cips )
		{
			MemoryStream ms = new MemoryStream( );
			// type id   0xB2:UnConnected Data Item  0xB1:Connected Data Item  0xA1:Connect Address Item
			ms.WriteByte( 0xB2 );
			ms.WriteByte( 0x00 );
			ms.WriteByte( 0x00 );     // 后续数据的长度
			ms.WriteByte( 0x00 );

			if (cips.Length == 1)
			{
				ms.Write( cips[0], 0, cips[0].Length );
			}
			else
			{
				ms.WriteByte( 0x0A );   // 固定
				ms.WriteByte( 0x02 );
				ms.WriteByte( 0x20 );
				ms.WriteByte( 0x02 );
				ms.WriteByte( 0x24 );
				ms.WriteByte( 0x01 );

				ms.Write( BitConverter.GetBytes( (ushort)cips.Length ), 0, 2 );  // 写入项数
				ushort offect = (ushort)(0x02 + 2 * cips.Length);

				for (int i = 0; i < cips.Length; i++)
				{
					ms.Write( BitConverter.GetBytes( offect ), 0, 2 );
					offect = (ushort)(offect + cips[i].Length);
				}

				for (int i = 0; i < cips.Length; i++)
				{
					ms.Write( cips[i], 0, cips[i].Length );
				}
			}

			ms.WriteByte( (byte)((portSlot.Length + 1) / 2) );     // Path Size
			ms.WriteByte( 0x00 );
			ms.Write( portSlot, 0, portSlot.Length );
			if (portSlot.Length % 2 == 1) ms.WriteByte( 0x00 );

			byte[] data = ms.ToArray( );
			BitConverter.GetBytes( (short)(data.Length - 4) ).CopyTo( data, 2 );
			return data;
		}

		/// <summary>
		/// 打包一个读取所有特性数据的报文信息，需要传入slot
		/// </summary>
		/// <param name="portSlot">站号信息</param>
		/// <param name="sessionHandle">会话的ID信息</param>
		/// <returns>最终发送的报文数据</returns>
		public static byte[] PackCommandGetAttributesAll( byte[] portSlot, uint sessionHandle )
		{
			byte[] commandSpecificData = AllenBradleyHelper.PackCommandSpecificData( new byte[4],
				AllenBradleyHelper.PackCommandService( portSlot, new byte[] { 0x01, 0x02, 0x20, 0x01, 0x24, 0x01 } ) );
			return AllenBradleyHelper.PackRequestHeader( 0x6F, sessionHandle, commandSpecificData );
		}

		/// <summary>
		/// 根据数据创建反馈的数据信息
		/// </summary>
		/// <param name="data">反馈的数据信息</param>
		/// <param name="isRead">是否是读取</param>
		/// <returns>数据</returns>
		public static byte[] PackCommandResponse( byte[] data, bool isRead )
		{
			if (data == null)
			{
				return new byte[] { 0x00, 0x00, 0x04, 0x00, 0x00, 0x00 };
			}
			else
			{
				return BasicFramework.SoftBasic.SpliceArray( new byte[] { (byte)(isRead ? 0xCC : 0xCD), 0x00, 0x00, 0x00, 0x00, 0x00 }, data );
			}
		}

		/// <summary>
		/// 生成读取直接节点数据信息的内容
		/// </summary>
		/// <param name="service">cip指令内容</param>
		/// <returns>最终的指令值</returns>
		public static byte[] PackCommandSpecificData( params byte[][] service )
		{
			MemoryStream ms = new MemoryStream( );
			ms.WriteByte( 0x00 );
			ms.WriteByte( 0x00 );
			ms.WriteByte( 0x00 );
			ms.WriteByte( 0x00 );
			ms.WriteByte( 0x0A );     // 超时
			ms.WriteByte( 0x00 );
			ms.WriteByte( BitConverter.GetBytes( service.Length )[0] );    // 项数
			ms.WriteByte( BitConverter.GetBytes( service.Length )[1] );
			for (int i = 0; i < service.Length; i++)
			{
				ms.Write( service[i], 0, service[i].Length );
			}
			return ms.ToArray( );
		}


		/// <summary>
		/// 将所有的cip指定进行打包操作。
		/// </summary>
		/// <param name="command">指令信息</param>
		/// <param name="code">服务器的代号信息</param>
		/// <param name="isConnected">是否基于连接的服务</param>
		/// <returns>包含服务的信息</returns>
		public static byte[] PackCommandSingleService( byte[] command, ushort code = 0xB2, bool isConnected = false )
		{
			if (command == null) command = new byte[0];

			byte[] buffer = isConnected ? new byte[6 + command.Length] : new byte[4 + command.Length];
			buffer[0] = BitConverter.GetBytes( code )[0];
			buffer[1] = BitConverter.GetBytes( code )[1];
			buffer[2] = BitConverter.GetBytes( buffer.Length - 4 )[0];
			buffer[3] = BitConverter.GetBytes( buffer.Length - 4 )[1];

			command.CopyTo( buffer, isConnected ? 6 : 4 );
			return buffer;
		}

		/// <summary>
		/// 向PLC注册会话ID的报文<br />
		/// Register a message with the PLC for the session ID
		/// </summary>
		/// <param name="senderContext">发送的上下文信息</param>
		/// <returns>报文信息 -> Message information </returns>
		public static byte[] RegisterSessionHandle( byte[] senderContext = null )
		{
			byte[] commandSpecificData = new byte[] { 0x01, 0x00, 0x00, 0x00, };
			return AllenBradleyHelper.PackRequestHeader( 0x65, 0, commandSpecificData, senderContext );
		}

		/// <summary>
		/// 获取卸载一个已注册的会话的报文<br />
		/// Get a message to uninstall a registered session
		/// </summary>
		/// <param name="sessionHandle">当前会话的ID信息</param>
		/// <returns>字节报文信息 -> BYTE message information </returns>
		public static byte[] UnRegisterSessionHandle( uint sessionHandle )
		{
			return AllenBradleyHelper.PackRequestHeader( 0x66, sessionHandle, new byte[0] );
		}

		/// <summary>
		/// 初步检查返回的CIP协议的报文是否正确<br />
		/// Initially check whether the returned CIP protocol message is correct
		/// </summary>
		/// <param name="response">CIP的报文信息</param>
		/// <returns>是否正确的结果信息</returns>
		public static OperateResult CheckResponse( byte[] response )
		{
			try
			{
				int status = BitConverter.ToInt32( response, 8 );
				if (status == 0) return OperateResult.CreateSuccessResult( );

				string msg = string.Empty;
				switch (status)
				{
					case 0x01: msg = StringResources.Language.AllenBradleySessionStatus01; break;
					case 0x02: msg = StringResources.Language.AllenBradleySessionStatus02; break;
					case 0x03: msg = StringResources.Language.AllenBradleySessionStatus03; break;
					case 0x64: msg = StringResources.Language.AllenBradleySessionStatus64; break;
					case 0x65: msg = StringResources.Language.AllenBradleySessionStatus65; break;
					case 0x69: msg = StringResources.Language.AllenBradleySessionStatus69; break;
					default:   msg = StringResources.Language.UnknownError; break;
				}

				return new OperateResult( status, msg );
			}
			catch (Exception ex)
			{
				return new OperateResult( ex.Message );
			}
		}

		/// <summary>
		/// 从PLC反馈的数据解析，返回解析后的数据内容，数据类型（在多项数据返回中无效），以及是否有更多的数据
		/// </summary>
		/// <param name="response">PLC的反馈数据</param>
		/// <param name="isRead">是否是返回的操作</param>
		/// <returns>带有结果标识的最终数据</returns>
		public static OperateResult<byte[], ushort, bool> ExtractActualData( byte[] response, bool isRead )
		{
			List<byte> data = new List<byte>( );

			int offset = 38;
			bool hasMoreData = false;
			ushort dataType = 0;
			ushort count = BitConverter.ToUInt16( response, 38 );    // 剩余总字节长度，在剩余的字节里，有可能是一项数据，也有可能是多项
			if (BitConverter.ToInt32( response, 40 ) == 0x8A)
			{
				// 00 01 02 03 04 05 06 07 08 09 10 11 12 13 14 15 16 17 18 19 20 21 22 23 24 25 26 27 28 29 30 31 32 33 34 35 36 37 38 39 40 41 42 43 
				// 6F 00 31 00 47 00 39 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 02 00 00 00 00 00 B2 00 21 00 8A 00 00 00 

				// 44 45 46 47 48 49 50 51 52 53 54 55 56 57 58 59 60 61 62 63 64 65 66 67 68 69 70 71 72
				// 03 00 08 00 0F 00 16 00 CC 00 00 00 C1 00 01 CC 00 00 00 C1 00 01 CC 00 00 00 C1 00 01
				// 多项数据
				offset = 44;
				int dataCount = BitConverter.ToUInt16( response, offset );
				for (int i = 0; i < dataCount; i++)
				{
					int offectStart = BitConverter.ToUInt16( response, offset + 2 + i * 2 ) + offset;
					int offectEnd = (i == dataCount - 1) ? response.Length : (BitConverter.ToUInt16( response, offset + 4 + i * 2 ) + offset);
					ushort err = BitConverter.ToUInt16( response, offectStart + 2 );
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
						for (int j = offectStart + 6; j < offectEnd; j++)
						{
							data.Add( response[j] );
						}
					}
				}
			}
			else
			{
				// 单项数据
				byte err = response[offset + 4];
				switch (err)
				{
					case 0x04: return new OperateResult<byte[], ushort, bool>( ) { ErrorCode = err, Message = StringResources.Language.AllenBradley04 };
					case 0x05: return new OperateResult<byte[], ushort, bool>( ) { ErrorCode = err, Message = StringResources.Language.AllenBradley05 };
					case 0x06: hasMoreData = true; break;
					case 0x0A: return new OperateResult<byte[], ushort, bool>( ) { ErrorCode = err, Message = StringResources.Language.AllenBradley0A };
					case 0x13: return new OperateResult<byte[], ushort, bool>( ) { ErrorCode = err, Message = StringResources.Language.AllenBradley13 };
					case 0x1C: return new OperateResult<byte[], ushort, bool>( ) { ErrorCode = err, Message = StringResources.Language.AllenBradley1C };
					case 0x1E: return new OperateResult<byte[], ushort, bool>( ) { ErrorCode = err, Message = StringResources.Language.AllenBradley1E };
					case 0x20: return new OperateResult<byte[], ushort, bool>( ) { ErrorCode = err, Message = StringResources.Language.AllenBradley20 };
					case 0x26: return new OperateResult<byte[], ushort, bool>( ) { ErrorCode = err, Message = StringResources.Language.AllenBradley26 };
					case 0x00: break;
					default: return new OperateResult<byte[], ushort, bool>( ) { ErrorCode = err, Message = StringResources.Language.UnknownError };
				}

				if (response[offset + 2] == 0xCD || response[offset + 2] == 0xD3) return OperateResult.CreateSuccessResult( data.ToArray( ), dataType, hasMoreData );

				if (response[offset + 2] == 0xCC || response[offset + 2] == 0xD2)
				{
					for (int i = offset + 8; i < offset + 2 + count; i++)
					{
						data.Add( response[i] );
					}
					dataType = BitConverter.ToUInt16( response, offset + 6 );
				}
				else if (response[offset + 2] == 0xD5) // 对应的service为0x55
				{
					for (int i = offset + 6; i < offset + 2 + count; i++)
					{
						data.Add( response[i] );
					}
				}
			}

			return OperateResult.CreateSuccessResult( data.ToArray( ), dataType, hasMoreData );
		}

		/// <summary>
		/// 从PLC里读取当前PLC的型号信息<br />
		/// Read the current PLC model information from the PLC
		/// </summary>
		/// <param name="plc">PLC对象</param>
		/// <returns>型号数据信息</returns>
		public static OperateResult<string> ReadPlcType( IReadWriteDevice plc )
		{
			byte[] buffer = @"00 00 00 00 00 00 02 00 00 00 00 00 b2 00 06 00 01 02 20 01 24 01".ToHexBytes( );
			OperateResult<byte[]> read = plc.ReadFromCoreServer( buffer );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<string>( read );

			if (read.Content.Length > 59) return OperateResult.CreateSuccessResult( Encoding.UTF8.GetString( read.Content, 59, read.Content[58] ) );
			return new OperateResult<string>( "Data is too short: " + read.Content.ToHexString( ' ' ) );
		}
#if !NET20 && !NET35
		/// <inheritdoc cref="ReadPlcType(IReadWriteDevice)"/>
		public static async Task<OperateResult<string>> ReadPlcTypeAsync( IReadWriteDevice plc )
		{
			byte[] buffer = @"00 00 00 00 00 00 02 00 00 00 00 00 b2 00 06 00 01 02 20 01 24 01".ToHexBytes( );
			OperateResult<byte[]> read = await plc.ReadFromCoreServerAsync( buffer );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<string>( read );

			if (read.Content.Length > 59) return OperateResult.CreateSuccessResult( Encoding.UTF8.GetString( read.Content, 59, read.Content[58] ) );
			return new OperateResult<string>( "Data is too short: " + read.Content.ToHexString( ' ' ) );
		}
#endif


		#region Date ReadWrite

		/// <summary>
		/// 读取指定地址的日期数据，最小日期为 1970年1月1日，当PLC的变量类型为 "Date" 和 "TimeAndDate" 时，都可以用本方法读取。<br />
		/// Read the date data of the specified address. The minimum date is January 1, 1970. When the PLC variable type is "Date" and "TimeAndDate", this method can be used to read.
		/// </summary>
		/// <param name="plc">当前的通信对象信息</param>
		/// <param name="address">PLC里变量的地址</param>
		/// <returns>日期结果对象</returns>
		public static OperateResult<DateTime> ReadDate( IReadWriteCip plc, string address )
		{
			OperateResult<long> read = plc.ReadInt64( address );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<DateTime>( read );

			long tick = read.Content / 100;
			DateTime dateTime = new DateTime( 1970, 1, 1 );
			return OperateResult.CreateSuccessResult( dateTime.AddTicks( tick ) );
		}

		/// <summary>
		/// 使用日期格式（Date）将指定的数据写入到指定的地址里，PLC的地址类型变量必须为 "Date"，否则写入失败。<br />
		/// Use the date format (Date) to write the specified data to the specified address. The PLC address type variable must be "Date", otherwise the writing will fail.
		/// </summary>
		/// <param name="plc">当前的通信对象信息</param>
		/// <param name="address">PLC里变量的地址</param>
		/// <param name="date">时间信息</param>
		/// <returns>是否写入成功</returns>
		public static OperateResult WriteDate( IReadWriteCip plc, string address, DateTime date )
		{
			long tick = (date.Date - new DateTime( 1970, 1, 1 )).Ticks * 100;
			return plc.WriteTag( address, AllenBradleyHelper.CIP_Type_DATE, plc.ByteTransform.TransByte( tick ) );
		}

		/// <inheritdoc cref="WriteDate(IReadWriteCip, string, DateTime)"/>
		public static OperateResult WriteTimeAndDate( IReadWriteCip plc, string address, DateTime date )
		{
			long tick = (date - new DateTime( 1970, 1, 1 )).Ticks * 100;
			return plc.WriteTag( address, AllenBradleyHelper.CIP_Type_TimeAndDate, plc.ByteTransform.TransByte( tick ) );
		}

		/// <summary>
		/// 读取指定地址的时间数据，最小时间为 0，如果获取秒，可以访问 <see cref="TimeSpan.TotalSeconds"/>，当PLC的变量类型为 "Time" 和 "TimeOfDate" 时，都可以用本方法读取。<br />
		/// Read the time data of the specified address. The minimum time is 0. If you get seconds, you can access <see cref="TimeSpan.TotalSeconds"/>. 
		/// When the PLC variable type is "Time" and "TimeOfDate", you can use this Method to read.
		/// </summary>
		/// <param name="plc">当前的通信对象信息</param>
		/// <param name="address">PLC里变量的地址</param>
		/// <returns>时间的结果对象</returns>
		public static OperateResult<TimeSpan> ReadTime( IReadWriteCip plc, string address )
		{
			OperateResult<long> read = plc.ReadInt64( address );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<TimeSpan>( read );

			long tick = read.Content / 100;
			return OperateResult.CreateSuccessResult( TimeSpan.FromTicks( tick ) );
		}

		/// <summary>
		/// 使用时间格式（TIME）将时间数据写入到PLC中指定的地址里去，PLC的地址类型变量必须为 "TIME"，否则写入失败。<br />
		/// Use the time format (TIME) to write the time data to the address specified in the PLC. The PLC address type variable must be "TIME", otherwise the writing will fail.
		/// </summary>
		/// <param name="plc">当前的通信对象信息</param>
		/// <param name="address">PLC里变量的地址</param>
		/// <param name="time">时间参数变量</param>
		/// <returns>是否写入成功</returns>
		public static OperateResult WriteTime( IReadWriteCip plc, string address, TimeSpan time )
		{
			return plc.WriteTag( address, AllenBradleyHelper.CIP_Type_TIME, plc.ByteTransform.TransByte( time.Ticks * 100 ) );
		}

		/// <summary>
		/// 使用时间格式（TimeOfDate）将时间数据写入到PLC中指定的地址里去，PLC的地址类型变量必须为 "TimeOfDate"，否则写入失败。<br />
		/// Use the time format (TimeOfDate) to write the time data to the address specified in the PLC. The PLC address type variable must be "TimeOfDate", otherwise the writing will fail.
		/// </summary>
		/// <param name="plc">当前的通信对象信息</param>
		/// <param name="address">PLC里变量的地址</param>
		/// <param name="timeOfDate">时间参数变量</param>
		/// <returns>是否写入成功</returns>
		public static OperateResult WriteTimeOfDate( IReadWriteCip plc, string address, TimeSpan timeOfDate )
		{
			return plc.WriteTag( address, AllenBradleyHelper.CIP_Type_TimeOfDate, plc.ByteTransform.TransByte( timeOfDate.Ticks * 100 ) );
		}
#if !NET20 && !NET35
		/// <inheritdoc cref="ReadDate(IReadWriteCip, string)"/>
		public static async Task<OperateResult<DateTime>> ReadDateAsync( IReadWriteCip plc, string address )
		{
			OperateResult<long> read = await plc.ReadInt64Async( address );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<DateTime>( read );

			long tick = read.Content / 100;
			DateTime dateTime = new DateTime( 1970, 1, 1 );
			return OperateResult.CreateSuccessResult( dateTime.AddTicks( tick ) );
		}
		/// <inheritdoc cref="WriteDate(IReadWriteCip, string, DateTime)"/>
		public static async Task<OperateResult> WriteDateAsync( IReadWriteCip plc, string address, DateTime date )
		{
			long tick = (date.Date - new DateTime( 1970, 1, 1 )).Ticks * 100;
			return await plc.WriteTagAsync( address, AllenBradleyHelper.CIP_Type_DATE, plc.ByteTransform.TransByte( tick ) );
		}
		/// <inheritdoc cref="WriteTimeAndDate(IReadWriteCip, string, DateTime)"/>
		public static async Task<OperateResult> WriteTimeAndDateAsync( IReadWriteCip plc, string address, DateTime date )
		{
			long tick = (date - new DateTime( 1970, 1, 1 )).Ticks * 100;
			return await plc.WriteTagAsync( address, AllenBradleyHelper.CIP_Type_TimeAndDate, plc.ByteTransform.TransByte( tick ) );
		}
		/// <inheritdoc cref="ReadTime(IReadWriteCip, string)"/>
		public static async Task<OperateResult<TimeSpan>> ReadTimeAsync( IReadWriteCip plc, string address )
		{
			OperateResult<long> read = await plc.ReadInt64Async( address );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<TimeSpan>( read );

			long tick = read.Content / 100;
			return OperateResult.CreateSuccessResult( TimeSpan.FromTicks( tick ) );
		}
		/// <inheritdoc cref="WriteTime(IReadWriteCip, string, TimeSpan)"/>
		public static async Task<OperateResult> WriteTimeAsync( IReadWriteCip plc, string address, TimeSpan time )
		{
			return await plc.WriteTagAsync( address, AllenBradleyHelper.CIP_Type_TIME, plc.ByteTransform.TransByte( time.Ticks * 100 ) );
		}
		/// <inheritdoc cref="WriteTimeOfDate(IReadWriteCip, string, TimeSpan)"/>
		public static async Task<OperateResult> WriteTimeOfDateAsync( IReadWriteCip plc, string address, TimeSpan timeOfDate )
		{
			return await plc.WriteTagAsync( address, AllenBradleyHelper.CIP_Type_TimeOfDate, plc.ByteTransform.TransByte( timeOfDate.Ticks * 100 ) );
		}
#endif
		#endregion
	}
}
