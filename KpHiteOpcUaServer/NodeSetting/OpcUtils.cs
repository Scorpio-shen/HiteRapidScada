using Opc.Ua;
using Opc.Ua.Security.Certificates;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace KpHiteOpcUaServer.NodeSetting
{
    public static class Utils
    {
        public enum TraceOutput
        {
            Off,
            FileOnly,
            DebugAndFile
        }

        public static class TraceMasks
        {
            public const int None = 0;

            public const int Error = 1;

            public const int Information = 2;

            public const int StackTrace = 4;

            public const int Service = 8;

            public const int ServiceDetail = 16;

            public const int Operation = 32;

            public const int OperationDetail = 64;

            public const int StartStop = 128;

            public const int ExternalSystem = 256;

            public const int Security = 512;

            public const int All = int.MaxValue;
        }

        public class Nonce
        {
            private static RandomNumberGenerator m_rng = RandomNumberGenerator.Create();

            public static byte[] CreateNonce(uint length)
            {
                byte[] array = new byte[length];
                m_rng.GetBytes(array);
                return array;
            }
        }

        public const string UriSchemeHttps = "https";

        public const string UriSchemeOpcTcp = "opc.tcp";

        public const int UaTcpDefaultPort = 4840;

        public static readonly string[] DiscoveryUrls = new string[4] { "opc.tcp://{0}:4840", "https://{0}:4843", "http://{0}:52601/UADiscovery", "http://{0}/UADiscovery/Default.svc" };

        public const string UaTcpBindingDefault = "Opc.Ua.Bindings.UaTcpBinding";

        public const string DefaultStoreType = "Directory";

        public const string DefaultStorePath = "%CommonApplicationData%/OPC Foundation/CertificateStores/MachineDefault";

        public static string DefaultLocalFolder = Directory.GetCurrentDirectory();

        private static int s_traceOutput = 2;

        private static int s_traceMasks = int.MaxValue;

        private static string s_traceFileName = string.Empty;

        private static long s_BaseLineTicks = DateTime.UtcNow.Ticks;

        private static object s_traceFileLock = new object();

        private const int MAX_MESSAGE_LENGTH = 1024;

        private const uint FORMAT_MESSAGE_IGNORE_INSERTS = 512u;

        private const uint FORMAT_MESSAGE_FROM_SYSTEM = 4096u;

        private static readonly DateTime s_TimeBase = new DateTime(1601, 1, 1);

        private static readonly Lazy<bool> IsRunningOnMonoValue = new Lazy<bool>(() => Type.GetType("Mono.Runtime") != null);

        public static int TraceMask => s_traceMasks;

        public static Tracing Tracing => Tracing.Instance;

        public static DateTime TimeBase => s_TimeBase;

        public static void SetTraceOutput(TraceOutput output)
        {
            lock (s_traceFileLock)
            {
                s_traceOutput = (int)output;
            }
        }

        public static void SetTraceMask(int masks)
        {
            s_traceMasks = masks;
        }

        private static void TraceWriteLine(string message, params object[] args)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            string text = message;
            if (args != null && args.Length != 0)
            {
                try
                {
                    text = string.Format(CultureInfo.InvariantCulture, message, args);
                }
                catch (Exception)
                {
                    text = message;
                }
            }

            lock (s_traceFileLock)
            {
                if (s_traceOutput == 2)
                {
                    Debug.WriteLine(text);
                }

                string text2 = s_traceFileName;
                if (s_traceOutput == 0 || string.IsNullOrEmpty(text2))
                {
                    return;
                }

                try
                {
                    FileInfo fileInfo = new FileInfo(text2);
                    bool flag = false;
                    if (fileInfo.Exists && fileInfo.Length > 10000000)
                    {
                        fileInfo.Delete();
                        flag = true;
                    }

                    using (StreamWriter streamWriter = new StreamWriter(File.Open(fileInfo.FullName, FileMode.Append, FileAccess.Write, FileShare.Read)))
                    {
                        if (flag)
                        {
                            streamWriter.WriteLine("WARNING - LOG FILE TRUNCATED.");
                        }

                        streamWriter.WriteLine(text);
                        streamWriter.Flush();
                        streamWriter.Dispose();
                    } ;
                   
                }
                catch (Exception ex2)
                {
                    Debug.WriteLine("Could not write to trace file. Error={0}\r\nFilePath={1}", ex2.Message, text2);
                }
            }
        }

        public static void SetTraceLog(string filePath, bool deleteExisting)
        {
            lock (s_traceFileLock)
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    s_traceFileName = null;
                    return;
                }

                s_traceFileName = GetAbsoluteFilePath(filePath, checkCurrentDirectory: true, throwOnError: false, createAlways: true, writable: true);
                if (s_traceOutput == 0)
                {
                    s_traceOutput = 1;
                }

                try
                {
                    FileInfo fileInfo = new FileInfo(s_traceFileName);
                    if (deleteExisting && fileInfo.Exists)
                    {
                        fileInfo.Delete();
                    }

                    TraceWriteLine("\r\n{1} Logging started at {0}", DateTime.Now, new string('*', 25));
                }
                catch (Exception ex)
                {
                    TraceWriteLine(ex.Message, null);
                }
            }
        }

        public static void Trace(string format, params object[] args)
        {
            Trace(2, format, handled: false, args);
        }

        [Conditional("DEBUG")]
        public static void TraceDebug(string format, params object[] args)
        {
            Trace(64, format, handled: false, args);
        }

        public static void Trace(Exception e, string format, params object[] args)
        {
            Trace(e, format, handled: false, args);
        }

        public static void Trace(Exception e, string format, bool handled, params object[] args)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (args != null && args.Length != 0)
            {
                try
                {
                    stringBuilder.AppendFormat(CultureInfo.InvariantCulture, format, args);
                }
                catch (Exception)
                {
                    stringBuilder.Append(format);
                }
            }
            else
            {
                stringBuilder.Append(format);
            }

            if (e != null)
            {
                ServiceResultException ex2 = e as ServiceResultException;
                if (ex2 != null)
                {
                    stringBuilder.AppendFormat(CultureInfo.InvariantCulture, " {0} '{1}'", StatusCodes.GetBrowseName(ex2.StatusCode), ex2.Message);
                }
                else
                {
                    stringBuilder.AppendFormat(CultureInfo.InvariantCulture, " {0} '{1}'", e.GetType().Name, e.Message);
                }

                if (((uint)s_traceMasks & 4u) != 0)
                {
                    stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "\r\n\r\n{0}\r\n", new string('=', 40));
                    stringBuilder.Append(new ServiceResult(e).ToLongString());
                    stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "\r\n{0}\r\n", new string('=', 40));
                }
            }

            Trace(1, stringBuilder.ToString(), handled, null);
        }

        public static void Trace(int traceMask, string format, params object[] args)
        {
            Trace(traceMask, format, handled: false, args);
        }

        public static void Trace(int traceMask, string format, bool handled, params object[] args)
        {
            //if (!handled)
            //{
            //    Tracing.Instance.RaiseTraceEvent(new TraceEventArgs(traceMask, format, string.Empty, null, args));
            //}

            if ((s_traceMasks & traceMask) == 0)
            {
                return;
            }

            double num = (double)(HiResClock.UtcNow.Ticks - s_BaseLineTicks) / 10000000.0;
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("{0:d} {0:HH:mm:ss.fff} ", HiResClock.UtcNow.ToLocalTime());
            if (args != null && args.Length != 0)
            {
                try
                {
                    stringBuilder.AppendFormat(CultureInfo.InvariantCulture, format, args);
                }
                catch (Exception)
                {
                    stringBuilder.Append(format);
                }
            }
            else
            {
                stringBuilder.Append(format);
            }

            TraceWriteLine(stringBuilder.ToString(), null);
        }

        public static bool IsPathRooted(string path)
        {
            return Path.IsPathRooted(path) || path[0] == '.';
        }

        private static string ReplaceSpecialFolderWithEnvVar(string input)
        {
            if (input == "CommonApplicationData")
            {
                return "ProgramData";
            }

            return input;
        }

        public static string ReplaceSpecialFolderNames(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return null;
            }

            if (IsPathRooted(input))
            {
                return input;
            }

            if (input[0] != '%')
            {
                return input;
            }

            string text = null;
            string text2 = null;
            int num = input.IndexOf('%', 1);
            if (num == -1)
            {
                text = input.Substring(1);
                text2 = string.Empty;
            }
            else
            {
                text = input.Substring(1, num - 1);
                text2 = input.Substring(num + 1);
            }

            StringBuilder stringBuilder = new StringBuilder();
            if (!Enum.TryParse<Environment.SpecialFolder>(text, out var result))
            {
                text = ReplaceSpecialFolderWithEnvVar(text);
                string environmentVariable = Environment.GetEnvironmentVariable(text);
                if (environmentVariable != null)
                {
                    stringBuilder.Append(environmentVariable);
                }
                else if (text == "LocalFolder")
                {
                    stringBuilder.Append(DefaultLocalFolder);
                }
            }
            else
            {
                stringBuilder.Append(Environment.GetFolderPath(result));
            }

            stringBuilder.Append(text2);
            return stringBuilder.ToString();
        }

        public static string FindInstalledFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return null;
            }

            string text = null;
            for (DirectoryInfo directoryInfo = new DirectoryInfo(Directory.GetCurrentDirectory()); directoryInfo != null; directoryInfo = directoryInfo.Parent)
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append(directoryInfo.FullName);
                stringBuilder.Append(Path.DirectorySeparatorChar + "Bin" + Path.DirectorySeparatorChar);
                stringBuilder.Append(fileName);
                text = GetAbsoluteFilePath(stringBuilder.ToString(), checkCurrentDirectory: false, throwOnError: false, createAlways: false);
                if (text != null)
                {
                    break;
                }
            }

            return text;
        }

        public static string GetAbsoluteFilePath(string filePath)
        {
            return GetAbsoluteFilePath(filePath, checkCurrentDirectory: false, throwOnError: true, createAlways: false);
        }

        public static string GetAbsoluteFilePath(string filePath, bool checkCurrentDirectory, bool throwOnError, bool createAlways, bool writable = false)
        {
            filePath = ReplaceSpecialFolderNames(filePath);
            if (!string.IsNullOrEmpty(filePath))
            {
                FileInfo fileInfo = new FileInfo(filePath);
                bool flag = IsPathRooted(filePath);
                if (flag)
                {
                    if (fileInfo.Exists)
                    {
                        return filePath;
                    }

                    if (createAlways)
                    {
                        return CreateFile(fileInfo, filePath, throwOnError);
                    }
                }

                if (!flag && checkCurrentDirectory)
                {
                    FileInfo fileInfo2 = null;
                    fileInfo2 = (writable ? new FileInfo(Format("{0}{1}{2}", Path.GetTempPath(), Path.DirectorySeparatorChar, filePath)) : new FileInfo(Format("{0}{1}{2}", Directory.GetCurrentDirectory(), Path.DirectorySeparatorChar, filePath)));
                    if (fileInfo2.Exists)
                    {
                        return fileInfo2.FullName;
                    }

                    if (fileInfo.Exists && !writable)
                    {
                        return fileInfo.FullName;
                    }

                    if (createAlways && writable)
                    {
                        return CreateFile(fileInfo2, fileInfo2.FullName, throwOnError);
                    }
                }
            }

            if (throwOnError)
            {
                throw ServiceResultException.Create(2156462080u, "File does not exist: {0}\r\nCurrent directory is: {1}", filePath, Directory.GetCurrentDirectory());
            }

            return null;
        }

        private static string CreateFile(FileInfo file, string filePath, bool throwOnError)
        {
            try
            {
                if (!file.Directory.Exists)
                {
                    Directory.CreateDirectory(file.DirectoryName);
                }

                using (file.Open(FileMode.CreateNew, FileAccess.ReadWrite))
                {
                    return filePath;
                }
            }
            catch (Exception ex)
            {
                Trace(ex, "Could not create file: {0}", filePath);
                if (throwOnError)
                {
                    throw ex;
                }

                return filePath;
            }
        }

        public static string GetAbsoluteDirectoryPath(string dirPath, bool checkCurrentDirectory, bool throwOnError)
        {
            return GetAbsoluteDirectoryPath(dirPath, checkCurrentDirectory, throwOnError, createAlways: false);
        }

        public static string GetAbsoluteDirectoryPath(string dirPath, bool checkCurrentDirectory, bool throwOnError, bool createAlways)
        {
            string text = dirPath;
            dirPath = ReplaceSpecialFolderNames(dirPath);
            if (!string.IsNullOrEmpty(dirPath))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(dirPath);
                bool flag = IsPathRooted(dirPath);
                if (flag)
                {
                    if (directoryInfo.Exists)
                    {
                        return dirPath;
                    }

                    if (createAlways && !directoryInfo.Exists)
                    {
                        directoryInfo = Directory.CreateDirectory(dirPath);
                        return directoryInfo.FullName;
                    }
                }

                if (!flag)
                {
                    if (checkCurrentDirectory && !directoryInfo.Exists)
                    {
                        directoryInfo = new DirectoryInfo(Format("{0}{1}{2}", Directory.GetCurrentDirectory(), Path.DirectorySeparatorChar, dirPath));
                    }

                    if (directoryInfo.Exists)
                    {
                        return directoryInfo.FullName;
                    }

                    if (createAlways)
                    {
                        directoryInfo = Directory.CreateDirectory(directoryInfo.FullName);
                        return directoryInfo.FullName;
                    }
                }
            }

            if (throwOnError)
            {
                throw ServiceResultException.Create(2156462080u, "Directory does not exist: {0}\r\nCurrent directory is: {1}", text, Directory.GetCurrentDirectory());
            }

            return null;
        }

        public static string GetFilePathDisplayName(string filePath, int maxLength)
        {
            if (filePath == null || maxLength <= 0 || filePath.Length < maxLength)
            {
                return filePath;
            }

            int num = filePath.IndexOf(Path.DirectorySeparatorChar);
            if (num == -1)
            {
                return Format("{0}...", filePath.Substring(0, maxLength));
            }

            int num2 = filePath.LastIndexOf(Path.DirectorySeparatorChar);
            while (num2 > num && filePath.Length - num2 < maxLength)
            {
                num2 = filePath.LastIndexOf(Path.DirectorySeparatorChar, num2 - 1);
                if (filePath.Length - num2 > maxLength)
                {
                    num2 = filePath.IndexOf(Path.DirectorySeparatorChar, num2 + 1);
                    break;
                }
            }

            return Format("{0}...{1}", filePath.Substring(0, num + 1), filePath.Substring(num2));
        }

        public static void SilentDispose(object objectToDispose)
        {
            IDisposable disposable = objectToDispose as IDisposable;
            if (disposable != null)
            {
                try
                {
                    disposable.Dispose();
                }
                catch (Exception e)
                {
                    Trace(e, "Error disposing object: {0}", disposable.GetType().Name);
                }
            }
        }

        public static DateTime GetDeadline(TimeSpan timeSpan)
        {
            DateTime utcNow = DateTime.UtcNow;
            if (DateTime.MaxValue.Ticks - utcNow.Ticks < timeSpan.Ticks)
            {
                return DateTime.MaxValue;
            }

            return utcNow + timeSpan;
        }

        public static int GetTimeout(TimeSpan timeSpan)
        {
            if (timeSpan.TotalMilliseconds > 2147483647.0)
            {
                return -1;
            }

            if (timeSpan.TotalMilliseconds < 0.0)
            {
                return 0;
            }

            return (int)timeSpan.TotalMilliseconds;
        }

        public static async Task<IPAddress[]> GetHostAddresses(string remoteHostName)
        {
            return await Dns.GetHostAddressesAsync(remoteHostName);
        }

        public static string GetHostName()
        {
            return Dns.GetHostName().Split('.')[0].ToLowerInvariant();
        }

        public static string GetFullQualifiedDomainName()
        {
            string text = null;
            try
            {
                text = Dns.GetHostEntry("localhost").HostName;
            }
            catch
            {
            }

            if (string.IsNullOrEmpty(text))
            {
                return Dns.GetHostName();
            }

            return text;
        }

        public static string NormalizedIPAddress(string ipAddress)
        {
            try
            {
                IPAddress iPAddress = IPAddress.Parse(ipAddress);
                return iPAddress.ToString();
            }
            catch
            {
                return ipAddress;
            }
        }

        public static string ReplaceLocalhost(string uri, string hostname = null)
        {
            if (string.IsNullOrEmpty(uri))
            {
                return uri;
            }

            if (!string.IsNullOrEmpty(hostname) && hostname.Contains(':'))
            {
                hostname = "[" + hostname + "]";
            }

            int num = uri.IndexOf("localhost", StringComparison.OrdinalIgnoreCase);
            if (num == -1)
            {
                return uri;
            }

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(uri.Substring(0, num));
            stringBuilder.Append((hostname == null) ? GetHostName() : hostname);
            stringBuilder.Append(uri.Substring(num + "localhost".Length));
            return stringBuilder.ToString();
        }

        public static string ReplaceDCLocalhost(string subjectName, string hostname = null)
        {
            if (string.IsNullOrEmpty(subjectName))
            {
                return subjectName;
            }

            if (!string.IsNullOrEmpty(hostname) && hostname.Contains(':'))
            {
                hostname = "[" + hostname + "]";
            }

            int num = subjectName.IndexOf("DC=localhost", StringComparison.OrdinalIgnoreCase);
            if (num == -1)
            {
                return subjectName;
            }

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(subjectName.Substring(0, num + 3));
            stringBuilder.Append((hostname == null) ? GetHostName() : hostname);
            stringBuilder.Append(subjectName.Substring(num + "DC=localhost".Length));
            return stringBuilder.ToString();
        }

        public static Uri ParseUri(string uri)
        {
            try
            {
                if (string.IsNullOrEmpty(uri))
                {
                    return null;
                }

                return new Uri(uri);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static bool AreDomainsEqual(Uri url1, Uri url2)
        {
            if (url1 == null || url2 == null)
            {
                return false;
            }

            try
            {
                string text = url1.DnsSafeHost;
                string text2 = url2.DnsSafeHost;
                if (text == "localhost")
                {
                    text = GetHostName();
                }

                if (text2 == "localhost")
                {
                    text2 = GetHostName();
                }

                if (AreDomainsEqual(text, text2))
                {
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool AreDomainsEqual(string domain1, string domain2)
        {
            if (string.IsNullOrEmpty(domain1) || string.IsNullOrEmpty(domain2))
            {
                return false;
            }

            if (string.Compare(domain1, domain2, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return true;
            }

            return false;
        }

        public static string UpdateInstanceUri(string instanceUri)
        {
            if (string.IsNullOrEmpty(instanceUri))
            {
                UriBuilder uriBuilder = new UriBuilder();
                uriBuilder.Scheme = "https";
                uriBuilder.Host = GetHostName();
                uriBuilder.Port = -1;
                uriBuilder.Path = Guid.NewGuid().ToString();
                return uriBuilder.Uri.ToString();
            }

            if (!instanceUri.StartsWith("https", StringComparison.Ordinal))
            {
                UriBuilder uriBuilder2 = new UriBuilder();
                uriBuilder2.Scheme = "https";
                uriBuilder2.Host = GetHostName();
                uriBuilder2.Port = -1;
                uriBuilder2.Path = Uri.EscapeDataString(instanceUri);
                return uriBuilder2.Uri.ToString();
            }

            Uri uri = ParseUri(instanceUri);
            if (uri != null && uri.DnsSafeHost == "localhost")
            {
                UriBuilder uriBuilder3 = new UriBuilder(uri);
                uriBuilder3.Host = GetHostName();
                return uriBuilder3.Uri.ToString();
            }

            return instanceUri;
        }

        public static uint IncrementIdentifier(ref long identifier)
        {
            Interlocked.CompareExchange(ref identifier, 0L, 4294967295L);
            return (uint)Interlocked.Increment(ref identifier);
        }

        public static int IncrementIdentifier(ref int identifier)
        {
            Interlocked.CompareExchange(ref identifier, 0, int.MaxValue);
            return Interlocked.Increment(ref identifier);
        }

        public static int ToInt32(uint identifier)
        {
            if (identifier <= int.MaxValue)
            {
                return (int)identifier;
            }

            return -(int)(4294967295L - (long)identifier + 1);
        }

        public static uint ToUInt32(int identifier)
        {
            if (identifier >= 0)
            {
                return (uint)identifier;
            }

            return (uint)(4294967296L + identifier);
        }

        public static Array FlattenArray(Array array)
        {
            Array array2 = Array.CreateInstance(array.GetType().GetElementType(), array.Length);
            int[] array3 = new int[array.Rank];
            int[] array4 = new int[array.Rank];
            for (int num = array.Rank - 1; num >= 0; num--)
            {
                array4[num] = array.GetLength(array.Rank - num - 1);
            }

            for (int i = 0; i < array.Length; i++)
            {
                array3[array.Rank - 1] = i % array4[0];
                for (int j = 1; j < array.Rank; j++)
                {
                    int num2 = 1;
                    for (int k = 0; k < j; k++)
                    {
                        num2 *= array4[k];
                    }

                    array3[array.Rank - j - 1] = i / num2 % array4[j];
                }

                array2.SetValue(array.GetValue(array3), i);
            }

            return array2;
        }

        public static string ToHexString(byte[] buffer)
        {
            if (buffer == null || buffer.Length == 0)
            {
                return string.Empty;
            }

            StringBuilder stringBuilder = new StringBuilder(buffer.Length * 2);
            for (int i = 0; i < buffer.Length; i++)
            {
                stringBuilder.AppendFormat("{0:X2}", buffer[i]);
            }

            return stringBuilder.ToString();
        }

        public static byte[] FromHexString(string buffer)
        {
            if (buffer == null)
            {
                return null;
            }

            if (buffer.Length == 0)
            {
                return new byte[0];
            }

            string text = buffer.ToUpperInvariant();
            byte[] array = new byte[buffer.Length / 2 + buffer.Length % 2];
            for (int i = 0; i < array.Length * 2; i += 2)
            {
                int num = "0123456789ABCDEF".IndexOf(buffer[i]);
                if (num == -1)
                {
                    break;
                }

                byte b = (byte)num;
                b = (byte)(b << 4);
                if (i < buffer.Length - 1)
                {
                    num = "0123456789ABCDEF".IndexOf(buffer[i + 1]);
                    if (num == -1)
                    {
                        break;
                    }

                    b = (byte)(b + (byte)num);
                }

                array[i / 2] = b;
            }

            return array;
        }

        public static string ToString(object source)
        {
            if (source != null)
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}", source);
            }

            return string.Empty;
        }

        public static string Format(string text, params object[] args)
        {
            return string.Format(CultureInfo.InvariantCulture, text, args);
        }

        public static bool IsValidLocaleId(string localeId)
        {
            if (string.IsNullOrEmpty(localeId))
            {
                return false;
            }

            try
            {
                CultureInfo cultureInfo = new CultureInfo(localeId);
                if (cultureInfo != null)
                {
                    return true;
                }
            }
            catch (Exception)
            {
            }

            return false;
        }

        public static string GetLanguageId(string localeId)
        {
            if (localeId == null)
            {
                return string.Empty;
            }

            int num = localeId.IndexOf('-');
            if (num != -1)
            {
                return localeId.Substring(0, num);
            }

            return localeId;
        }

        public static LocalizedText SelectLocalizedText(IList<string> localeIds, IList<LocalizedText> names, LocalizedText defaultName)
        {
            if (localeIds == null || localeIds.Count == 0)
            {
                return defaultName;
            }

            if (names == null || names.Count == 0)
            {
                return defaultName;
            }

            for (int i = 0; i < localeIds.Count; i++)
            {
                for (int j = 0; j < names.Count; j++)
                {
                    if (!LocalizedText.IsNullOrEmpty(names[j]) && string.Compare(names[j].Locale, localeIds[i], StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return names[j];
                    }
                }
            }

            for (int k = 0; k < localeIds.Count; k++)
            {
                string languageId = GetLanguageId(localeIds[k]);
                for (int l = 0; l < names.Count; l++)
                {
                    if (!LocalizedText.IsNullOrEmpty(names[l]))
                    {
                        string languageId2 = GetLanguageId(names[l].Locale);
                        if (string.Compare(languageId, languageId2, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            return names[l];
                        }
                    }
                }
            }

            return defaultName;
        }

        public static object Clone(object value)
        {
            if (value == null)
            {
                return null;
            }

            Type type = value.GetType();
            if (type.GetTypeInfo().IsValueType)
            {
                return value;
            }

            if (type == typeof(string))
            {
                return value;
            }

            Array array = value as Array;
            if (array != null)
            {
                if (array.Rank == 1)
                {
                    Array array2 = Array.CreateInstance(type.GetElementType(), array.Length);
                    for (int i = 0; i < array.Length; i++)
                    {
                        array2.SetValue(Clone(array.GetValue(i)), i);
                    }

                    return array2;
                }

                int[] array3 = new int[array.Rank];
                int[] array4 = new int[array.Rank];
                for (int j = 0; j < array.Rank; j++)
                {
                    array3[j] = array.GetLength(j);
                    array4[j] = 0;
                }

                Array array5 = Array.CreateInstance(type.GetElementType(), array3);
                for (int k = 0; k < array.Length; k++)
                {
                    array5.SetValue(Clone(array.GetValue(array4)), array4);
                    for (int l = 0; l < array.Rank; l++)
                    {
                        array4[l]++;
                        if (array4[l] < array3[l])
                        {
                            break;
                        }

                        array4[l] = 0;
                    }
                }

                return array5;
            }

            XmlNode xmlNode = value as XmlNode;
            if (xmlNode != null)
            {
                return xmlNode.CloneNode(deep: true);
            }

            ExtensionObject extensionObject = value as ExtensionObject;
            if (extensionObject != null)
            {
                return extensionObject.MemberwiseClone();
            }

            ExtensionObjectCollection extensionObjectCollection = value as ExtensionObjectCollection;
            if (extensionObjectCollection != null)
            {
                return extensionObjectCollection.MemberwiseClone();
            }

            EnumValueType enumValueType = value as EnumValueType;
            if (enumValueType != null)
            {
                return enumValueType.MemberwiseClone();
            }

            LocalizedText localizedText = value as LocalizedText;
            if (localizedText != null)
            {
                return localizedText.MemberwiseClone();
            }

            Argument argument = value as Argument;
            if (argument != null)
            {
                return argument.MemberwiseClone();
            }

            NodeId nodeId = value as NodeId;
            if (nodeId != null)
            {
                return nodeId.MemberwiseClone();
            }

            UInt32Collection uInt32Collection = value as UInt32Collection;
            if (uInt32Collection != null)
            {
                return uInt32Collection.MemberwiseClone();
            }

            QualifiedName qualifiedName = value as QualifiedName;
            if (qualifiedName != null)
            {
                return qualifiedName.MemberwiseClone();
            }

            ServerDiagnosticsSummaryDataType serverDiagnosticsSummaryDataType = value as ServerDiagnosticsSummaryDataType;
            if (serverDiagnosticsSummaryDataType != null)
            {
                return serverDiagnosticsSummaryDataType.MemberwiseClone();
            }

            ApplicationDescription applicationDescription = value as ApplicationDescription;
            if (applicationDescription != null)
            {
                return applicationDescription.MemberwiseClone();
            }

            StringCollection stringCollection = value as StringCollection;
            if (stringCollection != null)
            {
                return stringCollection.MemberwiseClone();
            }

            UserTokenPolicyCollection userTokenPolicyCollection = value as UserTokenPolicyCollection;
            if (userTokenPolicyCollection != null)
            {
                return userTokenPolicyCollection.MemberwiseClone();
            }

            UserTokenPolicy userTokenPolicy = value as UserTokenPolicy;
            if (userTokenPolicy != null)
            {
                return userTokenPolicy.MemberwiseClone();
            }

            SessionDiagnosticsDataType sessionDiagnosticsDataType = value as SessionDiagnosticsDataType;
            if (sessionDiagnosticsDataType != null)
            {
                return sessionDiagnosticsDataType.MemberwiseClone();
            }

            ServiceCounterDataType serviceCounterDataType = value as ServiceCounterDataType;
            if (serviceCounterDataType != null)
            {
                return serviceCounterDataType.MemberwiseClone();
            }

            SessionSecurityDiagnosticsDataType sessionSecurityDiagnosticsDataType = value as SessionSecurityDiagnosticsDataType;
            if (sessionSecurityDiagnosticsDataType != null)
            {
                return sessionSecurityDiagnosticsDataType.MemberwiseClone();
            }

            AnonymousIdentityToken anonymousIdentityToken = value as AnonymousIdentityToken;
            if (anonymousIdentityToken != null)
            {
                return anonymousIdentityToken.MemberwiseClone();
            }

            EventFilter eventFilter = value as EventFilter;
            if (eventFilter != null)
            {
                return eventFilter.MemberwiseClone();
            }

            DataChangeFilter dataChangeFilter = value as DataChangeFilter;
            if (dataChangeFilter != null)
            {
                return dataChangeFilter.MemberwiseClone();
            }

            SimpleAttributeOperandCollection simpleAttributeOperandCollection = value as SimpleAttributeOperandCollection;
            if (simpleAttributeOperandCollection != null)
            {
                return simpleAttributeOperandCollection.MemberwiseClone();
            }

            SimpleAttributeOperand simpleAttributeOperand = value as SimpleAttributeOperand;
            if (simpleAttributeOperand != null)
            {
                return simpleAttributeOperand.MemberwiseClone();
            }

            QualifiedNameCollection qualifiedNameCollection = value as QualifiedNameCollection;
            if (qualifiedNameCollection != null)
            {
                return qualifiedNameCollection.MemberwiseClone();
            }

            ContentFilter contentFilter = value as ContentFilter;
            if (contentFilter != null)
            {
                return contentFilter.MemberwiseClone();
            }

            ContentFilterElement contentFilterElement = value as ContentFilterElement;
            if (contentFilterElement != null)
            {
                return contentFilterElement.MemberwiseClone();
            }

            ContentFilterElementCollection contentFilterElementCollection = value as ContentFilterElementCollection;
            if (contentFilterElementCollection != null)
            {
                return contentFilterElementCollection.MemberwiseClone();
            }

            SubscriptionDiagnosticsDataType subscriptionDiagnosticsDataType = value as SubscriptionDiagnosticsDataType;
            if (subscriptionDiagnosticsDataType != null)
            {
                return subscriptionDiagnosticsDataType.MemberwiseClone();
            }

            UserNameIdentityToken userNameIdentityToken = value as UserNameIdentityToken;
            if (userNameIdentityToken != null)
            {
                return userNameIdentityToken.MemberwiseClone();
            }

            ServerStatusDataType serverStatusDataType = value as ServerStatusDataType;
            if (serverStatusDataType != null)
            {
                return serverStatusDataType.MemberwiseClone();
            }

            BuildInfo buildInfo = value as BuildInfo;
            if (buildInfo != null)
            {
                return buildInfo.MemberwiseClone();
            }

            X509IdentityToken x509IdentityToken = value as X509IdentityToken;
            if (x509IdentityToken != null)
            {
                return x509IdentityToken.MemberwiseClone();
            }

            Range range = value as Range;
            if (range != null)
            {
                return range.MemberwiseClone();
            }

            EUInformation eUInformation = value as EUInformation;
            if (eUInformation != null)
            {
                return eUInformation.MemberwiseClone();
            }

            WriteValueCollection writeValueCollection = value as WriteValueCollection;
            if (writeValueCollection != null)
            {
                return writeValueCollection.MemberwiseClone();
            }

            WriteValue writeValue = value as WriteValue;
            if (writeValue != null)
            {
                return writeValue.MemberwiseClone();
            }

            DataValue dataValue = value as DataValue;
            if (dataValue != null)
            {
                return dataValue.MemberwiseClone();
            }

            ExpandedNodeId expandedNodeId = value as ExpandedNodeId;
            if (expandedNodeId != null)
            {
                return expandedNodeId.MemberwiseClone();
            }

            TimeZoneDataType timeZoneDataType = value as TimeZoneDataType;
            if (timeZoneDataType != null)
            {
                return timeZoneDataType.MemberwiseClone();
            }

            LiteralOperand literalOperand = value as LiteralOperand;
            if (literalOperand != null)
            {
                return literalOperand.MemberwiseClone();
            }

            MethodInfo method = type.GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.Public);
            if (method != null)
            {
                object obj = method.Invoke(value, null);
                if (obj != null)
                {
                    return obj;
                }
            }

            throw new NotSupportedException(Format("Don't know how to clone objects of type '{0}'", type.FullName));
        }

        public static bool IsEqual(object value1, object value2)
        {
            if (value1 == value2)
            {
                return true;
            }

            if (value1 == null)
            {
                return value2?.Equals(value1) ?? true;
            }

            if (value2 == null)
            {
                return value1.Equals(value2);
            }

            if (value1.GetType() != value2.GetType())
            {
                return value1.Equals(value2);
            }

            IComparable comparable = value1 as IComparable;
            if (comparable != null)
            {
                return comparable.CompareTo(value2) == 0;
            }

            IEncodeable encodeable = value1 as IEncodeable;
            if (encodeable != null)
            {
                IEncodeable encodeable2 = value2 as IEncodeable;
                if (encodeable2 == null)
                {
                    return false;
                }

                return encodeable.IsEqual(encodeable2);
            }

            XmlElement xmlElement = value1 as XmlElement;
            if (xmlElement != null)
            {
                XmlElement xmlElement2 = value2 as XmlElement;
                if (xmlElement2 == null)
                {
                    return false;
                }

                return xmlElement.OuterXml == xmlElement2.OuterXml;
            }

            Array array = value1 as Array;
            if (array != null)
            {
                Array array2 = value2 as Array;
                if (array2 == null)
                {
                    return false;
                }

                if (array.Length != array2.Length)
                {
                    return false;
                }

                for (int i = 0; i < array.Length; i++)
                {
                    if (!IsEqual(array.GetValue(i), array2.GetValue(i)))
                    {
                        return false;
                    }
                }

                return true;
            }

            IEnumerable enumerable = value1 as IEnumerable;
            if (enumerable != null)
            {
                IEnumerable enumerable2 = value2 as IEnumerable;
                if (enumerable2 == null)
                {
                    return false;
                }

                IEnumerator enumerator = enumerable.GetEnumerator();
                IEnumerator enumerator2 = enumerable2.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    if (!enumerator2.MoveNext())
                    {
                        return false;
                    }

                    if (!IsEqual(enumerator.Current, enumerator2.Current))
                    {
                        return false;
                    }
                }

                if (enumerator2.MoveNext())
                {
                    return false;
                }

                return true;
            }

            return value1.Equals(value2);
        }

        public static bool Match(string target, string pattern, bool caseSensitive)
        {
            if (pattern == null || pattern.Length == 0)
            {
                return true;
            }

            if (target == null || target.Length == 0)
            {
                return false;
            }

            if (caseSensitive)
            {
                if (target == pattern)
                {
                    return true;
                }
            }
            else if (target.ToUpperInvariant() == pattern.ToUpperInvariant())
            {
                return true;
            }

            int num = 0;
            int num2 = 0;
            while (num2 < target.Length && num < pattern.Length)
            {
                char c = ConvertCase(pattern[num++], caseSensitive);
                if (num > pattern.Length)
                {
                    return num2 >= target.Length;
                }

                switch (c)
                {
                    case '*':
                        while (num2 < target.Length)
                        {
                            if (Match(target.Substring(num2++), pattern.Substring(num), caseSensitive))
                            {
                                return true;
                            }
                        }

                        return Match(target, pattern.Substring(num), caseSensitive);
                    case '?':
                        if (num2 >= target.Length)
                        {
                            return false;
                        }

                        if (num >= pattern.Length && num2 < target.Length - 1)
                        {
                            return false;
                        }

                        num2++;
                        break;
                    case '[':
                        {
                            char c2 = ConvertCase(target[num2++], caseSensitive);
                            if (num2 > target.Length)
                            {
                                return false;
                            }

                            char c3 = '\0';
                            if (pattern[num] == '!')
                            {
                                num++;
                                c = ConvertCase(pattern[num++], caseSensitive);
                                while (num < pattern.Length && c != ']')
                                {
                                    if (c == '-')
                                    {
                                        c = ConvertCase(pattern[num], caseSensitive);
                                        if (num > pattern.Length || c == ']')
                                        {
                                            return false;
                                        }

                                        if (c2 >= c3 && c2 <= c)
                                        {
                                            return false;
                                        }
                                    }

                                    c3 = c;
                                    if (c2 == c)
                                    {
                                        return false;
                                    }

                                    c = ConvertCase(pattern[num++], caseSensitive);
                                }

                                break;
                            }

                            c = ConvertCase(pattern[num++], caseSensitive);
                            while (num < pattern.Length)
                            {
                                if (c == ']')
                                {
                                    return false;
                                }

                                if (c == '-')
                                {
                                    c = ConvertCase(pattern[num], caseSensitive);
                                    if (num > pattern.Length || c == ']')
                                    {
                                        return false;
                                    }

                                    if (c2 >= c3 && c2 <= c)
                                    {
                                        break;
                                    }
                                }

                                c3 = c;
                                if (c2 == c)
                                {
                                    break;
                                }

                                c = ConvertCase(pattern[num++], caseSensitive);
                            }

                            while (num < pattern.Length && c != ']')
                            {
                                c = pattern[num++];
                            }

                            break;
                        }
                    case '#':
                        {
                            char c2 = target[num2++];
                            if (!char.IsDigit(c2))
                            {
                                return false;
                            }

                            break;
                        }
                    default:
                        {
                            char c2 = ConvertCase(target[num2++], caseSensitive);
                            if (c2 != c)
                            {
                                return false;
                            }

                            if (num >= pattern.Length && num2 < target.Length - 1)
                            {
                                return false;
                            }

                            break;
                        }
                }
            }

            if (num2 >= target.Length)
            {
                return num >= pattern.Length;
            }

            return true;
        }

        private static char ConvertCase(char c, bool caseSensitive)
        {
            return caseSensitive ? c : char.ToUpperInvariant(c);
        }

        public static TimeZoneDataType GetTimeZoneInfo()
        {
            TimeZoneDataType timeZoneDataType = new TimeZoneDataType();
            timeZoneDataType.Offset = (short)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalMinutes;
            timeZoneDataType.DaylightSavingInOffset = true;
            return timeZoneDataType;
        }

        public static T ParseExtension<T>(IList<XmlElement> extensions, XmlQualifiedName elementName)
        {
            if (extensions == null || extensions.Count == 0)
            {
                return default(T);
            }

            if (elementName == null)
            {
                XmlQualifiedName xmlName = EncodeableFactory.GetXmlName(typeof(T));
                if (xmlName == null)
                {
                    throw new ArgumentException("Type does not seem to support DataContract serialization");
                }

                elementName = xmlName;
            }

            for (int i = 0; i < extensions.Count; i++)
            {
                XmlElement xmlElement = extensions[i];
                if (xmlElement.LocalName != elementName.Name || xmlElement.NamespaceURI != elementName.Namespace)
                {
                    continue;
                }

                XmlReader xmlReader = XmlReader.Create(new StringReader(xmlElement.OuterXml));
                try
                {
                    DataContractSerializer dataContractSerializer = new DataContractSerializer(typeof(T));
                    return (T)dataContractSerializer.ReadObject(xmlReader);
                }
                catch (Exception ex)
                {
                    Trace("Exception parsing extension: " + ex.Message);
                    throw ex;
                }
                finally
                {
                    xmlReader.Dispose();
                }
            }

            return default(T);
        }

        public static void UpdateExtension<T>(ref XmlElementCollection extensions, XmlQualifiedName elementName, object value)
        {
            XmlDocument xmlDocument = new XmlDocument();
            StringBuilder stringBuilder = new StringBuilder();
            using (XmlWriter xmlWriter = XmlWriter.Create(stringBuilder))
            {
                if (value != null)
                {
                    try
                    {
                        DataContractSerializer dataContractSerializer = new DataContractSerializer(typeof(T));
                        dataContractSerializer.WriteObject(xmlWriter, value);
                    }
                    finally
                    {
                        xmlWriter.Dispose();
                    }

                    xmlDocument.InnerXml = stringBuilder.ToString();
                }
            }

            if (elementName == null)
            {
                XmlQualifiedName xmlName = EncodeableFactory.GetXmlName(typeof(T));
                if (xmlName == null)
                {
                    throw new ArgumentException("Type does not seem to support DataContract serialization");
                }

                elementName = xmlName;
            }

            if (extensions != null)
            {
                for (int i = 0; i < extensions.Count; i++)
                {
                    if (extensions[i] != null && extensions[i].LocalName == elementName.Name && extensions[i].NamespaceURI == elementName.Namespace)
                    {
                        if (value == null)
                        {
                            extensions.RemoveAt(i);
                        }
                        else
                        {
                            extensions[i] = xmlDocument.DocumentElement;
                        }

                        return;
                    }
                }
            }

            if (value != null)
            {
                if (extensions == null)
                {
                    extensions = new XmlElementCollection();
                }

                extensions.Add(xmlDocument.DocumentElement);
            }
        }

        public static string[] GetFieldNames(Type systemType)
        {
            FieldInfo[] fields = systemType.GetFields(BindingFlags.Static | BindingFlags.Public);
            int num = 0;
            string[] array = new string[fields.Length];
            FieldInfo[] array2 = fields;
            foreach (FieldInfo fieldInfo in array2)
            {
                array[num++] = fieldInfo.Name;
            }

            return array;
        }

        public static string GetDataMemberName(PropertyInfo property)
        {
            object[] array = property.GetCustomAttributes(typeof(DataMemberAttribute), inherit: true).ToArray();
            if (array != null)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    DataMemberAttribute dataMemberAttribute = array[i] as DataMemberAttribute;
                    if (dataMemberAttribute != null)
                    {
                        if (string.IsNullOrEmpty(dataMemberAttribute.Name))
                        {
                            return property.Name;
                        }

                        return dataMemberAttribute.Name;
                    }
                }
            }

            return null;
        }

        public static uint GetIdentifier(string name, Type constants)
        {
            FieldInfo[] fields = constants.GetFields(BindingFlags.Static | BindingFlags.Public);
            FieldInfo[] array = fields;
            foreach (FieldInfo fieldInfo in array)
            {
                if (fieldInfo.Name == name)
                {
                    return (uint)fieldInfo.GetValue(constants);
                }
            }

            return 0u;
        }

        public static DateTime GetAssemblyTimestamp()
        {
            try
            {
                return File.GetLastWriteTimeUtc(typeof(Utils).GetTypeInfo().Assembly.Location);
            }
            catch
            {
            }

            return new DateTime(1970, 1, 1, 0, 0, 0);
        }

        public static string GetAssemblySoftwareVersion()
        {
            return typeof(Utils).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        }

        public static string GetAssemblyBuildNumber()
        {
            return typeof(Utils).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>().Version;
        }

        public static byte[] Append(params byte[][] arrays)
        {
            if (arrays == null)
            {
                return new byte[0];
            }

            int num = 0;
            for (int i = 0; i < arrays.Length; i++)
            {
                if (arrays[i] != null)
                {
                    num += arrays[i].Length;
                }
            }

            byte[] array = new byte[num];
            int num2 = 0;
            for (int j = 0; j < arrays.Length; j++)
            {
                if (arrays[j] != null)
                {
                    Array.Copy(arrays[j], 0, array, num2, arrays[j].Length);
                    num2 += arrays[j].Length;
                }
            }

            return array;
        }

        public static X509Certificate2 ParseCertificateBlob(byte[] certificateData)
        {
            try
            {
                return CertificateFactory.Create(certificateData, useCache: true);
            }
            catch (Exception e)
            {
                throw new ServiceResultException(2148663296u, "Could not parse DER encoded form of an X509 certificate.", e);
            }
        }

        public static X509Certificate2Collection ParseCertificateChainBlob(byte[] certificateData)
        {
            X509Certificate2Collection x509Certificate2Collection = new X509Certificate2Collection();
            List<byte> list = new List<byte>(certificateData);
            X509Certificate2 x509Certificate = null;
            while (list.Count > 0)
            {
                try
                {
                    x509Certificate = CertificateFactory.Create(list.ToArray(), useCache: true);
                }
                catch (Exception e)
                {
                    throw new ServiceResultException(2148663296u, "Could not parse DER encoded form of an X509 certificate.", e);
                }

                x509Certificate2Collection.Add(x509Certificate);
                list.RemoveRange(0, x509Certificate.RawData.Length);
            }

            return x509Certificate2Collection;
        }

        public static bool CompareNonce(byte[] a, byte[] b)
        {
            if (a == null || b == null)
            {
                return false;
            }

            if (a.Length != b.Length)
            {
                return false;
            }

            byte b2 = 0;
            for (int i = 0; i < a.Length; i++)
            {
                b2 = (byte)(b2 | (byte)(a[i] ^ b[i]));
            }

            return b2 == 0;
        }

        public static byte[] PSHA1(byte[] secret, string label, byte[] data, int offset, int length)
        {
            if (secret == null)
            {
                throw new ArgumentNullException("secret");
            }

            HMACSHA1 hmac = new HMACSHA1(secret);
            return PSHA(hmac, label, data, offset, length);
        }

        public static byte[] PSHA256(byte[] secret, string label, byte[] data, int offset, int length)
        {
            if (secret == null)
            {
                throw new ArgumentNullException("secret");
            }

            HMACSHA256 hmac = new HMACSHA256(secret);
            return PSHA(hmac, label, data, offset, length);
        }

        private static byte[] PSHA(HMAC hmac, string label, byte[] data, int offset, int length)
        {
            if (hmac == null)
            {
                throw new ArgumentNullException("hmac");
            }

            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset");
            }

            if (length < 0)
            {
                throw new ArgumentOutOfRangeException("length");
            }

            byte[] array = null;
            if (!string.IsNullOrEmpty(label))
            {
                array = new UTF8Encoding().GetBytes(label);
            }

            if (data != null && data.Length != 0)
            {
                if (array != null)
                {
                    byte[] array2 = new byte[array.Length + data.Length];
                    array.CopyTo(array2, 0);
                    data.CopyTo(array2, array.Length);
                    array = array2;
                }
                else
                {
                    array = data;
                }
            }

            if (array == null)
            {
                throw new ServiceResultException(2147549184u, "The HMAC algorithm requires a non-null seed.");
            }

            byte[] array3 = hmac.ComputeHash(array);
            byte[] array4 = new byte[hmac.HashSize / 8 + array.Length];
            Array.Copy(array3, array4, array3.Length);
            Array.Copy(array, 0, array4, array3.Length, array.Length);
            byte[] array5 = new byte[length];
            int num = 0;
            do
            {
                byte[] array6 = hmac.ComputeHash(array4);
                if (offset < array6.Length)
                {
                    int num2 = offset;
                    while (num < length && num2 < array6.Length)
                    {
                        array5[num++] = array6[num2];
                        num2++;
                    }
                }

                offset = ((offset > array6.Length) ? (offset - array6.Length) : 0);
                array3 = hmac.ComputeHash(array3);
                Array.Copy(array3, array4, array3.Length);
            }
            while (num < length);
            return array5;
        }

        public static List<string> ParseDistinguishedName(string name)
        {
            List<string> list = new List<string>();
            if (string.IsNullOrEmpty(name))
            {
                return list;
            }

            char c = ',';
            bool flag = false;
            bool flag2 = false;
            for (int num = name.Length - 1; num >= 0; num--)
            {
                char c2 = name[num];
                if (c2 == '"')
                {
                    flag2 = !flag2;
                }
                else if (!flag2 && c2 == '=')
                {
                    num--;
                    while (num >= 0 && char.IsWhiteSpace(name[num]))
                    {
                        num--;
                    }

                    while (num >= 0 && (char.IsLetterOrDigit(name[num]) || name[num] == '.'))
                    {
                        num--;
                    }

                    while (num >= 0 && char.IsWhiteSpace(name[num]))
                    {
                        num--;
                    }

                    if (num >= 0)
                    {
                        c = name[num];
                    }

                    break;
                }
            }

            StringBuilder stringBuilder = new StringBuilder();
            string value = null;
            string text = null;
            flag = false;
            for (int i = 0; i < name.Length; i++)
            {
                for (; i < name.Length && char.IsWhiteSpace(name[i]); i++)
                {
                }

                if (i >= name.Length)
                {
                    break;
                }

                char c3 = name[i];
                if (flag)
                {
                    char c4 = c;
                    if (i < name.Length && name[i] == '"')
                    {
                        i++;
                        c4 = '"';
                    }

                    for (; i < name.Length; i++)
                    {
                        c3 = name[i];
                        if (c3 == c4)
                        {
                            for (; i < name.Length && name[i] != c; i++)
                            {
                            }

                            break;
                        }

                        stringBuilder.Append(c3);
                    }

                    text = stringBuilder.ToString().TrimEnd();
                    flag = false;
                    stringBuilder.Length = 0;
                    stringBuilder.Append(value);
                    stringBuilder.Append('=');
                    if (text.IndexOfAny(new char[3] { '/', ',', '=' }) != -1)
                    {
                        if (text.Length > 0 && text[0] != '"')
                        {
                            stringBuilder.Append('"');
                        }

                        stringBuilder.Append(text);
                        if (text.Length > 0 && text[text.Length - 1] != '"')
                        {
                            stringBuilder.Append('"');
                        }
                    }
                    else
                    {
                        stringBuilder.Append(text);
                    }

                    list.Add(stringBuilder.ToString());
                    stringBuilder.Length = 0;
                    continue;
                }

                for (; i < name.Length; i++)
                {
                    c3 = name[i];
                    if (c3 == '=')
                    {
                        break;
                    }

                    stringBuilder.Append(c3);
                }

                value = stringBuilder.ToString().TrimEnd().ToUpperInvariant();
                stringBuilder.Length = 0;
                flag = true;
            }

            return list;
        }

        public static bool FindStringIgnoreCase(IList<string> strings, string target)
        {
            if (strings == null || strings.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < strings.Count; i++)
            {
                if (string.Compare(strings[i], target, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return true;
                }
            }

            return false;
        }

        public static IList<string> GetDomainsFromCertficate(X509Certificate2 certificate)
        {
            List<string> list = new List<string>();
            List<string> list2 = ParseDistinguishedName(certificate.Subject);
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < list2.Count; i++)
            {
                if (list2[i].StartsWith("DC="))
                {
                    if (stringBuilder.Length > 0)
                    {
                        stringBuilder.Append('.');
                    }

                    stringBuilder.Append(list2[i].Substring(3));
                }
            }

            if (stringBuilder.Length > 0)
            {
                list.Add(stringBuilder.ToString().ToUpperInvariant());
            }

            X509SubjectAltNameExtension x509SubjectAltNameExtension = null;
            X509ExtensionEnumerator enumerator = certificate.Extensions.GetEnumerator();
            while (enumerator.MoveNext())
            {
                X509Extension current = enumerator.Current;
                if (current.Oid.Value == X509SubjectAltNameExtension.SubjectAltNameOid || current.Oid.Value == X509SubjectAltNameExtension.SubjectAltName2Oid)
                {
                    x509SubjectAltNameExtension = new X509SubjectAltNameExtension(current, current.Critical);
                    break;
                }
            }

            if (x509SubjectAltNameExtension != null)
            {
                for (int j = 0; j < x509SubjectAltNameExtension.DomainNames.Count; j++)
                {
                    string text = x509SubjectAltNameExtension.DomainNames[j];
                    bool flag = false;
                    for (int k = 0; k < list.Count; k++)
                    {
                        if (string.Compare(list[k], text, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            flag = true;
                            break;
                        }
                    }

                    if (!flag)
                    {
                        list.Add(text.ToUpperInvariant());
                    }
                }

                for (int l = 0; l < x509SubjectAltNameExtension.IPAddresses.Count; l++)
                {
                    string item = x509SubjectAltNameExtension.IPAddresses[l];
                    if (!list.Contains(item))
                    {
                        list.Add(item);
                    }
                }
            }

            return list;
        }

        public static string GetApplicationUriFromCertificate(X509Certificate2 certificate)
        {
            X509SubjectAltNameExtension x509SubjectAltNameExtension = null;
            X509ExtensionEnumerator enumerator = certificate.Extensions.GetEnumerator();
            while (enumerator.MoveNext())
            {
                X509Extension current = enumerator.Current;
                if (current.Oid.Value == X509SubjectAltNameExtension.SubjectAltNameOid || current.Oid.Value == X509SubjectAltNameExtension.SubjectAltName2Oid)
                {
                    x509SubjectAltNameExtension = new X509SubjectAltNameExtension(current, current.Critical);
                    break;
                }
            }

            if (x509SubjectAltNameExtension != null && x509SubjectAltNameExtension.Uris.Count > 0)
            {
                return x509SubjectAltNameExtension.Uris[0];
            }

            return string.Empty;
        }

        public static bool HasApplicationURN(X509Certificate2 certificate)
        {
            X509SubjectAltNameExtension x509SubjectAltNameExtension = null;
            X509ExtensionEnumerator enumerator = certificate.Extensions.GetEnumerator();
            while (enumerator.MoveNext())
            {
                X509Extension current = enumerator.Current;
                if (current.Oid.Value == X509SubjectAltNameExtension.SubjectAltNameOid || current.Oid.Value == X509SubjectAltNameExtension.SubjectAltName2Oid)
                {
                    x509SubjectAltNameExtension = new X509SubjectAltNameExtension(current, current.Critical);
                    break;
                }
            }

            if (x509SubjectAltNameExtension != null && x509SubjectAltNameExtension.Uris.Count > 0)
            {
                string text = "urn:";
                for (int i = 0; i < x509SubjectAltNameExtension.Uris.Count; i++)
                {
                    if (string.Compare(x509SubjectAltNameExtension.Uris[i], 0, text, 0, text.Length, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool DoesUrlMatchCertificate(X509Certificate2 certificate, Uri endpointUrl)
        {
            if (endpointUrl == null || certificate == null)
            {
                return false;
            }

            IList<string> domainsFromCertficate = GetDomainsFromCertficate(certificate);
            for (int i = 0; i < domainsFromCertficate.Count; i++)
            {
                if (string.Compare(domainsFromCertficate[i], endpointUrl.DnsSafeHost, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool CompareDistinguishedName(string name1, string name2)
        {
            if (string.Compare(name1, name2, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return true;
            }

            List<string> list = ParseDistinguishedName(name1);
            List<string> list2 = ParseDistinguishedName(name2);
            if (list.Count != list2.Count)
            {
                return false;
            }

            list.Sort(StringComparer.OrdinalIgnoreCase);
            list2.Sort(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < list.Count; i++)
            {
                if (string.Compare(list[i], list2[i], StringComparison.OrdinalIgnoreCase) != 0)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool CompareDistinguishedName(X509Certificate2 certificate, List<string> parsedName)
        {
            if (parsedName.Count == 0)
            {
                return false;
            }

            List<string> list = ParseDistinguishedName(certificate.Subject);
            if (parsedName.Count != list.Count)
            {
                return false;
            }

            parsedName.Sort(StringComparer.OrdinalIgnoreCase);
            list.Sort(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < parsedName.Count; i++)
            {
                if (string.Compare(parsedName[i], list[i], StringComparison.OrdinalIgnoreCase) != 0)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsRunningOnMono()
        {
            return IsRunningOnMonoValue.Value;
        }
    }
}
