using HslCommunication.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HslCommunication.CNC.Fanuc
{
	/// <summary>
	/// 刀具信息
	/// </summary>
	public class CutterInfo
	{
		/// <summary>
		/// 长度形状补偿
		/// </summary>
		public double LengthSharpOffset { get; set; }

		/// <summary>
		/// 长度磨损补偿
		/// </summary>
		public double LengthWearOffset { get; set; }

		/// <summary>
		/// 半径形状补偿
		/// </summary>
		public double RadiusSharpOffset { get; set; }

		/// <summary>
		/// 半径磨损补偿
		/// </summary>
		public double RadiusWearOffset { get; set; }

		/// <inheritdoc/>
		public override string ToString( )
		{
			return $"LengthSharpOffset:{LengthSharpOffset:10} LengthWearOffset:{LengthWearOffset:10} RadiusSharpOffset:{RadiusSharpOffset:10} RadiusWearOffset:{RadiusWearOffset:10}";
		}
	}


	/// <summary>
	/// Fanuc的系统信息
	/// </summary>
	public class FanucSysInfo
	{
		/// <summary>
		/// 实例化一个空对象
		/// </summary>
		public FanucSysInfo( )
		{

		}

		/// <summary>
		/// 使用缓存数据来实例化一个对象
		/// </summary>
		/// <param name="buffer">原始的字节信息</param>
		public FanucSysInfo( byte[] buffer )
		{
			TypeCode = Encoding.ASCII.GetString( buffer, 32, 2 );
			switch (TypeCode)
			{
				case "15": CncType = "Series 15/15i"; break;
				case "16": CncType = "Series 16/16i"; break;
				case "18": CncType = "Series 18/18i"; break;
				case "21": CncType = "Series 21/210i"; break;
				case "30": CncType = "Series 30i"; break;
				case "31": CncType = "Series 31i"; break;
				case "32": CncType = "Series 32i"; break;
				case " 0": CncType = "Series 0i"; break;
				case "PD": CncType = "Power Mate i-D"; break;
				case "PH": CncType = "Power Mate i-H"; break;
			}
			CncType += "-";
			switch (Encoding.ASCII.GetString( buffer, 34, 2 ))
			{
				case " M": MtType = "Machining center"; break;
				case " T": MtType = "Lathe"; break;
				case "MM": MtType = "M series with 2 path control"; break;
				case "TT": MtType = "T series with 2/3 path control"; break;
				case "MT": MtType = "T series with compound machining function"; break;
				case " P": MtType = "Punch press"; break;
				case " L": MtType = "Laser"; break;
				case " W": MtType = "Wire cut"; break;
			}
			CncType += Encoding.ASCII.GetString( buffer, 34, 2 ).Trim( );
			switch (buffer[28])
			{
				case 1: CncType += "A"; break;
				case 2: CncType += "B"; break;
				case 3: CncType += "C"; break;
				case 4: CncType += "D"; break;
				case 6: CncType += "F"; break;
			}

			Series = Encoding.ASCII.GetString( buffer, 36, 4 );
			Version = Encoding.ASCII.GetString( buffer, 40, 4 );
			Axes = int.Parse( Encoding.ASCII.GetString( buffer, 44, 2 ) );
		}

		/// <summary>
		/// CNC的类型代号
		/// </summary>
		public string TypeCode { get; set; }

		/// <summary>
		/// CNC的类型
		/// </summary>
		public string CncType { get; set; }

		/// <summary>
		/// Kind of M/T,
		/// </summary>
		public string MtType { get; set; }

		/// <summary>
		/// 系列信息
		/// </summary>
		public string Series { get; set; }

		/// <summary>
		/// 版本号信息
		/// </summary>
		public string Version { get; set; }

		/// <summary>
		/// Current controlled axes
		/// </summary>
		public int Axes { get; set; }
	}

	/// <summary>
	/// Fanuc机床的操作信息
	/// </summary>
	public class FanucOperatorMessage
	{
		/// <summary>
		/// Number of operator's message
		/// </summary>
		public short Number { get; set; }

		/// <summary>
		/// Kind of operator's message
		/// </summary>
		public short Type { get; set; }

		/// <summary>
		/// Operator's message strings
		/// </summary>
		public string Data { get; set; }

		/// <summary>
		/// 创建一个fanuc的操作消息对象
		/// </summary>
		/// <param name="byteTransform">数据变换对象</param>
		/// <param name="buffer">读取的数据缓存信息</param>
		/// <param name="encoding">解析的编码信息</param>
		/// <returns>fanuc设备的操作信息</returns>
		public static FanucOperatorMessage CreateMessage( IByteTransform byteTransform, byte[] buffer, Encoding encoding )
		{
			FanucOperatorMessage fanucOperator = new FanucOperatorMessage( );
			fanucOperator.Number               = byteTransform.TransInt16( buffer, 2 );
			fanucOperator.Type                 = byteTransform.TransInt16( buffer, 6 );
			short len                          = byteTransform.TransInt16( buffer, 10 );
			if (len + 12 <= buffer.Length)
				fanucOperator.Data = encoding.GetString( buffer, 12, len );
			else
				fanucOperator.Data = encoding.GetString( buffer, 12, buffer.Length - 12 ).TrimEnd( '\u0000' );
			return fanucOperator;
		}
	}

	/// <summary>
	/// 文件或是文件夹的信息
	/// </summary>
	public class FileDirInfo
	{
		/// <summary>
		/// 实例化一个默认的对象
		/// </summary>
		public FileDirInfo( )
		{

		}

		/// <summary>
		/// 使用原始字节来实例化对象
		/// </summary>
		/// <param name="byteTransform">字节变换对象</param>
		/// <param name="buffer">原始的字节信息</param>
		/// <param name="index">起始的索引信息</param>
		public FileDirInfo( IByteTransform byteTransform, byte[] buffer, int index )
		{
			IsDirectory = byteTransform.TransInt16( buffer, index ) == 0x00;
			Name        = buffer.GetStringOrEndChar( index + 28, 36, Encoding.ASCII );

			if (!IsDirectory)
			{
				LastModified = new DateTime(
					 byteTransform.TransInt16( buffer, index + 2 ),
					 byteTransform.TransInt16( buffer, index + 4 ),
					 byteTransform.TransInt16( buffer, index + 6 ),
					 byteTransform.TransInt16( buffer, index + 8 ),
					 byteTransform.TransInt16( buffer, index + 10 ),
					 byteTransform.TransInt16( buffer, index + 12 ) );
				Size = byteTransform.TransInt32( buffer, index + 20 );
			}
		}

		/// <summary>
		/// 是否为文件夹，True就是文件夹，False就是文件
		/// </summary>
		public bool IsDirectory { get; set; }

		/// <summary>
		/// 文件或是文件夹的名称
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// 最后一次更新时间，当为文件的时候有效
		/// </summary>
		public DateTime LastModified { get; set; }

		/// <summary>
		/// 文件的大小，当为文件的时候有效
		/// </summary>
		public int Size { get; set; }

		/// <inheritdoc/>
		public override string ToString( )
		{
			StringBuilder stringBuilder = new StringBuilder( );
			stringBuilder.Append( IsDirectory ? "[PATH]   " : "[FILE]   " );
			stringBuilder.Append( Name.PadRight( 40 ) );

			if (!IsDirectory)
			{
				stringBuilder.Append( "     " );
				stringBuilder.Append( LastModified.ToString( "yyyy-MM-dd HH:mm:ss" ) );
				stringBuilder.Append( "         " );
				stringBuilder.Append( HslCommunication.BasicFramework.SoftBasic.GetSizeDescription( Size ) );
			}
			return stringBuilder.ToString( );
		}
	}
}
