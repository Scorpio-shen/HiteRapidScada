using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace KpCommon.Helper
{
    public class EncryptionHelper
    {
        #region DES

        /// <summary>
        /// DES加密
        /// </summary>
        /// <param name="encryptString"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string DesEncrypt(string encryptString, string key)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key.Substring(0,8));
            var inputByteArray = Encoding.UTF8.GetBytes(encryptString);
            var provider = new DESCryptoServiceProvider { Mode = CipherMode.ECB };
            var mStream = new MemoryStream();
            var cStream = new CryptoStream(mStream, provider.CreateEncryptor(keyBytes, keyBytes), CryptoStreamMode.Write);
            cStream.Write(inputByteArray, 0, inputByteArray.Length);
            cStream.FlushFinalBlock();
            return System.Convert.ToBase64String(mStream.ToArray());
        }

        /// <summary>
        /// DES解密
        /// </summary>
        /// <param name="decryptString"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string DesDecrypt(string decryptString, string key)
        {
            try
            {
                while (key.Length < 8)
                {
                    var newKey = new StringBuilder();
                    newKey.Append(key).Append("0");
                    key = newKey.ToString();
                }
                var keyBytes = Encoding.UTF8.GetBytes(key.Substring(0, 8));
                var inputByteArray = System.Convert.FromBase64String(decryptString);
                var provider = new DESCryptoServiceProvider();
                provider.Mode = CipherMode.ECB;
                var mStream = new MemoryStream();
                var cStream = new CryptoStream(mStream, provider.CreateDecryptor(keyBytes, keyBytes), CryptoStreamMode.Write);
                cStream.Write(inputByteArray, 0, inputByteArray.Length);
                cStream.FlushFinalBlock();
                return Encoding.UTF8.GetString(mStream.ToArray());
            }
            catch (Exception e)
            {
                throw new Exception($"DES decryption error：{e}");
            }
        }

        #endregion DES

        #region MD5

        /// <summary>
        /// Md5加密
        /// </summary>
        /// <param name="strText"></param>
        /// <returns></returns>
        public static string Md5Encrypt(string strText)
        {
            return BitConverter.ToString(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(strText)));
        }

        /// <summary>
        /// 标准MD5加密32位大写
        /// </summary>
        /// <param name="strText"></param>
        /// <returns></returns>
        public static string StandMd5Encrypt(string strText)
        {
            return BitConverter.ToString(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(strText))).Replace("-", "");
        }

        /// <summary>
        /// 标准MD5加密32位小写
        /// </summary>
        /// <param name="strText"></param>
        /// <returns></returns>
        public static string StandMd5EncryptToLower(string strText)
        {
            return BitConverter.ToString(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(strText))).Replace("-", "").ToLower();
        }

        /// <summary>
        /// MD5获取字符串Hash
        /// </summary>
        /// <param name="strText"></param>
        /// <param name="encode"></param>
        /// <returns></returns>
        public static byte[] Md5Hash(string strText, string encode = "utf-8")
        {
            //MD5CryptoServiceProvider md5Hasher = new MD5CryptoServiceProvider();
            var md5 = MD5.Create();//utf-8
            var hashedDataBytes = md5.ComputeHash(Encoding.GetEncoding(encode).GetBytes(strText));

            return hashedDataBytes;
        }

        /// <summary>
        /// Md5加密
        /// </summary>
        /// <param name="preString"></param>
        /// <param name="inputCharset"></param>
        /// <returns></returns>
        public static string Md5Sign(string preString, string inputCharset)
        {
            var sb = new StringBuilder(32);

            MD5 md5 = new MD5CryptoServiceProvider();
            var t = md5.ComputeHash(Encoding.GetEncoding(inputCharset).GetBytes(preString));
            foreach (var t1 in t)
            {
                sb.Append(t1.ToString("x").PadLeft(2, '0'));
            }

            return sb.ToString();
        }

        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="preString"></param>
        /// <param name="charset"></param>
        /// <returns></returns>
        public static string Md5SignV2(string preString, string charset = "UTF-8")
        {
            var m5 = new MD5CryptoServiceProvider();

            //创建md5对象
            byte[] inputBye;

            //使用编码方式把字符串转化为字节数组．
            try
            {
                inputBye = Encoding.GetEncoding(charset).GetBytes(preString);
            }
            catch (Exception)
            {
                inputBye = Encoding.GetEncoding("GB2312").GetBytes(preString);
            }
            var outputBye = m5.ComputeHash(inputBye);

            var retStr = System.BitConverter.ToString(outputBye);
            retStr = retStr.Replace("-", "").ToUpper();
            return retStr;
        }

        /// <summary>
        /// MD5 16位加密 加密后密码为大写
        /// </summary>
        /// <param name="convertString"></param>
        /// <returns></returns>
        public static string StandMd5Encrypt16(string convertString)
        {
            var md5 = new MD5CryptoServiceProvider();
            var t2 = BitConverter.ToString(md5.ComputeHash(Encoding.Default.GetBytes(convertString)), 4, 8);
            t2 = t2.Replace("-", "");
            return t2;
        }

        #endregion MD5

        #region Mqtt Base64加密
        /// <summary>
        /// Base64加密
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string Base64Encode(string content)
        {
            var buffer = Encoding.UTF8.GetBytes(content);
            return Convert.ToBase64String(buffer);
        }
        /// <summary>
        /// Base64解密
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string Base64Decode(string content)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(content));
        }
        /// <summary>
        /// Base64解密
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static string Base64Decode(byte[] buffer)
        {
            var content = Encoding.UTF8.GetString(buffer);
            return Encoding.UTF8.GetString(Convert.FromBase64String(content));
        }
        #endregion

    }
}
