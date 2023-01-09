using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HslCommunication.BasicFramework;

namespace HslCommunication.Core
{
	/// <summary>
	/// 按照字节错位的数据转换类<br />
	/// Data conversion class according to byte misalignment
	/// </summary>
	public class ReverseWordTransform : ByteTransformBase
	{
		#region Constructor

		/// <inheritdoc cref="ByteTransformBase()"/>
		public ReverseWordTransform( )
		{
			this.DataFormat = DataFormat.ABCD;
			this.IsInteger16Reverse = true;
		}

		/// <inheritdoc cref="ByteTransformBase(DataFormat)"/>
		public ReverseWordTransform( DataFormat dataFormat ) : base( dataFormat )
		{
			this.IsInteger16Reverse = true;
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// 获取或设置双字节的整数是否进行翻转操作，主要针对的类型为 <see cref="short"/> 和 <see cref="ushort"/><br />
		/// Get or set whether the double-byte integer is to be flipped, the main types are <see cref="short"/> and <see cref="ushort"/>
		/// </summary>
		/// <remarks>
		/// 默认为 <c>True</c>，即发生数据翻转，当修改为 <c>False</c> 时，和C#的字节顺序一致
		/// </remarks>
		public bool IsInteger16Reverse { get; set; }

		#endregion

		#region Get Short From Bytes

		/// <inheritdoc cref="IByteTransform.TransInt16(byte[], int)"/>
		public override short TransInt16( byte[] buffer, int index )
		{
			if (IsInteger16Reverse)
			{
				byte[] tmp = new byte[2];
				tmp[0] = buffer[index + 1];
				tmp[1] = buffer[index + 0];
				return BitConverter.ToInt16( tmp, 0 );
			}
			else
				return base.TransInt16( buffer, index);
		}

		/// <inheritdoc cref="IByteTransform.TransUInt16(byte[], int)"/>
		public override ushort TransUInt16( byte[] buffer, int index )
		{
			if (IsInteger16Reverse)
			{
				byte[] byt = new byte[2];
				byt[0] = buffer[index + 1];
				byt[1] = buffer[index + 0];
				return BitConverter.ToUInt16( byt, 0 );
			}
			else
				return base.TransUInt16( buffer, index );
		}

		#endregion

		#region Get Bytes From Short

		/// <inheritdoc cref="IByteTransform.TransByte(short[])"/>
		public override byte[] TransByte( short[] values )
		{
			byte[] buffer = base.TransByte( values );
			return IsInteger16Reverse ? SoftBasic.BytesReverseByWord( buffer ) : buffer;
		}

		/// <inheritdoc cref="IByteTransform.TransByte(ushort[])"/>
		public override byte[] TransByte( ushort[] values )
		{
			byte[] buffer = base.TransByte( values );
			return IsInteger16Reverse ? SoftBasic.BytesReverseByWord( buffer ) : buffer;
		}

		#endregion

		/// <inheritdoc cref="IByteTransform.CreateByDateFormat(DataFormat)"/>
		public override IByteTransform CreateByDateFormat( DataFormat dataFormat ) => new ReverseWordTransform( dataFormat ) 
		{ 
			IsStringReverseByteWord = this.IsStringReverseByteWord,
			IsInteger16Reverse      = this.IsInteger16Reverse
		};

		#region Object Override

		/// <inheritdoc/>
		public override string ToString( ) => $"ReverseWordTransform[{DataFormat}]";

		#endregion
	}
}
