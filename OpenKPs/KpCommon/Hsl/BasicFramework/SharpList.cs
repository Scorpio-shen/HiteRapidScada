﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HslCommunication.Core;

namespace HslCommunication.BasicFramework
{
	/// <summary>
	/// 一个高效的数组管理类，用于高效控制固定长度的数组实现<br />
	/// An efficient array management class for efficient control of fixed-length array implementations
	/// </summary>
	/// <typeparam name="T">泛型类型</typeparam>
	public class SharpList<T>
	{
		#region Constructor

		/// <summary>
		/// 实例化一个对象，需要指定数组的最大数据对象
		/// </summary>
		/// <param name="count">数据的个数</param>
		/// <param name="appendLast">是否从最后一个数添加</param>
		public SharpList( int count, bool appendLast = false )
		{
			if (count > 8192) capacity = 4096;

			this.array = new T[capacity + count];
			this.hybirdLock = new SimpleHybirdLock( );
			this.count = count;
			if (appendLast) this.lastIndex = count;
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// 获取数组的个数<br />
		/// Get the number of arrays
		/// </summary>
		public int Count => count;

		#endregion

		#region Public Method

		/// <summary>
		/// 新增一个数据值<br />
		/// Add a data value
		/// </summary>
		/// <param name="value">数据值</param>
		public void Add( T value )
		{
			hybirdLock.Enter( );

			if (lastIndex >= (capacity + count))
			{
				// 需要重新挪位置了
				T[] buffer = new T[capacity + count];
				Array.Copy( array, capacity, buffer, 0, count );
				array = buffer;
				lastIndex = count;
			}
			array[lastIndex++] = value;
			hybirdLock.Leave( );
		}

		/// <summary>
		/// 批量的增加数据<br />
		/// Increase data in batches
		/// </summary>
		/// <param name="values">批量数据信息</param>
		public void Add( IEnumerable<T> values )
		{
			foreach(var m in values)
			{
				Add( m );
			}
		}

		/// <summary>
		/// 获取数据的数组值<br />
		/// Get array value of data
		/// </summary>
		/// <returns>数组值</returns>
		public T[] ToArray( )
		{
			T[] result = null;
			hybirdLock.Enter( );

			if (lastIndex < count)
			{
				result = new T[lastIndex];
				Array.Copy( array, 0, result, 0, lastIndex );
			}
			else
			{
				result = new T[count];
				Array.Copy( array, lastIndex - count, result, 0, count );
			}
			hybirdLock.Leave( );
			return result;
		}

		/// <summary>
		/// 获取或设置指定索引的位置的数据<br />
		/// Gets or sets the data at the specified index
		/// </summary>
		/// <param name="index">索引位置</param>
		/// <returns>数据值</returns>
		public T this[int index]
		{
			get
			{
				if (index < 0) throw new IndexOutOfRangeException( "Index must larger than zero" );
				if (index >= count) throw new IndexOutOfRangeException( "Index must smaller than array length" );
				T tmp = default( T );
				hybirdLock.Enter( );

				if (lastIndex < count)
				{
					tmp = array[index];
				}
				else
				{
					tmp = array[index + lastIndex - count];
				}

				hybirdLock.Leave( );
				return tmp;
			}
			set
			{
				if (index < 0) throw new IndexOutOfRangeException( "Index must larger than zero" );
				if (index >= count) throw new IndexOutOfRangeException( "Index must smaller than array length" );
				hybirdLock.Enter( );

				if (lastIndex < count)
				{
					array[index] = value;
				}
				else
				{
					array[index + lastIndex - count] = value;
				}

				hybirdLock.Leave( );
			}
		}

		/// <summary>
		/// 获取最后一个值，如果从来没有添加过，则引发异常<br />
		/// Gets the last value and throws an exception if it has never been added
		/// </summary>
		/// <returns>值信息</returns>
		public T LastValue( )
		{
			T result = default(T);
			hybirdLock.Enter( );
			try
			{
				if (lastIndex - 1 < count + capacity)
				{
					result = array[lastIndex - 1];
				}
				hybirdLock.Leave( );
			}
			catch
			{
				hybirdLock.Leave( );
				throw;
			}
			return result;
		}

		#endregion

		#region private Member

		private T[] array;
		private int capacity = 2048;          // 整个数组的附加容量
		private int count = 0;                // 数组的实际数据容量
		private int lastIndex = 0;            // 最后一个数的索引位置
		private SimpleHybirdLock hybirdLock;  // 数组的操作锁

		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString( ) => $"SharpList<{typeof(T)}>[{capacity}]";

		#endregion
	}
}
