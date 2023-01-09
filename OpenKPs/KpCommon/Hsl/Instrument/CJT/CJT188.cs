using HslCommunication.BasicFramework;
using HslCommunication.Core;
using HslCommunication.Instrument.CJT.Helper;
using HslCommunication.Reflection;
using HslCommunication.Serial;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
#if !NET20 && !NET35
using System.Threading.Tasks;
#endif

namespace HslCommunication.Instrument.CJT
{
	/// <summary>
	/// 城市建设部的188协议，基于DJ/T188-2004实现的协议
	/// </summary>
	public class CJT188 : SerialDeviceBase, ICjt188
	{
		#region Constructor

		/// <summary>
		/// 指定地址域，密码，操作者代码来实例化一个对象，密码及操作者代码在写入操作的时候进行验证<br />
		/// Specify the address field, password, and operator code to instantiate an object, and the password and operator code are validated during write operations, 
		/// which address field is a 14-character BCD code, for example: 149100007290
		/// </summary>
		/// <param name="station">设备的地址信息，是一个14字符的BCD码</param>
		public CJT188( string station )
		{
			this.ByteTransform = new RegularByteTransform( );
			this.station = station;
			//this.password = string.IsNullOrEmpty( password ) ? "00000000" : password;
			//this.opCode = string.IsNullOrEmpty( opCode ) ? "00000000" : opCode;
			this.ReceiveEmptyDataCount = 5;
		}

		/// <inheritdoc/>
		protected override bool CheckReceiveDataComplete( MemoryStream ms )
		{
			byte[] buffer = ms.ToArray( );
			if (buffer.Length < 11) return false;

			// 判断接收的数据是否完整，即使数据0x68前面包含了无用的字节信息
			int begin = DLT.Helper.DLT645Helper.FindHeadCode68H( buffer );
			if (begin < 0) return false;

			if (buffer[begin + 10] + 13 + begin == buffer.Length && buffer[buffer.Length - 1] == 0x16) return true;
			return base.CheckReceiveDataComplete( ms );
		}

		/// <inheritdoc/>
		public override OperateResult<byte[]> ReadFromCoreServer( byte[] send )
		{
			OperateResult<byte[]> read = base.ReadFromCoreServer( send );
			if (!read.IsSuccess) return read;

			// 自动移除0x68前面的无用的字符信息
			int begin = DLT.Helper.DLT645Helper.FindHeadCode68H( read.Content );
			if (begin > 0) return OperateResult.CreateSuccessResult( read.Content.RemoveBegin( begin ) );
			return read;
		}

		/// <inheritdoc/>
		public override byte[] PackCommandWithHeader( byte[] command )
		{
			if (EnableCodeFE)
				return SoftBasic.SpliceArray( new byte[] { 0xfe, 0xfe }, command );
			return base.PackCommandWithHeader( command );
		}

		#endregion

		#region Public Method

		/// <summary>
		/// 激活设备的命令，只发送数据到设备，不等待设备数据返回<br />
		/// The command to activate the device, only send data to the device, do not wait for the device data to return
		/// </summary>
		/// <returns>是否发送成功</returns>
		public OperateResult ActiveDeveice( ) => ReadFromCoreServer( new byte[] { 0xFE, 0xFE }, hasResponseData: false, usePackAndUnpack: false );

		/// <inheritdoc cref="Helper.CJT188Helper.Read(ICjt188, string, ushort)"/>
		[HslMqttApi( "ReadByteArray", "" )]
		public override OperateResult<byte[]> Read( string address, ushort length ) => Helper.CJT188Helper.Read( this, address, length );

		/// <inheritdoc cref="Helper.CJT188Helper.Write(ICjt188, string, byte[])"/>
		[HslMqttApi( "WriteByteArray", "" )]
		public override OperateResult Write( string address, byte[] value ) => Helper.CJT188Helper.Write( this, address, value );

		/// <inheritdoc/>
		[HslMqttApi( "ReadFloatArray", "" )]
		public override OperateResult<float[]> ReadFloat( string address, ushort length ) => Helper.CJT188Helper.ReadValue( this, address, length, float.Parse );

		/// <inheritdoc/>
		[HslMqttApi( "ReadDoubleArray", "" )]
		public override OperateResult<double[]> ReadDouble( string address, ushort length ) => Helper.CJT188Helper.ReadValue( this, address, length, double.Parse );

		/// <inheritdoc/>
		public override OperateResult<string> ReadString( string address, ushort length, Encoding encoding ) => ByteTransformHelper.GetResultFromArray( ReadStringArray( address ) );

		/// <inheritdoc cref="Helper.CJT188Helper.ReadStringArray(ICjt188, string)"/>
		public OperateResult<string[]> ReadStringArray( string address ) => Helper.CJT188Helper.ReadStringArray( this, address );

#if !NET35 && !NET20

		/// <inheritdoc cref="ReadFloat(string, ushort)"/>
		public async override Task<OperateResult<float[]>> ReadFloatAsync( string address, ushort length ) => await Task.Run( ( ) => ReadFloat( address, length ) );

		/// <inheritdoc cref="ReadDouble(string, ushort)"/>
		public async override Task<OperateResult<double[]>> ReadDoubleAsync( string address, ushort length ) => await Task.Run( ( ) => ReadDouble( address, length ) );

		/// <inheritdoc/>
		public async override Task<OperateResult<string>> ReadStringAsync( string address, ushort length, Encoding encoding ) => await Task.Run( ( ) => ReadString( address, length, encoding ) );
#endif
		/// <inheritdoc cref="Helper.CJT188Helper.ReadAddress(ICjt188)"/>
		public OperateResult<string> ReadAddress( ) => Helper.CJT188Helper.ReadAddress( this );

		/// <inheritdoc cref="CJT188Helper.WriteAddress(ICjt188, string)"/>
		public OperateResult WriteAddress( string address ) => Helper.CJT188Helper.WriteAddress( this, address );

		#endregion

		#region Public Property

		/// <summary>
		/// 仪表的类型
		/// </summary>
		public byte InstrumentType { get; set; }

		/// <inheritdoc cref="DLT.Helper.IDlt645.Station"/>
		public string Station { get => this.station; set => this.station = value; }

		/// <inheritdoc cref="DLT.Helper.IDlt645.EnableCodeFE"/>
		public bool EnableCodeFE { get; set; } = true;

		#endregion

		#region Private Member

		private string station = "1";                  // 地址域信息
		private string password = "00000000";          // 密码
		private string opCode = "00000000";            // 操作者代码

		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString( ) => $"CJT188[{PortName}:{BaudRate}]";

		#endregion
	}
}
