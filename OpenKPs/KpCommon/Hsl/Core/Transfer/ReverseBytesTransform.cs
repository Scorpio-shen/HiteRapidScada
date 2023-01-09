using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/**********************************************************************************************
 * 
 *    说明：一般的转换类
 *    日期：2018年3月14日 17:05:30
 * 
 **********************************************************************************************/

namespace HslCommunication.Core
{
	/// <summary>
	/// 字节倒序的转换类，字节的顺序和C#的原生字节的顺序是完全相反的，高字节在前，低字节在后。<br />
	/// In the reverse byte order conversion class, the byte order is completely opposite to the native byte order of C#, 
	/// with the high byte first and the low byte following.
	/// </summary>
	/// <remarks>
	/// 适用西门子PLC的S7协议的数据转换
	/// </remarks>
	public class ReverseBytesTransform : ByteTransformBase
	{
		#region Constructor

		/// <inheritdoc cref="ByteTransformBase()"/>
		public ReverseBytesTransform( ) 
		{
			DataFormat = DataFormat.ABCD;
		}

		/// <inheritdoc cref="ByteTransformBase(DataFormat)"/>
		public ReverseBytesTransform( DataFormat dataFormat ) : base( dataFormat ) { }

		#endregion

		#region Get Value From Bytes

		/// <inheritdoc cref="IByteTransform.TransInt16(byte[], int)"/>
		public override short TransInt16( byte[] buffer, int index )
		{
			byte[] tmp = new byte[2];
			tmp[0] = buffer[1 + index];
			tmp[1] = buffer[0 + index];
			return BitConverter.ToInt16( tmp, 0 );
		}

		/// <inheritdoc cref="IByteTransform.TransUInt16(byte[], int)"/>
		public override ushort TransUInt16( byte[] buffer, int index )
		{
			byte[] tmp = new byte[2];
			tmp[0] = buffer[1 + index];
			tmp[1] = buffer[0 + index];
			return BitConverter.ToUInt16( tmp, 0 );
		}

		#endregion

		#region Get Bytes From Value

		/// <inheritdoc cref="IByteTransform.TransByte(short[])"/>
		public override byte[] TransByte( short[] values )
		{
			if (values == null) return null;

			byte[] buffer = new byte[values.Length * 2];
			for (int i = 0; i < values.Length; i++)
			{
				byte[] tmp = BitConverter.GetBytes( values[i] );
				Array.Reverse( tmp );
				tmp.CopyTo( buffer, 2 * i );
			}

			return buffer;
		}

		/// <inheritdoc cref="IByteTransform.TransByte(ushort[])"/>
		public override byte[] TransByte( ushort[] values )
		{
			if (values == null) return null;

			byte[] buffer = new byte[values.Length * 2];
			for (int i = 0; i < values.Length; i++)
			{
				byte[] tmp = BitConverter.GetBytes( values[i] );
				Array.Reverse( tmp );
				tmp.CopyTo( buffer, 2 * i );
			}

			return buffer;
		}

		#endregion

		/// <inheritdoc cref="IByteTransform.CreateByDateFormat(DataFormat)"/>
		public override IByteTransform CreateByDateFormat( DataFormat dataFormat ) => new ReverseBytesTransform( dataFormat ) { IsStringReverseByteWord = this.IsStringReverseByteWord };

		#region Object Override

		///<inheritdoc/>
		public override string ToString( ) => $"ReverseBytesTransform[{DataFormat}]";

		#endregion
	}
}
