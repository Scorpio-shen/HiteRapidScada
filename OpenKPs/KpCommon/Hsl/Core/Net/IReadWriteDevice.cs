﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if !NET20 && !NET35
using System.Threading.Tasks;
#endif

namespace HslCommunication.Core
{
	/// <summary>
	/// 用于读写的设备接口，相较于<see cref="IReadWriteNet"/>，增加了<see cref="ReadFromCoreServer(byte[])"/>相关的方法，可以用来和设备进行额外的交互。<br />
	/// The device interface used for reading and writing. Compared with <see cref="IReadWriteNet"/>, 
	/// a method related to <see cref="ReadFromCoreServer(byte[])"/> is added, which can be used for additional interaction with the device .
	/// </summary>
	public interface IReadWriteDevice : IReadWriteNet
	{
		#region Core Read

		/// <summary>
		/// 将当前的数据报文发送到设备去，具体使用什么通信方式取决于设备信息，然后从设备接收数据回来，并返回给调用者。<br />
		/// Send the current data message to the device, the specific communication method used depends on the device information, and then receive the data back from the device and return it to the caller.
		/// </summary>
		/// <param name="send">发送的完整的报文信息</param>
		/// <returns>接收的完整的报文信息</returns>
		/// <remarks>
		/// 本方法用于实现本组件还未实现的一些报文功能，例如有些modbus服务器会有一些特殊的功能码支持，需要收发特殊的报文，详细请看示例
		/// </remarks>
		OperateResult<byte[]> ReadFromCoreServer( byte[] send );

		/// <summary>
		/// 将多个数据报文按顺序发到设备，并从设备接收返回的数据内容，然后拼接成一个Byte[]信息，需要重写<see cref="HslCommunication.Core.Net.NetworkDoubleBase.UnpackResponseContent(byte[], byte[])"/>方法才能返回正确的结果。<br />
		/// Send multiple data packets to the device in sequence, and receive the returned data content from the device, and then splicing them into a Byte[] message, 
		/// you need to rewrite <see cref="HslCommunication.Core.Net.NetworkDoubleBase.UnpackResponseContent(byte[], byte[])"/> method to return the correct result.
		/// </summary>
		/// <param name="send">发送的报文列表信息</param>
		/// <returns>是否接收成功</returns>
		OperateResult<byte[]> ReadFromCoreServer( IEnumerable<byte[]> send );

#if !NET20 && !NET35
		/// <inheritdoc cref="ReadFromCoreServer(byte[])"/>
		Task<OperateResult<byte[]>> ReadFromCoreServerAsync( byte[] send );

		/// <inheritdoc cref="ReadFromCoreServer(IEnumerable{byte[]})"/>
		Task<OperateResult<byte[]>> ReadFromCoreServerAsync( IEnumerable<byte[]> send );
#endif
		#endregion

	}
}
