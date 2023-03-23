using HslCommunication.BasicFramework;
using HslCommunication.Core;
using HslCommunication.Core.Net;
using HslCommunication.Reflection;
using HslCommunication.Profinet.Beckhoff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using HslCommunication.Core.IMessage;
using HslCommunication.Profinet.Beckhoff.Helper;
#if !NET20 && !NET35
using System.Threading.Tasks;
#endif

namespace HslCommunication.Profinet.Beckhoff
{
	/// <summary>
	/// 倍福Ads协议的虚拟服务器
	/// </summary>
	public class BeckhoffAdsServer : NetworkDataServerBase
	{
		#region Constructor

		/// <summary>
		/// 实例化一个基于ADS协议的虚拟的倍福PLC对象，可以用来和<see cref="BeckhoffAdsNet"/>进行通信测试。
		/// </summary>
		public BeckhoffAdsServer( )
		{
			mBuffer       = new SoftBuffer( DataPoolLength );
			iBuffer       = new SoftBuffer( DataPoolLength );
			qBuffer       = new SoftBuffer( DataPoolLength );

			ByteTransform = new RegularByteTransform( );
			WordLength    = 2;
			memoryAddress = 0x100000;
			adsValues      = new Dictionary<string, AdsTagItem>( );
		}

		#endregion

		#region Data Persistence

		/// <inheritdoc/>
		protected override byte[] SaveToBytes( )
		{
			byte[] buffer = new byte[DataPoolLength * 3];
			mBuffer.GetBytes( ).CopyTo( buffer, DataPoolLength * 0 );
			iBuffer.GetBytes( ).CopyTo( buffer, DataPoolLength * 1 );
			qBuffer.GetBytes( ).CopyTo( buffer, DataPoolLength * 2 );
			return buffer;
		}

		/// <inheritdoc/>
		protected override void LoadFromBytes( byte[] content )
		{
			if (content.Length < DataPoolLength * 3) throw new Exception( "File is not correct" );
			mBuffer.SetBytes( content, 0 * DataPoolLength, DataPoolLength * 1 );
			iBuffer.SetBytes( content, 1 * DataPoolLength, DataPoolLength * 1 );
			qBuffer.SetBytes( content, 2 * DataPoolLength, DataPoolLength * 1 );
		}

		#endregion

		#region NetworkDataServerBase Override

		/// <inheritdoc cref="BeckhoffAdsNet.Read(string, ushort)"/>
		[HslMqttApi( "ReadByteArray", "" )]
		public override OperateResult<byte[]> Read( string address, ushort length )
		{
			OperateResult<uint, uint> analysis = AdsHelper.AnalysisAddress( address, false );
			if (!analysis.IsSuccess) return analysis.ConvertFailed<byte[]>( );

			if (analysis.Content1 == 0xF003)
			{
				string tagName = address.Substring( 2 );
				lock (dicLock)
				{
					if (adsValues.ContainsKey( tagName ))
						return OperateResult.CreateSuccessResult( adsValues[tagName].Buffer.SelectBegin( Math.Min(length, adsValues[tagName].Buffer.Length ) ) );
					else
						return new OperateResult<byte[]>( StringResources.Language.AllenBradley04 ); // 不存在
				}
			}
			else
			{
				switch (analysis.Content1)
				{
					case 0x4020: return OperateResult.CreateSuccessResult( mBuffer.GetBytes( (int)analysis.Content2, length ) );
					case 0xF020: return OperateResult.CreateSuccessResult( iBuffer.GetBytes( (int)analysis.Content2, length ) );
					case 0xF030: return OperateResult.CreateSuccessResult( qBuffer.GetBytes( (int)analysis.Content2, length ) );
				}
				return new OperateResult<byte[]>( StringResources.Language.NotSupportedDataType );
			}
		}

		/// <inheritdoc cref="BeckhoffAdsNet.Write(string, byte[])"/>
		[HslMqttApi( "WriteByteArray", "" )]
		public override OperateResult Write( string address, byte[] value )
		{
			OperateResult<uint, uint> analysis = AdsHelper.AnalysisAddress( address, false );
			if (!analysis.IsSuccess) return analysis.ConvertFailed<byte[]>( );

			if (analysis.Content1 == 0xF003)
			{
				string tagName = address.Substring( 2 );
				lock (dicLock)
				{
					if (adsValues.ContainsKey( tagName ))
					{
						Array.Copy( value, 0, adsValues[tagName].Buffer, 0, Math.Min( value.Length, adsValues[tagName].Buffer.Length ) );
						return OperateResult.CreateSuccessResult( );
					}
					else
						return new OperateResult( StringResources.Language.AllenBradley04 ); // 不存在
				}
			}
			else
			{
				switch (analysis.Content1)
				{
					case 0x4020: mBuffer.SetBytes( value, (int)analysis.Content2 ); break;
					case 0xF020: iBuffer.SetBytes( value, (int)analysis.Content2 ); break;
					case 0xF030: qBuffer.SetBytes( value, (int)analysis.Content2 ); break;
					default: return new OperateResult<byte[]>( StringResources.Language.NotSupportedDataType );
				}
				return OperateResult.CreateSuccessResult( );
			}
		}

		/// <inheritdoc cref="IReadWriteNet.ReadBool(string, ushort)"/>
		[HslMqttApi( "ReadBoolArray", "" )]
		public override OperateResult<bool[]> ReadBool( string address, ushort length )
		{
			OperateResult<uint, uint> analysis = AdsHelper.AnalysisAddress( address, true );
			if (!analysis.IsSuccess) return analysis.ConvertFailed<bool[]>( );

			if (analysis.Content1 == 0xF003)
			{
				string tagName = address.Substring( 2 );
				lock (dicLock)
				{
					if (adsValues.ContainsKey( tagName ))
					{
						return OperateResult.CreateSuccessResult( adsValues[tagName].Buffer.SelectBegin( Math.Min( length, adsValues[tagName].Buffer.Length ) ).Select( m => m != 0x00 ).ToArray( ) );
					}
					else
						return new OperateResult<bool[]>( StringResources.Language.AllenBradley04 ); // 不存在
				}
			}
			else
			{
				switch (analysis.Content1)
				{
					case 0x4021: return OperateResult.CreateSuccessResult( mBuffer.GetBool( (int)analysis.Content2, length ) );
					case 0xF021: return OperateResult.CreateSuccessResult( iBuffer.GetBool( (int)analysis.Content2, length ) );
					case 0xF031: return OperateResult.CreateSuccessResult( qBuffer.GetBool( (int)analysis.Content2, length ) );
				}
				return new OperateResult<bool[]>( StringResources.Language.NotSupportedDataType );
			}
		}

		/// <inheritdoc cref="IReadWriteNet.Write(string, bool[])"/>
		[HslMqttApi( "WriteBoolArray", "" )]
		public override OperateResult Write( string address, bool[] value )
		{
			OperateResult<uint, uint> analysis = AdsHelper.AnalysisAddress( address, true );
			if (!analysis.IsSuccess) return analysis.ConvertFailed<bool[]>( );

			if (analysis.Content1 == 0xF003)
			{
				string tagName = address.Substring( 2 );
				lock (dicLock)
				{
					if (adsValues.ContainsKey( tagName ))
					{
						Array.Copy( value.Select( m => m ? (byte)0x01 : (byte)0x00 ).ToArray( ), 0, adsValues[tagName].Buffer, 0, Math.Min( value.Length, adsValues[tagName].Buffer.Length ) );
						return OperateResult.CreateSuccessResult( );
					}
					else
						return new OperateResult( StringResources.Language.AllenBradley04 ); // 不存在
				}
			}
			else
			{
				switch (analysis.Content1)
				{
					case 0x4021: mBuffer.SetBool( value, (int)analysis.Content2 ); break;
					case 0xF021: iBuffer.SetBool( value, (int)analysis.Content2 ); break;
					case 0xF031: qBuffer.SetBool( value, (int)analysis.Content2 ); break;
					default: return new OperateResult( StringResources.Language.NotSupportedDataType );
				}
				return OperateResult.CreateSuccessResult( );
			}
		}

		#endregion

		#region NetServer Override

		/// <inheritdoc/>
		protected override INetMessage GetNewNetMessage( ) => new AdsNetMessage( );

		/// <inheritdoc/>
		protected override OperateResult<byte[]> ReadFromCoreServer( AppSession session, byte[] receive )
		{
			return OperateResult.CreateSuccessResult( ReadFromAdsCore( session, receive ) );
		}

		#endregion

		#region Core Read

		private byte[] PackCommand( byte[] cmd, int err, byte[] data )
		{
			if (data == null) data = new byte[0];
			byte[] buffer = new byte[32 + data.Length];
			Array.Copy( cmd, 0, buffer, 0, 32 );
			byte[] amsTarget = buffer.SelectBegin( 8 );
			byte[] amsSource = buffer.SelectMiddle( 8, 8 );

			amsTarget.CopyTo( buffer, 8 );
			amsSource.CopyTo( buffer, 0 );
			buffer[18] = 0x05;
			buffer[19] = 0x00;
			BitConverter.GetBytes( data.Length ).CopyTo( buffer, 20 );
			BitConverter.GetBytes( err ).CopyTo( buffer, 24 );
			buffer[11] = 0x00;
			if (data.Length > 0) data.CopyTo( buffer, 32 );
			return AdsHelper.PackAmsTcpHelper( AmsTcpHeaderFlags.Command, buffer );
		}

		private byte[] PackDataResponse( int err, byte[] data )
		{
			if (data != null)
			{
				byte[] buffer = new byte[8 + data.Length];
				BitConverter.GetBytes( err ).CopyTo( buffer, 0 );
				BitConverter.GetBytes( data.Length ).CopyTo( buffer, 4 );
				if (data.Length > 0) data.CopyTo( buffer, 8 );
				return buffer;
			}
			else
			{
				return BitConverter.GetBytes( err );
			}
		}

		private byte[] ReadFromAdsCore( AppSession session, byte[] receive )
		{
			AmsTcpHeaderFlags ams = (AmsTcpHeaderFlags)BitConverter.ToUInt16( receive, 0 );
			if(ams == AmsTcpHeaderFlags.Command)
			{
				receive = receive.RemoveBegin( 6 );
				if (session.Tag == null)
				{
					session.Tag = 1;
					LogNet?.WriteDebug( ToString( ), $"TargetId:{AdsHelper.GetAmsNetIdString( receive, 0 )} SenderId:{AdsHelper.GetAmsNetIdString( receive, 8 )}" );
				}
				short commandId = BitConverter.ToInt16( receive, 16 );
				if (commandId == BeckhoffCommandId.Read)      return ReadByCommand(      receive );
				if (commandId == BeckhoffCommandId.Write)     return WriteByCommand(     receive );
				if (commandId == BeckhoffCommandId.ReadWrite) return ReadWriteByCommand( receive );
				return PackCommand( receive, 0x20, null );
			}
			else if (ams == AmsTcpHeaderFlags.GetLocalNetId)
			{
				return AdsHelper.PackAmsTcpHelper( AmsTcpHeaderFlags.GetLocalNetId, AdsHelper.StrToAMSNetId( "192.168.163.8.1.1" ) );
			}
			else if (ams == AmsTcpHeaderFlags.PortConnect)
			{
				return AdsHelper.PackAmsTcpHelper( AmsTcpHeaderFlags.PortConnect, AdsHelper.StrToAMSNetId( "192.168.163.9.1.1:32957" ) );
			}
			else
			{
				LogNet?.WriteDebug( ToString( ), $"Unknown Source: " + receive.ToHexString( ' ' ) );
			}
			return null;
		}

		private OperateResult<byte[]> ReadByCommand( byte[] command, int indexGroup, int address, int length )
		{
			switch (indexGroup)
			{
				case 0x4020: return OperateResult.CreateSuccessResult( mBuffer.GetBytes( address, length ) );
				case 0xF020: return OperateResult.CreateSuccessResult( iBuffer.GetBytes( address, length ) );
				case 0xF030: return OperateResult.CreateSuccessResult( qBuffer.GetBytes( address, length ) );
				case 0x4021: return OperateResult.CreateSuccessResult( mBuffer.GetBool( address, length ).ToByteArray( ) );
				case 0xF021: return OperateResult.CreateSuccessResult( iBuffer.GetBool( address, length ).ToByteArray( ) );
				case 0xF031: return OperateResult.CreateSuccessResult( qBuffer.GetBool( address, length ).ToByteArray( ) );
				case 0xF005:
					{
						uint memAdd = (uint)address;
						lock (dicLock)
						{
							foreach (AdsTagItem tag in adsValues.Values)
								if (tag.Location == memAdd) return OperateResult.CreateSuccessResult( tag.Buffer.SelectBegin( Math.Min( length, tag.Buffer.Length ) ) );
							return new OperateResult<byte[]>( ) { Content = PackCommand( command, 0x00, PackDataResponse( 0x710, null ) ) };
						}
					}
				default: return new OperateResult<byte[]>( ) { Content = PackCommand( command, 0x40, null ) };
			}
		}

		private byte[] ReadByCommand( byte[] command )
		{
			try
			{
				int indexGroup = BitConverter.ToInt32( command, 32 );
				int address    = BitConverter.ToInt32( command, 36 );
				int length     = BitConverter.ToInt32( command, 40 );

				OperateResult<byte[]> read = ReadByCommand( command, indexGroup, address, length );
				if (!read.IsSuccess) return read.Content;

				return PackCommand( command, 0x00, PackDataResponse( 0, read.Content ) );
			}
			catch
			{
				return PackCommand( command, 0xA4, null );
			}
		}

		private byte[] WriteByCommand( byte[] command )
		{
			// 先判断是否有写入的权利，没有的话，直接返回写入异常
			if (!this.EnableWrite) return PackCommand( command, 0x10, null );

			try
			{ 
				int indexGroup = BitConverter.ToInt32( command, 32 );
				int address    = BitConverter.ToInt32( command, 36 );
				int length     = BitConverter.ToInt32( command, 40 );
				byte[] data = command.RemoveBegin( 44 );

				switch (indexGroup)
				{
					case 0x4020: mBuffer.SetBytes( data, address ); return PackCommand( command, 0x00, PackDataResponse( 0, null ) );
					case 0xF020: iBuffer.SetBytes( data, address ); return PackCommand( command, 0x00, PackDataResponse( 0, null ) );
					case 0xF030: qBuffer.SetBytes( data, address ); return PackCommand( command, 0x00, PackDataResponse( 0, null ) );
					case 0x4021: mBuffer.SetBool( data.Select( m => m != 0x00 ).ToArray( ), address ); return PackCommand( command, 0x00, PackDataResponse( 0, null ) );
					case 0xF021: iBuffer.SetBool( data.Select( m => m != 0x00 ).ToArray( ), address ); return PackCommand( command, 0x00, PackDataResponse( 0, null ) );
					case 0xF031: qBuffer.SetBool( data.Select( m => m != 0x00 ).ToArray( ), address ); return PackCommand( command, 0x00, PackDataResponse( 0, null ) );
					case 0xF005:
						{
							uint memAdd = (uint)address;
							lock (dicLock)
							{
								foreach (AdsTagItem tag in adsValues.Values)
									if (tag.Location == memAdd)
									{
										Array.Copy( data, 0, tag.Buffer, 0, Math.Min( data.Length, tag.Buffer.Length ) );
										return PackCommand( command, 0x00, PackDataResponse( 0, null ) );
									}
								return PackCommand( command, 0x00, PackDataResponse( 0x710, null ) );
							}
						}
				}

				return PackCommand( command, 0x40, null );
			}
			catch
			{
				return PackCommand( command, 0xA4, null );
			}
		}

		private byte[] ReadWriteByCommand( byte[] command )
		{
			try
			{ 
				int indexGroup  = BitConverter.ToInt32( command, 32 );
				int address     = BitConverter.ToInt32( command, 36 );
				int readLength  = BitConverter.ToInt32( command, 40 );
				int writeLength = BitConverter.ToInt32( command, 44 );
				byte[] data = command.RemoveBegin( 48 );

				switch (indexGroup)
				{
					case 0x4020: mBuffer.SetBytes( data, address ); return PackCommand( command, 0x00, PackDataResponse( 0, null ) );
					case 0xF020: iBuffer.SetBytes( data, address ); return PackCommand( command, 0x00, PackDataResponse( 0, null ) );
					case 0xF030: qBuffer.SetBytes( data, address ); return PackCommand( command, 0x00, PackDataResponse( 0, null ) );
					case 0x4021: mBuffer.SetBytes( data, address ); return PackCommand( command, 0x00, PackDataResponse( 0, null ) );
					case 0xF021: iBuffer.SetBytes( data, address ); return PackCommand( command, 0x00, PackDataResponse( 0, null ) );
					case 0xF031: qBuffer.SetBytes( data, address ); return PackCommand( command, 0x00, PackDataResponse( 0, null ) );
					case 0xF003:
						{
							// 读取标签的内存地址
							if (data[data.Length - 1] == 0x00) data = data.RemoveLast( 1 );
							string tag = Encoding.ASCII.GetString( data );
							lock (dicLock)
							{
								if (adsValues.ContainsKey( tag ))
								{
									return PackCommand( command, 0x00, PackDataResponse( 0x00, BitConverter.GetBytes( adsValues[tag].Location ) ) );
								}
								else
								{
									return PackCommand( command, 0x00, PackDataResponse( 0x710, null ) );
								}
							}
						}
					case 0xF080:
						{
							List<byte> array = new List<byte>( );
							// 批量读取
							for (int i = 0; i < data.Length / 12; i++)
							{
								int ig       = BitConverter.ToInt32( data, 12 * i + 0 );
								int offset   = BitConverter.ToInt32( data, 12 * i + 4 );
								int length   = BitConverter.ToInt32( data, 12 * i + 8 );

								OperateResult<byte[]> read = ReadByCommand( command, ig, offset, length );
								if (!read.IsSuccess) return read.Content;

								array.AddRange( read.Content );
							}
							return PackCommand( command, 0x00, PackDataResponse( 0x00, array.ToArray( ) ) );
						}
				}

				return PackCommand( command, 0x40, null );
			}
			catch
			{
				return PackCommand( command, 0xA4, null );
			}
		}

		#endregion

		#region Add Tag

		/// <inheritdoc cref="AllenBradley.AllenBradleyServer.AddTagValue(string, AllenBradley.AllenBradleyItemValue)"/>
		public void AddTagValue( string key, AdsTagItem value )
		{
			value.Location = (uint)System.Threading.Interlocked.Increment( ref memoryAddress );
			lock (dicLock)
			{
				if (adsValues.ContainsKey( key ))
					adsValues[key] = value;
				else
					adsValues.Add( key, value );
			}
		}

		/// <inheritdoc cref="AllenBradley.AllenBradleyServer.AddTagValue(string, AllenBradley.AllenBradleyItemValue)"/>
		public void AddTagValue( string key, bool value ) => AddTagValue( key, new AdsTagItem( key, value ? new byte[1] { 0x01 } : new byte[1] { 0x00 } ) );

		/// <inheritdoc cref="AllenBradley.AllenBradleyServer.AddTagValue(string, AllenBradley.AllenBradleyItemValue)"/>
		public void AddTagValue( string key, bool[] value ) => AddTagValue( key, new AdsTagItem( key, value.Select( m => m ? (byte)0x01 : (byte)0x00 ).ToArray( ) ) );

		/// <inheritdoc cref="AllenBradley.AllenBradleyServer.AddTagValue(string, short)"/>
		public void AddTagValue( string key, short value ) => AddTagValue( key, new AdsTagItem( key, ByteTransform.TransByte( value ) ) );

		/// <inheritdoc cref="AllenBradley.AllenBradleyServer.AddTagValue(string, short[])"/>
		public void AddTagValue( string key, short[] value ) => AddTagValue( key, new AdsTagItem( key, ByteTransform.TransByte( value ) ) );

		/// <inheritdoc cref="AllenBradley.AllenBradleyServer.AddTagValue(string, ushort)"/>
		public void AddTagValue( string key, ushort value ) => AddTagValue( key, new AdsTagItem( key, ByteTransform.TransByte(value ) ) );

		/// <inheritdoc cref="AllenBradley.AllenBradleyServer.AddTagValue(string, ushort[])"/>
		public void AddTagValue( string key, ushort[] value ) => AddTagValue( key, new AdsTagItem( key, ByteTransform.TransByte( value ) ) );

		/// <inheritdoc cref="AllenBradley.AllenBradleyServer.AddTagValue(string, int)"/>
		public void AddTagValue( string key, int value ) => AddTagValue( key, new AdsTagItem( key, ByteTransform.TransByte( value ) ) );

		/// <inheritdoc cref="AllenBradley.AllenBradleyServer.AddTagValue(string, int[])"/>
		public void AddTagValue( string key, int[] value ) => AddTagValue( key, new AdsTagItem( key, ByteTransform.TransByte( value ) ) );

		/// <inheritdoc cref="AllenBradley.AllenBradleyServer.AddTagValue(string, uint)"/>
		public void AddTagValue( string key, uint value ) => AddTagValue( key, new AdsTagItem( key, ByteTransform.TransByte( value ) ) );

		/// <inheritdoc cref="AllenBradley.AllenBradleyServer.AddTagValue(string, uint[])"/>
		public void AddTagValue( string key, uint[] value ) => AddTagValue( key, new AdsTagItem( key, ByteTransform.TransByte( value ) ) );

		/// <inheritdoc cref="AllenBradley.AllenBradleyServer.AddTagValue(string, long)"/>
		public void AddTagValue( string key, long value ) => AddTagValue( key, new AdsTagItem( key, ByteTransform.TransByte( value ) ) );

		/// <inheritdoc cref="AllenBradley.AllenBradleyServer.AddTagValue(string, long[])"/>
		public void AddTagValue( string key, long[] value ) => AddTagValue( key, new AdsTagItem( key, ByteTransform.TransByte( value ) ) );

		/// <inheritdoc cref="AllenBradley.AllenBradleyServer.AddTagValue(string, ulong)"/>
		public void AddTagValue( string key, ulong value ) => AddTagValue( key, new AdsTagItem( key, ByteTransform.TransByte( value ) ) );

		/// <inheritdoc cref="AllenBradley.AllenBradleyServer.AddTagValue(string, ulong[])"/>
		public void AddTagValue( string key, ulong[] value ) => AddTagValue( key, new AdsTagItem( key, ByteTransform.TransByte( value ) ) );

		/// <inheritdoc cref="AllenBradley.AllenBradleyServer.AddTagValue(string, float)"/>
		public void AddTagValue( string key, float value ) => AddTagValue( key, new AdsTagItem( key, ByteTransform.TransByte( value ) ) );

		/// <inheritdoc cref="AllenBradley.AllenBradleyServer.AddTagValue(string, float[])"/>
		public void AddTagValue( string key, float[] value ) => AddTagValue( key, new AdsTagItem( key, ByteTransform.TransByte( value ) ) );

		/// <inheritdoc cref="AllenBradley.AllenBradleyServer.AddTagValue(string, double)"/>
		public void AddTagValue( string key, double value ) => AddTagValue( key, new AdsTagItem( key, ByteTransform.TransByte( value ) ) );

		/// <inheritdoc cref="AllenBradley.AllenBradleyServer.AddTagValue(string, double[])"/>
		public void AddTagValue( string key, double[] value ) => AddTagValue( key, new AdsTagItem( key, ByteTransform.TransByte( value ) ) );

		/// <inheritdoc cref="AllenBradley.AllenBradleyServer.AddTagValue(string, string, int)"/>
		public void AddTagValue( string key, string value, int maxLength )
		{
			byte[] strBuffer = SoftBasic.ArrayExpandToLength( Encoding.UTF8.GetBytes( value ), maxLength );
			AddTagValue( key, new AdsTagItem( key, strBuffer ) );
		}

		#endregion

		#region IDisposable Support

		/// <inheritdoc/>
		protected override void Dispose( bool disposing )
		{
			if (disposing)
			{
				mBuffer.Dispose( );
				iBuffer.Dispose( );
				qBuffer.Dispose( );
			}
			base.Dispose( disposing );
		}

		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString( ) => $"BeckhoffAdsServer[{Port}]";

		#endregion

		#region Private Member

		private SoftBuffer mBuffer;       // 寄存器的数据池
		private SoftBuffer iBuffer;       // 寄存器的数据池
		private SoftBuffer qBuffer;       // 寄存器的数据池
		private Dictionary<string, AdsTagItem> adsValues;          // 词典
		private object dicLock = new object( );                    // 词典的锁
		private int memoryAddress;                                 // 随机数

		private const int DataPoolLength = 65536;     // 数据的长度

		#endregion
	}
}
