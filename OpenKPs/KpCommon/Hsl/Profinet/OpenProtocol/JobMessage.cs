using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HslCommunication.Profinet.OpenProtocol
{
	/// <summary>
	/// Job message
	/// </summary>
	public class JobMessage
	{
		/// <summary>
		/// 指定Open协议实例化一个任务消息对象
		/// </summary>
		/// <param name="openProtocol">连接通道</param>
		public JobMessage( OpenProtocolNet openProtocol )
		{
			this.openProtocol = openProtocol;
		}

		/// <summary>
		/// This is a request for a transmission of all the valid Job IDs of the controller. The result of this command is a transmission of all the valid Job IDs.
		/// </summary>
		/// <param name="revision">Revision</param>
		/// <returns>任务ID的列表信息</returns>
		public OperateResult<int[]> JobIDUpload( int revision = 1 )
		{
			OperateResult<string> read = this.openProtocol.ReadCustomer( 30, revision, -1, -1, null );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<int[]>( read );

			return PraseMID0031( read.Content );
		}

		/// <summary>
		/// Request to upload the data for a specific Job from the controller.
		/// </summary>
		/// <param name="id">job id</param>
		/// <returns>任务数据的结果对象</returns>
		public OperateResult<JobData> JobDataUpload( int id )
		{
			OperateResult<string> read = this.openProtocol.ReadCustomer( 32, 1, -1, -1, new List<string>( ) { id.ToString( "D2" ) } );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<JobData>( read );

			return PraseMID0033( read.Content );
		}

		/// <summary>
		/// A subscription for the Job info. MID 0035 Job info is sent to the integrator when a new Job is selected and after each tightening performed during the Job.
		/// </summary>
		/// <returns>是否成功的结果对象</returns>
		public OperateResult JobInfoSubscribe( ) => this.openProtocol.ReadCustomer( 34, 1, -1, -1, null );

		/// <summary>
		/// Reset the subscription for a Job info message.
		/// </summary>
		/// <returns>是否成功的结果对象</returns>
		public OperateResult JobInfoUnsubscribe( ) => this.openProtocol.ReadCustomer( 37, 1, -1, -1, null );

		/// <summary>
		/// Message to select Job. If the requested ID is not present in the controller, then the command will not be performed.
		/// </summary>
		/// <param name="id">Job ID</param>
		/// <param name="revision">Revision</param>
		/// <returns>是否成功的结果对象</returns>
		public OperateResult SelectJob( int id, int revision = 1 ) => this.openProtocol.ReadCustomer( 38, revision, -1, -1, new List<string>( ) { (revision == 1 ? id.ToString( "D2" ) : id.ToString( "D4" )) } );

		/// <summary>
		/// Job restart message.
		/// </summary>
		/// <param name="id">Job ID</param>
		/// <returns>是否成功的结果对象</returns>
		public OperateResult JobRestart( int id ) => this.openProtocol.ReadCustomer( 39, 1, -1, -1, new List<string>( ) { id.ToString( "D2" ) } );

		#region Private Method

		private OperateResult<int[]> PraseMID0031( string reply )
		{
			try
			{
				int revision = Convert.ToInt32( reply.Substring( 8, 3 ) );
				int everyLen = revision == 1 ? 2 : 4;

				int count = Convert.ToInt32( reply.Substring( 20, everyLen ) );
				int[] ints = new int[count];
				for (int i = 0; i < count; i++)
				{
					ints[i] = Convert.ToInt32( reply.Substring( 20 + everyLen + i * everyLen, everyLen ) );
				}
				return OperateResult.CreateSuccessResult( ints );
			}
			catch (Exception ex)
			{
				return new OperateResult<int[]>( "MID0031 prase failed: " + ex.Message + Environment.NewLine + "Source: " + reply );
			}
		}

		private OperateResult<JobData> PraseMID0033( string reply )
		{
			try
			{
				JobData job = new JobData( );
				job.JobID = Convert.ToInt32( reply.Substring( 22, 2 ) );
				job.JobName = reply.Substring( 26, 25 ).Trim( );
				job.ForcedOrder = Convert.ToInt32( reply.Substring( 53, 1 ) );
				job.MaxTimeForFirstTightening = Convert.ToInt32( reply.Substring( 56, 4 ) );
				job.MaxTimeToCompleteJob = Convert.ToInt32( reply.Substring( 62, 5 ) );
				job.JobBatchMode = Convert.ToInt32( reply.Substring( 69, 1 ) );
				job.LockAtJobDone = reply[72] == '1';
				job.UseLineControl = reply[75] == '1';
				job.RepeatJob = reply[78] == '1';
				job.ToolLoosening = Convert.ToInt32( reply.Substring( 81, 1 ) );
				job.Reserved = Convert.ToInt32( reply.Substring( 86, 1 ) );
				job.JobList = new List<JobItem>( );

				int number = Convert.ToInt32( reply.Substring( 89, 2 ) );
				for (int i = 0; i < number; i++)
				{
					job.JobList.Add( new JobItem( reply.Substring( 92 + i * 12, 11 ) ) );
				}

				return OperateResult.CreateSuccessResult( job );
			}
			catch (Exception ex)
			{
				return new OperateResult<JobData>( "MID0033 prase failed: " + ex.Message + Environment.NewLine + "Source: " + reply );
			}
		}

		#endregion

		#region Private Member

		private OpenProtocolNet openProtocol;

		#endregion
	}

	/// <summary>
	/// Job data
	/// </summary>
	public class JobData
	{
		/// <summary>
		/// Job ID
		/// </summary>
		public int JobID { get; set; }

		/// <summary>
		/// Job name
		/// </summary>
		public string JobName { get; set; }

		/// <summary>
		/// Forced order: 0=free order, 1=forced order, 2=free and forced
		/// </summary>
		public int ForcedOrder { get; set; }

		/// <summary>
		/// Max time for first tightening
		/// </summary>
		public int MaxTimeForFirstTightening { get; set; }

		/// <summary>
		/// Max time to complete Job
		/// </summary>
		public int MaxTimeToCompleteJob { get; set; }

		/// <summary>
		/// Job batch mode
		/// </summary>
		public int JobBatchMode { get; set; }

		/// <summary>
		/// Lock at Job done
		/// </summary>
		public bool LockAtJobDone { get; set; }

		/// <summary>
		/// Use line control
		/// </summary>
		public bool UseLineControl { get; set; }

		/// <summary>
		/// Repeat Job
		/// </summary>
		public bool RepeatJob { get; set; }

		/// <summary>
		/// Tool loosening: 0=Enable, 1=Disable, 2=Enable only on NOK tightening
		/// </summary>
		public int ToolLoosening { get; set; }

		/// <summary>
		/// Reserved for Job repair. 0=E, 1=G
		/// </summary>
		public int Reserved { get; set; }

		/// <summary>
		/// A list of parameter sets
		/// </summary>
		public List<JobItem> JobList { get; set; }
	}

	/// <summary>
	/// JobItem
	/// </summary>
	public class JobItem
	{
		/// <summary>
		/// 实例化一个默认的对象
		/// </summary>
		public JobItem( )
		{

		}

		/// <summary>
		/// 指定原始数据实例化一个对象信息
		/// </summary>
		/// <param name="data">等待分析的原始数据，例如：15:011:0:22</param>
		public JobItem( string data )
		{
			if (data.Length == 12) data = data.Substring( 0, 11 );

			string[] splits = data.Split( new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries );
			ChannelID = Convert.ToInt32( splits[0] );
			TypeID = Convert.ToInt32( splits[1] );
			AutoValue = Convert.ToInt32( splits[2] );
			BatchSize = Convert.ToInt32( splits[3] );
		}

		/// <summary>
		/// Channel-ID
		/// </summary>
		public int ChannelID { get; set; }

		/// <summary>
		/// Type-ID
		/// </summary>
		public int TypeID { get; set; }

		/// <summary>
		/// AutoValue
		/// </summary>
		public int AutoValue { get; set; }

		/// <summary>
		/// BatchSize
		/// </summary>
		public int BatchSize { get; set; }
	}
}
