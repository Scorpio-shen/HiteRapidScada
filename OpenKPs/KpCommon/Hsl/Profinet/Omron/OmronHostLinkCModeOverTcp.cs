using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HslCommunication.Core;
using HslCommunication.Reflection;
using HslCommunication.BasicFramework;
using System.Net.Sockets;
using HslCommunication.Core.IMessage;
#if !NET20 && !NET35
using System.Threading.Tasks;
#endif
using HslCommunication.Core.Net;

namespace HslCommunication.Profinet.Omron
{
	/// <summary>
	/// 欧姆龙的HostLink的C-Mode实现形式，当前的类是通过以太网透传实现。地址支持携带站号信息，例如：s=2;D100<br />
	/// The C-Mode implementation form of Omron’s HostLink, the current class is realized through Ethernet transparent transmission. 
	/// Address supports carrying station number information, for example: s=2;D100
	/// </summary>
	/// <remarks>
	/// 暂时只支持的字数据的读写操作，不支持位的读写操作。另外本模式下，程序要在监视模式运行才能写数据，欧姆龙官方回复的。
	/// </remarks>
	public class OmronHostLinkCModeOverTcp : NetworkDeviceBase
	{
		#region Constructor

		/// <inheritdoc cref="OmronFinsNet()"/>
		public OmronHostLinkCModeOverTcp( )
		{
			this.ByteTransform                         = new ReverseWordTransform( );
			this.WordLength                            = 1;
			this.ByteTransform.DataFormat              = DataFormat.CDAB;
			this.ByteTransform.IsStringReverseByteWord = true;
			this.LogMsgFormatBinary                    = false;
			//this.SleepTime                             = 20;
		}

		/// <inheritdoc cref="OmronCipNet(string,int)"/>
		public OmronHostLinkCModeOverTcp( string ipAddress, int port ) : this( )
		{
			this.IpAddress = ipAddress;
			this.Port      = port;
		}

		/// <inheritdoc/>
		protected override INetMessage GetNewNetMessage( ) => new SpecifiedCharacterMessage( AsciiControl.CR );

		#endregion

		#region Public Member

		/// <inheritdoc cref="OmronHostLinkOverTcp.UnitNumber"/>
		public byte UnitNumber { get; set; }

		#endregion

		#region Override

		///// <inheritdoc/>
		//protected override OperateResult<byte[]> ReceiveByMessage( Socket socket, int timeOut, INetMessage netMessage, Action<long, long> reportProgress = null )
		//{
		//	System.IO.MemoryStream ms = new System.IO.MemoryStream( );
		//	while (true)
		//	{
		//		OperateResult<byte[]> read = ReceiveCommandLineFromSocket( socket, 0x0D, this.receiveTimeOut );
		//		if (!read.IsSuccess) return read;

		//		if(read.Content.Length > 1)
		//		{
		//			if (read.Content[read.Content.Length - 1] == 0x0D && read.Content[read.Content.Length - 1] == '*') // 接收完毕
		//			{
		//				if (read.Content[0] == '@') ms.Write( read.Content, 7, read.Content.Length - 11 );
		//				else ms.Write( read.Content, 0, read.Content.Length - 4 );
		//				return OperateResult.CreateSuccessResult( ms.ToArray( ) );
		//			}
		//			else // 还没接收完毕
		//			{
		//				if (read.Content[0] == '@') ms.Write( read.Content, 7, read.Content.Length - 10 );
		//				else ms.Write( read.Content, 0, read.Content.Length - 3 );

		//				// 回发一个 Delimiter
		//				OperateResult send = Send( socket, new byte[] { 0x0D } );
		//				if (!send.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( send );
		//			}
		//		}
		//		else
		//		{
		//			return new OperateResult<byte[]>( StringResources.Language.ReceiveDataLengthTooShort + " Data: " + read.Content.ToHexString( ' ' ) );
		//		}
		//	}
		//}

		#endregion

		#region Read Write Support

		/// <inheritdoc cref="Helper.OmronHostLinkCModeHelper.Read(IReadWriteDevice, byte, string, ushort)"/>
		[HslMqttApi( "ReadByteArray", "" )]
		public override OperateResult<byte[]> Read( string address, ushort length ) => Helper.OmronHostLinkCModeHelper.Read( this, this.UnitNumber, address, length );

		/// <inheritdoc cref="Helper.OmronHostLinkCModeHelper.Write(IReadWriteDevice, byte, string, byte[])"/>
		[HslMqttApi( "WriteByteArray", "" )]
		public override OperateResult Write( string address, byte[] value ) => Helper.OmronHostLinkCModeHelper.Write( this, this.UnitNumber, address, value );

#if !NET20 && !NET35

		/// <inheritdoc cref="Read(string, ushort)"/>
		public async override Task<OperateResult<byte[]>> ReadAsync( string address, ushort length ) => await Helper.OmronHostLinkCModeHelper.ReadAsync( this, this.UnitNumber, address, length );

		/// <inheritdoc cref="Write(string, byte[])"/>
		public async override Task<OperateResult> WriteAsync( string address, byte[] value ) => await Helper.OmronHostLinkCModeHelper.WriteAsync( this, this.UnitNumber, address, value );
#endif
		#endregion

		#region Bool Read Write


		#endregion

		#region Public Method

		/// <inheritdoc cref="Helper.OmronHostLinkCModeHelper.ReadPlcType(IReadWriteDevice, byte)"/>
		[HslMqttApi( "读取PLC的当前的型号信息" )]
		public OperateResult<string> ReadPlcType( ) => ReadPlcType( this.UnitNumber );

		/// <inheritdoc cref="Helper.OmronHostLinkCModeHelper.ReadPlcType(IReadWriteDevice, byte)"/>
		public OperateResult<string> ReadPlcType( byte unitNumber ) => Helper.OmronHostLinkCModeHelper.ReadPlcType( this, unitNumber );

		/// <inheritdoc cref="Helper.OmronHostLinkCModeHelper.ReadPlcMode(IReadWriteDevice, byte)"/>
		[HslMqttApi( "读取PLC当前的操作模式，0: 编程模式  1: 运行模式  2: 监视模式" )]
		public OperateResult<int> ReadPlcMode( ) => ReadPlcMode( this.UnitNumber );

		/// <inheritdoc cref="Helper.OmronHostLinkCModeHelper.ReadPlcMode(IReadWriteDevice, byte)"/>
		public OperateResult<int> ReadPlcMode( byte unitNumber ) => Helper.OmronHostLinkCModeHelper.ReadPlcMode( this, unitNumber );

		/// <inheritdoc cref="Helper.OmronHostLinkCModeHelper.ChangePlcMode(IReadWriteDevice, byte, byte)"/>
		[HslMqttApi( "将当前PLC的模式变更为指定的模式，0: 编程模式  1: 运行模式  2: 监视模式" )]
		public OperateResult ChangePlcMode( byte mode ) => ChangePlcMode( this.UnitNumber, mode );

		/// <inheritdoc cref="Helper.OmronHostLinkCModeHelper.ChangePlcMode(IReadWriteDevice, byte, byte)"/>
		public OperateResult ChangePlcMode( byte unitNumber, byte mode ) => Helper.OmronHostLinkCModeHelper.ChangePlcMode( this, unitNumber, mode );

		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString( ) => $"OmronHostLinkCModeOverTcp[{IpAddress}:{Port}]";

		#endregion

	}
}
