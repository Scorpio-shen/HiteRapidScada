using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HslCommunication.Instrument.IEC
{
	/// <summary>
	/// IEC的数据对象，带值，品质信息，地址信息，时标信息
	/// </summary>
	/// <typeparam name="T">数据的类型</typeparam>
	public class IecValueObject<T>
	{
		/// <summary>
		/// 值信息
		/// </summary>
		public T Value { get; set; }

		/// <summary>
		/// 品质数据
		/// </summary>
		public byte Quality { get; set; }

		/// <summary>
		/// 时间
		/// </summary>
		public DateTime Time { get; set; }

		/// <summary>
		/// 地址
		/// </summary>
		public int Address { get; set; }

		/// <summary>
		/// 解析出一个浮点数的数据对象
		/// </summary>
		/// <param name="source">原始字节信息</param>
		/// <param name="index">起始偏移地址，字节为单位</param>
		/// <param name="includeTime">是否包含时标</param>
		/// <returns>浮点数的结果数据</returns>
		public static IecValueObject<float> PraseFloat( byte[] source, int index, bool includeTime = true )
		{
			IecValueObject<float> iec = new IecValueObject<float>( );
			iec.Address          = BitConverter.ToUInt16( source, index );
			iec.Value            = BitConverter.ToSingle( source, index + 3 );
			iec.Quality          = source[index + 7];
			if (includeTime && source.Length >= index + 8 + 7) 
				iec.Time = Helper.IECHelper.PraseTimeFromAbsoluteTimeScale( source, index + 8 );
			return iec;
		}
	}
}
