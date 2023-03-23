﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HslCommunication.Profinet.Beckhoff.Helper
{
	/// <summary>
	/// Ads标签信息
	/// </summary>
	public class AdsTagItem
	{
		/// <summary>
		/// 实例化一个默认的对象
		/// </summary>
		/// <param name="name">标签名</param>
		/// <param name="buffer">缓存的数据对象</param>
		public AdsTagItem( string name, byte[] buffer )
		{
			this.TagName = name;
			this.Buffer  = buffer;
		}

		/// <summary>
		/// 标签的名称
		/// </summary>
		public string TagName { get; set; }

		/// <summary>
		/// 标签的数据缓存信息
		/// </summary>
		public byte[] Buffer { get; set; }

		/// <summary>
		/// 绝对地址信息
		/// </summary>
		public uint Location { get; set; }
	}
}
