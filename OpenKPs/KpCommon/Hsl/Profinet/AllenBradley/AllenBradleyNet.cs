using HslCommunication.BasicFramework;
using HslCommunication.Core;
using HslCommunication.Core.IMessage;
using HslCommunication.Core.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using HslCommunication.Reflection;
using KpCommon.Hsl.Profinet.AllenBradley.InterFace;
#if !NET35 && !NET20
using System.Threading.Tasks;
#endif

/*********************************************************************************************
 * 
 *    thanks: 江阴-  ∮溪风-⊙_⌒ 提供了测试的PLC
 *    thanks: 上海-null 给了大量改进的意见和说明
 *    
 *    感谢一个开源的java项目支持才使得本项目顺利开发：https://github.com/Tulioh/Ethernetip4j
 *    尽管本项目的ab类实现的功能已经超过上面的开源库很多了，还是由衷的感谢
 * 
 ***********************************************************************************************/

namespace HslCommunication.Profinet.AllenBradley
{
	/// <summary>
	/// AB PLC的数据通信类，使用CIP协议实现，适用1756，1769等型号，支持使用标签的形式进行读写操作，支持标量数据，一维数组，二维数组，三维数组等等。如果是局部变量，那么使用 Program:MainProgram.[变量名]。<br />
	/// The data communication class of AB PLC is implemented using the CIP protocol. It is suitable for 1756, 1769 and other models. 
	/// It supports reading and writing in the form of tags, scalar data, one-dimensional array, two-dimensional array, 
	/// three-dimensional array, and so on. If it is a local variable, use the Program:MainProgram.[Variable name].
	/// </summary>
	/// <remarks>
	/// thanks 江阴-  ∮溪风-⊙_⌒ help test the dll
	/// <br />
	/// thanks 上海-null 测试了这个dll
	/// <br />
	/// <br />
	/// 默认的地址就是PLC里的TAG名字，比如A，B，C；如果你需要读取的数据是一个数组，那么A就是默认的A[0]，如果想要读取偏移量为10的数据，那么地址为A[10]，
	/// 多维数组同理，使用A[10,10,10]的操作。
	/// <br />
	/// <br />
	/// 假设你读取的是局部变量，那么使用 Program:MainProgram.变量名<br />
	/// 目前适用的系列为1756 ControlLogix, 1756 GuardLogix, 1769 CompactLogix, 1769 Compact GuardLogix, 1789SoftLogix, 5069 CompactLogix, 5069 Compact GuardLogix, Studio 5000 Logix Emulate
	/// <br />
	/// <br />
	/// 如果你有个Bool数组要读取，变量名为 A, 那么读第0个位，可以通过 ReadBool("A")，但是第二个位需要使用<br />
	/// ReadBoolArray("A[0]")   // 返回32个bool长度，0-31的索引，如果我想读取32-63的位索引，就需要 ReadBoolArray("A[1]") ，以此类推。
	/// <br />
	/// <br />
	/// 地址可以携带站号信息，只要在前面加上slot=2;即可，这就是访问站号2的数据了，例如 slot=2;AAA，如果使用了自定义的消息路由，例如：[IP or Hostname],1,[Optional Routing Path],CPU Slot 172.20.1.109,1,[15,2,18,1],12<br />
	/// 在实例化之后，连接PLC之前，需要调用如下代码 plc.MessageRouter = new MessageRouter( "1.15.2.18.1.12" )
	/// </remarks>
	public class AllenBradleyNet : NetworkDeviceBase, IReadWriteCip, IAbReadWriteCip
    {
		#region Constructor

		/// <summary>
		/// Instantiate a communication object for a Allenbradley PLC protocol
		/// </summary>
		public AllenBradleyNet( )
		{
			WordLength          = 2;
			ByteTransform       = new RegularByteTransform( );
		}

		/// <summary>
		/// Instantiate a communication object for a Allenbradley PLC protocol
		/// </summary>
		/// <param name="ipAddress">PLC IpAddress</param>
		/// <param name="port">PLC Port</param>
		public AllenBradleyNet( string ipAddress, int port = 44818 ) : this( )
		{
			IpAddress          = ipAddress;
			Port               = port;
		}

		/// <inheritdoc/>
		protected override INetMessage GetNewNetMessage( ) => new AllenBradleyMessage( );

		/// <inheritdoc/>
		public override byte[] PackCommandWithHeader( byte[] command )
		{
			return AllenBradleyHelper.PackRequestHeader( CipCommand, SessionHandle, command );
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// The current session handle, which is determined by the PLC when communicating with the PLC handshake
		/// </summary>
		public uint SessionHandle { get; protected set; }

		/// <summary>
		/// Gets or sets the slot number information for the current plc, which should be set before connections
		/// </summary>
		public byte Slot { get; set; } = 0;

		/// <summary>
		/// port and slot information
		/// </summary>
		public byte[] PortSlot { get; set; }

		/// <summary>
		/// 获取或设置整个交互指令的控制码，默认为0x6F，通常不需要修改<br />
		/// Gets or sets the control code of the entire interactive instruction. The default is 0x6F, and usually does not need to be modified.
		/// </summary>
		public ushort CipCommand { get; set; } = 0x6F;

		/// <summary>
		/// 获取或设置当前的通信的消息路由信息，可以实现一些复杂情况的通信，数据包含背板号，路由参数，slot，例如：1.15.2.18.1.1<br />
		/// Get or set the message routing information of the current communication, which can realize some complicated communication. 
		/// The data includes the backplane number, routing parameters, and slot, for example: 1.15.2.18.1.1
		/// </summary>
		public MessageRouter MessageRouter { get; set; }

		#endregion

		#region Double Mode Override

		/// <inheritdoc/>
		protected override OperateResult InitializationOnConnect( Socket socket )
		{
			// Registering Session Information
			OperateResult<byte[]> read = ReadFromCoreServer( socket, AllenBradleyHelper.RegisterSessionHandle( ), hasResponseData: true, usePackAndUnpack: false );
			if (!read.IsSuccess) return read;

			// Check the returned status
			OperateResult check = AllenBradleyHelper.CheckResponse( read.Content );
			if (!check.IsSuccess) return check;

			// Extract session ID
			SessionHandle = ByteTransform.TransUInt32( read.Content, 4 );

			// 使用消息路由
			if (MessageRouter != null)
			{
				byte[] cip = MessageRouter.GetRouterCIP( );
				OperateResult<byte[]> messageRouter = ReadFromCoreServer( socket, AllenBradleyHelper.PackRequestHeader( 0x6f, SessionHandle, AllenBradleyHelper.PackCommandSpecificData( new byte[] { 0x00, 0x00, 0x00, 0x00 }, 
					AllenBradleyHelper.PackCommandSingleService( cip, 0xB2 ) ) ), hasResponseData: true, usePackAndUnpack: false );
				if (!messageRouter.IsSuccess) return messageRouter;
			}

			return OperateResult.CreateSuccessResult( );
		}

		/// <inheritdoc/>
		protected override OperateResult ExtraOnDisconnect( Socket socket )
		{
			if (socket != null)
			{
				// Unregister session Information
				OperateResult<byte[]> read = ReadFromCoreServer( socket, AllenBradleyHelper.UnRegisterSessionHandle( SessionHandle ), hasResponseData: true, usePackAndUnpack: false );
				if (!read.IsSuccess) return read;
			}

			return OperateResult.CreateSuccessResult( );
		}

		#endregion

		#region Async Double Mode Override
#if !NET35 && !NET20
		/// <inheritdoc/>
		protected override async Task<OperateResult> InitializationOnConnectAsync( Socket socket )
		{
			// Registering Session Information
			OperateResult<byte[]> read = await ReadFromCoreServerAsync( socket, AllenBradleyHelper.RegisterSessionHandle( ), hasResponseData: true, usePackAndUnpack: false );
			if (!read.IsSuccess) return read;

			// Check the returned status
			OperateResult check = AllenBradleyHelper.CheckResponse( read.Content );
			if (!check.IsSuccess) return check;

			// Extract session ID
			SessionHandle = ByteTransform.TransUInt32( read.Content, 4 );

			// 使用消息路由
			if (MessageRouter != null)
			{
				byte[] cip = MessageRouter.GetRouterCIP( );
				OperateResult<byte[]> messageRouter = await ReadFromCoreServerAsync( socket, AllenBradleyHelper.PackRequestHeader( 0x6f, SessionHandle, AllenBradleyHelper.PackCommandSpecificData( new byte[] { 0x00, 0x00, 0x00, 0x00 },
					AllenBradleyHelper.PackCommandSingleService( cip, 0xB2 ) ) ), hasResponseData: true, usePackAndUnpack: false );
				if (!messageRouter.IsSuccess) return messageRouter;
			}

			return OperateResult.CreateSuccessResult( );
		}

		/// <inheritdoc/>
		protected override async Task<OperateResult> ExtraOnDisconnectAsync( Socket socket )
		{
			if (socket != null)
			{
				// Unregister session Information
				OperateResult<byte[]> read = await ReadFromCoreServerAsync( socket, AllenBradleyHelper.UnRegisterSessionHandle( SessionHandle ), hasResponseData: true, usePackAndUnpack: false );
				if (!read.IsSuccess) return read;
			}

			return OperateResult.CreateSuccessResult( );
		}
#endif
		#endregion

		#region Build Command

		/// <summary>
		/// 创建一个读取标签的报文指定，标签地址可以手动动态指定slot编号，例如 slot=2;AAA<br />
		/// Build a read command bytes, The label address can manually specify the slot number dynamically, for example slot=2;AAA
		/// </summary>
		/// <param name="address">the address of the tag name</param>
		/// <param name="length">Array information, if not arrays, is 1 </param>
		/// <returns>Message information that contains the result object </returns>
		public virtual OperateResult<byte[]> BuildReadCommand( string[] address, int[] length )
		{
			if (address == null || length == null) return new OperateResult<byte[]>( "address or length is null" );
			if (address.Length != length.Length) return new OperateResult<byte[]>( "address and length is not same array" );

			try
			{
				byte slotTmp = this.Slot;
				List<byte[]> cips = new List<byte[]>( );
				for (int i = 0; i < address.Length; i++)
				{
					slotTmp = (byte)HslHelper.ExtractParameter( ref address[i], "slot", this.Slot );
					cips.Add( AllenBradleyHelper.PackRequsetRead( address[i], length[i] ) );
				}
				byte[] commandSpecificData = AllenBradleyHelper.PackCommandSpecificData( new byte[4], PackCommandService( PortSlot ?? new byte[] { 0x01, slotTmp }, cips.ToArray( ) ) );

				return OperateResult.CreateSuccessResult( commandSpecificData );
			}
			catch (Exception ex)
			{
				return new OperateResult<byte[]>( "Address Wrong:" + ex.Message );
			}
		}

		/// <summary>
		/// 创建一个读取多标签的报文<br />
		/// Build a read command bytes
		/// </summary>
		/// <param name="address">The address of the tag name </param>
		/// <returns>Message information that contains the result object </returns>
		public OperateResult<byte[]> BuildReadCommand( string[] address )
		{
			if (address == null) return new OperateResult<byte[]>( "address or length is null" );

			int[] length = new int[address.Length];
			for (int i = 0; i < address.Length; i++)
			{
				length[i] = 1;
			}

			return BuildReadCommand( address, length );
		}

		/// <summary>
		/// Create a written message instruction
		/// </summary>
		/// <param name="address">The address of the tag name </param>
		/// <param name="typeCode">Data type</param>
		/// <param name="data">Source Data </param>
		/// <param name="length">In the case of arrays, the length of the array </param>
		/// <returns>Message information that contains the result object</returns>
		protected virtual OperateResult<byte[]> BuildWriteCommand( string address, ushort typeCode, byte[] data, int length = 1 )
		{
			try
			{
				byte slotTmp = (byte)HslHelper.ExtractParameter( ref address, "slot", this.Slot );
				byte[] cip = AllenBradleyHelper.PackRequestWrite( address, typeCode, data, length );
				byte[] commandSpecificData = AllenBradleyHelper.PackCommandSpecificData( new byte[4], PackCommandService( PortSlot ?? new byte[] { 0x01, slotTmp }, cip ) );

				return OperateResult.CreateSuccessResult( commandSpecificData );
			}
			catch (Exception ex)
			{
				return new OperateResult<byte[]>( "Address Wrong:" + ex.Message );
			}
		}

		/// <summary>
		/// Create a written message instruction
		/// </summary>
		/// <param name="address">The address of the tag name </param>
		/// <param name="data">Bool Data </param>
		/// <returns>Message information that contains the result object</returns>
		public OperateResult<byte[]> BuildWriteCommand( string address, bool data )
		{
			try
			{
				byte slotTmp = (byte)HslHelper.ExtractParameter( ref address, "slot", this.Slot );
				byte[] cip = AllenBradleyHelper.PackRequestWrite( address, data );
				byte[] commandSpecificData = AllenBradleyHelper.PackCommandSpecificData( new byte[4], PackCommandService( PortSlot ?? new byte[] { 0x01, slotTmp }, cip ) );

				return OperateResult.CreateSuccessResult( commandSpecificData );
			}
			catch (Exception ex)
			{
				return new OperateResult<byte[]>( "Address Wrong:" + ex.Message );
			}
		}

		private OperateResult CheckResponse( byte[] response )
		{
			OperateResult check = AllenBradleyHelper.CheckResponse( response );
			if (!check.IsSuccess)
			{
				if (check.ErrorCode == 0x64)
					GetPipeSocket( ).IsSocketError = true;
			}
			return check;
		}

		#endregion

		#region Override Read

		/// <summary>
		/// Read data information, data length for read array length information
		/// </summary>
		/// <param name="address">Address format of the node</param>
		/// <param name="length">In the case of arrays, the length of the array </param>
		/// <returns>Result data with result object </returns>
		[HslMqttApi( "ReadByteArray", "" )]
		public override OperateResult<byte[]> Read( string address, ushort length )
		{
			HslHelper.ExtractParameter( ref address, "type", 0 );
			if (length > 1)
				return ReadSegment( address, 0, length );
			else
				return Read( new string[] { address }, new int[] { length } );
		}

		/// <summary>
		/// <b>[商业授权]</b> 批量读取多地址的数据信息，例如我可以读取两个标签的数据 "A","B[0]"，每个地址的数据长度为1，表示一个数据，最终读取返回的是一整个的字节数组，需要自行解析<br />
		/// <b>[Authorization]</b> Batch read data information of multiple addresses, for example, I can read the data of two tags "A", "B[0]", the data length of each address is 1, 
		/// which means one data, and the final read returns a The entire byte array, which needs to be parsed by itself
		/// </summary>
		/// <param name="address">Name of the node </param>
		/// <returns>Result data with result object </returns>
		[HslMqttApi( "ReadAddress", "" )]
		public OperateResult<byte[]> Read( string[] address )
		{
			if (address == null) return new OperateResult<byte[]>( "address can not be null" );

			int[] length = new int[address.Length];
			for (int i = 0; i < length.Length; i++)
			{
				length[i] = 1;
			}

			return Read( address, length );
		}

		/// <summary>
		/// <b>[商业授权]</b> 批量读取多地址的数据信息，例如我可以读取两个标签的数据 "A","B[0]"， 长度为 [1, 5]，返回的是一整个的字节数组，需要自行解析<br />
		/// <b>[Authorization]</b> Read the data information of multiple addresses in batches. For example, I can read the data "A", "B[0]" of two tags, 
		/// the length is [1, 5], and the return is an entire byte array, and I need to do it myself Parsing
		/// </summary>
		/// <param name="address">节点的名称 -> Name of the node </param>
		/// <param name="length">如果是数组，就为数组长度 -> In the case of arrays, the length of the array </param>
		/// <returns>带有结果对象的结果数据 -> Result data with result object </returns>
		public OperateResult<byte[]> Read( string[] address, int[] length )
		{
			if(address?.Length > 1)
			{
				if (!Authorization.asdniasnfaksndiqwhawfskhfaiw( )) return new OperateResult<byte[]>( StringResources.Language.InsufficientPrivileges );
			}

			OperateResult<byte[], ushort, bool> read = ReadWithType( address, length );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( read );

			return OperateResult.CreateSuccessResult( read.Content1 );
		}

		private OperateResult<byte[], ushort, bool> ReadWithType( string[] address, int[] length )
		{
			// 指令生成 -> Instruction Generation
			OperateResult<byte[]> command = BuildReadCommand( address, length );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<byte[], ushort, bool>( command ); ;

			// 核心交互 -> Core Interactions
			OperateResult<byte[]> read = ReadFromCoreServer( command.Content );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[], ushort, bool>( read ); ;

			// 检查反馈 -> Check Feedback
			OperateResult check = CheckResponse( read.Content );
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[], ushort, bool>( check );

			// 提取数据 -> Extracting data
			return AllenBradleyHelper.ExtractActualData( read.Content, true );
		}

		/// <summary>
		/// Read Segment Data Array form plc, use address tag name
		/// </summary>
		/// <param name="address">Tag name in plc</param>
		/// <param name="startIndex">array start index, uint byte index</param>
		/// <param name="length">array length, data item length</param>
		/// <returns>Results Bytes</returns>
		[HslMqttApi( "ReadSegment", "" )]
		public OperateResult<byte[]> ReadSegment( string address, int startIndex, int length )
		{
			try
			{
				List<byte> bytesContent = new List<byte>( );
				while (true)
				{
					OperateResult<byte[]> read = ReadCipFromServer( AllenBradleyHelper.PackRequestReadSegment( address, startIndex, length ) );
					if (!read.IsSuccess) return read;

					// 提取数据 -> Extracting data
					OperateResult<byte[], ushort, bool> analysis = AllenBradleyHelper.ExtractActualData( read.Content, true );
					if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( analysis );

					startIndex += analysis.Content1.Length;
					bytesContent.AddRange( analysis.Content1 );

					if (!analysis.Content3) break;
				}

				return OperateResult.CreateSuccessResult( bytesContent.ToArray( ) );
			}
			catch (Exception ex)
			{
				return new OperateResult<byte[]>( "Address Wrong:" + ex.Message );
			}
		}

		private OperateResult<byte[]> ReadByCips( params byte[][] cips )
		{
			OperateResult<byte[]> read = ReadCipFromServer( cips );
			if (!read.IsSuccess) return read;

			// 提取数据 -> Extracting data
			OperateResult<byte[], ushort, bool> analysis = AllenBradleyHelper.ExtractActualData( read.Content, true );
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( analysis );

			return OperateResult.CreateSuccessResult( analysis.Content1 );
		}

		/// <summary>
		/// 使用CIP报文和服务器进行核心的数据交换
		/// </summary>
		/// <param name="cips">Cip commands</param>
		/// <returns>Results Bytes</returns>
		public OperateResult<byte[]> ReadCipFromServer( params byte[][] cips )
		{
			byte[] commandSpecificData = AllenBradleyHelper.PackCommandSpecificData( new byte[4], PackCommandService( PortSlot ?? new byte[] { 0x01, Slot }, cips.ToArray( ) ) );

			// 核心交互 -> Core Interactions
			OperateResult<byte[]> read = ReadFromCoreServer( commandSpecificData );
			if (!read.IsSuccess) return read;

			// 检查反馈 -> Check Feedback
			OperateResult check = CheckResponse( read.Content );
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( check );

			return OperateResult.CreateSuccessResult( read.Content );
		}

		/// <summary>
		/// 使用EIP报文和服务器进行核心的数据交换
		/// </summary>
		/// <param name="eip">eip commands</param>
		/// <returns>Results Bytes</returns>
		public OperateResult<byte[]> ReadEipFromServer( params byte[][] eip )
		{
			byte[] commandSpecificData = AllenBradleyHelper.PackCommandSpecificData( eip );

			// 核心交互 -> Core Interactions
			OperateResult<byte[]> read = ReadFromCoreServer( commandSpecificData );
			if (!read.IsSuccess) return read;

			// 检查反馈 -> Check Feedback
			OperateResult check = CheckResponse( read.Content );
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( check );

			return OperateResult.CreateSuccessResult( read.Content );
		}

		/// <summary>
		/// 读取单个的bool数据信息，如果读取的是单bool变量，就直接写变量名，如果是由int组成的bool数组的一个值，一律带"i="开头访问，例如"i=A[0]" <br />
		/// Read a single bool data information, if it is a single bool variable, write the variable name directly, 
		/// if it is a value of a bool array composed of int, it is always accessed with "i=" at the beginning, for example, "i=A[0]"
		/// </summary>
		/// <param name="address">节点的名称 -> Name of the node </param>
		/// <returns>带有结果对象的结果数据 -> Result data with result info </returns>
		[HslMqttApi( "ReadBool", "" )]
		public override OperateResult<bool> ReadBool( string address )
		{
			if ( address.StartsWith( "i=" ) )
			{
				return ByteTransformHelper.GetResultFromArray( ReadBool( address, 1 ) );
			}
			else
			{
				OperateResult<byte[]> read = Read( address, 1 );
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool>( read );

				return OperateResult.CreateSuccessResult( ByteTransform.TransBool( read.Content, 0 ) );
			}
		}

		/// <inheritdoc/>
		[HslMqttApi( "ReadBoolArray", "" )]
		public override OperateResult<bool[]> ReadBool( string address, ushort length )
		{
			if (address.StartsWith( "i=" ))
			{
				address = address.Substring( 2 );
				address = AllenBradleyHelper.AnalysisArrayIndex( address, out int bitIndex );

				string uintIndex = (bitIndex / 32) == 0 ? $"" : $"[{bitIndex / 32}]";
				ushort len = (ushort)HslHelper.CalculateOccupyLength( bitIndex, length, 32 );

				OperateResult<byte[]> read = Read( address + uintIndex, len );
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( read );

				return OperateResult.CreateSuccessResult( read.Content.ToBoolArray( ).SelectMiddle( bitIndex % 32, length ) );
			}
			else
			{
				OperateResult<byte[]> read = Read( address, length );
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( read );

				return OperateResult.CreateSuccessResult( SoftBasic.ByteToBoolArray( read.Content, length ) );
			}
		}

		/// <summary>
		/// 批量读取的bool数组信息，如果你有个Bool数组变量名为 A, 那么读第0个位，可以通过 ReadBool("A")，但是第二个位需要使用 
		/// ReadBoolArray("A[0]")   // 返回32个bool长度，0-31的索引，如果我想读取32-63的位索引，就需要 ReadBoolArray("A[1]") ，以此类推。<br />
		/// For batch read bool array information, if you have a Bool array variable named A, then you can read the 0th bit through ReadBool("A"), 
		/// but the second bit needs to use ReadBoolArray("A[0]" ) // Returns the length of 32 bools, the index is 0-31, 
		/// if I want to read the bit index of 32-63, I need ReadBoolArray("A[1]"), and so on.
		/// </summary>
		/// <param name="address">节点的名称 -> Name of the node </param>
		/// <returns>带有结果对象的结果数据 -> Result data with result info </returns>
		[HslMqttApi( "ReadBoolArrayAddress", "" )]
		public OperateResult<bool[]> ReadBoolArray( string address )
		{
			OperateResult<byte[]> read = Read( address, 1 );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( read );

			return OperateResult.CreateSuccessResult( read.Content.ToBoolArray( ) );
		}

		/// <summary>
		/// 读取PLC的byte类型的数据<br />
		/// Read the byte type of PLC data
		/// </summary>
		/// <param name="address">节点的名称 -> Name of the node </param>
		/// <returns>带有结果对象的结果数据 -> Result data with result info </returns>
		[HslMqttApi( "ReadByte", "" )]
		public OperateResult<byte> ReadByte( string address ) => ByteTransformHelper.GetResultFromArray( Read( address, 1 ) );

		/// <summary>
		/// 从PLC里读取一个指定标签名的原始数据信息及其数据类型信息<br />
		/// Read the original data information of a specified tag name and its data type information from the PLC
		/// </summary>
		/// <remarks>
		/// 数据类型的定义，可以参考 <see cref="AllenBradleyHelper"/> 的常量资源信息。
		/// </remarks>
		/// <param name="address">PLC的标签地址信息</param>
		/// <param name="length">读取的数据长度</param>
		/// <returns>包含原始数据信息及数据类型的结果对象</returns>
		public OperateResult<ushort, byte[]> ReadTag( string address, int length = 1 )
		{
			OperateResult<byte[], ushort, bool> read = ReadWithType( new string[] { address }, new int[] { length } );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<ushort, byte[]>( read );

			return OperateResult.CreateSuccessResult( read.Content2, read.Content1 );
		}

		#endregion

		#region Async Override Read
#if !NET35 && !NET20
		/// <inheritdoc cref="Read(string, ushort)"/>
		public override async Task<OperateResult<byte[]>> ReadAsync( string address, ushort length )
		{
			HslHelper.ExtractParameter( ref address, "type", 0 );
			if (length > 1)
				return await ReadSegmentAsync( address, 0, length );
			else
				return await ReadAsync( new string[] { address }, new int[] { length } );
		}

		/// <inheritdoc cref="Read(string[])"/>
		public async Task<OperateResult<byte[]>> ReadAsync( string[] address )
		{
			if (address == null) return new OperateResult<byte[]>( "address can not be null" );

			int[] length = new int[address.Length];
			for (int i = 0; i < length.Length; i++)
			{
				length[i] = 1;
			}

			return await ReadAsync( address, length );
		}

		/// <inheritdoc cref="Read(string[], int[])"/>
		public async Task<OperateResult<byte[]>> ReadAsync( string[] address, int[] length )
		{
			if (address?.Length > 1)
			{
				if (!Authorization.asdniasnfaksndiqwhawfskhfaiw( )) return new OperateResult<byte[]>( StringResources.Language.InsufficientPrivileges );
			}

			OperateResult<byte[], ushort, bool> read = await ReadWithTypeAsync( address, length );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( read );

			return OperateResult.CreateSuccessResult( read.Content1 );
		}

		private async Task<OperateResult<byte[], ushort, bool>> ReadWithTypeAsync( string[] address, int[] length )
		{
			// 指令生成 -> Instruction Generation
			OperateResult<byte[]> command = BuildReadCommand( address, length );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<byte[], ushort, bool>( command ); ;

			// 核心交互 -> Core Interactions
			OperateResult<byte[]> read = await ReadFromCoreServerAsync( command.Content );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[], ushort, bool>( read ); ;

			// 检查反馈 -> Check Feedback
			OperateResult check = CheckResponse( read.Content );
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[], ushort, bool>( check );

			// 提取数据 -> Extracting data
			return AllenBradleyHelper.ExtractActualData( read.Content, true );
		}

		/// <inheritdoc cref="ReadSegment(string, int, int)"/>
		public async Task<OperateResult<byte[]>> ReadSegmentAsync( string address, int startIndex, int length )
		{
			try
			{
				List<byte> bytesContent = new List<byte>( );
				while (true)
				{
					OperateResult<byte[]> read = await ReadCipFromServerAsync( AllenBradleyHelper.PackRequestReadSegment( address, startIndex, length ) );
					if (!read.IsSuccess) return read;

					// 提取数据 -> Extracting data
					OperateResult<byte[], ushort, bool> analysis = AllenBradleyHelper.ExtractActualData( read.Content, true );
					if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( analysis );

					startIndex += analysis.Content1.Length;
					bytesContent.AddRange( analysis.Content1 );

					if (!analysis.Content3) break;
				}

				return OperateResult.CreateSuccessResult( bytesContent.ToArray( ) );
			}
			catch (Exception ex)
			{
				return new OperateResult<byte[]>( "Address Wrong:" + ex.Message );
			}
		}

		/// <inheritdoc cref="ReadCipFromServer(byte[][])"/>
		public async Task<OperateResult<byte[]>> ReadCipFromServerAsync( params byte[][] cips )
		{
			byte[] commandSpecificData = AllenBradleyHelper.PackCommandSpecificData( new byte[4], PackCommandService( PortSlot ?? new byte[] { 0x01, Slot }, cips.ToArray( ) ) );

			// 核心交互 -> Core Interactions
			OperateResult<byte[]> read = await ReadFromCoreServerAsync( commandSpecificData );
			if (!read.IsSuccess) return read;

			// 检查反馈 -> Check Feedback
			OperateResult check = CheckResponse( read.Content );
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( check );

			return OperateResult.CreateSuccessResult( read.Content );
		}

		/// <inheritdoc cref="ReadEipFromServer(byte[][])"/>
		public async Task<OperateResult<byte[]>> ReadEipFromServerAsync( params byte[][] eip )
		{
			byte[] commandSpecificData = AllenBradleyHelper.PackCommandSpecificData( eip );

			// 核心交互 -> Core Interactions
			OperateResult<byte[]> read = await ReadFromCoreServerAsync( commandSpecificData );
			if (!read.IsSuccess) return read;

			// 检查反馈 -> Check Feedback
			OperateResult check = CheckResponse( read.Content );
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( check );

			return OperateResult.CreateSuccessResult( read.Content );
		}

		/// <inheritdoc cref="ReadBool(string)"/>
		public async override Task<OperateResult<bool>> ReadBoolAsync( string address )
		{
			if (address.StartsWith( "i=" ))
			{
				return ByteTransformHelper.GetResultFromArray( await ReadBoolAsync( address, 1 ) );
			}
			else
			{
				OperateResult<byte[]> read = await ReadAsync( address, 1 );
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool>( read );

				return OperateResult.CreateSuccessResult( ByteTransform.TransBool( read.Content, 0 ) );
			}
		}

		/// <inheritdoc/>
		public async override Task<OperateResult<bool[]>> ReadBoolAsync( string address, ushort length )
		{
			if (address.StartsWith( "i=" ))
			{
				address = address.Substring( 2 );
				address = AllenBradleyHelper.AnalysisArrayIndex( address, out int bitIndex );

				string uintIndex = (bitIndex / 32) == 0 ? $"" : $"[{bitIndex / 32}]";
				ushort len = (ushort)HslHelper.CalculateOccupyLength( bitIndex, length, 32 );

				OperateResult<byte[]> read = await ReadAsync( address + uintIndex, len );
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( read );

				return OperateResult.CreateSuccessResult( read.Content.ToBoolArray( ).SelectMiddle( bitIndex % 32, length ) );
			}
			else
			{
				OperateResult<byte[]> read = await ReadAsync( address, length );
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( read );

				return OperateResult.CreateSuccessResult( SoftBasic.ByteToBoolArray( read.Content, length ) );
			}
		}

		/// <inheritdoc cref="ReadBoolArray(string)"/>
		public async Task<OperateResult<bool[]>> ReadBoolArrayAsync( string address )
		{
			OperateResult<byte[]> read = await ReadAsync( address, 1 );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( read );

			return OperateResult.CreateSuccessResult( read.Content.ToBoolArray( ) );
		}

		/// <inheritdoc cref="ReadByte(string)"/>
		public async Task<OperateResult<byte>> ReadByteAsync( string address ) => ByteTransformHelper.GetResultFromArray( await ReadAsync( address, 1 ) );


		/// <inheritdoc cref="ReadTag(string, int)"/>
		public async Task<OperateResult<ushort, byte[]>> ReadTagAsync( string address, int length = 1 )
		{
			OperateResult<byte[], ushort, bool> read = await ReadWithTypeAsync( new string[] { address }, new int[] { length } );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<ushort, byte[]>( read );

			return OperateResult.CreateSuccessResult( read.Content2, read.Content1 );
		}

#endif
		#endregion

		#region Tag Enumerator

		/// <summary>
		/// 枚举当前的所有的变量名字，包含结构体信息，除去系统自带的名称数据信息<br />
		/// Enumerate all the current variable names, including structure information, except the name data information that comes with the system
		/// </summary>
		/// <returns>结果对象</returns>
		public OperateResult<AbTagItem[]> TagEnumerator( )
		{
			List<AbTagItem> lists = new List<AbTagItem>( );
			for (int i = 0; i < 2; i++)
			{
				uint instanceAddress = 0;
				while (true)
				{
					OperateResult<byte[]> readCip = ReadCipFromServer( i == 0 ? AllenBradleyHelper.BuildEnumeratorCommand( instanceAddress ) : 
						AllenBradleyHelper.BuildEnumeratorProgrameMainCommand( instanceAddress ) );
					if (!readCip.IsSuccess) return OperateResult.CreateFailedResult<AbTagItem[]>( readCip );

					// 提取数据 -> Extracting data
					OperateResult<byte[], ushort, bool> analysis = AllenBradleyHelper.ExtractActualData( readCip.Content, true );
					if (!analysis.IsSuccess)
					{
						if (i == 1) return OperateResult.CreateSuccessResult( lists.ToArray( ) );
						return OperateResult.CreateFailedResult<AbTagItem[]>( analysis );
					}

					if (readCip.Content.Length >= 43 && BitConverter.ToUInt16( readCip.Content, 40 ) == 0xD5)
					{
						lists.AddRange( AbTagItem.PraseAbTagItems( readCip.Content, 44, isGlobalVariable: i == 0, out uint instance ) );
						instanceAddress = instance + 1;
						if (!analysis.Content3) break;
					}
					else
						return new OperateResult<AbTagItem[]>( StringResources.Language.UnknownError + " Source: " + readCip.Content.ToHexString( ' ' ) );
				}
			}
			return OperateResult.CreateSuccessResult( lists.ToArray( ) );
		}

#if !NET35 && !NET20
		/// <inheritdoc cref="TagEnumerator"/>
		public async Task<OperateResult<AbTagItem[]>> TagEnumeratorAsync( )
		{
			List<AbTagItem> lists = new List<AbTagItem>( );
			for (int i = 0; i < 2; i++)
			{
				uint instanceAddress = 0;
				while (true)
				{
					OperateResult<byte[]> readCip = await ReadCipFromServerAsync( i == 0 ? AllenBradleyHelper.BuildEnumeratorCommand( instanceAddress ) :
						AllenBradleyHelper.BuildEnumeratorProgrameMainCommand( instanceAddress ) );
					if (!readCip.IsSuccess) return OperateResult.CreateFailedResult<AbTagItem[]>( readCip );

					// 提取数据 -> Extracting data
					OperateResult<byte[], ushort, bool> analysis = AllenBradleyHelper.ExtractActualData( readCip.Content, true );
					if (!analysis.IsSuccess)
					{
						if (i == 1) return OperateResult.CreateSuccessResult( lists.ToArray( ) );
						return OperateResult.CreateFailedResult<AbTagItem[]>( analysis );
					}

					if (readCip.Content.Length >= 43 && BitConverter.ToUInt16( readCip.Content, 40 ) == 0xD5)
					{
						lists.AddRange( AbTagItem.PraseAbTagItems( readCip.Content, 44, isGlobalVariable: i == 0, out uint instance ) );
						instanceAddress = instance + 1;
						if (!analysis.Content3) break;
					}
					else
						return new OperateResult<AbTagItem[]>( StringResources.Language.UnknownError + " Source: " + readCip.Content.ToHexString( ' ' ) );
				}
			}
			return OperateResult.CreateSuccessResult( lists.ToArray( ) );
		}
#endif

		private OperateResult<AbStructHandle> ReadTagStructHandle( AbTagItem structTag )
		{
			OperateResult<byte[]> readCip = ReadCipFromServer( AllenBradleyHelper.GetStructHandleCommand( structTag.SymbolType ) );
			if (!readCip.IsSuccess) return OperateResult.CreateFailedResult<AbStructHandle>( readCip );

			if (readCip.Content.Length >= 43 && BitConverter.ToInt32( readCip.Content, 40 ) == 0x83)
				return OperateResult.CreateSuccessResult( new AbStructHandle( readCip.Content, 44 ) );
			else
				return new OperateResult<AbStructHandle>( StringResources.Language.UnknownError + " Source Data: " + readCip.Content.ToHexString( ' ' ) );
		}

		/// <summary>
		/// 枚举结构体的方法，传入结构体的标签对象，返回结构体子属性标签列表信息，子属性有可能是标量数据，也可能是另一个结构体。<br />
		/// The method of enumerating the structure, passing in the tag object of the structure, 
		/// and returning the tag list information of the sub-attributes of the structure. The sub-attributes may be scalar data or another structure.
		/// </summary>
		/// <param name="structTag">结构体的标签</param>
		/// <returns>是否成功</returns>
		public OperateResult<AbTagItem[]> StructTagEnumerator( AbTagItem structTag )
		{
			OperateResult<AbStructHandle> readStruct = ReadTagStructHandle( structTag );
			if (!readStruct.IsSuccess) return OperateResult.CreateFailedResult<AbTagItem[]>( readStruct );

			OperateResult<byte[]> read = ReadCipFromServer( AllenBradleyHelper.GetStructItemNameType( structTag.SymbolType, readStruct.Content, 0x00 ) );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<AbTagItem[]>( read );

			if (read.Content.Length >= 43 && read.Content[40] == 0xCC && read.Content[41] == 0x00 && read.Content[42] == 0x00)
			{
				return OperateResult.CreateSuccessResult( AbTagItem.PraseAbTagItemsFromStruct( read.Content, 44, readStruct.Content ).ToArray( ) );
			}
			else
				return new OperateResult<AbTagItem[]>(StringResources.Language.UnknownError + " Status:" + read.Content[42] );
		}

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
		public OperateResult<string> ReadString( string address ) => ReadString( address, 1 );

		/// <summary>
		/// 读取字符串数据，默认为<see cref="Encoding.UTF8"/>编码<br />
		/// Read string data, default is the <see cref="Encoding.UTF8"/> encoding
		/// </summary>
		/// <param name="address">起始地址</param>
		/// <param name="length">数据长度</param>
		/// <returns>带有成功标识的string数据</returns>
		/// <example>
		/// 以下为三菱的连接对象示例，其他的设备读写情况参照下面的代码：
		/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Core\NetworkDeviceBase.cs" region="ReadString" title="String类型示例" />
		/// </example>
		[HslMqttApi( "ReadString", "" )]
		public override OperateResult<string> ReadString( string address, ushort length )
		{
			return ReadString( address, length, Encoding.UTF8 );
		}

		/// <inheritdoc/>
		public override OperateResult<string> ReadString( string address, ushort length, Encoding encoding )
		{
			OperateResult<byte[]> read = Read( address, length );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<string>( read );

			try
			{
				if (read.Content.Length >= 6)
				{
					int strLength = ByteTransform.TransInt32( read.Content, 2 );
					return OperateResult.CreateSuccessResult( encoding.GetString( read.Content, 6, strLength ) );
				}
				else
				{
					return OperateResult.CreateSuccessResult( encoding.GetString( read.Content ) );
				}
			}
			catch(Exception ex)
			{
				return new OperateResult<string>( ex.Message + " Source: " + read.Content.ToHexString( ' ' ) );
			}
		}

		/// <inheritdoc cref="AllenBradleyHelper.ReadPlcType(IReadWriteDevice)"/>
		[HslMqttApi( Description = "获取PLC的型号信息" )]
		public OperateResult<string> ReadPlcType( ) => AllenBradleyHelper.ReadPlcType( this );

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
		public async Task<OperateResult<string>> ReadStringAsync( string address ) => await ReadStringAsync( address, 1 );

		/// <inheritdoc cref="ReadString(string, ushort)"/>
		public async override Task<OperateResult<string>> ReadStringAsync( string address, ushort length )
		{
			return await ReadStringAsync( address, length, Encoding.UTF8 );
		}

		/// <inheritdoc/>
		public override async Task<OperateResult<string>> ReadStringAsync( string address, ushort length, Encoding encoding )
		{
			OperateResult<byte[]> read = await ReadAsync( address, length );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<string>( read );

			if (read.Content.Length >= 6)
			{
				int strLength = ByteTransform.TransInt32( read.Content, 2 );
				return OperateResult.CreateSuccessResult( encoding.GetString( read.Content, 6, strLength ) );
			}
			else
			{
				return OperateResult.CreateSuccessResult( encoding.GetString( read.Content ) );
			}
		}

		/// <inheritdoc cref="AllenBradleyHelper.ReadPlcType(IReadWriteDevice)"/>
		public async Task<OperateResult<string>> ReadPlcTypeAsync( ) => await AllenBradleyHelper.ReadPlcTypeAsync( this );

#endif
		#endregion

		#region Write Support

		/// <summary>
		/// 当前写入字节数组使用数据类型 0xD1 写入，如果其他的字节类型需要调用 <see cref="WriteTag(string, ushort, byte[], int)"/> 方法来实现。<br />
		/// The currently written byte array is written using the data type 0xD1. If other byte types need to be called <see cref="WriteTag(string, ushort, byte[], int)"/> Method to achieve. <br />
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <returns>写入结果值</returns>
		[HslMqttApi( "WriteByteArray", "" )]
		public override OperateResult Write( string address, byte[] value ) => WriteTag( address, AllenBradleyHelper.CIP_Type_D1, value, HslHelper.IsAddressEndWithIndex( address ) ? value.Length : 1 );
		
		/// <summary>
		/// 使用指定的类型写入指定的节点数据，类型信息参考API文档，地址支持协议类型代号信息，例如 "type=0xD1;A"<br />
		/// Use the specified type to write the specified node data. For type information, refer to the API documentation. The address supports protocol type code information, such as "type=0xD1;A"
		/// </summary>
		/// <remarks>
		/// 关于参数 length 的含义，表示的是地址长度，一般的标量数据都是 1，如果PLC有个标签是 A，数据类型为 byte[10]，那我们写入 3 个byte就是 WriteTag( "A[5]", 0xD1, new byte[]{1,2,3}, 3 );<br />
		/// Regarding the meaning of the parameter length, it represents the address length. The general scalar data is 1. If the PLC has a tag of A and the data type is byte[10], then we write 3 bytes as WriteTag( "A[5 ]", 0xD1, new byte[]{1,2,3}, 3 );
		/// </remarks>
		/// <param name="address">节点的名称 -> Name of the node </param>
		/// <param name="typeCode">类型代码，详细参见<see cref="AllenBradleyHelper"/>上的常用字段 ->  Type code, see the commonly used Fields section on the <see cref= "AllenBradleyHelper"/> in detail</param>
		/// <param name="value">实际的数据值 -> The actual data value </param>
		/// <param name="length">如果节点是数组，就是数组长度 -> If the node is an array, it is the array length </param>
		/// <returns>是否写入成功 -> Whether to write successfully</returns>
		public virtual OperateResult WriteTag( string address, ushort typeCode, byte[] value, int length = 1 )
		{
			typeCode = (ushort)HslHelper.ExtractParameter( ref address, "type", typeCode );

			OperateResult<byte[]> command = BuildWriteCommand( address, typeCode, value, length );
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> read = ReadFromCoreServer( command.Content );
			if (!read.IsSuccess) return read;

			OperateResult check = CheckResponse( read.Content );
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

			OperateResult check = CheckResponse( read.Content );
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( check );

			return AllenBradleyHelper.ExtractActualData( read.Content, false );
		}
#endif
		#endregion

		#region Write Override

		/// <inheritdoc/>
		[HslMqttApi( "WriteInt16Array", "" )]
		public override OperateResult Write( string address, short[] values ) => WriteTag( address, AllenBradleyHelper.CIP_Type_Word, ByteTransform.TransByte( values ), GetWriteValueLength( address, values.Length ) );

		/// <inheritdoc/>
		[HslMqttApi( "WriteUInt16Array", "" )]
		public override OperateResult Write( string address, ushort[] values ) => WriteTag( address, AllenBradleyHelper.CIP_Type_UInt, ByteTransform.TransByte( values ), GetWriteValueLength( address, values.Length ) );

		/// <inheritdoc/>
		[HslMqttApi( "WriteInt32Array", "" )]
		public override OperateResult Write( string address, int[] values ) => WriteTag( address, AllenBradleyHelper.CIP_Type_DWord, ByteTransform.TransByte( values ), GetWriteValueLength( address, values.Length ) );

		/// <inheritdoc/>
		[HslMqttApi( "WriteUInt32Array", "" )]
		public override OperateResult Write( string address, uint[] values ) => WriteTag( address, AllenBradleyHelper.CIP_Type_UDint, ByteTransform.TransByte( values ), GetWriteValueLength( address, values.Length ) );

		/// <inheritdoc/>
		[HslMqttApi( "WriteFloatArray", "" )]
		public override OperateResult Write( string address, float[] values ) => WriteTag( address, AllenBradleyHelper.CIP_Type_Real, ByteTransform.TransByte( values ), GetWriteValueLength( address, values.Length ) );

		/// <inheritdoc/>
		[HslMqttApi( "WriteInt64Array", "" )]
		public override OperateResult Write( string address, long[] values ) => WriteTag( address, AllenBradleyHelper.CIP_Type_LInt, ByteTransform.TransByte( values ), GetWriteValueLength( address, values.Length ) );

		/// <inheritdoc/>
		[HslMqttApi( "WriteUInt64Array", "" )]
		public override OperateResult Write( string address, ulong[] values ) => WriteTag( address, AllenBradleyHelper.CIP_Type_ULint, ByteTransform.TransByte( values ), GetWriteValueLength( address, values.Length ) );

		/// <inheritdoc/>
		[HslMqttApi( "WriteDoubleArray", "" )]
		public override OperateResult Write( string address, double[] values ) => WriteTag( address, AllenBradleyHelper.CIP_Type_Double, ByteTransform.TransByte( values ), GetWriteValueLength( address, values.Length ) );

		/// <inheritdoc/>
		public override OperateResult Write( string address, string value, Encoding encoding )
		{
			if (string.IsNullOrEmpty( value )) value = string.Empty;

			byte[] data = encoding.GetBytes( value );
			OperateResult write = Write( $"{address}.LEN", data.Length );
			if (!write.IsSuccess) return write;

			byte[] buffer = SoftBasic.ArrayExpandToLengthEven( data );
			return WriteTag( $"{address}.DATA[0]", AllenBradleyHelper.CIP_Type_Byte, buffer, data.Length );
		}

		/// <summary>
		/// 写入单个Bool的数据信息。如果读取的是单bool变量，就直接写变量名，如果是bool数组的一个值，一律带下标访问，例如a[0]<br />
		/// Write the data information of a single Bool. If the read is a single bool variable, write the variable name directly, 
		/// if it is a value of the bool array, it will always be accessed with a subscript, such as a[0]
		/// </summary>
		/// <remarks>
		/// 如果写入的是类型代号 0xC1 的bool变量或是数组，直接使用标签名即可，比如：A,A[10]，如果写入的是类型代号0xD3的bool数组的值，则需要使用地址"i="开头，例如：i=A[10]<br />
		/// If you write a bool variable or array of type code 0xC1, you can use the tag name directly, such as: A,A[10], 
		/// if you write the value of a bool array of type code 0xD3, you need to use the address" i=" at the beginning, for example: i=A[10]
		/// </remarks>
		/// <param name="address">标签的地址数据</param>
		/// <param name="value">bool数据值</param>
		/// <returns>是否写入成功</returns>
		[HslMqttApi( "WriteBool", "" )]
		public override OperateResult Write( string address, bool value )
		{
			if (address.StartsWith( "i=" ) && Regex.IsMatch( address, @"\[[0-9]+\]$" ))
			{
				OperateResult<byte[]> command = BuildWriteCommand( address.Substring( 2 ), value );
				if (!command.IsSuccess) return command;

				OperateResult<byte[]> read = ReadFromCoreServer( command.Content );
				if (!read.IsSuccess) return read;

				OperateResult check = CheckResponse( read.Content );
				if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( check );

				return AllenBradleyHelper.ExtractActualData( read.Content, false );
			}
			else
			{
				return WriteTag( address, AllenBradleyHelper.CIP_Type_Bool, value ? new byte[] { 0xFF, 0xFF } : new byte[] { 0x00, 0x00 } );
			}
		}

		/// <inheritdoc cref="IReadWriteNet.Write(string, bool[])"/>
		[HslMqttApi( "WriteBoolArray", "" )]
		public override OperateResult Write( string address, bool[] value )
		{
			return WriteTag( address, AllenBradleyHelper.CIP_Type_Bool, value.Select( m => m ? (byte)0x01 : (byte)0x00 ).ToArray( ), HslHelper.IsAddressEndWithIndex( address ) ? value.Length : 1 );
		}

		/// <summary>
		/// 写入Byte数据，返回是否写入成功，默认使用类型 0xC2, 如果PLC的变量类型不一样，则需要指定实际的变量类型，例如PLC的变量 A 是0xD1类型，那么地址需要携带类型信息，type=0xD1;A <br />
		/// Write Byte data and return whether the writing is successful. The default type is 0xC2. If the variable types of the PLC are different, you need to specify the actual variable type. 
		/// For example, the variable A of the PLC is of type 0xD1, then the address needs to carry the type information, type= 0xD1;A
		/// </summary>
		/// <remarks>
		/// 如何确认PLC的变量的类型呢？可以在HslCommunicationDemo程序上测试知道，也可以直接调用 <see cref="ReadWithType(string[], int[])"/> 来知道类型信息。
		/// </remarks>
		/// <param name="address">标签的地址数据</param>
		/// <param name="value">Byte数据</param>
		/// <returns>是否写入成功</returns>
		[HslMqttApi( "WriteByte", "" )]
		public virtual OperateResult Write( string address, byte value ) => WriteTag( address, AllenBradleyHelper.CIP_Type_Byte, new byte[] { value, 0x00 } );

		#endregion

		#region Async Write Override
#if !NET35 && !NET20
		/// <inheritdoc cref="Write(string, short[])"/>
		public override async Task<OperateResult> WriteAsync( string address, short[] values ) => await WriteTagAsync( address, AllenBradleyHelper.CIP_Type_Word, ByteTransform.TransByte( values ), GetWriteValueLength( address, values.Length ) );

		/// <inheritdoc cref="Write(string, ushort[])"/>
		public override async Task<OperateResult> WriteAsync( string address, ushort[] values ) => await WriteTagAsync( address, AllenBradleyHelper.CIP_Type_UInt, ByteTransform.TransByte( values ), GetWriteValueLength( address, values.Length ) );

		/// <inheritdoc cref="Write(string, int[])"/>
		public override async Task<OperateResult> WriteAsync( string address, int[] values ) => await WriteTagAsync( address, AllenBradleyHelper.CIP_Type_DWord, ByteTransform.TransByte( values ), GetWriteValueLength( address, values.Length ) );

		/// <inheritdoc cref="Write(string, uint[])"/>
		public override async Task<OperateResult> WriteAsync( string address, uint[] values ) => await WriteTagAsync( address, AllenBradleyHelper.CIP_Type_UDint, ByteTransform.TransByte( values ), GetWriteValueLength( address, values.Length ) );

		/// <inheritdoc cref="Write(string, float[])"/>
		public override async Task<OperateResult> WriteAsync( string address, float[] values ) => await WriteTagAsync( address, AllenBradleyHelper.CIP_Type_Real, ByteTransform.TransByte( values ), GetWriteValueLength( address, values.Length ) );

		/// <inheritdoc cref="Write(string, long[])"/>
		public override async Task<OperateResult> WriteAsync( string address, long[] values ) => await WriteTagAsync( address, AllenBradleyHelper.CIP_Type_LInt, ByteTransform.TransByte( values ), GetWriteValueLength( address, values.Length ) );

		/// <inheritdoc cref="Write(string, ulong[])"/>
		public override async Task<OperateResult> WriteAsync( string address, ulong[] values ) => await WriteTagAsync( address, AllenBradleyHelper.CIP_Type_ULint, ByteTransform.TransByte( values ), GetWriteValueLength( address, values.Length ) );

		/// <inheritdoc cref="Write(string, double[])"/>
		public override async Task<OperateResult> WriteAsync( string address, double[] values ) => await WriteTagAsync( address, AllenBradleyHelper.CIP_Type_Double, ByteTransform.TransByte( values ), GetWriteValueLength( address, values.Length ) );

		/// <inheritdoc/>
		public override async Task<OperateResult> WriteAsync( string address, string value, Encoding encoding )
		{
			if (string.IsNullOrEmpty( value )) value = string.Empty;

			byte[] data = encoding.GetBytes( value );
			OperateResult write = await WriteAsync( $"{address}.LEN", data.Length );
			if (!write.IsSuccess) return write;

			byte[] buffer = SoftBasic.ArrayExpandToLengthEven( data );
			return await WriteTagAsync( $"{address}.DATA[0]", AllenBradleyHelper.CIP_Type_Byte, buffer, data.Length );
		}

		/// <inheritdoc cref="Write(string, bool)"/>
		public override async Task<OperateResult> WriteAsync( string address, bool value )
		{
			if (address.StartsWith( "i=" ) && Regex.IsMatch( address, @"\[[0-9]+\]$" ))
			{
				OperateResult<byte[]> command = BuildWriteCommand( address.Substring( 2 ), value );
				if (!command.IsSuccess) return command;

				OperateResult<byte[]> read = await ReadFromCoreServerAsync( command.Content );
				if (!read.IsSuccess) return read;

				OperateResult check = CheckResponse( read.Content );
				if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( check );

				return AllenBradleyHelper.ExtractActualData( read.Content, false );
			}
			else
			{
				return await WriteTagAsync( address, AllenBradleyHelper.CIP_Type_Bool, value ? new byte[] { 0xFF, 0xFF } : new byte[] { 0x00, 0x00 } );
			}
		}

		/// <inheritdoc cref="Write(string, bool[])"/>
		public override async Task<OperateResult> WriteAsync( string address, bool[] value )
		{
			return await WriteTagAsync( address, AllenBradleyHelper.CIP_Type_Bool, value.Select( m => m ? (byte)0x01 : (byte)0x00 ).ToArray( ), HslHelper.IsAddressEndWithIndex( address ) ? value.Length : 1 );
		}

		/// <inheritdoc cref="Write(string, byte)"/>
		public virtual async Task<OperateResult> WriteAsync( string address, byte value ) => await WriteTagAsync( address, AllenBradleyHelper.CIP_Type_Byte, new byte[] { value, 0x00 } );
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
		public async Task<OperateResult> WriteTimeAndDateAsync( string address, DateTime date ) => await AllenBradleyHelper.WriteTimeAndDateAsync(this, address, date );

		/// <inheritdoc cref="ReadTime(string)"/>
		public async Task<OperateResult<TimeSpan>> ReadTimeAsync( string address ) => await AllenBradleyHelper.ReadTimeAsync( this, address );

		/// <inheritdoc cref="WriteTime(string, TimeSpan)"/>
		public async Task<OperateResult> WriteTimeAsync( string address, TimeSpan time ) => await AllenBradleyHelper.WriteTimeAsync( this, address, time );

		/// <inheritdoc cref="WriteTimeOfDate(string, TimeSpan)"/>
		public async Task<OperateResult> WriteTimeOfDateAsync( string address, TimeSpan timeOfDate ) => await AllenBradleyHelper.WriteTimeOfDateAsync( this, address, timeOfDate );
#endif
		#endregion

		#region PackCommandService

		/// <inheritdoc cref="AllenBradleyHelper.PackCommandService(byte[], byte[][])"/>
		protected virtual byte[] PackCommandService( byte[] portSlot, params byte[][] cips )
		{
			if (this.MessageRouter != null) portSlot = this.MessageRouter.GetRouter( );
			return AllenBradleyHelper.PackCommandService( portSlot, cips );
		}

		/// <summary>
		/// 获取写入数据的长度信息，此处直接返回数组的长度信息
		/// </summary>
		/// <param name="address">地址信息</param>
		/// <param name="length">数组长度信息</param>
		/// <returns>实际的写入长度信息</returns>
		protected virtual int GetWriteValueLength( string address, int length ) => length;

		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString( ) => $"AllenBradleyNet[{IpAddress}:{Port}]";

		#endregion
	}
}
