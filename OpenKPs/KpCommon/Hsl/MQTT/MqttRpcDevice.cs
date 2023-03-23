using HslCommunication.BasicFramework;
using HslCommunication.Core;
using HslCommunication.Core.Net;
using HslCommunication.Reflection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
#if !NET35 && !NET20
using System.Threading.Tasks;
#endif

namespace HslCommunication.MQTT
{
	/// <summary>
	/// 基于MRPC实现的远程设备访问的接口，实现了和基础PLC一样的访问功能，适用的设备为 <see cref="MqttServer"/> 将PLC实际的通信对象注册为 RPC 接口服务。<br />
	/// The interface for remote device access based on MRPC implements the same access function as the basic PLC. The applicable device is <see cref="MqttServer"/> to register the actual communication object of the PLC as an RPC interface service.
	/// </summary>
	/// <remarks>
	/// 什么时候会用到本设备对象呢？如果你得PLC端口只有一个，只能支持一个连接，但是实际通信的客户端不止一个时，就可以使用本类实现一对多通信。或是你希望在最终通信的客户端和PLC之间做个隔离，并增加安全校验。详细的例子参考API文档。<br />
	/// When will this device object be used? If you have only one PLC port and can only support one connection, but there are more than one client for actual communication, you can use this class to implement one-to-many communication. 
	/// Or you want to isolate the final communication client from the PLC and add security checks. For a detailed example, refer to the API documentation.
	/// </remarks>
	/// <example>
	/// 如何和服务器进行配套调用使用呢？先在服务器端创建服务，并注册Api接口对象
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\MQTT\MqttRpcDeviceSample.cs" region="Server" title="服务器侧示例" />
	/// 然后客户端就支持多连接了，客户端的代码如下
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\MQTT\MqttRpcDeviceSample.cs" region="Client" title="客户端侧示例" />
	/// </example>
	public class MqttRpcDevice : MqttSyncClient, IReadWriteDevice
	{
		#region Constructor

		/// <summary>
		/// 实例化一个MQTT的同步客户端<br />
		/// Instantiate an MQTT synchronization client
		/// </summary>
		/// <param name="options">连接的参数信息，可以指定IP地址，端口，账户名，密码，客户端ID信息</param>
		/// <param name="topic">设备关联的主题信息</param>
		public MqttRpcDevice( MqttConnectionOptions options, string topic = null ) : base( options )
		{
			this.deviceTopic = topic;
		}

		/// <summary>
		/// 通过指定的ip地址及端口来实例化一个同步的MQTT客户端<br />
		/// Instantiate a synchronized MQTT client with the specified IP address and port
		/// </summary>
		/// <param name="ipAddress">IP地址信息</param>
		/// <param name="port">端口号信息</param>
		/// <param name="topic">设备关联的主题信息</param>
		public MqttRpcDevice( string ipAddress, int port, string topic = null ) : base( ipAddress, port )
		{
			this.deviceTopic = topic;
		}

		private string GetTopic( string topic )
		{
			if (string.IsNullOrEmpty( this.deviceTopic )) return topic;
			if (this.deviceTopic.EndsWith( "/" )) return this.deviceTopic + topic;
			return this.deviceTopic + "/" + topic;
		}

		#endregion

		#region IReadWriteDevice

		/// <inheritdoc cref="IReadWriteNet.Read(string, ushort)"/>
		[HslMqttApi( "ReadByteArray", "" )]
		public virtual OperateResult<byte[]> Read( string address, ushort length ) => ReadRpc<byte[]>( GetTopic( "ReadByteArray" ), new { address = address, length = length } );

		/// <inheritdoc cref="IReadWriteNet.Write(string, byte[])"/>
		[HslMqttApi( "WriteByteArray", "" )]
		public virtual OperateResult Write( string address, byte[] value ) => ReadRpc<string>( GetTopic( "WriteByteArray" ), new { address = address, value = value.ToHexString( ) } );

		// 以下是bool的读写操作实现

		/// <inheritdoc cref="IReadWriteNet.ReadBool(string, ushort)"/>
		[HslMqttApi( "ReadBoolArray", "" )]
		public virtual OperateResult<bool[]> ReadBool( string address, ushort length ) => ReadRpc<bool[]>( GetTopic( "ReadBoolArray" ), new { address = address, length = length } );

		/// <inheritdoc cref="IReadWriteNet.ReadBool(string)"/>
		[HslMqttApi( "ReadBool", "" )]
		public virtual OperateResult<bool> ReadBool( string address ) => ReadRpc<bool>( GetTopic( "ReadBool" ), new { address = address } );

		/// <inheritdoc cref="IReadWriteNet.Write(string, bool[])"/>
		[HslMqttApi( "WriteBoolArray", "" )]
		public virtual OperateResult Write( string address, bool[] value ) => ReadRpc<string>( GetTopic( "WriteBoolArray" ), new { address = address, value = value } );

		/// <inheritdoc cref="IReadWriteNet.Write(string, bool)"/>
		[HslMqttApi( "WriteBool", "" )]
		public virtual OperateResult Write( string address, bool value ) => ReadRpc<string>( GetTopic( "WriteBool" ), new { address = address, value = value } );

		#endregion

		#region Customer Read Write

		/// <inheritdoc cref="IReadWriteNet.ReadCustomer{T}(string)"/>
		public OperateResult<T> ReadCustomer<T>( string address ) where T : IDataTransfer, new() => ReadWriteNetHelper.ReadCustomer<T>( this, address );

		/// <inheritdoc cref="IReadWriteNet.ReadCustomer{T}(string, T)"/>
		public OperateResult<T> ReadCustomer<T>( string address, T obj ) where T : IDataTransfer, new() => ReadWriteNetHelper.ReadCustomer( this, address, obj );

		/// <inheritdoc cref="IReadWriteNet.WriteCustomer{T}(string, T)"/>
		public OperateResult WriteCustomer<T>( string address, T data ) where T : IDataTransfer, new() => ReadWriteNetHelper.WriteCustomer( this, address, data );

		#endregion

		#region Reflection Read Write

		/// <inheritdoc cref="IReadWriteNet.Read{T}"/>
		public virtual OperateResult<T> Read<T>( ) where T : class, new() => HslReflectionHelper.Read<T>( this );

		/// <inheritdoc cref="IReadWriteNet.Write{T}(T)"/>
		public virtual OperateResult Write<T>( T data ) where T : class, new() => HslReflectionHelper.Write<T>( data, this );

		/// <inheritdoc cref="IReadWriteNet.ReadStruct{T}(string, ushort)"/>
		public virtual OperateResult<T> ReadStruct<T>( string address, ushort length ) where T : class, new() => ReadWriteNetHelper.ReadStruct<T>( this, address, length, this.ByteTransform );

		#endregion

		#region Read Support

		/// <inheritdoc cref="IReadWriteNet.ReadInt16(string)"/>
		[HslMqttApi( "ReadInt16", "" )]
		public OperateResult<short> ReadInt16( string address ) => ReadRpc<short>( GetTopic( "ReadInt16" ), new { address = address } );

		/// <inheritdoc cref="IReadWriteNet.ReadInt16(string, ushort)"/>
		[HslMqttApi( "ReadInt16Array", "" )]
		public virtual OperateResult<short[]> ReadInt16( string address, ushort length ) => ReadRpc<short[]>( GetTopic( "ReadInt16Array" ), new { address = address, length = length } );

		/// <inheritdoc cref="IReadWriteNet.ReadUInt16(string)"/>
		[HslMqttApi( "ReadUInt16", "" )]
		public OperateResult<ushort> ReadUInt16( string address ) => ReadRpc<ushort>( GetTopic( "ReadUInt16" ), new { address = address } );

		/// <inheritdoc cref="IReadWriteNet.ReadUInt16(string, ushort)"/>
		[HslMqttApi( "ReadUInt16Array", "" )]
		public virtual OperateResult<ushort[]> ReadUInt16( string address, ushort length ) => ReadRpc<ushort[]>( GetTopic( "ReadUInt16Array" ), new { address = address, length = length } );

		/// <inheritdoc cref="IReadWriteNet.ReadInt32(string)"/>
		[HslMqttApi( "ReadInt32", "" )]
		public OperateResult<int> ReadInt32( string address ) => ReadRpc<int>( GetTopic( "ReadInt32" ), new { address = address } );

		/// <inheritdoc cref="IReadWriteNet.ReadInt32(string, ushort)"/>
		[HslMqttApi( "ReadInt32Array", "" )]
		public virtual OperateResult<int[]> ReadInt32( string address, ushort length ) => ReadRpc<int[]>( GetTopic( "ReadInt32Array" ), new { address = address, length = length } );

		/// <inheritdoc cref="IReadWriteNet.ReadUInt32(string)"/>
		[HslMqttApi( "ReadUInt32", "" )]
		public OperateResult<uint> ReadUInt32( string address ) => ReadRpc<uint>( GetTopic( "ReadUInt32" ), new { address = address } );

		/// <inheritdoc cref="IReadWriteNet.ReadUInt32(string, ushort)"/>
		[HslMqttApi( "ReadUInt32Array", "" )]
		public virtual OperateResult<uint[]> ReadUInt32( string address, ushort length ) => ReadRpc<uint[]>( GetTopic( "ReadUInt32Array" ), new { address = address, length = length } );

		/// <inheritdoc cref="IReadWriteNet.ReadFloat(string)"/>
		[HslMqttApi( "ReadFloat", "" )]
		public OperateResult<float> ReadFloat( string address ) => ReadRpc<float>( GetTopic( "ReadFloat" ), new { address = address } );

		/// <inheritdoc cref="IReadWriteNet.ReadFloat(string, ushort)"/>
		[HslMqttApi( "ReadFloatArray", "" )]
		public virtual OperateResult<float[]> ReadFloat( string address, ushort length ) => ReadRpc<float[]>( GetTopic( "ReadFloatArray" ), new { address = address, length = length } );

		/// <inheritdoc cref="IReadWriteNet.ReadInt64(string)"/>
		[HslMqttApi( "ReadInt64", "" )]
		public OperateResult<long> ReadInt64( string address ) => ReadRpc<long>( GetTopic( "ReadInt64" ), new { address = address } );

		/// <inheritdoc cref="IReadWriteNet.ReadInt64(string, ushort)"/>
		[HslMqttApi( "ReadInt64Array", "" )]
		public virtual OperateResult<long[]> ReadInt64( string address, ushort length ) => ReadRpc<long[]>( GetTopic( "ReadInt64Array" ), new { address = address, length = length } );

		/// <inheritdoc cref="IReadWriteNet.ReadUInt64(string)"/>
		[HslMqttApi( "ReadUInt64", "" )]
		public OperateResult<ulong> ReadUInt64( string address ) => ReadRpc<ulong>( GetTopic( "ReadUInt64" ), new { address = address } );

		/// <inheritdoc cref="IReadWriteNet.ReadUInt64(string, ushort)"/>
		[HslMqttApi( "ReadUInt64Array", "" )]
		public virtual OperateResult<ulong[]> ReadUInt64( string address, ushort length ) => ReadRpc<ulong[]>( GetTopic( "ReadUInt64Array" ), new { address = address, length = length } );

		/// <inheritdoc cref="IReadWriteNet.ReadDouble(string)"/>
		[HslMqttApi( "ReadDouble", "" )]
		public OperateResult<double> ReadDouble( string address ) => ReadRpc<double>( GetTopic( "ReadDouble" ), new { address = address } );

		/// <inheritdoc cref="IReadWriteNet.ReadDouble(string, ushort)"/>
		[HslMqttApi( "ReadDoubleArray", "" )]
		public virtual OperateResult<double[]> ReadDouble( string address, ushort length ) => ReadRpc<double[]>( GetTopic( "ReadDoubleArray" ), new { address = address, length = length } );

		/// <inheritdoc cref="IReadWriteNet.ReadString(string, ushort)"/>
		[HslMqttApi( "ReadString", "" )]
		public virtual OperateResult<string> ReadString( string address, ushort length ) => ReadRpc<string>( GetTopic( "ReadString" ), new { address = address, length = length } );

		/// <inheritdoc cref="IReadWriteNet.ReadString(string, ushort, Encoding)"/>
		public virtual OperateResult<string> ReadString( string address, ushort length, Encoding encoding ) => ByteTransformHelper.GetResultFromBytes( Read( address, length ), m => ByteTransform.TransString( m, 0, m.Length, encoding ) );

		#endregion

		#region Write Support

		/// <inheritdoc cref="IReadWriteNet.Write(string, short[])"/>
		[HslMqttApi( "WriteInt16Array", "" )]
		public virtual OperateResult Write( string address, short[] values ) => ReadRpc<string>( GetTopic( "WriteInt16Array" ), new { address = address, values = values } );

		/// <inheritdoc cref="IReadWriteNet.Write(string, short)"/>
		[HslMqttApi( "WriteInt16", "" )]
		public virtual OperateResult Write( string address, short value ) => ReadRpc<string>( GetTopic( "WriteInt16" ), new { address = address, value = value } );

		/// <inheritdoc cref="IReadWriteNet.Write(string, ushort[])"/>
		[HslMqttApi( "WriteUInt16Array", "" )]
		public virtual OperateResult Write( string address, ushort[] values ) => ReadRpc<string>( GetTopic( "WriteUInt16Array" ), new { address = address, values = values } );

		/// <inheritdoc cref="IReadWriteNet.Write(string, ushort)"/>
		[HslMqttApi( "WriteUInt16", "" )]
		public virtual OperateResult Write( string address, ushort value ) => ReadRpc<string>( GetTopic( "WriteUInt16" ), new { address = address, value = value } );

		/// <inheritdoc cref="IReadWriteNet.Write(string, int[])"/>
		[HslMqttApi( "WriteInt32Array", "" )]
		public virtual OperateResult Write( string address, int[] values ) => ReadRpc<string>( GetTopic( "WriteInt32Array" ), new { address = address, values = values } );

		/// <inheritdoc cref="IReadWriteNet.Write(string, int)"/>
		[HslMqttApi( "WriteInt32", "" )]
		public OperateResult Write( string address, int value ) => ReadRpc<string>( GetTopic( "WriteInt32" ), new { address = address, value = value } );

		/// <inheritdoc cref="IReadWriteNet.Write(string, uint[])"/>
		[HslMqttApi( "WriteUInt32Array", "" )]
		public virtual OperateResult Write( string address, uint[] values ) => ReadRpc<string>( GetTopic( "WriteUInt32Array" ), new { address = address, values = values } );

		/// <inheritdoc cref="IReadWriteNet.Write(string, uint)"/>
		[HslMqttApi( "WriteUInt32", "" )]
		public OperateResult Write( string address, uint value ) => ReadRpc<string>( GetTopic( "WriteUInt32" ), new { address = address, value = value } );

		/// <inheritdoc cref="IReadWriteNet.Write(string, float[])"/>
		[HslMqttApi( "WriteFloatArray", "" )]
		public virtual OperateResult Write( string address, float[] values ) => ReadRpc<string>( GetTopic( "WriteFloatArray" ), new { address = address, values = values } );

		/// <inheritdoc cref="IReadWriteNet.Write(string, float)"/>
		[HslMqttApi( "WriteFloat", "" )]
		public OperateResult Write( string address, float value ) => ReadRpc<string>( GetTopic( "WriteFloat" ), new { address = address, value = value } );

		/// <inheritdoc cref="IReadWriteNet.Write(string, long[])"/>
		[HslMqttApi( "WriteInt64Array", "" )]
		public virtual OperateResult Write( string address, long[] values ) => ReadRpc<string>( GetTopic( "WriteInt64Array" ), new { address = address, values = values } );

		/// <inheritdoc cref="IReadWriteNet.Write(string, long)"/>
		[HslMqttApi( "WriteInt64", "" )]
		public OperateResult Write( string address, long value ) => ReadRpc<string>( GetTopic( "WriteInt64" ), new { address = address, value = value } );

		/// <inheritdoc cref="IReadWriteNet.Write(string, ulong[])"/>
		[HslMqttApi( "WriteUInt64Array", "" )]
		public virtual OperateResult Write( string address, ulong[] values ) => ReadRpc<string>( GetTopic( "WriteUInt64Array" ), new { address = address, values = values } );

		/// <inheritdoc cref="IReadWriteNet.Write(string, ulong)"/>
		[HslMqttApi( "WriteUInt64", "" )]
		public OperateResult Write( string address, ulong value ) => ReadRpc<string>( GetTopic( "WriteUInt64" ), new { address = address, value = value } );

		/// <inheritdoc cref="IReadWriteNet.Write(string, double[])"/>
		[HslMqttApi( "WriteDoubleArray", "" )]
		public virtual OperateResult Write( string address, double[] values ) => ReadRpc<string>( GetTopic( "WriteDoubleArray" ), new { address = address, values = values } );

		/// <inheritdoc cref="IReadWriteNet.Write(string, double)"/>
		[HslMqttApi( "WriteDouble", "" )]
		public OperateResult Write( string address, double value ) => ReadRpc<string>( GetTopic( "WriteDouble" ), new { address = address, value = value } );

		/// <inheritdoc cref="IReadWriteNet.Write(string, string)"/>
		[HslMqttApi( "WriteString", "" )]
		public virtual OperateResult Write( string address, string value ) => ReadRpc<string>( GetTopic( "WriteString" ), new { address = address, value = value } );

		/// <inheritdoc cref="IReadWriteNet.Write(string, string, int)"/>
		public virtual OperateResult Write( string address, string value, int length ) => Write( address, value, length, Encoding.ASCII );

		/// <inheritdoc cref="IReadWriteNet.Write(string, string, Encoding)"/>
		public virtual OperateResult Write( string address, string value, Encoding encoding )
		{
			byte[] temp = ByteTransform.TransByte( value, encoding );
			return Write( address, temp );
		}

		/// <inheritdoc cref="IReadWriteNet.Write(string, string, int, Encoding)"/>
		public virtual OperateResult Write( string address, string value, int length, Encoding encoding )
		{
			byte[] temp = ByteTransform.TransByte( value, encoding );
			temp = SoftBasic.ArrayExpandToLength( temp, length );
			return Write( address, temp );
		}
		#endregion

		#region Wait Support

		/// <inheritdoc cref="IReadWriteNet.Wait(string, bool, int, int)"/>
		[HslMqttApi( "WaitBool", "" )]
		public OperateResult<TimeSpan> Wait( string address, bool waitValue, int readInterval = 100, int waitTimeout = -1 ) => ReadWriteNetHelper.Wait( this, address, waitValue, readInterval, waitTimeout );

		/// <inheritdoc cref="IReadWriteNet.Wait(string, short, int, int)"/>
		[HslMqttApi( "WaitInt16", "" )]
		public OperateResult<TimeSpan> Wait( string address, short waitValue, int readInterval = 100, int waitTimeout = -1 ) => ReadWriteNetHelper.Wait( this, address, waitValue, readInterval, waitTimeout );

		/// <inheritdoc cref="IReadWriteNet.Wait(string, ushort, int, int)"/>
		[HslMqttApi( "WaitUInt16", "" )]
		public OperateResult<TimeSpan> Wait( string address, ushort waitValue, int readInterval = 100, int waitTimeout = -1 ) => ReadWriteNetHelper.Wait( this, address, waitValue, readInterval, waitTimeout );

		/// <inheritdoc cref="IReadWriteNet.Wait(string, int, int, int)"/>
		[HslMqttApi( "WaitInt32", "" )]
		public OperateResult<TimeSpan> Wait( string address, int waitValue, int readInterval = 100, int waitTimeout = -1 ) => ReadWriteNetHelper.Wait( this, address, waitValue, readInterval, waitTimeout );

		/// <inheritdoc cref="IReadWriteNet.Wait(string, uint, int, int)"/>
		[HslMqttApi( "WaitUInt32", "" )]
		public OperateResult<TimeSpan> Wait( string address, uint waitValue, int readInterval = 100, int waitTimeout = -1 ) => ReadWriteNetHelper.Wait( this, address, waitValue, readInterval, waitTimeout );

		/// <inheritdoc cref="IReadWriteNet.Wait(string, long, int, int)"/>
		[HslMqttApi( "WaitInt64", "" )]
		public OperateResult<TimeSpan> Wait( string address, long waitValue, int readInterval = 100, int waitTimeout = -1 ) => ReadWriteNetHelper.Wait( this, address, waitValue, readInterval, waitTimeout );

		/// <inheritdoc cref="IReadWriteNet.Wait(string, ulong, int, int)"/>
		[HslMqttApi( "WaitUInt64", "" )]
		public OperateResult<TimeSpan> Wait( string address, ulong waitValue, int readInterval = 100, int waitTimeout = -1 ) => ReadWriteNetHelper.Wait( this, address, waitValue, readInterval, waitTimeout );
#if !NET20 && !NET35

		/// <inheritdoc cref="IReadWriteNet.Wait(string, bool, int, int)"/>
		public async Task<OperateResult<TimeSpan>> WaitAsync( string address, bool waitValue, int readInterval = 100, int waitTimeout = -1 ) => await ReadWriteNetHelper.WaitAsync( this, address, waitValue, readInterval, waitTimeout );

		/// <inheritdoc cref="IReadWriteNet.Wait(string, short, int, int)"/>
		public async Task<OperateResult<TimeSpan>> WaitAsync( string address, short waitValue, int readInterval = 100, int waitTimeout = -1 ) => await ReadWriteNetHelper.WaitAsync( this, address, waitValue, readInterval, waitTimeout );

		/// <inheritdoc cref="IReadWriteNet.Wait(string, ushort, int, int)"/>
		public async Task<OperateResult<TimeSpan>> WaitAsync( string address, ushort waitValue, int readInterval = 100, int waitTimeout = -1 ) => await ReadWriteNetHelper.WaitAsync( this, address, waitValue, readInterval, waitTimeout );

		/// <inheritdoc cref="IReadWriteNet.Wait(string, int, int, int)"/>
		public async Task<OperateResult<TimeSpan>> WaitAsync( string address, int waitValue, int readInterval = 100, int waitTimeout = -1 ) => await ReadWriteNetHelper.WaitAsync( this, address, waitValue, readInterval, waitTimeout );

		/// <inheritdoc cref="IReadWriteNet.Wait(string, uint, int, int)"/>
		public async Task<OperateResult<TimeSpan>> WaitAsync( string address, uint waitValue, int readInterval = 100, int waitTimeout = -1 ) => await ReadWriteNetHelper.WaitAsync( this, address, waitValue, readInterval, waitTimeout );

		/// <inheritdoc cref="IReadWriteNet.Wait(string, long, int, int)"/>
		public async Task<OperateResult<TimeSpan>> WaitAsync( string address, long waitValue, int readInterval = 100, int waitTimeout = -1 ) => await ReadWriteNetHelper.WaitAsync( this, address, waitValue, readInterval, waitTimeout );

		/// <inheritdoc cref="IReadWriteNet.Wait(string, ulong, int, int)"/>
		public async Task<OperateResult<TimeSpan>> WaitAsync( string address, ulong waitValue, int readInterval = 100, int waitTimeout = -1 ) => await ReadWriteNetHelper.WaitAsync( this, address, waitValue, readInterval, waitTimeout );
#endif
		#endregion

		#region Asycn Read Write Bytes Bool
#if !NET35 && !NET20
		/// <inheritdoc cref="IReadWriteNet.ReadAsync(string, ushort)"/>
		public virtual async Task<OperateResult<byte[]>> ReadAsync( string address, ushort length ) => await ReadRpcAsync<byte[]>( GetTopic( "ReadByteArray" ), new { address = address, length = length } );

		/// <inheritdoc cref="IReadWriteNet.WriteAsync(string, byte[])"/>
		public virtual async Task<OperateResult> WriteAsync( string address, byte[] value ) => await ReadRpcAsync<string>( GetTopic( "WriteByteArray" ), new { address = address, value = value.ToHexString( ) } );


		/*************************************************************************************************************
		 * 
		 * Bool类型的读写，不一定所有的设备都实现，比如西门子，就没有实现bool[]的读写，Siemens的fetch/write没有实现bool操作
		 * 假设不进行任何的实现，那么就将使用同步代码来封装异步操作
		 * 
		 ************************************************************************************************************/

		/// <inheritdoc cref="IReadWriteNet.ReadBoolAsync(string, ushort)"/>
		public virtual async Task<OperateResult<bool[]>> ReadBoolAsync( string address, ushort length ) => await ReadRpcAsync<bool[]>( GetTopic( "ReadBoolArray" ), new { address = address, length = length } );

		/// <inheritdoc cref="IReadWriteNet.ReadBoolAsync(string)"/>
		public virtual async Task<OperateResult<bool>> ReadBoolAsync( string address ) => await ReadRpcAsync<bool>( GetTopic( "ReadBool" ), new { address = address } );

		/// <inheritdoc cref="IReadWriteNet.WriteAsync(string, bool[])"/>
		public virtual async Task<OperateResult> WriteAsync( string address, bool[] value ) => await ReadRpcAsync<string>( GetTopic( "WriteBoolArray" ), new { address = address, value = value } );

		/// <inheritdoc cref="IReadWriteNet.WriteAsync(string, bool)"/>
		public virtual async Task<OperateResult> WriteAsync( string address, bool value ) => await ReadRpcAsync<string>( GetTopic( "WriteBool" ), new { address = address, value = value } );
#endif
		#endregion

		#region Async Customer Read Write
#if !NET35 && !NET20
		/// <inheritdoc cref="IReadWriteNet.ReadCustomerAsync{T}(string)"/>
		public async Task<OperateResult<T>> ReadCustomerAsync<T>( string address ) where T : IDataTransfer, new() => await ReadWriteNetHelper.ReadCustomerAsync<T>( this, address );

		/// <inheritdoc cref="IReadWriteNet.ReadCustomerAsync{T}(string, T)"/>
		public async Task<OperateResult<T>> ReadCustomerAsync<T>( string address, T obj ) where T : IDataTransfer, new() => await ReadWriteNetHelper.ReadCustomerAsync( this, address, obj );

		/// <inheritdoc cref="IReadWriteNet.WriteCustomerAsync{T}(string, T)"/>
		public async Task<OperateResult> WriteCustomerAsync<T>( string address, T data ) where T : IDataTransfer, new() => await ReadWriteNetHelper.WriteCustomerAsync( this, address, data );
#endif
		#endregion

		#region Async Reflection Read Write
#if !NET35 && !NET20
		/// <inheritdoc cref="IReadWriteNet.ReadAsync{T}"/>
		public virtual async Task<OperateResult<T>> ReadAsync<T>( ) where T : class, new() => await HslReflectionHelper.ReadAsync<T>( this );

		/// <inheritdoc cref="IReadWriteNet.WriteAsync{T}(T)"/>
		public virtual async Task<OperateResult> WriteAsync<T>( T data ) where T : class, new() => await HslReflectionHelper.WriteAsync<T>( data, this );

		/// <inheritdoc cref="IReadWriteNet.ReadStruct{T}(string, ushort)"/>
		public virtual async Task<OperateResult<T>> ReadStructAsync<T>( string address, ushort length ) where T : class, new() => await ReadWriteNetHelper.ReadStructAsync<T>( this, address, length, this.ByteTransform );

#endif
		#endregion


		#region Async Read Support
#if !NET35 && !NET20
		/// <inheritdoc cref="IReadWriteNet.ReadInt16Async(string)"/>
		public async Task<OperateResult<short>> ReadInt16Async( string address ) => await ReadRpcAsync<short>( GetTopic( "ReadInt16" ), new { address = address } );

		/// <inheritdoc cref="IReadWriteNet.ReadInt16Async(string, ushort)"/>
		public virtual async Task<OperateResult<short[]>> ReadInt16Async( string address, ushort length ) => await ReadRpcAsync<short[]>( GetTopic( "ReadInt16Array" ), new { address = address, length = length } );

		/// <inheritdoc cref="IReadWriteNet.ReadUInt16Async(string)"/>
		public async Task<OperateResult<ushort>> ReadUInt16Async( string address ) => await ReadRpcAsync<ushort>( GetTopic( "ReadUInt16" ), new { address = address } );

		/// <inheritdoc cref="IReadWriteNet.ReadUInt16Async(string, ushort)"/>
		public virtual async Task<OperateResult<ushort[]>> ReadUInt16Async( string address, ushort length ) => await ReadRpcAsync<ushort[]>( GetTopic( "ReadUInt16Array" ), new { address = address, length = length } );

		/// <inheritdoc cref="IReadWriteNet.ReadInt32Async(string)"/>
		public async Task<OperateResult<int>> ReadInt32Async( string address ) => await ReadRpcAsync<int>( GetTopic( "ReadInt32" ), new { address = address } );

		/// <inheritdoc cref="IReadWriteNet.ReadInt32Async(string, ushort)"/>
		public virtual async Task<OperateResult<int[]>> ReadInt32Async( string address, ushort length ) => await ReadRpcAsync<int[]>( GetTopic( "ReadInt32Array" ), new { address = address, length = length } );

		/// <inheritdoc cref="IReadWriteNet.ReadUInt32Async(string)"/>
		public async Task<OperateResult<uint>> ReadUInt32Async( string address ) => await ReadRpcAsync<uint>( GetTopic( "ReadUInt32" ), new { address = address } );

		/// <inheritdoc cref="IReadWriteNet.ReadUInt32Async(string, ushort)"/>
		public virtual async Task<OperateResult<uint[]>> ReadUInt32Async( string address, ushort length ) => await ReadRpcAsync<uint[]>( GetTopic( "ReadUInt32Array" ), new { address = address, length = length } );

		/// <inheritdoc cref="IReadWriteNet.ReadFloatAsync(string)"/>
		public async Task<OperateResult<float>> ReadFloatAsync( string address ) => await ReadRpcAsync<float>( GetTopic( "ReadFloat" ), new { address = address } );

		/// <inheritdoc cref="IReadWriteNet.ReadFloatAsync(string, ushort)"/>
		public virtual async Task<OperateResult<float[]>> ReadFloatAsync( string address, ushort length ) => await ReadRpcAsync<float[]>( GetTopic( "ReadFloatArray" ), new { address = address, length = length } );

		/// <inheritdoc cref="IReadWriteNet.ReadInt64Async(string)"/>
		public async Task<OperateResult<long>> ReadInt64Async( string address ) => await ReadRpcAsync<long>( GetTopic( "ReadInt64" ), new { address = address } );

		/// <inheritdoc cref="IReadWriteNet.ReadInt64Async(string, ushort)"/>
		public virtual async Task<OperateResult<long[]>> ReadInt64Async( string address, ushort length ) => await ReadRpcAsync<long[]>( GetTopic( "ReadInt64Array" ), new { address = address, length = length } );

		/// <inheritdoc cref="IReadWriteNet.ReadUInt64Async(string)"/>
		public async Task<OperateResult<ulong>> ReadUInt64Async( string address ) => await ReadRpcAsync<ulong>( GetTopic( "ReadUInt64" ), new { address = address } );

		/// <inheritdoc cref="IReadWriteNet.ReadUInt64Async(string, ushort)"/>
		public virtual async Task<OperateResult<ulong[]>> ReadUInt64Async( string address, ushort length ) => await ReadRpcAsync<ulong[]>( GetTopic( "ReadUInt64Array" ), new { address = address, length = length } );

		/// <inheritdoc cref="IReadWriteNet.ReadDoubleAsync(string)"/>
		public async Task<OperateResult<double>> ReadDoubleAsync( string address ) => await ReadRpcAsync<double>( GetTopic( "ReadDouble" ), new { address = address } );

		/// <inheritdoc cref="IReadWriteNet.ReadDoubleAsync(string, ushort)"/>
		public virtual async Task<OperateResult<double[]>> ReadDoubleAsync( string address, ushort length ) => await ReadRpcAsync<double[]>( GetTopic( "ReadDoubleArray" ), new { address = address, length = length } );

		/// <inheritdoc cref="IReadWriteNet.ReadStringAsync(string, ushort)"/>
		public virtual async Task<OperateResult<string>> ReadStringAsync( string address, ushort length ) => await ReadRpcAsync<string>( GetTopic( "ReadString" ), new { address = address, length = length } );

		/// <inheritdoc cref="IReadWriteNet.ReadStringAsync(string, ushort, Encoding)"/>
		public virtual async Task<OperateResult<string>> ReadStringAsync( string address, ushort length, Encoding encoding ) => ByteTransformHelper.GetResultFromBytes( await ReadAsync( address, length ), m => ByteTransform.TransString( m, 0, m.Length, encoding ) );
#endif
		#endregion

		#region Async Write Support
#if !NET35 && !NET20
		/// <inheritdoc cref="IReadWriteNet.WriteAsync(string, short[])"/>
		public virtual async Task<OperateResult> WriteAsync( string address, short[] values ) => await ReadRpcAsync<string>( GetTopic( "WriteInt16Array" ), new { address = address, values = values } );

		/// <inheritdoc cref="IReadWriteNet.WriteAsync(string, short)"/>
		public virtual async Task<OperateResult> WriteAsync( string address, short value ) => await ReadRpcAsync<string>( GetTopic( "WriteInt16" ), new { address = address, value = value } );

		/// <inheritdoc cref="IReadWriteNet.WriteAsync(string, ushort[])"/>
		public virtual async Task<OperateResult> WriteAsync( string address, ushort[] values ) => await ReadRpcAsync<string>( GetTopic( "WriteUInt16Array" ), new { address = address, values = values } );

		/// <inheritdoc cref="IReadWriteNet.WriteAsync(string, ushort)"/>
		public virtual async Task<OperateResult> WriteAsync( string address, ushort value ) => await ReadRpcAsync<string>( GetTopic( "WriteUInt16" ), new { address = address, value = value } );

		/// <inheritdoc cref="IReadWriteNet.WriteAsync(string, int[])"/>
		public virtual async Task<OperateResult> WriteAsync( string address, int[] values ) => await ReadRpcAsync<string>( GetTopic( "WriteInt32Array" ), new { address = address, values = values } );

		/// <inheritdoc cref="IReadWriteNet.WriteAsync(string, int)"/>
		public async Task<OperateResult> WriteAsync( string address, int value ) => await ReadRpcAsync<string>( GetTopic( "WriteInt32" ), new { address = address, value = value } );

		/// <inheritdoc cref="IReadWriteNet.WriteAsync(string, uint[])"/>
		public virtual async Task<OperateResult> WriteAsync( string address, uint[] values ) => await ReadRpcAsync<string>( GetTopic( "WriteUInt32Array" ), new { address = address, values = values } );

		/// <inheritdoc cref="IReadWriteNet.WriteAsync(string, uint)"/>
		public async Task<OperateResult> WriteAsync( string address, uint value ) => await ReadRpcAsync<string>( GetTopic( "WriteUInt32" ), new { address = address, value = value } );

		/// <inheritdoc cref="IReadWriteNet.WriteAsync(string, float[])"/>
		public virtual async Task<OperateResult> WriteAsync( string address, float[] values ) => await ReadRpcAsync<string>( GetTopic( "WriteFloatArray" ), new { address = address, values = values } );

		/// <inheritdoc cref="IReadWriteNet.WriteAsync(string, float)"/>
		public async Task<OperateResult> WriteAsync( string address, float value ) => await ReadRpcAsync<string>( GetTopic( "WriteFloat" ), new { address = address, value = value } );

		/// <inheritdoc cref="IReadWriteNet.WriteAsync(string, long[])"/>
		public virtual async Task<OperateResult> WriteAsync( string address, long[] values ) => await ReadRpcAsync<string>( GetTopic( "WriteInt64Array" ), new { address = address, values = values } );

		/// <inheritdoc cref="IReadWriteNet.WriteAsync(string, long)"/>
		public async Task<OperateResult> WriteAsync( string address, long value ) => await ReadRpcAsync<string>( GetTopic( "WriteInt64" ), new { address = address, value = value } );

		/// <inheritdoc cref="IReadWriteNet.WriteAsync(string, ulong[])"/>
		public virtual async Task<OperateResult> WriteAsync( string address, ulong[] values ) => await ReadRpcAsync<string>( GetTopic( "WriteUInt64Array" ), new { address = address, values = values } );

		/// <inheritdoc cref="IReadWriteNet.WriteAsync(string, ulong)"/>
		public async Task<OperateResult> WriteAsync( string address, ulong value ) => await ReadRpcAsync<string>( GetTopic( "WriteUInt64" ), new { address = address, value = value } );

		/// <inheritdoc cref="IReadWriteNet.WriteAsync(string, double[])"/>
		public virtual async Task<OperateResult> WriteAsync( string address, double[] values ) => await ReadRpcAsync<string>( GetTopic( "WriteDoubleArray" ), new { address = address, values = values } );

		/// <inheritdoc cref="IReadWriteNet.WriteAsync(string, double)"/>
		public async Task<OperateResult> WriteAsync( string address, double value ) => await ReadRpcAsync<string>( GetTopic( "WriteDouble" ), new { address = address, value = value } );

		/// <inheritdoc cref="IReadWriteNet.WriteAsync(string, string)" />
		public virtual async Task<OperateResult> WriteAsync( string address, string value ) => await ReadRpcAsync<string>( GetTopic( "WriteString" ), new { address = address, value = value } );

		/// <inheritdoc cref="IReadWriteNet.WriteAsync(string, string, Encoding)"/>
		public virtual async Task<OperateResult> WriteAsync( string address, string value, Encoding encoding )
		{
			byte[] temp = ByteTransform.TransByte( value, encoding );
			return await WriteAsync( address, temp );
		}

		/// <inheritdoc cref="IReadWriteNet.WriteAsync(string, string, int)"/>
		public virtual async Task<OperateResult> WriteAsync( string address, string value, int length ) => await WriteAsync( address, value, length, Encoding.ASCII );

		/// <inheritdoc cref="IReadWriteNet.WriteAsync(string, string, int, Encoding)"/>
		public virtual async Task<OperateResult> WriteAsync( string address, string value, int length, Encoding encoding )
		{
			byte[] temp = ByteTransform.TransByte( value, encoding );
			temp = SoftBasic.ArrayExpandToLength( temp, length );
			return await WriteAsync( address, temp );
		}
#endif
		#endregion

		#region Private Member

		private string deviceTopic;                               // 设备关联的主题信息

		#endregion
	}
}
