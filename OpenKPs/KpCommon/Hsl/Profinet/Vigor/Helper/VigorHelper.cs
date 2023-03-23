using HslCommunication.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if !NET35 && !NET20
using System.Threading.Tasks;
#endif

namespace HslCommunication.Profinet.Vigor.Helper
{
	/// <summary>
	/// 丰炜PLC的辅助方法
	/// </summary>
	public class VigorHelper
	{
		/// <inheritdoc cref="IReadWriteNet.Read(string, ushort)"/>
		/// <remarks>
		/// 支持字地址，单次最多读取64字节，支持D,SD,R,T,C的数据读取，同时地址支持携带站号信息，s=2;D100
		/// </remarks>
		public static OperateResult<byte[]> Read( IReadWriteDevice plc, byte station, string address, ushort length )
		{
			byte stat = (byte)HslHelper.ExtractParameter( ref address, "s", station );

			// 解析指令
			OperateResult<List<byte[]>> command = VigorVsHelper.BuildReadCommand( stat, address, length, false );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( command );

			List<byte> result = new List<byte>( );
			for (int i = 0; i < command.Content.Count; i++)
			{
				// 核心交互
				OperateResult<byte[]> read = plc.ReadFromCoreServer( command.Content[i] );
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( read );

				// 结果验证
				OperateResult<byte[]> check = VigorVsHelper.CheckResponseContent( read.Content );
				if (!check.IsSuccess) return check;

				result.AddRange( check.Content );
			}
			// 提取结果
			return OperateResult.CreateSuccessResult( result.ToArray( ) );
		}

		/// <inheritdoc cref="IReadWriteNet.ReadBool(string, ushort)"/>
		/// <remarks>
		/// 需要输入位地址，最多读取1024位，支持X,Y,M,SM,S,TS(定时器触点),TC（定时器线圈）,CS(计数器触点),CC（计数器线圈）
		/// </remarks>
		public static OperateResult<bool[]> ReadBool( IReadWriteDevice plc, byte station, string address, ushort length )
		{
			byte stat = (byte)HslHelper.ExtractParameter( ref address, "s", station );

			// 解析指令
			OperateResult<List<byte[]>> command = VigorVsHelper.BuildReadCommand( stat, address, length, true );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( command );

			List<bool> result = new List<bool>( );
			for (int i = 0;i < command.Content.Count; i++)
			{
				// 核心交互
				OperateResult<byte[]> read = plc.ReadFromCoreServer( command.Content[i] );
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( read );

				// 结果验证
				OperateResult<byte[]> check = VigorVsHelper.CheckResponseContent( read.Content );
				if (!check.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( check );

				result.AddRange( check.Content.ToBoolArray( ).SelectBegin( length ) );
			}
			// 提取结果
			return OperateResult.CreateSuccessResult( result.ToArray( ) );
		}

		/// <inheritdoc cref="IReadWriteNet.Write(string, byte[])"/>
		/// <remarks>
		/// 支持字地址，单次最多读取64字节，支持D,SD,R,T,C的数据写入，其中C199~C200不能连续写入，前者是16位计数器，后者是32位计数器
		/// </remarks>
		public static OperateResult Write( IReadWriteDevice plc, byte station, string address, byte[] value )
		{
			byte stat = (byte)HslHelper.ExtractParameter( ref address, "s", station );

			// 解析指令
			OperateResult<byte[]> command = VigorVsHelper.BuildWriteWordCommand( stat, address, value );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( command );

			// 核心交互
			OperateResult<byte[]> read = plc.ReadFromCoreServer( command.Content );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( read );

			// 结果验证
			return VigorVsHelper.CheckResponseContent( read.Content );
		}

		/// <inheritdoc cref="IReadWriteNet.Write(string, bool[])"/>
		/// <remarks>
		/// 支持位地址的写入，支持X,Y,M,SM,S,TS(定时器触点),TC（定时器线圈）,CS(计数器触点),CC（计数器线圈）
		/// </remarks>
		public static OperateResult Write( IReadWriteDevice plc, byte station, string address, bool[] value )
		{
			byte stat = (byte)HslHelper.ExtractParameter( ref address, "s", station );

			// 解析指令
			OperateResult<byte[]> command = VigorVsHelper.BuildWriteBoolCommand( stat, address, value );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( command );

			// 核心交互
			OperateResult<byte[]> read = plc.ReadFromCoreServer( command.Content );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( read );

			// 结果验证
			return VigorVsHelper.CheckResponseContent( read.Content );
		}

#if !NET35 && !NET20
		/// <inheritdoc cref="Read(IReadWriteDevice, byte, string, ushort)"/>
		public async static Task<OperateResult<byte[]>> ReadAsync( IReadWriteDevice plc, byte station, string address, ushort length )
		{
			byte stat = (byte)HslHelper.ExtractParameter( ref address, "s", station );

			// 解析指令
			OperateResult<List<byte[]>> command = VigorVsHelper.BuildReadCommand( stat, address, length, false );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( command );

			List<byte> result = new List<byte>( );
			for (int i = 0; i < command.Content.Count; i++)
			{
				// 核心交互
				OperateResult<byte[]> read = await plc.ReadFromCoreServerAsync( command.Content[i] );
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( read );

				// 结果验证
				OperateResult<byte[]> check = VigorVsHelper.CheckResponseContent( read.Content );
				if (!check.IsSuccess) return check;

				result.AddRange( check.Content );
			}
			// 提取结果
			return OperateResult.CreateSuccessResult( result.ToArray( ) );
		}

		/// <inheritdoc cref="ReadBool(IReadWriteDevice, byte, string, ushort)"/>
		public async static Task<OperateResult<bool[]>> ReadBoolAsync( IReadWriteDevice plc, byte station, string address, ushort length )
		{
			byte stat = (byte)HslHelper.ExtractParameter( ref address, "s", station );

			// 解析指令
			OperateResult<List<byte[]>> command = VigorVsHelper.BuildReadCommand( stat, address, length, true );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( command );

			List<bool> result = new List<bool>( );
			for (int i = 0; i < command.Content.Count; i++)
			{
				// 核心交互
				OperateResult<byte[]> read = await plc.ReadFromCoreServerAsync( command.Content[i] );
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( read );

				// 结果验证
				OperateResult<byte[]> check = VigorVsHelper.CheckResponseContent( read.Content );
				if (!check.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( check );

				result.AddRange( check.Content.ToBoolArray( ).SelectBegin( length ) );
			}
			// 提取结果
			return OperateResult.CreateSuccessResult( result.ToArray( ) );
		}

		/// <inheritdoc cref="Write(IReadWriteDevice, byte, string, byte[])"/>
		public async static Task<OperateResult> WriteAsync( IReadWriteDevice plc, byte station, string address, byte[] value )
		{
			byte stat = (byte)HslHelper.ExtractParameter( ref address, "s", station );

			// 解析指令
			OperateResult<byte[]> command = VigorVsHelper.BuildWriteWordCommand( stat, address, value );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( command );

			// 核心交互
			OperateResult<byte[]> read = await plc.ReadFromCoreServerAsync( command.Content );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( read );

			// 结果验证
			return VigorVsHelper.CheckResponseContent( read.Content );
		}

		/// <inheritdoc cref="Write(IReadWriteDevice, byte, string, bool[])"/>
		public async static Task<OperateResult> WriteAsync( IReadWriteDevice plc, byte station, string address, bool[] value )
		{
			byte stat = (byte)HslHelper.ExtractParameter( ref address, "s", station );

			// 解析指令
			OperateResult<byte[]> command = VigorVsHelper.BuildWriteBoolCommand( stat, address, value );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( command );

			// 核心交互
			OperateResult<byte[]> read = await plc.ReadFromCoreServerAsync( command.Content );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( read );

			// 结果验证
			return VigorVsHelper.CheckResponseContent( read.Content );
		}

#endif
	}

}
