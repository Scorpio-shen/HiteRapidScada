using HslCommunication.BasicFramework;
using HslCommunication.Core;
using HslCommunication.Core.IMessage;
using HslCommunication.Core.Net;
using System.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
#if !NET20 && !NET35
using System.Threading.Tasks;
#endif

namespace HslCommunication.Profinet.Omron
{
	/// <summary>
	/// 欧姆龙的HostLinkCMode协议的虚拟服务器
	/// </summary>
	public class OmronHostLinkCModeServer : OmronFinsServer
	{
		/// <inheritdoc cref="OmronFinsServer.OmronFinsServer()"/>
		public OmronHostLinkCModeServer( )
		{
			this.LogMsgFormatBinary                    = false;
			this.connectionInitialization              = false;
		}

		/// <inheritdoc cref="OmronHostLink.UnitNumber"/>
		public byte UnitNumber { get; set; }

		#region NetServer Override

		/// <inheritdoc/>
		protected override INetMessage GetNewNetMessage( ) => new SpecifiedCharacterMessage( AsciiControl.CR );

		/// <inheritdoc/>
		protected override OperateResult<byte[]> ReadFromCoreServer( AppSession session, byte[] receive )
		{
			if (receive.Length < 9) return new OperateResult<byte[]>( $"Uknown Data：{SoftBasic.GetAsciiStringRender( receive )}" );

			return OperateResult.CreateSuccessResult( ReadFromFinsCore( receive ) );
		}

		/// <inheritdoc/>
		protected override byte[] ReadFromFinsCore( byte[] finsCore )
		{
			string command = Encoding.ASCII.GetString( finsCore, 3, 2 );
			if (command == "RD" || command == "RR" || command == "RL" || command == "RH" || command == "RJ")
			{
				SoftBuffer softBuffer = null;
				switch (command)
				{
					case "RD": softBuffer = dBuffer; break;
					case "RR": softBuffer = cioBuffer; break;
					case "RL": softBuffer = cioBuffer; break;
					case "RH": softBuffer = hBuffer; break;
					case "RJ": softBuffer = arBuffer; break;
					case "RE": softBuffer = emBuffer; break;
					default: return PackCommand( 0x16, finsCore, null );
				}

				if (command == "RE")
				{
					int address   = Convert.ToInt32(  Encoding.ASCII.GetString( finsCore,  7, 4 ) );
					ushort length = Convert.ToUInt16( Encoding.ASCII.GetString( finsCore, 11, 4 ) );

					byte[] buffer = softBuffer.GetBytes( address * 2, length * 2 );
					return PackCommand( 0, finsCore, buffer );
				}
				else
				{
					int address   = Convert.ToInt32(  Encoding.ASCII.GetString( finsCore, 5, 4 ) );
					ushort length = Convert.ToUInt16( Encoding.ASCII.GetString( finsCore, 9, 4 ) );

					byte[] buffer = softBuffer.GetBytes( address * 2, length * 2 );
					return PackCommand( 0, finsCore, buffer );
				}
			}
			else if (command == "WD" || command == "WR" || command == "WL" || command == "WH" || command == "WJ" || command == "WE")
			{
				SoftBuffer softBuffer = null;
				switch (command)
				{
					case "WD": softBuffer = dBuffer; break;
					case "WR": softBuffer = cioBuffer; break;
					case "WL": softBuffer = cioBuffer; break;
					case "WH": softBuffer = hBuffer; break;
					case "WJ": softBuffer = arBuffer; break;
					case "WE": softBuffer = emBuffer; break;
					default: return PackCommand( 0x16, finsCore, null );
				}

				if (command == "WE")
				{
					int address = Convert.ToInt32( Encoding.ASCII.GetString( finsCore, 7, 4 ) );
					byte[] buffer = Encoding.ASCII.GetString( finsCore, 11, finsCore.Length - 11 ).ToHexBytes( );

					softBuffer.SetBytes( buffer, address * 2 );
					return PackCommand( 0, finsCore, null );
				}
				else
				{
					int address   = Convert.ToInt32(  Encoding.ASCII.GetString( finsCore, 5, 4 ) );
					byte[] buffer = Encoding.ASCII.GetString( finsCore, 9, finsCore.Length - 9 ).ToHexBytes( );

					softBuffer.SetBytes( buffer, address * 2 );
					return PackCommand( 0, finsCore, null );
				}
			}
			else if (command == "MM") // 读取型号信息
			{
				return PackCommand( 0, finsCore, new byte[] { 0x30 } );
			}
			else if (command == "MS") // 读取状态
			{
				return PackCommand( 0, finsCore, new byte[] { operationMode, 0x30 } );
			}
			else if (command == "SC") // 修改PLC的状态
			{
				byte mode = Convert.ToByte( Encoding.ASCII.GetString( finsCore, 5, 2 ), 16 );
				if (mode >= 0 && mode <= 2) operationMode = mode;
				return PackCommand( 0, finsCore, null );
			}
			else
			{
				return PackCommand( 0x16, finsCore, null );
			}
		}

		/// <inheritdoc/>
		protected override byte[] PackCommand( int status, byte[] finsCore, byte[] data )
		{
			if (data == null) data = new byte[0];
			data = SoftBasic.BytesToAsciiBytes( data );

			byte[] back = new byte[11 + data.Length];
			Encoding.ASCII.GetBytes( "@0000" ).CopyTo( back, 0 );
			Encoding.ASCII.GetBytes( UnitNumber.ToString( "X2" ) ).CopyTo( back, 1 );
			Array.Copy( finsCore, 3, back, 3, 2 );                                       // 命令也拷贝过去
			Encoding.ASCII.GetBytes( status.ToString( "X2" ) ).CopyTo( back, 5 );        // 状态的信息

			if (data.Length > 0) data.CopyTo( back, 7 );

			// 计算FCS
			int tmp = back[0];
			for (int i = 1; i < back.Length - 4; i++)
			{
				tmp ^= back[i];
			}
			SoftBasic.BuildAsciiBytesFrom( (byte)tmp ).CopyTo( back, back.Length - 4 );
			back[back.Length - 2] = (byte)'*';
			back[back.Length - 1] = 0x0D;
			return back;
		}
		#endregion


		#region Serial Support

		/// <inheritdoc/>
		protected override bool CheckSerialReceiveDataComplete( byte[] buffer, int receivedLength )
		{
			if (receivedLength > 1) return buffer[receivedLength - 1] == 0x0D;
			return false;
		}

		#endregion

		#region Private Member

		private byte operationMode = 1;                        // 操作模式

		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString( ) => $"OmronHostLinkCModeServer[{Port}]";

		#endregion
	}
}
