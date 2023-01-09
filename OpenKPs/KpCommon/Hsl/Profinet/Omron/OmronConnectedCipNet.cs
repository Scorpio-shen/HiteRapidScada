using HslCommunication.BasicFramework;
using HslCommunication.Core;
using HslCommunication.Core.IMessage;
using HslCommunication.Core.Net;
using HslCommunication.Profinet.AllenBradley;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using HslCommunication.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
#if !NET35 && !NET20
using System.Threading.Tasks;
#endif

namespace HslCommunication.Profinet.Omron
{
	/// <summary>
	/// 基于连接的对象访问的CIP协议的实现，用于对Omron PLC进行标签的数据读写，对数组，多维数组进行读写操作，支持的数据类型请参照API文档手册。<br />
	/// The implementation of the CIP protocol based on connected object access is used to read and write tag data to Omron PLC, 
	/// and read and write arrays and multidimensional arrays. For the supported data types, please refer to the API documentation manual.
	/// </summary>
	/// <remarks>
	/// 支持普通标签的读写，类型要和标签对应上。如果标签是数组，例如 A 是 INT[0...9] 那么Read("A", 1)，返回的是10个short所有字节数组。
	/// 如果需要返回10个长度的short数组，请调用 ReadInt16("A[0], 10"); 地址必须写 "A[0]"，不能写 "A" , 如需要读取结构体，参考 <see cref="ReadStruct{T}(string)"/>
	/// </remarks>
	/// <example>
	/// 首先说明支持的类型地址，在PLC里支持了大量的类型，有些甚至在C#里是不存在的。现在做个统一的声明
	/// <list type="table">
	///   <listheader>
	///     <term>PLC类型</term>
	///     <term>含义</term>
	///     <term>代号</term>
	///     <term>C# 类型</term>
	///     <term>备注</term>
	///   </listheader>
	///   <item>
	///     <term>bool</term>
	///     <term>位类型数据</term>
	///     <term>0xC1</term>
	///     <term>bool</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>SINT</term>
	///     <term>8位的整型</term>
	///     <term>0xC2</term>
	///     <term>sbyte</term>
	///     <term>有符号8位很少用，HSL直接用byte</term>
	///   </item>
	///   <item>
	///     <term>USINT</term>
	///     <term>无符号8位的整型</term>
	///     <term>0xC6</term>
	///     <term>byte</term>
	///     <term>如需要，使用<see cref="WriteTag(string, ushort, byte[], int)"/>实现</term>
	///   </item>
	///   <item>
	///     <term>BYTE</term>
	///     <term>8位字符数据</term>
	///     <term>0xD1</term>
	///     <term>byte</term>
	///     <term>如需要，使用<see cref="WriteTag(string, ushort, byte[], int)"/>实现</term>
	///   </item>
	///   <item>
	///     <term>INT</term>
	///     <term>16位的整型</term>
	///     <term>0xC3</term>
	///     <term>short</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>UINT</term>
	///     <term>无符号的16位整型</term>
	///     <term>0xC7</term>
	///     <term>ushort</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>DINT</term>
	///     <term>32位的整型</term>
	///     <term>0xC4</term>
	///     <term>int</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>UDINT</term>
	///     <term>无符号的32位整型</term>
	///     <term>0xC8</term>
	///     <term>uint</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>LINT</term>
	///     <term>64位的整型</term>
	///     <term>0xC5</term>
	///     <term>long</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>ULINT</term>
	///     <term>无符号的64位的整型</term>
	///     <term>0xC9</term>
	///     <term>ulong</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>REAL</term>
	///     <term>单精度浮点数</term>
	///     <term>0xCA</term>
	///     <term>float</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>DOUBLE</term>
	///     <term>双精度浮点数</term>
	///     <term>0xCB</term>
	///     <term>double</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>STRING</term>
	///     <term>字符串数据</term>
	///     <term>0xD0</term>
	///     <term>string</term>
	///     <term>前两个字节为字符长度</term>
	///   </item>
	///   <item>
	///     <term>8bit string BYTE</term>
	///     <term>8位的字符串</term>
	///     <term>0xD1</term>
	///     <term></term>
	///     <term>本质是BYTE数组</term>
	///   </item>
	///   <item>
	///     <term>16bit string WORD</term>
	///     <term>16位的字符串</term>
	///     <term>0xD2</term>
	///     <term></term>
	///     <term>本质是WORD数组，可存放中文</term>
	///   </item>
	///   <item>
	///     <term>32bit string DWORD</term>
	///     <term>32位的字符串</term>
	///     <term>0xD2</term>
	///     <term></term>
	///     <term>本质是DWORD数组，可存放中文</term>
	///   </item>
	/// </list>
	/// 在读写操作之前，先看看怎么实例化和连接PLC<br />
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\OmronConnectedCipNetSample.cs" region="Usage" title="实例化及连接示例" />
	/// 现在来说明以下具体的操作细节。我们假设有如下的变量：<br />
	/// CESHI_A       SINT<br />
	/// CESHI_B       BYTE<br />
	/// CESHI_C       INT<br />
	/// CESHI_D       UINT<br />
	/// CESHI_E       SINT[0..9]<br />
	/// CESHI_F       BYTE[0..9]<br />
	/// CESHI_G       INT[0..9]<br />
	/// CESHI_H       UINT[0..9]<br />
	/// CESHI_I       INT[0..511]<br />
	/// CESHI_J       STRING[12]<br />
	/// ToPc_ID1      ARRAY[0..99] OF STRING[20]<br />
	/// CESHI_O       BOOL<br />
	/// CESHI_P       BOOL[0..31]<br />
	/// 对 CESHI_A 来说，读写这么操作
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\OmronConnectedCipNetSample.cs" region="Usage2" title="读写示例" />
	/// 对于 CESHI_B 来说，写入的操作有点特殊
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\OmronConnectedCipNetSample.cs" region="Usage3" title="读写示例" />
	/// 对于 CESHI_C, CESHI_D 来说，就是 ReadInt16(string address) , Write( string address, short value ) 和 ReadUInt16(string address) 和 Write( string address, ushort value ) 差别不大。
	/// 所以我们着重来看看数组的情况，以 CESHI_G 标签为例子:<br />
	/// 情况一，我想一次读取这个标签所有的字节数组（当长度满足的情况下，会一次性返回数据）
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\OmronConnectedCipNetSample.cs" region="Usage4" title="读写示例" />
	/// 情况二，我想读取第3个数，或是第6个数开始，一共5个数
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\OmronConnectedCipNetSample.cs" region="Usage5" title="读写示例" />
	/// 其他的数组情况都是类似的，我们来看看字符串 CESHI_J 变量
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\OmronConnectedCipNetSample.cs" region="Usage6" title="读写示例" />
	/// 对于 bool 变量来说，就是 ReadBool("CESHI_O") 和 Write("CESHI_O", true) 操作，如果是bool数组，就不一样了
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\OmronConnectedCipNetSample.cs" region="Usage7" title="读写示例" />
	/// 最后我们来看看结构体的操作，假设我们有个结构体<br />
	/// MyData.Code     STRING(12)<br />
	/// MyData.Value1   INT<br />
	/// MyData.Value2   INT<br />
	/// MyData.Value3   REAL<br />
	/// MyData.Value4   INT<br />
	/// MyData.Value5   INT<br />
	/// MyData.Value6   INT[0..3]<br />
	/// 因为bool比较复杂，暂时不考虑。要读取上述的结构体，我们需要定义结构一样的数据
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\OmronConnectedCipNetSample.cs" region="Usage8" title="结构体" />
	/// 定义好后，我们再来读取就很简单了。
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\OmronConnectedCipNetSample.cs" region="Usage9" title="读写示例" />
	/// </example>
	public class OmronConnectedCipNet : NetworkConnectedCip, IReadWriteCip
	{
		#region Contructor

		/// <summary>
		/// 实例化一个默认的对象
		/// </summary>
		public OmronConnectedCipNet( )
		{
			WordLength     = 2;
			ByteTransform  = new RegularByteTransform( );
		}

		/// <summary>
		/// 根据指定的IP及端口来实例化这个连接对象
		/// </summary>
		/// <param name="ipAddress">PLC的Ip地址</param>
		/// <param name="port">PLC的端口号信息</param>
		public OmronConnectedCipNet(string ipAddress, int port = 44818 ) : this( )
		{
			IpAddress       = ipAddress;
			Port            = port;
		}

		//private long oTConnectionId = 0x80000001;

		/// <inheritdoc/>
		protected override byte[] GetLargeForwardOpen( ushort connectionID )
		{
			uint tOConnectionId = 0x80fe0001 + connectionID;
			TOConnectionId = tOConnectionId;
			byte[] buffer = @"
00 00 00 00 00 00 02 00 00 00 00 00 b2 00 34 00
5b 02 20 06 24 01 0e 9c 02 00 00 80 01 00 fe 80
02 00 1b 05 30 a7 2b 03 02 00 00 00 80 84 1e 00
cc 07 00 42 80 84 1e 00 cc 07 00 42 a3 03 20 02
24 01 2c 01".ToHexBytes( );
			BitConverter.GetBytes( (uint)(0x80000002 + connectionID)         ).CopyTo( buffer, 24 );   // O->T Network Connection ID
			BitConverter.GetBytes( tOConnectionId                            ).CopyTo( buffer, 28 );   // T->O Network Connection ID
			BitConverter.GetBytes( (ushort)(2 + connectionID)                ).CopyTo( buffer, 32 );   // Connection Serial Number
			BitConverter.GetBytes( AllenBradleyHelper.OriginatorVendorID     ).CopyTo( buffer, 34 );   // Originator Vendor ID
			HslHelper.HslRandom.GetBytes( 4                                  ).CopyTo( buffer, 36 );   // Originator Serial Number
			buffer[40] = ConnectionTimeoutMultiplier;

			return buffer;
		}

		/// <summary>
		/// 当前产品的型号信息<br />
		/// Model information of the current product
		/// </summary>
		public string ProductName { get; private set; }

		/// <summary>
		/// 获取或设置不通信时超时的时间，默认02，为 32 秒，设置06 时为8分多，计算方法为 (2的x次方乘以8) 的秒数<br />
		/// Get or set the timeout time when there is no communication. The default is 02, which is 32 seconds, and when 06 is set, it is more than 8 minutes. The calculation method is (2 times the power of x times 8) seconds.
		/// </summary>
		public byte ConnectionTimeoutMultiplier { get; set; } = 0x02;

		/// <inheritdoc/>
		protected override OperateResult InitializationOnConnect( Socket socket )
		{
			OperateResult ini = base.InitializationOnConnect( socket );
			if (!ini.IsSuccess) return ini;

			OperateResult<byte[]> read = ReadFromCoreServer( socket, AllenBradleyHelper.PackRequestHeader( 0x6f, SessionHandle, GetAttributeAll( ) ), hasResponseData: true, usePackAndUnpack: false );
			if (!read.IsSuccess) return read;

			if (read.Content.Length > 59) ProductName = Encoding.UTF8.GetString( read.Content, 59, read.Content[58] );
			return OperateResult.CreateSuccessResult( );
		}

#if !NET35 && !NET20
		/// <inheritdoc/>
		protected async override Task<OperateResult> InitializationOnConnectAsync( Socket socket )
		{
			OperateResult ini = await base.InitializationOnConnectAsync( socket );
			if (!ini.IsSuccess) return ini;


			OperateResult<byte[]> read = await ReadFromCoreServerAsync( socket, AllenBradleyHelper.PackRequestHeader( 0x6f, SessionHandle, GetAttributeAll( ) ), hasResponseData: true, usePackAndUnpack: false );
			if (!read.IsSuccess) return read;

			if (read.Content.Length > 59) ProductName = Encoding.UTF8.GetString( read.Content, 59, read.Content[58] );

			return OperateResult.CreateSuccessResult( );
		}
#endif
		#endregion

		#region Private Method


		private byte[] GetAttributeAll( )
		{
			byte[] buffer = @"00 00 00 00 00 00 02 00 00 00 00 00 b2 00 06 00 01 02 20 01 24 01".ToHexBytes( );
			return buffer;
		}

		private OperateResult<byte[]> BuildReadCommand( string[] address, ushort[] length )
		{
			try
			{
				List<byte[]> cips = new List<byte[]>( );
				for (int i = 0; i < address.Length; i++)
				{
					cips.Add( AllenBradleyHelper.PackRequsetRead( address[i], length[i], true ) );
				}
				return OperateResult.CreateSuccessResult( PackCommandService( cips.ToArray( ) ) );
			}
			catch (Exception ex)
			{
				return new OperateResult<byte[]>( "Address Wrong:" + ex.Message );
			}
		}

		private OperateResult<byte[]> BuildWriteCommand( string address, ushort typeCode, byte[] data, int length = 1 )
		{
			try
			{
				return OperateResult.CreateSuccessResult( PackCommandService( AllenBradleyHelper.PackRequestWrite( address, typeCode, data, length, true ) ) );
			}
			catch (Exception ex)
			{
				return new OperateResult<byte[]>( "Address Wrong:" + ex.Message );
			}
		}

		private OperateResult<byte[], ushort, bool> ReadWithType( string[] address, ushort[] length )
		{
			// 指令生成 -> Instruction Generation
			OperateResult<byte[]> command = BuildReadCommand( address, length );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<byte[], ushort, bool>( command ); ;

			// 核心交互 -> Core Interactions
			OperateResult<byte[]> read = ReadFromCoreServer( command.Content );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[], ushort, bool>( read ); ;

			// 检查反馈 -> Check Feedback
			OperateResult check = AllenBradleyHelper.CheckResponse( read.Content );
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[], ushort, bool>( check );

			// 提取数据 -> Extracting data
			return ExtractActualData( read.Content, true );
		}

		#endregion

		/// <inheritdoc cref="AllenBradleyNet.ReadCipFromServer(byte[][])"/>
		public OperateResult<byte[]> ReadCipFromServer( params byte[][] cips )
		{
			byte[] command = PackCommandService( cips.ToArray( ) );

			// 核心交互 -> Core Interactions
			OperateResult<byte[]> read = ReadFromCoreServer( command );
			if (!read.IsSuccess) return read;

			// 检查反馈 -> Check Feedback
			OperateResult check = AllenBradleyHelper.CheckResponse( read.Content );
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( check );

			return OperateResult.CreateSuccessResult( read.Content );
		}

		/// <summary>
		/// <b>[商业授权]</b> 读取一个结构体的对象，需要事先根据实际的数据点位定义好结构体，然后使用本方法进行读取，当结构体定义不对时，本方法将会读取失败<br />
		/// <b>[Authorization]</b> To read a structure object, you need to define the structure in advance according to the actual data points, 
		/// and then use this method to read. When the structure definition is incorrect, this method will fail to read
		/// </summary>
		/// <remarks>
		/// 本方法需要商业授权支持，具体的使用方法，参考API文档的示例代码
		/// </remarks>
		/// <example>
		/// 我们来看看结构体的操作，假设我们有个结构体<br />
		/// MyData.Code     STRING(12)<br />
		/// MyData.Value1   INT<br />
		/// MyData.Value2   INT<br />
		/// MyData.Value3   REAL<br />
		/// MyData.Value4   INT<br />
		/// MyData.Value5   INT<br />
		/// MyData.Value6   INT[0..3]<br />
		/// 因为bool比较复杂，暂时不考虑。要读取上述的结构体，我们需要定义结构一样的数据
		/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\OmronConnectedCipNetSample.cs" region="Usage8" title="结构体" />
		/// 定义好后，我们再来读取就很简单了。
		/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\OmronConnectedCipNetSample.cs" region="Usage9" title="读写示例" />
		/// </example>
		/// <typeparam name="T">结构体的类型</typeparam>
		/// <param name="address">结构体对象的地址</param>
		/// <returns>是否读取成功的对象</returns>
		public OperateResult<T> ReadStruct<T>( string address ) where T : struct
		{
			OperateResult<byte[]> read = Read( address, 1 );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<T>( read );

			return HslHelper.ByteArrayToStruct<T>( read.Content.RemoveBegin( 2 ) );
		}
#if !NET35 && !NET20
		private async Task<OperateResult<byte[], ushort, bool>> ReadWithTypeAsync( string[] address, ushort[] length )
		{
			// 指令生成 -> Instruction Generation
			OperateResult<byte[]> command = BuildReadCommand( address, length );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<byte[], ushort, bool>( command ); ;

			// 核心交互 -> Core Interactions
			OperateResult<byte[]> read = await ReadFromCoreServerAsync( command.Content );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[], ushort, bool>( read ); ;

			// 检查反馈 -> Check Feedback
			OperateResult check = AllenBradleyHelper.CheckResponse( read.Content );
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[], ushort, bool>( check );

			// 提取数据 -> Extracting data
			return ExtractActualData( read.Content, true );
		}

		/// <inheritdoc cref="ReadCipFromServer(byte[][])"/>
		public async Task<OperateResult<byte[]>> ReadCipFromServerAsync( params byte[][] cips )
		{
			byte[] command = PackCommandService( cips.ToArray( ) );

			// 核心交互 -> Core Interactions
			OperateResult<byte[]> read = await ReadFromCoreServerAsync( command );
			if (!read.IsSuccess) return read;

			// 检查反馈 -> Check Feedback
			OperateResult check = AllenBradleyHelper.CheckResponse( read.Content );
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( check );

			return OperateResult.CreateSuccessResult( read.Content );
		}

		/// <inheritdoc cref="ReadStruct{T}(string)"/>
		public async Task<OperateResult<T>> ReadStructAsync<T>( string address ) where T : struct
		{
			OperateResult<byte[]> read = await ReadAsync( address, 1 );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<T>( read );

			return HslHelper.ByteArrayToStruct<T>( read.Content.RemoveBegin( 2 ) );
		}
#endif

		/// <summary>
		/// 获取传递的最大长度的字节信息
		/// </summary>
		/// <returns>字节长度</returns>
		protected virtual int GetMaxTransferBytes( ) => 1988;

		private int GetLengthFromRemain( ushort dataType, int length )
		{
			if (dataType == AllenBradleyHelper.CIP_Type_Bool || dataType == AllenBradleyHelper.CIP_Type_Byte || dataType == AllenBradleyHelper.CIP_Type_USInt || dataType == AllenBradleyHelper.CIP_Type_BitArray)
			{
				return Math.Min( length, GetMaxTransferBytes( ) );
			}
			else if (dataType == AllenBradleyHelper.CIP_Type_UInt || dataType == AllenBradleyHelper.CIP_Type_Word)
			{
				return Math.Min( length, GetMaxTransferBytes( ) / 2 );
			}
			else if (dataType == AllenBradleyHelper.CIP_Type_DWord || dataType == AllenBradleyHelper.CIP_Type_UDint || dataType == AllenBradleyHelper.CIP_Type_Real)
			{
				return Math.Min( length, GetMaxTransferBytes( ) / 4 );
			}
			else
			{
				return Math.Min( length, GetMaxTransferBytes( ) / 8 );
			}
		}

		/// <inheritdoc/>
		[HslMqttApi( "ReadByteArray", "" )]
		public override OperateResult<byte[]> Read( string address, ushort length )
		{
			HslHelper.ExtractParameter( ref address, "type", 0 );

			if (length == 1)
			{
				OperateResult<byte[], ushort, bool> read = ReadWithType( new string[] { address }, new ushort[] { length } );
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( read );

				return OperateResult.CreateSuccessResult( read.Content1 );
			}
			else
			{
				// 如果长度超过248，那么第一次读取248，然后根据类型来确定读取的长度信息，按照字节长度最大 1988 字节
				int count = 0;
				int index = 0;
				List<byte> array = new List<byte>( );
				Match match = Regex.Match( address, @"\[[0-9]+\]$" );
				if (match.Success)
				{
					address = address.Remove( match.Index, match.Length );
					index = int.Parse( match.Value.Substring( 1, match.Value.Length - 2 ) );
				}

				ushort dataType = 0x00;
				while (count < length)
				{
					if (count == 0)
					{
						ushort first = Math.Min( length, (ushort)248 );

						OperateResult<byte[], ushort, bool> read = ReadWithType( new string[] { address + $"[{index}]" }, new ushort[] { first } );
						if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( read );

						dataType = read.Content2;
						count += first;
						index += first;
						array.AddRange( read.Content1 );
					}
					else
					{
						ushort len = (ushort)GetLengthFromRemain( dataType, length - count );

						OperateResult<byte[], ushort, bool> read = ReadWithType( new string[] { address + $"[{index}]" }, new ushort[] { len } );
						if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( read );

						count += len;
						index += len;
						array.AddRange( read.Content1 );
					}
				}
				return OperateResult.CreateSuccessResult( array.ToArray( ) );
			}
		}

		/// <inheritdoc cref="AllenBradleyNet.Read(string[], int[])"/>
		[HslMqttApi( "ReadMultiAddress", "" )]
		public OperateResult<byte[]> Read( string[] address, ushort[] length )
		{
			if (!Authorization.asdniasnfaksndiqwhawfskhfaiw( )) return new OperateResult<byte[]>( StringResources.Language.InsufficientPrivileges );

			OperateResult<byte[], ushort, bool> read = ReadWithType( address, length );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( read );

			return OperateResult.CreateSuccessResult( read.Content1 );
		}

		/// <summary>
		/// 读取bool数据信息，如果读取的是单bool变量，就直接写变量名，如果是 bool 数组，就 <br />
		/// Read a single bool data information, if it is a single bool variable, write the variable name directly, 
		/// if it is a value of a bool array composed of int, it is always accessed with "i=" at the beginning, for example, "i=A[0]"
		/// </summary>
		/// <param name="address">节点的名称 -> Name of the node </param>
		/// <param name="length">读取的数组长度信息</param>
		/// <returns>带有结果对象的结果数据 -> Result data with result info </returns>
		[HslMqttApi( "ReadBoolArray", "" )]
		public override OperateResult<bool[]> ReadBool( string address, ushort length )
		{
			if (length == 1 && !Regex.IsMatch( address, @"\[[0-9]+\]$" ))
			{
				OperateResult<byte[]> read = Read( address, length );
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( read );

				return OperateResult.CreateSuccessResult( SoftBasic.ByteToBoolArray( read.Content ) );
			}
			else
			{
				OperateResult<byte[]> read = Read( address, length );
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( read );

				return OperateResult.CreateSuccessResult( read.Content.Select( m => m != 0x00 ).Take( length ).ToArray( ) );
			}
		}

		/// <summary>
		/// 读取PLC的byte类型的数据<br />
		/// Read the byte type of PLC data
		/// </summary>
		/// <param name="address">节点的名称 -> Name of the node </param>
		/// <returns>带有结果对象的结果数据 -> Result data with result info </returns>
		[HslMqttApi( "ReadByte", "" )]
		public OperateResult<byte> ReadByte( string address ) => ByteTransformHelper.GetResultFromArray( Read( address, 1 ) );

		/// <inheritdoc cref="AllenBradleyNet.ReadTag(string, int)"/>
		public OperateResult<ushort, byte[]> ReadTag( string address, ushort length = 1 )
		{
			OperateResult<byte[], ushort, bool> read = ReadWithType( new string[] { address }, new ushort[] { length } );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<ushort, byte[]>( read );

			return OperateResult.CreateSuccessResult( read.Content2, read.Content1 );
		}

#if !NET35 && !NET20

		/// <inheritdoc cref="ReadBool(string, ushort)"/>
		public async override Task<OperateResult<bool[]>> ReadBoolAsync( string address, ushort length )
		{
			if(length == 1 && !Regex.IsMatch( address, "\\[[0-9]+\\]$" ))
			{
				OperateResult<byte[]> read = await ReadAsync( address, length );
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( read );

				return OperateResult.CreateSuccessResult( SoftBasic.ByteToBoolArray( read.Content ) );
			}
			else
			{
				OperateResult<byte[]> read = await ReadAsync( address, length );
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( read );

				return OperateResult.CreateSuccessResult( read.Content.Select( m => m != 0x00 ).Take( length ).ToArray( ) );
			}
		}

		/// <inheritdoc/>
		public override async Task<OperateResult<byte[]>> ReadAsync( string address, ushort length )
		{
			HslHelper.ExtractParameter( ref address, "type", 0 );

			if (length == 1)
			{
				OperateResult<byte[], ushort, bool> read = await ReadWithTypeAsync( new string[] { address }, new ushort[] { length } );
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( read );

				return OperateResult.CreateSuccessResult( read.Content1 );
			}
			else
			{
				// 如果长度超过248，那么第一次读取248，然后根据类型来确定读取的长度信息，按照字节长度最大 1988 字节
				int count = 0;
				int index = 0;
				List<byte> array = new List<byte>( );
				Match match = Regex.Match( address, @"\[[0-9]+\]$" );
				if (match.Success)
				{
					address = address.Remove( match.Index, match.Length );
					index = int.Parse( match.Value.Substring( 1, match.Value.Length - 2 ) );
				}

				ushort dataType = 0x00;
				while (count < length)
				{
					if (count == 0)
					{
						ushort first = Math.Min( length, (ushort)248 );

						OperateResult<byte[], ushort, bool> read = await ReadWithTypeAsync( new string[] { address + $"[{index}]" }, new ushort[] { first } );
						if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( read );

						dataType = read.Content2;
						count += first;
						index += first;
						array.AddRange( read.Content1 );
					}
					else
					{
						ushort len = (ushort)GetLengthFromRemain( dataType, length - count );

						OperateResult<byte[], ushort, bool> read = await ReadWithTypeAsync( new string[] { address + $"[{index}]" }, new ushort[] { len } );
						if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( read );

						count += len;
						index += len;
						array.AddRange( read.Content1 );
					}
				}
				return OperateResult.CreateSuccessResult( array.ToArray( ) );
			}
		}

		/// <inheritdoc cref="Read(string[], ushort[])"/>
		public async Task<OperateResult<byte[]>> ReadAsync( string[] address, ushort[] length )
		{
			if (!Authorization.asdniasnfaksndiqwhawfskhfaiw( )) return new OperateResult<byte[]>( StringResources.Language.InsufficientPrivileges );

			OperateResult<byte[], ushort, bool> read = await ReadWithTypeAsync( address, length );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( read );

			return OperateResult.CreateSuccessResult( read.Content1 );
		}

		/// <inheritdoc cref="ReadByte(string)"/>
		public async Task<OperateResult<byte>> ReadByteAsync( string address ) => ByteTransformHelper.GetResultFromArray( await ReadAsync( address, 1 ) );

		/// <inheritdoc cref="AllenBradleyNet.ReadTag(string, int)"/>
		public async Task<OperateResult<ushort, byte[]>> ReadTagAsync( string address, ushort length = 1 )
		{
			OperateResult<byte[], ushort, bool> read = await ReadWithTypeAsync( new string[] { address }, new ushort[] { length } );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<ushort, byte[]>( read );

			return OperateResult.CreateSuccessResult( read.Content2, read.Content1 );
		}
#endif

		#region Write Support

		/// <inheritdoc cref="AllenBradleyNet.Write(string, byte[])"/>
		[HslMqttApi( "WriteByteArray", "" )]
		public override OperateResult Write( string address, byte[] value ) => WriteTag( address, AllenBradleyHelper.CIP_Type_D1, value, HslHelper.IsAddressEndWithIndex( address ) ? value.Length : 1 );

		/// <inheritdoc cref="AllenBradleyNet.WriteTag(string, ushort, byte[], int)"/>
		public virtual OperateResult WriteTag( string address, ushort typeCode, byte[] value, int length = 1 )
		{
			typeCode = (ushort)HslHelper.ExtractParameter( ref address, "type", typeCode );

			OperateResult<byte[]> command = BuildWriteCommand( address, typeCode, value, length );
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> read = ReadFromCoreServer( command.Content );
			if (!read.IsSuccess) return read;

			OperateResult check = AllenBradleyHelper.CheckResponse( read.Content );
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( check );

			return AllenBradleyHelper.ExtractActualData( read.Content, false );
		}

		#endregion

		#region Async Write Support
#if !NET35 && !NET20

		/// <inheritdoc cref="Write(string, byte[])"/>
		public override async Task<OperateResult> WriteAsync( string address, byte[] value ) => await WriteTagAsync( address, AllenBradleyHelper.CIP_Type_D1, value, HslHelper.IsAddressEndWithIndex( address ) ? value.Length : 1 );

		/// <inheritdoc cref="WriteTag(string, ushort, byte[], int)"/>
		public virtual async Task<OperateResult> WriteTagAsync( string address, ushort typeCode, byte[] value, int length = 1 )
		{
			typeCode = (ushort)HslHelper.ExtractParameter( ref address, "type", typeCode );

			OperateResult<byte[]> command = BuildWriteCommand( address, typeCode, value, length );
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> read = await ReadFromCoreServerAsync( command.Content );
			if (!read.IsSuccess) return read;

			OperateResult check = AllenBradleyHelper.CheckResponse( read.Content );
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( check );

			return AllenBradleyHelper.ExtractActualData( read.Content, false );
		}
#endif
		#endregion

		#region Device Override

		/// <inheritdoc/>
		[HslMqttApi( "ReadInt16Array", "" )]
		public override OperateResult<short[]> ReadInt16( string address, ushort length ) => ByteTransformHelper.GetResultFromBytes( Read( address, length ), m => ByteTransform.TransInt16( m, 0, length ) );

		/// <inheritdoc/>
		[HslMqttApi( "ReadUInt16Array", "" )]
		public override OperateResult<ushort[]> ReadUInt16( string address, ushort length ) => ByteTransformHelper.GetResultFromBytes( Read( address, length ), m => ByteTransform.TransUInt16( m, 0, length ) );

		/// <inheritdoc/>
		[HslMqttApi( "ReadInt32Array", "" )]
		public override OperateResult<int[]> ReadInt32( string address, ushort length ) => ByteTransformHelper.GetResultFromBytes( Read( address, length ), m => ByteTransform.TransInt32( m, 0, length ) );

		/// <inheritdoc/>
		[HslMqttApi( "ReadUInt32Array", "" )]
		public override OperateResult<uint[]> ReadUInt32( string address, ushort length ) => ByteTransformHelper.GetResultFromBytes( Read( address, length ), m => ByteTransform.TransUInt32( m, 0, length ) );

		/// <inheritdoc/>
		[HslMqttApi( "ReadFloatArray", "" )]
		public override OperateResult<float[]> ReadFloat( string address, ushort length ) => ByteTransformHelper.GetResultFromBytes( Read( address, length ), m => ByteTransform.TransSingle( m, 0, length ) );

		/// <inheritdoc/>
		[HslMqttApi( "ReadInt64Array", "" )]
		public override OperateResult<long[]> ReadInt64( string address, ushort length ) => ByteTransformHelper.GetResultFromBytes( Read( address, length ), m => ByteTransform.TransInt64( m, 0, length ) );

		/// <inheritdoc/>
		[HslMqttApi( "ReadUInt64Array", "" )]
		public override OperateResult<ulong[]> ReadUInt64( string address, ushort length ) => ByteTransformHelper.GetResultFromBytes( Read( address, length ), m => ByteTransform.TransUInt64( m, 0, length ) );

		/// <inheritdoc/>
		[HslMqttApi( "ReadDoubleArray", "" )]
		public override OperateResult<double[]> ReadDouble( string address, ushort length ) => ByteTransformHelper.GetResultFromBytes( Read( address, length ), m => ByteTransform.TransDouble( m, 0, length ) );

		///<inheritdoc/>
		public OperateResult<string> ReadString( string address ) => ReadString( address, 1, Encoding.UTF8 );

		/// <summary>
		/// 读取字符串数据，默认为UTF-8编码<br />
		/// Read string data, default is UTF-8 encoding
		/// </summary>
		/// <param name="address">起始地址</param>
		/// <param name="length">数据长度</param>
		/// <returns>带有成功标识的string数据</returns>
		/// <example>
		/// 以下为三菱的连接对象示例，其他的设备读写情况参照下面的代码：
		/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Core\NetworkDeviceBase.cs" region="ReadString" title="String类型示例" />
		/// </example>
		[HslMqttApi( "ReadString", "" )]
		public override OperateResult<string> ReadString( string address, ushort length ) => ReadString( address, length, Encoding.UTF8 );

		/// <inheritdoc/>
		public override OperateResult<string> ReadString( string address, ushort length, Encoding encoding )
		{
			OperateResult<byte[]> read = Read( address, length );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<string>( read );

			if (read.Content.Length >= 2)
			{
				int strLength = ByteTransform.TransUInt16( read.Content, 0 );
				return OperateResult.CreateSuccessResult( encoding.GetString( read.Content, 2, strLength ) );
			}
			else
			{
				return OperateResult.CreateSuccessResult( encoding.GetString( read.Content ) );
			}
		}

		#endregion

		#region Async Device Override
#if !NET35 && !NET20

		/// <inheritdoc/>
		public override async Task<OperateResult<short[]>> ReadInt16Async( string address, ushort length ) => ByteTransformHelper.GetResultFromBytes( await ReadAsync( address, length ), m => ByteTransform.TransInt16( m, 0, length ) );

		/// <inheritdoc/>
		public override async Task<OperateResult<ushort[]>> ReadUInt16Async( string address, ushort length ) => ByteTransformHelper.GetResultFromBytes( await ReadAsync( address, length ), m => ByteTransform.TransUInt16( m, 0, length ) );

		/// <inheritdoc/>
		public override async Task<OperateResult<int[]>> ReadInt32Async( string address, ushort length ) => ByteTransformHelper.GetResultFromBytes( await ReadAsync( address, length ), m => ByteTransform.TransInt32( m, 0, length ) );

		/// <inheritdoc/>
		public override async Task<OperateResult<uint[]>> ReadUInt32Async( string address, ushort length ) => ByteTransformHelper.GetResultFromBytes( await ReadAsync( address, length ), m => ByteTransform.TransUInt32( m, 0, length ) );

		/// <inheritdoc/>
		public override async Task<OperateResult<float[]>> ReadFloatAsync( string address, ushort length ) => ByteTransformHelper.GetResultFromBytes( await ReadAsync( address, length ), m => ByteTransform.TransSingle( m, 0, length ) );

		/// <inheritdoc/>
		public override async Task<OperateResult<long[]>> ReadInt64Async( string address, ushort length ) => ByteTransformHelper.GetResultFromBytes( await ReadAsync( address, length ), m => ByteTransform.TransInt64( m, 0, length ) );

		/// <inheritdoc/>
		public override async Task<OperateResult<ulong[]>> ReadUInt64Async( string address, ushort length ) => ByteTransformHelper.GetResultFromBytes( await ReadAsync( address, length ), m => ByteTransform.TransUInt64( m, 0, length ) );

		/// <inheritdoc/>
		public override async Task<OperateResult<double[]>> ReadDoubleAsync( string address, ushort length ) => ByteTransformHelper.GetResultFromBytes( await ReadAsync( address, length ), m => ByteTransform.TransDouble( m, 0, length ) );

		/// <inheritdoc/>
		public async Task<OperateResult<string>> ReadStringAsync( string address ) => await ReadStringAsync( address, 1, Encoding.UTF8 );

		/// <inheritdoc cref="ReadString(string, ushort)"/>
		public override async Task<OperateResult<string>> ReadStringAsync( string address, ushort length ) => await ReadStringAsync( address, length, Encoding.UTF8 );

		/// <inheritdoc/>
		public override async Task<OperateResult<string>> ReadStringAsync( string address, ushort length, Encoding encoding )
		{
			OperateResult<byte[]> read = await ReadAsync( address, length );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<string>( read );

			if (read.Content.Length >= 2)
			{
				int strLength = ByteTransform.TransUInt16( read.Content, 0 );
				return OperateResult.CreateSuccessResult( encoding.GetString( read.Content, 2, strLength ) );
			}
			else
			{
				return OperateResult.CreateSuccessResult( encoding.GetString( read.Content ) );
			}
		}
#endif
		#endregion

		#region Write Override

		/// <inheritdoc/>
		[HslMqttApi( "WriteInt16Array", "" )]
		public override OperateResult Write( string address, short[] values ) => WriteTag( address, AllenBradleyHelper.CIP_Type_Word, ByteTransform.TransByte( values ), values.Length );

		/// <inheritdoc/>
		[HslMqttApi( "WriteUInt16Array", "" )]
		public override OperateResult Write( string address, ushort[] values ) => WriteTag( address, AllenBradleyHelper.CIP_Type_UInt, ByteTransform.TransByte( values ), values.Length );

		/// <inheritdoc/>
		[HslMqttApi( "WriteInt32Array", "" )]
		public override OperateResult Write( string address, int[] values ) => WriteTag( address, AllenBradleyHelper.CIP_Type_DWord, ByteTransform.TransByte( values ), values.Length );

		/// <inheritdoc/>
		[HslMqttApi( "WriteUInt32Array", "" )]
		public override OperateResult Write( string address, uint[] values ) => WriteTag( address, AllenBradleyHelper.CIP_Type_UDint, ByteTransform.TransByte( values ), values.Length );

		/// <inheritdoc/>
		[HslMqttApi( "WriteFloatArray", "" )]
		public override OperateResult Write( string address, float[] values ) => WriteTag( address, AllenBradleyHelper.CIP_Type_Real, ByteTransform.TransByte( values ), values.Length );

		/// <inheritdoc/>
		[HslMqttApi( "WriteInt64Array", "" )]
		public override OperateResult Write( string address, long[] values ) => WriteTag( address, AllenBradleyHelper.CIP_Type_LInt, ByteTransform.TransByte( values ), values.Length );

		/// <inheritdoc/>
		[HslMqttApi( "WriteUInt64Array", "" )]
		public override OperateResult Write( string address, ulong[] values ) => WriteTag( address, AllenBradleyHelper.CIP_Type_ULint, ByteTransform.TransByte( values ), values.Length );

		/// <inheritdoc/>
		[HslMqttApi( "WriteDoubleArray", "" )]
		public override OperateResult Write( string address, double[] values ) => WriteTag( address, AllenBradleyHelper.CIP_Type_Double, ByteTransform.TransByte( values ), values.Length );

		/// <inheritdoc/>
		[HslMqttApi( "WriteString", "" )]
		public override OperateResult Write( string address, string value )
		{
			return Write( address, value, Encoding.UTF8 );
		}

		/// <inheritdoc/>
		public override OperateResult Write( string address, string value, Encoding encoding )
		{
			byte[] buffer = string.IsNullOrEmpty( value ) ? new byte[0] : encoding.GetBytes( value );
			return WriteTag( address, AllenBradleyHelper.CIP_Type_String, SoftBasic.SpliceArray( BitConverter.GetBytes( (ushort)buffer.Length ), buffer ) );
		}

		/// <inheritdoc/>
		[HslMqttApi( "WriteBool", "" )]
		public override OperateResult Write( string address, bool value ) => WriteTag( address, AllenBradleyHelper.CIP_Type_Bool, value ? new byte[] { 0xFF, 0xFF } : new byte[] { 0x00, 0x00 } );

		/// <inheritdoc cref="IReadWriteNet.Write(string, bool[])"/>
		[HslMqttApi( "WriteBoolArray", "" )]
		public override OperateResult Write( string address, bool[] value )
		{
			return WriteTag( address, AllenBradleyHelper.CIP_Type_Bool, value.Select( m => m ? (byte)0x01 : (byte)0x00 ).ToArray( ), HslHelper.IsAddressEndWithIndex(address) ? value.Length : 1 );
		}

		/// <inheritdoc/>
		[HslMqttApi( "WriteByte", "" )]
		public OperateResult Write( string address, byte value ) => WriteTag( address, AllenBradleyHelper.CIP_Type_Byte, new byte[] { value } );

		// public OperateResult Write( string address, sbyte value ) => WriteTag( address, AllenBradleyHelper.CIP_Type_USInt, new byte[] { (byte)value } );

		#endregion

		#region Async Write Override
#if !NET35 && !NET20
		/// <inheritdoc cref="Write(string, short[])"/>
		public override async Task<OperateResult> WriteAsync( string address, short[] values ) => await WriteTagAsync( address, AllenBradleyHelper.CIP_Type_Word, ByteTransform.TransByte( values ), values.Length );

		/// <inheritdoc cref="Write(string, ushort[])"/>
		public override async Task<OperateResult> WriteAsync( string address, ushort[] values ) => await WriteTagAsync( address, AllenBradleyHelper.CIP_Type_UInt, ByteTransform.TransByte( values ), values.Length );

		/// <inheritdoc cref="Write(string, int[])"/>
		public override async Task<OperateResult> WriteAsync( string address, int[] values ) => await WriteTagAsync( address, AllenBradleyHelper.CIP_Type_DWord, ByteTransform.TransByte( values ), values.Length );

		/// <inheritdoc cref="Write(string, uint[])"/>
		public override async Task<OperateResult> WriteAsync( string address, uint[] values ) => await WriteTagAsync( address, AllenBradleyHelper.CIP_Type_UDint, ByteTransform.TransByte( values ), values.Length );

		/// <inheritdoc cref="Write(string, float[])"/>
		public override async Task<OperateResult> WriteAsync( string address, float[] values ) => await WriteTagAsync( address, AllenBradleyHelper.CIP_Type_Real, ByteTransform.TransByte( values ), values.Length );

		/// <inheritdoc cref="Write(string, long[])"/>
		public override async Task<OperateResult> WriteAsync( string address, long[] values ) => await WriteTagAsync( address, AllenBradleyHelper.CIP_Type_LInt, ByteTransform.TransByte( values ), values.Length );

		/// <inheritdoc cref="Write(string, ulong[])"/>
		public override async Task<OperateResult> WriteAsync( string address, ulong[] values ) => await WriteTagAsync( address, AllenBradleyHelper.CIP_Type_ULint, ByteTransform.TransByte( values ), values.Length );

		/// <inheritdoc cref="Write(string, double[])"/>
		public override async Task<OperateResult> WriteAsync( string address, double[] values ) => await WriteTagAsync( address, AllenBradleyHelper.CIP_Type_Double, ByteTransform.TransByte( values ), values.Length );

		/// <inheritdoc cref="Write(string, string)"/>
		public override async Task<OperateResult> WriteAsync( string address, string value )
		{
			return await WriteAsync( address, value, Encoding.UTF8 );
		}

		/// <inheritdoc/>
		public override async Task<OperateResult> WriteAsync( string address, string value, Encoding encoding )
		{
			byte[] buffer = string.IsNullOrEmpty( value ) ? new byte[0] : encoding.GetBytes( value );
			return await WriteTagAsync( address, AllenBradleyHelper.CIP_Type_String, SoftBasic.SpliceArray( BitConverter.GetBytes( (ushort)buffer.Length ), buffer ) );
		}

		/// <inheritdoc cref="Write(string, bool)"/>
		public override async Task<OperateResult> WriteAsync( string address, bool value ) => await WriteTagAsync( address, AllenBradleyHelper.CIP_Type_Bool, value ? new byte[] { 0xFF, 0xFF } : new byte[] { 0x00, 0x00 } );

		/// <inheritdoc cref="Write(string, bool[])"/>
		public override async Task<OperateResult> WriteAsync( string address, bool[] value )
		{
			return await WriteTagAsync( address, AllenBradleyHelper.CIP_Type_Bool, value.Select( m => m ? (byte)0x01 : (byte)0x00 ).ToArray( ), HslHelper.IsAddressEndWithIndex( address ) ? value.Length : 1 );
		}

		/// <inheritdoc cref="Write(string, byte)"/>
		public async Task<OperateResult> WriteAsync( string address, byte value ) => await WriteTagAsync( address, AllenBradleyHelper.CIP_Type_Byte, new byte[] { value } );
#endif
		#endregion

		#region Date ReadWrite

		/// <inheritdoc cref="AllenBradleyHelper.ReadDate(IReadWriteCip, string)"/>
		public OperateResult<DateTime> ReadDate( string address ) => AllenBradleyHelper.ReadDate( this, address );

		/// <inheritdoc cref="AllenBradleyHelper.WriteDate(IReadWriteCip, string, DateTime)"/>
		public OperateResult WriteDate( string address, DateTime date ) => AllenBradleyHelper.WriteDate( this, address, date );

		/// <inheritdoc cref="WriteDate(string, DateTime)"/>
		public OperateResult WriteTimeAndDate( string address, DateTime date ) => AllenBradleyHelper.WriteTimeAndDate( this, address, date );

		/// <inheritdoc cref="AllenBradleyHelper.ReadTime(IReadWriteCip, string)"/>
		public OperateResult<TimeSpan> ReadTime( string address ) => AllenBradleyHelper.ReadTime( this, address );

		/// <inheritdoc cref="AllenBradleyHelper.WriteTime(IReadWriteCip, string, TimeSpan)"/>
		public OperateResult WriteTime( string address, TimeSpan time ) => AllenBradleyHelper.WriteTime( this, address, time );

		/// <inheritdoc cref="AllenBradleyHelper.WriteTimeOfDate(IReadWriteCip, string, TimeSpan)"/>
		public OperateResult WriteTimeOfDate( string address, TimeSpan timeOfDate ) => AllenBradleyHelper.WriteTimeOfDate( this, address, timeOfDate );
#if !NET20 && !NET35
		/// <inheritdoc cref="ReadDate(string)"/>
		public async Task<OperateResult<DateTime>> ReadDateAsync( string address ) => await AllenBradleyHelper.ReadDateAsync( this, address );

		/// <inheritdoc cref="WriteDate(string, DateTime)"/>
		public async Task<OperateResult> WriteDateAsync( string address, DateTime date ) => await AllenBradleyHelper.WriteDateAsync( this, address, date );

		/// <inheritdoc cref="WriteTimeAndDate(string, DateTime)"/>
		public async Task<OperateResult> WriteTimeAndDateAsync( string address, DateTime date ) => await AllenBradleyHelper.WriteTimeAndDateAsync( this, address, date );

		/// <inheritdoc cref="ReadTime(string)"/>
		public async Task<OperateResult<TimeSpan>> ReadTimeAsync( string address ) => await AllenBradleyHelper.ReadTimeAsync( this, address );

		/// <inheritdoc cref="WriteTime(string, TimeSpan)"/>
		public async Task<OperateResult> WriteTimeAsync( string address, TimeSpan time ) => await AllenBradleyHelper.WriteTimeAsync( this, address, time );

		/// <inheritdoc cref="WriteTimeOfDate(string, TimeSpan)"/>
		public async Task<OperateResult> WriteTimeOfDateAsync( string address, TimeSpan timeOfDate ) => await AllenBradleyHelper.WriteTimeOfDateAsync( this, address, timeOfDate );
#endif
		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString( ) => $"OmronConnectedCipNet[{IpAddress}:{Port}]";

		#endregion
	}
}
