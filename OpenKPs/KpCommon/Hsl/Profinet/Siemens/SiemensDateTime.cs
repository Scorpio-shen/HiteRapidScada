﻿using HslCommunication.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HslCommunication.Profinet.Siemens
{
	/// <summary>
	/// Contains the methods to convert between <see cref="T:System.DateTime"/> and S7 representation of datetime values.
	/// </summary>
	/// <remarks>
	/// 这部分的代码参考了另一个s7的库，感谢原作者，此处贴出出处，遵循 MIT 协议
	/// 
	/// https://github.com/S7NetPlus/s7netplus
	/// </remarks>
	public class SiemensDateTime
	{
		/// <summary>
		/// The minimum <see cref="T:System.DateTime"/> value supported by the specification.
		/// </summary>
		public static readonly DateTime SpecMinimumDateTime = new DateTime( 1990, 1, 1 );

		/// <summary>
		/// The maximum <see cref="T:System.DateTime"/> value supported by the specification.
		/// </summary>
		public static readonly DateTime SpecMaximumDateTime = new DateTime( 2089, 12, 31, 23, 59, 59, 999 );

		/// <summary>
		/// Parses a <see cref="T:System.DateTime"/> value from bytes.
		/// </summary>
		/// <param name="bytes">Input bytes read from PLC.</param>
		/// <returns>A <see cref="T:System.DateTime"/> object representing the value read from PLC.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the length of
		///   <paramref name="bytes"/> is not 8 or any value in <paramref name="bytes"/>
		///   is outside the valid range of values.</exception>
		public static DateTime FromByteArray( byte[] bytes )
		{
			return FromByteArrayImpl( bytes );
		}

		/// <summary>
		/// 从西门子的原始字节数据中，提取出DTL格式的时间信息
		/// </summary>
		/// <param name="byteTransform">西门子的字节变换对象</param>
		/// <param name="buffer">原始字节数据</param>
		/// <param name="index">字节偏移索引</param>
		/// <returns>时间信息</returns>
		public static DateTime GetDTLTime( IByteTransform byteTransform, byte[] buffer, int index )
		{
			int year = byteTransform.TransInt16( buffer, index );
			int month = buffer[index + 2];
			int day = buffer[index + 3];
			int hour = buffer[index + 5];
			int minute = buffer[index + 6];
			int second = buffer[index + 7];
			int microsecond = byteTransform.TransInt32( buffer, index + 8 ) / 1000 / 1000;
			return new DateTime( year, month, day, hour, minute, second, microsecond );
		}

		/// <summary>
		/// 将时间数据转换为西门子的DTL格式的时间数据
		/// </summary>
		/// <param name="byteTransform">西门子的字节变换对象</param>
		/// <param name="dateTime">指定的时间信息</param>
		/// <returns>原始字节数据信息</returns>
		public static byte[] GetBytesFromDTLTime( IByteTransform byteTransform, DateTime dateTime )
		{
			byte[] buffer = new byte[12];
			byteTransform.TransByte( (short)dateTime.Year ).CopyTo( buffer, 0 );
			buffer[2] = (byte)dateTime.Month;
			buffer[3] = (byte)dateTime.Day;
			buffer[4] = 0x05;
			buffer[5] = (byte)dateTime.Hour;
			buffer[6] = (byte)dateTime.Minute;
			buffer[7] = (byte)dateTime.Second;
			byteTransform.TransByte( dateTime.Millisecond * 1000 * 1000 ).CopyTo( buffer, 8 );
			return buffer;
		}

		/// <summary>
		/// Parses an array of <see cref="T:System.DateTime"/> values from bytes.
		/// </summary>
		/// <param name="bytes">Input bytes read from PLC.</param>
		/// <returns>An array of <see cref="T:System.DateTime"/> objects representing the values read from PLC.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the length of
		///   <paramref name="bytes"/> is not a multiple of 8 or any value in
		///   <paramref name="bytes"/> is outside the valid range of values.</exception>
		public static DateTime[] ToArray( byte[] bytes )
		{
			if (bytes.Length % 8 != 0)
				throw new ArgumentOutOfRangeException( nameof( bytes ), bytes.Length,
					$"Parsing an array of DateTime requires a multiple of 8 bytes of input data, input data is '{bytes.Length}' long." );

			var cnt = bytes.Length / 8;
			var result = new System.DateTime[bytes.Length / 8];

			for (var i = 0; i < cnt; i++)
				result[i] = FromByteArrayImpl( new ArraySegment<byte>( bytes, i * 8, 8 ).Array );

			return result;
		}

		private static DateTime FromByteArrayImpl( IList<byte> bytes )
		{
			if (bytes.Count != 8)
				throw new ArgumentOutOfRangeException( nameof( bytes ), bytes.Count,
					$"Parsing a DateTime requires exactly 8 bytes of input data, input data is {bytes.Count} bytes long." );

			int DecodeBcd( byte input ) => 10 * (input >> 4) + (input & 0b00001111);

			int ByteToYear( byte bcdYear )
			{
				var input = DecodeBcd( bcdYear );
				if (input < 90) return input + 2000;
				if (input < 100) return input + 1900;

				throw new ArgumentOutOfRangeException( nameof( bcdYear ), bcdYear,
					$"Value '{input}' is higher than the maximum '99' of S7 date and time representation." );
			}

			int AssertRangeInclusive( int input, byte min, byte max, string field )
			{
				if (input < min)
					throw new ArgumentOutOfRangeException( nameof( input ), input,
						$"Value '{input}' is lower than the minimum '{min}' allowed for {field}." );
				if (input > max)
					throw new ArgumentOutOfRangeException( nameof( input ), input,
						$"Value '{input}' is higher than the maximum '{max}' allowed for {field}." );

				return input;
			}

			var year = ByteToYear( bytes[0] );
			var month = AssertRangeInclusive( DecodeBcd( bytes[1] ), 1, 12, "month" );
			var day = AssertRangeInclusive( DecodeBcd( bytes[2] ), 1, 31, "day of month" );
			var hour = AssertRangeInclusive( DecodeBcd( bytes[3] ), 0, 23, "hour" );
			var minute = AssertRangeInclusive( DecodeBcd( bytes[4] ), 0, 59, "minute" );
			var second = AssertRangeInclusive( DecodeBcd( bytes[5] ), 0, 59, "second" );
			var hsec = AssertRangeInclusive( DecodeBcd( bytes[6] ), 0, 99, "first two millisecond digits" );
			var msec = AssertRangeInclusive( bytes[7] >> 4, 0, 9, "third millisecond digit" );
			var dayOfWeek = AssertRangeInclusive( bytes[7] & 0b00001111, 1, 7, "day of week" );

			return new System.DateTime( year, month, day, hour, minute, second, hsec * 10 + msec );
		}

		/// <summary>
		/// Converts a <see cref="T:System.DateTime"/> value to a byte array.
		/// </summary>
		/// <param name="dateTime">The DateTime value to convert.</param>
		/// <returns>A byte array containing the S7 date time representation of <paramref name="dateTime"/>.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the value of
		///   <paramref name="dateTime"/> is before <see cref="P:SpecMinimumDateTime"/>
		///   or after <see cref="P:SpecMaximumDateTime"/>.</exception>
		public static byte[] ToByteArray( DateTime dateTime )
		{
			byte EncodeBcd( int value )
			{
				return (byte)((value / 10 << 4) | value % 10);
			}

			if (dateTime < SpecMinimumDateTime)
				throw new ArgumentOutOfRangeException( nameof( dateTime ), dateTime,
					$"Date time '{dateTime}' is before the minimum '{SpecMinimumDateTime}' supported in S7 date time representation." );

			if (dateTime > SpecMaximumDateTime)
				throw new ArgumentOutOfRangeException( nameof( dateTime ), dateTime,
					$"Date time '{dateTime}' is after the maximum '{SpecMaximumDateTime}' supported in S7 date time representation." );

			byte MapYear( int year ) => (byte)(year < 2000 ? year - 1900 : year - 2000);

			int DayOfWeekToInt( DayOfWeek dayOfWeek ) => (int)dayOfWeek + 1;

			return new[]
			{
				EncodeBcd(MapYear(dateTime.Year)),
				EncodeBcd(dateTime.Month),
				EncodeBcd(dateTime.Day),
				EncodeBcd(dateTime.Hour),
				EncodeBcd(dateTime.Minute),
				EncodeBcd(dateTime.Second),
				EncodeBcd(dateTime.Millisecond / 10),
				(byte) (dateTime.Millisecond % 10 << 4 | DayOfWeekToInt(dateTime.DayOfWeek))
			};
		}

		/// <summary>
		/// Converts an array of <see cref="T:System.DateTime"/> values to a byte array.
		/// </summary>
		/// <param name="dateTimes">The DateTime values to convert.</param>
		/// <returns>A byte array containing the S7 date time representations of <paramref name="dateTimes"/>.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when any value of
		///   <paramref name="dateTimes"/> is before <see cref="P:SpecMinimumDateTime"/>
		///   or after <see cref="P:SpecMaximumDateTime"/>.</exception>
		public static byte[] ToByteArray( System.DateTime[] dateTimes )
		{
			var bytes = new List<byte>( dateTimes.Length * 8 );
			foreach (var dateTime in dateTimes) bytes.AddRange( ToByteArray( dateTime ) );

			return bytes.ToArray( );
		}
	}
}
