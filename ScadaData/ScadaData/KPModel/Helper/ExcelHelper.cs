using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Scada.Helper
{
    public class ExcelHelper
    {
        /// <summary>
        /// 第一步
        /// 上传Excel 并返回上传路径  
        /// </summary>
        /// <param name="files"></param>
        /// <param name="directoryName">文件夹名称</param>
        /// <returns>
        /// 返回路径
        /// </returns>
        //private static string DepositExcel(IFormFile file, string directoryName)
        //{
        //    //创建需要存放的位置 返回一个准确的路径
        //    var path = CreateDirectory("Upload/Excels/" + directoryName);
        //    //文件名
        //    string fileName = DateTime.Now.Ticks.ToString() + "." + file.FileName.Split('.').Last();
        //    path = path + "/" + fileName;
        //    if (File.Exists(path))
        //    {
        //        File.Delete(path);
        //    }

        //    using (var stream = new FileStream(path, FileMode.CreateNew))
        //    {
        //        file.CopyTo(stream);
        //    }
        //    return path;
        //}


        /// <summary>
        /// 第二步
        /// 得到Excel 内容
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static ISheet GetSheet(string path)
        {
            ISheet sheet;
            using (var file = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                MemoryStream ms = new MemoryStream();
                if (Path.GetExtension(path) == ".xls")
                {
                    HSSFWorkbook workbook = new HSSFWorkbook(file);
                    //获取一个sheetName
                    sheet = workbook.GetSheetAt(0);
                }
                else
                {
                    XSSFWorkbook workbook = new XSSFWorkbook(file);
                    //获取一个sheetName
                    sheet = workbook.GetSheetAt(0);
                }
            }
            return sheet;
        }

        /// <summary>
        /// 第三步
        /// 根据Excel 内容得到想要的List
        /// </summary>
        /// <typeparam name="T"></typeparam> 
        /// <param name="sheet"></param>
        /// <param name="headNum"></param>
        /// <returns></returns>
        private static List<T> GetList<T>(ISheet sheet, int headNum)
        {
            List<T> list = new List<T>();
            Dictionary<int, string> dict = new Dictionary<int, string>();
            //获得列名所对应的字段名
            var propertys = GetPropertyByType<T>(false);
            //得到每个字段对应的序号
            IRow head = sheet.GetRow(headNum);

            for (int i = 0; i < head.LastCellNum; i++)
            {
                ICell cell = head.GetCell(i);
                if (propertys.ContainsKey(cell.StringCellValue.Trim()))
                {
                    dict.Add(i, propertys[cell.StringCellValue.Trim()]);
                }
            }
            if (dict.Count != head.LastCellNum)
            {
                throw new Exception("Import tables head and requirements inconsistency");
            }
            var type = typeof(T).GetProperties();
            int c = 0;
            try
            {
                for (int i = headNum + 1; i <= sheet.LastRowNum; i++)
                {
                    c = i;
                    IRow row = sheet.GetRow(i);
                    if (row != null)
                    {
                        bool isAddList = true;
                        T t = Activator.CreateInstance<T>();
                        for (int j = 0; j < row.LastCellNum; j++)
                        {
                            ICell cell = row.GetCell(j);
                            string name = "";
                            dict.TryGetValue(j, out name);
                            if (cell != null)
                            {
                                if (cell.CellType == CellType.Blank)//空值
                                {
                                    isAddList = IsAdd(name, true);
                                }
                                else
                                {
                                    var item = type.FirstOrDefault(m => m.Name == name);
                                    if (item != null)
                                    {
                                        if (item.PropertyType == typeof(DateTime))
                                        {
                                            try
                                            {
                                                if (cell.CellType == CellType.String)
                                                {
                                                    var value = Convert.ToDateTime(cell.ToString());
                                                    item.SetValue(t, value, null);
                                                }
                                                else
                                                {
                                                    item.SetValue(t, cell.DateCellValue, null);
                                                }
                                            }
                                            catch
                                            {
                                                throw new Exception($"DateTime{cell.ToString()}格式不正确!");
                                            }
                                        }
                                        else if (item.PropertyType == typeof(int))
                                        {
                                            try
                                            {
                                                item.SetValue(t, Convert.ToInt32(cell.ToString()), null);
                                            }
                                            catch
                                            {
                                                throw new Exception($"int{cell.ToString()}格式不正确!");
                                            }
                                        }
                                        else if (item.PropertyType == typeof(string))
                                        {
                                            if (cell.CellType == CellType.String)
                                            {
                                                item.SetValue(t, cell.ToString(), null);
                                                isAddList = IsAdd(name, string.IsNullOrEmpty(cell.ToString()));
                                            }
                                            else
                                            {
                                                item.SetValue(t, cell.NumericCellValue.ToString(), null);
                                            }

                                        }
                                        else if (item.PropertyType == typeof(decimal?) || item.PropertyType == typeof(decimal))
                                        {
                                            if (cell != null)
                                            {
                                                try
                                                {
                                                    var value = 0m;
                                                    if (cell.CellType == CellType.String)
                                                    {
                                                        value = Convert.ToDecimal(cell.ToString());
                                                    }
                                                    else
                                                    {
                                                        value = Convert.ToDecimal(cell.NumericCellValue);
                                                    }
                                                    item.SetValue(t, value, null);
                                                    isAddList = IsAdd(name, value == 0);
                                                }
                                                catch
                                                {
                                                    throw new Exception($"decimal{cell.ToString()}格式不正确!");
                                                }
                                            }
                                        }
                                    }
                                }
                                if (isAddList == false)
                                {
                                    break;
                                }
                            }
                        }
                        if (isAddList)
                        {
                            list.Add(t);
                        }
                    }

                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return list;
        }

        private static bool IsAdd(string name, bool isOk)
        {
            bool result = true;
            string[] isNotStrs = { };
            if (isNotStrs.Contains(name) && isOk)
            {
                result = false;
            }
            return result;
        }

        #region 辅助方法
        /// <summary>
        /// 创建目录
        /// </summary>
        /// <param name="directoryPath">目录路径</param>
        private static string CreateDirectory(string directoryPath = "")
        {
            var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content");
            if (!string.IsNullOrEmpty(directoryPath))
            {
                if (directoryPath.Substring(0, 1) != "/")
                {
                    directoryPath = "/" + directoryPath;
                }
                path += directoryPath;
            }
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        /// <summary>
        /// 获得Excel列名
        /// </summary>
        private static Dictionary<string, string> GetPropertyByType<In>(bool isToExcel)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            var type = typeof(In);
            try
            {
                foreach (var item in type.GetProperties())
                {
                    var displayName = item.GetCustomAttributes<DisplayNameAttribute>(false).FirstOrDefault();
                    if (displayName != null)
                    {
                        if (isToExcel)
                        {
                            dict.Add(item.Name, displayName.DisplayName);
                        }
                        else
                        {
                            dict.Add(displayName.DisplayName, item.Name);
                        }
                    }

                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return dict;
        }
        #endregion

        /// <summary>
        ///  功能,
        ///  导入Excel
        ///  列头名和实体的DispName 要一致。
        /// </summary>
        /// <typeparam name="T">要转换的实体</typeparam>
        /// <param name="files">上传的Excel文件</param>
        /// <param name="headNum">Excel头部行数</param>
        /// <returns>
        /// 获得转换后的List 集合
        /// </returns>
        //public static List<T> GetList<T>(IFormFile files, int headNum)
        //{
        //    List<T> list = new List<T>();
        //    string path = DepositExcel(files, typeof(T).Name);
        //    //得到上传文件内容
        //    ISheet sheet = GetSheet(path);
        //    //转换成List
        //    var t = GetList<T>(sheet, headNum);
        //    if (t != null && t.Count > 0)
        //    {
        //        list.AddRange(t);
        //    }
        //    return list;
        //}

        public static List<T> GetList<T>(string filePath, int headNum)
        {
            List<T> list = new List<T>();
            string path = filePath;// DepositExcel(files, typeof(T).Name);
            //得到上传文件内容
            ISheet sheet = GetSheet(path);
            //转换成List
            var t = GetList<T>(sheet, headNum);
            if (t != null && t.Count > 0)
            {
                list.AddRange(t);
            }
            return list;
        }

        /// <summary>
        ///  功能,
        ///  导入Excel
        ///  列头名和实体的DispName 要一致。扩展兼容空白列
        /// </summary>
        /// <typeparam name="T">要转换的实体</typeparam>
        /// <param name="files">上传的Excel文件</param>
        /// <param name="headNum">Excel头部行数</param>
        /// <returns>
        /// 获得转换后的List 集合
        /// </returns>
        //public static List<T> GetListExtend<T>(IFormFile files, int headNum)
        //{
        //    List<T> list = new List<T>();
        //    string path = DepositExcel(files, typeof(T).Name);
        //    //得到上传文件内容
        //    ISheet sheet = GetSheet(path);
        //    //转换成List
        //    var t = GetListExtend<T>(sheet, headNum);
        //    if (t != null && t.Count > 0)
        //    {
        //        list.AddRange(t);
        //    }
        //    return list;
        //}

        public static List<T> GetListExtend<T>(string filePath, int headNum)
        {
            List<T> list = new List<T>();
            string path = filePath;//DepositExcel(files, typeof(T).Name);
            //得到上传文件内容
            ISheet sheet = GetSheet(path);
            //转换成List
            var t = GetListExtend<T>(sheet, headNum);
            if (t != null && t.Count > 0)
            {
                list.AddRange(t);
            }
            return list;
        }

        /// <summary>
        /// 兼容模板空白列
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sheet"></param>
        /// <param name="headNum"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private static List<T> GetListExtend<T>(ISheet sheet, int headNum)
        {
            List<T> list = new List<T>();
            Dictionary<int, string> dict = new Dictionary<int, string>();
            //获得列名所对应的字段名
            var propertys = GetPropertyByType<T>(false);
            //得到每个字段对应的序号
            IRow head = sheet.GetRow(headNum);

            for (int i = 0; i < head.Cells.Count; i++)
            {
                ICell cell = head.GetCell(i);
                if (propertys.ContainsKey(cell.StringCellValue.Trim()))
                {
                    dict.Add(i, propertys[cell.StringCellValue.Trim()]);
                }
            }
            //if (dict.Count != head.LastCellNum)
            //{
            //    throw new Exception("Import tables head and requirements inconsistency");
            //}
            var type = typeof(T).GetProperties();
            int c = 0;
            try
            {
                for (int i = headNum + 1; i <= sheet.LastRowNum; i++)
                {
                    c = i;
                    IRow row = sheet.GetRow(i);
                    if (row != null)
                    {
                        bool isAddList = true;
                        T t = Activator.CreateInstance<T>();
                        for (int j = 0; j < row.Cells.Count; j++)
                        {
                            ICell cell = row.GetCell(j);
                            string name = "";
                            dict.TryGetValue(j, out name);
                            if (cell != null)
                            {
                                if (cell.CellType == CellType.Blank)//空值
                                {
                                    isAddList = IsAdd(name, true);
                                }
                                else
                                {
                                    var item = type.FirstOrDefault(m => m.Name == name);
                                    if (item != null)
                                    {
                                        if (item.PropertyType == typeof(DateTime))
                                        {
                                            try
                                            {
                                                if (cell.CellType == CellType.String)
                                                {
                                                    var value = Convert.ToDateTime(cell.ToString());
                                                    item.SetValue(t, value);
                                                }
                                                else
                                                {
                                                    item.SetValue(t, cell.DateCellValue);
                                                }
                                            }
                                            catch
                                            {
                                                throw new Exception($"DateTime{cell.ToString()}格式不正确!");
                                            }
                                        }
                                        else if (item.PropertyType == typeof(int))
                                        {
                                            try
                                            {
                                                item.SetValue(t, Convert.ToInt32(cell.ToString()));
                                            }
                                            catch
                                            {
                                                throw new Exception($"int{cell.ToString()}格式不正确!");
                                            }
                                        }
                                        else if (item.PropertyType == typeof(string))
                                        {
                                            if (cell.CellType == CellType.String)
                                            {
                                                item.SetValue(t, cell.ToString());
                                                isAddList = IsAdd(name, string.IsNullOrEmpty(cell.ToString()));
                                            }
                                            else
                                            {
                                                item.SetValue(t, cell.NumericCellValue.ToString());
                                            }

                                        }
                                        else if (item.PropertyType == typeof(decimal?) || item.PropertyType == typeof(decimal))
                                        {
                                            if (cell != null)
                                            {
                                                try
                                                {
                                                    var value = 0m;
                                                    if (cell.CellType == CellType.String)
                                                    {
                                                        value = Convert.ToDecimal(cell.ToString());
                                                    }
                                                    else
                                                    {
                                                        value = Convert.ToDecimal(cell.NumericCellValue);
                                                    }
                                                    item.SetValue(t, value);
                                                    isAddList = IsAdd(name, value == 0);
                                                }
                                                catch
                                                {
                                                    throw new Exception($"decimal{cell.ToString()}格式不正确!");
                                                }
                                            }
                                        }
                                    }
                                }
                                if (isAddList == false)
                                {
                                    break;
                                }
                            }
                        }
                        if (isAddList)
                        {
                            list.Add(t);
                        }
                    }

                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return list;
        }

        /// <summary>
        /// 生成Excel流数据，
        /// return File(memoryStream.ToArray(), "application/vnd.ms-excel", fileName); //vnd.ms 此模式有些不兼容
        /// 或者
        /// return File(memoryStream.ToArray(), "application/ms-excel", "红包列表.xls")
        /// </summary>
        /// <typeparam name="T">数据模型</typeparam>
        /// <param name="excelType">excel扩展名类型</param>
        /// <param name="data">数据集</param>
        /// <param name="sheetSize">Excel的单个Sheet的行数，不能超过65535，否则会抛出异常</param>
        /// <returns></returns>
        public static MemoryStream ToExcel<T>(List<T> data, string excelType = "xls", int sheetSize = 50000)
        {

            IWorkbook wk;
            if (excelType.Equals("xlsx") || excelType.Equals(".xlsx"))
            {
                wk = new XSSFWorkbook();
            }
            else
            {
                wk = new HSSFWorkbook();
            }
            var itemType = Activator.CreateInstance<T>().GetType();


            if (data.Count <= 0 || data == null)
            {
                var headers = GetPropertyByType<T>(true);
                CreateHeaders(wk, headers, "sheet");
            }
            else
            {
                int baseNum = 65535;//单个Sheet最大行数65535
                int cNum = data.Count / baseNum;
                int myForCount = data.Count % baseNum == 0 ? cNum : cNum + 1;
                for (int i = 0; i < myForCount; i++)
                {
                    var list = data.Skip(i * baseNum).Take(baseNum).ToList();
                    string sheetName = "sheet" + i;
                    CreateSheet(wk, list, itemType, sheetName);
                }
            }

            MemoryStream m = new MemoryStream();
            wk.Write(m);
            return m;
        }

        /// <summary>
        /// 创建并得到一个 sheet
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="IN"></typeparam>
        /// <param name="wk"></param>
        /// <param name="data"></param>
        /// <param name="itemType"></param>
        /// <param name="sheetName"></param>
        /// <param name="sheetSize"></param>
        /// <param name="valueHandlerDict"></param>
        private static void CreateSheet<T>(IWorkbook wk, List<T> data, Type itemType, string sheetName, int sheetSize = 50000)
        {
            try
            {
                ISheet sheet = null;
                var headers = GetPropertyByType<T>(true);
                sheet = CreateHeaders(wk, headers, sheetName);
                if (data.Count > 0)
                {
                    for (var i = 0; i < data.Count; i++)
                    {
                        //创建内容
                        IRow row = sheet.CreateRow(i % sheetSize + 1);

                        //遍历填充每条数据
                        int j = 0;
                        foreach (var item in headers)
                        {
                            var p = itemType.GetProperty(item.Key);//获取对应列名
                            if (p != null)
                            {
                                var value = p.GetValue(data[i]);
                                value = value == null ? string.Empty : value;
                                ICell cell = row.CreateCell(j);
                                cell.SetCellValue(value.ToString());
                            }
                            j++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        /// <summary>
        /// 创建sheet 表头
        /// </summary>
        /// <param name="wk">workbook</param>
        /// <param name="headers">表头</param>
        /// <param name="sheetName"></param>
        /// <returns>
        /// 返回一个sheet
        /// </returns>
        private static ISheet CreateHeaders(IWorkbook wk, Dictionary<string, string> headers, string sheetName)
        {
            var sheet = wk.CreateSheet(sheetName);
            IRow rowHead = sheet.CreateRow(0);
            ICellStyle style = wk.CreateCellStyle();
            IFont font = wk.CreateFont();//创建字体样式
            font.Boldweight = (short)FontBoldWeight.Bold;
            style.SetFont(font);

            int i = 0;
            foreach (var item in headers)
            {
                ICell cellHead = rowHead.CreateCell(i);
                cellHead.SetCellValue(item.Value);
                cellHead.CellStyle = style;
                i++;
            }
            return sheet;
        }
    }
}
