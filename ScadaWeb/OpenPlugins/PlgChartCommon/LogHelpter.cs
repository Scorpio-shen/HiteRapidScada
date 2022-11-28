using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace Web.Helpter
{
    class LogHelpter
    {
        /// <summary>
        /// 限制文件生成大小，258000字节,大约253kb
        /// </summary>
        const long fileBytes = 258000;

        static string GetNewFileName(string fileFolderPath, string fileNameNoExtension)
        {
            string fileName = string.Empty;
            if (fileNameNoExtension.LastIndexOf('_') == -1)
            {
                int index = 1;
                fileName = fileFolderPath + "\\" + DateTime.Now.ToString("yyyy-MM-dd") + "_" + index + ".txt";
                while (File.Exists(fileName))
                {
                    if (File.ReadAllBytes(fileName).Length < fileBytes)
                    {
                        break;
                    }
                    index++;
                    fileName = fileFolderPath + "\\" + DateTime.Now.ToString("yyyy-MM-dd") + "_" + index + ".txt";
                }
            }
            else
            {
                int index = 1;
                int.TryParse(fileNameNoExtension.Split('_')[1], out index);
                if (index == 0)
                {
                    index = 1;
                }
                fileName = fileFolderPath + "\\" + DateTime.Now.ToString("yyyy-MM-dd") + "_" + index + ".txt";
                while (File.Exists(fileName))
                {
                    if (File.ReadAllBytes(fileName).Length < fileBytes)
                    {
                        break;
                    }
                    index++;
                    fileName = fileFolderPath + "\\" + DateTime.Now.ToString("yyyy-MM-dd") + "_" + index + ".txt";
                }
            }
            return fileName;
        }

        /// <summary>  
        /// 日志记录，在程序执行的根目录，写入txt文件，文件固定大小，超过限定大小自动创建新日志文件
        /// </summary>  
        /// <param name="msg">记录内容</param>  
        /// <returns></returns>  
        public static void AddLog(string msg)
        {
            string saveFolder = "Log";//日志文件保存路径   
            try
            {
                string fileFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, saveFolder);
                if (Directory.Exists(fileFolderPath) == false)
                {
                    Directory.CreateDirectory(fileFolderPath);
                }
                string fileName = string.Empty;
                string[] files = Directory.GetFiles(fileFolderPath);
                if (files.Length == 0)
                {
                    fileName = fileFolderPath + "\\" + DateTime.Now.ToString("yyyy-MM-dd") + "_1.txt";
                    goto DO_WRITE;
                }
                string[] files2 = files.OrderByDescending(x => x).ToArray();
                FileInfo fileInfo = new FileInfo(files2[0]);
                string fileNameNoExtension = Path.GetFileNameWithoutExtension(fileInfo.Name);
                if (fileInfo.Length > fileBytes)
                {
                    fileName = GetNewFileName(fileFolderPath, fileNameNoExtension);
                }
                else
                {
                    fileName = GetNewFileName(fileFolderPath, fileNameNoExtension);
                }
            DO_WRITE:
                FileStream fs = new FileStream(fileName, FileMode.Append);
                string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                msg = time + ">" + msg + System.Environment.NewLine;
                byte[] logBytes = UTF8Encoding.UTF8.GetBytes(msg);
                fs.Write(logBytes, 0, logBytes.Length);
                fs.Flush();
                fs.Close();
                fs.Dispose();
                //  tishiMsg = "写入日志成功";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    class LogHelpterAnDa
    {
        /// <summary>
        /// 限制文件生成大小，258000字节,大约253kb
        /// </summary>
        //const long fileBytes = 258000;
        const long fileBytes = 128000;

        static string GetNewFileName(string fileFolderPath)
        {
            string fileName = fileFolderPath + "\\" + "web_log_cur.txt";
            string fileNameBackup = fileFolderPath + "\\" + "web_log_bak.txt";

            if (File.Exists(fileName))
            {
                if (File.ReadAllBytes(fileName).Length < fileBytes)
                {
                    return fileName;
                }
                else
                {
                    if (File.Exists(fileNameBackup))
                    {
                        File.Delete(fileNameBackup);
                    }

                    File.Move(fileName, fileNameBackup);
                }
            }

            return fileName;
        }

        //返回""表示没有log
        public static void GetLogUpload()
        {
            string saveFolder = "Log";//日志文件保存路径   
            try
            {
                string fileFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, saveFolder);
                if (Directory.Exists(fileFolderPath) == false)
                {
                    Directory.CreateDirectory(fileFolderPath);
                }

                string fileName = fileFolderPath + "\\" + "web_log_cur.txt";
                string fileNameBackup = fileFolderPath + "\\" + "web_log_bak.txt";
                string fileNameUpload = fileFolderPath + "\\" + "web_log_upload.txt";

                if (File.Exists(fileName) && File.Exists(fileNameBackup))
                {
                    String[] file = new String[2];
                    file[0] = fileNameBackup;
                    file[1] = fileName;
                    CombineFile(file, fileNameUpload);
                }
                else if (File.Exists(fileName))
                {
                    String[] file = new String[1];
                    file[0] = fileName;
                    CombineFile(file, fileNameUpload);
                }
                else if (File.Exists(fileNameBackup))
                {
                    String[] file = new String[1];
                    file[0] = fileNameBackup;
                    CombineFile(file, fileNameUpload);
                }
                else
                {
                    if (!File.Exists(fileNameUpload))
                    {
                        //fileNameUpload = "";
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static void CombineFile(String[] infileName, String outfileName)
        {
            int b;
            int n = infileName.Length;
            FileStream[] fileIn = new FileStream[n];
            using (FileStream fileOut = new FileStream(outfileName, FileMode.Create))
            {
                for (int i = 0; i < n; i++)
                {
                    try
                    {
                        fileIn[i] = new FileStream(infileName[i], FileMode.Open);
                        while ((b = fileIn[i].ReadByte()) != -1)
                            fileOut.WriteByte((byte)b);
                    }
                    catch (System.Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    finally
                    {
                        fileIn[i].Close();
                    }

                }
            }
        }

        /// <summary>  
        /// 日志记录，在程序执行的根目录，写入txt文件，文件固定大小，超过限定大小自动创建新日志文件
        /// </summary>  
        /// <param name="msg">记录内容</param>  
        /// <returns></returns>  
        public static void AddLog(string msg)
        {
            string saveFolder = "Log";//日志文件保存路径   
            try
            {
                string fileFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, saveFolder);
                if (Directory.Exists(fileFolderPath) == false)
                {
                    Directory.CreateDirectory(fileFolderPath);
                }
                string fileName = string.Empty;
                string[] files = Directory.GetFiles(fileFolderPath);
                if (files.Length == 0)
                {
                    fileName = fileFolderPath + "\\" + "web_log_cur.txt";
                    goto DO_WRITE;
                }
                string[] files2 = files.OrderByDescending(x => x).ToArray();
                FileInfo fileInfo = new FileInfo(files2[0]);
                string fileNameNoExtension = Path.GetFileNameWithoutExtension(fileInfo.Name);
                if (fileInfo.Length > fileBytes)
                {
                    fileName = GetNewFileName(fileFolderPath);
                }
                else
                {
                    fileName = GetNewFileName(fileFolderPath);
                }
            DO_WRITE:
                FileStream fs = new FileStream(fileName, FileMode.Append);
                string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                msg = time + ">" + msg + System.Environment.NewLine;
                byte[] logBytes = UTF8Encoding.UTF8.GetBytes(msg);
                fs.Write(logBytes, 0, logBytes.Length);
                fs.Flush();
                fs.Close();
                fs.Dispose();
                //  tishiMsg = "写入日志成功";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}