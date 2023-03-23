using HslCommunication.Secs.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HslCommunication.Secs.Types
{
	/// <summary>
	/// 变量名称类对象
	/// </summary>
	public class VariableName
	{
		/// <summary>
		/// 变量的ID信息
		/// </summary>
		public long ID { get; set; }

		/// <summary>
		/// 变量的名称信息
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// 变量的单位信息
		/// </summary>
		public string Units { get; set; }

		/// <inheritdoc/>
		public override string ToString( ) => Name;




		#region Implicit Support

		/// <summary>
		/// 赋值操作，可以直接赋值 <see cref="OnlineData"/> 数据
		/// </summary>
		/// <param name="value"><see cref="SecsValue"/> 数值</param>
		/// <returns>等值的消息对象</returns>
		public static implicit operator VariableName( SecsValue value )
		{
			TypeHelper.TypeListCheck( value );
			SecsValue[] values = value.Value as SecsValue[];
			if (values != null)
				return new VariableName( )
				{
					ID    = Convert.ToInt64( values[0].Value ),
					Name  = values[1].Value as string,
					Units = values[2].Value as string,
				};
			else
				return null;
		}

		/// <summary>
		/// 也可以赋值给<see cref="SecsValue"/> 数据
		/// </summary>
		/// <param name="value"><see cref="SecsValue"/> 对象</param>
		/// <returns>等值的消息对象</returns>
		public static implicit operator SecsValue( VariableName value )
		{
			return new SecsValue( new object[] { value.ID, value.Name, value.Units } );
		}

		#endregion

	}
}
