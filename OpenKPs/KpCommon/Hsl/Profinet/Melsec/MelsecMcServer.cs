using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using HslCommunication.BasicFramework;
using HslCommunication.Core;
using HslCommunication.Core.Net;
using HslCommunication.Core.IMessage;
using HslCommunication.Core.Address;
using HslCommunication.Reflection;
using System.IO;

namespace HslCommunication.Profinet.Melsec
{
	/// <summary>
	/// <b>[商业授权]</b> 三菱MC协议的虚拟服务器，支持M,X,Y,D,W的数据池读写操作，支持二进制及ASCII格式进行读写操作，需要在实例化的时候指定。<br />
	/// <b>[Authorization]</b> The Mitsubishi MC protocol virtual server supports M, X, Y, D, W data pool read and write operations, 
	/// and supports binary and ASCII format read and write operations, which need to be specified during instantiation.
	/// </summary>
	/// <remarks>
	/// 本三菱的虚拟PLC仅限商业授权用户使用，感谢支持。
	/// 如果你没有可以测试的三菱PLC，想要测试自己开发的上位机软件，或是想要在本机实现虚拟PLC，然后进行IO的输入输出练习，都可以使用本类来实现，先来说明下地址信息
	/// <br />
	/// 地址的输入的格式说明如下：
	/// <list type="table">
	///   <listheader>
	///     <term>地址名称</term>
	///     <term>地址代号</term>
	///     <term>示例</term>
	///     <term>地址进制</term>
	///     <term>字操作</term>
	///     <term>位操作</term>
	///     <term>备注</term>
	///   </listheader>
	///   <item>
	///     <term>内部继电器</term>
	///     <term>M</term>
	///     <term>M100,M200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>输入继电器</term>
	///     <term>X</term>
	///     <term>X100,X1A0</term>
	///     <term>16</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>输出继电器</term>
	///     <term>Y</term>
	///     <term>Y100,Y1A0</term>
	///     <term>16</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>数据寄存器</term>
	///     <term>D</term>
	///     <term>D1000,D2000</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>×</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>链接寄存器</term>
	///     <term>W</term>
	///     <term>W100,W1A0</term>
	///     <term>16</term>
	///     <term>√</term>
	///     <term>×</term>
	///     <term></term>
	///   </item>
	/// </list>
	/// </remarks>
	public class MelsecMcServer : NetworkDataServerBase
	{
		#region Constructor

		/// <summary>
		/// 实例化一个默认参数的mc协议的服务器<br />
		/// Instantiate a mc protocol server with default parameters
		/// </summary>
		/// <param name="isBinary">是否是二进制，默认是二进制，否则是ASCII格式</param>
		public MelsecMcServer( bool isBinary = true )
		{
			// 共计使用了五个数据池
			xBuffer  = new SoftBuffer( DataPoolLength );
			yBuffer  = new SoftBuffer( DataPoolLength );
			mBuffer  = new SoftBuffer( DataPoolLength );
			lBuffer  = new SoftBuffer( DataPoolLength );
			dBuffer  = new SoftBuffer( DataPoolLength * 4 );
			wBuffer  = new SoftBuffer( DataPoolLength * 2 );
			bBuffer  = new SoftBuffer( DataPoolLength );
			rBuffer  = new SoftBuffer( DataPoolLength * 2 );
			zrBuffer = new SoftBuffer( DataPoolLength * 4 );

			this.WordLength         = 1;
			this.ByteTransform      = new RegularByteTransform( );
			this.isBinary           = isBinary;
			this.LogMsgFormatBinary = isBinary;
		}

		#endregion

		#region NetworkDataServerBase Override

		/// <inheritdoc/>
		[HslMqttApi( "ReadByteArray", "" )]
		public override OperateResult<byte[]> Read( string address, ushort length )
		{
			// 分析地址
			OperateResult<McAddressData> analysis = McAddressData.ParseMelsecFrom( address, length );
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( analysis );

			if(analysis.Content.McDataType.DataCode == MelsecMcDataType.M.DataCode)
			{
				bool[] buffer = mBuffer.GetBytes( analysis.Content.AddressStart, length * 16 ).Select( m => m != 0x00 ).ToArray( );
				return OperateResult.CreateSuccessResult( SoftBasic.BoolArrayToByte( buffer ) );
			}
			else if(analysis.Content.McDataType.DataCode == MelsecMcDataType.X.DataCode)
			{
				bool[] buffer = xBuffer.GetBytes( analysis.Content.AddressStart, length * 16 ).Select( m => m != 0x00 ).ToArray( );
				return OperateResult.CreateSuccessResult( SoftBasic.BoolArrayToByte( buffer ) );
			}
			else if (analysis.Content.McDataType.DataCode == MelsecMcDataType.Y.DataCode)
			{
				bool[] buffer = yBuffer.GetBytes( analysis.Content.AddressStart, length * 16 ).Select( m => m != 0x00 ).ToArray( );
				return OperateResult.CreateSuccessResult( SoftBasic.BoolArrayToByte( buffer ) );
			}
			else if (analysis.Content.McDataType.DataCode == MelsecMcDataType.L.DataCode)
			{
				bool[] buffer = lBuffer.GetBytes( analysis.Content.AddressStart, length * 16 ).Select( m => m != 0x00 ).ToArray( );
				return OperateResult.CreateSuccessResult( SoftBasic.BoolArrayToByte( buffer ) );
			}
			else if (analysis.Content.McDataType.DataCode == MelsecMcDataType.B.DataCode)
			{
				bool[] buffer = bBuffer.GetBytes( analysis.Content.AddressStart, length * 16 ).Select( m => m != 0x00 ).ToArray( );
				return OperateResult.CreateSuccessResult( SoftBasic.BoolArrayToByte( buffer ) );
			}
			else if (analysis.Content.McDataType.DataCode == MelsecMcDataType.D.DataCode)
			{
				return OperateResult.CreateSuccessResult( dBuffer.GetBytes( analysis.Content.AddressStart * 2, length * 2 ) );
			}
			else if (analysis.Content.McDataType.DataCode == MelsecMcDataType.W.DataCode)
			{
				return OperateResult.CreateSuccessResult( wBuffer.GetBytes( analysis.Content.AddressStart * 2, length * 2 ) );
			}
			else if (analysis.Content.McDataType.DataCode == MelsecMcDataType.R.DataCode)
			{
				return OperateResult.CreateSuccessResult( rBuffer.GetBytes( analysis.Content.AddressStart * 2, length * 2 ) );
			}
			else if (analysis.Content.McDataType.DataCode == MelsecMcDataType.ZR.DataCode)
			{
				return OperateResult.CreateSuccessResult( zrBuffer.GetBytes( analysis.Content.AddressStart * 2, length * 2 ) );
			}
			else
			{
				return new OperateResult<byte[]>( StringResources.Language.NotSupportedDataType );
			}
		}

		/// <inheritdoc/>
		[HslMqttApi( "WriteByteArray", "" )]
		public override OperateResult Write( string address, byte[] value )
		{
			// 分析地址
			OperateResult<McAddressData> analysis = McAddressData.ParseMelsecFrom( address, 0 );
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( analysis );

			if (analysis.Content.McDataType.DataCode == MelsecMcDataType.M.DataCode)
			{
				byte[] buffer = SoftBasic.ByteToBoolArray( value ).Select( m => m ? (byte)1 : (byte)0 ).ToArray( );
				mBuffer.SetBytes( buffer, analysis.Content.AddressStart );
				return OperateResult.CreateSuccessResult( );
			}
			else if (analysis.Content.McDataType.DataCode == MelsecMcDataType.X.DataCode)
			{
				byte[] buffer = SoftBasic.ByteToBoolArray( value ).Select( m => m ? (byte)1 : (byte)0 ).ToArray( );
				xBuffer.SetBytes( buffer, analysis.Content.AddressStart );
				return OperateResult.CreateSuccessResult( );
			}
			else if (analysis.Content.McDataType.DataCode == MelsecMcDataType.Y.DataCode)
			{
				byte[] buffer = SoftBasic.ByteToBoolArray( value ).Select( m => m ? (byte)1 : (byte)0 ).ToArray( );
				yBuffer.SetBytes( buffer, analysis.Content.AddressStart );
				return OperateResult.CreateSuccessResult( );
			}
			else if (analysis.Content.McDataType.DataCode == MelsecMcDataType.L.DataCode)
			{
				byte[] buffer = SoftBasic.ByteToBoolArray( value ).Select( m => m ? (byte)1 : (byte)0 ).ToArray( );
				lBuffer.SetBytes( buffer, analysis.Content.AddressStart );
				return OperateResult.CreateSuccessResult( );
			}
			else if (analysis.Content.McDataType.DataCode == MelsecMcDataType.B.DataCode)
			{
				byte[] buffer = SoftBasic.ByteToBoolArray( value ).Select( m => m ? (byte)1 : (byte)0 ).ToArray( );
				bBuffer.SetBytes( buffer, analysis.Content.AddressStart );
				return OperateResult.CreateSuccessResult( );
			}
			else if (analysis.Content.McDataType.DataCode == MelsecMcDataType.D.DataCode)
			{
				dBuffer.SetBytes( value, analysis.Content.AddressStart * 2 );
				return OperateResult.CreateSuccessResult( );
			}
			else if (analysis.Content.McDataType.DataCode == MelsecMcDataType.W.DataCode)
			{
				wBuffer.SetBytes( value, analysis.Content.AddressStart * 2 );
				return OperateResult.CreateSuccessResult( );
			}
			else if (analysis.Content.McDataType.DataCode == MelsecMcDataType.R.DataCode)
			{
				rBuffer.SetBytes( value, analysis.Content.AddressStart * 2 );
				return OperateResult.CreateSuccessResult( );
			}
			else if (analysis.Content.McDataType.DataCode == MelsecMcDataType.ZR.DataCode)
			{
				zrBuffer.SetBytes( value, analysis.Content.AddressStart * 2 );
				return OperateResult.CreateSuccessResult( );
			}
			else
			{
				return new OperateResult<byte[]>( StringResources.Language.NotSupportedDataType );
			}
		}

		#endregion

		#region Bool Read Write Operate

		/// <inheritdoc/>
		[HslMqttApi( "ReadBoolArray", "" )]
		public override OperateResult<bool[]> ReadBool( string address, ushort length )
		{
			// 分析地址
			OperateResult<McAddressData> analysis = McAddressData.ParseMelsecFrom( address, 0 );
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( analysis );

			if (analysis.Content.McDataType.DataType == 0) return new OperateResult<bool[]>( StringResources.Language.MelsecCurrentTypeNotSupportedWordOperate );

			if (analysis.Content.McDataType.DataCode == MelsecMcDataType.M.DataCode)
				return OperateResult.CreateSuccessResult( mBuffer.GetBytes( analysis.Content.AddressStart, length ).Select( m => m != 0x00 ).ToArray( ) );
			else if (analysis.Content.McDataType.DataCode == MelsecMcDataType.X.DataCode)
				return OperateResult.CreateSuccessResult( xBuffer.GetBytes( analysis.Content.AddressStart, length ).Select( m => m != 0x00 ).ToArray( ) );
			else if (analysis.Content.McDataType.DataCode == MelsecMcDataType.Y.DataCode)
				return OperateResult.CreateSuccessResult( yBuffer.GetBytes( analysis.Content.AddressStart, length ).Select( m => m != 0x00 ).ToArray( ) );
			else if (analysis.Content.McDataType.DataCode == MelsecMcDataType.L.DataCode)
				return OperateResult.CreateSuccessResult( lBuffer.GetBytes( analysis.Content.AddressStart, length ).Select( m => m != 0x00 ).ToArray( ) );
			else if (analysis.Content.McDataType.DataCode == MelsecMcDataType.B.DataCode)
				return OperateResult.CreateSuccessResult( bBuffer.GetBytes( analysis.Content.AddressStart, length ).Select( m => m != 0x00 ).ToArray( ) );
			else
				return new OperateResult<bool[]>( StringResources.Language.NotSupportedDataType );
		}

		/// <inheritdoc/>
		[HslMqttApi( "WriteBoolArray", "" )]
		public override OperateResult Write( string address, bool[] value )
		{
			// 分析地址
			OperateResult<McAddressData> analysis = McAddressData.ParseMelsecFrom( address, 0 );
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( analysis );

			if (analysis.Content.McDataType.DataType == 0) return new OperateResult<bool[]>( StringResources.Language.MelsecCurrentTypeNotSupportedWordOperate );

			if (analysis.Content.McDataType.DataCode == MelsecMcDataType.M.DataCode)
			{
				mBuffer.SetBytes( value.Select( m => m ? (byte)1 : (byte)0 ).ToArray( ), analysis.Content.AddressStart );
				return OperateResult.CreateSuccessResult( );
			}
			else if (analysis.Content.McDataType.DataCode == MelsecMcDataType.X.DataCode)
			{
				xBuffer.SetBytes( value.Select( m => m ? (byte)1 : (byte)0 ).ToArray( ), analysis.Content.AddressStart );
				return OperateResult.CreateSuccessResult( );
			}
			else if (analysis.Content.McDataType.DataCode == MelsecMcDataType.Y.DataCode)
			{
				yBuffer.SetBytes( value.Select( m => m ? (byte)1 : (byte)0 ).ToArray( ), analysis.Content.AddressStart );
				return OperateResult.CreateSuccessResult( );
			}
			else if (analysis.Content.McDataType.DataCode == MelsecMcDataType.L.DataCode)
			{
				lBuffer.SetBytes( value.Select( m => m ? (byte)1 : (byte)0 ).ToArray( ), analysis.Content.AddressStart );
				return OperateResult.CreateSuccessResult( );
			}
			else if (analysis.Content.McDataType.DataCode == MelsecMcDataType.B.DataCode)
			{
				bBuffer.SetBytes( value.Select( m => m ? (byte)1 : (byte)0 ).ToArray( ), analysis.Content.AddressStart );
				return OperateResult.CreateSuccessResult( );
			}
			else
			{
				return new OperateResult<bool[]>( StringResources.Language.NotSupportedDataType );
			}
		}

		#endregion

		#region NetServer Override

		/// <inheritdoc/>
		protected override INetMessage GetNewNetMessage( )
		{
			if (isBinary)
				return new MelsecQnA3EBinaryMessage( );
			else
				return new MelsecQnA3EAsciiMessage( );
		}

		/// <inheritdoc/>
		protected override OperateResult<byte[]> ReadFromCoreServer( AppSession session, byte[] receive )
		{
			if (isBinary)
				return OperateResult.CreateSuccessResult( ReadFromMcCore( receive.RemoveBegin( 11 ) ) );
			else
				return OperateResult.CreateSuccessResult( ReadFromMcAsciiCore( receive.RemoveBegin( 22 ) ) );
		}

		/// <summary>
		/// 当收到mc协议的报文的时候应该触发的方法，允许继承重写，来实现自定义的返回，或是数据监听。<br />
		/// The method that should be triggered when a message of the mc protocol is received, 
		/// allowing inheritance to be rewritten to implement custom return or data monitoring.
		/// </summary>
		/// <param name="mcCore">mc报文</param>
		/// <returns>返回的报文信息</returns>
		protected virtual byte[] ReadFromMcCore( byte[] mcCore )
		{
			if (mcCore[0] == 0x01 && mcCore[1] == 0x04)
			{
				return ReadByCommand( mcCore ); // 读数据
			}
			else if (mcCore[0] == 0x03 && mcCore[1] == 0x04)
			{
				return ReadRandomByCommand( mcCore );
			}
			else if (mcCore[0] == 0x06 && mcCore[1] == 0x04)
			{
				return ReadBlockByCommand( mcCore );
			}
			else if (mcCore[0] == 0x01 && mcCore[1] == 0x14)
			{
				// 先判断是否有写入的权利，没有的话，直接返回写入异常
				if (!EnableWrite) return PackCommand( 0xC062, null );
				return PackCommand( 0, WriteByMessage( mcCore ) ); // 写数据
			}
			else return null;
		}

		/// <summary>
		/// 当收到mc协议的报文的时候应该触发的方法，允许继承重写，来实现自定义的返回，或是数据监听。<br />
		/// The method that should be triggered when a message of the mc protocol is received, 
		/// allowing inheritance to be rewritten to implement custom return or data monitoring.
		/// </summary>
		/// <param name="mcCore">mc报文</param>
		/// <returns>返回的报文信息</returns>
		protected virtual byte[] ReadFromMcAsciiCore( byte[] mcCore )
		{
			if (mcCore[0] == 0x30 && mcCore[1] == 0x34 && mcCore[2] == 0x30 && mcCore[3] == 0x31)
				return ReadAsciiByCommand( mcCore );
			else if (mcCore[0] == 0x31 && mcCore[1] == 0x34 && mcCore[2] == 0x30 && mcCore[3] == 0x31)
			{
				// 先判断是否有写入的权利，没有的话，直接返回写入异常
				if (!EnableWrite) return PackCommand( 0xC062, null );
				return PackCommand( 0, WriteAsciiByMessage( mcCore ) );
			}
			else
				return null;
		}

		/// <summary>
		/// 将状态码，数据打包成一个完成的回复报文信息
		/// </summary>
		/// <param name="status">状态信息</param>
		/// <param name="data">数据</param>
		/// <returns>状态信息</returns>
		protected virtual byte[] PackCommand( ushort status, byte[] data )
		{
			if (data == null) data = new byte[0];
			if (isBinary)
			{
				byte[] back = new byte[11 + data.Length];
				SoftBasic.HexStringToBytes( "D0 00 00 FF FF 03 00 00 00 00 00" ).CopyTo( back, 0 );
				if (data.Length > 0) data.CopyTo( back, 11 );

				BitConverter.GetBytes( (short)(data.Length + 2) ).CopyTo( back, 7 );
				BitConverter.GetBytes( status ).CopyTo( back, 9 );
				return back;
			}
			else
			{
				byte[] back = new byte[22 + data.Length];
				Encoding.ASCII.GetBytes( "D00000FF03FF0000000000" ).CopyTo( back, 0 );
				if (data.Length > 0) data.CopyTo( back, 22 );

				Encoding.ASCII.GetBytes( (data.Length + 4).ToString( "X4" ) ).CopyTo( back, 14 );
				Encoding.ASCII.GetBytes( status.ToString( "X4" ) ).CopyTo( back, 18 );
				return back;
			}
		}

		private byte[] ReadByCommand( byte[] command )
		{
			ushort length = ByteTransform.TransUInt16( command, 8 );
			int startIndex = (command[6] * 65536 + command[5] * 256 + command[4]);

			if (command[2] == 0x01)
			{
				// 二进制位读取
				if (length > 7168) return PackCommand( 0xC051, null );
				if (     command[7] == MelsecMcDataType.M.DataCode) return PackCommand( 0, MelsecHelper.TransBoolArrayToByteData( mBuffer.GetBytes( startIndex, length ) ) );
				else if (command[7] == MelsecMcDataType.X.DataCode) return PackCommand( 0, MelsecHelper.TransBoolArrayToByteData( xBuffer.GetBytes( startIndex, length ) ) );
				else if (command[7] == MelsecMcDataType.Y.DataCode) return PackCommand( 0, MelsecHelper.TransBoolArrayToByteData( yBuffer.GetBytes( startIndex, length ) ) );
				else if (command[7] == MelsecMcDataType.B.DataCode) return PackCommand( 0, MelsecHelper.TransBoolArrayToByteData( bBuffer.GetBytes( startIndex, length ) ) );
				else if (command[7] == MelsecMcDataType.L.DataCode) return PackCommand( 0, MelsecHelper.TransBoolArrayToByteData( lBuffer.GetBytes( startIndex, length ) ) );
				else return PackCommand( 0xC05A, null );
			}
			else
			{
				// 字读取
				if (length > 960) return PackCommand( 0xC051, null );
				if (command[7] == MelsecMcDataType.M.DataCode)       return PackCommand( 0, mBuffer.GetBytes(  startIndex,     length * 16 ).Select( m => m != 0x00 ).ToArray( ).ToByteArray( ) );
				else if (command[7] == MelsecMcDataType.X.DataCode)  return PackCommand( 0, xBuffer.GetBytes(  startIndex,     length * 16 ).Select( m => m != 0x00 ).ToArray( ).ToByteArray( ) );
				else if (command[7] == MelsecMcDataType.Y.DataCode)  return PackCommand( 0, yBuffer.GetBytes(  startIndex,     length * 16 ).Select( m => m != 0x00 ).ToArray( ).ToByteArray( ) );
				else if (command[7] == MelsecMcDataType.B.DataCode)  return PackCommand( 0, bBuffer.GetBytes(  startIndex,     length * 16 ).Select( m => m != 0x00 ).ToArray( ).ToByteArray( ) );
				else if (command[7] == MelsecMcDataType.L.DataCode)  return PackCommand( 0, lBuffer.GetBytes(  startIndex,     length * 16 ).Select( m => m != 0x00 ).ToArray( ).ToByteArray( ) );
				else if (command[7] == MelsecMcDataType.D.DataCode)  return PackCommand( 0, dBuffer.GetBytes(  startIndex * 2, length * 2 ) );
				else if (command[7] == MelsecMcDataType.W.DataCode)  return PackCommand( 0, wBuffer.GetBytes(  startIndex * 2, length * 2 ) );
				else if (command[7] == MelsecMcDataType.R.DataCode)  return PackCommand( 0, rBuffer.GetBytes(  startIndex * 2, length * 2 ) );
				else if (command[7] == MelsecMcDataType.ZR.DataCode) return PackCommand( 0, zrBuffer.GetBytes( startIndex * 2, length * 2 ) );
				else return PackCommand( 0xC05A, null );
			}
		}

		private byte[] ReadRandomByCommand( byte[] command )
		{
			int count = command[4];
			byte[] buffer = new byte[count * 2];

			for (int i = 0; i < count; i++)
			{
				int startIndex = command[8 + 4 * i] * 65536 + command[7 + 4 * i] * 256 + command[6 + 4 * i];
				if      (command[9 + 4 * i] == MelsecMcDataType.M.DataCode) mBuffer.GetBytes( startIndex, 16 ).Select( m => m != 0x00 ).ToArray( ).ToByteArray( ).CopyTo( buffer, i * 2 );
				else if (command[9 + 4 * i] == MelsecMcDataType.X.DataCode) xBuffer.GetBytes( startIndex, 16 ).Select( m => m != 0x00 ).ToArray( ).ToByteArray( ).CopyTo( buffer, i * 2 );
				else if (command[9 + 4 * i] == MelsecMcDataType.Y.DataCode) yBuffer.GetBytes( startIndex, 16 ).Select( m => m != 0x00 ).ToArray( ).ToByteArray( ).CopyTo( buffer, i * 2 );
				else if (command[9 + 4 * i] == MelsecMcDataType.B.DataCode) bBuffer.GetBytes( startIndex, 16 ).Select( m => m != 0x00 ).ToArray( ).ToByteArray( ).CopyTo( buffer, i * 2 );
				else if (command[9 + 4 * i] == MelsecMcDataType.L.DataCode) lBuffer.GetBytes( startIndex, 16 ).Select( m => m != 0x00 ).ToArray( ).ToByteArray( ).CopyTo( buffer, i * 2 );
				else if (command[9 + 4 * i] == MelsecMcDataType.D.DataCode) dBuffer.GetBytes( startIndex * 2, 2 ).CopyTo( buffer, i * 2 );
				else if (command[9 + 4 * i] == MelsecMcDataType.W.DataCode) wBuffer.GetBytes( startIndex * 2, 2 ).CopyTo( buffer, i * 2 );
				else if (command[9 + 4 * i] == MelsecMcDataType.R.DataCode) rBuffer.GetBytes( startIndex * 2, 2 ).CopyTo( buffer, i * 2 );
				else if (command[9 + 4 * i] == MelsecMcDataType.ZR.DataCode) zrBuffer.GetBytes( startIndex * 2, 2 ).CopyTo( buffer, i * 2 );
			}
			return PackCommand( 0, buffer );
		}

		private byte[] ReadBlockByCommand( byte[] command )
		{
			int count = command[4];
			MemoryStream ms = new MemoryStream( );

			for (int i = 0; i < count; i++)
			{
				int startIndex = command[8 + 6 * i] * 65536 + command[7 + 6 * i] * 256 + command[6 + 6 * i];
				ushort length = ByteTransform.TransUInt16( command, 10 + 6 * i );

				if      (command[9 + 6 * i] == MelsecMcDataType.M.DataCode) ms.Write( mBuffer.GetBytes( startIndex, length * 16 ).Select( m => m != 0x00 ).ToArray( ).ToByteArray( ) );
				else if (command[9 + 6 * i] == MelsecMcDataType.X.DataCode) ms.Write( xBuffer.GetBytes( startIndex, length * 16 ).Select( m => m != 0x00 ).ToArray( ).ToByteArray( ) );
				else if (command[9 + 6 * i] == MelsecMcDataType.Y.DataCode) ms.Write( yBuffer.GetBytes( startIndex, length * 16 ).Select( m => m != 0x00 ).ToArray( ).ToByteArray( ) );
				else if (command[9 + 6 * i] == MelsecMcDataType.B.DataCode) ms.Write( bBuffer.GetBytes( startIndex, length * 16 ).Select( m => m != 0x00 ).ToArray( ).ToByteArray( ) );
				else if (command[9 + 6 * i] == MelsecMcDataType.L.DataCode) ms.Write( lBuffer.GetBytes( startIndex, length * 16 ).Select( m => m != 0x00 ).ToArray( ).ToByteArray( ) );
				else if (command[9 + 6 * i] == MelsecMcDataType.D.DataCode) ms.Write( dBuffer.GetBytes( startIndex * 2, length * 2 ) );
				else if (command[9 + 6 * i] == MelsecMcDataType.W.DataCode) ms.Write( wBuffer.GetBytes( startIndex * 2, length * 2 ) );
				else if (command[9 + 6 * i] == MelsecMcDataType.R.DataCode) ms.Write( rBuffer.GetBytes( startIndex * 2, length * 2 ) );
				else if (command[9 + 6 * i] == MelsecMcDataType.ZR.DataCode) ms.Write( zrBuffer.GetBytes( startIndex * 2, length * 2 ) );
			}
			return PackCommand( 0, ms.ToArray( ) );
		}

		private byte[] ReadAsciiPackCommand( SoftBuffer softBuffer, int startIndex, ushort length, bool isBool )
		{
			if (isBool)
			{
				bool[] buffer = softBuffer.GetBytes( startIndex, length * 16 ).Select( m => m != 0x00 ).ToArray( );
				return PackCommand( 0, MelsecHelper.TransByteArrayToAsciiByteArray( SoftBasic.BoolArrayToByte( buffer ) ) );
			}
			else
			{
				return PackCommand( 0, MelsecHelper.TransByteArrayToAsciiByteArray( dBuffer.GetBytes( startIndex * 2, length * 2 ) ) );
			}
		}

		private byte[] ReadAsciiByCommand( byte[] command )
		{
			ushort length = Convert.ToUInt16( Encoding.ASCII.GetString( command, 16, 4 ), 16 );
			string typeCode = Encoding.ASCII.GetString( command, 8, 2 );
			int startIndex = 0;
			if( typeCode == MelsecMcDataType.X.AsciiCode || 
				typeCode == MelsecMcDataType.Y.AsciiCode || 
				typeCode == MelsecMcDataType.W.AsciiCode ||
				typeCode == MelsecMcDataType.B.AsciiCode ||
				typeCode == MelsecMcDataType.L.AsciiCode)
				startIndex = Convert.ToInt32( Encoding.ASCII.GetString( command, 10, 6 ), 16 );
			else
				startIndex = Convert.ToInt32( Encoding.ASCII.GetString( command, 10, 6 ) );

			if (command[7] == 0x31)
			{
				if (length > 3584) return PackCommand( 0xC051, null );
				// 位读取
				if (typeCode == MelsecMcDataType.M.AsciiCode)      return PackCommand( 0, mBuffer.GetBytes( startIndex, length ).Select( m => m != 0x00 ? (byte)0x31 : (byte)0x30 ).ToArray( ) );
				else if (typeCode == MelsecMcDataType.X.AsciiCode) return PackCommand( 0, xBuffer.GetBytes( startIndex, length ).Select( m => m != 0x00 ? (byte)0x31 : (byte)0x30 ).ToArray( ) );
				else if (typeCode == MelsecMcDataType.Y.AsciiCode) return PackCommand( 0, yBuffer.GetBytes( startIndex, length ).Select( m => m != 0x00 ? (byte)0x31 : (byte)0x30 ).ToArray( ) );
				else if (typeCode == MelsecMcDataType.B.AsciiCode) return PackCommand( 0, bBuffer.GetBytes( startIndex, length ).Select( m => m != 0x00 ? (byte)0x31 : (byte)0x30 ).ToArray( ) );
				else if (typeCode == MelsecMcDataType.L.AsciiCode) return PackCommand( 0, lBuffer.GetBytes( startIndex, length ).Select( m => m != 0x00 ? (byte)0x31 : (byte)0x30 ).ToArray( ) );
				else return PackCommand( 0xC05A, null );
			}
			else
			{
				// 字读取
				if (length > 960) return PackCommand( 0xC051, null );
				if (typeCode == MelsecMcDataType.M.AsciiCode)      return ReadAsciiPackCommand( mBuffer, startIndex, length, true );
				else if (typeCode == MelsecMcDataType.X.AsciiCode) return ReadAsciiPackCommand( xBuffer, startIndex, length, true );
				else if (typeCode == MelsecMcDataType.Y.AsciiCode) return ReadAsciiPackCommand( yBuffer, startIndex, length, true );
				else if (typeCode == MelsecMcDataType.B.AsciiCode) return ReadAsciiPackCommand( bBuffer, startIndex, length, true );
				else if (typeCode == MelsecMcDataType.L.AsciiCode) return ReadAsciiPackCommand( lBuffer, startIndex, length, true );
				else if (typeCode == MelsecMcDataType.D.AsciiCode) return ReadAsciiPackCommand( dBuffer, startIndex, length, false );
				else if (typeCode == MelsecMcDataType.W.AsciiCode) return ReadAsciiPackCommand( wBuffer, startIndex, length, false );
				else if (typeCode == MelsecMcDataType.R.AsciiCode) return ReadAsciiPackCommand( rBuffer, startIndex, length, false );
				else if (typeCode == MelsecMcDataType.ZR.AsciiCode) return ReadAsciiPackCommand( zrBuffer, startIndex, length, false );
				else return PackCommand( 0xC05A, null );
			}
		}

		private byte[] WriteByMessage( byte[] command )
		{
			// 先判断是否有写入的权利，没有的话，直接返回写入异常
			if (!this.EnableWrite) return null;

			ushort length = ByteTransform.TransUInt16( command, 8 );
			int startIndex = command[6] * 65536 + command[5] * 256 + command[4];
			if (command[2] == 0x01)
			{
				// 位写入
				byte[] buffer = Melsec.Helper.McBinaryHelper.ExtractActualDataHelper( command.RemoveBegin( 10 ), true );

				if (     command[7] == MelsecMcDataType.M.DataCode) mBuffer.SetBytes( buffer.Take( length ).ToArray( ), startIndex );
				else if (command[7] == MelsecMcDataType.X.DataCode) xBuffer.SetBytes( buffer.Take( length ).ToArray( ), startIndex );
				else if (command[7] == MelsecMcDataType.Y.DataCode) yBuffer.SetBytes( buffer.Take( length ).ToArray( ), startIndex );
				else if (command[7] == MelsecMcDataType.B.DataCode) bBuffer.SetBytes( buffer.Take( length ).ToArray( ), startIndex );
				else if (command[7] == MelsecMcDataType.L.DataCode) lBuffer.SetBytes( buffer.Take( length ).ToArray( ), startIndex );
				else throw new Exception( StringResources.Language.NotSupportedDataType );

				return new byte[0];
			}
			else
			{
				// 字写入
				if (command[7] == MelsecMcDataType.M.DataCode)
				{
					byte[] buffer = SoftBasic.ByteToBoolArray( SoftBasic.ArrayRemoveBegin( command, 10 ) ).Select( m => m ? (byte)1 : (byte)0 ).ToArray( );
					mBuffer.SetBytes( buffer, startIndex );
					return new byte[0];
				}
				else if (command[7] == MelsecMcDataType.X.DataCode)
				{
					byte[] buffer = SoftBasic.ByteToBoolArray( SoftBasic.ArrayRemoveBegin( command, 10 ) ).Select( m => m ? (byte)1 : (byte)0 ).ToArray( );
					xBuffer.SetBytes( buffer, startIndex );
					return new byte[0];
				}
				else if (command[7] == MelsecMcDataType.Y.DataCode)
				{
					byte[] buffer = SoftBasic.ByteToBoolArray( SoftBasic.ArrayRemoveBegin( command, 10 ) ).Select( m => m ? (byte)1 : (byte)0 ).ToArray( );
					yBuffer.SetBytes( buffer, startIndex );
					return new byte[0];
				}
				else if (command[7] == MelsecMcDataType.B.DataCode)
				{
					byte[] buffer = SoftBasic.ByteToBoolArray( SoftBasic.ArrayRemoveBegin( command, 10 ) ).Select( m => m ? (byte)1 : (byte)0 ).ToArray( );
					bBuffer.SetBytes( buffer, startIndex );
					return new byte[0];
				}
				else if (command[7] == MelsecMcDataType.L.DataCode)
				{
					byte[] buffer = SoftBasic.ByteToBoolArray( SoftBasic.ArrayRemoveBegin( command, 10 ) ).Select( m => m ? (byte)1 : (byte)0 ).ToArray( );
					lBuffer.SetBytes( buffer, startIndex );
					return new byte[0];
				}
				else if (command[7] == MelsecMcDataType.D.DataCode)
				{
					dBuffer.SetBytes( SoftBasic.ArrayRemoveBegin( command, 10 ), startIndex * 2 );
					return new byte[0];
				}
				else if (command[7] == MelsecMcDataType.W.DataCode)
				{
					wBuffer.SetBytes( SoftBasic.ArrayRemoveBegin( command, 10 ), startIndex * 2 );
					return new byte[0];
				}
				else if (command[7] == MelsecMcDataType.R.DataCode)
				{
					rBuffer.SetBytes( SoftBasic.ArrayRemoveBegin( command, 10 ), startIndex * 2 );
					return new byte[0];
				}
				else if (command[7] == MelsecMcDataType.ZR.DataCode)
				{
					zrBuffer.SetBytes( SoftBasic.ArrayRemoveBegin( command, 10 ), startIndex * 2 );
					return new byte[0];
				}
				else
				{
					throw new Exception( StringResources.Language.NotSupportedDataType );
				}
			}
		}

		private byte[] WriteAsciiByMessage( byte[] command )
		{
			ushort length = Convert.ToUInt16( Encoding.ASCII.GetString( command, 16, 4 ), 16 );
			string typeCode = Encoding.ASCII.GetString( command, 8, 2 );
			int startIndex = 0;
			if (typeCode == MelsecMcDataType.X.AsciiCode || 
				typeCode == MelsecMcDataType.Y.AsciiCode || 
				typeCode == MelsecMcDataType.W.AsciiCode ||
				typeCode == MelsecMcDataType.B.AsciiCode ||
				typeCode == MelsecMcDataType.L.AsciiCode)
				startIndex = Convert.ToInt32( Encoding.ASCII.GetString( command, 10, 6 ), 16 );
			else
				startIndex = Convert.ToInt32( Encoding.ASCII.GetString( command, 10, 6 ) );

			if (command[7] == 0x31)
			{
				// 位写入
				byte[] buffer = command.RemoveBegin( 20 ).Select( m => m == 0x31 ? (byte)1 : (byte)0 ).ToArray( );

				if (     typeCode == MelsecMcDataType.M.AsciiCode) mBuffer.SetBytes( buffer.Take( length ).ToArray( ), startIndex );
				else if (typeCode == MelsecMcDataType.X.AsciiCode) xBuffer.SetBytes( buffer.Take( length ).ToArray( ), startIndex );
				else if (typeCode == MelsecMcDataType.Y.AsciiCode) yBuffer.SetBytes( buffer.Take( length ).ToArray( ), startIndex );
				else if (typeCode == MelsecMcDataType.B.AsciiCode) bBuffer.SetBytes( buffer.Take( length ).ToArray( ), startIndex );
				else if (typeCode == MelsecMcDataType.L.AsciiCode) lBuffer.SetBytes( buffer.Take( length ).ToArray( ), startIndex );
				else throw new Exception( StringResources.Language.NotSupportedDataType );

				return new byte[0];
			}
			else
			{
				// 字写入
				if (typeCode == MelsecMcDataType.M.AsciiCode)
				{
					byte[] buffer = SoftBasic.ByteToBoolArray( MelsecHelper.TransAsciiByteArrayToByteArray( command.RemoveBegin(20) ) ).Select( m => m ? (byte)1 : (byte)0 ).ToArray( );
					mBuffer.SetBytes( buffer, startIndex );
					return new byte[0];
				}
				else if (typeCode == MelsecMcDataType.X.AsciiCode)
				{
					byte[] buffer = SoftBasic.ByteToBoolArray( MelsecHelper.TransAsciiByteArrayToByteArray( command.RemoveBegin( 20 ) ) ).Select( m => m ? (byte)1 : (byte)0 ).ToArray( );
					xBuffer.SetBytes( buffer, startIndex );
					return new byte[0];
				}
				else if (typeCode == MelsecMcDataType.Y.AsciiCode)
				{
					byte[] buffer = SoftBasic.ByteToBoolArray( MelsecHelper.TransAsciiByteArrayToByteArray( command.RemoveBegin( 20 ) ) ).Select( m => m ? (byte)1 : (byte)0 ).ToArray( );
					yBuffer.SetBytes( buffer, startIndex );
					return new byte[0];
				}
				else if (typeCode == MelsecMcDataType.B.AsciiCode)
				{
					byte[] buffer = SoftBasic.ByteToBoolArray( MelsecHelper.TransAsciiByteArrayToByteArray( command.RemoveBegin( 20 ) ) ).Select( m => m ? (byte)1 : (byte)0 ).ToArray( );
					bBuffer.SetBytes( buffer, startIndex );
					return new byte[0];
				}
				else if (typeCode == MelsecMcDataType.L.AsciiCode)
				{
					byte[] buffer = SoftBasic.ByteToBoolArray( MelsecHelper.TransAsciiByteArrayToByteArray( command.RemoveBegin( 20 ) ) ).Select( m => m ? (byte)1 : (byte)0 ).ToArray( );
					lBuffer.SetBytes( buffer, startIndex );
					return new byte[0];
				}
				else if (typeCode == MelsecMcDataType.D.AsciiCode)
				{
					dBuffer.SetBytes( MelsecHelper.TransAsciiByteArrayToByteArray( command.RemoveBegin( 20 ) ), startIndex * 2 );
					return new byte[0];
				}
				else if (typeCode == MelsecMcDataType.W.AsciiCode)
				{
					wBuffer.SetBytes( MelsecHelper.TransAsciiByteArrayToByteArray( command.RemoveBegin( 20 ) ), startIndex * 2 );
					return new byte[0];
				}
				else if (typeCode == MelsecMcDataType.R.AsciiCode)
				{
					rBuffer.SetBytes( MelsecHelper.TransAsciiByteArrayToByteArray( command.RemoveBegin( 20 ) ), startIndex * 2 );
					return new byte[0];
				}
				else if (typeCode == MelsecMcDataType.ZR.AsciiCode)
				{
					zrBuffer.SetBytes( MelsecHelper.TransAsciiByteArrayToByteArray( command.RemoveBegin( 20 ) ), startIndex * 2 );
					return new byte[0];
				}
				else
				{
					throw new Exception( StringResources.Language.NotSupportedDataType );
				}
			}
		}

		#endregion

		#region Data Save Load Override

		/// <inheritdoc/>
		protected override void LoadFromBytes( byte[] content )
		{
			if (content.Length < DataPoolLength * 7) throw new Exception( "File is not correct" );

			mBuffer.SetBytes( content, DataPoolLength * 0, 0, DataPoolLength );
			xBuffer.SetBytes( content, DataPoolLength * 1, 0, DataPoolLength );
			yBuffer.SetBytes( content, DataPoolLength * 2, 0, DataPoolLength );
			dBuffer.SetBytes( content, DataPoolLength * 3, 0, DataPoolLength * 2 );
			wBuffer.SetBytes( content, DataPoolLength * 5, 0, DataPoolLength * 2 );

			if(content.Length >= 12)
			{
				bBuffer.SetBytes( content, DataPoolLength * 7, 0, DataPoolLength );
				rBuffer.SetBytes( content, DataPoolLength * 8, 0, DataPoolLength * 2 );
				zrBuffer.SetBytes( content, DataPoolLength * 10, 0, DataPoolLength * 2 );
			}
			if (content.Length >= 15)
			{
				dBuffer.SetBytes( content, DataPoolLength * 12, DataPoolLength * 2, DataPoolLength * 2 );
				lBuffer.SetBytes( content, DataPoolLength * 14, 0, DataPoolLength );
			}
		}

		/// <inheritdoc/>
		[HslMqttApi]
		protected override byte[] SaveToBytes( )
		{
			byte[] buffer = new byte[DataPoolLength * 15];
			Array.Copy( mBuffer.GetBytes( ), 0, buffer, DataPoolLength * 0, DataPoolLength );
			Array.Copy( xBuffer.GetBytes( ), 0, buffer, DataPoolLength * 1, DataPoolLength );
			Array.Copy( yBuffer.GetBytes( ), 0, buffer, DataPoolLength * 2, DataPoolLength );
			Array.Copy( dBuffer.GetBytes( ), 0, buffer, DataPoolLength * 3, DataPoolLength * 2 );
			Array.Copy( wBuffer.GetBytes( ), 0, buffer, DataPoolLength * 5, DataPoolLength * 2 );
			Array.Copy( bBuffer.GetBytes( ), 0, buffer, DataPoolLength * 7, DataPoolLength );
			Array.Copy( rBuffer.GetBytes( ), 0, buffer, DataPoolLength * 8, DataPoolLength * 2 );
			Array.Copy( zrBuffer.GetBytes( ), 0, buffer, DataPoolLength * 10, DataPoolLength * 2 );
			Array.Copy( dBuffer.GetBytes( ), DataPoolLength * 2, buffer, DataPoolLength * 12, DataPoolLength * 2 );
			Array.Copy( lBuffer.GetBytes( ), 0, buffer, DataPoolLength * 14, DataPoolLength );
			return buffer;
		}

		#endregion

		#region IDisposable Support

		/// <summary>
		/// 释放当前的对象
		/// </summary>
		/// <param name="disposing">是否托管对象</param>
		protected override void Dispose( bool disposing )
		{
			if (disposing)
			{
				xBuffer?.Dispose( );
				yBuffer?.Dispose( );
				mBuffer?.Dispose( );
				dBuffer?.Dispose( );
				wBuffer?.Dispose( );
			}
			base.Dispose( disposing );
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// 获取或设置当前的通信格式是否是二进制<br />
		/// Get or set whether the current communication format is binary
		/// </summary>
		public bool IsBinary
		{
			get => isBinary;
			set => isBinary = value;
		}

		#endregion

		#region Private Member

		private SoftBuffer xBuffer;                    // x寄存器的数据池
		private SoftBuffer yBuffer;                    // y寄存器的数据池
		private SoftBuffer mBuffer;                    // m寄存器的数据池
		private SoftBuffer lBuffer;                    // l寄存器的数据池
		private SoftBuffer dBuffer;                    // d寄存器的数据池
		private SoftBuffer wBuffer;                    // w寄存器的数据池
		private SoftBuffer bBuffer;                    // b继电器的数据池
		private SoftBuffer rBuffer;                    // r文件寄存器的数据池
		private SoftBuffer zrBuffer;                   // zr文件寄存器的数据池

		private const int DataPoolLength = 65536;      // 数据的长度
		private bool isBinary = true;                  // 当前的服务器是否是二进制服务器

		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString( ) => $"MelsecMcServer[{Port}]";

		#endregion
	}
}
