﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace HslCommunication.Reflection
{
	/// <summary>
	/// Hsl相关地址的属性信息
	/// </summary>
	public class HslAddressProperty
	{
		/// <summary>
		/// 该属性绑定的地址特性
		/// </summary>
		public HslDeviceAddressAttribute DeviceAddressAttribute { get; set; }

		/// <summary>
		/// 地址绑定的属性信息
		/// </summary>
		public PropertyInfo PropertyInfo { get; set; }

		/// <summary>
		/// 起始的字节偏移信息
		/// </summary>
		public int ByteOffset { get; set; }

		/// <summary>
		/// 读取的字节的长度信息
		/// </summary>
		public int ByteLength { get; set; }

		/// <summary>
		/// 缓存的数据对象
		/// </summary>
		public byte[] Buffer { get; set; }
	}
}
