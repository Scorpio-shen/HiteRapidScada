using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HslCommunication.Core.Address
{
	/// <summary>
	/// 信捷内部协议的地址类对象<br />
	/// The address class object of Xinjie internal protocol
	/// </summary>
	public class XinJEAddress : DeviceAddressDataBase
	{
		#region Construction

		/// <summary>
		/// 实例化一个默认的对象<br />
		/// instantiate a default object
		/// </summary>
		public XinJEAddress( )
		{

		}

		/// <summary>
		/// 指定类型，地址偏移，临界地址来实例化一个对象<br />
		/// Specify the type, address offset, and critical address to instantiate an object
		/// </summary>
		/// <param name="dataCode">数据的类型代号</param>
		/// <param name="address">偏移地址信息</param>
		/// <param name="criticalAddress">临界地址信息</param>
		/// <param name="station">站号信息</param>
		public XinJEAddress( byte dataCode, int address, int criticalAddress, byte station )
		{
			this.DataCode        = dataCode;
			this.AddressStart    = address;
			this.CriticalAddress = criticalAddress;
			this.Station         = station;
		}

		#endregion

		/// <summary>
		/// 获取或设置等待读取的数据的代码<br />
		/// Get or set the code of the data waiting to be read
		/// </summary>
		public byte DataCode { get; set; }

		/// <summary>
		/// 获取或设置当前的站号信息<br />
		/// Get or set the current station number information
		/// </summary>
		public byte Station { get; set; }

		/// <summary>
		/// 获取或设置协议升级时候的临界地址信息<br />
		/// Get or set the critical address information when the protocol is upgraded
		/// </summary>
		public int CriticalAddress { get; set; }

		/// <inheritdoc/>
		public override string ToString( )
		{
			return AddressStart.ToString( );
		}


		#region Static Method

		/// <summary>
		/// 从实际的信捷PLC的地址里面解析出地址对象<br />
		/// Resolve the address object from the actual XinJE address
		/// </summary>
		/// <param name="address">信捷的地址数据信息</param>
		/// <param name="length">读取的长度信息</param>
		/// <param name="defaultStation">默认的站号信息</param>
		/// <returns>是否成功的结果对象</returns>
		public static OperateResult<XinJEAddress> ParseFrom( string address, ushort length, byte defaultStation )
		{
			OperateResult<XinJEAddress> analysis = ParseFrom( address, defaultStation );
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<XinJEAddress>( analysis );

			analysis.Content.Length = length;
			return OperateResult.CreateSuccessResult( analysis.Content );
		}

		/// <inheritdoc cref="ParseFrom(string, ushort, byte)"/>
		public static OperateResult<XinJEAddress> ParseFrom( string address, byte defaultStation )
		{
			try
			{
				byte stat = (byte)HslHelper.ExtractParameter( ref address, "s", defaultStation );
				if      (address.StartsWith( "HSCD" )) return OperateResult.CreateSuccessResult( new XinJEAddress( 0x8B, int.Parse( address.Substring( 4 ) ), int.MaxValue  , stat ) );
				else if (address.StartsWith( "ETD" ))  return OperateResult.CreateSuccessResult( new XinJEAddress( 0x85, int.Parse( address.Substring( 3 ) ), 0             , stat ) );
				else if (address.StartsWith( "HSD" ))  return OperateResult.CreateSuccessResult( new XinJEAddress( 0x8C, int.Parse( address.Substring( 3 ) ), 1024          , stat));
				else if (address.StartsWith( "HTD" ))  return OperateResult.CreateSuccessResult( new XinJEAddress( 0x89, int.Parse( address.Substring( 3 ) ), 1024          , stat ) );
				else if (address.StartsWith( "HCD" ))  return OperateResult.CreateSuccessResult( new XinJEAddress( 0x8A, int.Parse( address.Substring( 3 ) ), 1024          , stat ) );
				else if (address.StartsWith( "SFD" ))  return OperateResult.CreateSuccessResult( new XinJEAddress( 0x8E, int.Parse( address.Substring( 3 ) ), 4096          , stat ) );
				else if (address.StartsWith( "HSC" ))  return OperateResult.CreateSuccessResult( new XinJEAddress( 0x0C, int.Parse( address.Substring( 3 ) ), int.MaxValue  , stat ) );
				else if (address.StartsWith( "SD" ))   return OperateResult.CreateSuccessResult( new XinJEAddress( 0x83, int.Parse( address.Substring( 2 ) ), 4096          , stat));
				else if (address.StartsWith( "TD" ))   return OperateResult.CreateSuccessResult( new XinJEAddress( 0x81, int.Parse( address.Substring( 2 ) ), 4096          , stat));
				else if (address.StartsWith( "CD" ))   return OperateResult.CreateSuccessResult( new XinJEAddress( 0x82, int.Parse( address.Substring( 2 ) ), 4096          , stat));
				else if (address.StartsWith( "HD" ))   return OperateResult.CreateSuccessResult( new XinJEAddress( 0x88, int.Parse( address.Substring( 2 ) ), 6144          , stat));
				else if (address.StartsWith( "FD" ))   return OperateResult.CreateSuccessResult( new XinJEAddress( 0x8D, int.Parse( address.Substring( 2 ) ), 8192          , stat));
				else if (address.StartsWith( "ID" ))   return OperateResult.CreateSuccessResult( new XinJEAddress( 0x86, int.Parse( address.Substring( 2 ) ), 0             , stat));
				else if (address.StartsWith( "QD" ))   return OperateResult.CreateSuccessResult( new XinJEAddress( 0x87, int.Parse( address.Substring( 2 ) ), 0             , stat));
				else if (address.StartsWith( "SM" ))   return OperateResult.CreateSuccessResult( new XinJEAddress( 0x0D, int.Parse( address.Substring( 2 ) ), 4096          , stat));
				else if (address.StartsWith( "ET" ))   return OperateResult.CreateSuccessResult( new XinJEAddress( 0x07, int.Parse( address.Substring( 2 ) ), 0             , stat));
				else if (address.StartsWith( "HM" ))   return OperateResult.CreateSuccessResult( new XinJEAddress( 0x08, int.Parse( address.Substring( 2 ) ), 6144          , stat));
				else if (address.StartsWith( "HS" ))   return OperateResult.CreateSuccessResult( new XinJEAddress( 0x09, int.Parse( address.Substring( 2 ) ), int.MaxValue  , stat));
				else if (address.StartsWith( "HT" ))   return OperateResult.CreateSuccessResult( new XinJEAddress( 0x0A, int.Parse( address.Substring( 2 ) ), 1024          , stat));
				else if (address.StartsWith( "HC" ))   return OperateResult.CreateSuccessResult( new XinJEAddress( 0x0B, int.Parse( address.Substring( 2 ) ), 1024          , stat));
				else if (address.StartsWith( "D" ))    return OperateResult.CreateSuccessResult( new XinJEAddress( 0x80, int.Parse( address.Substring( 1 ) ), 20480         , stat));
				else if (address.StartsWith( "M" ))    return OperateResult.CreateSuccessResult( new XinJEAddress( 0x03, int.Parse( address.Substring( 1 ) ), 20480         , stat));
				else if (address.StartsWith( "T" ))    return OperateResult.CreateSuccessResult( new XinJEAddress( 0x05, int.Parse( address.Substring( 1 ) ), 4096          , stat));
				else if (address.StartsWith( "C" ))    return OperateResult.CreateSuccessResult( new XinJEAddress( 0x06, int.Parse( address.Substring( 1 ) ), 4096          , stat));
				else if (address.StartsWith( "Y" ))    return OperateResult.CreateSuccessResult( new XinJEAddress( 0x02, Convert.ToInt32( address.Substring( 1 ), 8 ), int.MaxValue  , stat ) );
				else if (address.StartsWith( "X" ))    return OperateResult.CreateSuccessResult( new XinJEAddress( 0x01, Convert.ToInt32( address.Substring( 1 ), 8 ), int.MaxValue  , stat ) );
				else if (address.StartsWith( "S" ))    return OperateResult.CreateSuccessResult( new XinJEAddress( 0x04, int.Parse( address.Substring( 1 ) ), 8000          , stat ) );
				else return new OperateResult<XinJEAddress>( StringResources.Language.NotSupportedDataType );
			}
			catch (Exception ex)
			{
				return new OperateResult<XinJEAddress>( StringResources.Language.AddressFormatWrong + ex.Message );
			}
		}

		#endregion

	}
}
