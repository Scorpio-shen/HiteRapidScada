using HslCommunication.BasicFramework;
using HslCommunication.Core;
using HslCommunication.Core.IMessage;
using HslCommunication.Reflection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
#if !NET35 && !NET20
using System.Threading.Tasks;
#endif

namespace HslCommunication.Instrument.DLT
{
	/// <summary>
	/// 698.45协议的串口转网口透传通信类(不是TCP通信)，面向对象的用电信息数据交换协议，使用明文的通信方式。支持读取功率，总功，电压，电流，频率，功率因数等数据。<br />
	/// 698.45 protocol serial port to network port transparent transmission communication (not TCP communication), 
	/// object-oriented power consumption information data exchange protocol, using plaintext communication. Support reading power, 
	/// total power, voltage, current, frequency, power factor and other data.
	/// </summary>
	/// <remarks>
	/// <inheritdoc cref="DLT698" path="remarks"/>
	/// </remarks>
	/// <example>
	/// <inheritdoc cref="DLT698" path="example"/>
	/// /// </example>
	public class DLT698OverTcp : HslCommunication.Core.Net.NetworkDeviceBase
	{

		#region Constructor

		/// <summary>
		/// 指定地址域，密码，操作者代码来实例化一个对象，密码及操作者代码在写入操作的时候进行验证<br />
		/// Specify the address field, password, and operator code to instantiate an object, and the password and operator code are validated during write operations, 
		/// which address field is a 12-character BCD code, for example: 149100007290
		/// </summary>
		/// <param name="station">设备的地址信息，通常是一个12字符的BCD码</param>
		public DLT698OverTcp( string station )
		{
			this.ByteTransform = new ReverseBytesTransform( );
			this.station = station;
		}

		/// <summary>
		/// 通过指定IP地址，端口号，设备站号来初始化一个通信对象信息
		/// </summary>
		/// <param name="ipAddress">IP地址信息</param>
		/// <param name="port">端口号信息</param>
		/// <param name="station">设备站号信息</param>
		public DLT698OverTcp( string ipAddress, int port, string station ) : this( station )
		{
			this.IpAddress = ipAddress;
			this.Port = port;
		}

		/// <inheritdoc/>
		protected override INetMessage GetNewNetMessage( ) => new DLT698Message( );

		/// <inheritdoc/>
		public override byte[] PackCommandWithHeader( byte[] command )
		{
			if (EnableCodeFE)
				return SoftBasic.SpliceArray( new byte[] { 0xfe, 0xfe, 0xfe, 0xfe }, command );
			return base.PackCommandWithHeader( command );
		}

		#endregion

		#region Read Write

		/// <summary>
		/// 根据指定的数据标识来读取相关的原始数据信息，地址标识根据手册来，从高位到地位，例如 00-00-00-00，分割符可以任意特殊字符或是没有分隔符。<br />
		/// Read the relevant original data information according to the specified data identifier. The address identifier is based on the manual, 
		/// from high to position, such as 00-00-00-00. The separator can be any special character or no separator.
		/// </summary>
		/// <remarks>
		/// 地址可以携带地址域信息，例如 "s=2;00-00-00-00" 或是 "s=100000;00-00-02-00"，关于数据域信息，需要查找手册，例如:00-01-00-00 表示： (当前)正向有功总电能
		/// </remarks>
		/// <param name="address">数据标识，具体需要查找手册来对应</param>
		/// <param name="length">数据长度信息</param>
		/// <returns>结果信息</returns>
		[HslMqttApi( "ReadByteArray", "" )]
		public override OperateResult<byte[]> Read( string address, ushort length )
		{
			OperateResult<byte[]> command = DLT698.BuildReadSingleObject( address, this.station );
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> read = ReadFromCoreServer( command.Content );
			if (!read.IsSuccess) return read;

			return DLT698.CheckResponse( read.Content );
		}

		/// <summary>
		/// 根据指定的数据标识来写入相关的原始数据信息，地址标识根据手册来，从高位到地位，例如 00-00-00-00，分割符可以任意特殊字符或是没有分隔符。<br />
		/// Read the relevant original data information according to the specified data identifier. The address identifier is based on the manual, 
		/// from high to position, such as 00-00-00-00. The separator can be any special character or no separator.
		/// </summary>
		/// <remarks>
		/// 地址可以携带地址域信息，例如 "s=2;00-00-00-00" 或是 "s=100000;00-00-02-00"，关于数据域信息，需要查找手册，例如:00-01-00-00 表示： (当前)正向有功总电能<br />
		/// 注意：本命令必须与编程键配合使用
		/// </remarks>
		/// <param name="address">地址信息</param>
		/// <param name="value">写入的数据值</param>
		/// <returns>是否写入成功</returns>
		public override OperateResult Write( string address, byte[] value )
		{
			return base.Write( address, value );
		}

		/// <inheritdoc/>
		[HslMqttApi( "ReadBoolArray", "" )]
		public override OperateResult<bool[]> ReadBool( string address, ushort length )
		{
			OperateResult<string[]> read = ReadStringArray( address );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( read );

			try
			{
				List<bool> array = new List<bool>( );
				for (int i = 0; i < read.Content.Length; i++)
				{
					array.AddRange( read.Content[i].ToStringArray<bool>( ) );
				}
				return OperateResult.CreateSuccessResult( array.ToArray( ) );
			}
			catch (Exception ex)
			{
				return new OperateResult<bool[]>( "bool.Parse failed: " + ex.Message + Environment.NewLine + "Source: " + read.Content.ToArrayString( ) );
			}
		}

#if !NET20 && !NET35

		/// <inheritdoc cref="Read(string, ushort)"/>
		public override async Task<OperateResult<byte[]>> ReadAsync( string address, ushort length )
		{
			OperateResult<byte[]> command = DLT698.BuildReadSingleObject( address, this.station );
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> read = await ReadFromCoreServerAsync( command.Content );
			if (!read.IsSuccess) return read;

			return DLT698.CheckResponse( read.Content );
		}

		/// <inheritdoc cref="ReadBool(string, ushort)"/>
		public override async Task<OperateResult<bool[]>> ReadBoolAsync( string address, ushort length )
		{
			OperateResult<string[]> read = await ReadStringArrayAsync( address );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( read );

			try
			{
				List<bool> array = new List<bool>( );
				for (int i = 0; i < read.Content.Length; i++)
				{
					array.AddRange( read.Content[i].ToStringArray<bool>( ) );
				}
				return OperateResult.CreateSuccessResult( array.ToArray( ) );
			}
			catch (Exception ex)
			{
				return new OperateResult<bool[]>( "bool.Parse failed: " + ex.Message + Environment.NewLine + "Source: " + read.Content.ToArrayString( ) );
			}
		}

#endif

		#endregion

		#region Public Method

		/// <summary>
		/// 根据传入的APDU的命令读取原始的字节数据返回
		/// </summary>
		/// <param name="apdu">apdu报文信息</param>
		/// <returns>原始字节数据信息</returns>
		public OperateResult<byte[]> ReadByApdu( byte[] apdu )
		{
			OperateResult<byte[]> command = DLT698.BuildEntireCommand( 0x43, station, 0x00, apdu );
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> read = ReadFromCoreServer( command.Content );
			if (!read.IsSuccess) return read;

			return DLT698.CheckResponse( read.Content );
		}

		/// <summary>
		/// 激活设备的命令，只发送数据到设备，不等待设备数据返回<br />
		/// The command to activate the device, only send data to the device, do not wait for the device data to return
		/// </summary>
		/// <returns>是否发送成功</returns>
		public OperateResult ActiveDeveice( ) => ReadFromCoreServer( new byte[] { 0xFE, 0xFE, 0xFE, 0xFE }, hasResponseData: false, usePackAndUnpack: false );

		/// <summary>
		/// 读取指定地址的所有的字符串数据信息，一般来说，一个地址只有一个数据，当属性为数组或是结构体的时候，存在多个数据，具体几个数据，需要根据
		/// </summary>
		/// <remarks>
		/// 地址可以携带地址域信息，例如 "s=2;20-00-02-00" 或是 "s=100000;20-00-02-00"，
		/// </remarks>
		/// <param name="address">数据标识，具体需要查找手册来对应</param>
		/// <returns>字符串数组信息</returns>
		public OperateResult<string[]> ReadStringArray( string address )
		{
			OperateResult<byte[]> read = Read( address, 1 );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<string[]>( read );

			int index = 8;
			return OperateResult.CreateSuccessResult( DLT698.ExtraStringsValues( this.ByteTransform, read.Content, ref index ) );
		}

		private OperateResult<T[]> ReadDataAndParse<T>( string address, ushort length, Func<string, T> trans )
		{
			OperateResult<string[]> read = ReadStringArray( address );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<T[]>( read );

			try
			{
				return OperateResult.CreateSuccessResult( read.Content.Take( length ).Select( m => trans( m ) ).ToArray( ) );
			}
			catch (Exception ex)
			{
				return new OperateResult<T[]>( typeof( T ).Name + ".Parse failed: " + ex.Message + Environment.NewLine + "Source: " + read.Content.ToArrayString( ) );
			}
		}

		/// <summary>
		/// 读取设备的通信地址，仅支持点对点通讯的情况，返回地址域数据，例如：149100007290<br />
		/// Read the communication address of the device, only support point-to-point communication, and return the address field data, for example: 149100007290
		/// </summary>
		/// <returns>设备的通信地址</returns>
		public OperateResult<string> ReadAddress( )
		{
			OperateResult<byte[]> build = DLT698.BuildReadSingleObject( "40-01-02-00", "AAAAAAAAAAAA" );
			if (!build.IsSuccess) return OperateResult.CreateFailedResult<string>( build );

			OperateResult<byte[]> read = ReadFromCoreServer( build.Content );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<string>( read );

			OperateResult<byte[]> extra = DLT698.CheckResponse( read.Content );
			if (!extra.IsSuccess) return OperateResult.CreateFailedResult<string>( extra );

			// 85 01 01 40 01 02 00 01 09 06 00 37 03 61 29 92 00 00
			this.station = extra.Content.SelectMiddle( 10, extra.Content[9] ).ToHexString( );
			return OperateResult.CreateSuccessResult( this.station );
		}

		/// <summary>
		/// 写入设备的地址域信息，仅支持点对点通讯的情况，需要指定地址域信息，例如：149100007290<br />
		/// Write the address domain information of the device, only support point-to-point communication, 
		/// you need to specify the address domain information, for example: 149100007290
		/// </summary>
		/// <param name="address">等待写入的地址域</param>
		/// <returns>是否写入成功</returns>
		public OperateResult WriteAddress( string address )
		{
			OperateResult<byte[]> build = DLT698.BuildWriteSingleObject( "40-01-02-00", "AAAAAAAAAAAA", DLT698.CreateStringValueBuffer( address ) );
			if (!build.IsSuccess) return OperateResult.CreateFailedResult<string>( build );

			OperateResult<byte[]> read = ReadFromCoreServer( build.Content );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<string>( read );

			return DLT698.CheckResponse( read.Content );
		}
#if !NET35 && !NET20

		private async Task<OperateResult<T[]>> ReadDataAndParseAsync<T>( string address, ushort length, Func<string, T> trans )
		{
			OperateResult<string[]> read = await ReadStringArrayAsync( address );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<T[]>( read );

			try
			{
				return OperateResult.CreateSuccessResult( read.Content.Take( length ).Select( m => trans( m ) ).ToArray( ) );
			}
			catch (Exception ex)
			{
				return new OperateResult<T[]>( typeof( T ).Name + ".Parse failed: " + ex.Message + Environment.NewLine + "Source: " + read.Content.ToArrayString( ) );
			}
		}

		/// <inheritdoc cref="ReadByApdu(byte[])"/>
		public async Task<OperateResult<byte[]>> ReadByApduAsync( byte[] apdu )
		{
			OperateResult<byte[]> command = DLT698.BuildEntireCommand( 0x43, station, 0x00, apdu );
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> read = await ReadFromCoreServerAsync( command.Content );
			if (!read.IsSuccess) return read;

			return DLT698.CheckResponse( read.Content );
		}

		/// <inheritdoc cref="ActiveDeveice"/>
		public async Task<OperateResult> ActiveDeveiceAsync( ) => await ReadFromCoreServerAsync( new byte[] { 0xFE, 0xFE, 0xFE, 0xFE }, hasResponseData: false, usePackAndUnpack: true );

		/// <inheritdoc cref="ReadStringArray(string)"/>
		public async Task<OperateResult<string[]>> ReadStringArrayAsync( string address )
		{
			OperateResult<byte[]> read = await ReadAsync( address, 1 );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<string[]>( read );

			int index = 8;
			return OperateResult.CreateSuccessResult( DLT698.ExtraStringsValues( this.ByteTransform, read.Content, ref index ) );
		}

		/// <inheritdoc cref="ReadAddress"/>
		public async Task<OperateResult<string>> ReadAddressAsync( )
		{
			OperateResult<byte[]> build = DLT698.BuildReadSingleObject( "40-01-02-00", "AAAAAAAAAAAA" );
			if (!build.IsSuccess) return OperateResult.CreateFailedResult<string>( build );

			OperateResult<byte[]> read = await ReadFromCoreServerAsync( build.Content );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<string>( read );

			OperateResult<byte[]> extra = DLT698.CheckResponse( read.Content );
			if (!extra.IsSuccess) return OperateResult.CreateFailedResult<string>( extra );

			// 85 01 01 40 01 02 00 01 09 06 00 37 03 61 29 92 00 00
			this.station = extra.Content.SelectMiddle( 10, extra.Content[9] ).ToHexString( );
			return OperateResult.CreateSuccessResult( this.station );
		}

		/// <inheritdoc cref="WriteAddress(string)"/>
		public async Task<OperateResult> WriteAddressAsync( string address )
		{
			OperateResult<byte[]> build = DLT698.BuildWriteSingleObject( "40-01-02-00", "AAAAAAAAAAAA", DLT698.CreateStringValueBuffer( address ) );
			if (!build.IsSuccess) return OperateResult.CreateFailedResult<string>( build );

			OperateResult<byte[]> read = await ReadFromCoreServerAsync( build.Content );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<string>( read );

			return DLT698.CheckResponse( read.Content );
		}
#endif
		#endregion

		#region Device Read Write

		/// <inheritdoc/>
		[HslMqttApi( "ReadInt16Array", "" )]
		public override OperateResult<short[]> ReadInt16( string address, ushort length ) => ReadDataAndParse( address, length, short.Parse );

		/// <inheritdoc/>
		[HslMqttApi( "ReadUInt16Array", "" )]
		public override OperateResult<ushort[]> ReadUInt16( string address, ushort length ) => ReadDataAndParse( address, length, ushort.Parse );

		/// <inheritdoc/>
		[HslMqttApi( "ReadInt32Array", "" )]
		public override OperateResult<int[]> ReadInt32( string address, ushort length ) => ReadDataAndParse( address, length, int.Parse );

		/// <inheritdoc/>
		[HslMqttApi( "ReadUInt32Array", "" )]
		public override OperateResult<uint[]> ReadUInt32( string address, ushort length ) => ReadDataAndParse( address, length, uint.Parse );

		/// <inheritdoc/>
		[HslMqttApi( "ReadInt64Array", "" )]
		public override OperateResult<long[]> ReadInt64( string address, ushort length ) => ReadDataAndParse( address, length, long.Parse );

		/// <inheritdoc/>
		[HslMqttApi( "ReadUInt64Array", "" )]
		public override OperateResult<ulong[]> ReadUInt64( string address, ushort length ) => ReadDataAndParse( address, length, ulong.Parse );

		/// <inheritdoc/>
		[HslMqttApi( "ReadFloatArray", "" )]
		public override OperateResult<float[]> ReadFloat( string address, ushort length ) => ReadDataAndParse( address, length, float.Parse );

		/// <inheritdoc/>
		[HslMqttApi( "ReadDoubleArray", "" )]
		public override OperateResult<double[]> ReadDouble( string address, ushort length ) => ReadDataAndParse( address, length, double.Parse );

		/// <inheritdoc/>
		public override OperateResult<string> ReadString( string address, ushort length, Encoding encoding ) => ByteTransformHelper.GetResultFromArray( ReadStringArray( address ) );

#if !NET35 && !NET20

		/// <inheritdoc/>
		public async override Task<OperateResult<short[]>> ReadInt16Async( string address, ushort length ) => await ReadDataAndParseAsync( address, length, short.Parse );

		/// <inheritdoc/>
		public async override Task<OperateResult<ushort[]>> ReadUInt16Async( string address, ushort length ) => await ReadDataAndParseAsync( address, length, ushort.Parse );

		/// <inheritdoc/>
		public async override Task<OperateResult<int[]>> ReadInt32Async( string address, ushort length ) => await ReadDataAndParseAsync( address, length, int.Parse );

		/// <inheritdoc/>
		public async override Task<OperateResult<uint[]>> ReadUInt32Async( string address, ushort length ) => await ReadDataAndParseAsync( address, length, uint.Parse );

		/// <inheritdoc/>
		public async override Task<OperateResult<long[]>> ReadInt64Async( string address, ushort length ) => await ReadDataAndParseAsync( address, length, long.Parse );

		/// <inheritdoc/>
		public async override Task<OperateResult<ulong[]>> ReadUInt64Async( string address, ushort length ) => await ReadDataAndParseAsync( address, length, ulong.Parse );

		/// <inheritdoc/>
		public async override Task<OperateResult<float[]>> ReadFloatAsync( string address, ushort length ) => await ReadDataAndParseAsync( address, length, float.Parse );

		/// <inheritdoc cref="ReadDouble(string, ushort)"/>
		public async override Task<OperateResult<double[]>> ReadDoubleAsync( string address, ushort length ) => await ReadDataAndParseAsync( address, length, double.Parse );

		/// <inheritdoc/>
		public async override Task<OperateResult<string>> ReadStringAsync( string address, ushort length, Encoding encoding ) => ByteTransformHelper.GetResultFromArray( await ReadStringArrayAsync( address ) );

#endif
		#endregion

		#region Public Property

		/// <summary>
		/// 获取或设置当前的地址域信息，是一个12个字符的BCD码，例如：149100007290<br />
		/// Get or set the current address domain information, which is a 12-character BCD code, for example: 149100007290
		/// </summary>
		public string Station { get => this.station; set => this.station = value; }

		/// <inheritdoc cref="DLT645.EnableCodeFE"/>
		public bool EnableCodeFE { get; set; }

		#endregion

		#region Private Member

		private string station = "1";                  // 地址域信息
													   //private string password = "00000000";          // 密码
													   //private string opCode = "00000000";            // 操作者代码


		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString( ) => $"DLT698OverTcp[{IpAddress}:{Port}]";

		#endregion
	}
}
