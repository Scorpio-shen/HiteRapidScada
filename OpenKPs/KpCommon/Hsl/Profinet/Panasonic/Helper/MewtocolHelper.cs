using HslCommunication.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if !NET35 && !NET20
using System.Threading.Tasks;
#endif

namespace HslCommunication.Profinet.Panasonic.Helper
{
	/// <summary>
	/// Mewtocol协议的辅助类信息
	/// </summary>
	public class MewtocolHelper
	{
		/// <summary>
		/// 读取单个的地址信息的bool值，地址举例：SR0.0  X0.0  Y0.0  R0.0  L0.0<br />
		/// Read the bool value of a single address, for example: SR0.0 X0.0 Y0.0 R0.0 L0.0
		/// </summary>
		/// <param name="plc">PLC通信对象</param>
		/// <param name="station">站号信息</param>
		/// <param name="address">起始地址</param>
		/// <returns>读取结果对象</returns>
		public static OperateResult<bool> ReadBool( IReadWriteDevice plc, byte station, string address )
		{
			station = (byte)HslHelper.ExtractParameter( ref address, "s", station );

			// 创建指令
			OperateResult<byte[]> command = PanasonicHelper.BuildReadOneCoil( station, address );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<bool>( command );

			// 数据交互
			OperateResult<byte[]> read = plc.ReadFromCoreServer( command.Content );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool>( read );

			// 提取数据
			return ByteTransformHelper.GetResultFromArray( PanasonicHelper.ExtraActualBool( read.Content ) );
		}

		/// <summary>
		/// 批量读取松下PLC的位数据，按照字为单位，地址为 X0,X10,Y10，读取的长度为16的倍数<br />
		/// Read the bit data of Panasonic PLC in batches, the unit is word, the address is X0, X10, Y10, and the read length is a multiple of 16
		/// </summary>
		/// <param name="plc">PLC通信对象</param>
		/// <param name="station">站号信息</param>
		/// <param name="address">起始地址</param>
		/// <param name="length">数据长度</param>
		/// <returns>读取结果对象</returns>
		public static OperateResult<bool[]> ReadBool( IReadWriteDevice plc, byte station, string address, ushort length )
		{
			station = (byte)HslHelper.ExtractParameter( ref address, "s", station );

			OperateResult<string, int> analysis = PanasonicHelper.AnalysisAddress( address );
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( analysis );

			// 创建指令
			OperateResult<List<byte[]>> command = PanasonicHelper.BuildReadCommand( station, address, length, true );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( command );

			List<byte> list = new List<byte>( );
			for (int i = 0; i < command.Content.Count; i++)
			{
				// 数据交互
				OperateResult<byte[]> read = plc.ReadFromCoreServer( command.Content[i] );
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( read );

				// 提取数据
				OperateResult<byte[]> extra = PanasonicHelper.ExtraActualData( read.Content );
				if (!extra.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( extra );

				// 提取bool
				list.AddRange( extra.Content );
			}

			return OperateResult.CreateSuccessResult( list.ToArray( ).ToBoolArray( ).SelectMiddle( analysis.Content2 % 16, length ) );
		}

		/// <summary>
		/// 批量读取松下PLC的位数据，传入一个读取的地址列表，地址支持X,Y,R,T,C,L, 举例：R1.0, X2.0, R3.A<br />
		/// Batch read the bit data of Panasonic PLC, pass in a read address list, the address supports X, Y, R, T, C, L, for example: R1.0, X2.0, R3.A
		/// </summary>
		/// <param name="plc">PLC通信对象</param>
		/// <param name="station">站号信息</param>
		/// <param name="address">等待读取的地址列表，数组长度不限制</param>
		/// <returns>读取结果对象</returns>
		public static OperateResult<bool[]> ReadBool( IReadWriteDevice plc, byte station, string[] address )
		{
			// 创建指令
			OperateResult<List<byte[]>> command = PanasonicHelper.BuildReadCoils( station, address );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( command );

			List<bool> list = new List<bool>( );
			for (int i = 0; i < command.Content.Count; i++)
			{
				// 数据交互
				OperateResult<byte[]> read = plc.ReadFromCoreServer( command.Content[i] );
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( read );

				// 提取数据
				OperateResult<bool[]> extra = PanasonicHelper.ExtraActualBool( read.Content );
				if (!extra.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( extra );

				list.AddRange( extra.Content );
			}

			return OperateResult.CreateSuccessResult( list.ToArray( ) );
		}

		/// <summary>
		/// 往指定的地址写入bool数据，地址举例：SR0.0  X0.0  Y0.0  R0.0  L0.0<br />
		/// Write bool data to the specified address. Example address: SR0.0 X0.0 Y0.0 R0.0 L0.0
		/// </summary>
		/// <param name="plc">PLC通信对象</param>
		/// <param name="station">站号信息</param>
		/// <param name="address">起始地址</param>
		/// <param name="value">数据值信息</param>
		/// <returns>返回是否成功的结果对象</returns>
		public static OperateResult Write( IReadWriteDevice plc, byte station, string address, bool value )
		{
			station = (byte)HslHelper.ExtractParameter( ref address, "s", station );

			// 创建指令
			OperateResult<byte[]> command = PanasonicHelper.BuildWriteOneCoil( station, address, value );
			if (!command.IsSuccess) return command;

			// 数据交互
			OperateResult<byte[]> read = plc.ReadFromCoreServer( command.Content );
			if (!read.IsSuccess) return read;

			// 提取结果
			return PanasonicHelper.ExtraActualData( read.Content );
		}

		/// <summary>
		/// 往指定的地址写入 <see cref="bool"/> 数组，地址举例 X0.0  Y0.0  R0.0  L0.0，
		/// 起始的位地址必须为16的倍数，写入的 <see cref="bool"/> 数组长度也为16的倍数。<br />
		/// Write the <see cref="bool"/> array to the specified address, address example: SR0.0 X0.0 Y0.0 R0.0 L0.0, 
		/// the starting bit address must be a multiple of 16. <see cref="bool"/> The length of the array is also a multiple of 16. <br />
		/// </summary>
		/// <param name="plc">PLC通信对象</param>
		/// <param name="station">站号信息</param>
		/// <param name="address">起始地址</param>
		/// <param name="values">数据值信息</param>
		/// <returns>返回是否成功的结果对象</returns>
		public static OperateResult Write( IReadWriteDevice plc, byte station, string address, bool[] values )
		{
			station = (byte)HslHelper.ExtractParameter( ref address, "s", station );

			// 强制地址从字单位开始，强制写入长度为16个长度
			OperateResult<string, int> analysis = PanasonicHelper.AnalysisAddress( address );
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( analysis );

			if (analysis.Content2 % 16 != 0) return new OperateResult( StringResources.Language.PanasonicAddressBitStartMulti16 );
			if (values.Length % 16 != 0) return new OperateResult( StringResources.Language.PanasonicBoolLengthMulti16 );

			// 计算字节数据
			byte[] buffer = BasicFramework.SoftBasic.BoolArrayToByte( values );

			// 创建指令
			OperateResult<byte[]> command = PanasonicHelper.BuildWriteCommand( station, address, buffer );
			if (!command.IsSuccess) return command;

			// 数据交互
			OperateResult<byte[]> read = plc.ReadFromCoreServer( command.Content );
			if (!read.IsSuccess) return read;

			// 提取结果
			return PanasonicHelper.ExtraActualData( read.Content );
		}

		/// <summary>
		/// 将Bool数组值写入到指定的离散地址里，一个地址对应一个bool值，地址数组长度和值数组长度必须相等，地址支持X,Y,R,T,C,L, 举例：R1.0, X2.0, R3.A<br />
		/// Write the Bool array value to the specified discrete address, one address corresponds to one bool value, 
		/// the length of the address array and the length of the value array must be equal, the address supports X, Y, R, T, C, L, for example: R1.0, X2.0, R3.A
		/// </summary>
		/// <param name="plc">PLC通信对象</param>
		/// <param name="station">站号信息</param>
		/// <param name="address">离散的地址列表</param>
		/// <param name="value">bool数组值</param>
		/// <returns>是否写入成功的结果对象</returns>
		public static OperateResult Write( IReadWriteDevice plc, byte station, string[] address, bool[] value )
		{
			// 创建指令
			OperateResult<List<byte[]>> command = PanasonicHelper.BuildWriteCoils( station, address, value );
			if (!command.IsSuccess) return command;

			for (int i = 0; i < command.Content.Count; i++)
			{
				// 数据交互
				OperateResult<byte[]> read = plc.ReadFromCoreServer( command.Content[i] );
				if (!read.IsSuccess) return read;

				// 提取结果
				OperateResult extra = PanasonicHelper.ExtraActualData( read.Content );
				if (!extra.IsSuccess) return extra;
			}

			return OperateResult.CreateSuccessResult( );
		}

		/// <summary>
		/// 读取指定地址的原始数据，地址示例：D0  F0  K0  T0  C0, 地址支持携带站号的访问方式，例如：s=2;D100<br />
		/// Read the original data of the specified address, address example: D0 F0 K0 T0 C0, the address supports carrying station number information, for example: s=2;D100
		/// </summary>
		/// <param name="plc">PLC通信对象</param>
		/// <param name="station">站号信息</param>
		/// <param name="address">起始地址</param>
		/// <param name="length">长度</param>
		/// <returns>原始的字节数据的信息</returns>
		public static OperateResult<byte[]> Read( IReadWriteDevice plc, byte station, string address, ushort length )
		{
			station = (byte)HslHelper.ExtractParameter( ref address, "s", station );

			// 创建指令
			OperateResult<List<byte[]>> command = PanasonicHelper.BuildReadCommand( station, address, length, false );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( command );

			List<byte> list = new List<byte>( );
			for (int i = 0; i < command.Content.Count; i++)
			{
				// 数据交互
				OperateResult<byte[]> read = plc.ReadFromCoreServer( command.Content[i] );
				if (!read.IsSuccess) return read;

				// 提取数据
				OperateResult<byte[]> extra = PanasonicHelper.ExtraActualData( read.Content );
				if (!extra.IsSuccess) return extra;

				list.AddRange( extra.Content );
			}

			return OperateResult.CreateSuccessResult( list.ToArray( ) );
		}

		/// <summary>
		/// 将数据写入到指定的地址里去，地址示例：D0  F0  K0  T0  C0, 地址支持携带站号的访问方式，例如：s=2;D100<br />
		/// Write data to the specified address, address example: D0 F0 K0 T0 C0, the address supports carrying station number information, for example: s=2;D100
		/// </summary>
		/// <param name="plc">PLC对象</param>
		/// <param name="station">站号信息</param>
		/// <param name="address">起始地址</param>
		/// <param name="value">真实数据</param>
		/// <returns>是否写入成功</returns>
		public static OperateResult Write( IReadWriteDevice plc, byte station, string address, byte[] value )
		{
			station = (byte)HslHelper.ExtractParameter( ref address, "s", station );

			// 创建指令
			OperateResult<byte[]> command = PanasonicHelper.BuildWriteCommand( station, address, value );
			if (!command.IsSuccess) return command;

			// 数据交互
			OperateResult<byte[]> read = plc.ReadFromCoreServer( command.Content );
			if (!read.IsSuccess) return read;

			// 提取结果
			return PanasonicHelper.ExtraActualData( read.Content );
		}
#if !NET35 && !NET20
		/// <inheritdoc cref="ReadBool(IReadWriteDevice, byte, string)"/>
		public static async Task<OperateResult<bool>> ReadBoolAsync( IReadWriteDevice plc, byte station, string address )
		{
			station = (byte)HslHelper.ExtractParameter( ref address, "s", station );

			// 创建指令
			OperateResult<byte[]> command = PanasonicHelper.BuildReadOneCoil( station, address );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<bool>( command );

			// 数据交互
			OperateResult<byte[]> read = await plc.ReadFromCoreServerAsync( command.Content );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool>( read );

			// 提取数据
			return ByteTransformHelper.GetResultFromArray( PanasonicHelper.ExtraActualBool( read.Content ) );
		}

		/// <inheritdoc cref="ReadBool(IReadWriteDevice, byte, string, ushort)"/>
		public static async Task<OperateResult<bool[]>> ReadBoolAsync( IReadWriteDevice plc, byte station, string address, ushort length )
		{
			station = (byte)HslHelper.ExtractParameter( ref address, "s", station );

			OperateResult<string, int> analysis = PanasonicHelper.AnalysisAddress( address );
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( analysis );

			// 创建指令
			OperateResult<List<byte[]>> command = PanasonicHelper.BuildReadCommand( station, address, length, true );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( command );

			List<byte> list = new List<byte>( );
			for (int i = 0; i < command.Content.Count; i++)
			{
				// 数据交互
				OperateResult<byte[]> read = await plc.ReadFromCoreServerAsync( command.Content[i] );
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( read );

				// 提取数据
				OperateResult<byte[]> extra = PanasonicHelper.ExtraActualData( read.Content );
				if (!extra.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( extra );

				// 提取bool
				list.AddRange( extra.Content );
			}

			return OperateResult.CreateSuccessResult( list.ToArray( ).ToBoolArray( ).SelectMiddle( analysis.Content2 % 16, length ) );
		}

		/// <inheritdoc cref="ReadBool(IReadWriteDevice, byte, string[])"/>
		public static async Task<OperateResult<bool[]>> ReadBoolAsync( IReadWriteDevice plc, byte station, string[] address )
		{
			// 创建指令
			OperateResult<List<byte[]>> command = PanasonicHelper.BuildReadCoils( station, address );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( command );

			List<bool> list = new List<bool>( );
			for (int i = 0; i < command.Content.Count; i++)
			{
				// 数据交互
				OperateResult<byte[]> read = await plc.ReadFromCoreServerAsync( command.Content[i] );
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( read );

				// 提取数据
				OperateResult<bool[]> extra = PanasonicHelper.ExtraActualBool( read.Content );
				if (!extra.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( extra );

				list.AddRange( extra.Content );
			}

			return OperateResult.CreateSuccessResult( list.ToArray( ) );
		}

		/// <inheritdoc cref="Write(IReadWriteDevice, byte, string, bool)"/>
		public static async Task<OperateResult> WriteAsync( IReadWriteDevice plc, byte station, string address, bool value )
		{
			station = (byte)HslHelper.ExtractParameter( ref address, "s", station );

			// 创建指令
			OperateResult<byte[]> command = PanasonicHelper.BuildWriteOneCoil( station, address, value );
			if (!command.IsSuccess) return command;

			// 数据交互
			OperateResult<byte[]> read = await plc.ReadFromCoreServerAsync( command.Content );
			if (!read.IsSuccess) return read;

			// 提取结果
			return PanasonicHelper.ExtraActualData( read.Content );
		}

		/// <inheritdoc cref="Write(IReadWriteDevice, byte, string, bool[])"/>
		public static async Task<OperateResult> WriteAsync( IReadWriteDevice plc, byte station, string address, bool[] values )
		{
			station = (byte)HslHelper.ExtractParameter( ref address, "s", station );

			// 强制地址从字单位开始，强制写入长度为16个长度
			OperateResult<string, int> analysis = PanasonicHelper.AnalysisAddress( address );
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( analysis );

			if (analysis.Content2 % 16 != 0) return new OperateResult( StringResources.Language.PanasonicAddressBitStartMulti16 );
			if (values.Length % 16 != 0) return new OperateResult( StringResources.Language.PanasonicBoolLengthMulti16 );

			// 计算字节数据
			byte[] buffer = BasicFramework.SoftBasic.BoolArrayToByte( values );

			// 创建指令
			OperateResult<byte[]> command = PanasonicHelper.BuildWriteCommand( station, address, buffer );
			if (!command.IsSuccess) return command;

			// 数据交互
			OperateResult<byte[]> read = await plc.ReadFromCoreServerAsync( command.Content );
			if (!read.IsSuccess) return read;

			// 提取结果
			return PanasonicHelper.ExtraActualData( read.Content );
		}

		/// <inheritdoc cref="Write(IReadWriteDevice, byte, string[], bool[])"/>
		public static async Task<OperateResult> WriteAsync( IReadWriteDevice plc, byte station, string[] address, bool[] value )
		{
			// 创建指令
			OperateResult<List<byte[]>> command = PanasonicHelper.BuildWriteCoils( station, address, value );
			if (!command.IsSuccess) return command;

			for (int i = 0; i < command.Content.Count; i++)
			{
				// 数据交互
				OperateResult<byte[]> read = await plc.ReadFromCoreServerAsync( command.Content[i] );
				if (!read.IsSuccess) return read;

				// 提取结果
				OperateResult extra = PanasonicHelper.ExtraActualData( read.Content );
				if (!extra.IsSuccess) return extra;
			}

			return OperateResult.CreateSuccessResult( );
		}

		/// <inheritdoc cref="Read(IReadWriteDevice, byte, string, ushort)"/>
		public static async Task<OperateResult<byte[]>> ReadAsync( IReadWriteDevice plc, byte station, string address, ushort length )
		{
			station = (byte)HslHelper.ExtractParameter( ref address, "s", station );

			// 创建指令
			OperateResult<List<byte[]>> command = PanasonicHelper.BuildReadCommand( station, address, length, false );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( command );

			List<byte> list = new List<byte>( );
			for (int i = 0; i < command.Content.Count; i++)
			{
				// 数据交互
				OperateResult<byte[]> read = await plc.ReadFromCoreServerAsync( command.Content[i] );
				if (!read.IsSuccess) return read;

				// 提取数据
				OperateResult<byte[]> extra = PanasonicHelper.ExtraActualData( read.Content );
				if (!extra.IsSuccess) return extra;

				list.AddRange( extra.Content );
			}

			return OperateResult.CreateSuccessResult( list.ToArray( ) );
		}

		/// <inheritdoc cref="Write(IReadWriteDevice, byte, string, byte[])"/>
		public static async Task<OperateResult> WriteAsync( IReadWriteDevice plc, byte station, string address, byte[] value )
		{
			station = (byte)HslHelper.ExtractParameter( ref address, "s", station );

			// 创建指令
			OperateResult<byte[]> command = PanasonicHelper.BuildWriteCommand( station, address, value );
			if (!command.IsSuccess) return command;

			// 数据交互
			OperateResult<byte[]> read = await plc.ReadFromCoreServerAsync( command.Content );
			if (!read.IsSuccess) return read;

			// 提取结果
			return PanasonicHelper.ExtraActualData( read.Content );
		}
#endif

	}
}
