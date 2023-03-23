﻿using HslCommunication.BasicFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Linq.Expressions;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.IO;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System.Threading;

namespace HslCommunication
{
	/// <summary>
	/// 扩展的辅助类方法
	/// </summary>
	public static class HslExtension
	{
		/// <inheritdoc cref="SoftBasic.ByteToHexString(byte[])"/>
		public static string ToHexString( this byte[] InBytes ) => SoftBasic.ByteToHexString( InBytes );

		/// <inheritdoc cref="SoftBasic.ByteToHexString(byte[], char)"/>
		public static string ToHexString( this byte[] InBytes, char segment ) => SoftBasic.ByteToHexString( InBytes, segment );

		/// <inheritdoc cref="SoftBasic.ByteToHexString(byte[], char, int, string)"/>
		public static string ToHexString( this byte[] InBytes, char segment, int newLineCount, string format = "{0:X2}" ) => SoftBasic.ByteToHexString( InBytes, segment, newLineCount, format );

		/// <inheritdoc cref="SoftBasic.HexStringToBytes( string )"/>
		public static byte[] ToHexBytes( this string value ) => SoftBasic.HexStringToBytes( value );

		/// <inheritdoc cref="SoftBasic.BoolArrayToByte"/>
		public static byte[] ToByteArray( this bool[] array ) => SoftBasic.BoolArrayToByte( array );

		/// <inheritdoc cref="SoftBasic.ByteToBoolArray(byte[],int)"/>
		public static bool[] ToBoolArray( this byte[] InBytes, int length ) => SoftBasic.ByteToBoolArray( InBytes, length );

		/// <summary>
		/// 获取当前数组的倒序数组，这是一个新的实例，不改变原来的数组值<br />
		/// Get the reversed array of the current byte array, this is a new instance, does not change the original array value
		/// </summary>
		/// <param name="value">输入的原始数组</param>
		/// <returns>反转之后的数组信息</returns>
		public static T[] ReverseNew<T>( this T[] value )
		{
			T[] buffer = value.CopyArray( );
			Array.Reverse( buffer );
			return buffer;
		}

		/// <inheritdoc cref="SoftBasic.ByteToBoolArray(byte[])"/>
		public static bool[] ToBoolArray( this byte[] InBytes ) => SoftBasic.ByteToBoolArray( InBytes );

		/// <summary>
		/// 获取Byte数组的第 bytIndex 个位置的，boolIndex偏移的bool值<br />
		/// Get the bool value of the bytIndex position of the Byte array and the boolIndex offset
		/// </summary>
		/// <param name="bytes">字节数组信息</param>
		/// <param name="bytIndex">字节的偏移位置</param>
		/// <param name="boolIndex">指定字节的位偏移</param>
		/// <returns>bool值</returns>
		public static bool GetBoolValue( this byte[] bytes, int bytIndex, int boolIndex )
		{
			return SoftBasic.BoolOnByteIndex( bytes[bytIndex], boolIndex );
		}

		/// <summary>
		/// 获取Byte数组的第 boolIndex 偏移的bool值，这个偏移值可以为 10，就是第 1 个字节的 第3位 <br />
		/// Get the bool value of the boolIndex offset of the Byte array. The offset value can be 10, which is the third bit of the first byte
		/// </summary>
		/// <param name="bytes">字节数组信息</param>
		/// <param name="boolIndex">指定字节的位偏移</param>
		/// <returns>bool值</returns>
		public static bool GetBoolByIndex( this byte[] bytes, int boolIndex )
		{
			return SoftBasic.BoolOnByteIndex( bytes[boolIndex / 8], boolIndex % 8 );
		}

		/// <summary>
		/// 获取Byte的第 boolIndex 偏移的bool值，比如3，就是第4位 <br />
		/// Get the bool value of Byte's boolIndex offset, such as 3, which is the 4th bit
		/// </summary>
		/// <param name="byt">字节信息</param>
		/// <param name="boolIndex">指定字节的位偏移</param>
		/// <returns>bool值</returns>
		public static bool GetBoolByIndex( this byte byt, int boolIndex )
		{
			return SoftBasic.BoolOnByteIndex( byt, boolIndex % 8 );
		}

		/// <summary>
		/// 获取short类型数据的第 boolIndex (从0起始)偏移的bool值，比如3，就是第4位 <br />
		/// Get the bool value of the boolIndex (starting from 0) offset of the short type data, such as 3, which is the 4th bit
		/// </summary>
		/// <param name="value">short数据值</param>
		/// <param name="boolIndex">位偏移索引，从0开始</param>
		/// <returns>bool值</returns>
		public static bool GetBoolByIndex( this short value, int boolIndex ) => BitConverter.GetBytes( value ).GetBoolByIndex( boolIndex );

		/// <summary>
		/// 获取ushort类型数据的第 boolIndex (从0起始)偏移的bool值，比如3，就是第4位 <br />
		/// Get the bool value of the boolIndex (starting from 0) offset of the ushort type data, such as 3, which is the 4th bit
		/// </summary>
		/// <param name="value">ushort数据值</param>
		/// <param name="boolIndex">位偏移索引，从0开始</param>
		/// <returns>bool值</returns>
		public static bool GetBoolByIndex( this ushort value, int boolIndex ) => BitConverter.GetBytes( value ).GetBoolByIndex( boolIndex );

		/// <summary>
		/// 获取int类型数据的第 boolIndex (从0起始)偏移的bool值，比如3，就是第4位 <br />
		/// Get the bool value of the boolIndex (starting from 0) offset of the int type data, such as 3, which is the 4th bit
		/// </summary>
		/// <param name="value">int数据值</param>
		/// <param name="boolIndex">位偏移索引，从0开始</param>
		/// <returns>bool值</returns>
		public static bool GetBoolByIndex( this int value, int boolIndex ) => BitConverter.GetBytes( value ).GetBoolByIndex( boolIndex );

		/// <summary>
		/// 获取uint类型数据的第 boolIndex (从0起始)偏移的bool值，比如3，就是第4位 <br />
		/// Get the bool value of the boolIndex (starting from 0) offset of the uint type data, such as 3, which is the 4th bit
		/// </summary>
		/// <param name="value">uint数据值</param>
		/// <param name="boolIndex">位偏移索引，从0开始</param>
		/// <returns>bool值</returns>
		public static bool GetBoolByIndex( this uint value, int boolIndex ) => BitConverter.GetBytes( value ).GetBoolByIndex( boolIndex );

		/// <summary>
		/// 获取long类型数据的第 boolIndex (从0起始)偏移的bool值，比如3，就是第4位 <br />
		/// Get the bool value of the boolIndex (starting from 0) offset of the long type data, such as 3, which is the 4th bit
		/// </summary>
		/// <param name="value">long数据值</param>
		/// <param name="boolIndex">位偏移索引，从0开始</param>
		/// <returns>bool值</returns>
		public static bool GetBoolByIndex( this long value, int boolIndex ) => BitConverter.GetBytes( value ).GetBoolByIndex( boolIndex );

		/// <summary>
		/// 获取ulong类型数据的第 boolIndex (从0起始)偏移的bool值，比如3，就是第4位 <br />
		/// Get the bool value of the boolIndex (starting from 0) offset of the ulong type data, such as 3, which is the 4th bit
		/// </summary>
		/// <param name="value">ulong数据值</param>
		/// <param name="boolIndex">位偏移索引，从0开始</param>
		/// <returns>bool值</returns>
		public static bool GetBoolByIndex( this ulong value, int boolIndex ) => BitConverter.GetBytes( value ).GetBoolByIndex( boolIndex );

		/// <summary>
		/// 从字节数组里提取字符串数据，如果碰到0x00字节，就直接结束
		/// </summary>
		/// <param name="buffer">原始字节信息</param>
		/// <param name="index">起始的偏移地址</param>
		/// <param name="length">字节长度信息</param>
		/// <param name="encoding">编码</param>
		/// <returns>字符串信息</returns>
		public static string GetStringOrEndChar( this byte[] buffer, int index, int length, Encoding encoding )
		{
			for (int i = index; i < index + length; i++)
			{
				if (buffer[i] == 0x00)
				{
					length = i - index;
					break;
				}
			}
			return Encoding.UTF8.GetString( buffer, index, length );
		}

		/// <summary>
		/// 设置Byte的第 boolIndex 位的bool值，可以强制为 true 或是 false, 不影响其他的位<br />
		/// Set the bool value of the boolIndex bit of Byte, which can be forced to true or false, without affecting other bits
		/// </summary>
		/// <param name="byt">字节信息</param>
		/// <param name="boolIndex">指定字节的位偏移</param>
		/// <param name="value">bool的值</param>
		/// <returns>修改之后的byte值</returns>
		public static byte SetBoolByIndex( this byte byt, int boolIndex, bool value ) => SoftBasic.SetBoolOnByteIndex( byt, boolIndex, value );

		/// <summary>
		/// 设置Byte[]的第 boolIndex 位的bool值，可以强制为 true 或是 false, 不影响其他的位，如果是第 10 位，则表示第 1 个字节的第 2 位（都是从 0 地址开始算的）<br />
		/// Set the bool value of the boolIndex bit of Byte[], which can be forced to true or false, without affecting other bits. 
		/// If it is the 10th bit, it means the second bit of the first byte (both starting from the 0 address Calculated)
		/// </summary>
		/// <param name="buffer">字节数组信息</param>
		/// <param name="boolIndex">位偏移的索引</param>
		/// <param name="value">bool的值</param>
		public static void SetBoolByIndex( this byte[] buffer, int boolIndex, bool value ) => buffer[boolIndex / 8] = buffer[boolIndex / 8].SetBoolByIndex( boolIndex % 8, value );

		/// <summary>
		/// 修改short数据的某个位，并且返回修改后的值，不影响原来的值。位索引为 0~15，之外的值会引发异常<br />
		/// Modify a bit of short data and return the modified value without affecting the original value. Bit index is 0~15, values outside will raise an exception
		/// </summary>
		/// <param name="shortValue">等待修改的short值</param>
		/// <param name="boolIndex">位索引，位索引为 0~15，之外的值会引发异常</param>
		/// <param name="value">bool值</param>
		/// <returns>修改之后的short值</returns>
		public static short SetBoolByIndex( this short shortValue, int boolIndex, bool value )
		{
			byte[] buffer = BitConverter.GetBytes( shortValue );
			buffer.SetBoolByIndex( boolIndex, value );
			return BitConverter.ToInt16( buffer, 0 );
		}

		/// <summary>
		/// 修改ushort数据的某个位，并且返回修改后的值，不影响原来的值。位索引为 0~15，之外的值会引发异常<br />
		/// Modify a bit of ushort data and return the modified value without affecting the original value. Bit index is 0~15, values outside will raise an exception
		/// </summary>
		/// <param name="ushortValue">等待修改的ushort值</param>
		/// <param name="boolIndex">位索引，位索引为 0~15，之外的值会引发异常</param>
		/// <param name="value">bool值</param>
		/// <returns>修改之后的ushort值</returns>
		public static ushort SetBoolByIndex( this ushort ushortValue, int boolIndex, bool value )
		{
			byte[] buffer = BitConverter.GetBytes( ushortValue );
			buffer.SetBoolByIndex( boolIndex, value );
			return BitConverter.ToUInt16( buffer, 0 );
		}

		/// <summary>
		/// 修改int数据的某个位，并且返回修改后的值，不影响原来的值。位索引为 0~31，之外的值会引发异常<br />
		/// Modify a bit of int data and return the modified value without affecting the original value. Bit index is 0~31, values outside will raise an exception
		/// </summary>
		/// <param name="intValue">等待修改的int值</param>
		/// <param name="boolIndex">位索引，位索引为 0~31，之外的值会引发异常</param>
		/// <param name="value">bool值</param>
		/// <returns>修改之后的int值</returns>
		public static int SetBoolByIndex( this int intValue, int boolIndex, bool value )
		{
			byte[] buffer = BitConverter.GetBytes( intValue );
			buffer.SetBoolByIndex( boolIndex, value );
			return BitConverter.ToInt32( buffer, 0 );
		}

		/// <summary>
		/// 修改uint数据的某个位，并且返回修改后的值，不影响原来的值。位索引为 0~31，之外的值会引发异常<br />
		/// Modify a bit of uint data and return the modified value without affecting the original value. Bit index is 0~31, values outside will raise an exception
		/// </summary>
		/// <param name="uintValue">等待修改的uint值</param>
		/// <param name="boolIndex">位索引，位索引为 0~31，之外的值会引发异常</param>
		/// <param name="value">bool值</param>
		/// <returns>修改之后的uint值</returns>
		public static uint SetBoolByIndex( this uint uintValue, int boolIndex, bool value )
		{
			byte[] buffer = BitConverter.GetBytes( uintValue );
			buffer.SetBoolByIndex( boolIndex, value );
			return BitConverter.ToUInt32( buffer, 0 );
		}

		/// <summary>
		/// 修改long数据的某个位，并且返回修改后的值，不影响原来的值。位索引为 0~63，之外的值会引发异常<br />
		/// Modify a bit of long data and return the modified value without affecting the original value. Bit index is 0~63, values outside will raise an exception
		/// </summary>
		/// <param name="longValue">等待修改的long值</param>
		/// <param name="boolIndex">位索引，位索引为 0~63，之外的值会引发异常</param>
		/// <param name="value">bool值</param>
		/// <returns>修改之后的long值</returns>
		public static long SetBoolByIndex( this long longValue, int boolIndex, bool value )
		{
			byte[] buffer = BitConverter.GetBytes( longValue );
			buffer.SetBoolByIndex( boolIndex, value );
			return BitConverter.ToInt64( buffer, 0 );
		}

		/// <summary>
		/// 修改ulong数据的某个位，并且返回修改后的值，不影响原来的值。位索引为 0~63，之外的值会引发异常<br />
		/// Modify a bit of ulong data and return the modified value without affecting the original value. Bit index is 0~63, values outside will raise an exception
		/// </summary>
		/// <param name="ulongValue">等待修改的ulong值</param>
		/// <param name="boolIndex">位索引，位索引为 0~63，之外的值会引发异常</param>
		/// <param name="value">bool值</param>
		/// <returns>修改之后的ulong值</returns>
		public static ulong SetBoolByIndex( this ulong ulongValue, int boolIndex, bool value )
		{
			byte[] buffer = BitConverter.GetBytes( ulongValue );
			buffer.SetBoolByIndex( boolIndex, value );
			return BitConverter.ToUInt64( buffer, 0 );
		}

		/// <inheritdoc cref="SoftBasic.ArrayRemoveDouble"/>
		public static T[] RemoveDouble<T>( this T[] value, int leftLength, int rightLength ) => SoftBasic.ArrayRemoveDouble( value, leftLength, rightLength );

		/// <inheritdoc cref="SoftBasic.ArrayRemoveBegin"/>
		public static T[] RemoveBegin<T>( this T[] value, int length ) => SoftBasic.ArrayRemoveBegin( value, length );

		/// <inheritdoc cref="SoftBasic.ArrayRemoveLast"/>
		public static T[] RemoveLast<T>( this T[] value, int length ) => SoftBasic.ArrayRemoveLast( value, length );

		/// <inheritdoc cref="SoftBasic.ArraySelectMiddle"/>
		public static T[] SelectMiddle<T>( this T[] value, int index, int length ) => SoftBasic.ArraySelectMiddle( value, index, length );

		/// <inheritdoc cref="SoftBasic.ArraySelectBegin"/>
		public static T[] SelectBegin<T>( this T[] value, int length ) => SoftBasic.ArraySelectBegin( value, length );

		/// <inheritdoc cref="SoftBasic.ArraySelectLast"/>
		public static T[] SelectLast<T>( this T[] value, int length ) => SoftBasic.ArraySelectLast( value, length );

		/// <inheritdoc cref="SoftBasic.GetValueFromJsonObject{T}(JObject, string, T)"/>
		public static T GetValueOrDefault<T>( JObject jObject, string name, T defaultValue ) => SoftBasic.GetValueFromJsonObject<T>( jObject, name, defaultValue );

		/// <inheritdoc cref="SoftBasic.SpliceArray{T}(T[][])"/>
		public static T[] SpliceArray<T>( this T[] value, params T[][] arrays )
		{
			List<T[]> list = new List<T[]>( arrays.Length + 1 );
			list.Add( value );
			list.AddRange( arrays );
			return SoftBasic.SpliceArray( list.ToArray( ) );
		}

		/// <summary>
		/// 移除指定字符串数据的最后 length 个字符。如果字符串本身的长度不足 length，则返回为空字符串。<br />
		/// Remove the last "length" characters of the specified string data. If the length of the string itself is less than length, 
		/// an empty string is returned.
		/// </summary>
		/// <param name="value">等待操作的字符串数据</param>
		/// <param name="length">准备移除的长度信息</param>
		/// <returns>移除之后的数据信息</returns>
		public static string RemoveLast( this string value, int length )
		{
			if (value == null) return null;
			if (value.Length < length) return string.Empty;
			return value.Remove( value.Length - length );
		}

		/// <summary>
		/// 将指定的数据添加到数组的每个元素上去，会改变每个元素的值
		/// </summary>
		/// <param name="array">原始数组</param>
		/// <param name="value">值</param>
		/// <returns>修改后的数组信息</returns>
		public static byte[] EveryByteAdd( this byte[] array, int value )
		{
			if ( array == null ) return null;
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = (byte)(array[i] + value);
			}
			return array;
		}

		/// <summary>
		/// 将指定的数据添加到数组的每个元素上去，使用表达式树的形式实现，将会修改原数组。不适用byte类型
		/// </summary>
		/// <typeparam name="T">数组的类型</typeparam>
		/// <param name="array">原始数据</param>
		/// <param name="value">数据值</param>
		/// <returns>返回的结果信息</returns>
		public static T[] IncreaseBy<T>( this T[] array, T value )
		{
			if (typeof( T ) == typeof( byte ))
			{
				ParameterExpression firstArg = Expression.Parameter( typeof( int ), "first" );
				ParameterExpression secondArg = Expression.Parameter( typeof( int ), "second" );
				Expression body = Expression.Add( firstArg, secondArg );

				Expression<Func<int, int, int>> adder = Expression.Lambda<Func<int, int, int>>( body, firstArg, secondArg );
				Func<int, int, int> addDelegate = adder.Compile( );
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = (T)(object)(byte)addDelegate( Convert.ToInt32( array[i] ), Convert.ToInt32( value ) );
				}
			}
			else
			{
				ParameterExpression firstArg = Expression.Parameter( typeof( T ), "first" );
				ParameterExpression secondArg = Expression.Parameter( typeof( T ), "second" );
				Expression body = Expression.Add( firstArg, secondArg );

				Expression<Func<T, T, T>> adder = Expression.Lambda<Func<T, T, T>>( body, firstArg, secondArg );
				Func<T, T, T> addDelegate = adder.Compile( );
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = addDelegate( array[i], value );
				}
			}

			return array;
		}

		/// <summary>
		/// 拷贝当前的实例数组，是基于引用层的浅拷贝，如果类型为值类型，那就是深度拷贝，如果类型为引用类型，就是浅拷贝
		/// </summary>
		/// <typeparam name="T">类型对象</typeparam>
		/// <param name="value">数组对象</param>
		/// <returns>拷贝的结果内容</returns>
		public static T[] CopyArray<T>( this T[] value )
		{
			if (value == null) return null;
			T[] buffer = new T[value.Length];
			Array.Copy( value, buffer, value.Length );
			return buffer;
		}

		/// <inheritdoc cref="SoftBasic.ArrayFormat{T}(T[])"/>
		public static string ToArrayString<T>( this T[] value ) => SoftBasic.ArrayFormat( value );

		/// <inheritdoc cref="SoftBasic.ArrayFormat{T}(T, string)"/>
		public static string ToArrayString<T>( this T[] value, string format ) => SoftBasic.ArrayFormat( value, format );

		/// <summary>
		/// 将字符串数组转换为实际的数据数组。例如字符串格式[1,2,3,4,5]，可以转成实际的数组对象<br />
		/// Converts a string array into an actual data array. For example, the string format [1,2,3,4,5] can be converted into an actual array object
		/// </summary>
		/// <typeparam name="T">类型对象</typeparam>
		/// <param name="value">字符串数据</param>
		/// <param name="selector">转换方法</param>
		/// <returns>实际的数组</returns>
		public static T[] ToStringArray<T>( this string value, Func<string, T> selector )
		{
			if (value.IndexOf( '[' ) >= 0) value = value.Replace( "[", "" );
			if (value.IndexOf( ']' ) >= 0) value = value.Replace( "]", "" );

			string[] splits = value.Split( new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries );
			return splits.Select( selector ).ToArray( );
		}

		/// <summary>
		/// 将字符串数组转换为实际的数据数组。支持byte,sbyte,bool,short,ushort,int,uint,long,ulong,float,double，使用默认的十进制，例如字符串格式[1,2,3,4,5]，可以转成实际的数组对象<br />
		/// Converts a string array into an actual data array. Support byte, sbyte, bool, short, ushort, int, uint, long, ulong, float, double, use the default decimal, 
		/// such as the string format [1,2,3,4,5], which can be converted into an actual array Object
		/// </summary>
		/// <typeparam name="T">类型对象</typeparam>
		/// <param name="value">字符串数据</param>
		/// <returns>实际的数组</returns>
		public static T[] ToStringArray<T>( this string value )
		{
			Type type = typeof( T );
			if (type == typeof( byte )) return (T[])(object)value.ToStringArray( byte.Parse );
			else if (type == typeof( sbyte )) return (T[])(object)value.ToStringArray( sbyte.Parse );
			else if (type == typeof( bool )) return (T[])(object)value.ToStringArray( bool.Parse );
			else if (type == typeof( short )) return (T[])(object)value.ToStringArray( short.Parse );
			else if (type == typeof( ushort )) return (T[])(object)value.ToStringArray( ushort.Parse );
			else if (type == typeof( int )) return (T[])(object)value.ToStringArray( int.Parse );
			else if (type == typeof( uint )) return (T[])(object)value.ToStringArray( uint.Parse );
			else if (type == typeof( long )) return (T[])(object)value.ToStringArray( long.Parse );
			else if (type == typeof( ulong )) return (T[])(object)value.ToStringArray( ulong.Parse );
			else if (type == typeof( float )) return (T[])(object)value.ToStringArray( float.Parse );
			else if (type == typeof( double )) return (T[])(object)value.ToStringArray( double.Parse );
			else if (type == typeof( DateTime )) return (T[])(object)value.ToStringArray( DateTime.Parse );
#if !NET20 && !NET35
			else if (type == typeof( Guid )) return (T[])(object)value.ToStringArray( Guid.Parse );
#endif
			else if (type == typeof( string )) return (T[])(object)value.ToStringArray( m => m );
			else throw new Exception( "use ToArray<T>(Func<string,T>) method instead" );
		}

		/// <summary>
		/// 启动接收数据，需要传入回调方法，传递对象<br />
		/// To start receiving data, you need to pass in a callback method and pass an object
		/// </summary>
		/// <param name="socket">socket对象</param>
		/// <param name="callback">回调方法</param>
		/// <param name="obj">数据对象</param>
		/// <returns>是否启动成功</returns>
		public static OperateResult BeginReceiveResult( this Socket socket, AsyncCallback callback, object obj )
		{
			try
			{
				socket.BeginReceive( new byte[0], 0, 0, SocketFlags.None, callback, obj );
				return OperateResult.CreateSuccessResult( );
			}
			catch (Exception ex)
			{
				socket?.Close( );
				return new OperateResult( ex.Message );
			}
		}

		/// <summary>
		/// 启动接收数据，需要传入回调方法，传递对象默认为socket本身<br />
		/// To start receiving data, you need to pass in a callback method. The default object is the socket itself.
		/// </summary>
		/// <param name="socket">socket对象</param>
		/// <param name="callback">回调方法</param>
		/// <returns>是否启动成功</returns>
		public static OperateResult BeginReceiveResult( this Socket socket, AsyncCallback callback )
		{
			return BeginReceiveResult( socket, callback, socket );
		}

		/// <summary>
		/// 结束挂起的异步读取，返回读取的字节数，如果成功的情况。<br />
		/// Ends the pending asynchronous read and returns the number of bytes read, if successful.
		/// </summary>
		/// <param name="socket">socket对象</param>
		/// <param name="ar">回调方法</param>
		/// <returns>是否启动成功</returns>
		public static OperateResult<int> EndReceiveResult( this Socket socket, IAsyncResult ar )
		{
			try
			{
				return OperateResult.CreateSuccessResult( socket.EndReceive( ar ) );
			}
			catch (Exception ex)
			{
				socket?.Close( );
				return new OperateResult<int>( ex.Message );
			}
		}

		/// <summary>
		/// 根据英文小数点进行切割字符串，去除空白的字符<br />
		/// Cut the string according to the English decimal point and remove the blank characters
		/// </summary>
		/// <param name="str">字符串本身</param>
		/// <returns>切割好的字符串数组，例如输入 "100.5"，返回 "100", "5"</returns>
		public static string[] SplitDot( this string str ) => str.Split( new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries );

		/// <summary>
		/// 获取当前对象的JSON格式表示的字符串。<br />
		/// Gets the string represented by the JSON format of the current object.
		/// </summary>
		/// <returns>字符串对象</returns>
		public static string ToJsonString( this object obj, Newtonsoft.Json.Formatting formatting = Newtonsoft.Json.Formatting.Indented ) => Newtonsoft.Json.JsonConvert.SerializeObject( obj, formatting );

		/// <inheritdoc cref="HslCommunication.Core.Security.RSAHelper.GetPrivateKeyFromRSA"/>
		public static byte[] GetPEMPrivateKey( this RSACryptoServiceProvider rsa )
		{
			return HslCommunication.Core.Security.RSAHelper.GetPrivateKeyFromRSA( rsa );
		}

		/// <inheritdoc cref="HslCommunication.Core.Security.RSAHelper.GetPublicKeyFromRSA"/>
		public static byte[] GetPEMPublicKey( this RSACryptoServiceProvider rsa )
		{
			return HslCommunication.Core.Security.RSAHelper.GetPublicKeyFromRSA( rsa );
		}

		/// <inheritdoc cref="HslCommunication.Core.Security.RSAHelper.EncryptLargeDataByRSA(RSACryptoServiceProvider, byte[])"/>
		public static byte[] EncryptLargeData( this RSACryptoServiceProvider rsa, byte[] data )
		{
			return HslCommunication.Core.Security.RSAHelper.EncryptLargeDataByRSA( rsa, data );
		}

		/// <inheritdoc cref="HslCommunication.Core.Security.RSAHelper.DecryptLargeDataByRSA(RSACryptoServiceProvider, byte[])"/>
		public static byte[] DecryptLargeData( this RSACryptoServiceProvider rsa, byte[] data )
		{
			return HslCommunication.Core.Security.RSAHelper.DecryptLargeDataByRSA( rsa, data );
		}

		/// <inheritdoc cref="MemoryStream.Write(byte[], int, int)"/>
		public static void Write( this MemoryStream ms, byte[] buffer )
		{
			if (buffer != null) ms.Write( buffer, 0, buffer.Length );
		}

		/// <summary>
		/// 将<see cref="ushort"/>数据写入到字节流，字节顺序为相反<br />
		/// Write <see cref="ushort"/> data to the byte stream, the byte order is reversed
		/// </summary>
		/// <param name="ms">字节流</param>
		/// <param name="value">等待写入的值</param>
		public static void WriteReverse( this MemoryStream ms, ushort value )
		{
			byte[] buffer = BitConverter.GetBytes( value );
			byte tmp = buffer[0];
			buffer[0] = buffer[1];
			buffer[1] = tmp;
			ms.Write( buffer );
		}

		/// <summary>
		/// 设置套接字的活动时间和活动间歇时间，此值会设置到socket低级别的控制中，传入值如果为负数，则表示不使用 KeepAlive 功能。<br />
		/// Set the active time and active intermittent time of the socket. This value will be set to the low-level control of the socket.
		/// If the incoming value is a negative number, it means that the KeepAlive function is not used.
		/// </summary>
		/// <param name="socket">套接字对象</param>
		/// <param name="keepAliveTime">保持活动时间</param>
		/// <param name="keepAliveInterval">保持活动的间歇时间</param>
		/// <returns>返回获取的参数的字节</returns>
		public static int SetKeepAlive( this Socket socket, int keepAliveTime, int keepAliveInterval )
		{
			byte[] buffer = new byte[12];
			BitConverter.GetBytes( keepAliveTime < 0 ? 0 : 1 ).CopyTo( buffer, 0 );
			BitConverter.GetBytes( keepAliveTime ).CopyTo( buffer, 4 );
			BitConverter.GetBytes( keepAliveInterval ).CopyTo( buffer, 8 );

			try
			{
				return socket.IOControl( IOControlCode.KeepAliveValues, buffer, null );
			}
			catch
			{
				return 0;
			}
		}

		/// <summary>
		/// 使用格式化的串口参数信息来初始化串口的参数，举例：9600-8-N-1，分别表示波特率，数据位，奇偶校验，停止位，当然也可以携带串口名称，例如：COM3-9600-8-N-1，linux环境也是支持的。<br />
		/// Use the formatted serial port parameter information to initialize the serial port parameters, for example: 9600-8-N-1, which means baud rate, data bit, parity, 
		/// stop bit, of course, can also carry the serial port name, for example: COM3- 9600-8-N-1, linux environment is also supported.
		/// </summary>
		/// <remarks>
		/// 其中奇偶校验的字母可选，N:无校验，O：奇校验，E:偶校验，停止位可选 0, 1, 2, 1.5 四种选项<br />
		/// Among them, the letters of the parity check are optional, N: no check, O: odd check, E: even check, stop bit optional 0, 1, 2, 1.5 four options
		/// </remarks>
		/// <param name="serialPort">串口对象信息</param>
		/// <param name="format">格式化的参数内容，例如：9600-8-N-1</param>
		public static void IniSerialByFormatString( this SerialPort serialPort, string format )
		{
			string[] splits = format.Split( new char[] { '-', ';' }, StringSplitOptions.RemoveEmptyEntries );
			if (splits.Length == 0) return;
			int index = 0;
			if (!Regex.IsMatch( splits[0], "^[0-9]+$" ))
			{
				serialPort.PortName = splits[0];
				index = 1;
			}

			if (index < splits.Length) serialPort.BaudRate = Convert.ToInt32( splits[index++] );
			if (index < splits.Length) serialPort.DataBits = Convert.ToInt32( splits[index++] );
			if (index < splits.Length)
			{
				string parity = splits[index++].ToUpper( );
				serialPort.Parity = parity == "N" ? Parity.None : parity == "O" ? Parity.Odd : parity == "E" ? Parity.Even : Parity.Space;
			}
			if (index < splits.Length)
			{
				string stopBits = splits[index++];
				serialPort.StopBits = stopBits == "1" ? StopBits.One : stopBits == "2" ? StopBits.Two : stopBits == "0" ? StopBits.None : StopBits.OnePointFive;
			}
		}

		/// <summary>
		/// 从串口接收指定长度的字节数组信息，还可以指定超时时间，以及休眠间歇时间。<br />
		/// Receive byte array information of specified length from the serial port, and can also specify the timeout time and sleep intermittent time.
		/// </summary>
		/// <param name="serialPort">串口信息</param>
		/// <param name="length">准备接收的字节长度</param>
		/// <param name="timeout">超时时间</param>
		/// <param name="sleepTime">间歇休眠时间</param>
		/// <returns>接收的结果内容对象</returns>
		public static OperateResult<byte[]> Receive( this SerialPort serialPort, int length, int timeout, int sleepTime )
		{
			if (!Authorization.nzugaydgwadawdibbas( )) return new OperateResult<byte[]>( StringResources.Language.AuthorizationFailed );

			byte[] buffer = new byte[1024];
			MemoryStream ms = new MemoryStream( );
			DateTime start = DateTime.Now;                                  // 开始时间，用于确认是否超时的信息
			while (true)
			{
				Thread.Sleep( sleepTime );
				try
				{
					if (serialPort.BytesToRead < 1)
					{
						if ((DateTime.Now - start).TotalMilliseconds > timeout)
						{
							ms.Dispose( );
							return new OperateResult<byte[]>( $"Time out: {timeout}" );
						}
					}

					// 继续接收数据
					int len = (int)Math.Min( length - ms.Length, buffer.Length );
					int sp_receive = serialPort.Read( buffer, 0, len );
					if (sp_receive > 0) ms.Write( buffer, 0, sp_receive );

					if (ms.Length >= length) break;
				}
				catch (Exception ex)
				{
					ms.Dispose( );
					return new OperateResult<byte[]>( ex.Message );
				}
			}

			return OperateResult.CreateSuccessResult( ms.ToArray( ) );
		}

		/// <summary>
		/// 根据指定的字节长度信息，获取到随机的字节信息<br />
		/// Obtain random byte information according to the specified byte length information
		/// </summary>
		/// <param name="random">随机数对象</param>
		/// <param name="length">字节的长度信息</param>
		/// <returns>原始字节数组</returns>
		public static byte[] GetBytes( this Random random, int length )
		{
			byte[] buffer = new byte[length];
			random.NextBytes( buffer );
			return buffer;
		}

		/// <inheritdoc cref="SoftBasic.BytesReverseByWord(byte[])"/>
		public static byte[] ReverseByWord( this byte[] inBytes ) => SoftBasic.BytesReverseByWord( inBytes );

	}
}
