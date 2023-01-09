using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HslCommunication.Serial;
using HslCommunication.Core;
using System.Text.RegularExpressions;
using HslCommunication.BasicFramework;
using System.IO;
using HslCommunication.Reflection;
using HslCommunication.Instrument.DLT.Helper;
#if !NET35 && !NET20
using System.Threading.Tasks;
#endif

namespace HslCommunication.Instrument.DLT
{
	/// <summary>
	/// 基于多功能电能表通信协议实现的通讯类，参考的文档是DLT645-2007，主要实现了对电表数据的读取和一些功能方法，
	/// 在点对点模式下，需要在打开串口后调用 <see cref="ReadAddress"/> 方法，数据标识格式为 00-00-00-00，具体参照文档手册。<br />
	/// The communication type based on the communication protocol of the multifunctional electric energy meter. 
	/// The reference document is DLT645-2007, which mainly realizes the reading of the electric meter data and some functional methods. 
	/// In the point-to-point mode, you need to call <see cref="ReadAddress" /> method after opening the serial port.
	/// the data identification format is 00-00-00-00, refer to the documentation manual for details.
	/// </summary>
	/// <remarks>
	/// 如果一对多的模式，地址可以携带地址域访问，例如 "s=2;00-00-00-00"，主要使用 <see cref="ReadDouble(string, ushort)"/> 方法来读取浮点数，
	/// <see cref="SerialDeviceBase.ReadString(string, ushort)"/> 方法来读取字符串
	/// </remarks>
	/// <example>
	/// 具体的地址请参考相关的手册内容，如果没有，可以联系HSL作者或者，下面列举一些常用的地址<br />
	/// 对于电能来说，DI0是结算日的信息，现在的就是写0，上一结算日的就写 01，上12结算日就写 0C
	/// <list type="table">
	///   <listheader>
	///     <term>DI3</term>
	///     <term>DI2</term>
	///     <term>DI1</term>
	///     <term>DI0</term>
	///     <term>地址示例</term>
	///     <term>读取方式</term>
	///     <term>数据项名称</term>
	///     <term>备注</term>
	///   </listheader>
	///   <item>
	///     <term>00</term>
	///     <term>00</term>
	///     <term>00</term>
	///     <term>00</term>
	///     <term>00-00-00-00</term>
	///     <term>ReadDouble</term>
	///     <term>（当前）组合有功总电能(kwh)</term>
	///     <term>00-00-01-00到00-00-3F-00分别是组合有功费率1~63电能</term>
	///   </item>
	///   <item>
	///     <term>00</term>
	///     <term>01</term>
	///     <term>00</term>
	///     <term>00</term>
	///     <term>00-01-00-00</term>
	///     <term>ReadDouble</term>
	///     <term>（当前）正向有功总电能(kwh)</term>
	///     <term>00-01-01-00到00-01-3F-00分别是正向有功费率1~63电能</term>
	///   </item>
	///   <item>
	///     <term>00</term>
	///     <term>02</term>
	///     <term>00</term>
	///     <term>00</term>
	///     <term>00-02-00-00</term>
	///     <term>ReadDouble</term>
	///     <term>（当前）反向有功总电能(kwh)</term>
	///     <term>00-02-01-00到00-02-3F-00分别是反向有功费率1~63电能</term>
	///   </item>
	///   <item>
	///     <term>00</term>
	///     <term>03</term>
	///     <term>00</term>
	///     <term>00</term>
	///     <term>00-03-00-00</term>
	///     <term>ReadDouble</term>
	///     <term>（当前）组合无功总电能(kvarh)</term>
	///     <term>00-03-01-00到00-03-3F-00分别是组合无功费率1~63电能</term>
	///   </item>
	///   <item>
	///     <term>00</term>
	///     <term>09</term>
	///     <term>00</term>
	///     <term>00</term>
	///     <term>00-09-00-00</term>
	///     <term>ReadDouble</term>
	///     <term>（当前）正向视在总电能(kvah)</term>
	///     <term>00-09-01-00到00-09-3F-00分别是正向视在费率1~63电能</term>
	///   </item>
	///   <item>
	///     <term>00</term>
	///     <term>0A</term>
	///     <term>00</term>
	///     <term>00</term>
	///     <term>00-0A-00-00</term>
	///     <term>ReadDouble</term>
	///     <term>（当前）反向视在总电能(kvah)</term>
	///     <term>00-0A-01-00到00-0A-3F-00分别是反向视在费率1~63电能</term>
	///   </item>
	///   <item>
	///     <term>02</term>
	///     <term>01</term>
	///     <term>01</term>
	///     <term>00</term>
	///     <term>02-01-01-00</term>
	///     <term>ReadDouble</term>
	///     <term>A相电压(V)</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>02</term>
	///     <term>01</term>
	///     <term>02</term>
	///     <term>00</term>
	///     <term>02-01-02-00</term>
	///     <term>ReadDouble</term>
	///     <term>B相电压(V)</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>02</term>
	///     <term>01</term>
	///     <term>03</term>
	///     <term>00</term>
	///     <term>02-01-03-00</term>
	///     <term>ReadDouble</term>
	///     <term>C相电压(V)</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>02</term>
	///     <term>02</term>
	///     <term>01</term>
	///     <term>00</term>
	///     <term>02-02-01-00</term>
	///     <term>ReadDouble</term>
	///     <term>A相电流(A)</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>02</term>
	///     <term>02</term>
	///     <term>02</term>
	///     <term>00</term>
	///     <term>02-02-02-00</term>
	///     <term>ReadDouble</term>
	///     <term>B相电流(A)</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>02</term>
	///     <term>02</term>
	///     <term>03</term>
	///     <term>00</term>
	///     <term>02-02-03-00</term>
	///     <term>ReadDouble</term>
	///     <term>C相电流(A)</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>02</term>
	///     <term>03</term>
	///     <term>00</term>
	///     <term>00</term>
	///     <term>02-03-00-00</term>
	///     <term>ReadDouble</term>
	///     <term>瞬时总有功功率(kw)</term>
	///     <term>DI1=1时表示A相，2时表示B相，3时表示C相</term>
	///   </item>
	///   <item>
	///     <term>02</term>
	///     <term>04</term>
	///     <term>00</term>
	///     <term>00</term>
	///     <term>02-04-00-00</term>
	///     <term>ReadDouble</term>
	///     <term>瞬时总无功功率(kvar)</term>
	///     <term>DI1=1时表示A相，2时表示B相，3时表示C相</term>
	///   </item>
	///   <item>
	///     <term>02</term>
	///     <term>05</term>
	///     <term>00</term>
	///     <term>00</term>
	///     <term>02-05-00-00</term>
	///     <term>ReadDouble</term>
	///     <term>瞬时总视在功率(kva)</term>
	///     <term>DI1=1时表示A相，2时表示B相，3时表示C相</term>
	///   </item>
	///   <item>
	///     <term>02</term>
	///     <term>06</term>
	///     <term>00</term>
	///     <term>00</term>
	///     <term>02-06-00-00</term>
	///     <term>ReadDouble</term>
	///     <term>总功率因素</term>
	///     <term>DI1=1时表示A相，2时表示B相，3时表示C相</term>
	///   </item>
	///   <item>
	///     <term>02</term>
	///     <term>07</term>
	///     <term>01</term>
	///     <term>00</term>
	///     <term>02-07-01-00</term>
	///     <term>ReadDouble</term>
	///     <term>A相相角(°)</term>
	///     <term>DI1=1时表示A相，2时表示B相，3时表示C相</term>
	///   </item>
	///   <item>
	///     <term>02</term>
	///     <term>08</term>
	///     <term>01</term>
	///     <term>00</term>
	///     <term>02-08-01-00</term>
	///     <term>ReadDouble</term>
	///     <term>A相电压波形失真度(%)</term>
	///     <term>DI1=1时表示A相，2时表示B相，3时表示C相</term>
	///   </item>
	///   <item>
	///     <term>02</term>
	///     <term>80</term>
	///     <term>00</term>
	///     <term>01</term>
	///     <term>02-80-00-01</term>
	///     <term>ReadDouble</term>
	///     <term>零线电流(A)</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>02</term>
	///     <term>80</term>
	///     <term>00</term>
	///     <term>02</term>
	///     <term>02-80-00-02</term>
	///     <term>ReadDouble</term>
	///     <term>电网频率(HZ)</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>02</term>
	///     <term>80</term>
	///     <term>00</term>
	///     <term>03</term>
	///     <term>02-80-00-03</term>
	///     <term>ReadDouble</term>
	///     <term>一分钟有功总平均功率(kw)</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>02</term>
	///     <term>80</term>
	///     <term>00</term>
	///     <term>04</term>
	///     <term>02-80-00-04</term>
	///     <term>ReadDouble</term>
	///     <term>当前有功需量(kw)</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>02</term>
	///     <term>80</term>
	///     <term>00</term>
	///     <term>05</term>
	///     <term>02-80-00-05</term>
	///     <term>ReadDouble</term>
	///     <term>当前无功需量(kvar)</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>02</term>
	///     <term>80</term>
	///     <term>00</term>
	///     <term>06</term>
	///     <term>02-80-00-06</term>
	///     <term>ReadDouble</term>
	///     <term>当前视在需量(kva)</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>02</term>
	///     <term>80</term>
	///     <term>00</term>
	///     <term>07</term>
	///     <term>02-80-00-07</term>
	///     <term>ReadDouble</term>
	///     <term>表内温度(摄氏度)</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>02</term>
	///     <term>80</term>
	///     <term>00</term>
	///     <term>08</term>
	///     <term>02-80-00-08</term>
	///     <term>ReadDouble</term>
	///     <term>时钟电池电压(V)</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>02</term>
	///     <term>80</term>
	///     <term>00</term>
	///     <term>09</term>
	///     <term>02-80-00-09</term>
	///     <term>ReadDouble</term>
	///     <term>停电抄表电池电压(V)</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>02</term>
	///     <term>80</term>
	///     <term>00</term>
	///     <term>0A</term>
	///     <term>02-80-00-0A</term>
	///     <term>ReadDouble</term>
	///     <term>内部电池工作时间(分钟)</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>04</term>
	///     <term>00</term>
	///     <term>04</term>
	///     <term>03</term>
	///     <term>04-00-04-03</term>
	///     <term>ReadString("04-00-04-03", 32)</term>
	///     <term>资产管理编码</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>04</term>
	///     <term>00</term>
	///     <term>04</term>
	///     <term>0B</term>
	///     <term>04-00-04-0B</term>
	///     <term>ReadString("04-00-04-0B", 10)</term>
	///     <term>电表型号</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>04</term>
	///     <term>00</term>
	///     <term>04</term>
	///     <term>0C</term>
	///     <term>04-00-04-0C</term>
	///     <term>ReadString("04-00-04-0C", 10)</term>
	///     <term>生产日期</term>
	///     <term></term>
	///   </item>
	/// </list>
	/// 直接串口初始化，打开串口，就可以对数据进行读取了，地址如上图所示。
	/// </example>
	public class DLT645 : SerialDeviceBase, IDlt645
	{
		#region Constructor

		/// <summary>
		/// 指定地址域，密码，操作者代码来实例化一个对象，密码及操作者代码在写入操作的时候进行验证<br />
		/// Specify the address field, password, and operator code to instantiate an object, and the password and operator code are validated during write operations, 
		/// which address field is a 12-character BCD code, for example: 149100007290
		/// </summary>
		/// <param name="station">设备的地址信息，是一个12字符的BCD码</param>
		/// <param name="password">密码，写入的时候进行验证的信息</param>
		/// <param name="opCode">操作者代码</param>
		public DLT645( string station, string password = "", string opCode = "" )
		{
			this.ByteTransform         = new RegularByteTransform( );
			this.station               = station;
			this.password              = string.IsNullOrEmpty( password ) ? "00000000" : password;
			this.opCode                = string.IsNullOrEmpty( opCode ) ? "00000000" : opCode;
			this.ReceiveEmptyDataCount = 5;
		}

		/// <inheritdoc/>
		protected override bool CheckReceiveDataComplete( MemoryStream ms )
		{
			byte[] buffer = ms.ToArray( );
			if (buffer.Length < 10) return false;

			// 判断接收的数据是否完整，即使数据0x68前面包含了无用的字节信息
			int begin = Helper.DLT645Helper.FindHeadCode68H( buffer );
			if (begin < 0) return false;

			if (buffer[begin + 9] + 12 + begin == buffer.Length && buffer[buffer.Length - 1] == 0x16) return true;
			return base.CheckReceiveDataComplete( ms );
		}

		/// <inheritdoc/>
		public override OperateResult<byte[]> ReadFromCoreServer( byte[] send )
		{
			OperateResult<byte[]> read = base.ReadFromCoreServer( send );
			if (!read.IsSuccess) return read;

			// 自动移除0x68前面的无用的字符信息
			int begin = Helper.DLT645Helper.FindHeadCode68H( read.Content );
			if (begin > 0) return OperateResult.CreateSuccessResult( read.Content.RemoveBegin( begin ) );
			return read;
		}

		/// <inheritdoc/>
		public override byte[] PackCommandWithHeader( byte[] command )
		{
			if (EnableCodeFE)
				return SoftBasic.SpliceArray( new byte[] { 0xfe, 0xfe, 0xfe, 0xfe }, command );
			return base.PackCommandWithHeader( command );
		}

		#endregion

		#region Public Method

		/// <summary>
		/// 激活设备的命令，只发送数据到设备，不等待设备数据返回<br />
		/// The command to activate the device, only send data to the device, do not wait for the device data to return
		/// </summary>
		/// <returns>是否发送成功</returns>
		public OperateResult ActiveDeveice( ) => ReadFromCoreServer( new byte[] { 0xFE, 0xFE, 0xFE, 0xFE }, hasResponseData: false, usePackAndUnpack: false );

		/// <inheritdoc cref="Helper.DLT645Helper.Read(IDlt645, string, ushort)"/>
		[HslMqttApi( "ReadByteArray", "" )]
		public override OperateResult<byte[]> Read( string address, ushort length ) => Helper.DLT645Helper.Read( this, address, length );

		/// <inheritdoc/>
		[HslMqttApi( "ReadDoubleArray", "" )]
		public override OperateResult<double[]> ReadDouble( string address, ushort length ) => Helper.DLT645Helper.ReadDouble( this, address, length );

		/// <inheritdoc/>
		public override OperateResult<string> ReadString( string address, ushort length, Encoding encoding ) => ByteTransformHelper.GetResultFromArray( ReadStringArray( address ) );

		/// <inheritdoc cref="Helper.DLT645Helper.ReadStringArray(IDlt645, string)"/>
		public OperateResult<string[]> ReadStringArray( string address ) => Helper.DLT645Helper.ReadStringArray( this, address );

#if !NET35 && !NET20
		/// <inheritdoc cref="ReadDouble(string, ushort)"/>
		public async override Task<OperateResult<double[]>> ReadDoubleAsync( string address, ushort length ) => await Task.Run( ( ) => ReadDouble( address, length ) );

		/// <inheritdoc/>
		public async override Task<OperateResult<string>> ReadStringAsync( string address, ushort length, Encoding encoding ) => await Task.Run( ( ) => ReadString( address, length, encoding ) );
#endif
		/// <inheritdoc cref="Helper.DLT645Helper.Write(IDlt645, string, string, string, byte[])"/>
		public override OperateResult Write( string address, byte[] value ) => Helper.DLT645Helper.Write( this, this.password, this.opCode, address, value );

		/// <inheritdoc cref="Helper.DLT645Helper.ReadAddress(IDlt645)"/>
		public OperateResult<string> ReadAddress( ) => Helper.DLT645Helper.ReadAddress( this );

		/// <inheritdoc cref="DLT645Helper.WriteAddress(IDlt645, string)"/>
		public OperateResult WriteAddress( string address ) => Helper.DLT645Helper.WriteAddress( this, address );

		/// <inheritdoc cref="Helper.DLT645Helper.BroadcastTime(IDlt645, DateTime)"/>
		public OperateResult BroadcastTime( DateTime dateTime ) => Helper.DLT645Helper.BroadcastTime( this, dateTime );

		/// <inheritdoc cref="Helper.DLT645Helper.FreezeCommand(IDlt645, string)"/>
		public OperateResult FreezeCommand( string dataArea ) => Helper.DLT645Helper.FreezeCommand( this, dataArea );

		/// <inheritdoc cref="Helper.DLT645Helper.ChangeBaudRate(IDlt645, string)"/>
		public OperateResult ChangeBaudRate( string baudRate ) => Helper.DLT645Helper.ChangeBaudRate( this, baudRate );

		#endregion

		#region Public Property

		/// <inheritdoc cref="IDlt645.Station"/>
		public string Station { get => this.station; set => this.station = value; }

		/// <inheritdoc cref="IDlt645.EnableCodeFE"/>
		public bool EnableCodeFE { get; set; }

		/// <inheritdoc cref="IDlt645.DLTType"/>
		public DLT645Type DLTType { get; } = DLT645Type.DLT2007;

		#endregion

		#region Private Member

		private string station = "1";                  // 地址域信息
		private string password = "00000000";          // 密码
		private string opCode = "00000000";            // 操作者代码

		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString( ) => $"DLT645[{PortName}:{BaudRate}]";

		#endregion

	}
}
