using HslCommunication.Core;
using HslCommunication.Reflection;
using HslCommunication.Serial;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HslCommunication.Core.Address;
using System.Text.RegularExpressions;
#if !NET35 && !NET20
using System.Threading.Tasks;
#endif

namespace HslCommunication.Profinet.Vigor
{
	/// <summary>
	/// 丰炜通信协议的串口通信，支持VS系列，地址支持携带站号，例如 s=2;D100, 字地址支持 D,SD,R,T,C(C200-C255是32位寄存器), 位地址支持X,Y,M,SM,S,TS(定时器触点),TC（定时器线圈）,CS(计数器触点),CC（计数器线圈)<br />
	/// The network port transparent transmission version of Fengwei communication protocol supports VS series, and the address supports carrying station number, 
	/// such as s=2;D100, word address supports D, SD, R, T, C (C200-C255 are 32-bit registers), Bit address supports X, Y, M, SM, S, TS (timer contact), 
	/// TC (timer coil), CS (counter contact), CC (counter coil)
	/// </summary>
	/// <remarks>
	/// 串口默认的参数为 19200波特率，8 - N - 1方式，暂时不支持对字寄存器(D,R)进行读写位操作，感谢随时关注库的更新日志
	/// </remarks>
	public class VigorSerial : SerialDeviceBase
	{
		#region Constructor

		/// <summary>
		/// 实例化一个默认的对象
		/// </summary>
		public VigorSerial( )
		{
			this.ByteTransform         = new RegularByteTransform( );
			this.WordLength            = 1;
			this.ReceiveEmptyDataCount = 2;
		}

		/// <inheritdoc/>
		protected override ushort GetWordLength( string address, int length, int dataTypeLength )
		{
			if ( Regex.IsMatch(address, "^C2[0-9][0-9]$" ))
			{
				int len = length * dataTypeLength * 2 / 4;
				return len == 0 ? (ushort)1 : (ushort)len;
			}
			return base.GetWordLength( address, length, dataTypeLength );
		}

		/// <inheritdoc/>
		protected override bool CheckReceiveDataComplete( MemoryStream ms )
		{
			byte[] buffer = ms.ToArray( );
			return Helper.VigorVsHelper.CheckReceiveDataComplete( buffer, buffer.Length );
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// 获取或设置当前PLC的站号信息
		/// </summary>
		public byte Station { get; set; }

		#endregion

		#region ReadWrite

		/// <inheritdoc cref="Helper.VigorHelper.Read(IReadWriteDevice, byte, string, ushort)"/>
		[HslMqttApi( "ReadByteArray", "" )]
		public override OperateResult<byte[]> Read( string address, ushort length ) => Helper.VigorHelper.Read( this, this.Station, address, length );

		/// <inheritdoc cref="Helper.VigorHelper.Write(IReadWriteDevice, byte, string, byte[])"/>
		[HslMqttApi( "WriteByteArray", "" )]
		public override OperateResult Write( string address, byte[] value ) => Helper.VigorHelper.Write( this, this.Station, address, value );

		/// <inheritdoc cref="Helper.VigorHelper.ReadBool(IReadWriteDevice, byte, string, ushort)"/>
		[HslMqttApi( "ReadBoolArray", "" )]
		public override OperateResult<bool[]> ReadBool( string address, ushort length ) => Helper.VigorHelper.ReadBool( this, this.Station, address, length );

		/// <inheritdoc cref="Helper.VigorHelper.Write(IReadWriteDevice, byte, string, bool[])"/>
		[HslMqttApi( "WriteBoolArray", "" )]
		public override OperateResult Write( string address, bool[] value ) => Helper.VigorHelper.Write( this, this.Station, address, value );

		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString( ) => $"VigorSerial[{PortName}:{BaudRate}]";

		#endregion
	}
}
