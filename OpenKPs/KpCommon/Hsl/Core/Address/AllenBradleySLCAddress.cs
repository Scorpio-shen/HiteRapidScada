using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace HslCommunication.Core.Address
{
	/// <summary>
	/// 罗克韦尔PLC的地址信息
	/// </summary>
	public class AllenBradleySLCAddress : DeviceAddressDataBase
	{
		/// <summary>
		/// 获取或设置等待读取的数据的代码<br />
		/// Get or set the code of the data waiting to be read
		/// </summary>
		public byte DataCode { get; set; }

		/// <summary>
		/// 获取或设置PLC的DB块数据信息<br />
		/// Get or set PLC DB data information
		/// </summary>
		public ushort DbBlock { get; set; }

		/// <summary>
		/// 从指定的地址信息解析成真正的设备地址信息
		/// </summary>
		/// <param name="address">地址信息</param>
		/// <param name="length">数据长度</param>
		public override void Parse( string address, ushort length )
		{
			OperateResult<AllenBradleySLCAddress> addressData = ParseFrom( address, length );
			if (addressData.IsSuccess)
			{
				AddressStart = addressData.Content.AddressStart;
				Length = addressData.Content.Length;
				DataCode = addressData.Content.DataCode;
				DbBlock = addressData.Content.DbBlock;
			}
		}

		/// <inheritdoc/>
		public override string ToString( )
		{
			switch (DataCode)
			{
				case 0x8E: return $"A{DbBlock}:{AddressStart}";
				case 0x85: return $"B{DbBlock}:{AddressStart}";
				case 0x89: return $"N{DbBlock}:{AddressStart}";
				case 0x8A: return $"F{DbBlock}:{AddressStart}";
				case 0x8D: return $"ST{DbBlock}:{AddressStart}";
				case 0x84: return $"S{DbBlock}:{AddressStart}";
				case 0x87: return $"C{DbBlock}:{AddressStart}";
				case 0x83: return $"I{DbBlock}:{AddressStart}";
				case 0x82: return $"O{DbBlock}:{AddressStart}";
				case 0x88: return $"R{DbBlock}:{AddressStart}";
				case 0x86: return $"T{DbBlock}:{AddressStart}";
				case 0x91: return $"L{DbBlock}:{AddressStart}";
			}
			return AddressStart.ToString( );
		}

		#region Static Method

		/// <summary>
		/// 从实际的罗克韦尔的地址里面解析出地址对象，例如 A9:0<br />
		/// Parse the address object from the actual Rockwell address, such as A9:0
		/// </summary>
		/// <param name="address">实际的地址数据信息，例如 A9:0</param>
		/// <returns>是否成功的结果对象</returns>
		public static OperateResult<AllenBradleySLCAddress> ParseFrom( string address )
		{
			return ParseFrom( address, 0 );
		}

		/// <summary>
		/// 从实际的罗克韦尔的地址里面解析出地址对象，例如 A9:0<br />
		/// Parse the address object from the actual Rockwell address, such as A9:0
		/// </summary>
		/// <param name="address">实际的地址数据信息，例如 A9:0</param>
		/// <param name="length">读取的数据长度</param>
		/// <returns>是否成功的结果对象</returns>
		public static OperateResult<AllenBradleySLCAddress> ParseFrom( string address, ushort length )
		{
			if (!address.Contains( ":" )) return new OperateResult<AllenBradleySLCAddress>( "Address can't find ':', example : A9:0" );
			string[] adds = address.Split( new char[] { ':' } );

			try
			{
				AllenBradleySLCAddress allenBradleySLC = new AllenBradleySLCAddress( );
				//OperateResult<byte, ushort, ushort> result = new OperateResult<byte, ushort, ushort>( );
				// 还有一个没有添加，   0x8f BCD
				switch (adds[0][0])
				{
					case 'A': allenBradleySLC.DataCode = 0x8E; break;   // ASCII
					case 'B': allenBradleySLC.DataCode = 0x85; break;   // bit
					case 'N': allenBradleySLC.DataCode = 0x89; break;   // integer
					case 'F': allenBradleySLC.DataCode = 0x8A; break;   // floating point
					case 'S':
						{
							if (adds[0].Length > 1 && adds[0][1] == 'T')
								allenBradleySLC.DataCode = 0x8D;        // string
							else
								allenBradleySLC.DataCode = 0x84;        // status
							break;
						}
					case 'C': allenBradleySLC.DataCode = 0x87; break;   // counter
					case 'I': allenBradleySLC.DataCode = 0x83; break;   // input
					case 'O': allenBradleySLC.DataCode = 0x82; break;   // output
					case 'R': allenBradleySLC.DataCode = 0x88; break;   // control
					case 'T': allenBradleySLC.DataCode = 0x86; break;   // timer
					case 'L': allenBradleySLC.DataCode = 0x91; break;   // long integer
					default: throw new Exception( "Address code wrong, must be A,B,N,F,S,C,I,O,R,T,ST,L" );
				};
				switch (allenBradleySLC.DataCode)
				{
					case 0x84: allenBradleySLC.DbBlock = adds[0].Length == 1 ? (ushort)2 : ushort.Parse( adds[0].Substring( 1 ) ); break;
					case 0x82: allenBradleySLC.DbBlock = adds[0].Length == 1 ? (ushort)0 : ushort.Parse( adds[0].Substring( 1 ) ); break;
					case 0x83: allenBradleySLC.DbBlock = adds[0].Length == 1 ? (ushort)1 : ushort.Parse( adds[0].Substring( 1 ) ); break;
					case 0x8D: allenBradleySLC.DbBlock = adds[0].Length == 2 ? (ushort)1 : ushort.Parse( adds[0].Substring( 2 ) ); break;
					default:   allenBradleySLC.DbBlock = ushort.Parse( adds[0].Substring( 1 ) ); break;
				}

				allenBradleySLC.AddressStart = ushort.Parse( adds[1] );
				return OperateResult.CreateSuccessResult( allenBradleySLC );
			}
			catch (Exception ex)
			{
				return new OperateResult<AllenBradleySLCAddress>( "Wrong Address format: " + ex.Message );
			}
		}

		#endregion
	}
}
