﻿using HslCommunication.BasicFramework;
using HslCommunication.Core;
using HslCommunication.Core.IMessage;
using HslCommunication.Core.Net;
using HslCommunication.Reflection;
using HslCommunication.Core.Address;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
#if !NET20 && !NET35
using System.Threading.Tasks;
#endif

namespace HslCommunication.Profinet.Keyence
{
	/// <summary>
	/// 基恩士的上位链路协议的虚拟服务器
	/// </summary>
	public class KeyenceNanoServer : NetworkDataServerBase
	{
		#region Constructor

		/// <summary>
		/// 实例化一个基于上位链路协议的虚拟的基恩士PLC对象，可以用来和<see cref="KeyenceNanoSerialOverTcp"/>进行通信测试。
		/// </summary>
		public KeyenceNanoServer( )
		{
			rBuffer  = new SoftBuffer( DataPoolLength );
			bBuffer  = new SoftBuffer( DataPoolLength );
			mrBuffer = new SoftBuffer( DataPoolLength );
			lrBuffer = new SoftBuffer( DataPoolLength );
			crBuffer = new SoftBuffer( DataPoolLength );
			vbBuffer = new SoftBuffer( DataPoolLength );
			dmBuffer = new SoftBuffer( DataPoolLength * 2 );
			emBuffer = new SoftBuffer( DataPoolLength * 2 );
			wBuffer  = new SoftBuffer( DataPoolLength * 2 );
			atBuffer = new SoftBuffer( DataPoolLength );

			ByteTransform = new RegularByteTransform( );
			WordLength = 1;
			LogMsgFormatBinary = false;
		}

		#endregion

		#region Data Persistence

		/// <inheritdoc/>
		protected override byte[] SaveToBytes( )
		{
			byte[] buffer = new byte[DataPoolLength * 13];
			rBuffer. GetBytes( ).CopyTo( buffer, DataPoolLength * 0 );
			bBuffer. GetBytes( ).CopyTo( buffer, DataPoolLength * 1 );
			mrBuffer.GetBytes( ).CopyTo( buffer, DataPoolLength * 2 );
			lrBuffer.GetBytes( ).CopyTo( buffer, DataPoolLength * 3 );
			crBuffer.GetBytes( ).CopyTo( buffer, DataPoolLength * 4 );
			vbBuffer.GetBytes( ).CopyTo( buffer, DataPoolLength * 5 );
			dmBuffer.GetBytes( ).CopyTo( buffer, DataPoolLength * 6 );
			emBuffer.GetBytes( ).CopyTo( buffer, DataPoolLength * 8 );
			wBuffer. GetBytes( ).CopyTo( buffer, DataPoolLength * 10 );
			atBuffer.GetBytes( ).CopyTo( buffer, DataPoolLength * 12 );
			return buffer;
		}

		/// <inheritdoc/>
		protected override void LoadFromBytes( byte[] content )
		{
			if (content.Length < DataPoolLength * 13) throw new Exception( "File is not correct" );
			rBuffer. SetBytes( content,  0 * DataPoolLength, DataPoolLength * 1 );
			bBuffer. SetBytes( content,  1 * DataPoolLength, DataPoolLength * 1 );
			mrBuffer.SetBytes( content,  2 * DataPoolLength, DataPoolLength * 1 );
			lrBuffer.SetBytes( content,  3 * DataPoolLength, DataPoolLength * 1 );
			crBuffer.SetBytes( content,  4 * DataPoolLength, DataPoolLength * 1 );
			vbBuffer.SetBytes( content,  5 * DataPoolLength, DataPoolLength * 1 );
			dmBuffer.SetBytes( content,  6 * DataPoolLength, DataPoolLength * 2 );
			emBuffer.SetBytes( content,  8 * DataPoolLength, DataPoolLength * 2 );
			wBuffer. SetBytes( content, 10 * DataPoolLength, DataPoolLength * 2 );
			atBuffer.SetBytes( content, 12 * DataPoolLength, DataPoolLength * 1 );
		}

		#endregion

		#region NetworkDataServerBase Override

		/// <inheritdoc cref="KeyenceNanoSerialOverTcp.Read(string, ushort)"/>
		[HslMqttApi( "ReadByteArray", "" )]
		public override OperateResult<byte[]> Read( string address, ushort length )
		{
			OperateResult<KeyenceNanoAddress> nanoAddress = KeyenceNanoAddress.ParseFrom( address, length );
			if (!nanoAddress.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( nanoAddress );

			try
			{
				if (address.StartsWith( "DM" )) return OperateResult.CreateSuccessResult( dmBuffer.GetBytes( nanoAddress.Content.AddressStart * 2, length * 2 ) );
				if (address.StartsWith( "EM" )) return OperateResult.CreateSuccessResult( emBuffer.GetBytes( nanoAddress.Content.AddressStart * 2, length * 2 ) ); 
				if (address.StartsWith( "W" ))  return OperateResult.CreateSuccessResult( wBuffer.GetBytes(  nanoAddress.Content.AddressStart * 2, length * 2 ) ); 
				if (address.StartsWith( "AT" )) return OperateResult.CreateSuccessResult( atBuffer.GetBytes( nanoAddress.Content.AddressStart * 4, length * 4 ) );
				return new OperateResult<byte[]>( StringResources.Language.NotSupportedDataType ); 
			}
			catch(Exception ex)
			{
				return new OperateResult<byte[]>( StringResources.Language.NotSupportedDataType + " Reason:" + ex.Message );
			}
		}

		/// <inheritdoc cref="KeyenceNanoSerialOverTcp.Write(string, byte[])"/>
		[HslMqttApi( "WriteByteArray", "" )]
		public override OperateResult Write( string address, byte[] value )
		{
			OperateResult<KeyenceNanoAddress> nanoAddress = KeyenceNanoAddress.ParseFrom( address, 0 );
			if (!nanoAddress.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( nanoAddress );

			try
			{
				if      (address.StartsWith( "DM" )) dmBuffer.SetBytes( value,  nanoAddress.Content.AddressStart * 2 );
				else if (address.StartsWith( "EM" )) emBuffer.SetBytes( value,  nanoAddress.Content.AddressStart * 2 );
				else if (address.StartsWith( "W" ))  wBuffer. SetBytes(  value, nanoAddress.Content.AddressStart * 2 );
				else if (address.StartsWith( "AT" )) atBuffer.SetBytes( value,  nanoAddress.Content.AddressStart * 4 );
				else return new OperateResult<byte[]>( StringResources.Language.NotSupportedDataType );
				return OperateResult.CreateSuccessResult( );
			}
			catch(Exception ex)
			{
				return new OperateResult<byte[]>( StringResources.Language.NotSupportedDataType + " Reason:" + ex.Message );
			}
		}

		/// <inheritdoc cref="IReadWriteNet.ReadBool(string, ushort)"/>
		[HslMqttApi( "ReadBoolArray", "" )]
		public override OperateResult<bool[]> ReadBool( string address, ushort length )
		{
			OperateResult<KeyenceNanoAddress> nanoAddress = KeyenceNanoAddress.ParseFrom( address, 0 );
			if (!nanoAddress.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( nanoAddress );

			try
			{
				if (address.StartsWith( "R" ))  return OperateResult.CreateSuccessResult( rBuffer.GetBytes(  nanoAddress.Content.AddressStart, length ).Select( m => m != 0 ).ToArray( ) );
				if (address.StartsWith( "B" ))  return OperateResult.CreateSuccessResult( bBuffer.GetBytes(  nanoAddress.Content.AddressStart, length ).Select( m => m != 0 ).ToArray( ) );
				if (address.StartsWith( "MR" )) return OperateResult.CreateSuccessResult( mrBuffer.GetBytes( nanoAddress.Content.AddressStart, length ).Select( m => m != 0 ).ToArray( ) );
				if (address.StartsWith( "LR" )) return OperateResult.CreateSuccessResult( lrBuffer.GetBytes( nanoAddress.Content.AddressStart, length ).Select( m => m != 0 ).ToArray( ) );
				if (address.StartsWith( "CR" )) return OperateResult.CreateSuccessResult( crBuffer.GetBytes( nanoAddress.Content.AddressStart, length ).Select( m => m != 0 ).ToArray( ) );
				if (address.StartsWith( "VB" )) return OperateResult.CreateSuccessResult( vbBuffer.GetBytes( nanoAddress.Content.AddressStart, length ).Select( m => m != 0 ).ToArray( ) );
				return new OperateResult<bool[]>( StringResources.Language.NotSupportedDataType );
			}
			catch(Exception ex)
			{
				return new OperateResult<bool[]>( StringResources.Language.NotSupportedDataType + " Reason:" + ex.Message );
			}
		}

		/// <inheritdoc cref="IReadWriteNet.Write(string, bool[])"/>
		[HslMqttApi( "WriteBoolArray", "" )]
		public override OperateResult Write( string address, bool[] value )
		{
			OperateResult<KeyenceNanoAddress> nanoAddress = KeyenceNanoAddress.ParseFrom( address, 0 );
			if (!nanoAddress.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( nanoAddress );

			try
			{
				byte[] values = value.Select( m => m ? (byte)1 : (byte)0 ).ToArray( );

				if      (address.StartsWith( "R" ))  rBuffer. SetBytes( values, nanoAddress.Content.AddressStart );
				else if (address.StartsWith( "B" ))  bBuffer. SetBytes( values, nanoAddress.Content.AddressStart );
				else if (address.StartsWith( "MR" )) mrBuffer.SetBytes( values, nanoAddress.Content.AddressStart );
				else if (address.StartsWith( "LR" )) lrBuffer.SetBytes( values, nanoAddress.Content.AddressStart );
				else if (address.StartsWith( "CR" )) crBuffer.SetBytes( values, nanoAddress.Content.AddressStart );
				else if (address.StartsWith( "VB" )) vbBuffer.SetBytes( values, nanoAddress.Content.AddressStart );
				else return new OperateResult<bool[]>( StringResources.Language.NotSupportedDataType );
				return OperateResult.CreateSuccessResult( );
			}
			catch (Exception ex)
			{
				return new OperateResult<bool[]>( StringResources.Language.NotSupportedDataType + " Reason:" + ex.Message );
			}
		}

		#endregion

		#region NetServer Override

		/// <inheritdoc/>
		protected override INetMessage GetNewNetMessage( ) => new SpecifiedCharacterMessage( AsciiControl.CR );

		/// <inheritdoc/>
		protected override void ThreadPoolLoginAfterClientCheck( Socket socket, System.Net.IPEndPoint endPoint )
		{
			// 开始接收数据信息，先接收CR\r的命令
			OperateResult<byte[]> read = ReceiveByMessage( socket, 5000, GetNewNetMessage( ) );
			if(!read.IsSuccess) { socket?.Close( ); return; }

			// 过滤不是CR开头的指令
			if(!Encoding.ASCII.GetString(read.Content).StartsWith("CR")) { socket?.Close( ); return; }

			OperateResult send = Send( socket, Encoding.ASCII.GetBytes( "CC\r\n" ) );
			if(!send.IsSuccess) { return; }

			base.ThreadPoolLoginAfterClientCheck( socket, endPoint );
		}

		/// <inheritdoc/>
		protected override OperateResult<byte[]> ReadFromCoreServer( AppSession session, byte[] receive )
		{
			return OperateResult.CreateSuccessResult( ReadFromNanoCore( receive ) );
		}

		#endregion

		#region Core Read

		private byte[] GetBoolResponseData( byte[] data )
		{
			StringBuilder sb = new StringBuilder( );
			for (int i = 0; i < data.Length; i++)
			{
				sb.Append( data[i] );
				if (i != data.Length - 1) sb.Append( " " );
			}
			sb.Append( "\r\n" );
			return Encoding.ASCII.GetBytes( sb.ToString( ) );
		}

		private byte[] GetWordResponseData( byte[] data )
		{
			StringBuilder sb = new StringBuilder( ); 
			for (int i = 0; i < data.Length / 2; i++)
			{
				sb.Append( BitConverter.ToUInt16( data, i * 2 ) );
				if (i != data.Length / 2 - 1) sb.Append( " " );
			}
			sb.Append( "\r\n" );
			return Encoding.ASCII.GetBytes( sb.ToString( ) );
		}

		private byte[] GetDoubleWordResponseData( byte[] data )
		{
			StringBuilder sb = new StringBuilder( );
			for (int i = 0; i < data.Length / 4; i++)
			{
				sb.Append( BitConverter.ToUInt32( data, i * 4 ) );
				if (i != data.Length / 4 - 1) sb.Append( " " );
			}
			sb.Append( "\r\n" );
			return Encoding.ASCII.GetBytes( sb.ToString( ) );
		}

		private byte[] ReadFromNanoCore( byte[] receive )
		{
			string[] cmds = Encoding.ASCII.GetString( receive ).Trim( '\r', '\n' ).Split( new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries );
			if (cmds[0] == "CR") return Encoding.ASCII.GetBytes( "CC\r\n" );
			if (cmds[0] == "CQ") return Encoding.ASCII.GetBytes( "CF\r\n" );
			if (cmds[0] == "ER") return Encoding.ASCII.GetBytes( "OK\r\n" );
			if (cmds[0] == "RD" || cmds[0] == "RDS") return ReadByCommand( cmds );  // 读取数据和连续读取数据
			if (cmds[0] == "WR" || cmds[0] == "WRS") return WriteByCommand( cmds ); // 写入数据和连续写入数据
			if (cmds[0] == "ST") return WriteByCommand( new string[] { "WRS", cmds[1], "1", "1" } );
			if (cmds[0] == "RS") return WriteByCommand( new string[] { "WRS", cmds[1], "1", "0" } );
			if (cmds[0] == "?K") return Encoding.ASCII.GetBytes( "53\r\n" );
			if (cmds[0] == "?M") return Encoding.ASCII.GetBytes( "1\r\n" );
			return Encoding.ASCII.GetBytes( "E0\r\n" );
		}

		private byte[] ReadByCommand( string[] command )
		{
			try
			{
				if (command[1].EndsWith( ".U" ) || command[1].EndsWith( ".S" ) || command[1].EndsWith( ".D" ) ||
					command[1].EndsWith( ".L" ) || command[1].EndsWith( ".H" )) command[1] = command[1].Remove( command[1].Length - 2 );
				int length = command.Length > 2 ? int.Parse( command[2] ) : 1;
				if (System.Text.RegularExpressions.Regex.IsMatch( command[1], "^[0-9]+$" )) command[1] = "R" + command[1];  // 如果不指定数据块，就默认为R

				OperateResult<KeyenceNanoAddress> address = KeyenceNanoAddress.ParseFrom( command[1], (ushort)length );
				if (!address.IsSuccess) return Encoding.ASCII.GetBytes( "E0\r\n" );

				KeyenceNanoAddress nanoAddress = address.Content;
				if (length > nanoAddress.SplitLength) return Encoding.ASCII.GetBytes( "E0\r\n" ); // 读取的长度太大，返回错误信息

				switch (nanoAddress.DataCode)
				{
					case ""   :
					case "R"  : return GetBoolResponseData(       rBuffer.GetBytes(  nanoAddress.AddressStart, length ) );
					case "B"  : return GetBoolResponseData(       bBuffer.GetBytes(  nanoAddress.AddressStart, length ) );
					case "MR" : return GetBoolResponseData(       mrBuffer.GetBytes( nanoAddress.AddressStart, length ) );
					case "LR" : return GetBoolResponseData(       lrBuffer.GetBytes( nanoAddress.AddressStart, length ) );
					case "CR" : return GetBoolResponseData(       crBuffer.GetBytes( nanoAddress.AddressStart, length ) );
					case "VB" : return GetBoolResponseData(       vbBuffer.GetBytes( nanoAddress.AddressStart, length ) );
					case "DM" : return GetWordResponseData(       dmBuffer.GetBytes( nanoAddress.AddressStart * 2, length * 2 ) );
					case "EM" : return GetWordResponseData(       emBuffer.GetBytes( nanoAddress.AddressStart * 2, length * 2 ) ); 
					case "W"  : return GetWordResponseData(       wBuffer.GetBytes(  nanoAddress.AddressStart * 2, length * 2 ) );
					case "AT" : return GetDoubleWordResponseData( atBuffer.GetBytes( nanoAddress.AddressStart * 4, length * 4 ) );
					default   : return Encoding.ASCII.GetBytes( "E0\r\n" );
				}
			}
			catch
			{
				return Encoding.ASCII.GetBytes( "E1\r\n" );
			}
		}

		private byte[] WriteByCommand( string[] command )
		{
			// 先判断是否有写入的权利，没有的话，直接返回写入异常
			if (!this.EnableWrite) return Encoding.ASCII.GetBytes( "E4\r\n" );

			try
			{
				if (command[1].EndsWith( ".U" ) || command[1].EndsWith( ".S" ) || command[1].EndsWith( ".D" ) ||
					command[1].EndsWith( ".L" ) || command[1].EndsWith( ".H" ))
					command[1] = command[1].Remove( command[1].Length - 2 );
				int length = command[0] == "WRS" ? int.Parse( command[2] ) : 1;
				if (System.Text.RegularExpressions.Regex.IsMatch( command[1], "^[0-9]+$" )) command[1] = "R" + command[1];  // 如果不指定数据块，就默认为R

				OperateResult<KeyenceNanoAddress> address = KeyenceNanoAddress.ParseFrom( command[1], (ushort)length );
				if (!address.IsSuccess) return Encoding.ASCII.GetBytes( "E0\r\n" );

				KeyenceNanoAddress nanoAddress = address.Content;
				if (length > nanoAddress.SplitLength) return Encoding.ASCII.GetBytes( "E0\r\n" ); // 读取的长度太大，返回错误信息

				if ( command[1].StartsWith( "R" )  || command[1].StartsWith( "B" )  || command[1].StartsWith( "MR" ) || 
					command[1].StartsWith( "LR" ) || command[1].StartsWith( "CR" ) || command[1].StartsWith( "VB" ))
				{
					byte[] values = command.RemoveBegin( command[0] == "WRS" ? 3 : 2 ).Select( m => byte.Parse( m ) ).ToArray( );

					if      (command[1].StartsWith( "R" ))  rBuffer. SetBytes( values, nanoAddress.AddressStart );
					else if (command[1].StartsWith( "B" ))  bBuffer. SetBytes( values, nanoAddress.AddressStart );
					else if (command[1].StartsWith( "MR" )) mrBuffer.SetBytes( values, nanoAddress.AddressStart );
					else if (command[1].StartsWith( "LR" )) lrBuffer.SetBytes( values, nanoAddress.AddressStart );
					else if (command[1].StartsWith( "CR" )) crBuffer.SetBytes( values, nanoAddress.AddressStart );
					else if (command[1].StartsWith( "VB" )) vbBuffer.SetBytes( values, nanoAddress.AddressStart );
					else return Encoding.ASCII.GetBytes( "E0\r\n" );
				}
				else
				{
					byte[] values = ByteTransform.TransByte( command.RemoveBegin( command[0] == "WRS" ? 3 : 2 ).Select( m => ushort.Parse( m ) ).ToArray( ) );

					if      (command[1].StartsWith( "DM" )) dmBuffer.SetBytes( values, nanoAddress.AddressStart * 2 );
					else if (command[1].StartsWith( "EM" )) emBuffer.SetBytes( values, nanoAddress.AddressStart * 2 );
					else if (command[1].StartsWith( "W" ))  wBuffer.SetBytes(  values, nanoAddress.AddressStart * 2 );
					else if (command[1].StartsWith( "AT" )) atBuffer.SetBytes( values, nanoAddress.AddressStart * 4 );
					else return Encoding.ASCII.GetBytes( "E0\r\n" );
				}

				return Encoding.ASCII.GetBytes( "OK\r\n" );
			}
			catch
			{
				return Encoding.ASCII.GetBytes( "E1\r\n" );
			}
		}

		#endregion

		#region Serial Support

		/// <inheritdoc/>
		protected override bool CheckSerialReceiveDataComplete( byte[] buffer, int receivedLength )
		{
			return buffer[receivedLength - 1] == AsciiControl.CR;
		}

		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString( ) => $"KeyenceNanoServer[{Port}]";

		#endregion

		#region Private Member

		private SoftBuffer rBuffer;       // 继电器的数据池
		private SoftBuffer bBuffer;       // 链路继电器的数据池
		private SoftBuffer mrBuffer;      // 内部辅助继电器的数据池
		private SoftBuffer lrBuffer;      // 锁存继电器的数据池
		private SoftBuffer crBuffer;      // 控制继电器
		private SoftBuffer vbBuffer;      // 工作继电器
		private SoftBuffer dmBuffer;      // 数据寄存器
		private SoftBuffer emBuffer;      // 扩展数据寄存器
		private SoftBuffer wBuffer;       // 链路寄存器
		private SoftBuffer atBuffer;      // 数字微调器


		private const int DataPoolLength = 65536;     // 数据的长度

		#endregion

	}
}
