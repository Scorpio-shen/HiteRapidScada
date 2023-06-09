﻿using HslCommunication.BasicFramework;
using HslCommunication.Core;
using HslCommunication.Core.Address;
using HslCommunication.Core.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HslCommunication.Serial;
#if !NET20 && !NET35
using System.Threading.Tasks;
#endif

namespace HslCommunication.Profinet.FATEK.Helper
{
	/// <summary>
	/// FatekProgram相关的辅助方法，例如报文构建，核心读写支持
	/// </summary>
	public class FatekProgramHelper
	{
		#region Static Helper

		/// <summary>
		/// 计算指令的和校验码
		/// </summary>
		/// <param name="data">指令</param>
		/// <returns>校验之后的信息</returns>
		public static string CalculateAcc( string data )
		{
			byte[] buffer = Encoding.ASCII.GetBytes( data );

			int count = 0;
			for (int i = 0; i < buffer.Length; i++)
			{
				count += buffer[i];
			}

			return count.ToString( "X4" ).Substring( 2 );
		}

		/// <summary>
		/// 检查当前的串口的数据接收是否完整
		/// </summary>
		/// <param name="ms">数据流</param>
		/// <returns>是否数据接收完成</returns>
		public static bool CheckReceiveDataComplete( MemoryStream ms )
		{
			byte[] buffer = ms.ToArray( );
			if ( buffer.Length < 5) return false;
			return buffer[buffer.Length - 1] == AsciiControl.ETX;
		}

		/// <summary>
		/// 将Fatek的基本命令打包成可以发送PLC的电文消息
		/// </summary>
		/// <param name="station">PLC的站号信息</param>
		/// <param name="cmd">基本命令信息</param>
		/// <returns>发送PLC的电文消息</returns>
		public static byte[] PackFatekCommand( byte station, string cmd )
		{
			StringBuilder stringBuilder = new StringBuilder( );
			stringBuilder.Append( (char)AsciiControl.STX );
			stringBuilder.Append( station.ToString( "X2" ) );

			byte[] buffer = new byte[6 + cmd.Length];
			buffer[0] = AsciiControl.STX;
			buffer[1] = SoftBasic.BuildAsciiBytesFrom( station )[0];
			buffer[2] = SoftBasic.BuildAsciiBytesFrom( station )[1];

			Encoding.ASCII.GetBytes( cmd ).CopyTo( buffer, 3 );
			SoftLRC.CalculateAccAndFill( buffer, 0, 3 );
			buffer[buffer.Length - 1] = AsciiControl.ETX;
			return buffer;
		}

		/// <summary>
		/// 创建一条读取的指令信息，需要指定一些参数
		/// </summary>
		/// <param name="station">PLC的站号</param>
		/// <param name="address">地址信息</param>
		/// <param name="length">数据长度</param>
		/// <returns>是否成功的结果对象</returns>
		public static OperateResult<List<byte[]>> BuildReadWordCommand( byte station, string address, ushort length )
		{
			station = (byte)HslHelper.ExtractParameter( ref address, "s", station );

			OperateResult<FatekProgramAddress> addressAnalysis = FatekProgramAddress.ParseFrom( address, length );
			if (!addressAnalysis.IsSuccess) return addressAnalysis.ConvertFailed<List<byte[]>>( );

			List<byte[]> contentArray = new List<byte[]>( );
			int[] splits = SoftBasic.SplitIntegerToArray( length, 64 );
			for (int i = 0; i < splits.Length; i++)
			{
				StringBuilder stringBuilder = new StringBuilder( );
				stringBuilder.Append( "46" );
				stringBuilder.Append( splits[i].ToString( "X2" ) );
				if (addressAnalysis.Content.DataCode.StartsWith( "X" ) ||
					addressAnalysis.Content.DataCode.StartsWith( "Y" ) ||
					addressAnalysis.Content.DataCode.StartsWith( "M" ) ||
					addressAnalysis.Content.DataCode.StartsWith( "S" ) ||
					addressAnalysis.Content.DataCode.StartsWith( "T" ) ||
					addressAnalysis.Content.DataCode.StartsWith( "C" ))
				{
					stringBuilder.Append( "W" );
				}

				stringBuilder.Append( addressAnalysis.Content.ToString( ) );

				contentArray.Add( PackFatekCommand( station, stringBuilder.ToString( ) ) );
				if (addressAnalysis.Content.DataCode.StartsWith( "X" ) ||
					addressAnalysis.Content.DataCode.StartsWith( "Y" ) ||
					addressAnalysis.Content.DataCode.StartsWith( "M" ) ||
					addressAnalysis.Content.DataCode.StartsWith( "S" ) ||
					addressAnalysis.Content.DataCode.StartsWith( "T" ) ||
					addressAnalysis.Content.DataCode.StartsWith( "C" ))
				{
					addressAnalysis.Content.AddressStart += splits[i] * 16;
				}
				else
				{
					addressAnalysis.Content.AddressStart += splits[i];
				}
			}

			return OperateResult.CreateSuccessResult( contentArray );
		}


		/// <summary>
		/// 创建一条读取的指令信息，需要指定一些参数
		/// </summary>
		/// <param name="station">PLC的站号</param>
		/// <param name="address">地址信息</param>
		/// <param name="length">数据长度</param>
		/// <returns>是否成功的结果对象</returns>
		public static OperateResult<List<byte[]>> BuildReadBoolCommand( byte station, string address, ushort length )
		{
			station = (byte)HslHelper.ExtractParameter( ref address, "s", station );

			OperateResult<FatekProgramAddress> addressAnalysis = FatekProgramAddress.ParseFrom( address, length );
			if (!addressAnalysis.IsSuccess) return addressAnalysis.ConvertFailed<List<byte[]>>( );

			List<byte[]> contentArray = new List<byte[]>( );
			int[] splits = SoftBasic.SplitIntegerToArray( length, 255 );
			for (int i = 0; i < splits.Length; i++)
			{
				StringBuilder stringBuilder = new StringBuilder( );
				stringBuilder.Append( "44" );
				stringBuilder.Append( splits[i].ToString( "X2" ) );
				stringBuilder.Append( addressAnalysis.Content.ToString( ) );
				contentArray.Add( PackFatekCommand( station, stringBuilder.ToString( ) ) );
				addressAnalysis.Content.AddressStart += splits[i];
			}

			return OperateResult.CreateSuccessResult( contentArray );
		}

		/// <summary>
		/// 提取当前的结果数据信息，针对的是字单位的方式
		/// </summary>
		/// <param name="response">PLC返回的数据信息</param>
		/// <param name="length">读取的长度内容</param>
		/// <returns>结果数组</returns>
		public static byte[] ExtraResponse( byte[] response, ushort length )
		{
			byte[] Content = new byte[length * 2];
			for (int i = 0; i < Content.Length / 2; i++)
			{
				ushort tmp = Convert.ToUInt16( Encoding.ASCII.GetString( response, i * 4 + 6, 4 ), 16 );
				BitConverter.GetBytes( tmp ).CopyTo( Content, i * 2 );
			}
			return Content;
		}

		/// <summary>
		/// 创建一条别入bool数据的指令信息，需要指定一些参数
		/// </summary>
		/// <param name="station">站号</param>
		/// <param name="address">地址</param>
		/// <param name="value">数组值</param>
		/// <returns>是否创建成功</returns>
		public static OperateResult<byte[]> BuildWriteBoolCommand( byte station, string address, bool[] value )
		{
			station = (byte)HslHelper.ExtractParameter( ref address, "s", station );

			OperateResult<FatekProgramAddress> addressAnalysis = FatekProgramAddress.ParseFrom( address, 0 );
			if (!addressAnalysis.IsSuccess) return addressAnalysis.ConvertFailed<byte[]>( );

			StringBuilder stringBuilder = new StringBuilder( );
			stringBuilder.Append( "45" );
			stringBuilder.Append( value.Length.ToString( "X2" ) );
			stringBuilder.Append( addressAnalysis.Content.ToString( ) );

			for (int i = 0; i < value.Length; i++)
			{
				stringBuilder.Append( value[i] ? "1" : "0" );
			}

			return OperateResult.CreateSuccessResult( PackFatekCommand( station, stringBuilder.ToString( ) ) );
		}

		/// <summary>
		/// 创建一条别入byte数据的指令信息，需要指定一些参数，按照字单位
		/// </summary>
		/// <param name="station">站号</param>
		/// <param name="address">地址</param>
		/// <param name="value">数组值</param>
		/// <returns>是否创建成功</returns>
		public static OperateResult<byte[]> BuildWriteByteCommand( byte station, string address, byte[] value )
		{
			station = (byte)HslHelper.ExtractParameter( ref address, "s", station );

			OperateResult<FatekProgramAddress> addressAnalysis = FatekProgramAddress.ParseFrom( address, 0 );
			if (!addressAnalysis.IsSuccess) return addressAnalysis.ConvertFailed<byte[]>( );

			StringBuilder stringBuilder = new StringBuilder( );
			stringBuilder.Append( "47" );
			stringBuilder.Append( (value.Length / 2).ToString( "X2" ) );

			if (addressAnalysis.Content.DataCode.StartsWith( "X" ) ||
				addressAnalysis.Content.DataCode.StartsWith( "Y" ) ||
				addressAnalysis.Content.DataCode.StartsWith( "M" ) ||
				addressAnalysis.Content.DataCode.StartsWith( "S" ) ||
				addressAnalysis.Content.DataCode.StartsWith( "T" ) ||
				addressAnalysis.Content.DataCode.StartsWith( "C" ))
			{
				stringBuilder.Append( "W" );
			}

			stringBuilder.Append( addressAnalysis.Content.ToString( ) );

			byte[] buffer = new byte[value.Length * 2];
			for (int i = 0; i < value.Length / 2; i++)
			{
				SoftBasic.BuildAsciiBytesFrom( BitConverter.ToUInt16( value, i * 2 ) ).CopyTo( buffer, 4 * i );
			}
			stringBuilder.Append( Encoding.ASCII.GetString( buffer ) );
			return OperateResult.CreateSuccessResult( PackFatekCommand( station, stringBuilder.ToString( ) ) );
		}

		/// <summary>
		/// 检查PLC反馈的报文是否正确，如果不正确，返回错误消息
		/// </summary>
		/// <param name="content">PLC反馈的报文信息</param>
		/// <returns>反馈的报文是否正确</returns>
		public static OperateResult CheckResponse( byte[] content )
		{
			if (content[0] != AsciiControl.STX) return new OperateResult( content[0], "Write Faild:" + SoftBasic.ByteToHexString( content, ' ' ) );
			if (content[5] != 0x30) return new OperateResult( content[5], GetErrorDescriptionFromCode( (char)content[5] ) );
			return OperateResult.CreateSuccessResult( );
		}

		/// <summary>
		/// 根据错误码获取到真实的文本信息
		/// </summary>
		/// <param name="code">错误码</param>
		/// <returns>错误的文本描述</returns>
		public static string GetErrorDescriptionFromCode( char code )
		{
			switch (code)
			{
				case '2': return StringResources.Language.FatekStatus02;
				case '3': return StringResources.Language.FatekStatus03;
				case '4': return StringResources.Language.FatekStatus04;
				case '5': return StringResources.Language.FatekStatus05;
				case '6': return StringResources.Language.FatekStatus06;
				case '7': return StringResources.Language.FatekStatus07;
				case '9': return StringResources.Language.FatekStatus09;
				case 'A': return StringResources.Language.FatekStatus10;
				default:  return StringResources.Language.UnknownError;
			}
		}

		#endregion

		/// <summary>
		/// 批量读取PLC的字节数据，以字为单位，支持读取X,Y,M,S,D,T,C,R,RT,RC具体的地址范围需要根据PLC型号来确认，地址可以携带站号信息，例如 s=2;D100<br />
		/// Read PLC byte data in batches, in word units. Supports reading X, Y, M, S, D, T, C, R, RT, RC. 
		/// The specific address range needs to be confirmed according to the PLC model, The address can carry station number information, such as s=2;D100
		/// </summary>
		/// <param name="device">PLC通信的对象</param>
		/// <param name="station">设备的站点信息</param>
		/// <param name="address">地址信息</param>
		/// <param name="length">数据长度</param>
		/// <returns>读取结果信息</returns>
		public static OperateResult<byte[]> Read( IReadWriteDevice device, byte station, string address, ushort length )
		{
			// 解析指令
			OperateResult<List<byte[]>> command = BuildReadWordCommand( station, address, length );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( command );

			List<byte> content = new List<byte>( );
			int[] splits = SoftBasic.SplitIntegerToArray( length, 64 );
			for (int i = 0; i < command.Content.Count; i++)
			{
				// 核心交互
				OperateResult<byte[]> read = device.ReadFromCoreServer( command.Content[i] );
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( read );

				// 结果验证
				OperateResult check = CheckResponse( read.Content );
				if (!check.IsSuccess) return check.ConvertFailed<byte[]>( );

				// 提取结果
				content.AddRange( ExtraResponse( read.Content, (ushort)splits[i] ) );
			}
			return OperateResult.CreateSuccessResult( content.ToArray( ) );
		}
#if !NET35 && !NET20
		/// <inheritdoc cref="Read(IReadWriteDevice, byte, string, ushort)"/>
		public static async Task<OperateResult<byte[]>> ReadAsync( IReadWriteDevice device, byte station, string address, ushort length )
		{
			// 解析指令
			OperateResult<List<byte[]>> command = BuildReadWordCommand( station, address, length );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( command );

			List<byte> content = new List<byte>( );
			int[] splits = SoftBasic.SplitIntegerToArray( length, 64 );
			for (int i = 0; i < command.Content.Count; i++)
			{
				// 核心交互
				OperateResult<byte[]> read = await device.ReadFromCoreServerAsync( command.Content[i] );
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( read );

				// 结果验证
				OperateResult check = CheckResponse( read.Content );
				if (!check.IsSuccess) return check.ConvertFailed<byte[]>( );

				// 提取结果
				content.AddRange( ExtraResponse( read.Content, (ushort)splits[i] ) );
			}
			return OperateResult.CreateSuccessResult( content.ToArray( ) );
		}
#endif
		/// <summary>
		/// 批量写入PLC的数据，以字为单位，也就是说最少2个字节信息，支持X,Y,M,S,D,T,C,R,RT,RC具体的地址范围需要根据PLC型号来确认，地址可以携带站号信息，例如 s=2;D100<br />
		/// The data written to the PLC in batches, in units of words, that is, at least 2 bytes of information, 
		/// supporting X, Y, M, S, D, T, C, R, RT, and RC. The specific address range needs to be based on the PLC model To confirm, The address can carry station number information, such as s=2;D100
		/// </summary>
		/// <param name="device">PLC通信的对象</param>
		/// <param name="station">设备的站号信息</param>
		/// <param name="address">地址信息，举例，D100，R200，RC100，RT200</param>
		/// <param name="value">数据值</param>
		/// <returns>是否写入成功</returns>
		public static OperateResult Write( IReadWriteDevice device, byte station, string address, byte[] value )
		{
			// 解析指令
			OperateResult<byte[]> command = BuildWriteByteCommand( station, address, value );
			if (!command.IsSuccess) return command;

			// 核心交互
			OperateResult<byte[]> read = device.ReadFromCoreServer( command.Content );
			if (!read.IsSuccess) return read;

			// 结果验证
			OperateResult check = CheckResponse( read.Content );
			if (!check.IsSuccess) return check;

			// 提取结果
			return OperateResult.CreateSuccessResult( );
		}

#if !NET35 && !NET20
		/// <inheritdoc cref="Write(IReadWriteDevice, byte, string, byte[])"/>
		public static async Task<OperateResult> WriteAsync( IReadWriteDevice device, byte station, string address, byte[] value )
		{
			// 解析指令
			OperateResult<byte[]> command = BuildWriteByteCommand( station, address, value );
			if (!command.IsSuccess) return command;

			// 核心交互
			OperateResult<byte[]> read = await device.ReadFromCoreServerAsync( command.Content );
			if (!read.IsSuccess) return read;

			// 结果验证
			OperateResult check = CheckResponse( read.Content );
			if (!check.IsSuccess) return check;

			// 提取结果
			return OperateResult.CreateSuccessResult( );
		}
#endif
		/// <summary>
		/// 批量读取bool类型数据，支持的类型为X,Y,M,S,T,C，具体的地址范围取决于PLC的类型，地址可以携带站号信息，例如 s=2;M100<br />
		/// Read bool data in batches. The supported types are X, Y, M, S, T, C. The specific address range depends on the type of PLC, 
		/// The address can carry station number information, such as s=2;M100
		/// </summary>
		/// <param name="device">PLC通信对象</param>
		/// <param name="station">设备的站号信息</param>
		/// <param name="address">地址信息，比如X10，Y17，M100</param>
		/// <param name="length">读取的长度</param>
		/// <returns>读取结果信息</returns>
		public static OperateResult<bool[]> ReadBool( IReadWriteDevice device, byte station, string address, ushort length )
		{
			// 解析指令
			OperateResult<List<byte[]>> command = BuildReadBoolCommand( station, address, length );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( command );

			// 核心交互
			List<bool> content = new List<bool>( );
			int[] splits = SoftBasic.SplitIntegerToArray( length, 255 );
			for (int i = 0; i < command.Content.Count; i++)
			{
				OperateResult<byte[]> read = device.ReadFromCoreServer( command.Content[i] );
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( read );

				// 结果验证
				OperateResult check = CheckResponse( read.Content );
				if (!check.IsSuccess) return check.ConvertFailed<bool[]>( );

				// 提取结果
				content.AddRange( read.Content.SelectMiddle( 6, splits[i] ).Select( m => m == 0x31 ) );
			}
			return OperateResult.CreateSuccessResult( content.ToArray( ) );
		}

#if !NET35 && !NET20
		/// <inheritdoc cref="ReadBool(IReadWriteDevice, byte, string, ushort)"/>
		public static async Task<OperateResult<bool[]>> ReadBoolAsync( IReadWriteDevice device, byte station, string address, ushort length )
		{
			// 解析指令
			OperateResult<List<byte[]>> command = BuildReadBoolCommand( station, address, length );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( command );

			// 核心交互
			List<bool> content = new List<bool>( );
			int[] splits = SoftBasic.SplitIntegerToArray( length, 255 );
			for (int i = 0; i < command.Content.Count; i++)
			{
				OperateResult<byte[]> read = await device.ReadFromCoreServerAsync( command.Content[i] );
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( read );

				// 结果验证
				OperateResult check = CheckResponse( read.Content );
				if (!check.IsSuccess) return check.ConvertFailed<bool[]>( );

				// 提取结果
				content.AddRange( read.Content.SelectMiddle( 6, splits[i] ).Select( m => m == 0x31 ) );
			}
			return OperateResult.CreateSuccessResult( content.ToArray( ) );
		}
#endif
		/// <summary>
		/// 批量写入bool类型的数组，支持的类型为X,Y,M,S,T,C，具体的地址范围取决于PLC的类型，地址可以携带站号信息，例如 s=2;M100<br />
		/// Write arrays of type bool in batches. The supported types are X, Y, M, S, T, C. The specific address range depends on the type of PLC, 
		/// The address can carry station number information, such as s=2;M100
		/// </summary>
		/// <param name="device">PLC通信对象</param>
		/// <param name="station">站号信息</param>
		/// <param name="address">PLC的地址信息</param>
		/// <param name="value">数据信息</param>
		/// <returns>是否写入成功</returns>
		public static OperateResult Write( IReadWriteDevice device, byte station, string address, bool[] value )
		{
			// 解析指令
			OperateResult<byte[]> command = BuildWriteBoolCommand( station, address, value );
			if (!command.IsSuccess) return command;

			// 核心交互
			OperateResult<byte[]> read = device.ReadFromCoreServer( command.Content );
			if (!read.IsSuccess) return read;

			// 结果验证
			OperateResult check = CheckResponse( read.Content );
			if (!check.IsSuccess) return check;

			// 提取结果
			return OperateResult.CreateSuccessResult( );
		}
#if !NET20 && !NET35
		/// <inheritdoc cref="Write(IReadWriteDevice, byte, string, bool[])"/>
		public static async Task<OperateResult> WriteAsync( IReadWriteDevice device, byte station, string address, bool[] value )
		{
			// 解析指令
			OperateResult<byte[]> command = BuildWriteBoolCommand( station, address, value );
			if (!command.IsSuccess) return command;

			// 核心交互
			OperateResult<byte[]> read = await device.ReadFromCoreServerAsync( command.Content );
			if (!read.IsSuccess) return read;

			// 结果验证
			OperateResult check = CheckResponse( read.Content );
			if (!check.IsSuccess) return check;

			// 提取结果
			return OperateResult.CreateSuccessResult( );
		}
#endif

		/// <summary>
		/// 使PLC处于RUN的状态
		/// </summary>
		/// <param name="device">PLC通信对象</param>
		/// <param name="station">站号信息</param>
		/// <returns>是否操作成功</returns>
		public static OperateResult Run( IReadWriteDevice device, byte station ) => device.ReadFromCoreServer( PackFatekCommand( station, "411" ) ).Then( CheckResponse );

#if !NET20 && !NET35
		/// <inheritdoc cref="Run(IReadWriteDevice, byte)"/>
		public static async Task<OperateResult> RunAsync( IReadWriteDevice device, byte station ) =>( await device.ReadFromCoreServerAsync( PackFatekCommand( station, "411" ) ) ).Then( CheckResponse );
#endif
		/// <summary>
		/// 使PLC处于STOP状态
		/// </summary>
		/// <param name="device">PLC通信对象</param>
		/// <param name="station">站号信息</param>
		/// <returns>是否操作成功</returns>
		public static OperateResult Stop( IReadWriteDevice device, byte station ) => device.ReadFromCoreServer( PackFatekCommand( station, "410" ) ).Then( CheckResponse );
#if !NET20 && !NET35
		/// <inheritdoc cref="Stop(IReadWriteDevice, byte)"/>
		public static async Task<OperateResult> StopAsync( IReadWriteDevice device, byte station ) => (await device.ReadFromCoreServerAsync( PackFatekCommand( station, "410" ) )).Then( CheckResponse );
#endif
		/// <summary>
		/// 读取当前PLC的状态信息，返回一个bool数组，同时包含了几种电量信息，分别为 0: RUN/STOP, 1: BAT LOW/正常, 2: Ladder checksum error/正常, 3: 使用ROM PACK/未使用, 
		/// 4: WDT Timeout/正常, 5: 设定ID/未设ID， 6： 紧急停机/正常<br />
		/// Read the status information of the current PLC and return a bool array, which also contains several power information, 0: RUN/STOP, 1: BAT LOW/normal, 
		/// 2: Ladder checksum error/normal, 3: Use ROM PACK/ Not used, 4: WDT Timeout/Normal, 5: ID set/ID not set, 6: Emergency stop/Normal
		/// </summary>
		/// <param name="device">PLC通信对象</param>
		/// <param name="station">站号信息</param>
		/// <returns>状态结果信息</returns>
		public static OperateResult<bool[]> ReadStatus( IReadWriteDevice device, byte station )
		{
			OperateResult<byte[]> read = device.ReadFromCoreServer( PackFatekCommand( station, "40" ) );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( read );

			// 结果验证
			OperateResult check = CheckResponse( read.Content );
			if (!check.IsSuccess) return check.ConvertFailed<bool[]>( );

			return OperateResult.CreateSuccessResult( Encoding.ASCII.GetString( read.Content, 6, 2 ).ToHexBytes( ).ToBoolArray( ) );
		}
#if !NET20 && !NET35
		/// <inheritdoc cref="Stop(IReadWriteDevice, byte)"/>
		public static async Task<OperateResult<bool[]>> ReadStatusAsync( IReadWriteDevice device, byte station )
		{
			OperateResult<byte[]> read = await device.ReadFromCoreServerAsync( PackFatekCommand( station, "40" ) );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( read );

			// 结果验证
			OperateResult check = CheckResponse( read.Content );
			if (!check.IsSuccess) return check.ConvertFailed<bool[]>( );

			return OperateResult.CreateSuccessResult( Encoding.ASCII.GetString( read.Content, 6, 2 ).ToHexBytes( ).ToBoolArray( ) );
		}
#endif
	}
}
