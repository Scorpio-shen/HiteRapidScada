using HslCommunication.BasicFramework;
using HslCommunication.Core;
using HslCommunication.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
#if !NET35 && !NET20
using System.Threading.Tasks;
#endif

namespace HslCommunication.Instrument.DLT.Helper
{
	/// <summary>
	/// DLT645相关的辅助类
	/// </summary>
	public class DLT645Helper
	{
		#region Static Helper

		/// <summary>
		/// 将地址解析成BCD码的地址，并且扩充到12位，不够的补0操作
		/// </summary>
		/// <param name="address">地址域信息</param>
		/// <returns>实际的结果</returns>
		public static OperateResult<byte[]> GetAddressByteFromString( string address )
		{
			if (address == null || address.Length == 0) return new OperateResult<byte[]>( StringResources.Language.DLTAddressCannotNull );
			if (address.Length > 12) return new OperateResult<byte[]>( StringResources.Language.DLTAddressCannotMoreThan12 );
			if (!Regex.IsMatch( address, "^[0-9A-A]+$" )) return new OperateResult<byte[]>( StringResources.Language.DLTAddressMatchFailed );
			if (address.Length < 12) address = address.PadLeft( 12, '0' );
			return OperateResult.CreateSuccessResult( address.ToHexBytes( ).Reverse( ).ToArray( ) );
		}

		/// <summary>
		/// 将指定的地址信息，控制码信息，数据域信息打包成完整的报文命令
		/// </summary>
		/// <param name="address">地址域信息，地址域由6个字节构成，每字节2位BCD码，地址长度可达12位十进制数。地址域支持锁位寻址，即从若干低位起，剩余高位补AAH作为通配符进行读表操作</param>
		/// <param name="control">控制码信息</param>
		/// <param name="dataArea">数据域的内容</param>
		/// <returns>返回是否报文创建成功</returns>
		public static OperateResult<byte[]> BuildDlt645EntireCommand( string address, byte control, byte[] dataArea )
		{
			if (dataArea == null) dataArea = new byte[0];
			OperateResult<byte[]> add = GetAddressByteFromString( address );
			if (!add.IsSuccess) return add;

			byte[] buffer = new byte[12 + dataArea.Length];
			buffer[0] = 0x68;                                  // 帧起始符
			add.Content.CopyTo( buffer, 1 );                   // BCD码的地址信息
			buffer[7] = 0x68;                                  // 帧起始符
			buffer[8] = control;                               // 控制码
			buffer[9] = (byte)dataArea.Length;                 // 数据域长度，读的时候小于等于200，写的时候，小于等于50
			if (dataArea.Length > 0)
			{
				dataArea.CopyTo( buffer, 10 );
				for (int i = 0; i < dataArea.Length; i++)
				{
					// 数据域，发送之前增加0x33
					buffer[i + 10] += 0x33;
				}
			}

			// 求校验码
			int count = 0;
			for (int i = 0; i < buffer.Length - 2; i++)
			{
				count += buffer[i];
			}
			buffer[buffer.Length - 2] = (byte)count;           // 校验码
			buffer[buffer.Length - 1] = AsciiControl.SYN;      // 结束符
			return OperateResult.CreateSuccessResult( buffer );
		}

		/// <summary>
		/// 从用户输入的地址信息中解析出真实的地址及数据标识
		/// </summary>
		/// <param name="type">DLT的类型</param>
		/// <param name="address">用户输入的地址信息</param>
		/// <param name="defaultStation">默认的地址域</param>
		/// <param name="length">数据长度信息</param>
		/// <returns>解析结果信息</returns>
		public static OperateResult<string, byte[]> AnalysisBytesAddress( DLT645Type type, string address, string defaultStation, ushort length = 1 )
		{
			try
			{
				string region = defaultStation;
				byte[] dataId = null;
				int offset = 0;
				if (type == DLT645Type.DLT2007)
				{
					dataId = length == 1 ? new byte[4] : new byte[5];
					if (length != 1) dataId[4] = (byte)length;
				}
				else
				{
					dataId = length == 1 ? new byte[2] : new byte[3];
					if (length != 1)
					{
						dataId[0] = (byte)length;
						offset = 1;
					}
				}

				if (address.IndexOf( ';' ) > 0)
				{
					string[] splits = address.Split( new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries );
					for (int i = 0; i < splits.Length; i++)
					{
						if (splits[i].StartsWith( "s=" ))
						{
							region = splits[i].Substring( 2 );
						}
						else
						{
							splits[i].ToHexBytes( ).Reverse( ).ToArray( ).CopyTo( dataId, offset );
						}
					}
				}
				else
				{
					address.ToHexBytes( ).Reverse( ).ToArray( ).CopyTo( dataId, offset );
				}
				return OperateResult.CreateSuccessResult( region, dataId );
			}
			catch(Exception ex)
			{
				return new OperateResult<string, byte[]>( "Address prase wrong: " + ex.Message );
			}
		}

		/// <summary>
		/// 从用户输入的地址信息中解析出真实的地址及数据标识
		/// </summary>
		/// <param name="address">用户输入的地址信息</param>
		/// <param name="defaultStation">默认的地址域</param>
		/// <returns>解析结果信息</returns>
		public static OperateResult<string, int> AnalysisIntegerAddress( string address, string defaultStation )
		{
			try
			{
				string region = defaultStation;
				int value = 0;
				if (address.IndexOf( ';' ) > 0)
				{
					string[] splits = address.Split( new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries );
					for (int i = 0; i < splits.Length; i++)
					{
						if (splits[i].StartsWith( "s=" ))
						{
							region = splits[i].Substring( 2 );
						}
						else
						{
							value = Convert.ToInt32( splits[i] );
						}
					}
				}
				else
				{
					value = Convert.ToInt32( address );
				}
				return OperateResult.CreateSuccessResult( region, value );
			}
			catch (Exception ex)
			{
				return new OperateResult<string, int>( ex.Message );
			}
		}

		/// <summary>
		/// 检查当前的反馈数据信息是否正确
		/// </summary>
		/// <param name="dlt">DLT通信设备</param>
		/// <param name="response">从仪表反馈的数据信息</param>
		/// <returns>是否校验成功</returns>
		public static OperateResult CheckResponse( IDlt645 dlt, byte[] response )
		{
			if (response.Length < 9) return new OperateResult( StringResources.Language.ReceiveDataLengthTooShort );
			if ((response[8] & 0x40) == 0x40)
			{
				// 异常的响应
				byte error = response[10];
				if (dlt.DLTType == DLT645Type.DLT2007)
				{
					if (error.GetBoolByIndex( 0 )) return new OperateResult( StringResources.Language.DLTErrorInfoBit0 );
					if (error.GetBoolByIndex( 1 )) return new OperateResult( StringResources.Language.DLTErrorInfoBit1 );
					if (error.GetBoolByIndex( 2 )) return new OperateResult( StringResources.Language.DLTErrorInfoBit2 );
					if (error.GetBoolByIndex( 3 )) return new OperateResult( StringResources.Language.DLTErrorInfoBit3 );
					if (error.GetBoolByIndex( 4 )) return new OperateResult( StringResources.Language.DLTErrorInfoBit4 );
					if (error.GetBoolByIndex( 5 )) return new OperateResult( StringResources.Language.DLTErrorInfoBit5 );
					if (error.GetBoolByIndex( 6 )) return new OperateResult( StringResources.Language.DLTErrorInfoBit6 );
					if (error.GetBoolByIndex( 7 )) return new OperateResult( StringResources.Language.DLTErrorInfoBit7 );
					return OperateResult.CreateSuccessResult( );
				}
				else
				{
					if (error.GetBoolByIndex( 0 )) return new OperateResult( StringResources.Language.DLT1997ErrorInfoBit0 );
					if (error.GetBoolByIndex( 1 )) return new OperateResult( StringResources.Language.DLT1997ErrorInfoBit1 );
					if (error.GetBoolByIndex( 2 )) return new OperateResult( StringResources.Language.DLT1997ErrorInfoBit2 );
					if (error.GetBoolByIndex( 4 )) return new OperateResult( StringResources.Language.DLT1997ErrorInfoBit4 );
					if (error.GetBoolByIndex( 5 )) return new OperateResult( StringResources.Language.DLT1997ErrorInfoBit5 );
					if (error.GetBoolByIndex( 6 )) return new OperateResult( StringResources.Language.DLT1997ErrorInfoBit6 );
					return OperateResult.CreateSuccessResult( );
				}
			}
			else
			{
				// 正常的响应
				return OperateResult.CreateSuccessResult( );
			}
		}

		/// <summary>
		/// 寻找0x68字节开头的位置信息
		/// </summary>
		/// <param name="buffer">缓存数据</param>
		/// <returns>如果有则为索引位置，如果没有则为空</returns>
		public static int FindHeadCode68H( byte[] buffer )
		{
			if (buffer == null) return -1;
			for (int i = 0; i < buffer.Length; i++)
			{
				if (buffer[i] == 0x68) return i;
			}
			return -1;
		}

		#endregion

		#region DLT Helper

		private static OperateResult<byte[]> ReadWithAddress( IDlt645 dlt, string address, byte[] dataArea )
		{
			OperateResult<byte[]> command = Helper.DLT645Helper.BuildDlt645EntireCommand( address, 
				dlt.DLTType == DLT645Type.DLT2007 ? DLTControl.DLT2007_ReadData : DLTControl.DLT1997_ReadData, dataArea );
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> read = dlt.ReadFromCoreServer( command.Content );
			if (!read.IsSuccess) return read;

			OperateResult check = Helper.DLT645Helper.CheckResponse( dlt, read.Content );
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( check );

			if (dlt.DLTType == DLT645Type.DLT2007)
			{
				if (read.Content.Length < 16) return OperateResult.CreateSuccessResult( new byte[0] );
				return OperateResult.CreateSuccessResult( read.Content.SelectMiddle( 14, read.Content.Length - 16 ) );
			}
			else
			{
				if (read.Content.Length < 14) return OperateResult.CreateSuccessResult( new byte[0] );
				return OperateResult.CreateSuccessResult( read.Content.SelectMiddle( 12, read.Content.Length - 14 ) );
			}
		}

		/// <summary>
		/// 根据指定的数据标识来读取相关的原始数据信息，地址标识根据手册来，从高位到地位，例如 00-00-00-00，分割符可以任意特殊字符或是没有分隔符。<br />
		/// Read the relevant original data information according to the specified data identifier. The address identifier is based on the manual, 
		/// from high to position, such as 00-00-00-00. The separator can be any special character or no separator.
		/// </summary>
		/// <remarks>
		/// 地址可以携带地址域信息，例如 "s=2;00-00-00-00" 或是 "s=100000;00-00-02-00"，关于数据域信息，需要查找手册，例如:00-01-00-00 表示： (当前)正向有功总电能
		/// </remarks>
		/// <param name="dlt">DLT通信对象</param>
		/// <param name="address">数据标识，具体需要查找手册来对应</param>
		/// <param name="length">数据长度信息</param>
		/// <returns>结果信息</returns>
		public static OperateResult<byte[]> Read( IDlt645 dlt, string address, ushort length )
		{
			OperateResult<string, byte[]> analysis = Helper.DLT645Helper.AnalysisBytesAddress( dlt.DLTType, address, dlt.Station, length );
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( analysis );

			return Helper.DLT645Helper.ReadWithAddress( dlt, analysis.Content1, analysis.Content2 );
		}

		/// <summary>
		/// 读取指定地址的所有的字符串数据信息，一般来说，一个地址只有一个数据，但是少部分的地址存在多个数据，例如 01-01-00-00 正向有功总需求及发生时间<br />
		/// Read all the string data information of the specified address, in general, there is only one data for one address, but there are multiple data for a small number of addresses, 
		/// such as 01-01-00-00 Forward active total demand and occurrence time
		/// </summary>
		/// <remarks>
		/// 地址可以携带地址域信息，例如 "s=2;00-00-00-00" 或是 "s=100000;00-00-02-00"，关于数据域信息，需要查找手册，例如:00-01-00-00 表示： (当前)正向有功总电能<br />
		/// 地址也可以携带是否数据翻转的标记，例如 "reverse=false;00-00-00-00" 解析数据的时候就不发生反转的操作
		/// </remarks>
		/// <param name="dlt">DLT通信对象</param>
		/// <param name="address">数据标识，具体需要查找手册来对应</param>
		/// <returns>字符串数组信息</returns>
		public static OperateResult<string[]> ReadStringArray( IDlt645 dlt, string address )
		{
			bool reverse = HslHelper.ExtractBooleanParameter( ref address, "reverse", true );

			OperateResult<string, byte[]> analysis = Helper.DLT645Helper.AnalysisBytesAddress( dlt.DLTType, address, dlt.Station, 1 );
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<string[]>( analysis );

			OperateResult<byte[]> read = Helper.DLT645Helper.ReadWithAddress( dlt, analysis.Content1, analysis.Content2 );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<string[]>( read );

			return DLTTransform.TransStringsFromDLt( dlt.DLTType, read.Content, analysis.Content2, reverse );
		}

		/// <summary>
		/// 读取指定地址的所有的double数据信息，一般来说，一个地址只有一个数据，但是少部分的地址存在多个数据，然后全部转换为double数据信息<br />
		/// Read all the double data information of the specified address, in general, an address has only one data, but a small number of addresses exist multiple data, 
		/// and then all converted to double data information
		/// </summary>
		/// <remarks>
		/// 地址可以携带地址域信息，例如 "s=2;00-00-00-00" 或是 "s=100000;00-00-02-00"，关于数据域信息，需要查找手册，例如:00-01-00-00 表示： (当前)正向有功总电能<br />
		/// 地址也可以携带是否数据翻转的标记，例如 "reverse=false;00-00-00-00" 解析数据的时候就不发生反转的操作
		/// </remarks>
		/// <param name="dlt">DLT通信对象</param>
		/// <param name="address">数据标识，具体需要查找手册来对应</param>
		/// <param name="length">读取的数据长度信息</param>
		public static OperateResult<double[]> ReadDouble( IDlt645 dlt, string address, ushort length )
		{
			OperateResult<string[]> read = Helper.DLT645Helper.ReadStringArray( dlt, address );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<double[]>( read );

			try
			{
				return OperateResult.CreateSuccessResult( read.Content.Take( length ).Select( m => double.Parse( m ) ).ToArray( ) );
			}
			catch (Exception ex)
			{
				return new OperateResult<double[]>( "double.Parse failed: " + ex.Message + Environment.NewLine + "Source: " + read.Content.ToArrayString( ) );
			}
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
		/// <param name="dlt">DLT通信对象</param>
		/// <param name="password">密钥信息</param>
		/// <param name="opCode">操作者代码</param>
		/// <param name="address">地址信息</param>
		/// <param name="value">写入的数据值</param>
		/// <returns>是否写入成功</returns>
		public static OperateResult Write( IDlt645 dlt, string password, string opCode, string address, byte[] value )
		{
			OperateResult<string, byte[]> analysis = Helper.DLT645Helper.AnalysisBytesAddress( dlt.DLTType, address, dlt.Station );
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( analysis );

			byte[] content = null;
			if (dlt.DLTType == DLT645Type.DLT2007)
				content = SoftBasic.SpliceArray( analysis.Content2, password.ToHexBytes( ), opCode.ToHexBytes( ), value );
			else
				content = SoftBasic.SpliceArray( analysis.Content2, value );

			OperateResult<byte[]> command = Helper.DLT645Helper.BuildDlt645EntireCommand( analysis.Content1, 
				dlt.DLTType == DLT645Type.DLT2007 ? DLTControl.DLT2007_WriteData : DLTControl.DLT1997_WriteData, content );
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> read = dlt.ReadFromCoreServer( command.Content );
			if (!read.IsSuccess) return read;

			return Helper.DLT645Helper.CheckResponse( dlt, read.Content );
		}

		/// <summary>
		/// 读取设备的通信地址，仅支持点对点通讯的情况，返回地址域数据，例如：149100007290<br />
		/// Read the communication address of the device, only support point-to-point communication, and return the address field data, for example: 149100007290
		/// </summary>
		/// <param name="dlt">DLT通信对象</param>
		/// <returns>设备的通信地址</returns>
		public static OperateResult<string> ReadAddress( IDlt645 dlt )
		{
			OperateResult<byte[]> command = Helper.DLT645Helper.BuildDlt645EntireCommand( "AAAAAAAAAAAA", DLTControl.DLT2007_ReadAddress, null );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<string>( command );

			OperateResult<byte[]> read = dlt.ReadFromCoreServer( command.Content );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<string>( read );

			OperateResult check = Helper.DLT645Helper.CheckResponse(dlt, read.Content );
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<string>( check );

			dlt.Station = read.Content.SelectMiddle( 1, 6 ).Reverse( ).ToArray( ).ToHexString( );
			return OperateResult.CreateSuccessResult( read.Content.SelectMiddle( 1, 6 ).Reverse( ).ToArray( ).ToHexString( ) );
		}

		/// <summary>
		/// 写入设备的地址域信息，仅支持点对点通讯的情况，需要指定地址域信息，例如：149100007290<br />
		/// Write the address domain information of the device, only support point-to-point communication, 
		/// you need to specify the address domain information, for example: 149100007290
		/// </summary>
		/// <param name="dlt">DLT通信对象</param>
		/// <param name="address">等待写入的地址域</param>
		/// <returns>是否写入成功</returns>
		public static OperateResult WriteAddress( IDlt645 dlt, string address )
		{
			OperateResult<byte[]> add = Helper.DLT645Helper.GetAddressByteFromString( address );
			if (!add.IsSuccess) return add;

			OperateResult<byte[]> command = Helper.DLT645Helper.BuildDlt645EntireCommand( "AAAAAAAAAAAA", 
				dlt.DLTType == DLT645Type.DLT2007 ? DLTControl.DLT2007_WriteAddress : DLTControl.DLT1997_WriteAddress, add.Content );
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> read = dlt.ReadFromCoreServer( command.Content );
			if (!read.IsSuccess) return read;

			OperateResult check = Helper.DLT645Helper.CheckResponse(dlt, read.Content );
			if (!check.IsSuccess) return check;

			if (SoftBasic.IsTwoBytesEquel( read.Content.SelectMiddle( 1, 6 ), Helper.DLT645Helper.GetAddressByteFromString( address ).Content ))
				return OperateResult.CreateSuccessResult( );
			else
				return new OperateResult( StringResources.Language.DLTErrorWriteReadCheckFailed );
		}

		/// <summary>
		/// 广播指定的时间，强制从站与主站时间同步，传入<see cref="DateTime"/>时间对象，没有数据返回。<br />
		/// Broadcast the specified time, force the slave station to synchronize with the master station time, 
		/// pass in the <see cref="DateTime"/> time object, and no data will be returned.
		/// </summary>
		/// <param name="dlt">DLT通信对象</param>
		/// <param name="dateTime">时间对象</param>
		/// <returns>是否成功</returns>
		public static OperateResult BroadcastTime( IDlt645 dlt, DateTime dateTime )
		{
			string hex = $"{dateTime.Second:D2}{dateTime.Minute:D2}{dateTime.Hour:D2}{dateTime.Day:D2}{dateTime.Month:D2}{dateTime.Year % 100:D2}";

			OperateResult<byte[]> command = Helper.DLT645Helper.BuildDlt645EntireCommand( "999999999999", 
				dlt.DLTType == DLT645Type.DLT2007 ? DLTControl.DLT2007_Broadcast : DLTControl.DLT1997_Broadcast, hex.ToHexBytes( ) );
			if (!command.IsSuccess) return command;

			return dlt.ReadFromCoreServer( command.Content, false );
		}

		/// <summary>
		/// 对设备发送冻结命令，默认点对点操作，地址域为 99999999999999 时为广播，数据域格式说明：MMDDhhmm(月日时分)，
		/// 99DDhhmm表示月为周期定时冻结，9999hhmm表示日为周期定时冻结，999999mm表示以小时为周期定时冻结，99999999表示瞬时冻结<br />
		/// Send a freeze command to the device, the default point-to-point operation, when the address field is 9999999999999, 
		/// it is broadcast, and the data field format description: MMDDhhmm (month, day, hour and minute), 
		/// 99DDhhmm means the month is the periodic fixed freeze, 9999hhmm means the day is the periodic periodic freeze, 
		/// and 999999mm means the hour It is periodic timed freezing, 99999999 means instantaneous freezing
		/// </summary>
		/// <param name="dlt">DLT通信对象</param>
		/// <param name="dataArea">数据域信息</param>
		/// <returns>是否成功冻结</returns>
		public static OperateResult FreezeCommand( IDlt645 dlt, string dataArea )
		{
			OperateResult<string, byte[]> analysis = Helper.DLT645Helper.AnalysisBytesAddress( dlt.DLTType, dataArea, dlt.Station );
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( analysis );

			OperateResult<byte[]> command = Helper.DLT645Helper.BuildDlt645EntireCommand( analysis.Content1, DLTControl.DLT2007_FreezeCommand, analysis.Content2 );
			if (!command.IsSuccess) return command;

			if (analysis.Content1 == "999999999999")
			{
				// 广播操作
				return dlt.ReadFromCoreServer( command.Content, false );
			}
			else
			{
				// 点对点操作
				OperateResult<byte[]> read = dlt.ReadFromCoreServer( command.Content );
				if (!read.IsSuccess) return read;

				return Helper.DLT645Helper.CheckResponse(dlt, read.Content );
			}
		}

		private static OperateResult<byte[]> BuildChangeBaudRateCommand( IDlt645 dlt, string baudRate, out byte code )
		{
			code = 0x00;
			OperateResult<string, int> analysis = Helper.DLT645Helper.AnalysisIntegerAddress( baudRate, dlt.Station );
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( analysis );

			if (dlt.DLTType == DLT645Type.DLT2007)
			{
				switch (analysis.Content2)
				{
					case 600: code = 0x02; break;
					case 1200: code = 0x04; break;
					case 2400: code = 0x08; break;
					case 4800: code = 0x10; break;
					case 9600: code = 0x20; break;
					case 19200: code = 0x40; break;
					default: return new OperateResult<byte[]>( StringResources.Language.NotSupportedFunction );
				}
			}
			else
			{
				switch (analysis.Content2)
				{
					case 300: code = 0x02; break;
					case 600: code = 0x04; break;
					case 2400: code = 0x10; break;
					case 4800: code = 0x20; break;
					case 9600: code = 0x40; break;
					default: return new OperateResult<byte[]>( StringResources.Language.NotSupportedFunction );
				}
			}

			return Helper.DLT645Helper.BuildDlt645EntireCommand( analysis.Content1,
				dlt.DLTType == DLT645Type.DLT2007 ? DLTControl.DLT2007_ChangeBaudRate : DLTControl.DLT1997_ChangeBaudRate, new byte[] { code } );
		}

		/// <summary>
		/// 更改通信速率，波特率可选 600,1200,2400,4800,9600,19200，其他值无效，可以携带地址域信息，s=1;9600 <br />
		/// Change the communication rate, the baud rate can be 600, 1200, 2400, 4800, 9600, 19200, 
		/// other values are invalid, you can carry address domain information, s=1;9600
		/// </summary>
		/// <remarks>
		/// 对于DLT1997来说，只支持 300, 600, 2400, 4800, 9600
		/// </remarks>
		/// <param name="dlt">DLT通信对象</param>
		/// <param name="baudRate">波特率的信息</param>
		/// <returns>是否更改成功</returns>
		public static OperateResult ChangeBaudRate( IDlt645 dlt, string baudRate )
		{
			OperateResult<byte[]> command = BuildChangeBaudRateCommand( dlt, baudRate, out byte code );
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> read = dlt.ReadFromCoreServer( command.Content );
			if (!read.IsSuccess) return read;

			OperateResult check = Helper.DLT645Helper.CheckResponse(dlt, read.Content );
			if (!check.IsSuccess) return check;

			if (read.Content[10] == code)
				return OperateResult.CreateSuccessResult( );
			else
				return new OperateResult( StringResources.Language.DLTErrorWriteReadCheckFailed );
		}

		#endregion

#if !NET35 && !NET20
		private async static Task<OperateResult<byte[]>> ReadWithAddressAsync( IDlt645 dlt, string address, byte[] dataArea )
		{
			OperateResult<byte[]> command = Helper.DLT645Helper.BuildDlt645EntireCommand( address, 
				dlt.DLTType == DLT645Type.DLT2007 ? DLTControl.DLT2007_ReadData : DLTControl.DLT1997_ReadData, dataArea );
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> read = await dlt.ReadFromCoreServerAsync( command.Content );
			if (!read.IsSuccess) return read;

			OperateResult check = Helper.DLT645Helper.CheckResponse(dlt, read.Content );
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( check );


			if (dlt.DLTType == DLT645Type.DLT2007)
			{
				if (read.Content.Length < 16) return OperateResult.CreateSuccessResult( new byte[0] );
				return OperateResult.CreateSuccessResult( read.Content.SelectMiddle( 14, read.Content.Length - 16 ) );
			}
			else
			{
				if (read.Content.Length < 14) return OperateResult.CreateSuccessResult( new byte[0] );
				return OperateResult.CreateSuccessResult( read.Content.SelectMiddle( 12, read.Content.Length - 14 ) );
			}
		}

		/// <inheritdoc cref="Read(IDlt645, string, ushort)"/>
		public async static Task<OperateResult<byte[]>> ReadAsync( IDlt645 dlt, string address, ushort length )
		{
			OperateResult<string, byte[]> analysis = Helper.DLT645Helper.AnalysisBytesAddress( dlt.DLTType, address, dlt.Station, length );
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( analysis );

			return await ReadWithAddressAsync( dlt, analysis.Content1, analysis.Content2 );
		}

		/// <inheritdoc cref="ReadDouble(IDlt645, string, ushort)"/>
		public async static Task<OperateResult<double[]>> ReadDoubleAsync( IDlt645 dlt, string address, ushort length )
		{
			OperateResult<string[]> read = await ReadStringArrayAsync( dlt, address );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<double[]>( read );

			try
			{
				return OperateResult.CreateSuccessResult( read.Content.Take( length ).Select( m => double.Parse( m ) ).ToArray( ) );
			}
			catch (Exception ex)
			{
				return new OperateResult<double[]>( "double.Parse failed: " + ex.Message + Environment.NewLine + "Source: " + read.Content.ToArrayString( ) );
			}
		}

		/// <inheritdoc cref="ReadStringArray(IDlt645, string)"/>
		public async static Task<OperateResult<string[]>> ReadStringArrayAsync( IDlt645 dlt, string address )
		{
			bool reverse = HslHelper.ExtractBooleanParameter( ref address, "reverse", true );

			OperateResult<string, byte[]> analysis = Helper.DLT645Helper.AnalysisBytesAddress( dlt.DLTType, address, dlt.Station, 1 );
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<string[]>( analysis );

			OperateResult<byte[]> read = await ReadWithAddressAsync( dlt, analysis.Content1, analysis.Content2 );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<string[]>( read );

			return DLTTransform.TransStringsFromDLt( dlt.DLTType, read.Content, analysis.Content2, reverse );
		}

		/// <inheritdoc cref="Write(IDlt645, string, string, string, byte[])"/>
		public async static Task<OperateResult> WriteAsync( IDlt645 dlt, string password, string opCode, string address, byte[] value )
		{
			OperateResult<string, byte[]> analysis = Helper.DLT645Helper.AnalysisBytesAddress( dlt.DLTType, address, dlt.Station );
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( analysis );

			byte[] content = null;
			if (dlt.DLTType == DLT645Type.DLT2007)
				content = SoftBasic.SpliceArray( analysis.Content2, password.ToHexBytes( ), opCode.ToHexBytes( ), value );
			else
				content = SoftBasic.SpliceArray( analysis.Content2, value );

			OperateResult<byte[]> command = Helper.DLT645Helper.BuildDlt645EntireCommand( analysis.Content1, 
				dlt.DLTType == DLT645Type.DLT2007 ? DLTControl.DLT2007_WriteData : DLTControl.DLT1997_WriteData, content );
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> read = await dlt.ReadFromCoreServerAsync( command.Content );
			if (!read.IsSuccess) return read;

			return Helper.DLT645Helper.CheckResponse( dlt, read.Content );
		}

		/// <inheritdoc cref="ReadAddress(IDlt645)"/>
		public async static Task<OperateResult<string>> ReadAddressAsync( IDlt645 dlt )
		{
			OperateResult<byte[]> command = Helper.DLT645Helper.BuildDlt645EntireCommand( "AAAAAAAAAAAA", DLTControl.DLT2007_ReadAddress, null );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<string>( command );

			OperateResult<byte[]> read = await dlt.ReadFromCoreServerAsync( command.Content );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<string>( read );

			OperateResult check = Helper.DLT645Helper.CheckResponse( dlt, read.Content );
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<string>( check );

			dlt.Station = read.Content.SelectMiddle( 1, 6 ).Reverse( ).ToArray( ).ToHexString( );
			return OperateResult.CreateSuccessResult( read.Content.SelectMiddle( 1, 6 ).Reverse( ).ToArray( ).ToHexString( ) );
		}

		/// <inheritdoc cref="WriteAddress(IDlt645, string)"/>
		public static async Task<OperateResult> WriteAddressAsync( IDlt645 dlt, string address )
		{
			OperateResult<byte[]> add = Helper.DLT645Helper.GetAddressByteFromString( address );
			if (!add.IsSuccess) return add;

			OperateResult<byte[]> command = Helper.DLT645Helper.BuildDlt645EntireCommand( "AAAAAAAAAAAA", 
				dlt.DLTType == DLT645Type.DLT2007 ? DLTControl.DLT2007_WriteAddress : DLTControl.DLT1997_WriteAddress, add.Content );
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> read = await dlt.ReadFromCoreServerAsync( command.Content );
			if (!read.IsSuccess) return read;

			OperateResult check = Helper.DLT645Helper.CheckResponse( dlt, read.Content );
			if (!check.IsSuccess) return check;

			if (SoftBasic.IsTwoBytesEquel( read.Content.SelectMiddle( 1, 6 ), Helper.DLT645Helper.GetAddressByteFromString( address ).Content ))
				return OperateResult.CreateSuccessResult( );
			else
				return new OperateResult( StringResources.Language.DLTErrorWriteReadCheckFailed );
		}

		/// <inheritdoc cref="BroadcastTime(IDlt645, DateTime)"/>
		public async static Task<OperateResult> BroadcastTimeAsync( IDlt645 dlt, DateTime dateTime, Func<byte[], bool,bool,Task<OperateResult<byte[]>>> func )
		{
			string hex = $"{dateTime.Second:D2}{dateTime.Minute:D2}{dateTime.Hour:D2}{dateTime.Day:D2}{dateTime.Month:D2}{dateTime.Year % 100:D2}";

			OperateResult<byte[]> command = Helper.DLT645Helper.BuildDlt645EntireCommand( "999999999999", 
				dlt.DLTType == DLT645Type.DLT2007 ? DLTControl.DLT2007_Broadcast : DLTControl.DLT1997_Broadcast, hex.ToHexBytes( ) );
			if (!command.IsSuccess) return command;

			return await func( command.Content, false, true );
		}

		/// <inheritdoc cref="FreezeCommand(IDlt645, string)"/>
		public async static Task<OperateResult> FreezeCommandAsync( DLT645OverTcp dlt, string dataArea )
		{
			OperateResult<string, byte[]> analysis = Helper.DLT645Helper.AnalysisBytesAddress( dlt.DLTType, dataArea, dlt.Station );
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( analysis );

			OperateResult<byte[]> command = Helper.DLT645Helper.BuildDlt645EntireCommand( analysis.Content1, DLTControl.DLT2007_FreezeCommand, analysis.Content2 );
			if (!command.IsSuccess) return command;

			if (analysis.Content1 == "999999999999")
			{
				// 广播操作
				return await dlt.ReadFromCoreServerAsync( command.Content, false );
			}
			else
			{
				// 点对点操作
				OperateResult<byte[]> read = await dlt.ReadFromCoreServerAsync( command.Content );
				if (!read.IsSuccess) return read;

				return Helper.DLT645Helper.CheckResponse( dlt, read.Content );
			}
		}

		/// <inheritdoc cref="ChangeBaudRate(IDlt645, string)"/>
		public async static Task<OperateResult> ChangeBaudRateAsync( IDlt645 dlt, string baudRate )
		{
			OperateResult<byte[]> command = BuildChangeBaudRateCommand( dlt, baudRate, out byte code );
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> read = await dlt.ReadFromCoreServerAsync( command.Content );
			if (!read.IsSuccess) return read;

			OperateResult check = Helper.DLT645Helper.CheckResponse( dlt, read.Content );
			if (!check.IsSuccess) return check;

			if (read.Content[10] == code)
				return OperateResult.CreateSuccessResult( );
			else
				return new OperateResult( StringResources.Language.DLTErrorWriteReadCheckFailed );
		}
#endif
	}
}
