using HslCommunication.BasicFramework;
using HslCommunication.Core;
using HslCommunication.Reflection;
using HslCommunication.Serial;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
#if !NET20 && !NET35
using System.Threading.Tasks;
#endif

namespace HslCommunication.Instrument.DLT
{
	/// <summary>
	/// 698.45协议的串口通信类，面向对象的用电信息数据交换协议，使用明文的通信方式。支持读取功率，总功，电压，电流，频率，功率因数等数据。<br />
	/// The serial communication class of the 698.45 protocol, an object-oriented power consumption information data exchange protocol, 
	/// uses the communication method of clear text. Support reading power, total power, voltage, current, frequency, power factor and other data.
	/// </summary>
	/// <remarks>
	/// 如果不知道表的地址，可以使用<see cref="ReadAddress"/>方法来获取表的地址，读取的数据地址使用 OAD 的标识方式，具体可以参照api文档<br />
	/// If you don't know the address of the table, you can use the <see cref="ReadAddress"/> method to get the address of the table, 
	/// and the read data address uses the OAD identification method. For details, please refer to the api documentation.
	/// </remarks>
	/// <example>
	/// 具体的地址请参考相关的手册内容，如果没有，可以联系HSL作者或者参考下面列举一些常用的地址<br />
	/// 支持的地址即为 OAD 的对象ID信息，该对象需要三个数据标记，分别是<br />
	/// <list type="number">
	/// <item>1. 对象标识 ushort 类型</item>
	/// <item>2. 属性标识 byte 类型, 0:所有属性，1：类型属性，2：值属性，3：单位及倍率</item>
	/// <item>3. 属性内元素索引，00：元素的全部内容，如果是数组或是结构体，01指向属性的第一个元素</item>
	/// </list>
	/// 那么好办了，例如 20-00-02-00 使用 ReadDouble("20-00-02-00", 3) 就是读三个电压，如果只读电压B，那么就是 ReadDouble("20-00-02-02")<br />
	/// 其他的地址参考下面的列表说明
	/// <list type="table">
	///   <listheader>
	///     <term>地址示例</term>
	///     <term>读取方式</term>
	///     <term>数据项名称</term>
	///     <term>备注</term>
	///   </listheader>
	///   <item>
	///     <term>00-00-02-00</term>
	///     <term>ReadDouble</term>
	///     <term>组合有功总电能(kwh)</term>
	///     <term>返回长度5的数组</term>
	///   </item>
	///   <item>
	///     <term>00-10-02-00</term>
	///     <term>ReadDouble</term>
	///     <term>正向有功总电能(kwh)</term>
	///     <term>返回长度5的数组</term>
	///   </item>
	///   <item>
	///     <term>00-20-02-00</term>
	///     <term>ReadDouble</term>
	///     <term>反向有功总电能(kwh)</term>
	///     <term>返回长度5的数组</term>
	///   </item>
	///   <item>
	///     <term>00-30-02-00</term>
	///     <term>ReadDouble</term>
	///     <term>组合无功1总电能(kwh)</term>
	///     <term>返回长度5的数组</term>
	///   </item>
	///   <item>
	///     <term>00-40-02-00</term>
	///     <term>ReadDouble</term>
	///     <term>组合无功2总电能(kwh)</term>
	///     <term>返回长度5的数组</term>
	///   </item>
	///   <item>
	///     <term>10-00-02-00</term>
	///     <term>ReadDouble</term>
	///     <term>当前组合有功总电能(kwh)</term>
	///     <term>返回长度5的数组</term>
	///   </item>
	///   <item>
	///     <term>10-10-02-00</term>
	///     <term>ReadDouble</term>
	///     <term>当前正向有功总电能(kwh)</term>
	///     <term>返回长度5的数组</term>
	///   </item>
	///   <item>
	///     <term>10-20-02-00</term>
	///     <term>ReadDouble</term>
	///     <term>当前反向有功总电能(kwh)</term>
	///     <term>返回长度5的数组</term>
	///   </item>
	///   <item>
	///     <term>10-30-02-00</term>
	///     <term>ReadDouble</term>
	///     <term>当前组合无功1总电能(kwh)</term>
	///     <term>返回长度5的数组</term>
	///   </item>
	///   <item>
	///     <term>10-40-02-00</term>
	///     <term>ReadDouble</term>
	///     <term>当前组合无功2总电能(kwh)</term>
	///     <term>返回长度5的数组</term>
	///   </item>
	///   <item>
	///     <term>20-00-02-00</term>
	///     <term>ReadDouble</term>
	///     <term>电压(v)</term>
	///     <term>电压A,电压B，电压C</term>
	///   </item>
	///   <item>
	///     <term>20-01-02-00</term>
	///     <term>ReadDouble</term>
	///     <term>电流(A)</term>
	///     <term>电流A, 电流B，电流C分别 20-01-02-01 到 20-01-02-03</term>
	///   </item>
	///   <item>
	///     <term>20-02-02-00</term>
	///     <term>ReadDouble</term>
	///     <term>电压相角(度)</term>
	///     <term>相角A,相角B，相角C，分别20-02-02-01 到 20-02-02-03</term>
	///   </item>
	///   <item>
	///     <term>20-03-02-00</term>
	///     <term>ReadDouble</term>
	///     <term>电压电流相角(度)</term>
	///     <term>相角A,相角B，相角C，分别20-03-02-01 到 20-03-02-03</term>
	///   </item>
	///   <item>
	///     <term>20-04-02-00</term>
	///     <term>ReadDouble</term>
	///     <term>有功功率(W 瓦)</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>20-05-02-00</term>
	///     <term>ReadDouble</term>
	///     <term>无功功率(Var)</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>20-06-02-00</term>
	///     <term>ReadDouble</term>
	///     <term>视在功率(VA)</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>20-07-02-00</term>
	///     <term>ReadDouble</term>
	///     <term>一分钟平均有功功率(W)</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>20-08-02-00</term>
	///     <term>ReadDouble</term>
	///     <term>一分钟平均无功功率(var)</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>20-09-02-00</term>
	///     <term>ReadDouble</term>
	///     <term>一分钟视在无功功率(VA)</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>20-0A-02-00</term>
	///     <term>ReadDouble</term>
	///     <term>功率因数</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>20-0F-02-00</term>
	///     <term>ReadDouble</term>
	///     <term>电网频率(Hz)</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>20-10-02-00</term>
	///     <term>ReadDouble</term>
	///     <term>表内温度(摄氏度)</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>20-11-02-00</term>
	///     <term>ReadDouble</term>
	///     <term>时钟电池电压(V)</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>20-12-02-00</term>
	///     <term>ReadDouble</term>
	///     <term>停电抄表电池电压(V)</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>20-13-02-00</term>
	///     <term>ReadDouble</term>
	///     <term>时钟电池工作时间(分钟)</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>20-14-02-00</term>
	///     <term>ReadStringArray</term>
	///     <term>电能表运行状态字</term>
	///     <term>共计7组数据，每组16个位</term>
	///   </item>
	///   <item>
	///     <term>20-15-02-00</term>
	///     <term>ReadStringArray</term>
	///     <term>电能表跟随上报状态字</term>
	///     <term>共计32个位</term>
	///   </item>
	///   <item>
	///     <term>20-17-02-00</term>
	///     <term>ReadDouble</term>
	///     <term>当前有功需量(kw)</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>20-18-02-00</term>
	///     <term>ReadDouble</term>
	///     <term>当前无功需量(kvar)</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>20-19-02-00</term>
	///     <term>ReadDouble</term>
	///     <term>当前视在需量(kva)</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>20-26-02-00</term>
	///     <term>ReadDouble</term>
	///     <term>电压不平衡率(百分比)</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>20-27-02-00</term>
	///     <term>ReadDouble</term>
	///     <term>电流不平衡率(百分比)</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>20-29-02-00</term>
	///     <term>ReadDouble</term>
	///     <term>负载率(百分比)</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>40-00-02-00</term>
	///     <term>ReadString</term>
	///     <term>日期时间</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>40-01-02-00</term>
	///     <term>ReadString</term>
	///     <term>通信地址</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>40-02-02-00</term>
	///     <term>ReadString</term>
	///     <term>表号</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>40-03-02-00</term>
	///     <term>ReadString</term>
	///     <term>客户编号</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>40-04-02-00</term>
	///     <term>ReadString</term>
	///     <term>设备地理坐标</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>41-00-02-00</term>
	///     <term>ReadDouble</term>
	///     <term>最大需量周期(分钟)</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>41-01-02-00</term>
	///     <term>ReadDouble</term>
	///     <term>滑差时间(分钟)</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>41-02-02-00</term>
	///     <term>ReadDouble</term>
	///     <term>校表脉冲宽度(毫秒)</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>41-03-02-00</term>
	///     <term>ReadString</term>
	///     <term>资产管理码</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>41-04-02-00</term>
	///     <term>ReadString</term>
	///     <term>额定电压(V)</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>41-05-02-00</term>
	///     <term>ReadString</term>
	///     <term>额定电流/基本电流</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>41-06-02-00</term>
	///     <term>ReadString</term>
	///     <term>最大电流</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>41-07-02-00</term>
	///     <term>ReadString</term>
	///     <term>有功准确度等级</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>41-08-02-00</term>
	///     <term>ReadString</term>
	///     <term>无功准确度等级</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>41-09-02-00</term>
	///     <term>ReadString</term>
	///     <term>电能表有功常数(imp/kWh)</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>41-0A-02-00</term>
	///     <term>ReadString</term>
	///     <term>电能表无功常数(imp/kWh)</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>41-0B-02-00</term>
	///     <term>ReadString</term>
	///     <term>电能表型号</term>
	///     <term></term>
	///   </item>
	/// </list>
	/// 直接串口初始化，打开串口，就可以对数据进行读取了，地址如上图所示。
	/// </example>
	public class DLT698 : SerialDeviceBase
	{
#region Constructor

		/// <summary>
		/// 指定地址域，密码，操作者代码来实例化一个对象，密码及操作者代码在写入操作的时候进行验证<br />
		/// Specify the address field, password, and operator code to instantiate an object, and the password and operator code are validated during write operations, 
		/// which address field is a 12-character BCD code, for example: 149100007290
		/// </summary>
		/// <param name="station">设备的地址信息，通常是一个12字符的BCD码</param>
		public DLT698( string station )
		{
			this.ByteTransform         = new ReverseBytesTransform( );
			this.station               = station;
			this.ReceiveEmptyDataCount = 5;
		}

		/// <inheritdoc/>
		protected override bool CheckReceiveDataComplete( MemoryStream ms )
		{
			byte[] buffer = ms.ToArray( );
			if (buffer.Length < 10) return false;

			// 判断接收的数据是否完整，即使数据0x68前面包含了无用的字节信息
			int begin = DLT.Helper.DLT645Helper.FindHeadCode68H( buffer );
			if (begin < 0) return false;

			if (BitConverter.ToInt16(buffer, 1 + begin) + 2 + begin == buffer.Length && buffer[buffer.Length - 1] == 0x16) return true;
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
		protected override OperateResult InitializationOnOpen( SerialPort sp )
		{
			//OperateResult<byte[]> read1 = ReadFromCoreServer( sp, BuildEntireCommand( 0x81, this.station, 0x00,  CreateLoginApdu( ) ).Content );
			//if (!read1.IsSuccess) return read1;

			//OperateResult<byte[]> read2 = ReadFromCoreServer( sp, BuildEntireCommand( 0x81, this.station, 0x00, CreateConnectApdu( ) ).Content );
			//if (!read2.IsSuccess) return read2;

			return base.InitializationOnOpen( sp );

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
		/// 根据传入的APDU的命令读取原始的字节数据返回
		/// </summary>
		/// <param name="apdu">apdu报文信息</param>
		/// <returns>原始字节数据信息</returns>
		public OperateResult<byte[]> ReadByApdu( byte[] apdu )
		{
			OperateResult<byte[]> command = BuildEntireCommand( 0x43, station, 0x00, apdu );
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> read = ReadFromCoreServer( command.Content );
			if (!read.IsSuccess) return read;

			return CheckResponse( read.Content );
		}

		/// <summary>
		/// 激活设备的命令，只发送数据到设备，不等待设备数据返回<br />
		/// The command to activate the device, only send data to the device, do not wait for the device data to return
		/// </summary>
		/// <returns>是否发送成功</returns>
		public OperateResult ActiveDeveice( ) => ReadFromCoreServer( new byte[] { 0xFE, 0xFE, 0xFE, 0xFE }, hasResponseData: false, usePackAndUnpack: false );

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
			OperateResult<byte[]> command = BuildReadSingleObject( address, this.station );
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> read = ReadFromCoreServer( command.Content );
			if (!read.IsSuccess) return read;

			return CheckResponse( read.Content );
		}

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
			return OperateResult.CreateSuccessResult( ExtraStringsValues( this.ByteTransform, read.Content, ref index ) );
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
			//OperateResult<string, byte[]> analysis = AnalysisBytesAddress( address, this.station );
			//if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( analysis );

			//byte[] content = SoftBasic.SpliceArray<byte>( analysis.Content2, password.ToHexBytes( ), opCode.ToHexBytes( ), value );

			//OperateResult<byte[]> command = BuildEntireCommand( analysis.Content1, DLTControl.WriteAddress, content );
			//if (!command.IsSuccess) return command;

			//OperateResult<byte[]> read = ReadFromCoreServer( command.Content );
			//if (!read.IsSuccess) return read;

			//return CheckResponse( read.Content );

			return base.Write( address, value );
		}

		/// <summary>
		/// 读取设备的通信地址，仅支持点对点通讯的情况，返回地址域数据，例如：149100007290<br />
		/// Read the communication address of the device, only support point-to-point communication, and return the address field data, for example: 149100007290
		/// </summary>
		/// <returns>设备的通信地址</returns>
		public OperateResult<string> ReadAddress( )
		{
			OperateResult<byte[]> build = BuildReadSingleObject( "40-01-02-00", "AAAAAAAAAAAA" );
			if (!build.IsSuccess) return OperateResult.CreateFailedResult<string>( build );

			OperateResult<byte[]> read = ReadFromCoreServer( build.Content );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<string>( read );

			OperateResult<byte[]> extra = CheckResponse( read.Content );
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
			OperateResult<byte[]> build = BuildWriteSingleObject( "40-01-02-00", "AAAAAAAAAAAA", CreateStringValueBuffer( address) );
			if (!build.IsSuccess) return OperateResult.CreateFailedResult<string>( build );

			OperateResult<byte[]> read = ReadFromCoreServer( build.Content );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<string>( read );

			return CheckResponse( read.Content );
		}

#endregion

#if !NET35 && !NET20

		/// <inheritdoc/>
		public async override Task<OperateResult<short[]>> ReadInt16Async( string address, ushort length ) => await Task.Run( ( ) => ReadInt16( address, length ) );

		/// <inheritdoc/>
		public async override Task<OperateResult<ushort[]>> ReadUInt16Async( string address, ushort length ) => await Task.Run( ( ) => ReadUInt16( address, length ) );

		/// <inheritdoc/>
		public async override Task<OperateResult<int[]>> ReadInt32Async( string address, ushort length ) => await Task.Run( ( ) => ReadInt32( address, length ) );

		/// <inheritdoc/>
		public async override Task<OperateResult<uint[]>> ReadUInt32Async( string address, ushort length ) => await Task.Run( ( ) => ReadUInt32( address, length ) );

		/// <inheritdoc/>
		public async override Task<OperateResult<long[]>> ReadInt64Async( string address, ushort length ) => await Task.Run( ( ) => ReadInt64( address, length ) );

		/// <inheritdoc/>
		public async override Task<OperateResult<ulong[]>> ReadUInt64Async( string address, ushort length ) => await Task.Run( ( ) => ReadUInt64( address, length ) );

		/// <inheritdoc/>
		public async override Task<OperateResult<float[]>> ReadFloatAsync( string address, ushort length ) => await Task.Run( ( ) => ReadFloat( address, length ) );

		/// <inheritdoc cref="ReadDouble(string, ushort)"/>
		public async override Task<OperateResult<double[]>> ReadDoubleAsync( string address, ushort length ) => await Task.Run( ( ) => ReadDouble( address, length ) );

		/// <inheritdoc/>
		public async override Task<OperateResult<string>> ReadStringAsync( string address, ushort length, Encoding encoding )
		{
			return await Task.Run( ( ) => ReadString( address, length, encoding ) );
		}

#endif
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
		public override string ToString( ) => $"DLT698[{PortName}:{BaudRate}]";

#endregion

#region Static Helper

		/// <summary>
		/// 根据地址类型，逻辑地址，实际的地址信息构建出真实的地址报文
		/// </summary>
		/// <param name="addressType">地址类型，0：单地址, 1：通配地址，2：组地址，3：广播地址</param>
		/// <param name="logicAddress">逻辑地址</param>
		/// <param name="address">地址信息</param>
		/// <param name="ca">客户机地址</param>
		/// <returns>原始字节信息</returns>
		private static byte[] CalculateAddressArea( int addressType, int logicAddress, string address, byte ca )
		{
			if (address.Length % 2 == 1) address += "F";
			if (logicAddress > 3) logicAddress = 3;

			byte[] buffer = new byte[2 + address.Length / 2];
			buffer[0] = (byte)((addressType << 6) | (logicAddress << 4) | (address.Length / 2 - 1));
			address.ToHexBytes( ).Reverse( ).ToArray( ).CopyTo( buffer, 1 );
			buffer[buffer.Length - 1] = ca;
			return buffer;
		}

		internal static byte[] CreateStringValueBuffer( string value )
		{
			if (value.Length % 2 == 1) value += "F";
			byte[] data = value.ToHexBytes( );

			byte[] buffer = new byte[data.Length + 2];
			buffer[0] = 0x09;
			buffer[1] = (byte)data.Length;
			data.CopyTo( buffer, 2 );
			return buffer;
		}

		/// <summary>
		/// 将指定的地址信息，控制码信息，数据域信息打包成完整的报文命令
		/// </summary>
		/// <param name="control">控制码信息</param>
		/// <param name="sa">服务器的地址</param>
		/// <param name="ca">客户机地址</param>
		/// <param name="apdu">链路用户数据</param>
		/// <returns>返回是否报文创建成功</returns>
		public static OperateResult<byte[]> BuildEntireCommand( byte control, string sa, byte ca, byte[] apdu )
		{
			int addressType = 0x00;
			if (sa == "AA") addressType = 3;                                   // 广播地址
			else if (sa.Contains( "A" )) addressType = 1;                      // 通配地址

			byte[] dataArea = CalculateAddressArea( addressType, 0, sa, ca );
			int index = 0;

			byte[] buffer = new byte[1 + 2 + 1 + dataArea.Length + 2 + apdu.Length + 2 + 1];
			buffer[index++] = 0x68;                                            // 帧起始符
			buffer[index++] = BitConverter.GetBytes( buffer.Length - 2 )[0];   // 长度信息
			buffer[index++] = BitConverter.GetBytes( buffer.Length - 2 )[1];
			buffer[index++] = control;                                         // 控制域
			dataArea.CopyTo( buffer, index );                                  // 地址域A
			index += dataArea.Length;

			// 帧头校验 HCS
			Helper.DLT698FcsHelper.CalculateFcs16( buffer, 1, index - 1 ).CopyTo( buffer, index );
			index += 2;

			apdu.CopyTo( buffer, index );
			index += apdu.Length;

			// 帧校验 FCS
			Helper.DLT698FcsHelper.CalculateFcs16( buffer, 1, index - 1 ).CopyTo( buffer, index );
			index += 2;
			buffer[index] = 0x16;   // 结束符
			return OperateResult.CreateSuccessResult( buffer );
		}

		/// <summary>
		/// 构建读取单个对象的报文数据
		/// </summary>
		/// <param name="address">数据地址信息</param>
		/// <param name="station">站号信息</param>
		/// <returns>单次读取的报文信息</returns>
		public static OperateResult<byte[]> BuildReadSingleObject( string address, string station )
		{
			// 10 00 08 05 01 01 40 01 02 00 00 01 10 11 22 33 44 55 66 77 88 99 00 AA BB CC DD EE FF
			if (address.IndexOf( ';' ) > 0)
			{
				string[] splits = address.Split( new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries );
				if (splits[0].StartsWith( "s=" )) station = splits[0].Substring( 2 );

				address = splits[1];
			}

			byte[] apdu = new byte[29];
			apdu[ 0] = 0x10;
			apdu[ 1] = 0x00;
			apdu[ 2] = 0x08;
			apdu[ 3] = 0x05;         // GET-Request
			apdu[ 4] = 0x01;         // GET-RequestNormal
			apdu[ 5] = 0x01;         // PIID
			apdu[ 6] = 0x00;         // 通信地址
			apdu[ 7] = 0x00;
			apdu[ 8] = 0x02;
			apdu[ 9] = 0x00;
			apdu[10] = 0x00;         // 没有时间标签
			apdu[11] = 0x01;
			apdu[12] = 0x10;
			apdu[13] = 0x11;
			apdu[14] = 0x22;
			apdu[15] = 0x33;
			apdu[16] = 0x44;
			apdu[17] = 0x55;
			apdu[18] = 0x66;
			apdu[19] = 0x77;
			apdu[20] = 0x88;
			apdu[21] = 0x99;
			apdu[22] = 0x00;
			apdu[23] = 0xAA;
			apdu[24] = 0xBB;
			apdu[25] = 0xCC;
			apdu[26] = 0xDD;
			apdu[27] = 0xEE;
			apdu[28] = 0xFF;
			address.ToHexBytes( ).CopyTo( apdu, 6 );

			return BuildEntireCommand( 0x43, station, 0x00, apdu );
		}

		/// <summary>
		/// 构建单个写得对象的数据操作
		/// </summary>
		/// <param name="address">数据地址信息</param>
		/// <param name="station">站号信息</param>
		/// <param name="data">数据信息</param>
		/// <returns>最终报文</returns>
		public static OperateResult<byte[]> BuildWriteSingleObject( string address, string station, byte[] data )
		{
			if (address.IndexOf( ';' ) > 0)
			{
				string[] splits = address.Split( new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries );
				if (splits[0].StartsWith( "s=" )) station = splits[0].Substring( 2 );

				address = splits[1];
			}

			byte[] apdu = new byte[29 + data.Length];
			apdu[ 0] = 0x10;
			apdu[ 1] = 0x00;
			apdu[ 2] = (byte)(0x08 + data.Length);
			apdu[ 3] = 0x06;         // Set-Request
			apdu[ 4] = 0x01;         // Set-RequestNormal
			apdu[ 5] = 0x02;         // PIID
			apdu[ 6] = 0x00;         // 通信地址
			apdu[ 7] = 0x00;
			apdu[ 8] = 0x02;
			apdu[ 9] = 0x00;
			data.CopyTo( apdu, 10 );
			apdu[10 + data.Length] = 0x00;         // 没有时间标签
			apdu[11 + data.Length] = 0x01;
			apdu[12 + data.Length] = 0x10;
			apdu[13 + data.Length] = 0x11;
			apdu[14 + data.Length] = 0x22;
			apdu[15 + data.Length] = 0x33;
			apdu[16 + data.Length] = 0x44;
			apdu[17 + data.Length] = 0x55;
			apdu[18 + data.Length] = 0x66;
			apdu[19 + data.Length] = 0x77;
			apdu[20 + data.Length] = 0x88;
			apdu[21 + data.Length] = 0x99;
			apdu[22 + data.Length] = 0x00;
			apdu[23 + data.Length] = 0xAA;
			apdu[24 + data.Length] = 0xBB;
			apdu[25 + data.Length] = 0xCC;
			apdu[26 + data.Length] = 0xDD;
			apdu[27 + data.Length] = 0xEE;
			apdu[28 + data.Length] = 0xFF;
			address.ToHexBytes( ).CopyTo( apdu, 6 );

			return BuildEntireCommand( 0x43, station, 0x00, apdu );
		}

		/// <summary>
		/// 检查当前的反馈数据信息是否正确
		/// </summary>
		/// <param name="response">从仪表反馈的数据信息</param>
		/// <returns>是否校验成功</returns>
		public static OperateResult<byte[]> CheckResponse( byte[] response )
		{
			if (response.Length < 9) return new OperateResult<byte[]>( StringResources.Language.ReceiveDataLengthTooShort );
			int index = 1;
			if (BitConverter.ToUInt16( response, index ) != response.Length - 2) return new OperateResult<byte[]>( "Receive length check faild, source: " + response.ToHexString( ' ' ) );
			if (!Helper.DLT698FcsHelper.CheckFcs16(response, 1, response.Length - 4)) return new OperateResult<byte[]>( "fcs 16 check failed: " + response.ToHexString( ' ' ) );
			index = 5 + (response[4] + 1) + 1 + 2;

			byte[] array = null;
			if (response[index] == 0x90)
			{
				index++;
				int length = response[index] * 256 + response[index + 1];
				index += 2;
				array = response.SelectMiddle( index, length );
			}
			else
			{
				array = response.SelectMiddle( index, response.Length - index - 3 );
			}

			if (array[7] == 0x00) return new OperateResult<byte[]>( array[8], GetErrorText( array[8] ) );
			return OperateResult.CreateSuccessResult( array );
		}


		private static string ExtraData( byte[] content, IByteTransform byteTransform, ref int index, byte oi1, byte oi2 )
		{
			byte type = content[index++];
			if (type == 3)
			{
				bool value = content[index++] != 0x00;
				return value.ToString( );
			}
			if (type == 4)
			{
				// bit array
				byte bitLength = content[index++];
				int byteLength = (bitLength + 7) / 8;
				byte[] tmp = content.SelectMiddle( index, byteLength );
				index += byteLength;

				return tmp.ToBoolArray( ).SelectBegin( bitLength ).ToArrayString( );
			}
			else if (type == 5)
			{
				// Int32
				int value = byteTransform.TransInt32( content, index );
				index += 4;
				return GetScale( value, oi1, oi2 );
			}
			else if (type == 6)
			{
				// UInt32
				uint value = byteTransform.TransUInt32( content, index );
				index += 4;
				return GetScale( value, oi1, oi2 );
			}
			else if (type == 9)
			{
				// hex string
				int length = content[index++];
				string tmp = content.SelectMiddle( index, length ).ToHexString( );
				index += length;
				return tmp;
			}
			else if (type == 10)
			{
				int length = content[index++];
				string tmp = Encoding.ASCII.GetString( content, index, length );
				index += length;
				return tmp;
			}
			else if (type == 10)
			{
				int length = content[index++];
				string tmp = Encoding.UTF8.GetString( content, index, length );
				index += length;
				return tmp;
			}
			else if (type == 15)
			{
				return GetScale( (sbyte)content[index++], oi1, oi2 );
			}
			else if (type == 16)
			{
				short value = byteTransform.TransInt16( content, index );
				index += 2;
				return GetScale( value, oi1, oi2 );
			}
			else if (type == 17)
			{
				return GetScale( content[index++], oi1, oi2 );
			}
			else if (type == 18)
			{
				ushort value = byteTransform.TransUInt16( content, index );
				index += 2;
				return GetScale( value, oi1, oi2 );
			}
			else if (type == 20)
			{
				long value = byteTransform.TransInt64( content, index );
				index += 8;
				return GetScale( value, oi1, oi2 );
			}
			else if (type == 21)
			{
				ulong value = byteTransform.TransUInt64( content, index );
				index += 8;
				return GetScale( value, oi1, oi2 );
			}
			else if (type == 22)
			{
				return content[index++].ToString( );
			}
			else if (type == 23)
			{
				float value = byteTransform.TransSingle( content, index );
				index += 4;
				return GetScale( value, oi1, oi2 );
			}
			else if (type == 24)
			{
				double value = byteTransform.TransDouble( content, index );
				index += 8;
				return GetScale( value, oi1, oi2 );
			}
			else if (type == 25)
			{
				// 时间
				ushort year = byteTransform.TransUInt16( content, index );
				index += 2;
				byte month  = content[index++];
				byte day    = content[index++];
				index++; // 星期
				byte hour   = content[index++];
				byte minute = content[index++];
				byte second = content[index++];
				ushort mill = byteTransform.TransUInt16( content, index );
				index += 2;
				return new DateTime( year, month, day, hour, minute, second, mill ).ToString( );
			}
			else if (type == 28)
			{
				// 时间
				ushort year = byteTransform.TransUInt16( content, index );
				index += 2;
				byte month   = content[index++];
				byte day    = content[index++];
				byte hour   = content[index++];
				byte minute = content[index++];
				byte second = content[index++];
				return new DateTime( year, month, day, hour, minute, second ).ToString( );
			}
			else if (type == 26)
			{
				// 时间
				ushort year = byteTransform.TransUInt16( content, index );
				index += 2;
				byte month = content[index++];
				byte day = content[index++];
				index++; // 星期
				return new DateTime( year, month, day ).ToString( );
			}
			else if (type == 27)
			{
				// 时间
				byte hour = content[index++];
				byte minute = content[index++];
				byte second = content[index++];
				return new TimeSpan( hour, minute, second ).ToString( );
			}
			return null;
		}

		private static string GetScale<T>( T value, byte oi1, byte oi2 )
		{
			int scale = 0;
			if ((oi1 & 0xf0) == 0x00) scale = -2;
			else if ((oi1 & 0xf0) == 0x10) scale = -4;
			else if (oi1 == 0x20)
			{
				if (oi2 == 0x00) scale = -1;
				else if (oi2 == 0x01) scale = -3;
				else if (oi2 < 0x0A) scale = -1;
				else if (oi2 == 0x0A) scale = -3;
				else if (oi2 < 0x10) scale = -2;
				else if (oi2 == 0x10) scale = -1;
				else if (oi2 < 0x13) scale = -2;
				else if (oi2 < 0x17) scale = 0;
				else if (oi2 < 0x1E) scale = -4;
				else if (oi2 < 0x26) scale = 0;
				else if (oi2 < 0x2A) scale = -2;
				else if (oi2 == 0x31 || oi2 == 0x32) scale = -2;
			}
			else if (oi1 == 0x25)
			{
				if (oi2 < 0x02) scale = -4;
				else if (oi2 < 0x04) scale = -2;
			}
			else if (oi1 == 0x40)
			{
				if (oi2 == 0x30) scale = -1;
			}
			else if (oi1 == 0x41)
			{
				if (oi2 == 0x0C || oi2 == 0x0D || oi2 == 0x0E || oi2 == 0x0F) scale = -3;
			}

			if (scale == 0) return value.ToString( );
			return (Convert.ToDouble( value ) * Math.Pow( 10, scale )).ToString( );
		}

		internal static string[] ExtraStringsValues( IByteTransform byteTransform, byte[] response, ref int index )
		{
			// 这是一个data
			List<string> strings = new List<string>( );

			if (response[index] == 0x01 || response[index] == 0x02)
			{
				// Array 或是 struct
				index++;
				int count = response[index++];
				for (int i = 0; i < count; i++)
				{
					strings.AddRange( ExtraStringsValues( byteTransform, response, ref index ) );
				}
				return strings.ToArray( );
			}
			else if (response[index] == 0x00)
			{
				// null
				return strings.ToArray( );
			}
			else
			{
				strings.Add( ExtraData( response, byteTransform, ref index, response[3], response[4] ) );
				return strings.ToArray( );
			}
		}

		/// <summary>
		/// 根据错误代码返回详细的错误文本消息
		/// </summary>
		/// <param name="err">错误代码</param>
		/// <returns>错误文本消息</returns>
		public static string GetErrorText( byte err )
		{
			switch (err)
			{
				case 01: return StringResources.Language.DLT698Error01;
				case 02: return StringResources.Language.DLT698Error02;
				case 03: return StringResources.Language.DLT698Error03;
				case 04: return StringResources.Language.DLT698Error04;
				case 05: return StringResources.Language.DLT698Error05;
				case 06: return StringResources.Language.DLT698Error06;
				case 07: return StringResources.Language.DLT698Error07;
				case 08: return StringResources.Language.DLT698Error08;
				case 09: return StringResources.Language.DLT698Error09;
				case 10: return StringResources.Language.DLT698Error10;
				case 11: return StringResources.Language.DLT698Error11;
				case 12: return StringResources.Language.DLT698Error12;
				case 13: return StringResources.Language.DLT698Error13;
				case 14: return StringResources.Language.DLT698Error14;
				case 15: return StringResources.Language.DLT698Error15;
				case 16: return StringResources.Language.DLT698Error16;
				case 17: return StringResources.Language.DLT698Error17;
				case 18: return StringResources.Language.DLT698Error18;
				case 19: return StringResources.Language.DLT698Error19;
				case 20: return StringResources.Language.DLT698Error20;
				case 21: return StringResources.Language.DLT698Error21;
				case 22: return StringResources.Language.DLT698Error22;
				case 23: return StringResources.Language.DLT698Error23;
				case 24: return StringResources.Language.DLT698Error24;
				case 25: return StringResources.Language.DLT698Error25;
				case 26: return StringResources.Language.DLT698Error26;
				case 27: return StringResources.Language.DLT698Error27;
				case 28: return StringResources.Language.DLT698Error28;
				case 29: return StringResources.Language.DLT698Error29;
				case 30: return StringResources.Language.DLT698Error30;
				case 31: return StringResources.Language.DLT698Error31;
				case 32: return StringResources.Language.DLT698Error32;
				case 33: return StringResources.Language.DLT698Error33;
				case 34: return StringResources.Language.DLT698Error34;
				case 35: return StringResources.Language.DLT698Error35;
				default: return StringResources.Language.UnknownError;
			}
		}

#endregion
	}
}
