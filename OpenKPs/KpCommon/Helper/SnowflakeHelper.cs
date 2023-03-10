using NewLife.Data;
using NewLife.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpCommon.Helper
{
    public static class SnowflakeHelper
    {
        private static Snowflake snowflake;
        static SnowflakeHelper()
        {
            //禁用newlife日志
            XTrace.Log.Enable = false;
            snowflake = new Snowflake();    
        }
        /// <summary>
        /// 获取随机Id
        /// </summary>
        /// <returns></returns>
        public static long GetNewId()
        {
            return snowflake.NewId(DateTime.Now);
        }
    }
}
