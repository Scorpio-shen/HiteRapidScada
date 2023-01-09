using HslCommunication.Core;
using HslCommunication.Secs.Helper;
using HslCommunication.Secs.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace HslCommunication.Secs.Types
{
	/// <summary>
	/// SECS数据的对象信息，可以用来表示层级及嵌套的数据内容，如果需要显示，只需要<see cref="ToString"/> 方法即可，
	/// 如果需要发送SECS设备，只需要 <see cref="ToSourceBytes()"/>，并支持反序列化操作 <see cref="SecsValue.ParseFromSource(byte[],Encoding)"/>，无论是XML元素还是byte[]类型。
	/// </summary>
	/// <remarks>
	/// XML序列化，反序列化例子：<br />
	/// SecsValue value = new SecsValue( new object[]{ 1.23f, "ABC" } );<br />
	/// XElement xml = value.ToXElement( ); <br />
	/// SecsValue value2 = new SecsValue(xml);
	/// </remarks>
	/// <example>
	/// 关于<see cref="SecsValue"/>类型，可以非常灵活的实例化，参考下面的示例代码
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Secs\SecsGemSample.cs" region="Sample2" title="SecsValue说明" />
	/// </example>
	public class SecsValue
	{
		#region Constructor

		/// <summary>
		/// 实例化一个空的SECS对象
		/// </summary>
		public SecsValue( )
		{
			this.ItemType = SecsItemType.None;
		}

		/// <summary>
		/// 从一个字符串对象初始化数据信息
		/// </summary>
		/// <param name="value">字符串信息</param>
		public SecsValue( string value ) : this( SecsItemType.ASCII, value ) { }

		/// <summary>
		/// 从一个类型为 <see cref="sbyte"/> 的对象初始化数据
		/// </summary>
		/// <param name="value">数据值信息</param>
		public SecsValue( sbyte value ) : this( SecsItemType.SByte, value ) { }

		/// <inheritdoc cref="SecsValue.SecsValue(sbyte)"/>
		public SecsValue( sbyte[] value ) : this( SecsItemType.SByte, value ) { }

		/// <summary>
		/// 从一个类型为 <see cref="byte"/> 的对象初始化数据
		/// </summary>
		/// <param name="value">数据值信息</param>
		public SecsValue( byte value ) : this( SecsItemType.Byte, value ) { }

		/// <summary>
		/// 从一个类型为 <see cref="short"/> 的对象初始化数据
		/// </summary>
		/// <param name="value">数据值信息</param>
		public SecsValue( short value ) : this( SecsItemType.Int16, value ) { }

		/// <summary>
		/// 从一个类型为 <see cref="int"/> 的对象初始化数据
		/// </summary>
		/// <param name="value">数据值信息</param>
		public SecsValue( short[] value ) : this( SecsItemType.Int16, value ) { }

		/// <summary>
		/// 从一个类型为 <see cref="ushort"/> 的对象初始化数据
		/// </summary>
		/// <param name="value">数据值信息</param>
		public SecsValue( ushort value ) : this( SecsItemType.UInt16, value ) { }

		/// <inheritdoc cref="SecsValue.SecsValue(ushort)"/>
		public SecsValue( ushort[] value ) : this( SecsItemType.UInt16, value ) { }

		/// <summary>
		/// 从一个类型为 <see cref="int"/> 的对象初始化数据
		/// </summary>
		/// <param name="value">数据值信息</param>
		public SecsValue( int value ) : this( SecsItemType.Int32, value ) { }

		/// <summary>
		/// 从一个类型为 <see cref="int"/> 的对象初始化数据
		/// </summary>
		/// <param name="value">数据值信息</param>
		public SecsValue( int[] value ) : this( SecsItemType.Int32, value ) { }

		/// <summary>
		/// 从一个类型为 <see cref="uint"/> 的对象初始化数据
		/// </summary>
		/// <param name="value">数据值信息</param>
		public SecsValue( uint value ) : this( SecsItemType.UInt32, value ) { }

		/// <inheritdoc cref="SecsValue.SecsValue(uint)"/>
		public SecsValue( uint[] value ) : this( SecsItemType.UInt32, value ) { }

		/// <summary>
		/// 从一个类型为 <see cref="long"/> 的对象初始化数据
		/// </summary>
		/// <param name="value">数据值信息</param>
		public SecsValue( long value ) : this( SecsItemType.Int64, value ) { }

		/// <inheritdoc cref="SecsValue.SecsValue(long)"/>
		public SecsValue( long[] value ) : this( SecsItemType.Int64, value ) { }

		/// <summary>
		/// 从一个类型为 <see cref="ulong"/> 的对象初始化数据
		/// </summary>
		/// <param name="value">数据值信息</param>
		public SecsValue( ulong value ) : this( SecsItemType.UInt64, value ) { }

		/// <inheritdoc cref="SecsValue.SecsValue(ulong)"/>
		public SecsValue( ulong[] value ) : this( SecsItemType.UInt64, value ) { }

		/// <summary>
		/// 从一个类型为 <see cref="float"/> 的对象初始化数据
		/// </summary>
		/// <param name="value">数据值信息</param>
		public SecsValue( float value ) : this( SecsItemType.Single, value ) { }

		/// <inheritdoc cref="SecsValue.SecsValue(float)"/>
		public SecsValue( float[] value ) : this( SecsItemType.Single, value ) { }

		/// <summary>
		/// 从一个类型为 <see cref="double"/> 的对象初始化数据
		/// </summary>
		/// <param name="value">数据值信息</param>
		public SecsValue( double value ) : this( SecsItemType.Double, value ) { }

		/// <inheritdoc cref="SecsValue.SecsValue(double)"/>
		public SecsValue( double[] value ) : this( SecsItemType.Double, value ) { }

		/// <summary>
		/// 从一个类型为 <see cref="byte"/> 数组的对象初始化数据，需要指定 <see cref="SecsItemType"/> 来表示二进制还是byte数组类型
		/// </summary>
		/// <param name="value">数据值信息</param>
		public SecsValue( byte[] value )
		{
			this.ItemType   = SecsItemType.Binary;
			this.Value      = value;
		}

		/// <summary>
		/// 从一个类型为 <see cref="bool"/> 的对象初始化数据
		/// </summary>
		/// <param name="value">数据值信息</param>
		public SecsValue( bool value ) : this( SecsItemType.Bool, value) { }

		/// <inheritdoc cref="SecsValue.SecsValue(bool)"/>
		public SecsValue( bool[] value ) : this( SecsItemType.Bool, value ) { }

		/// <summary>
		/// 从一个类型为 <see cref="object"/> 数组的对象初始化数据，初始化后，本对象为 <see cref="SecsItemType.List"/> 类型
		/// </summary>
		/// <param name="value">数据值信息</param>
		public SecsValue( IEnumerable<object> value )
		{
			this.ItemType = SecsItemType.List;
			List<SecsValue> list = new List<SecsValue>( );
			if (value == null) value = new object[0];
			foreach ( object obj in value)
			{
				if (obj.GetType( ) == typeof( SecsValue ))     list.Add( (SecsValue)obj );
				if (obj.GetType( ) == typeof( bool ))          list.Add( new SecsValue( (bool)obj ) );
				if (obj.GetType( ) == typeof( bool[] ))        list.Add( new SecsValue( (bool[])obj ) );
				if (obj.GetType( ) == typeof( sbyte ))         list.Add( new SecsValue( (sbyte)obj ) );
				if (obj.GetType( ) == typeof( sbyte[] ))       list.Add( new SecsValue( (sbyte[])obj ) );
				if (obj.GetType( ) == typeof( byte ))          list.Add( new SecsValue( (byte)obj ) );
				if (obj.GetType( ) == typeof( short ))         list.Add( new SecsValue( (short)obj ) );
				if (obj.GetType( ) == typeof( short[] ))       list.Add( new SecsValue( (short[])obj ) );
				if (obj.GetType( ) == typeof( ushort ))        list.Add( new SecsValue( (ushort)obj ) );
				if (obj.GetType( ) == typeof( ushort[] ))      list.Add( new SecsValue( (ushort[])obj ) );
				if (obj.GetType( ) == typeof( int ))           list.Add( new SecsValue( (int)obj ) );
				if (obj.GetType( ) == typeof( int[] ))         list.Add( new SecsValue( (int[])obj ) );
				if (obj.GetType( ) == typeof( uint ))          list.Add( new SecsValue( (uint)obj ) );
				if (obj.GetType( ) == typeof( uint[] ))        list.Add( new SecsValue( (uint[])obj ) );
				if (obj.GetType( ) == typeof( long ))          list.Add( new SecsValue( (long)obj ) );
				if (obj.GetType( ) == typeof( long[] ))        list.Add( new SecsValue( (long[])obj ) );
				if (obj.GetType( ) == typeof( ulong ))         list.Add( new SecsValue( (ulong)obj ) );
				if (obj.GetType( ) == typeof( ulong[] ))       list.Add( new SecsValue( (ulong[])obj ) );
				if (obj.GetType( ) == typeof( float ))         list.Add( new SecsValue( (float)obj ) );
				if (obj.GetType( ) == typeof( float[] ))       list.Add( new SecsValue( (float[])obj ) );
				if (obj.GetType( ) == typeof( double ))        list.Add( new SecsValue( (double)obj ) );
				if (obj.GetType( ) == typeof( double[] ))      list.Add( new SecsValue( (double[])obj ) );
				if (obj.GetType( ) == typeof( string ))        list.Add( new SecsValue( (string)obj ) );
				if (obj.GetType( ) == typeof( byte[] ))        list.Add( new SecsValue( (byte[])obj ) );
				if (obj.GetType( ) == typeof( object[] ))      list.Add( new SecsValue( (object[])obj ) );
				if (obj.GetType( ) == typeof( List<object> ))  list.Add( new SecsValue( (List<object>)obj ) );
			}
			this.Value  = list.ToArray( );
		}

		/// <summary>
		/// 通过指定的参数信息来实例化一个对象
		/// </summary>
		/// <param name="type">数据的类型信息</param>
		/// <param name="value">数据值信息，当是<see cref="SecsItemType.List"/>类型时，本值为空 </param>
		public SecsValue( SecsItemType type, object value )
		{
			this.ItemType   = type;
			this.Value      = value;
		}

		/// <summary>
		/// 从完整的XML元素进行实例化一个对象
		/// </summary>
		/// <param name="element">符合SECS的XML数据表示元素</param>
		/// <exception cref="ArgumentException">解析失败的异常</exception>
		public SecsValue( XElement element )
		{
			if (element.Name == nameof( SecsItemType.List ))
			{
				this.ItemType = SecsItemType.List;
				List<SecsValue> list = new List<SecsValue>( );
				foreach ( var item in element.Elements())
				{
					SecsValue secsValue = new SecsValue( item );
					if (secsValue != null)
					{
						list.Add( secsValue );
					}
				}
				this.Value  = list.ToArray( );
			}
			else
			{
				if      ( element.Name == nameof( SecsItemType.SByte ))  { this.ItemType = SecsItemType.SByte;  this.Value = GetObjectValue( element, sbyte.Parse ); }
				else if ( element.Name == nameof( SecsItemType.Byte ))   { this.ItemType = SecsItemType.Byte;   this.Value = GetObjectValue( element, byte.Parse ); }
				else if ( element.Name == nameof( SecsItemType.Int16 ))  { this.ItemType = SecsItemType.Int16;  this.Value = GetObjectValue( element, short.Parse ); }
				else if ( element.Name == nameof( SecsItemType.UInt16 )) { this.ItemType = SecsItemType.UInt16; this.Value = GetObjectValue( element, ushort.Parse ); }
				else if ( element.Name == nameof( SecsItemType.Int32 ))  { this.ItemType = SecsItemType.Int32;  this.Value = GetObjectValue( element, int.Parse ); }
				else if ( element.Name == nameof( SecsItemType.UInt32 )) { this.ItemType = SecsItemType.UInt32; this.Value = GetObjectValue( element, uint.Parse );}
				else if ( element.Name == nameof( SecsItemType.Int64 ))  { this.ItemType = SecsItemType.Int64;  this.Value = GetObjectValue( element, long.Parse ); }
				else if ( element.Name == nameof( SecsItemType.UInt64 )) { this.ItemType = SecsItemType.UInt64; this.Value = GetObjectValue( element, ulong.Parse ); }
				else if ( element.Name == nameof( SecsItemType.Single )) { this.ItemType = SecsItemType.Single; this.Value = GetObjectValue( element, float.Parse ); }
				else if ( element.Name == nameof( SecsItemType.Double )) { this.ItemType = SecsItemType.Double; this.Value = GetObjectValue( element, double.Parse ); }
				else if ( element.Name == nameof( SecsItemType.Bool ))   { this.ItemType = SecsItemType.Bool;   this.Value = GetObjectValue( element, bool.Parse ); }
				else if ( element.Name == nameof( SecsItemType.ASCII ))
				{
					this.ItemType = SecsItemType.ASCII;
					this.Value    = GetAttribute( element, nameof( SecsValue.Value ), default, m => m );
				}
				else if ( element.Name == nameof( SecsItemType.Binary ))
				{
					this.ItemType = SecsItemType.Binary;
					this.Value    = GetAttribute( element, nameof( SecsValue.Value ), new byte[0], m => m .ToHexBytes( ) );
				}
				else if ( element.Name == nameof( SecsItemType.JIS8 ))
				{
					this.ItemType = SecsItemType.JIS8;
					this.Value    = GetAttribute( element, nameof( SecsValue.Value ), new byte[0], m => m .ToHexBytes( ) );
				}
				else if (element.Name == nameof( SecsItemType.None ))
				{
					this.ItemType = SecsItemType.None;
					this.Value    = GetAttribute( element, nameof( SecsValue.Value ), default, m => m );
				}
				else
				{
					throw new ArgumentException( nameof( element ) );
				}
			}
		}

		#endregion

		/// <summary>
		/// 类型信息
		/// </summary>
		public SecsItemType ItemType { get; set; }

		/// <summary>
		/// 字节长度信息，如果是 <see cref="SecsItemType.List"/> 类型的话，就是数组长度，如果如 <see cref="SecsItemType.ASCII"/> 类型，就是字符串的字节长度，其他类型都是表示数据个数<br />
		/// Byte length information, if it is of type <see cref="SecsItemType.List"/>, it is the length of the array, if it is of type <see cref="SecsItemType.ASCII"/>,
		/// it is the byte length of the string, other types are the number of data
		/// </summary>
		public int Length { get => this.length; }

		/// <summary>
		/// 数据值信息，也可以是 <see cref="SecsValue"/> 的列表信息，在设置列表之前，必须先设置类型
		/// </summary>
		public object Value 
		{
			get => this.obj;
			set
			{
				if(this.ItemType == SecsItemType.None && value != null)
					throw new ArgumentException( "Must set ItemType before set value.", nameof( value ) );
				this.obj    = value;
				this.length = GetValueLength( this );
			}
		}

		/// <summary>
		/// 获取当前数值的XML表示形式
		/// </summary>
		/// <returns>XML元素信息</returns>
		public XElement ToXElement( )
		{
			if (this.ItemType == SecsItemType.List)
			{
				XElement element = new XElement( nameof( SecsItemType.List ) );
				IEnumerable<SecsValue> values = this.Value as IEnumerable<SecsValue>;
				if (values != null)
				{
					element.SetAttributeValue( nameof( SecsValue.Length ), values.Count( ) );
					foreach (SecsValue value in values)
					{
						element.Add( value.ToXElement( ) );
					}
				}
				else
				{
					element.SetAttributeValue( nameof( SecsValue.Length ), 0 );
				}
				return element;
			}
			else
			{
				XElement element = new XElement( this.ItemType.ToString( ) );
				element.SetAttributeValue( nameof( SecsValue.Length ), this.Length );

				if (this.ItemType == SecsItemType.Binary ||
					this.ItemType == SecsItemType.JIS8)
				{
					element.SetAttributeValue( nameof( this.Value ), (this.Value as byte[]).ToHexString( ) );
				}
				else if (this.ItemType == SecsItemType.ASCII)
				{
					element.SetAttributeValue( nameof( this.Value ), this.Value );
				}
				else if (this.ItemType != SecsItemType.None)
				{
					if (this.Value is Array array)
					{
						StringBuilder sb = new StringBuilder( "[" );
						for (int i = 0; i < array.Length; i++)
						{
							sb.Append( array.GetValue( i ).ToString( ) );
							if (i != array.Length - 1) sb.Append( "," );
						}
						sb.Append( "]" );
						element.SetAttributeValue( nameof( this.Value ), sb.ToString( ) );
					}
					else
						element.SetAttributeValue( nameof( this.Value ), this.Value );
				}
				return element;
			}
		}

		/// <summary>
		/// 当前的对象信息转换回实际的原始字节信息，方便写入操作
		/// </summary>
		/// <returns>原始字节数据</returns>
		public byte[] ToSourceBytes( ) => ToSourceBytes( Encoding.Default );

		/// <summary>
		/// 使用指定的编码将当前的对象信息转换回实际的原始字节信息，方便写入操作
		/// </summary>
		/// <param name="encoding">编码信息</param>
		/// <returns>原始字节数据</returns>
		public byte[] ToSourceBytes( Encoding encoding )
		{
			if (this.ItemType == SecsItemType.None) return new byte[0];
			List<byte> bytes = new List<byte>( );
			if (this.ItemType == SecsItemType.List)
			{
				Secs2.AddCodeAndValueSource( bytes, this, encoding );
				if (this.Value is SecsValue[] itemValues)
				{
					foreach (SecsValue value in itemValues)
					{
						bytes.AddRange( value.ToSourceBytes( encoding ) );
					}
				}
			}
			else
			{
				Secs2.AddCodeAndValueSource( bytes, this, encoding );
			}

			return bytes.ToArray( );
		}

		#region Private Member

		private object obj = null;                     // 当前的对象信息
		private int length = 1;                        // 数据长度信息

		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString( ) => ToXElement( ).ToString( );

		#endregion

		#region Other Transfer

		/// <summary>
		/// 将当前的对象转为 <see cref="VariableName"/> 数组对象信息，也就是标签名列表
		/// </summary>
		/// <returns><see cref="VariableName"/>数组对象</returns>
		/// <exception cref="InvalidCastException"></exception>
		public VariableName[] ToVaruableNames( )
		{
			TypeHelper.TypeListCheck( this );
			List<VariableName> names = new List<VariableName>( );
			if (this.Value is SecsValue[] itemValues)
			{
				foreach (var item in itemValues)
				{
					TypeHelper.TypeListCheck( item );
					names.Add( item );
				}
			}
			return names.ToArray( );
		}

		#endregion


		#region Static Method

		/// <summary>
		/// 从原始的字节数据中解析出实际的 <see cref="SecsValue"/> 对象内容。
		/// </summary>
		/// <param name="source">原始字节数据</param>
		/// <param name="encoding">编码信息</param>
		/// <returns>SecsItemValue对象</returns>
		public static SecsValue ParseFromSource( byte[] source, Encoding encoding ) => Secs2.ExtraToSecsItemValue( source, encoding );

		/// <summary>
		/// 获取空的列表信息
		/// </summary>
		/// <returns>secs数据对象</returns>
		public static SecsValue EmptyListValue( ) => new SecsValue( SecsItemType.List, null );

		/// <summary>
		/// 获取空的对象信息
		/// </summary>
		/// <returns>secs数据对象</returns>
		public static SecsValue EmptySecsValue( ) => new SecsValue( SecsItemType.None, null );

		/// <summary>
		/// 获取当前的 <see cref="SecsValue"/> 的数据长度信息
		/// </summary>
		/// <param name="secsValue">secs值</param>
		/// <returns>数据长度信息</returns>
		public static int GetValueLength( SecsValue secsValue )
		{
			if (secsValue.ItemType == SecsItemType.None) return 0;
			if (secsValue.ItemType == SecsItemType.List)
			{
				IEnumerable<SecsValue> secsValues = secsValue.Value as IEnumerable<SecsValue>;
				return secsValues == null ? 0 : secsValues.Count( );
			}
			if (secsValue.Value == null) return 0;
			if (secsValue.ItemType == SecsItemType.SByte)  return secsValue.Value.GetType( ) == typeof( sbyte ) ?  1 : (secsValue.Value as Array).Length;
			if (secsValue.ItemType == SecsItemType.Byte)   return secsValue.Value.GetType( ) == typeof( byte ) ?   1 : (secsValue.Value as Array).Length;
			if (secsValue.ItemType == SecsItemType.Int16)  return secsValue.Value.GetType( ) == typeof( short ) ?  1 : (secsValue.Value as Array).Length;
			if (secsValue.ItemType == SecsItemType.UInt16) return secsValue.Value.GetType( ) == typeof( ushort ) ? 1 : (secsValue.Value as Array).Length;
			if (secsValue.ItemType == SecsItemType.Int32)  return secsValue.Value.GetType( ) == typeof( int ) ?    1 : (secsValue.Value as Array).Length;
			if (secsValue.ItemType == SecsItemType.UInt32) return secsValue.Value.GetType( ) == typeof( uint ) ?   1 : (secsValue.Value as Array).Length;
			if (secsValue.ItemType == SecsItemType.Int64)  return secsValue.Value.GetType( ) == typeof ( long ) ?  1 : (secsValue.Value as Array).Length;
			if (secsValue.ItemType == SecsItemType.UInt64) return secsValue.Value.GetType( ) == typeof( ulong ) ?  1 : (secsValue.Value as Array).Length;
			if (secsValue.ItemType == SecsItemType.Single) return secsValue.Value.GetType( ) == typeof( float ) ?  1 : (secsValue.Value as Array).Length;
			if (secsValue.ItemType == SecsItemType.Double) return secsValue.Value.GetType( ) == typeof( double ) ? 1 : (secsValue.Value as Array).Length;
			if (secsValue.ItemType == SecsItemType.Bool)   return secsValue.Value.GetType( ) == typeof( bool ) ?   1 : (secsValue.Value as Array).Length;
			if (secsValue.ItemType == SecsItemType.Binary) return (secsValue.Value as byte[]).Length;
			if (secsValue.ItemType == SecsItemType.JIS8)   return (secsValue.Value as byte[]).Length;
			if (secsValue.ItemType == SecsItemType.ASCII)  return secsValue.Value.ToString( ).Length;
			return 0;
		}

		private static object GetObjectValue<T>( XElement element, Func<string, T> trans )
		{
			string value = GetAttribute( element, nameof( SecsValue.Value ), "", m => m );
			if (!value.Contains( "," )) return trans( value );
			else return value.ToStringArray( trans );
		}

		private static T GetAttribute<T>( XElement element, string name, T defaultValue, Func<string, T> trans )
		{
			XAttribute attribute = element.Attribute( name );
			if (attribute == null) return defaultValue;

			return trans( attribute.Value );
		}

		#endregion
	}
}
