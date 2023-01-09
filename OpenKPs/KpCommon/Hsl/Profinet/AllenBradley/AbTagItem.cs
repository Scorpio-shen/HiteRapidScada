using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HslCommunication.Profinet.AllenBradley
{
	/// <summary>
	/// AB PLC的数据标签实体类<br />
	/// Data tag entity class of AB PLC
	/// </summary>
	public class AbTagItem
	{
		/// <summary>
		/// 实例化一个默认的对象<br />
		/// instantiate a default object
		/// </summary>
		public AbTagItem( )
		{
			ArrayLength = new int[] { -1, -1, -1 };
		}

		/// <summary>
		/// 实例ID<br />
		/// instance ID
		/// </summary>
		public uint InstanceID { get; set; }

		/// <summary>
		/// 当前标签的名字<br />
		/// the name of the current label
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// 当前标签的类型代号，例如 0x0C1 表示bool类型，如果当前的标签的<see cref="IsStruct"/>为 <c>True</c>，那么本属性表示结构体的实例ID<br />
		/// The type code of the current tag, for example 0x0C1 means bool type, if the current tag's <see cref="IsStruct"/> is <c>True</c>, 
		/// then this attribute indicates the instance ID of the structure
		/// </summary>
		public ushort SymbolType { get; set; }

		/// <summary>
		/// 数据的维度信息，默认是0，标量数据，1表示一维数组，2表示二维数组<br />
		/// The dimension information of the data, the default is 0, scalar data, 1 means a one-dimensional array, 2 means a two-dimensional array
		/// </summary>
		public int ArrayDimension { get; set; }

		/// <summary>
		/// 当前的标签是否结构体数据<br />
		/// Whether the current label is structured data
		/// </summary>
		public bool IsStruct { get; set; }

		/// <summary>
		/// 当前如果是数组，表示数组的长度，仅在读取结构体的变量信息时有效，为-1则是无效。
		/// </summary>
		public int[] ArrayLength { get; set; }

		/// <summary>
		/// 如果当前的标签是结构体的标签，则表示为结构体的成员信息
		/// </summary>
		[JsonIgnore]
		public AbTagItem[] Members { get; set; }

		/// <summary>
		/// 用户自定义的额外的对象
		/// </summary>
		[JsonIgnore]
		public object Tag { get; set; }

		/// <summary>
		/// 获取或设置本属性实际数据在结构体中的偏移位置信息<br />
		/// Get or set the offset position information of the actual data of this property in the structure
		/// </summary>
		public int ByteOffset { get; set; }

		/// <summary>
		/// 获取类型的文本描述信息
		/// </summary>
		/// <returns>文本信息</returns>
		public string GetTypeText( )
		{
			string array = string.Empty;
			if (ArrayDimension == 1)
				array = ArrayLength[0] >= 0 ? $"[{ArrayLength[0]}]" : "[]";
			else if (ArrayDimension == 2)
				array = $"[{ArrayLength[0]},{ArrayLength[1]}]";
			else if (ArrayDimension == 3)
				array = $"[{ArrayLength[0]},{ArrayLength[1]},{ArrayLength[2]}]";
			if (IsStruct) return "struct" + array;
			if (SymbolType == 0x08) return "date" + array;
			if (SymbolType == 0x09) return "time" + array;
			if (SymbolType == 0x0A) return "timeAndDate" + array;
			if (SymbolType == 0x0B) return "timeOfDate" + array;
			if (SymbolType == 0xC1) return "bool" + array;
			if (SymbolType == 0xC2) return "sbyte" + array;
			if (SymbolType == 0xC3) return "short" + array;
			if (SymbolType == 0xC4) return "int" + array;
			if (SymbolType == 0xC5) return "long" + array;
			if (SymbolType == 0xC6) return "byte" + array;
			if (SymbolType == 0xC7) return "ushort" + array;
			if (SymbolType == 0xC8) return "uint" + array;
			if (SymbolType == 0xC9) return "ulong" + array;
			if (SymbolType == 0xCA) return "float" + array;
			if (SymbolType == 0xCB) return "double" + array;
			if (SymbolType == 0xCC) return "struct";
			if (SymbolType == 0xD0) return "string";
			if (SymbolType == 0xD1) return "byte-str";
			if (SymbolType == 0xD2) return "word-str";
			if (SymbolType == AllenBradleyHelper.CIP_Type_D3)
			{
				if (ArrayDimension == 0) 
					return "bool[32]";
				else if (ArrayDimension == 1)
					return "bool" + $"[{ArrayLength[0] * 32}]";
				else
					return "bool-str" + array;
			}
			return "";
		}


		#region Object Override

		/// <inheritdoc/>
		public override string ToString( ) => Name;

		#endregion

		#region Private Member

		private void SetSymbolType( ushort value )
		{
			ArrayDimension = (value & 0x4000) == 0x4000 ? 2 : 0 + (value & 0x2000) == 0x2000 ? 1 : 0;
			IsStruct = (value & 0x8000) == 0x8000;
			// 屏蔽最高的4位
			SymbolType = (ushort)(value & 0x0FFF);
		}

		#endregion

		#region Static Helper

		/// <summary>
		/// 克隆单个的标签数据信息
		/// </summary>
		/// <param name="abTagItem">标签信息</param>
		/// <returns>新的实例的标签</returns>
		public static AbTagItem CloneBy( AbTagItem abTagItem )
		{
			if ( abTagItem == null ) return null;
			AbTagItem abTag      = new AbTagItem( );
			abTag.InstanceID     = abTagItem.InstanceID;
			abTag.Name           = abTagItem.Name;
			abTag.ByteOffset     = abTagItem.ByteOffset;
			abTag.SymbolType     = abTagItem.SymbolType;
			abTag.ArrayDimension = abTagItem.ArrayDimension;
			abTag.ArrayLength[0] = abTagItem.ArrayLength[0];
			abTag.ArrayLength[1] = abTagItem.ArrayLength[1];
			abTag.ArrayLength[2] = abTagItem.ArrayLength[2];
			abTag.IsStruct       = abTagItem.IsStruct;
			return abTag;
		}

		/// <summary>
		/// 克隆整个的标签数组信息
		/// </summary>
		/// <param name="abTagItems">标签数组信息</param>
		/// <returns>标签数组</returns>
		public static AbTagItem[] CloneBy( AbTagItem[] abTagItems )
		{
			AbTagItem[] abTags = new AbTagItem[abTagItems.Length];
			for( int i = 0; i < abTagItems.Length; i++)
			{
				abTags[i] = CloneBy( abTagItems[i] );
			}
			return abTags;
		}

		/// <summary>
		/// 从指定的原始字节的数据中，解析出实际的节点信息
		/// </summary>
		/// <param name="source">原始字节数据</param>
		/// <param name="index">起始的索引</param>
		/// <returns>标签信息</returns>
		public static AbTagItem PraseAbTagItem( byte[] source, ref int index )
		{
			AbTagItem td = new AbTagItem( );
			td.InstanceID = BitConverter.ToUInt32( source, index );
			index += 4;

			ushort nameLen = BitConverter.ToUInt16( source, index );
			index += 2;
			td.Name = Encoding.ASCII.GetString( source, index, nameLen );
			index += nameLen;

			// 当为标量数据的时候 SymbolType 就是类型，当为结构体的时候， SymbolType 就是地址
			td.SetSymbolType( BitConverter.ToUInt16( source, index ) );
			index += 2;

			// 维度信息
			td.ArrayLength[0] = BitConverter.ToInt32( source, index );
			index += 4;

			td.ArrayLength[1] = BitConverter.ToInt32( source, index );
			index += 4;

			td.ArrayLength[2] = BitConverter.ToInt32( source, index );
			index += 4;

			return td;
		}

		/// <summary>
		/// 从指定的原始字节的数据中，解析出实际的标签数组，如果是系统保留的数组，或是__开头的，则自动忽略。
		/// </summary>
		/// <param name="source">原始字节数据</param>
		/// <param name="index">起始的索引</param>
		/// <param name="isGlobalVariable">是否局部变量</param>
		/// <param name="instance">输出最后一个标签的实例ID</param>
		/// <returns>标签信息</returns>
		public static List<AbTagItem> PraseAbTagItems( byte[] source, int index, bool isGlobalVariable, out uint instance )
		{
			List<AbTagItem> array = new List<AbTagItem>( );
			instance = 0;
			while (index < source.Length)
			{
				AbTagItem td = PraseAbTagItem( source, ref index );
				instance = td.InstanceID;
				
				// 去掉系统保留的数据信息
				if ((td.SymbolType & 0x1000) != 0x1000)
					//  && !td.Name.Contains(":")
					if (!td.Name.StartsWith( "__" ) && !td.Name.Contains( ":" )) // 去掉 __ 开头的变量名称，并且不能包含冒号  详细查看1756-pm020-en-p.pdf  page51
					{
						if (!isGlobalVariable) td.Name = "Program:MainProgram." + td.Name;
						array.Add( td );
					}
			}
			return array;
		}

		/// <summary>
		/// 计算到达指定的字节的长度信息，可以用来计算固定分割符得字节长度
		/// </summary>
		/// <param name="source">原始字节数据</param>
		/// <param name="index">索引位置</param>
		/// <param name="value">等待判断的字节</param>
		/// <returns>字符串长度，如果不存在，返回-1</returns>
		private static int CalculatesSpecifiedCharacterLength( byte[] source, int index, byte value )
		{
			for (int i = index; i < source.Length; i++)
			{
				if (source[i] == value) return i - index;
			}
			return -1;
		}

		private static string CalculatesString( byte[] source, ref int index, byte value )
		{
			if (index >= source.Length) return string.Empty;
			int length = CalculatesSpecifiedCharacterLength( source, index, value );
			if (length < 0)
			{
				index = source.Length;
				return string.Empty;
			}
			string name = Encoding.ASCII.GetString( source, index, length );
			index += length + 1;
			return name;
		}

		/// <summary>
		/// 从结构体的数据中解析出实际的子标签信息
		/// </summary>
		/// <param name="source">原始字节</param>
		/// <param name="index">偏移索引</param>
		/// <param name="structHandle">结构体句柄</param>
		/// <returns>结果内容</returns>
		public static List<AbTagItem> PraseAbTagItemsFromStruct( byte[] source, int index, AbStructHandle structHandle )
		{
			List<AbTagItem> array = new List<AbTagItem>( );
			int offset = structHandle.MemberCount * 8 + index;
			string structName = CalculatesString( source, ref offset, 0x00 );
			for (int i = 0; i < structHandle.MemberCount; i++)
			{
				AbTagItem abTagItem      = new AbTagItem( );
				abTagItem.ArrayLength[0] = BitConverter.ToUInt16( source, 8 * i + index + 0 );
				abTagItem.SetSymbolType( BitConverter.ToUInt16( source, 8 * i + index + 2 ) );
				abTagItem.ByteOffset     = BitConverter.ToInt32(  source, 8 * i + index + 4 ) + 2;
				abTagItem.Name           = CalculatesString( source, ref offset, 0x00 );
				//if (!abTagItem.Name.StartsWith( "ZZZZZZZZZZ" ))
				array.Add( abTagItem );
			}
			return array;
		}

		#endregion

	}
}
