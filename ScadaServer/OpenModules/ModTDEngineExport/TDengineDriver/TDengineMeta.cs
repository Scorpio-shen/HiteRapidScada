using System;

namespace TDengineDriver
{
	internal class TDengineMeta
	{
		public string TypeName()
		{
			switch (this.type)
			{
			case 1:
				return "BOOL";
			case 2:
				return "TINYINT";
			case 3:
				return "SMALLINT";
			case 4:
				return "INT";
			case 5:
				return "BIGINT";
			case 6:
				return "FLOAT";
			case 7:
				return "DOUBLE";
			case 8:
				return "STRING";
			case 9:
				return "TIMESTAMP";
			case 10:
				return "NCHAR";
			case 11:
				return "TINYINT UNSIGNED";
			case 12:
				return "SMALLINT UNSIGNED";
			case 13:
				return "INT UNSIGNED";
			case 14:
				return "BIGINT UNSIGNED";
			default:
				return "undefine";
			}
		}

		public string name;

		public short size;

		public byte type;
	}
}
