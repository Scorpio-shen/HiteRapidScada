using HslCommunication.BasicFramework;
using HslCommunication.Core;
using HslCommunication.Core.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using HslCommunication.Core.IMessage;
using HslCommunication.Core.Address;
using HslCommunication.Reflection;
using HslCommunication.Profinet.Siemens.Helper;
#if !NET35 && !NET20
using System.Threading.Tasks;
#endif

namespace HslCommunication.Profinet.Siemens
{
	/// <summary>
	/// <b>[商业授权]</b> 西门子S7协议的虚拟服务器，支持TCP协议，模拟的是1200的PLC进行通信，在客户端进行操作操作的时候，最好是选择1200的客户端对象进行通信。<br />
	/// <b>[Authorization]</b> The virtual server of Siemens S7 protocol supports TCP protocol. It simulates 1200 PLC for communication. When the client is operating, it is best to select the 1200 client object for communication.
	/// </summary>
	/// <remarks>
	/// 本西门子的虚拟PLC仅限商业授权用户使用，感谢支持。
	/// <note type="important">对于200smartPLC的V区，就是DB1.X，例如，V100=DB1.100</note>
	/// </remarks>
	/// <example>
	/// 地址支持的列表如下：
	/// <list type="table">
	///   <listheader>
	///     <term>地址名称</term>
	///     <term>地址代号</term>
	///     <term>示例</term>
	///     <term>地址进制</term>
	///     <term>字操作</term>
	///     <term>位操作</term>
	///     <term>备注</term>
	///   </listheader>
	///   <item>
	///     <term>中间寄存器</term>
	///     <term>M</term>
	///     <term>M100,M200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>输入寄存器</term>
	///     <term>I</term>
	///     <term>I100,I200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>输出寄存器</term>
	///     <term>Q</term>
	///     <term>Q100,Q200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>DB块寄存器</term>
	///     <term>DB</term>
	///     <term>DB1.100,DB1.200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>V寄存器</term>
	///     <term>V</term>
	///     <term>V100,V200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term>V寄存器本质就是DB块1</term>
	///   </item>
	///   <item>
	///     <term>定时器的值</term>
	///     <term>T</term>
	///     <term>T100,T200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term>未测试通过</term>
	///   </item>
	///   <item>
	///     <term>计数器的值</term>
	///     <term>C</term>
	///     <term>C100,C200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term>未测试通过</term>
	///   </item>
	/// </list>
	/// 你可以很快速并且简单的创建一个虚拟的s7服务器
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\SiemensS7ServerExample.cs" region="UseExample1" title="简单的创建服务器" />
	/// 当然如果需要高级的服务器，指定日志，限制客户端的IP地址，获取客户端发送的信息，在服务器初始化的时候就要参照下面的代码：
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\SiemensS7ServerExample.cs" region="UseExample4" title="定制服务器" />
	/// 服务器创建好之后，我们就可以对服务器进行一些读写的操作了，下面的代码是基础的BCL类型的读写操作。
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\SiemensS7ServerExample.cs" region="ReadWriteExample" title="基础的读写示例" />
	/// 高级的对于byte数组类型的数据进行批量化的读写操作如下：   
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\SiemensS7ServerExample.cs" region="BytesReadWrite" title="字节的读写示例" />
	/// 更高级操作请参见源代码。
	/// </example>
	public class SiemensS7Server : NetworkDataServerBase
	{
		#region Constructor

		/// <summary>
		/// 实例化一个S7协议的服务器，支持I，Q，M，DB1.X, DB2.X, DB3.X 数据区块的读写操作<br />
		/// Instantiate a server with S7 protocol, support I, Q, M, DB1.X data block read and write operations
		/// </summary>
		public SiemensS7Server( )
		{
			// 四个数据池初始化，输入寄存器，输出寄存器，中间寄存器，DB块寄存器
			inputBuffer             = new SoftBuffer( DataPoolLength );
			outputBuffer            = new SoftBuffer( DataPoolLength );
			memeryBuffer            = new SoftBuffer( DataPoolLength );
			countBuffer             = new SoftBuffer( DataPoolLength * 2 );
			timerBuffer             = new SoftBuffer( DataPoolLength * 2 );
			aiBuffer                = new SoftBuffer( DataPoolLength );
			aqBuffer                = new SoftBuffer( DataPoolLength );
			systemBuffer            = new SoftBuffer( DataPoolLength );
			this.systemBuffer.SetBytes( "43 50 55 20 32 32 36 20 43 4E 20 20 20 20 20 20 30 32 30 31".ToHexBytes( ), 0 );

			WordLength              = 2;
			ByteTransform           = new ReverseBytesTransform( );


			dbBlockBuffer           = new Dictionary<int, SoftBuffer>( );
			dbBlockBuffer.Add( 1, new SoftBuffer( DataPoolLength ) );      // DB1
			dbBlockBuffer.Add( 2, new SoftBuffer( DataPoolLength ) );      // DB2
			dbBlockBuffer.Add( 3, new SoftBuffer( DataPoolLength ) );      // DB3
		}

		#endregion

		#region NetworkDataServerBase Override

		/// <summary>
		/// 根据S7格式的地址，获取当前的数据缓存类对象
		/// </summary>
		/// <param name="s7Address">S7格式的数据地址信息</param>
		/// <returns>内存缓存对象</returns>
		private OperateResult<SoftBuffer> GetDataAreaFromS7Address( S7AddressData s7Address )
		{
			switch (s7Address.DataCode)
			{
				case 0x03: return OperateResult.CreateSuccessResult( systemBuffer );
				case 0x81: return OperateResult.CreateSuccessResult( inputBuffer );
				case 0x82: return OperateResult.CreateSuccessResult( outputBuffer );
				case 0x83: return OperateResult.CreateSuccessResult( memeryBuffer );
				case 0x84:
					if (dbBlockBuffer.ContainsKey( s7Address.DbBlock )) return OperateResult.CreateSuccessResult( dbBlockBuffer[s7Address.DbBlock] );
					else return new OperateResult<SoftBuffer>( 0x0a, StringResources.Language.SiemensError000A );
				case 0x1E: return OperateResult.CreateSuccessResult( countBuffer );
				case 0x1F: return OperateResult.CreateSuccessResult( timerBuffer );
				case 0x06: return OperateResult.CreateSuccessResult( aiBuffer );
				case 0x07: return OperateResult.CreateSuccessResult( aqBuffer );
				default: return new OperateResult<SoftBuffer>( 0x06, StringResources.Language.SiemensError0006 );
			}
		}

		/// <inheritdoc cref="SiemensS7Net.Read(string, ushort)"/>
		[HslMqttApi( "ReadByteArray", "" )]
		public override OperateResult<byte[]> Read( string address, ushort length )
		{
			OperateResult<S7AddressData> analysis = S7AddressData.ParseFrom( address, length );
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( analysis );

			OperateResult<SoftBuffer> buffer = GetDataAreaFromS7Address( analysis.Content );
			if(!buffer.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( buffer );

			if (analysis.Content.DataCode == 0x1E || analysis.Content.DataCode == 0x1F)
				return OperateResult.CreateSuccessResult( buffer.Content.GetBytes( analysis.Content.AddressStart * 2, length * 2 ) );
			else
				return OperateResult.CreateSuccessResult( buffer.Content.GetBytes( analysis.Content.AddressStart / 8, length ) );
		}

		/// <inheritdoc cref="SiemensS7Net.Write(string, byte[])"/>
		[HslMqttApi( "WriteByteArray", "" )]
		public override OperateResult Write( string address, byte[] value )
		{
			OperateResult<S7AddressData> analysis = S7AddressData.ParseFrom( address );
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( analysis );

			OperateResult<SoftBuffer> buffer = GetDataAreaFromS7Address( analysis.Content );
			if (!buffer.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( buffer );

			if (analysis.Content.DataCode == 0x1E || analysis.Content.DataCode == 0x1F)
				buffer.Content.SetBytes( value, analysis.Content.AddressStart * 2 );
			else
				buffer.Content.SetBytes( value, analysis.Content.AddressStart / 8 );
			return OperateResult.CreateSuccessResult( );
		}

		#endregion

		#region Byte Read Write Operate

		/// <inheritdoc cref="SiemensS7Net.ReadByte(string)"/>
		[HslMqttApi( "ReadByte", "" )]
		public OperateResult<byte> ReadByte( string address ) => ByteTransformHelper.GetResultFromArray( Read( address, 1 ) );

		/// <inheritdoc cref="SiemensS7Net.Write(string, byte)"/>
		[HslMqttApi( "WriteByte", "" )]
		public OperateResult Write(string address, byte value ) => Write( address, new byte[] { value } );

		#endregion

		#region Bool Read Write Operate

		/// <inheritdoc cref="SiemensS7Net.ReadBool(string)"/>
		[HslMqttApi( "ReadBool", "" )]
		public override OperateResult<bool> ReadBool(string address )
		{
			OperateResult<S7AddressData> analysis = S7AddressData.ParseFrom( address );
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<bool>( analysis );

			OperateResult<SoftBuffer> buffer = GetDataAreaFromS7Address( analysis.Content );
			if (!buffer.IsSuccess) return OperateResult.CreateFailedResult<bool>( buffer );

			return OperateResult.CreateSuccessResult( buffer.Content.GetBool( analysis.Content.AddressStart ) );
		}

		/// <inheritdoc cref="SiemensS7Net.Write(string, bool)"/>
		[HslMqttApi( "WriteBool", "" )]
		public override OperateResult Write(string address, bool value )
		{
			OperateResult<S7AddressData> analysis = S7AddressData.ParseFrom( address );
			if (!analysis.IsSuccess) return analysis;

			OperateResult<SoftBuffer> buffer = GetDataAreaFromS7Address( analysis.Content );
			if (!buffer.IsSuccess) return buffer;

			buffer.Content.SetBool( value, analysis.Content.AddressStart ); 
			return OperateResult.CreateSuccessResult( );
		}

		#endregion

		#region ReadWrite String

		/// <inheritdoc/>
		public override OperateResult Write( string address, string value, Encoding encoding ) => SiemensS7Helper.Write( this, SiemensPLCS.S1200, address, value, encoding );

		/// <inheritdoc cref="SiemensS7Helper.WriteWString(IReadWriteNet, SiemensPLCS, string, string)"/>
		[HslMqttApi( ApiTopic = "WriteWString", Description = "写入unicode编码的字符串，支持中文" )]
		public OperateResult WriteWString( string address, string value ) => SiemensS7Helper.WriteWString( this, SiemensPLCS.S1200, address, value );

		/// <inheritdoc/>
		public override OperateResult<string> ReadString( string address, ushort length, Encoding encoding ) => (length == 0) ? ReadString( address, encoding ) : base.ReadString( address, length, encoding );

		/// <inheritdoc cref="ReadString(string, Encoding)"/>
		[HslMqttApi( "ReadS7String", "读取S7格式的字符串" )]
		public OperateResult<string> ReadString( string address ) => ReadString( address, Encoding.ASCII );

		/// <inheritdoc cref="SiemensS7Helper.ReadString(IReadWriteNet, SiemensPLCS, string, Encoding)"/>
		public OperateResult<string> ReadString( string address, Encoding encoding ) => SiemensS7Helper.ReadString( this, SiemensPLCS.S1200, address, encoding );

		/// <summary>
		/// 读取西门子的地址的字符串信息，这个信息是和西门子绑定在一起，长度随西门子的信息动态变化的<br />
		/// Read the Siemens address string information. This information is bound to Siemens and its length changes dynamically with the Siemens information
		/// </summary>
		/// <param name="address">数据地址，具体的格式需要参照类的说明文档</param>
		/// <returns>带有是否成功的字符串结果类对象</returns>
		[HslMqttApi( "ReadWString", "读取S7格式的双字节字符串" )]
		public OperateResult<string> ReadWString( string address ) => SiemensS7Helper.ReadWString( this, SiemensPLCS.S1200, address );

		#endregion

		#region Async ReadWrite String
#if !NET35 && !NET20

		/// <inheritdoc cref="SiemensS7Helper.Write(IReadWriteNet, SiemensPLCS, string, string, Encoding)"/>
		public override async Task<OperateResult> WriteAsync( string address, string value, Encoding encoding ) => await SiemensS7Helper.WriteAsync( this, SiemensPLCS.S1200, address, value, encoding );

		/// <inheritdoc cref="WriteWString(string, string)"/>
		public async Task<OperateResult> WriteWStringAsync( string address, string value ) => await SiemensS7Helper.WriteWStringAsync( this, SiemensPLCS.S1200, address, value );

		/// <inheritdoc/>
		public async override Task<OperateResult<string>> ReadStringAsync( string address, ushort length, Encoding encoding ) => (length == 0) ? await ReadStringAsync( address, encoding ) : await base.ReadStringAsync( address, length, encoding );

		/// <inheritdoc cref="ReadString(string)"/>
		public async Task<OperateResult<string>> ReadStringAsync( string address ) => await ReadStringAsync( address, Encoding.ASCII );

		/// <inheritdoc cref="ReadString(string, Encoding)"/>
		public async Task<OperateResult<string>> ReadStringAsync( string address, Encoding encoding ) => await SiemensS7Helper.ReadStringAsync( this, SiemensPLCS.S1200, address, encoding );

		/// <inheritdoc cref="ReadWString(string)"/>
		public async Task<OperateResult<string>> ReadWStringAsync( string address ) => await SiemensS7Helper.ReadWStringAsync( this, SiemensPLCS.S1200, address );

#endif
		#endregion

		#region NetServer Override

		/// <inheritdoc/>
		protected override INetMessage GetNewNetMessage( ) => new S7Message( );

		/// <inheritdoc/>
		protected override void ThreadPoolLoginAfterClientCheck( Socket socket, System.Net.IPEndPoint endPoint )
		{
			if (IsNeedShakeHands( ))
			{
				// 接收2次的握手协议
				OperateResult<byte[]> read1 = ReceiveByMessage( socket, 5000, new S7Message( ) );
				if (!read1.IsSuccess) return;

				read1.Content[5] = 0xD0;
				read1.Content[6] = read1.Content[8];
				read1.Content[7] = read1.Content[9];
				read1.Content[8] = 0x00;
				read1.Content[9] = 0x0c;

				OperateResult send1 = Send( socket, read1.Content);
				if (!send1.IsSuccess) return;

				OperateResult<byte[]> read2 = ReceiveByMessage( socket, 5000, new S7Message( ) );
				if (!read2.IsSuccess) return;

				OperateResult send2 = Send( socket, SoftBasic.HexStringToBytes( @"03 00 00 1B 02 f0 80 32 03 00 00 00 00 00 08 00 00 00 00 f0 01 00 01 00 f0 00 f0" ) );
				if (!send2.IsSuccess) return;
			}

			// 开始接收数据信息
			base.ThreadPoolLoginAfterClientCheck( socket, endPoint );
		}

		/// <summary>
		/// 获取是否需要进行握手报文信息
		/// </summary>
		/// <returns>是否需要进行握手操作</returns>
		protected virtual bool IsNeedShakeHands() => true;

		/// <inheritdoc/>
		protected override OperateResult<byte[]> ReadFromCoreServer( AppSession session, byte[] receive )
		{
			byte[] back = null;
			if      (receive[17] == 0x04) back = ReadByMessage( receive );     // 读取数据
			else if (receive[17] == 0x05) back = WriteByMessage( receive );    // 写入数据
			else if (receive[17] == 0x00) back = ReadPlcType( );               // 读取PLC型号信息
			else
			{
				return new OperateResult<byte[]>( StringResources.Language.NotSupportedFunction );
			}

			receive.SelectMiddle( 11, 2 ).CopyTo( back, 11 );    // 复制消息ID到返回的报文里
			return OperateResult.CreateSuccessResult( back );
		}

		private byte[] ReadPlcType( )
		{
			return SoftBasic.HexStringToBytes( "03 00 00 7D 02 F0 80 32 07 00 00 00 01 00 0C 00 60 00 01 12 08 12 84 01 01 00 00 00 00 FF" +
					" 09 00 5C 00 11 00 00 00 1C 00 03 00 01 36 45 53 37 20 32 31 35 2D 31 41 47 34 30 2D 30 58 42 30 20 00 00 00 06 20 20 00 06 36 45 53 37 20" +
					" 32 31 35 2D 31 41 47 34 30 2D 30 58 42 30 20 00 00 00 06 20 20 00 07 36 45 53 37 20 32 31 35 2D 31 41 47 34 30 2D 30 58 42 30 20 00 00 56 04 02 01" );
		}

		/// <summary>
		/// 将读取的结果数据内容进行打包，返回客户端读取
		/// </summary>
		/// <param name="command">接收的命令信息</param>
		/// <param name="content">读取的原始字节信息</param>
		/// <returns>返回客户端的原始报文信息</returns>
		protected virtual byte[] PackReadBack( byte[] command, List<byte> content )
		{
			byte[] back = new byte[21 + content.Count];
			SoftBasic.HexStringToBytes( "03 00 00 1A 02 F0 80 32 03 00 00 00 01 00 02 00 05 00 00 04 01" ).CopyTo( back, 0 );
			back[ 2] = (byte)(back.Length / 256);
			back[ 3] = (byte)(back.Length % 256);
			back[15] = (byte)(content.Count / 256);
			back[16] = (byte)(content.Count % 256);
			back[20] = command[18];
			content.CopyTo( back, 21 );
			return back;
		}

		private byte[] ReadByMessage( byte[] packCommand )
		{
			List<byte> content = new List<byte>( );
			int count = packCommand[18];
			int index = 19;
			for (int i = 0; i < count; i++)
			{
				byte length = packCommand[index + 1];
				byte[] command = packCommand.SelectMiddle( index, length + 2 );
				index += length + 2;

				content.AddRange( ReadByCommand( command ) );
			}

			return PackReadBack( packCommand, content );
		}


		private byte[] ReadByCommand(byte[] command )
		{
			if(command[3] == 0x01)
			{
				// 位读取
				int startIndex = command[9] * 65536 + command[10] * 256 + command[11];
				ushort dbBlock = ByteTransform.TransUInt16( command, 6 );
				ushort length  = ByteTransform.TransUInt16( command, 4 );

				OperateResult<SoftBuffer> buffer = GetDataAreaFromS7Address( new S7AddressData( ) { AddressStart = startIndex, DataCode = command[8], DbBlock = dbBlock, Length = 1 } );
				if (!buffer.IsSuccess) throw new Exception( buffer.Message );

				return PackReadBitCommandBack( buffer.Content.GetBool( startIndex ) );
			}
			else if (command[3] == 0x1E || command[3] == 0x1F)
			{
				// 定时器，计数器读取
				ushort length = ByteTransform.TransUInt16( command, 4 );
				int startIndex = command[9] * 65536 + command[10] * 256 + command[11];

				OperateResult<SoftBuffer> buffer = GetDataAreaFromS7Address( new S7AddressData( ) { AddressStart = startIndex, DataCode = command[8], DbBlock = 0, Length = length } );
				if (!buffer.IsSuccess) throw new Exception( buffer.Message );

				return PackReadCTCommandBack( buffer.Content.GetBytes( startIndex * 2, length * 2 ), command[3] == 0x1E ? 0x03 : 0x05 );
			}
			else
			{
				// 字节读取
				ushort length = ByteTransform.TransUInt16( command, 4 );
				if (command[3] == 0x04) length *= 2;
				ushort dbBlock = ByteTransform.TransUInt16( command, 6 );
				int startIndex = (command[9] * 65536 + command[10] * 256 + command[11]) / 8;

				OperateResult<SoftBuffer> buffer = GetDataAreaFromS7Address( new S7AddressData( ) { AddressStart = startIndex, DataCode = command[8], DbBlock = dbBlock, Length = length } );
				if (!buffer.IsSuccess) return PackReadWordCommandBack( (short)buffer.ErrorCode, null );

				return PackReadWordCommandBack( 0, buffer.Content.GetBytes( startIndex, length ) );
			}
		}

		private byte[] PackReadWordCommandBack( short err, byte[] result )
		{
			if (err > 0)
			{
				byte[] back = new byte[4];
				BitConverter.GetBytes( err ).CopyTo( back, 0 );
				return back;
			}
			else
			{
				byte[] back = new byte[4 + result.Length];
				back[0] = 0xFF;
				back[1] = 0x04;

				ByteTransform.TransByte( (ushort)(result.Length * 8) ).CopyTo( back, 2 );
				result.CopyTo( back, 4 );
				return back;
			}
		}

		private byte[] PackReadCTCommandBack( byte[] result, int dataLength )
		{
			byte[] back = new byte[4 + result.Length * dataLength / 2];
			back[0] = 0xFF;
			back[1] = 0x09;

			ByteTransform.TransByte( (ushort)(back.Length - 4 ) ).CopyTo( back, 2 );
			for (int i = 0; i < result.Length / 2; i++)
			{
				result.SelectMiddle( i * 2, 2 ).CopyTo( back, 4 + dataLength - 2 + i * dataLength );
			}
			return back;
		}

		private byte[] PackReadBitCommandBack( bool value )
		{
			byte[] back = new byte[5];
			back[0] = 0xFF;
			back[1] = 0x03;
			back[2] = 0x00;
			back[3] = 0x01;
			back[4] = (byte)(value ? 0x01 : 0x00);
			return back;
		}

		/// <summary>
		/// 创建返回的报文信息
		/// </summary>
		/// <param name="packCommand">接收到的报文命令</param>
		/// <param name="status">返回的状态信息</param>
		/// <returns>返回的原始字节数组</returns>
		protected virtual byte[] PackWriteBack( byte[] packCommand, byte status )
		{
			byte[] buffer = SoftBasic.HexStringToBytes( "03 00 00 16 02 F0 80 32 03 00 00 00 01 00 02 00 01 00 00 05 01 04" );
			buffer[buffer.Length - 1] = status;
			return buffer;
		}

		private byte[] WriteByMessage( byte[] packCommand )
		{
			// 先判断是否有写入的权利，没有的话，直接返回写入异常
			if (!EnableWrite) return PackWriteBack( packCommand, 0x04 );

			if (packCommand[22] == 0x02 || packCommand[22] == 0x04)
			{
				// 字写入
				ushort dbBlock = ByteTransform.TransUInt16( packCommand, 25 );
				int count      = ByteTransform.TransInt16( packCommand, 23 );
				if (packCommand[22] == 0x04) count *= 2;
				int startIndex = (packCommand[28] * 65536 + packCommand[29] * 256 + packCommand[30]) / 8;
				byte[] data = ByteTransform.TransByte( packCommand, 35, count );

				OperateResult<SoftBuffer> buffer = GetDataAreaFromS7Address( new S7AddressData( ) { DataCode = packCommand[27], DbBlock = dbBlock, Length = 1 } );
				if (!buffer.IsSuccess) return PackWriteBack( packCommand, (byte)buffer.ErrorCode );

				buffer.Content.SetBytes( data, startIndex );
				return PackWriteBack( packCommand, 0xFF );
			}
			else
			{
				// 位写入
				ushort dbBlock = ByteTransform.TransUInt16( packCommand, 25 );
				int startIndex = packCommand[28] * 65536 + packCommand[29] * 256 + packCommand[30];
				bool value = packCommand[35] != 0x00;

				OperateResult<SoftBuffer> buffer = GetDataAreaFromS7Address( new S7AddressData( ) { DataCode = packCommand[27], DbBlock = dbBlock, Length = 1 } );
				if (!buffer.IsSuccess) return PackWriteBack( packCommand, (byte)buffer.ErrorCode );

				buffer.Content.SetBool( value, startIndex );
				return PackWriteBack( packCommand, 0xFF );
			}
		}

		#endregion

		#region DB Block

		/// <summary>
		/// 新增一个独立的DB块数据区，如果这个DB块已经存在，则新增无效。<br />
		/// Add a separate DB block data area, if the DB block already exists, the new one is invalid.
		/// </summary>
		/// <param name="db">DB块ID信息</param>
		public void AddDbBlock( int db )
		{
			if (!dbBlockBuffer.ContainsKey( db ))
				dbBlockBuffer.Add( db, new SoftBuffer( DataPoolLength ) );
		}

		/// <summary>
		/// 移除指定的DB块数据区，如果这个DB块不存在的话，操作无效，本方法无法移除1，2，3的DB块<br />
		/// Remove the specified DB block data area, if the DB block does not exist, the operation is invalid, and this method cannot remove the DB block of 1, 2, 3
		/// </summary>
		/// <param name="db">指定的ID信息</param>
		public void RemoveDbBlock( int db )
		{
			if (db == 1 || db == 2 || db == 3) return;
			if (dbBlockBuffer.ContainsKey( db ))
				dbBlockBuffer.Remove( db );
		}

		#endregion

		#region Data Save Load Override

		/// <inheritdoc/>
		protected override void LoadFromBytes( byte[] content )
		{
			if (content.Length < DataPoolLength * 7) throw new Exception( "File is not correct" );

			inputBuffer.SetBytes(          content, DataPoolLength * 0, 0, DataPoolLength );
			outputBuffer.SetBytes(         content, DataPoolLength * 1, 0, DataPoolLength );
			memeryBuffer.SetBytes(         content, DataPoolLength * 2, 0, DataPoolLength );
			dbBlockBuffer[1].SetBytes(     content, DataPoolLength * 3, 0, DataPoolLength );
			dbBlockBuffer[2].SetBytes(     content, DataPoolLength * 4, 0, DataPoolLength );
			dbBlockBuffer[3].SetBytes(     content, DataPoolLength * 5, 0, DataPoolLength );
			//dbOtherBlockBuffer.SetBytes( content, DataPoolLength * 6, 0, DataPoolLength );

			if (content.Length < DataPoolLength * 11) return;
			countBuffer.SetBytes(          content, DataPoolLength * 7, 0, DataPoolLength * 2 );
			timerBuffer.SetBytes(          content, DataPoolLength * 9, 0, DataPoolLength * 2 );
		}

		/// <inheritdoc/>
		protected override byte[] SaveToBytes( )
		{
			byte[] buffer = new byte[DataPoolLength * 11];
			Array.Copy( inputBuffer.GetBytes( ),           0, buffer, DataPoolLength * 0, DataPoolLength );
			Array.Copy( outputBuffer.GetBytes( ),          0, buffer, DataPoolLength * 1, DataPoolLength );
			Array.Copy( memeryBuffer.GetBytes( ),          0, buffer, DataPoolLength * 2, DataPoolLength );
			Array.Copy( dbBlockBuffer[1].GetBytes( ),      0, buffer, DataPoolLength * 3, DataPoolLength);
			Array.Copy( dbBlockBuffer[2].GetBytes( ),      0, buffer, DataPoolLength * 4, DataPoolLength );
			Array.Copy( dbBlockBuffer[3].GetBytes( ),      0, buffer, DataPoolLength * 5, DataPoolLength );
			//Array.Copy( dbOtherBlockBuffer.GetBytes( ),    0, buffer, DataPoolLength * 6, DataPoolLength );
			Array.Copy( countBuffer.GetBytes( ),           0, buffer, DataPoolLength * 7, DataPoolLength * 2 );
			Array.Copy( timerBuffer.GetBytes( ),           0, buffer, DataPoolLength * 9, DataPoolLength * 2 );

			return buffer;
		}

		#endregion

		#region IDisposable Support

		/// <inheritdoc/>
		protected override void Dispose( bool disposing )
		{
			if (disposing)
			{
				systemBuffer?.Dispose( );
				inputBuffer?.Dispose( );
				outputBuffer?.Dispose( );
				memeryBuffer?.Dispose( );
				countBuffer?.Dispose( );
				timerBuffer?.Dispose( );

				foreach (var item in dbBlockBuffer.Values)
				{
					item?.Dispose( );
				}
				aiBuffer?.Dispose( );
				aqBuffer?.Dispose( );
			}
			base.Dispose( disposing );
		}

		#endregion

		#region Private Member

		private SoftBuffer systemBuffer;                   // 系统的数据池
		private SoftBuffer inputBuffer;                    // 输入数据池
		private SoftBuffer outputBuffer;                   // 输出数据池
		private SoftBuffer memeryBuffer;                   // 内部数据池
		private SoftBuffer countBuffer;                    // 计数器的数据池
		private SoftBuffer timerBuffer;                    // 定时器的数据池
		private SoftBuffer aiBuffer;                       // AI缓存数据池
		private SoftBuffer aqBuffer;                       // AQ缓存数据池
		private Dictionary<int, SoftBuffer> dbBlockBuffer; // DB块的数据信息
		private const int DataPoolLength = 65536;          // 数据的长度

		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString( ) => $"SiemensS7Server[{Port}]";

		#endregion
	}
}
