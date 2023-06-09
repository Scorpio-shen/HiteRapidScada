﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/**********************************************************************************************
 * 
 *    说明：一般的转换类，适应于C#语言，三菱PLC数据
 *    日期：2018年5月2日 16:05:56
 *    
 *    常规的数据转换继承自基类，并不需要进行变换运算
 * 
 **********************************************************************************************/


namespace HslCommunication.Core
{
	/// <summary>
	/// 常规的字节转换类<br />
	/// Regular byte conversion class
	/// </summary>
	public class RegularByteTransform : ByteTransformBase
	{
		#region Constructor

		/// <inheritdoc cref="ByteTransformBase()"/>
		public RegularByteTransform( ) { }

		/// <inheritdoc cref="ByteTransformBase(DataFormat)"/>
		public RegularByteTransform( DataFormat dataFormat ) : base( dataFormat ) { }

		#endregion

		/// <inheritdoc cref="IByteTransform.CreateByDateFormat(DataFormat)"/>
		public override IByteTransform CreateByDateFormat( DataFormat dataFormat ) => new RegularByteTransform( dataFormat ) { IsStringReverseByteWord = this.IsStringReverseByteWord };

		#region Object Override

		/// <inheritdoc/>
		public override string ToString( ) => $"RegularByteTransform[{DataFormat}]";

		#endregion
	}
}
