using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HslCommunication.BasicFramework;
using HslCommunication.Core;
using HslCommunication.Serial;
using HslCommunication.Reflection;
using System.IO;

namespace HslCommunication.Profinet.Omron
{
	/// <summary>
	/// 欧姆龙的HostLink的C-Mode实现形式，地址支持携带站号信息，例如：s=2;D100<br />
	/// Omron's HostLink C-Mode implementation form, the address supports carrying station number information, for example: s=2;D100
	/// </summary>
	/// <remarks>
	/// 暂时只支持的字数据的读写操作，不支持位的读写操作。另外本模式下，程序要在监视模式运行才能写数据，欧姆龙官方回复的。
	/// </remarks>
	public class OmronHostLinkCMode : SerialDeviceBase
	{
		#region Constructor

		/// <inheritdoc cref="OmronFinsNet()"/>
		public OmronHostLinkCMode( )
		{
			this.ByteTransform                         = new ReverseWordTransform( );
			this.WordLength                            = 1;
			this.ByteTransform.DataFormat              = DataFormat.CDAB;
			this.ByteTransform.IsStringReverseByteWord = true;
			this.LogMsgFormatBinary                    = false;
			this.ReceiveEmptyDataCount                 = 5;
		}

		#endregion

		#region Public Member

		/// <inheritdoc cref="OmronHostLinkOverTcp.UnitNumber"/>
		public byte UnitNumber { get; set; }

		/// <inheritdoc/>
		protected override bool CheckReceiveDataComplete( MemoryStream ms )
		{
			byte[] data = ms.GetBuffer( );
			if (data.Length < 5) return false;
			return data[data.Length - 1] == AsciiControl.CR;
		}

		#endregion

		#region Read Write Support

		/// <inheritdoc cref="OmronFinsNet.Read(string, ushort)"/>
		[HslMqttApi( "ReadByteArray", "" )]
		public override OperateResult<byte[]> Read( string address, ushort length ) => Helper.OmronHostLinkCModeHelper.Read( this, this.UnitNumber, address, length );

		/// <inheritdoc cref="OmronFinsNet.Write(string, byte[])"/>
		[HslMqttApi( "WriteByteArray", "" )]
		public override OperateResult Write( string address, byte[] value ) => Helper.OmronHostLinkCModeHelper.Write( this, this.UnitNumber, address, value );

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
		public override string ToString( ) => $"OmronHostLinkCMode[{PortName}:{BaudRate}]";

		#endregion

	}
}
