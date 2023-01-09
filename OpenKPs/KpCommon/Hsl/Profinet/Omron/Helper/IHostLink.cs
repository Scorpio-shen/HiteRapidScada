using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HslCommunication.Core;
using HslCommunication;
using HslCommunication.Core.Net;

namespace HslCommunication.Profinet.Omron.Helper
{
	/// <summary>
	/// HostLink的接口实现
	/// </summary>
	public interface IHostLink : IReadWriteDevice
	{

		/// <inheritdoc cref="OmronHostLinkOverTcp.ICF"/>
		byte ICF { get; set; }

		/// <inheritdoc cref="OmronHostLinkOverTcp.DA2"/>
		byte DA2 { get; set; }

		/// <inheritdoc cref="OmronHostLinkOverTcp.SA2"/>
		byte SA2 { get; set; }

		/// <inheritdoc cref="OmronHostLinkOverTcp.SID"/>
		byte SID { get; set; }

		/// <inheritdoc cref="OmronHostLinkOverTcp.ResponseWaitTime"/>
		byte ResponseWaitTime { get; set; }

		/// <inheritdoc cref="OmronHostLinkOverTcp.UnitNumber"/>
		byte UnitNumber { get; set; }

		/// <inheritdoc cref="OmronHostLinkOverTcp.ReadSplits"/>
		int ReadSplits { get; set; }

	}
}
