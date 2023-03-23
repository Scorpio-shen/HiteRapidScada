using HslCommunication.Secs.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HslCommunication.Secs.Types
{
	/// <summary>
	/// 在线数据信息
	/// </summary>
	public class OnlineData
	{
		/// <summary>
		/// 实例化一个默认的对象
		/// </summary>
		public OnlineData( )
		{

		}

		/// <summary>
		/// 指定类型及其版本号来实例化一个对象
		/// </summary>
		/// <param name="model">类型信息</param>
		/// <param name="version">版本号</param>
		public OnlineData( string model, string version )
		{
			this.ModelType = model;
			this.SoftVersion = version;
		}

		/// <summary>
		/// equipment model type
		/// </summary>
		public string ModelType { get; set; }

		/// <summary>
		/// software revision
		/// </summary>
		public string SoftVersion { get; set; }



		#region Implicit Support

		/// <summary>
		/// 赋值操作，可以直接赋值 <see cref="OnlineData"/> 数据
		/// </summary>
		/// <param name="value"><see cref="SecsValue"/> 数值</param>
		/// <returns>等值的消息对象</returns>
		public static implicit operator OnlineData( SecsValue value )
		{
			TypeHelper.TypeListCheck( value );
			SecsValue[] values = value.Value as SecsValue[];
			if (values != null)
				return new OnlineData( values[0].Value.ToString( ), values[1].Value.ToString( ) );
			else
				return null;
		}

		/// <summary>
		/// 也可以赋值给<see cref="SecsValue"/> 数据
		/// </summary>
		/// <param name="value"><see cref="SecsValue"/> 对象</param>
		/// <returns>等值的消息对象</returns>
		public static implicit operator SecsValue( OnlineData value )
		{
			return new SecsValue( new object[] { value.ModelType, value.SoftVersion } );
		}

		#endregion
	}
}
