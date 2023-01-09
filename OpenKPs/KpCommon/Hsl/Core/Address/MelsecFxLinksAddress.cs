using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HslCommunication.Core.Address
{
	/// <summary>
	/// 三菱的FxLinks协议信息
	/// </summary>
	public class MelsecFxLinksAddress : DeviceAddressDataBase
	{
		#region Constructor

		/// <summary>
		/// 实例化一个默认的对象
		/// </summary>
		public MelsecFxLinksAddress( )
		{

		}

		#endregion

		/// <summary>
		/// 当前的地址类型信息
		/// </summary>
		public string TypeCode { get; set; }

		/// <inheritdoc/>
		public override void Parse( string address, ushort length )
		{
			base.Parse( address, length );
		}

		/// <inheritdoc/>
		public override string ToString( )
		{
			switch( TypeCode)
			{
				case "X":
				case "Y": return TypeCode + Convert.ToString( AddressStart, 8 ).PadLeft( AddressStart >= 10000 ? 6 : 4, '0' );
				case "M":
				case "S":
				case "TS":
				case "TN":
				case "CS":
				case "CN":
				case "D":
				case "R":
				default: return TypeCode + AddressStart.ToString( "D" + ( (AddressStart >= 10000 ? 7 : 5) - TypeCode.Length).ToString( ) );
			}
		}

		/// <inheritdoc cref="Parse(string, ushort)"/>
		public static OperateResult<MelsecFxLinksAddress> ParseFrom( string address ) => ParseFrom( address, 0 );

		/// <summary>
		/// 从三菱FxLinks协议里面解析出实际的地址信息
		/// </summary>
		/// <param name="address">三菱的地址信息</param>
		/// <param name="length">读取的长度信息</param>
		/// <returns>解析结果信息</returns>
		public static OperateResult<MelsecFxLinksAddress> ParseFrom( string address, ushort length )
		{
			MelsecFxLinksAddress melsecFxLinks = new MelsecFxLinksAddress( );
			melsecFxLinks.Length = length;
			try
			{
				switch (address[0])
				{
					case 'X':
					case 'x':
						{
							melsecFxLinks.AddressStart = Convert.ToUInt16( address.Substring( 1 ), 8 );
							melsecFxLinks.TypeCode = "X";
							break;
						}
					case 'Y':
					case 'y':
						{
							melsecFxLinks.AddressStart = Convert.ToUInt16( address.Substring( 1 ), 8 );
							melsecFxLinks.TypeCode = "Y";
							break;
						}
					case 'M':
					case 'm':
						{
							melsecFxLinks.AddressStart = Convert.ToUInt16( address.Substring( 1 ), 10 );
							melsecFxLinks.TypeCode = "M";
							break;
						}
					case 'S':
					case 's':
						{
							melsecFxLinks.AddressStart = Convert.ToUInt16( address.Substring( 1 ), 10 );
							melsecFxLinks.TypeCode = "S";
							break;
						}
					case 'T':
					case 't':
						{
							if (address[1] == 'S' || address[1] == 's')
							{
								melsecFxLinks.AddressStart = Convert.ToUInt16( address.Substring( 2 ), 10 );
								melsecFxLinks.TypeCode = "TS";
								break;
							}
							else if (address[1] == 'N' || address[1] == 'n')
							{
								melsecFxLinks.AddressStart = Convert.ToUInt16( address.Substring( 2 ), 10 );
								melsecFxLinks.TypeCode = "TN";
								break;
							}
							else
							{
								throw new Exception( StringResources.Language.NotSupportedDataType );
							}
						}
					case 'C':
					case 'c':
						{
							if (address[1] == 'S' || address[1] == 's')
							{
								melsecFxLinks.AddressStart = Convert.ToUInt16( address.Substring( 2 ), 10 );
								melsecFxLinks.TypeCode = "CS";
								break;
							}
							else if (address[1] == 'N' || address[1] == 'n')
							{
								melsecFxLinks.AddressStart = Convert.ToUInt16( address.Substring( 2 ), 10 );
								melsecFxLinks.TypeCode = "CN";
								break;
							}
							else
							{
								throw new Exception( StringResources.Language.NotSupportedDataType );
							}
						}
					case 'D':
					case 'd':
						{
							melsecFxLinks.AddressStart = Convert.ToUInt16( address.Substring( 1 ), 10 );
							melsecFxLinks.TypeCode = "D";
							break;
						}
					case 'R':
					case 'r':
						{
							melsecFxLinks.AddressStart = Convert.ToUInt16( address.Substring( 1 ), 10 );
							melsecFxLinks.TypeCode = "R";
							break;
						}
					default: throw new Exception( StringResources.Language.NotSupportedDataType );
				}
				return OperateResult.CreateSuccessResult( melsecFxLinks );
			}
			catch (Exception ex)
			{
				return new OperateResult<MelsecFxLinksAddress>( "Address Create failed: " + ex.Message );
			}
		}
	}
}
