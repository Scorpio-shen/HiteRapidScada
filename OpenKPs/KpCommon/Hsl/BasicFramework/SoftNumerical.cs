﻿using HslCommunication.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace HslCommunication.BasicFramework
{
	/********************************************************************************************
	 * 
	 *    一个高度灵活的流水号生成的类，允许根据指定规则加上时间数据进行生成
	 * 
	 *    根据保存机制进行优化，必须做到好并发量
	 * 
	 ********************************************************************************************/


	/// <summary>
	/// 一个用于自动流水号生成的类，必须指定保存的文件，实时保存来确认安全
	/// </summary>
	/// <remarks>
	/// <note type="important">
	/// 序号生成器软件，当获取序列号，清空序列号操作后，会自动的将ID号存储到本地的文件中，存储方式采用乐观并发模型实现。
	/// </note>
	/// </remarks>
	/// <example>
	/// 此处举个例子，也是Demo程序的源代码，包含了2个按钮的示例和瞬间调用100万次的性能示例。
	/// <note type="tip">百万次调用的实际耗时取决于计算机的性能，不同的计算机的表现存在差异，比如作者的：i5-4590cpu,内存ddr3-8G表示差不多在800毫秒左右</note>
	/// <code lang="cs" source="TestProject\HslCommunicationDemo\FormSeqCreate.cs" region="FormSeqCreate" title="示例代码" />
	/// </example>
	public sealed class SoftNumericalOrder : SoftFileSaveBase
	{
		#region Constructor

		/// <summary>
		/// 实例化一个流水号生成的对象
		/// </summary>
		/// <param name="textHead">流水号的头文本</param>
		/// <param name="timeFormate">流水号带的时间信息</param>
		/// <param name="numberLength">流水号数字的标准长度，不够补0</param>
		/// <param name="fileSavePath">流水号存储的文本位置</param>
		public SoftNumericalOrder( string textHead, string timeFormate, int numberLength, string fileSavePath )
		{
			LogHeaderText = "SoftNumericalOrder";
			TextHead = textHead;
			TimeFormate = timeFormate;
			NumberLength = numberLength;
			FileSavePath = fileSavePath;
			LoadByFile( );

			AsyncCoordinator = new HslAsyncCoordinator( ( ) =>
			   {
				   if (!string.IsNullOrEmpty( FileSavePath ))
				   {
					   using (System.IO.StreamWriter sw = new System.IO.StreamWriter( FileSavePath, false, Encoding.Default ))
					   {
						   sw.Write( CurrentIndex );
					   }
				   }
			   } );

		}

		#endregion

		#region Private Member

		/// <summary>
		/// 当前的生成序列号
		/// </summary>
		private long CurrentIndex = 0;
		/// <summary>
		/// 流水号的文本头
		/// </summary>
		private string TextHead = string.Empty;
		/// <summary>
		/// 时间格式默认年月日
		/// </summary>
		private string TimeFormate = "yyyyMMdd";
		/// <summary>
		/// 流水号数字应该显示的长度
		/// </summary>
		private int NumberLength = 5;

		#endregion

		#region Public Method

		/// <summary>
		/// 获取流水号的值
		/// </summary>
		/// <returns>字符串信息</returns>
		public override string ToSaveString( )
		{
			return CurrentIndex.ToString( );
		}

		/// <summary>
		/// 加载流水号
		/// </summary>
		/// <param name="content">源字符串信息</param>
		public override void LoadByString( string content )
		{
			CurrentIndex = Convert.ToInt64( content );
		}

		/// <summary>
		/// 清除流水号计数，进行重新计数
		/// </summary>
		public void ClearNumericalOrder( )
		{
			Interlocked.Exchange( ref CurrentIndex, 0 );
			AsyncCoordinator.StartOperaterInfomation( );
		}

		/// <summary>
		/// 获取流水号数据
		/// </summary>
		/// <returns>新增计数后的信息</returns>
		public string GetNumericalOrder( )
		{
			long number = Interlocked.Increment( ref CurrentIndex );
			AsyncCoordinator.StartOperaterInfomation( );
			if (string.IsNullOrEmpty( TimeFormate ))
			{
				return TextHead + number.ToString( ).PadLeft( NumberLength, '0' );
			}
			else
			{
				return TextHead + DateTime.Now.ToString( TimeFormate ) + number.ToString( ).PadLeft( NumberLength, '0' );
			}
		}

		/// <summary>
		/// 获取流水号数据
		/// </summary>
		/// <param name="textHead">指定一个新的文本头</param>
		/// <returns>带头信息的计数后的信息</returns>
		public string GetNumericalOrder( string textHead )
		{
			long number = Interlocked.Increment( ref CurrentIndex );
			AsyncCoordinator.StartOperaterInfomation( );
			if (string.IsNullOrEmpty( TimeFormate ))
			{
				return textHead + number.ToString( ).PadLeft( NumberLength, '0' );
			}
			else
			{
				return textHead + DateTime.Now.ToString( TimeFormate ) + number.ToString( ).PadLeft( NumberLength, '0' );
			}
		}

		/// <summary>
		/// 单纯的获取数字形式的流水号
		/// </summary>
		/// <returns>新增计数后的信息</returns>
		public long GetLongOrder( )
		{
			long number = Interlocked.Increment( ref CurrentIndex );
			AsyncCoordinator.StartOperaterInfomation( );
			return number;
		}

		#endregion

		#region High Performance Save

		/// <summary>
		/// 高性能存储块
		/// </summary>
		private HslAsyncCoordinator AsyncCoordinator = null;

		#endregion
	}

	/// <summary>
	/// 一个简单的不持久化的序号自增类，采用线程安全实现，并允许指定最大数字，将包含该最大值，到达后清空从指定数开始<br />
	/// A simple non-persistent serial number auto-increment class, which is implemented with thread safety, and allows the maximum number to be specified, which will contain the maximum number, and will be cleared from the specified number upon arrival.
	/// </summary>
	/// <example>
	/// 先来看看一个简单的应用的
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\BasicFramework\SoftIncrementCountSample.cs" region="Sample1" title="简单示例" />
	/// 再来看看一些复杂的情况
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\BasicFramework\SoftIncrementCountSample.cs" region="Sample2" title="复杂示例" />
	/// 其他一些特殊的设定
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\BasicFramework\SoftIncrementCountSample.cs" region="Sample3" title="其他示例" />
	/// </example>
	public sealed class SoftIncrementCount : IDisposable
	{
		#region Constructor

		/// <summary>
		/// 实例化一个自增信息的对象，包括最大值，初始值，增量值<br />
		/// Instantiate an object with incremental information, including the maximum value and initial value, IncreaseTick
		/// </summary>
		/// <param name="max">数据的最大值，必须指定</param>
		/// <param name="start">数据的起始值，默认为0</param>
		/// <param name="tick">每次的增量值</param>
		public SoftIncrementCount( long max, long start = 0, int tick = 1 )
		{
			this.start          = start;
			this.max            = max;
			this.current        = start;
			this.IncreaseTick   = tick;
			this.hybirdLock     = new SimpleHybirdLock( );
		}

		#endregion

		#region Private Member

		private long start = 0;
		private long current = 0;
		private long max = long.MaxValue;
		private SimpleHybirdLock hybirdLock;

		#endregion

		#region Public Method

		/// <summary>
		/// 获取自增信息，获得数据之后，下一次获取将会自增，如果自增后大于最大值，则会重置为最小值，如果小于最小值，则会重置为最大值。<br />
		/// Get the auto-increment information. After getting the data, the next acquisition will auto-increase. 
		/// If the auto-increment is greater than the maximum value, it will reset to the minimum value.
		/// If the auto-increment is smaller than the minimum value, it will reset to the maximum value.
		/// </summary>
		/// <returns>计数自增后的值</returns>
		public long GetCurrentValue( )
		{
			long value = 0;
			hybirdLock.Enter( );

			value = current;
			current += IncreaseTick;
			if (current > max)
			{
				current = start;
			}
			else if (current < start)
			{
				current = max;
			}

			hybirdLock.Leave( );
			return value;
		}

		/// <summary>
		/// 重置当前序号的最大值，最大值应该大于初始值，如果当前值大于最大值，则当前值被重置为最大值<br />
		/// Reset the maximum value of the current serial number. The maximum value should be greater than the initial value. 
		/// If the current value is greater than the maximum value, the current value is reset to the maximum value.
		/// </summary>
		/// <param name="max">最大值</param>
		public void ResetMaxValue( long max )
		{
			hybirdLock.Enter( );

			if (max > start)
			{
				if (max < current)
					current = start;
				this.max = max;
			}

			hybirdLock.Leave( );
		}

		/// <summary>
		/// 重置当前序号的初始值，需要小于最大值，如果当前值小于初始值，则当前值被重置为初始值。<br />
		/// To reset the initial value of the current serial number, it must be less than the maximum value. 
		/// If the current value is less than the initial value, the current value is reset to the initial value.
		/// </summary>
		/// <param name="start">初始值</param>
		public void ResetStartValue( long start )
		{
			hybirdLock.Enter( );

			if (start < this.max)
			{
				if (current < start)
					current = start;
				this.start = start;
			}

			hybirdLock.Leave( );
		}

		/// <summary>
		/// 将当前的值重置为初始值。<br />
		/// Reset the current value to the initial value.
		/// </summary>
		public void ResetCurrentValue( )
		{
			hybirdLock.Enter( );

			this.current = this.start;

			hybirdLock.Leave( );
		}

		/// <summary>
		/// 将当前的值重置为指定值，该值不能大于max，如果大于max值，就会自动设置为max<br />
		/// Reset the current value to the specified value. The value cannot be greater than max. If it is greater than max, it will be automatically set to max.
		/// </summary>
		/// <param name="value">指定的数据值</param>
		public void ResetCurrentValue( long value )
		{
			hybirdLock.Enter( );

			if (value > max)
			{
				this.current = max;
			}
			else if (value < start)
			{
				this.current = start;
			}
			else
			{
				this.current = value;
			}

			hybirdLock.Leave( );
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// 增加的单元，如果设置为0，就是不增加。如果为小于0，那就是减少，会变成负数的可能。<br />
		/// Increased units, if set to 0, do not increase. If it is less than 0, it is a decrease and it may become a negative number.
		/// </summary>
		public int IncreaseTick { get; set; } = 1;

		/// <summary>
		/// 获取当前的计数器的最大的设置值。<br />
		/// Get the maximum setting value of the current counter.
		/// </summary>
		public long MaxValue => max;

		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString( ) => $"SoftIncrementCount[{this.current}]";

		#endregion

		#region IDisposable Support

		private bool disposedValue = false; // 要检测冗余调用

		void Dispose( bool disposing )
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					hybirdLock.Dispose( );
				}

				disposedValue = true;
			}
		}

		/// <inheritdoc cref="IDisposable.Dispose"/>
		public void Dispose( )
		{
			Dispose( true );
		}

		#endregion
	}
}
