using HslCommunication.Secs.Helper;
using HslCommunication.Secs.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if !NET35 && !NET20
using System.Threading.Tasks;
#endif

namespace HslCommunication.Secs
{
	/// <summary>
	/// GEM相关的数据读写信息
	/// </summary>
	public class Gem
	{
		/// <summary>
		/// 使用指定的 <see cref="ISecs"/> 接口来初始化 GEM 对象，然后进行数据读写操作
		/// </summary>
		/// <param name="secs">Secs的通信对象</param>
		public Gem( ISecs secs )
		{
			this.secs = secs;
		}

		/// <summary>
		/// S1F1的功能方法
		/// </summary>
		/// <returns>在线数据信息</returns>
		public OperateResult<OnlineData> S1F1_AreYouThere( )
		{
			OperateResult<SecsMessage> read = this.secs.ReadSecsMessage( 1, 1, new SecsValue( ), true );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<OnlineData>( read );

			return OperateResult.CreateSuccessResult( (OnlineData)read.Content.GetItemValues( ) );
		}

		/// <summary>
		/// S1F11的功能方法
		/// </summary>
		/// <returns>变量名称数组</returns>
		public OperateResult<VariableName[]> S1F11_StatusVariableNamelist( )
		{
			OperateResult<SecsMessage> read = this.secs.ReadSecsMessage( 1, 11, new SecsValue( ), true );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<VariableName[]>( read );

			return OperateResult.CreateSuccessResult( read.Content.GetItemValues( ).ToVaruableNames( ) );
		}

		/// <summary>
		/// S1F11的功能方法，带参数传递
		/// </summary>
		/// <param name="statusVaruableId"></param>
		/// <returns></returns>
		public OperateResult<VariableName[]> S1F11_StatusVariableNamelist( params int[] statusVaruableId )
		{
			OperateResult<SecsMessage> read = this.secs.ReadSecsMessage( 1, 11, new SecsValue( statusVaruableId ), true );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<VariableName[]>( read );

			return OperateResult.CreateSuccessResult( read.Content.GetItemValues( ).ToVaruableNames( ) );
		}

		/// <summary>
		/// S1F13的功能方法，测试连接的
		/// </summary>
		/// <returns></returns>
		public OperateResult<OnlineData> S1F13_EstablishCommunications( )
		{
			OperateResult<SecsMessage> read = this.secs.ReadSecsMessage( 1, 13, new SecsValue( ), true );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<OnlineData>( read );

			SecsValue itemValue = read.Content.GetItemValues( );
			SecsValue[] itemValues = itemValue.Value as SecsValue[];

			byte[] result = (byte[])itemValues[0].Value;
			return result[0] == 0x00 ? OperateResult.CreateSuccessResult( (OnlineData)itemValues[1] ) : new OperateResult<OnlineData>( $"establish communications acknowledgement denied! source: {Environment.NewLine}{itemValue.ToXElement( )}" );
		}

		/// <summary>
		/// S1F15的功能方法
		/// </summary>
		/// <remarks>
		/// 返回值说明，0: ok, 1: refused, 2: already online
		/// </remarks>
		/// <returns>返回值说明，0: ok, 1: refused, 2: already online</returns>
		public OperateResult<byte> S1F15_OfflineRequest( )
		{
			OperateResult<SecsMessage> read = this.secs.ReadSecsMessage( 1, 15, new SecsValue( ), true );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte>( read );

			return OperateResult.CreateSuccessResult( ((byte[])read.Content.GetItemValues( ).Value)[0] );
		}

		/// <summary>
		/// S1F17的功能方法
		/// </summary>
		/// <remarks>
		/// 返回值说明，0: ok, 1: refused, 2: already online
		/// </remarks>
		/// <returns>返回值说明，0: ok, 1: refused, 2: already online</returns>
		public OperateResult<byte> S1F17_OnlineRequest( )
		{
			OperateResult<SecsMessage> read = this.secs.ReadSecsMessage( 1, 17, new SecsValue( ), true );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte>( read );

			return OperateResult.CreateSuccessResult( ((byte[])read.Content.GetItemValues( ).Value)[0] );
		}

		/// <summary>
		/// S2F13的功能方法
		/// </summary>
		/// <param name="list"></param>
		/// <returns></returns>
		public OperateResult<SecsValue> S2F13_EquipmentConstantRequest( object[] list = null )
		{
			OperateResult<SecsMessage> read = this.secs.ReadSecsMessage( 2, 13, new SecsValue( list ), true );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<SecsValue>( read );

			return OperateResult.CreateSuccessResult( read.Content.GetItemValues( ) );
		}

		private ISecs secs;
	}
}
