using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using HslCommunication.BasicFramework;
using HslCommunication.Core;
using HslCommunication.Core.Net;
using HslCommunication.Core.IMessage;
using HslCommunication.Reflection;
using System.Text.RegularExpressions;
#if !NET35 && !NET20
using System.Threading.Tasks;
#endif

namespace HslCommunication.Profinet.AllenBradley
{
	/// <summary>
	/// <b>[商业授权]</b> AB PLC的虚拟服务器，仅支持和HSL组件的完美通信，可以手动添加一些节点。<br />
	/// <b>[Authorization]</b> AB PLC's virtual server only supports perfect communication with HSL components. You can manually add some nodes.
	/// </summary>
	/// <remarks>
	/// 本AB的虚拟PLC仅限商业授权用户使用，感谢支持。
	/// </remarks>
	public class AllenBradleyServer : NetworkDataServerBase
	{
		#region Constructor

		/// <summary>
		/// 实例化一个AB PLC协议的服务器<br />
		/// Instantiate an AB PLC protocol server
		/// </summary>
		public AllenBradleyServer( )
		{
			WordLength    = 2;                                                          // 每个地址占1个字节的数据
			ByteTransform = new RegularByteTransform( );                                // 解析数据的类型
			Port          = 44818;                                                      // 端口的默认值
			simpleHybird  = new SimpleHybirdLock( );                                    // 词典锁
			abValues      = new Dictionary<string, AllenBradleyItemValue>( );           // 数据
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// 获取或设置当前的服务器的数据字节排序情况<br />
		/// Gets or sets the data byte ordering of the current server
		/// </summary>
		public DataFormat DataFormat
		{
			get => ByteTransform.DataFormat;
			set => ByteTransform.DataFormat = value;
		}

		/// <summary>
		/// 获取或设置当调用写入方法的时候，如果标签不存在，是否创建新的标签信息<br />
		/// Gets or sets whether to create a new tag information if the tag does not exist when the write method is called
		/// </summary>
		public bool CreateTagWithWrite
		{
			get => this.createTagWithWrite;
			set => this.createTagWithWrite = value;
		}

		#endregion

		#region Add Tag

		/// <summary>
		/// 向服务器新增一个新的Tag值<br />
		/// Add a new tag value to the server
		/// </summary>
		/// <param name="key">Tag名称</param>
		/// <param name="value">标签值</param>
		public void AddTagValue( string key, AllenBradleyItemValue value )
		{
			simpleHybird.Enter( );

			if (abValues.ContainsKey( key ))
				abValues[key] = value;
			else
				abValues.Add( key, value );

			simpleHybird.Leave( );
		}

		/// <summary>
		/// 向服务器新增一个新的bool类型的Tag值，并赋予初始化的值<br />
		/// Add a new tag value of type bool to the server and assign the initial value
		/// </summary>
		/// <param name="key">Tag名称</param>
		/// <param name="value">值信息</param>
		public void AddTagValue( string key, bool value )
		{
			AddTagValue( key, new AllenBradleyItemValue( )
			{
				IsArray    = false,
				Buffer     = value ? new byte[2] { 0xFF, 0xFF } : new byte[2] { 0x00, 0x00 },
				TypeLength = 2,
				TypeCode   = AllenBradleyHelper.CIP_Type_Bool
			} );
		}

		/// <summary>
		/// 向服务器新增一个新的bool数组类型的Tag值，并赋予初始化的值<br />
		/// Add a new tag value of type bool array to the server and assign the initial value
		/// </summary>
		/// <param name="key">Tag名称</param>
		/// <param name="value">值信息</param>
		public void AddTagValue( string key, bool[] value )
		{
			if (value == null) value = new bool[0];

			byte[] buffer = new byte[value.Length];
			for (int i = 0; i < value.Length; i++)
			{
				buffer[i] = (byte)(value[i] ? 1 : 0);
			}

			AddTagValue( key, new AllenBradleyItemValue( )
			{
				IsArray    = true,
				Buffer     = buffer,
				TypeLength = 1,
				TypeCode   = AllenBradleyHelper.CIP_Type_Bool
			} );
		}

		/// <summary>
		/// 向服务器新增一个新的type类型的Tag值，并赋予初始化的值<br />
		/// Add a new type tag value to the server and assign the initial value
		/// </summary>
		/// <param name="key">Tag名称</param>
		/// <param name="value">值信息</param>
		public void AddTagValue( string key, byte value )
		{
			AddTagValue( key, new AllenBradleyItemValue( )
			{
				IsArray = false,
				Buffer = new byte[] { value },
				TypeLength = 1,
				TypeCode = AllenBradleyHelper.CIP_Type_Byte,
			} );
		}

		/// <summary>
		/// 向服务器新增一个新的short类型的Tag值，并赋予初始化的值<br />
		/// Add a new short tag value to the server and assign the initial value
		/// </summary>
		/// <param name="key">Tag名称</param>
		/// <param name="value">值信息</param>
		public void AddTagValue( string key, short value )
		{
			AddTagValue( key, new AllenBradleyItemValue( )
			{
				IsArray    = false,
				Buffer     = ByteTransform.TransByte( value ),
				TypeLength = 2,
				TypeCode   = AllenBradleyHelper.CIP_Type_Word
			} );
		}

		/// <summary>
		/// 向服务器新增一个新的short数组的Tag值，并赋予初始化的值<br />
		/// Add a new short array Tag value to the server and assign the initial value
		/// </summary>
		/// <param name="key">Tag名称</param>
		/// <param name="value">值信息</param>
		public void AddTagValue( string key, short[] value )
		{
			AddTagValue( key, new AllenBradleyItemValue( )
			{
				IsArray    = true,
				Buffer     = ByteTransform.TransByte( value ),
				TypeLength = 2,
				TypeCode   = AllenBradleyHelper.CIP_Type_Word
			} );
		}

		/// <summary>
		/// 向服务器新增一个新的ushort类型的Tag值，并赋予初始化的值<br />
		/// Add a new tag value of ushort type to the server and assign the initial value
		/// </summary>
		/// <param name="key">Tag名称</param>
		/// <param name="value">值信息</param>
		public void AddTagValue( string key, ushort value )
		{
			AddTagValue( key, new AllenBradleyItemValue( )
			{
				IsArray    = false,
				Buffer     = ByteTransform.TransByte( value ),
				TypeLength = 2,
				TypeCode   = AllenBradleyHelper.CIP_Type_UInt
			} );
		}

		/// <summary>
		/// 向服务器新增一个新的ushort数组的Tag值，并赋予初始化的值<br />
		/// Add a new ushort array Tag value to the server and assign the initial value
		/// </summary>
		/// <param name="key">Tag名称</param>
		/// <param name="value">值信息</param>
		public void AddTagValue( string key, ushort[] value )
		{
			AddTagValue( key, new AllenBradleyItemValue( )
			{
				IsArray    = true,
				Buffer     = ByteTransform.TransByte( value ),
				TypeLength = 2,
				TypeCode   = AllenBradleyHelper.CIP_Type_UInt
			} );
		}

		/// <summary>
		/// 向服务器新增一个新的int类型的Tag值，并赋予初始化的值<br />
		/// Add a new Tag value of type int to the server and assign the initialized value
		/// </summary>
		/// <param name="key">Tag名称</param>
		/// <param name="value">值信息</param>
		public void AddTagValue( string key, int value )
		{
			AddTagValue( key, new AllenBradleyItemValue( )
			{
				IsArray    = false,
				Buffer     = ByteTransform.TransByte( value ),
				TypeLength = 4,
				TypeCode   = AllenBradleyHelper.CIP_Type_DWord
			} );
		}

		/// <summary>
		/// 向服务器新增一个新的int数组的Tag值，并赋予初始化的值<br />
		/// Add a new Tag value of the int array to the server and assign the initialized value
		/// </summary>
		/// <param name="key">Tag名称</param>
		/// <param name="value">值信息</param>
		public void AddTagValue( string key, int[] value )
		{
			AddTagValue( key, new AllenBradleyItemValue( )
			{
				IsArray    = true,
				Buffer     = ByteTransform.TransByte( value ),
				TypeLength = 4,
				TypeCode   = AllenBradleyHelper.CIP_Type_DWord
			} );
		}

		/// <summary>
		/// 向服务器新增一个新的uint类型的Tag值，并赋予初始化的值<br />
		/// Add a new uint tag value to the server and assign the initial value
		/// </summary>
		/// <param name="key">Tag名称</param>
		/// <param name="value">值信息</param>
		public void AddTagValue( string key, uint value )
		{
			AddTagValue( key, new AllenBradleyItemValue( )
			{
				IsArray    = false,
				Buffer     = ByteTransform.TransByte( value ),
				TypeLength = 4,
				TypeCode   = AllenBradleyHelper.CIP_Type_UDint
			} );
		}

		/// <summary>
		/// 向服务器新增一个新的uint数组的Tag值，并赋予初始化的值<br />
		/// Add a new uint array Tag value to the server and assign the initial value
		/// </summary>
		/// <param name="key">Tag名称</param>
		/// <param name="value">值信息</param>
		public void AddTagValue( string key, uint[] value )
		{
			AddTagValue( key, new AllenBradleyItemValue( )
			{
				IsArray    = true,
				Buffer     = ByteTransform.TransByte( value ),
				TypeLength = 4,
				TypeCode   = AllenBradleyHelper.CIP_Type_UDint
			} );
		}

		/// <summary>
		/// 向服务器新增一个新的long类型的Tag值，并赋予初始化的值<br />
		/// Add a new Tag value of type long to the server and assign the initialized value
		/// </summary>
		/// <param name="key">Tag名称</param>
		/// <param name="value">值信息</param>
		public void AddTagValue( string key, long value )
		{
			AddTagValue( key, new AllenBradleyItemValue( )
			{
				IsArray    = false,
				Buffer     = ByteTransform.TransByte( value ),
				TypeLength = 8,
				TypeCode   = AllenBradleyHelper.CIP_Type_LInt
			} );
		}

		/// <summary>
		/// 向服务器新增一个新的long数组的Tag值，并赋予初始化的值<br />
		/// Add a new Long array Tag value to the server and assign the initial value
		/// </summary>
		/// <param name="key">Tag名称</param>
		/// <param name="value">值信息</param>
		public void AddTagValue( string key, long[] value )
		{
			AddTagValue( key, new AllenBradleyItemValue( )
			{
				IsArray    = true,
				Buffer     = ByteTransform.TransByte( value ),
				TypeLength = 8,
				TypeCode   = AllenBradleyHelper.CIP_Type_LInt
			} );
		}

		/// <summary>
		/// 向服务器新增一个新的ulong类型的Tag值，并赋予初始化的值<br />
		/// Add a new Ulong type Tag value to the server and assign the initial value
		/// </summary>
		/// <param name="key">Tag名称</param>
		/// <param name="value">值信息</param>
		public void AddTagValue( string key, ulong value )
		{
			AddTagValue( key, new AllenBradleyItemValue( )
			{
				IsArray    = false,
				Buffer     = ByteTransform.TransByte( value ),
				TypeLength = 8,
				TypeCode   = AllenBradleyHelper.CIP_Type_ULint
			} );
		}

		/// <summary>
		/// 向服务器新增一个新的ulong数组的Tag值，并赋予初始化的值<br />
		/// Add a new Ulong array Tag value to the server and assign the initial value
		/// </summary>
		/// <param name="key">Tag名称</param>
		/// <param name="value">值信息</param>
		public void AddTagValue( string key, ulong[] value )
		{
			AddTagValue( key, new AllenBradleyItemValue( )
			{
				IsArray    = true,
				Buffer     = ByteTransform.TransByte( value ),
				TypeLength = 8,
				TypeCode   = AllenBradleyHelper.CIP_Type_ULint
			} );
		}

		/// <summary>
		/// 向服务器新增一个新的float类型的Tag值，并赋予初始化的值<br />
		/// Add a new tag value of type float to the server and assign the initial value
		/// </summary>
		/// <param name="key">Tag名称</param>
		/// <param name="value">值信息</param>
		public void AddTagValue( string key, float value )
		{
			AddTagValue( key, new AllenBradleyItemValue( )
			{
				IsArray    = false,
				Buffer     = ByteTransform.TransByte( value ),
				TypeLength = 4,
				TypeCode   = AllenBradleyHelper.CIP_Type_Real
			} );
		}

		/// <summary>
		/// 向服务器新增一个新的float数组的Tag值，并赋予初始化的值<br />
		/// Add a new Tag value of the float array to the server and assign the initialized value
		/// </summary>
		/// <param name="key">Tag名称</param>
		/// <param name="value">值信息</param>
		public void AddTagValue( string key, float[] value )
		{
			AddTagValue( key, new AllenBradleyItemValue( )
			{
				IsArray    = true,
				Buffer     = ByteTransform.TransByte( value ),
				TypeLength = 4,
				TypeCode   = AllenBradleyHelper.CIP_Type_Real
			} );
		}

		/// <summary>
		/// 向服务器新增一个新的double类型的Tag值，并赋予初始化的值<br />
		/// Add a new tag value of type double to the server and assign the initialized value
		/// </summary>
		/// <param name="key">Tag名称</param>
		/// <param name="value">值信息</param>
		public void AddTagValue( string key, double value )
		{
			AddTagValue( key, new AllenBradleyItemValue( )
			{
				IsArray    = false,
				Buffer     = ByteTransform.TransByte( value ),
				TypeLength = 8,
				TypeCode   = AllenBradleyHelper.CIP_Type_Double
			} );
		}

		/// <summary>
		/// 向服务器新增一个新的double数组的Tag值，并赋予初始化的值<br />
		/// Add a new double array Tag value to the server and assign the initialized value
		/// </summary>
		/// <param name="key">Tag名称</param>
		/// <param name="value">值信息</param>
		public void AddTagValue( string key, double[] value )
		{
			AddTagValue( key, new AllenBradleyItemValue( )
			{
				IsArray    = true,
				Buffer     = ByteTransform.TransByte( value ),
				TypeLength = 8,
				TypeCode   = AllenBradleyHelper.CIP_Type_Double
			} );
		}

		/// <summary>
		/// 向服务器新增一个新的string类型的Tag值，并赋予初始化的值<br />
		/// Add a new Tag value of string type to the server and assign the initial value
		/// </summary>
		/// <param name="key">Tag名称</param>
		/// <param name="value">值信息</param>
		/// <param name="maxLength">字符串的最长值</param>
		public void AddTagValue( string key, string value, int maxLength )
		{
			byte[] strBuffer = Encoding.UTF8.GetBytes( value );
			AddTagValue( key, new AllenBradleyItemValue( )
			{
				IsArray    = false,
				Buffer     = SoftBasic.ArrayExpandToLength( SoftBasic.SpliceArray( new byte[2], BitConverter.GetBytes( strBuffer.Length ), Encoding.UTF8.GetBytes( value ) ), maxLength ),
				TypeLength = maxLength,
				TypeCode   = AllenBradleyHelper.CIP_Type_D1
			} );
		}

		/// <summary>
		/// 向服务器新增一个新的string数组的Tag值，并赋予初始化的值<br />
		/// Add a new String array Tag value to the server and assign the initialized value
		/// </summary>
		/// <param name="key">Tag名称</param>
		/// <param name="value">值信息</param>
		/// <param name="maxLength">字符串的最长值</param>
		public void AddTagValue( string key, string[] value, int maxLength )
		{
			byte[] buffer = new byte[maxLength * value.Length];
			for (int i = 0; i < value.Length; i++)
			{
				byte[] strBuffer = Encoding.UTF8.GetBytes( value[i] );
				BitConverter.GetBytes( strBuffer.Length ).CopyTo( buffer, maxLength * i + 2 );
				strBuffer.CopyTo( buffer, maxLength * i + 6 );
			}
			AddTagValue( key, new AllenBradleyItemValue( )
			{
				IsArray    = true,
				Buffer     = buffer,
				TypeLength = maxLength,
				TypeCode   = AllenBradleyHelper.CIP_Type_D1
			} );
		}

		#endregion

		#region NetworkDataServerBase Override

		/// <inheritdoc/>
		[HslMqttApi( "ReadByteArray", "" )]
		public override OperateResult<byte[]> Read( string address, ushort length )
		{
			return ReadWithType( address, length ).Then( ( m, n ) => OperateResult.CreateSuccessResult( m ) );
		}

		private int GetAddressIndex( ref string address )
		{
			Match match = Regex.Match( address, @"\[[0-9]+\]$" );
			if ( match.Success)
			{
				address = address.Substring( 0, match.Index );
				return Convert.ToInt32( match.Value.Substring( 1, match.Length - 2 ) );
			}
			return 0;
		}

		private OperateResult<byte[], ushort> ReadWithType( string address, ushort length )
		{
			int index = GetAddressIndex( ref address );
			ushort type = 0;

			byte[] ret = null;
			simpleHybird.Enter( );
			if (abValues.ContainsKey( address ))
			{
				AllenBradleyItemValue abValue = abValues[address];
				type = abValue.TypeCode;
				if (!abValue.IsArray)
				{
					ret = new byte[abValue.Buffer.Length];
					abValue.Buffer.CopyTo( ret, 0 );
				}
				else
				{
					if ((index * abValue.TypeLength + length * abValue.TypeLength) <= abValue.Buffer.Length)
					{
						ret = new byte[length * abValue.TypeLength];
						Array.Copy( abValue.Buffer, index * abValue.TypeLength, ret, 0, ret.Length );
					}
				}
			}
			simpleHybird.Leave( );
			if (ret == null) return new OperateResult<byte[], ushort>( StringResources.Language.AllenBradley04 );

			return OperateResult.CreateSuccessResult( ret, type );
		}

		/// <inheritdoc/>
		[HslMqttApi( "WriteByteArray", "" )]
		public override OperateResult Write( string address, byte[] value )
		{
			int index = GetAddressIndex( ref address );

			bool isWrite = false;
			simpleHybird.Enter( );
			if (abValues.ContainsKey( address ))
			{
				AllenBradleyItemValue abValue = abValues[address];
				if (!abValue.IsArray)
				{
					if (abValue.Buffer.Length == value.Length)
					{
						abValue.Buffer = value;
						isWrite = true;
					}
				}
				else
				{
					if ((index * abValue.TypeLength + value.Length) <= abValue.Buffer.Length)
					{
						Array.Copy( value, 0, abValue.Buffer, index * abValue.TypeLength, value.Length );
						isWrite = true;
					}
				}
			}
			simpleHybird.Leave( );
			if (!isWrite) return new OperateResult( StringResources.Language.AllenBradley04 );
			return OperateResult.CreateSuccessResult( );
		}

		#endregion

		#region Byte Read Write

		/// <inheritdoc cref="AllenBradleyNet.ReadByte(string)"/>
		[HslMqttApi( "ReadByte", "" )]
		public OperateResult<byte> ReadByte( string address ) => ByteTransformHelper.GetResultFromArray( Read( address, 1 ) );

		/// <inheritdoc cref="AllenBradleyNet.Write(string, byte)"/>
		[HslMqttApi( "WriteByte", "" )]
		public OperateResult Write( string address, byte value ) => Write( address, new byte[] { value } );

		#endregion

		#region Bool Read Write Operate

		/// <inheritdoc/>
		[HslMqttApi( "ReadBoolArray", "" )]
		public override OperateResult<bool[]> ReadBool( string address, ushort length )
		{
			int index = GetAddressIndex( ref address );

			bool isExist = false;
			bool[] ret = null;
			simpleHybird.Enter( );
			if (abValues.ContainsKey( address ))
			{
				isExist = true;
				AllenBradleyItemValue abValue = abValues[address];
				if (!abValue.IsArray)
				{
					if (abValue.TypeCode == AllenBradleyHelper.CIP_Type_Bool)
					{
						if (abValue.Buffer[0] == 0xff && abValue.Buffer[1] == 0xff)
							ret = new bool[1] { true };
						else
							ret = new bool[1] { false };
					}
					else
					{
						ret = abValue.Buffer.ToBoolArray( );
					}
				}
				else
				{
					if ((index * abValue.TypeLength + length * abValue.TypeLength) <= abValue.Buffer.Length)
					{
						ret = new bool[length * abValue.TypeLength];
						for (int i = 0; i < ret.Length; i++)
						{
							ret[i] = abValue.Buffer[index * abValue.TypeLength + i] != 0x00;
						}
					}
				}
			}
			simpleHybird.Leave( );
			if (isExist == false) return new OperateResult<bool[]>( StringResources.Language.AllenBradley04 );

			return OperateResult.CreateSuccessResult( ret );
		}

		/// <inheritdoc/>
		[HslMqttApi( "WriteBoolArray", "" )]
		public override OperateResult Write( string address, bool[] value )
		{
			if (IsNeedCreateTag( address ))
			{
				AddTagValue( address, value );
				return OperateResult.CreateSuccessResult( );
			}
			else
			{
				int index = GetAddressIndex( ref address );
				bool isExist = false;

				simpleHybird.Enter( );
				if (abValues.ContainsKey( address ))
				{
					isExist = true;
					AllenBradleyItemValue abValue = abValues[address];
					if (!abValue.IsArray)
					{
						// 单个的变量数据
						if (value[0])
						{
							if (abValue.Buffer?.Length > 0) abValue.Buffer[0] = 0xFF;
							if (abValue.Buffer?.Length > 1) abValue.Buffer[1] = 0xFF;
						}
						else
						{
							if (abValue.Buffer?.Length > 0) abValue.Buffer[0] = 0x00;
							if (abValue.Buffer?.Length > 1) abValue.Buffer[1] = 0x00;
						}
					}
					else
					{
						// bool数组
						if ((index * abValue.TypeLength + value.Length * abValue.TypeLength) <= abValue.Buffer.Length)
						{
							for (int i = 0; i < value.Length; i++)
							{
								abValue.Buffer[index * abValue.TypeLength + i] = value[i] ? (byte)0x01 : (byte)0x00;
							}
						}
					}
				}
				simpleHybird.Leave( );
				if (isExist == false) return new OperateResult( StringResources.Language.AllenBradley04 );

				return OperateResult.CreateSuccessResult( );
			}
		}

		/// <inheritdoc/>
		public override OperateResult<string> ReadString( string address, ushort length, Encoding encoding )
		{
			OperateResult<byte[]> read = Read( address, length );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<string>( read );

			if (read.Content.Length >= 6)
			{
				int strLength = BitConverter.ToInt32( read.Content, 2 );
				return OperateResult.CreateSuccessResult( encoding.GetString( read.Content, 6, strLength ) );
			}
			else
			{
				return OperateResult.CreateSuccessResult( encoding.GetString( read.Content ) );
			}
		}

		/// <inheritdoc/>
		public override OperateResult Write( string address, string value, Encoding encoding )
		{
			bool isExist = false;
			int index = GetAddressIndex( ref address );

			simpleHybird.Enter( );
			if (abValues.ContainsKey( address ))
			{
				isExist = true;
				AllenBradleyItemValue abValue = abValues[address];
				if (abValue.Buffer?.Length >= 6)
				{
					byte[] buffer = encoding.GetBytes( value );
					BitConverter.GetBytes( buffer.Length ).CopyTo( abValue.Buffer, 2 + index * abValue.TypeLength );
					if (buffer.Length > 0) Array.Copy( buffer, 0, abValue.Buffer, 6 + index * abValue.TypeLength, Math.Min( buffer.Length, abValue.Buffer.Length - 6 ) );
				}
			}
			simpleHybird.Leave( );
			if (isExist == false) return new OperateResult<bool>( StringResources.Language.AllenBradley04 );

			return OperateResult.CreateSuccessResult( );
		}

		#endregion

		#region NetServer Override

		/// <inheritdoc/>
		protected override INetMessage GetNewNetMessage( ) => new AllenBradleyMessage( );

		/// <inheritdoc/>
		protected override void ThreadPoolLoginAfterClientCheck( Socket socket, System.Net.IPEndPoint endPoint )
		{
			// 开始接收数据信息
			OperateResult<byte[]> read1 = ReceiveByMessage( socket, 5000, new AllenBradleyMessage( ) );
			if (!read1.IsSuccess) return;

			// 构建随机的SessionID
			byte[] sessionId = new byte[4];
			HslHelper.HslRandom.NextBytes( sessionId );

			OperateResult send1 = Send( socket, AllenBradleyHelper.PackRequestHeader( 0x65, this.ByteTransform.TransUInt32(sessionId, 0), new byte[0] ) );
			if (!send1.IsSuccess) return;

			AppSession appSession = new AppSession( socket );
			appSession.LoginAlias = this.ByteTransform.TransUInt32( sessionId, 0 ).ToString( );
			try
			{
				socket.BeginReceive( new byte[0], 0, 0, SocketFlags.None, new AsyncCallback( SocketAsyncCallBack ), appSession );
				AddClient( appSession );
			}
			catch
			{
				socket.Close( );
				LogNet?.WriteDebug( ToString( ), string.Format( StringResources.Language.ClientOfflineInfo, endPoint ) );
			}
		}

		/// <inheritdoc/>
		protected override OperateResult<byte[]> ReadFromCoreServer( AppSession session, byte[] receive )
		{
			// 校验SessionID
			string sessionID = this.ByteTransform.TransUInt32( receive, 4 ).ToString( );
			if (sessionID != session.LoginAlias)
			{
				LogNet?.WriteDebug( ToString( ), $"SessionID 不一致的请求，要求ID：{session.LoginAlias} 实际ID：{sessionID}" );
				return OperateResult.CreateSuccessResult( AllenBradleyHelper.PackRequestHeader( 0x66, 0x64, this.ByteTransform.TransUInt32( receive, 4 ), new byte[0] ) );
			}
			byte[] back = ReadFromCipCore( session, receive );
			Array.Copy( receive, 4, back, 4, 4 ); // 修复会话ID
			return OperateResult.CreateSuccessResult( back );
		}

		/// <summary>
		/// 当收到mc协议的报文的时候应该触发的方法，允许继承重写，来实现自定义的返回，或是数据监听。
		/// </summary>
		/// <param name="session">当前客户端的会话信息</param>
		/// <param name="cipAll">CIP报文数据</param>
		/// <returns>返回的报文信息</returns>
		protected virtual byte[] ReadFromCipCore( AppSession session, byte[] cipAll )
		{
			if (BitConverter.ToInt16( cipAll, 2 ) == 0x66)
			{
				// 关闭连接。
				return AllenBradleyHelper.PackCommandResponse( new byte[0], true );
			}

			byte[] specificData = SoftBasic.ArrayRemoveBegin( cipAll, 24 );
			if ( specificData.Length == 22)
			{
				// 读取Attributes All
				return AllenBradleyHelper.PackRequestHeader( 0x66, 0x10, AllenBradleyHelper.PackCommandSpecificData( new byte[] { 0x00, 0x00, 0x00, 0x00 }, AllenBradleyHelper.PackCommandSingleService( "810000002f000c006706020630005be104010a4e4a3530312d31353030".ToHexBytes( ), 0xB2 ) ) );
			}
			if ( specificData[12] == 0xb2 && specificData[16] == 0x5b)
			{
				session.Tag = "ConnectedCIP";
				// Large Forward Open
				return AllenBradleyHelper.PackRequestHeader( 0x66, 0x10, AllenBradleyHelper.PackCommandSpecificData( new byte[] { 0x00, 0x00, 0x00, 0x00 }, AllenBradleyHelper.PackCommandSingleService( "db000000c109415f0100fe8002001b0558bcbf0280841e0080841e000000".ToHexBytes( ), 0xB2 ) ) );
			}

			if (specificData[26] == 0x0A && specificData[27] == 0x02 && specificData[28] == 0x20 && specificData[29] == 0x02 && specificData[30] == 0x24 && specificData[31] == 0x01)
			{
				// 多项的读取的情况暂时进行屏蔽
				return null;
			}

			if (session.Tag is string connectedCip)
			{
				// 如果是基于连接的协议的情况
				byte[] cipCore = ByteTransform.TransByte( specificData, 22, BitConverter.ToInt16( specificData, 18 ) - 2 );
				if (cipCore[0] == AllenBradleyHelper.CIP_READ_DATA)
				{
					// 读数据
					return AllenBradleyHelper.PackRequestHeader( 0x70, 0x10, AllenBradleyHelper.PackCommandSpecificData( new byte[] { 0xA1, 0x00, 0x04, 0x00, 0x41, 0x01, 0x19, 0x07 }, 
						AllenBradleyHelper.PackCommandSingleService( ReadByCommand( cipCore ), 0xB1, true ) ) );
				}
				else if (cipCore[0] == AllenBradleyHelper.CIP_WRITE_DATA)
				{
					// 写数据
					return AllenBradleyHelper.PackRequestHeader( 0x66, 0x10, AllenBradleyHelper.PackCommandSpecificData( new byte[] { 0xA1, 0x00, 0x04, 0x00, 0x41, 0x01, 0x19, 0x07 }, 
						AllenBradleyHelper.PackCommandSingleService( WriteByMessage( cipCore ), 0xB1, true ) ) );
				}
				else
				{
					return null;
				}
			}
			else
			{
				byte[] cipCore = ByteTransform.TransByte( specificData, 26, BitConverter.ToInt16( specificData, 24 ) );
				if (cipCore[0] == AllenBradleyHelper.CIP_READ_DATA || cipCore[0] == AllenBradleyHelper.CIP_READ_FRAGMENT)
				{
					// 读数据
					return AllenBradleyHelper.PackRequestHeader( 0x66, 0x10, AllenBradleyHelper.PackCommandSpecificData( new byte[] { 0x00, 0x00, 0x00, 0x00 }, AllenBradleyHelper.PackCommandSingleService( ReadByCommand( cipCore ) ) ) );
				}
				else if (cipCore[0] == AllenBradleyHelper.CIP_WRITE_DATA)
				{
					// 写数据
					return AllenBradleyHelper.PackRequestHeader( 0x66, 0x10, AllenBradleyHelper.PackCommandSpecificData( new byte[] { 0x00, 0x00, 0x00, 0x00 }, AllenBradleyHelper.PackCommandSingleService( WriteByMessage( cipCore ) ) ) );
				}
				else if (cipCore[0] == AllenBradleyHelper.CIP_READ_LIST)
				{
					// 读数据列表
					return AllenBradleyHelper.PackRequestHeader( 0x6F, 0x10, AllenBradleyHelper.PackCommandSpecificData( new byte[] { 0x00, 0x00, 0x00, 0x00 }, AllenBradleyHelper.PackCommandSingleService( ReadList( cipCore ) ) ) );
				}
				else
				{
					return null;
				}
			}
		}
		

		private byte[] ReadList( byte[] cipCore )
		{
			if (cipCore[1] == 0x0E && cipCore[2] == 0x91)
			{
				return SoftBasic.HexStringToBytes( @"
D5 00 00 00
fa 1b 00 00 02 00 42 41 c1 00 00 00 00 00 00 00
00 00 00 00 00 00 20 24 00 00 13 00 52 6f 75 74
69 6e 65 3a 4d 61 69 6e 52 6f 75 74 69 6e 65 6d
10 00 00 00 00 00 00 00 00 00 00 00 00 82 25 00
00 13 00 5f 5f 6c 30 31 44 38 34 31 38 46 32 46
38 31 46 31 46 30 c4 00 00 00 00 00 00 00 00 00
00 00 00 00 e9 2b 00 00 09 00 5f 5f 53 4c 34 39
31 30 39 c4 20 04 00 00 00 00 00 00 00 00 00 00
00 68 31 00 00 13 00 5f 5f 6c 30 31 44 38 35 46
41 34 32 46 38 31 43 32 35 33 c4 00 00 00 00 00
00 00 00 00 00 00 00 00 3d 38 00 00 13 00 5f 5f
6c 30 31 44 38 30 31 44 46 32 46 38 31 38 44 45
38 c4 00 00 00 00 00 00 00 00 00 00 00 00 00 24
59 00 00 0d 00 52 6f 75 74 69 6e 65 3a 54 49 4d
45 52 6d 10 00 00 00 00 00 00 00 00 00 00 00 00
4b 7c 00 00 13 00 5f 5f 6c 30 31 44 38 36 44 43
35 32 46 38 31 42 44 34 38 c4 00 00 00 00 00 00
00 00 00 00 00 00 00 97 8a 00 00 06 00 53 43 4c
5f 30 31 8a 8f 00 00 00 00 00 00 00 00 00 00 00
00 23 b7 00 00 02 00 41 42 c1 00 00 00 00 00 00
00 00 00 00 00 00 00 d5 bf 00 00 0d 00 52 6f 75
74 69 6e 65 3a 43 4f 55 4e 54 6d 10 00 00 00 00
00 00 00 00 00 00 00 00 1e da 00 00 0e 00 52 6f
75 74 69 6e 65 3a 61 6e 61 6c 6f 67 6d 10 00 00
00 00 00 00 00 00 00 00 00 00
" );
			}

			ushort start = BitConverter.ToUInt16( cipCore, 6 );
			if (start == 0)
			{
				return SoftBasic.HexStringToBytes( @"
d5 00 06 00
40 07 00 00 03 00 4f 55 54 c4 00 00 00 00 00 00
00 00 00 00 00 00 00 e7 0e 00 00 0d 00 54 69 6d
69 6e 67 5f 41 63 74 69 76 65 c1 00 00 00 00 00
00 00 00 00 00 00 00 00 d1 18 00 00 03 00 5a 58
43 c1 00 00 00 00 00 00 00 00 00 00 00 00 00 fa
1b 00 00 13 00 50 72 6f 67 72 61 6d 3a 4d 61 69
6e 50 72 6f 67 72 61 6d 68 10 00 00 00 00 00 00
00 00 00 00 00 00 3f 20 00 00 04 00 54 45 53 54
ce 8f 00 00 00 00 00 00 00 00 00 00 00 00 20 24
00 00 09 00 4d 61 70 3a 4c 6f 63 61 6c 69 10 00
00 00 00 00 00 00 00 00 00 00 00 82 25 00 00 12
00 43 78 6e 3a 46 75 73 65 64 3a 33 32 39 32 64
66 32 33 7e 10 00 00 00 00 00 00 00 00 00 00 00
00 eb 2a 00 00 02 00 49 4e c4 00 00 00 00 00 00
00 00 00 00 00 00 00 e9 2b 00 00 09 00 4c 6f 63
61 6c 3a 32 3a 43 23 82 00 00 00 00 00 00 00 00
00 00 00 00 68 31 00 00 09 00 4d 61 70 3a 4f 56
31 36 45 69 10 00 00 00 00 00 00 00 00 00 00 00
00 b4 35 00 00 06 00 54 49 4d 45 52 31 83 8f 00
00 00 00 00 00 00 00 00 00 00 00 3d 38 00 00 09
00 4c 6f 63 61 6c 3a 33 3a 4f d5 8a 00 00 00 00
00 00 00 00 00 00 00 00 2e 3f 00 00 08 00 49 6e
52 61 77 4d 61 78 c4 00 00 00 00 00 00 00 00 00
00 00 00 00 a5 41 00 00 0f 00 53 65 6c 65 63 74
6f 72 5f 53 77 69 74 63 68 c1 00 00 00 00 00 00
00 00 00 00 00 00 00 73 4a 00 00 07 00 49 4e 54
54 45 53 54 c4 20 01 00 00 00 00 00 00 00 00 00
00 00 ec 50 00 00 09 00 4c 6f 63 61 6c 3a 33 3a
43 24 86 00 00 00 00 00 00 00 00 00 00 00 00 24
59 00 00 08 00 4d 61 70 3a 49 56 33 32 69 10 00
00 00 00 00 00 00 00 00 00 00 00
" );
			}
			else if (start == 0x5925)
			{
				return SoftBasic.HexStringToBytes( @"
d5 00 00 00
36 71 00 00 07 00 49 6e 45 75 4d 61 78 c4 00 00
00 00 00 00 00 00 00 00 00 00 00 4b 7c 00 00 09
00 4c 6f 63 61 6c 3a 33 3a 49 fa 8e 00 00 00 00
00 00 00 00 00 00 00 00 c3 81 00 00 05 00 43 4f
55 4e 54 82 8f 00 00 00 00 00 00 00 00 00 00 00
00 97 8a 00 00 09 00 4c 6f 63 61 6c 3a 32 3a 49
20 89 00 00 00 00 00 00 00 00 00 00 00 00 b0 9b
00 00 08 00 70 65 69 66 61 6e 67 73 97 8d 00 00
00 00 00 00 00 00 00 00 00 00 0a b4 00 00 09 00
52 54 4f 5f 52 65 73 65 74 c1 00 00 00 00 00 00
00 00 00 00 00 00 00 a1 b5 00 00 07 00 49 6e 45
75 4d 69 6e c4 00 00 00 00 00 00 00 00 00 00 00
00 00 23 b7 00 00 0d 00 54 61 73 6b 3a 4d 61 69
6e 54 61 73 6b 70 10 00 00 00 00 00 00 00 00 00
00 00 00 d5 bf 00 00 08 00 4d 61 70 3a 4c 49 4e
4b 69 10 00 00 00 00 00 00 00 00 00 00 00 00 9b
c1 00 00 08 00 49 6e 52 61 77 4d 69 6e c4 00 00
00 00 00 00 00 00 00 00 00 00 00 26 c2 00 00 03
00 41 42 43 c1 00 00 00 00 00 00 00 00 00 00 00
00 00 1e da 00 00 1a 00 43 78 6e 3a 53 74 61 6e
64 61 72 64 49 6e 70 75 74 3a 32 34 66 36 36 38
30 36 7e 10 00 00 00 00 00 00 00 00 00 00 00 00
92 e7 00 00 0e 00 53 65 6c 65 63 74 6f 72 53 77
69 74 63 68 c1 00 00 00 00 00 00 00 00 00 00 00
00 00

" );
			}
			else
			{
				return null;
			}
		}

		private byte[] ReadByCommand( byte[] cipCore )
		{
			byte[] requestPath = ByteTransform.TransByte( cipCore, 2, cipCore[1] * 2 );
			string address = AllenBradleyHelper.ParseRequestPathCommand( requestPath );
			ushort length = BitConverter.ToUInt16( cipCore, 2 + requestPath.Length );

			OperateResult<byte[], ushort> read = ReadWithType( address, length );
			byte[] result = AllenBradleyHelper.PackCommandResponse( read.Content1, true );

			if(result.Length > 6) BitConverter.GetBytes( read.Content2 ).CopyTo( result, 4 );
			return result;
		}

		private byte[] WriteByMessage( byte[] cipCore )
		{
			// 先判断是否有写入的权利，没有的话，直接返回写入异常
			if (!this.EnableWrite) return AllenBradleyHelper.PackCommandResponse( null, false );

			byte[] requestPath = ByteTransform.TransByte( cipCore, 2, cipCore[1] * 2 );
			string address = AllenBradleyHelper.ParseRequestPathCommand( requestPath );

			if (address.EndsWith( ".LEN" )) return AllenBradleyHelper.PackCommandResponse( new byte[0], false );
			if (address.EndsWith( ".DATA[0]" ))
			{
				address = address.Replace( ".DATA[0]", "" );
				byte[] strValue = ByteTransform.TransByte( cipCore, 6 + requestPath.Length, cipCore.Length - 6 - requestPath.Length );
				if (Write( address, Encoding.ASCII.GetString( strValue ).TrimEnd( '\u0000' ) ).IsSuccess)
					return AllenBradleyHelper.PackCommandResponse( new byte[0], false );
				else
					return AllenBradleyHelper.PackCommandResponse( null, false );
			}

			ushort typeCode = BitConverter.ToUInt16( cipCore, 2 + requestPath.Length );
			ushort length   = BitConverter.ToUInt16( cipCore, 4 + requestPath.Length );
			byte[] value    = ByteTransform.TransByte( cipCore, 6 + requestPath.Length, cipCore.Length - 6 - requestPath.Length );

			if (typeCode == AllenBradleyHelper.CIP_Type_Bool && length == 1) // 写入单个的bool值
			{
				bool boolValue = false;
				if (value.Length == 2 && value[0] == 0x0ff && value[1] == 0xff) boolValue = true;
				if (Write( address, boolValue ).IsSuccess)
					return AllenBradleyHelper.PackCommandResponse( new byte[0], false );
				else
					return AllenBradleyHelper.PackCommandResponse( null, false );
			}
			else
			{
				if (Write( address, value ).IsSuccess)
					return AllenBradleyHelper.PackCommandResponse( new byte[0], false );
				else
					return AllenBradleyHelper.PackCommandResponse( null, false );
			}
		}

		#endregion

		#region IDisposable Support

		/// <inheritdoc/>
		protected override void Dispose( bool disposing )
		{
			if (disposing)
			{
				simpleHybird.Dispose( );
			}
			base.Dispose( disposing );
		}

		#endregion

		#region Read Override

		/// <inheritdoc/>
		[HslMqttApi( "ReadInt16Array", "" )]
		public override OperateResult<short[]> ReadInt16( string address, ushort length ) => ByteTransformHelper.GetResultFromBytes( Read( address, length ), m => ByteTransform.TransInt16( m, 0, length ) );

		/// <inheritdoc/>
		[HslMqttApi( "ReadUInt16Array", "" )]
		public override OperateResult<ushort[]> ReadUInt16( string address, ushort length ) => ByteTransformHelper.GetResultFromBytes( Read( address, length ), m => ByteTransform.TransUInt16( m, 0, length ) );

		/// <inheritdoc/>
		[HslMqttApi( "ReadInt32Array", "" )]
		public override OperateResult<int[]> ReadInt32( string address, ushort length ) => ByteTransformHelper.GetResultFromBytes( Read( address, length ), m => ByteTransform.TransInt32( m, 0, length ) );

		/// <inheritdoc/>
		[HslMqttApi( "ReadUInt32Array", "" )]
		public override OperateResult<uint[]> ReadUInt32( string address, ushort length ) => ByteTransformHelper.GetResultFromBytes( Read( address, length ), m => ByteTransform.TransUInt32( m, 0, length ) );

		/// <inheritdoc/>
		[HslMqttApi( "ReadInt64Array", "" )]
		public override OperateResult<long[]> ReadInt64( string address, ushort length ) => ByteTransformHelper.GetResultFromBytes( Read( address, length ), m => ByteTransform.TransInt64( m, 0, length ) );

		/// <inheritdoc/>
		[HslMqttApi( "ReadUInt64Array", "" )]
		public override OperateResult<ulong[]> ReadUInt64( string address, ushort length ) => ByteTransformHelper.GetResultFromBytes( Read( address, length ), m => ByteTransform.TransUInt64( m, 0, length ) );

		/// <inheritdoc/>
		[HslMqttApi( "ReadFloatArray", "" )]
		public override OperateResult<float[]> ReadFloat( string address, ushort length ) => ByteTransformHelper.GetResultFromBytes( Read( address, length ), m => ByteTransform.TransSingle( m, 0, length ) );

		/// <inheritdoc/>
		[HslMqttApi( "ReadDoubleArray", "" )]
		public override OperateResult<double[]> ReadDouble( string address, ushort length ) => ByteTransformHelper.GetResultFromBytes( Read( address, length ), m => ByteTransform.TransDouble( m, 0, length ) );
#if !NET35 && !NET20
		/// <inheritdoc/>
		public override async Task<OperateResult<short[]>> ReadInt16Async( string address, ushort length ) => ByteTransformHelper.GetResultFromBytes( await ReadAsync( address, length ), m => ByteTransform.TransInt16( m, 0, length ) );

		/// <inheritdoc/>
		public override async Task<OperateResult<ushort[]>> ReadUInt16Async( string address, ushort length ) => ByteTransformHelper.GetResultFromBytes( await ReadAsync( address, length ), m => ByteTransform.TransUInt16( m, 0, length ) );

		/// <inheritdoc/>
		public override async Task<OperateResult<int[]>> ReadInt32Async( string address, ushort length ) => ByteTransformHelper.GetResultFromBytes( await ReadAsync( address, length ), m => ByteTransform.TransInt32( m, 0, length ) );

		/// <inheritdoc/>
		public override async Task<OperateResult<uint[]>> ReadUInt32Async( string address, ushort length ) => ByteTransformHelper.GetResultFromBytes( await ReadAsync( address, length ), m => ByteTransform.TransUInt32( m, 0, length ) );

		/// <inheritdoc/>
		public override async Task<OperateResult<long[]>> ReadInt64Async( string address, ushort length ) => ByteTransformHelper.GetResultFromBytes( await ReadAsync( address, length ), m => ByteTransform.TransInt64( m, 0, length ) );

		/// <inheritdoc/>
		public override async Task<OperateResult<ulong[]>> ReadUInt64Async( string address, ushort length ) => ByteTransformHelper.GetResultFromBytes( await ReadAsync( address, length ), m => ByteTransform.TransUInt64( m, 0, length ) );

		/// <inheritdoc/>
		public override async Task<OperateResult<float[]>> ReadFloatAsync( string address, ushort length ) => ByteTransformHelper.GetResultFromBytes( await ReadAsync( address, length ), m => ByteTransform.TransSingle( m, 0, length ) );

		/// <inheritdoc/>
		public override async Task<OperateResult<double[]>> ReadDoubleAsync( string address, ushort length ) => ByteTransformHelper.GetResultFromBytes( await ReadAsync( address, length ), m => ByteTransform.TransDouble( m, 0, length ) );
#endif
		#endregion

		#region Write Override

		private bool IsNeedCreateTag( string address )
		{
			if (!CreateTagWithWrite) return false;
			if (Regex.IsMatch( address, @"\[[0-9]+\]$" )) return false;        // 索引结尾的一律不创建新的标签信息
			_ = GetAddressIndex( ref address );
			simpleHybird.Enter( );
			bool createTag = !abValues.ContainsKey( address );
			simpleHybird.Leave( );
			return createTag;
		}

		/// <inheritdoc/>
		[HslMqttApi( "WriteBool", "" )]
		public override OperateResult Write( string address, bool value )
		{
			if (IsNeedCreateTag( address ))
			{
				AddTagValue( address, value );
				return OperateResult.CreateSuccessResult( );
			}
			else
				return base.Write( address, value );
		}

		/// <inheritdoc/>
		[HslMqttApi( "WriteInt16", "" )]
		public override OperateResult Write( string address, short value )
		{
			if (IsNeedCreateTag( address ))
			{
				AddTagValue( address, value );
				return OperateResult.CreateSuccessResult( );
			}
			else
				return base.Write( address, value );
		}

		/// <inheritdoc/>
		[HslMqttApi( "WriteInt16Array", "" )]
		public override OperateResult Write( string address, short[] values )
		{
			if (IsNeedCreateTag( address ))
			{
				AddTagValue( address, values );
				return OperateResult.CreateSuccessResult( );
			}
			else return base.Write( address, values );
		}

		/// <inheritdoc/>
		[HslMqttApi( "WriteUInt16", "" )]
		public override OperateResult Write( string address, ushort value )
		{
			if (IsNeedCreateTag( address ))
			{
				AddTagValue( address, value );
				return OperateResult.CreateSuccessResult( );
			}
			else return base.Write( address, value );
		}

		/// <inheritdoc/>
		[HslMqttApi( "WriteUInt16Array", "" )]
		public override OperateResult Write( string address, ushort[] values )
		{
			if (IsNeedCreateTag( address ))
			{
				AddTagValue( address, values );
				return OperateResult.CreateSuccessResult( );
			}
			else return base.Write( address, values );
		}

		/// <inheritdoc/>
		[HslMqttApi( "WriteInt32", "" )]
		public override OperateResult Write( string address, int value )
		{
			if (IsNeedCreateTag( address ))
			{
				AddTagValue( address, value );
				return OperateResult.CreateSuccessResult( );
			}
			else return base.Write( address, value );
		}

		/// <inheritdoc/>
		[HslMqttApi( "WriteInt32Array", "" )]
		public override OperateResult Write( string address, int[] values )
		{
			if (IsNeedCreateTag( address ))
			{
				AddTagValue( address, values );
				return OperateResult.CreateSuccessResult( );
			}
			else return base.Write( address, values );
		}

		/// <inheritdoc/>
		[HslMqttApi( "WriteUInt32", "" )]
		public override OperateResult Write( string address, uint value )
		{
			if (IsNeedCreateTag( address ))
			{
				AddTagValue( address, value );
				return OperateResult.CreateSuccessResult( );
			}
			else return base.Write( address, value );
		}

		/// <inheritdoc/>
		[HslMqttApi( "WriteUInt32Array", "" )]
		public override OperateResult Write( string address, uint[] values )
		{
			if (IsNeedCreateTag( address ))
			{
				AddTagValue( address, values );
				return OperateResult.CreateSuccessResult( );
			}
			else return base.Write( address, values );
		}

		/// <inheritdoc/>
		[HslMqttApi( "WriteFloat", "" )]
		public override OperateResult Write( string address, float value )
		{
			if (IsNeedCreateTag( address ))
			{
				AddTagValue( address, value );
				return OperateResult.CreateSuccessResult( );
			}
			else return base.Write( address, value );
		}

		/// <inheritdoc/>
		[HslMqttApi( "WriteFloatArray", "" )]
		public override OperateResult Write( string address, float[] values )
		{
			if (IsNeedCreateTag( address ))
			{
				AddTagValue( address, values );
				return OperateResult.CreateSuccessResult( );
			}
			else return base.Write( address, values );
		}

		/// <inheritdoc/>
		[HslMqttApi( "WriteInt64", "" )]
		public override OperateResult Write( string address, long value )
		{
			if (IsNeedCreateTag( address ))
			{
				AddTagValue( address, value );
				return OperateResult.CreateSuccessResult( );
			}
			else return base.Write( address, value );
		}

		/// <inheritdoc/>
		[HslMqttApi( "WriteInt64Array", "" )]
		public override OperateResult Write( string address, long[] values )
		{
			if (IsNeedCreateTag( address ))
			{
				AddTagValue( address, values );
				return OperateResult.CreateSuccessResult( );
			}
			else return base.Write( address, values );
		}

		/// <inheritdoc/>
		[HslMqttApi( "WriteUInt64", "" )]
		public override OperateResult Write( string address, ulong value )
		{
			if (IsNeedCreateTag( address ))
			{
				AddTagValue( address, value );
				return OperateResult.CreateSuccessResult( );
			}
			else return base.Write( address, value );
		}

		/// <inheritdoc/>
		[HslMqttApi( "WriteUInt64Array", "" )]
		public override OperateResult Write( string address, ulong[] values )
		{
			if (IsNeedCreateTag( address ))
			{
				AddTagValue( address, values );
				return OperateResult.CreateSuccessResult( );
			}
			else return base.Write( address, values );
		}

		/// <inheritdoc/>
		[HslMqttApi( "WriteDouble", "" )]
		public override OperateResult Write( string address, double value )
		{
			if (IsNeedCreateTag( address ))
			{
				AddTagValue( address, value );
				return OperateResult.CreateSuccessResult( );
			}
			else return base.Write( address, value );
		}

		/// <inheritdoc/>
		[HslMqttApi( "WriteDoubleArray", "" )]
		public override OperateResult Write( string address, double[] values )
		{
			if (IsNeedCreateTag( address ))
			{
				AddTagValue( address, values );
				return OperateResult.CreateSuccessResult( );
			}
			else return base.Write( address, values );
		}
#if !NET35 && !NET20
		/// <inheritdoc/>
		public async override Task<OperateResult> WriteAsync( string address, bool value ) => await Task.Run( ( ) => Write( address, value ) );
		/// <inheritdoc/>
		public async override Task<OperateResult> WriteAsync( string address, short value ) => await Task.Run( ( ) => Write( address, value ) );
		/// <inheritdoc/>
		public async override Task<OperateResult> WriteAsync( string address, short[] values ) => await Task.Run( ( ) => Write( address, values ) );
		/// <inheritdoc/>
		public async override Task<OperateResult> WriteAsync( string address, ushort value ) => await Task.Run( ( ) => Write( address, value ) );
		/// <inheritdoc/>
		public async override Task<OperateResult> WriteAsync( string address, ushort[] values ) => await Task.Run( ( ) => Write( address, values ) );
		/// <inheritdoc/>
		public async override Task<OperateResult> WriteAsync( string address, int value ) => await Task.Run( ( ) => Write( address, value ) );
		/// <inheritdoc/>
		public async override Task<OperateResult> WriteAsync( string address, int[] values ) => await Task.Run( ( ) => Write( address, values ) );
		/// <inheritdoc/>
		public async override Task<OperateResult> WriteAsync( string address, uint value ) => await Task.Run( ( ) => Write( address, value ) );
		/// <inheritdoc/>
		public async override Task<OperateResult> WriteAsync( string address, uint[] values ) => await Task.Run( ( ) => Write( address, values ) );
		/// <inheritdoc/>
		public async override Task<OperateResult> WriteAsync( string address, float value ) => await Task.Run( ( ) => Write( address, value ) );
		/// <inheritdoc/>
		public async override Task<OperateResult> WriteAsync( string address, float[] values ) => await Task.Run( ( ) => Write( address, values ) );
		/// <inheritdoc/>
		public async override Task<OperateResult> WriteAsync( string address, long value ) => await Task.Run( ( ) => Write( address, value ) );
		/// <inheritdoc/>
		public async override Task<OperateResult> WriteAsync( string address, long[] values ) => await Task.Run( ( ) => Write( address, values ) );
		/// <inheritdoc/>
		public async override Task<OperateResult> WriteAsync( string address, ulong value ) => await Task.Run( ( ) => Write( address, value ) );
		/// <inheritdoc/>
		public async override Task<OperateResult> WriteAsync( string address, ulong[] values ) => await Task.Run( ( ) => Write( address, values ) );
		/// <inheritdoc/>
		public async override Task<OperateResult> WriteAsync( string address, double value ) => await Task.Run( ( ) => Write( address, value ) );
		/// <inheritdoc/>
		public async override Task<OperateResult> WriteAsync( string address, double[] values ) => await Task.Run( ( ) => Write( address, values ) );
		/// <inheritdoc/>
		public async override Task<OperateResult> WriteAsync( string address, bool[] value ) => await Task.Run( ( ) => Write( address, value ) );
#endif
		#endregion

		#region Private Member

		private const int DataPoolLength = 65536;                          // 数据的长度
		private Dictionary<string, AllenBradleyItemValue> abValues;        // 词典
		private SimpleHybirdLock simpleHybird;                             // 词典锁
		private bool createTagWithWrite = false;                           // 当服务器端写入数据时，是否进行创建节点，默认支持创建

		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString( ) => $"AllenBradleyServer[{Port}]";

		#endregion
	}
}
