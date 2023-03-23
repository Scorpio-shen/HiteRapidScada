using HslCommunication.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if !NET35 && !NET20
using System.Threading.Tasks;
#endif
namespace HslCommunication.Profinet.AllenBradley
{
	/// <summary>
	/// CIP协议的基础接口信息
	/// </summary>
	public interface IReadWriteCip : IReadWriteNet
	{

		/// <summary>
		/// 使用指定的类型写入指定的节点数据，类型信息参考API文档，地址支持携带类型代号信息，可以强制指定本次写入数据的类型信息，例如 "type=0xD1;A"<br />
		/// Use the specified type to write the specified node data. For the type information, refer to the API documentation. The address supports carrying type code information. 
		/// You can force the type information of the data to be written this time. For example, "type=0xD1;A"
		/// </summary>
		/// <remarks>
		/// 关于参数 length 的含义，表示的是地址长度，一般的标量数据都是 1，如果PLC有个标签是 A，数据类型为 byte[10]，那我们写入 3 个byte就是 WriteTag( "A[5]", 0xD1, new byte[]{1,2,3}, 3 );<br />
		/// Regarding the meaning of the parameter length, it represents the address length. The general scalar data is 1. If the PLC has a tag of A and the data type is byte[10], then we write 3 bytes as WriteTag( "A[5 ]", 0xD1, new byte[]{1,2,3}, 3 );
		/// </remarks>
		/// <param name="address">节点的名称 -> Name of the node </param>
		/// <param name="typeCode">类型代码，详细参见<see cref="AllenBradleyHelper"/>上的常用字段 ->  Type code, see the commonly used Fields section on the <see cref= "AllenBradleyHelper"/> in detail</param>
		/// <param name="value">实际的数据值 -> The actual data value </param>
		/// <param name="length">如果节点是数组，就是数组长度 -> If the node is an array, it is the array length </param>
		/// <returns>是否写入成功 -> Whether to write successfully</returns>
		OperateResult WriteTag( string address, ushort typeCode, byte[] value, int length = 1 );

		#if !NET35 && !NET20
		/// <inheritdoc cref="WriteTag(string, ushort, byte[], int)"/>
		Task<OperateResult> WriteTagAsync( string address, ushort typeCode, byte[] value, int length = 1 );
#endif

		/// <inheritdoc cref="HslCommunication.Core.Net.NetworkDoubleBase.ByteTransform"/>
		IByteTransform ByteTransform { get; set; }
	}
}
