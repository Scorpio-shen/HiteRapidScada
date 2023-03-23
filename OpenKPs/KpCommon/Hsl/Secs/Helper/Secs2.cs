using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HslCommunication.Core;
using HslCommunication.Secs.Types;

namespace HslCommunication.Secs.Helper
{
	/// <summary>
	/// Secs2相关的规则
	/// </summary>
	public class Secs2
	{
		/// <summary>
		/// 列表的类型信息
		/// </summary>
		public const int TypeList = 0x00;
		/// <summary>
		/// ASCII字符串的信息
		/// </summary>
		public const int TypeASCII = 0x40;
		/// <summary>
		/// 有符号的1个字节长度的整型
		/// </summary>
		public const int TypeSByte = 0x64;
		/// <summary>
		/// 无符号的1个字节长度的整型
		/// </summary>
		public const int TypeByte  = 0xA4;
		/// <summary>
		/// 有符号的2个字节长度的整型
		/// </summary>
		public const int TypeInt16 = 0x68;
		/// <summary>
		/// 无符号的2个字节长度的整型
		/// </summary>
		public const int TypeUInt16 = 0xA8;
		/// <summary>
		/// 有符号的4个字节长度的整型
		/// </summary>
		public const int TypeInt32 = 0x70;
		/// <summary>
		/// 无符号的4个字节长度的整型
		/// </summary>
		public const int TypeUInt32 = 0xB0;
		/// <summary>
		/// 有符号的8个字节长度的整型
		/// </summary>
		public const int TypeInt64 = 0x60;
		/// <summary>
		/// 无符号的8个字节长度的整型
		/// </summary>
		public const int TypeUInt64 = 0xA0;
		/// <summary>
		/// 单浮点精度的类型
		/// </summary>
		public const int TypeSingle = 0x90;
		/// <summary>
		/// 双浮点精度的类型
		/// </summary>
		public const int TypeDouble = 0x80;
		/// <summary>
		/// Bool值信息
		/// </summary>
		public const int TypeBool = 0x24;
		/// <summary>
		/// 二进制的数据信息
		/// </summary>
		public const int TypeBinary = 0x20;
		/// <summary>
		/// JIS8类型的数据
		/// </summary>
		public const int TypeJIS8 = 0x44;


		internal static SecsValue ExtraToSecsItemValue( IByteTransform byteTransform, byte[] buffer, ref int index, Encoding encoding )
		{
			if (index >= buffer.Length) return new SecsValue( );
			int typeLength = buffer[index] & 0x03;
			int type       = buffer[index] & 0xfc;

			int dataLength = 0;
			if      (typeLength == 1) { dataLength = buffer[index + 1]; index += 2; }
			else if (typeLength == 2) { dataLength = buffer[index + 1] * 256 + buffer[index + 2]; index += 3; }
			else if (typeLength == 3) { dataLength = buffer[index + 1] * 65536 + buffer[index + 2] * 256 + buffer[index + 3]; index += 4; }
			else index += 1;

			if (type == Secs2.TypeList)
			{
				SecsValue[] secsItems = new SecsValue[dataLength];
				for (int i = 0; i < secsItems.Length; i++)
				{
					secsItems[i] = ExtraToSecsItemValue( byteTransform, buffer, ref index, encoding );
				}
				return new SecsValue( SecsItemType.List, secsItems );
			}
			else
			{
				int byteIndex = index;
				index += dataLength;
				switch (type)
				{
					case Secs2.TypeASCII:  return new SecsValue( SecsItemType.ASCII,  encoding.GetString( buffer, byteIndex, dataLength ) );
					case Secs2.TypeSByte:  return new SecsValue( SecsItemType.SByte,  dataLength == 1 ? (sbyte)buffer[byteIndex] : (object)buffer.SelectMiddle( byteIndex, dataLength ).Select( m => (sbyte)m ).ToArray( ) );
					case Secs2.TypeByte:   return new SecsValue( SecsItemType.Byte,   dataLength == 1 ? buffer[byteIndex] : (object)buffer.SelectMiddle( byteIndex, dataLength ) );
					case Secs2.TypeInt16:  return new SecsValue( SecsItemType.Int16,  dataLength == 2 ? byteTransform.TransInt16(  buffer, byteIndex ) : (object)byteTransform.TransInt16(  buffer, byteIndex, dataLength / 2 ) );
					case Secs2.TypeUInt16: return new SecsValue( SecsItemType.UInt16, dataLength == 2 ? byteTransform.TransUInt16( buffer, byteIndex ) : (object)byteTransform.TransUInt16( buffer, byteIndex, dataLength / 2 ) );
					case Secs2.TypeInt32:  return new SecsValue( SecsItemType.Int32,  dataLength == 4 ? byteTransform.TransInt32(  buffer, byteIndex ) : (object)byteTransform.TransInt32(  buffer, byteIndex, dataLength / 4 ) );
					case Secs2.TypeUInt32: return new SecsValue( SecsItemType.UInt32, dataLength == 4 ? byteTransform.TransUInt32( buffer, byteIndex ) : (object)byteTransform.TransUInt32( buffer, byteIndex, dataLength / 4 ) );
					case Secs2.TypeInt64:  return new SecsValue( SecsItemType.Int64,  dataLength == 8 ? byteTransform.TransInt64(  buffer, byteIndex ) : (object)byteTransform.TransInt64(  buffer, byteIndex, dataLength / 8 ) );
					case Secs2.TypeUInt64: return new SecsValue( SecsItemType.UInt64, dataLength == 8 ? byteTransform.TransUInt64( buffer, byteIndex ) : (object)byteTransform.TransUInt64( buffer, byteIndex, dataLength / 8 ) );
					case Secs2.TypeSingle: return new SecsValue( SecsItemType.Single, dataLength == 4 ? byteTransform.TransSingle( buffer, byteIndex ) : (object)byteTransform.TransSingle( buffer, byteIndex, dataLength / 4 ) );
					case Secs2.TypeDouble: return new SecsValue( SecsItemType.Double, dataLength == 8 ? byteTransform.TransDouble( buffer, byteIndex ) : (object)byteTransform.TransDouble( buffer, byteIndex, dataLength / 8 ) );
					case Secs2.TypeBool:   return new SecsValue( SecsItemType.Bool,   dataLength == 1 ? buffer[byteIndex] != 0 : (object)buffer.SelectMiddle( byteIndex, dataLength ).Select( m => m != 0x00 ).ToArray( ) );
					case Secs2.TypeBinary: return new SecsValue( SecsItemType.Binary, buffer.SelectMiddle( byteIndex, dataLength ) );
					case Secs2.TypeJIS8:   return new SecsValue( SecsItemType.JIS8,   buffer.SelectMiddle( byteIndex, dataLength ) );
					default: return null;
				}
			}
		}

		internal static int GetTypeCodeFrom( SecsItemType type )
		{
			switch ( type)
			{
				case SecsItemType.ASCII:    return Secs2.TypeASCII; 
				case SecsItemType.SByte:    return Secs2.TypeSByte; 
				case SecsItemType.Byte:     return Secs2.TypeByte;  
				case SecsItemType.Int16:    return Secs2.TypeInt16; 
				case SecsItemType.UInt16:   return Secs2.TypeUInt16;
				case SecsItemType.Int32:    return Secs2.TypeInt32; 
				case SecsItemType.UInt32:   return Secs2.TypeUInt32;
				case SecsItemType.Int64:    return Secs2.TypeInt64;
				case SecsItemType.UInt64:   return Secs2.TypeUInt64;
				case SecsItemType.Single:   return Secs2.TypeSingle;
				case SecsItemType.Double:   return Secs2.TypeDouble;
				case SecsItemType.Bool:     return Secs2.TypeBool;
				case SecsItemType.Binary:   return Secs2.TypeBinary;
				case SecsItemType.JIS8:     return Secs2.TypeJIS8;
				default:                    return Secs2.TypeList;
			}
		}

		internal static void AddCodeSource( List<byte> bytes, SecsItemType type, int length )
		{
			int code = GetTypeCodeFrom( type );
			if (length < 256)
			{
				bytes.Add( (byte)(code | 0x01) );
				bytes.Add( (byte)length );
			}
			else if (length < 256 * 256)
			{
				byte[] buffer = BitConverter.GetBytes( length );
				bytes.Add( (byte)(code | 0x02) );
				bytes.Add( buffer[1] );
				bytes.Add( buffer[0] );
			}
			else
			{
				byte[] buffer = BitConverter.GetBytes( length );
				bytes.Add( (byte)(code | 0x03) );
				bytes.Add( buffer[2] );
				bytes.Add( buffer[1] );
				bytes.Add( buffer[0] );
			}
		}

		internal static void AddCodeAndValueSource( List<byte> bytes, SecsValue value, Encoding encoding )
		{
			if (value == null) return;
			if (value.ItemType == SecsItemType.List)
			{
				IEnumerable<SecsValue> secsValues = value.Value as IEnumerable<SecsValue>;
				int length = secsValues == null ? 0 : secsValues.Count( );
				AddCodeSource( bytes, value.ItemType, length );
			}
			else
			{
				byte[] buffer = null;
				switch (value.ItemType)
				{
					case SecsItemType.ASCII:    buffer = encoding.GetBytes( value.Value.ToString( ) ); break;
					case SecsItemType.SByte:    buffer = value.Value.GetType( ) == typeof( sbyte ) ?  new byte[] { (byte)value.Value } : ((sbyte[])value.Value).Select(m => (byte)m).ToArray( ); break;
					case SecsItemType.Byte:     buffer = value.Value.GetType( ) == typeof( byte ) ?   new byte[] { (byte)value.Value } : (byte[])value.Value; break;
					case SecsItemType.Int16:    buffer = value.Value.GetType( ) == typeof( short ) ?  SecsTransform.TransByte( (short)value.Value ) : SecsTransform.TransByte( (short[])value.Value ); break;
					case SecsItemType.UInt16:   buffer = value.Value.GetType( ) == typeof( ushort ) ? SecsTransform.TransByte( (ushort)value.Value ) : SecsTransform.TransByte( (ushort[])value.Value ); break;
					case SecsItemType.Int32:    buffer = value.Value.GetType( ) == typeof( int ) ?    SecsTransform.TransByte( (int)value.Value ) : SecsTransform.TransByte( (int[])value.Value ); break;
					case SecsItemType.UInt32:   buffer = value.Value.GetType( ) == typeof( uint ) ?   SecsTransform.TransByte( (uint)value.Value ) : SecsTransform.TransByte( (uint[])value.Value ); break;
					case SecsItemType.Int64:    buffer = value.Value.GetType( ) == typeof( long ) ?   SecsTransform.TransByte( (long)value.Value ) : SecsTransform.TransByte( (long[])value.Value ); break;
					case SecsItemType.UInt64:   buffer = value.Value.GetType( ) == typeof( ulong ) ?  SecsTransform.TransByte( (ulong)value.Value ) : SecsTransform.TransByte( (ulong[])value.Value ); break;
					case SecsItemType.Single:   buffer = value.Value.GetType( ) == typeof( float ) ?  SecsTransform.TransByte( (float)value.Value ) : SecsTransform.TransByte( (float[])value.Value ); break;
					case SecsItemType.Double:   buffer = value.Value.GetType( ) == typeof( double ) ? SecsTransform.TransByte( (double)value.Value ) : SecsTransform.TransByte( (double[])value.Value ); break;
					case SecsItemType.Bool:     buffer = value.Value.GetType( ) == typeof( bool ) ? (((bool)value.Value) ? new byte[] { 0xFF } : new byte[] { 0x00 }) : ((bool[])value.Value).Select( m => m ? (byte)0xff : (byte)0x00).ToArray( ); break;
					case SecsItemType.Binary:   buffer = (byte[])value.Value; break;
					case SecsItemType.JIS8:     buffer = (byte[])value.Value; break;
					case SecsItemType.None:     buffer = new byte[0]; break;
					default:                    buffer = (byte[])value.Value; break;
				}
				AddCodeSource( bytes, value.ItemType, buffer.Length );
				bytes.AddRange( buffer );
			}
		}

		/// <summary>
		/// 将返回的数据内容解析为实际的字符串信息，根据secsⅡ 协议定义的规则解析出实际的数据信息
		/// </summary>
		/// <param name="buffer">原始的字节数据内容</param>
		/// <param name="encoding">字符串对象的编码信息</param>
		/// <returns>字符串消息</returns>
		public static SecsValue ExtraToSecsItemValue( byte[] buffer, Encoding encoding )
		{
			if (buffer == null) return null;
			int index = 0;
			return ExtraToSecsItemValue( SecsTransform, buffer, ref index, encoding );
		}

		/// <summary>
		/// SECS的字节顺序信息
		/// </summary>
		public static IByteTransform SecsTransform = new ReverseBytesTransform( );
	}
}
