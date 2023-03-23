using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HslCommunication.Profinet.OpenProtocol
{
	/// <summary>
	/// Time messages
	/// </summary>
	public class TimeMessages
	{
		/// <summary>
		/// 指定Open通信类实例化一个对象
		/// </summary>
		/// <param name="openProtocol">开放协议的对象</param>
		public TimeMessages( OpenProtocolNet openProtocol )
		{
			this.openProtocol = openProtocol;
		}

		/// <summary>
		/// Read time request.
		/// </summary>
		/// <returns>包含时间的结果对象</returns>
		public OperateResult<DateTime> ReadTimeUpload( )
		{
			OperateResult<string> read = this.openProtocol.ReadCustomer( 80, 1, -1, -1, null );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<DateTime>( read );

			return OperateResult.CreateSuccessResult( DateTime.ParseExact( read.Content.Substring( 20, 19 ), "yyyy-MM-dd:HH:mm:ss", null ) );
		}

		/// <summary>
		/// Set the time in the controller.
		/// </summary>
		/// <param name="dateTime">指定的时间</param>
		/// <returns>是否设置成功的结果对象</returns>
		public OperateResult SetTime( DateTime dateTime ) => this.openProtocol.ReadCustomer( 82, 1, -1, -1, new List<string>( ) { dateTime.ToString( "yyyy-MM-dd:HH:mm:ss" ) } );

		#region Private Member

		private OpenProtocolNet openProtocol;

		#endregion
	}
}
