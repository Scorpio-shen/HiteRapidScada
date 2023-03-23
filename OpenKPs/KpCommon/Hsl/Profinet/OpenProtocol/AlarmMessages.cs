using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HslCommunication.Profinet.OpenProtocol
{
	/// <summary>
	/// 报警类消息功能方法
	/// </summary>
	public class AlarmMessages
	{
		/// <summary>
		/// 指定Open通信类实例化一个对象
		/// </summary>
		/// <param name="openProtocol">开放协议的对象</param>
		public AlarmMessages( OpenProtocolNet openProtocol )
		{
			this.openProtocol = openProtocol;
		}

		/// <summary>
		/// A subscription for the alarms that can appear in the controller.
		/// </summary>
		/// <returns>是否订阅成功的结果对象</returns>
		public OperateResult AlarmSubscrib( ) => this.openProtocol.ReadCustomer( 70, 1, -1, -1, null );

		/// <summary>
		/// Reset the subscription for the controller alarms
		/// </summary>
		/// <returns>取消订阅是否成功的结果对象</returns>
		public OperateResult AlarmUnsubscribe( ) => this.openProtocol.ReadCustomer( 73, 1, -1, -1, null );

		/// <summary>
		/// The integrator can remotely acknowledge the current alarm on the controller by sending MID 0078. If no alarm is currently active when the controller receives the command, the command will be rejected.
		/// </summary>
		/// <returns>是否操作成功的结果对象</returns>
		public OperateResult AcknowledgeAlarmRemotelyOnController( ) => this.openProtocol.ReadCustomer( 78, 1, -1, -1, null );

		#region Private Member

		private OpenProtocolNet openProtocol;

		#endregion
	}
}
