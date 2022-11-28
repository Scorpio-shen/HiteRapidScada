/*
 * Copyright 2019 Mikhail Shiryaev
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 * 
 * Product  : Rapid SCADA
 * Module   : ScadaWebCommon
 * Summary  : WCF service that provides the client API
 * 
 * Author   : Mikhail Shiryaev
 * Created  : 2016
 * Modified : 2019
 */
using Scada.Client;
using Scada.Data.Models;
using Scada.Data.Tables;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Web.Script.Serialization;
using TDengineDriver;



namespace Scada.Web
{
    /// <summary>
    /// WCF service that provides the client API
    /// <para>WCF-сервис, обеспечивающий работу клиентского API</para>
    /// </summary>
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class ClientApiSvc
    {
        /// <summary>
        /// Объект для передачи архивных данных и событий
        /// </summary>
        private class ArcDTO : DataTransferObject
        {
            /// <summary>
            /// Конструктор
            /// </summary>
            public ArcDTO()
                : base()
            {
                DataAge = null;
            }
            /// <summary>
            /// Конструктор
            /// </summary>
            public ArcDTO(object data, object dataAge)
                : base(data)
            {
                Data = data;
                DataAge = dataAge;
            }

            /// <summary>
            /// Получить или установить время изменения данных в источнике
            /// </summary>
            public object DataAge { get; set; }
        }

        /// <summary>
        /// Расширенные данные входого канала
        /// </summary>
        private class CnlDataExt
        {
            /// <summary>
            /// Конструктор
            /// </summary>
            public CnlDataExt(int cnlNum)
            {
                CnlNum = cnlNum;
                Val = 0.0;
                Stat = 0;
                Text = "";
                TextWithUnit = "";
                Color = "";
            }

            /// <summary>
            /// Получить номер входного канала
            /// </summary>
            public int CnlNum { get;  set; }
            /// <summary>
            /// Получить или установить значение
            /// </summary>
            public double Val { get; set; }
            /// <summary>
            /// Получить или установить статус
            /// </summary>
            public int Stat { get; set; }
            /// <summary>
            /// Получить или установить отображаемый текст
            /// </summary>
            public string Text { get; set; }
            /// <summary>
            /// Получить или установить отображаемый текст с размерностью
            /// </summary>
            public string TextWithUnit { get; set; }
            /// <summary>
            /// Получить или установить цвет
            /// </summary>
            public string Color { get; set; }
        }

        /// <summary>
        /// Часовые данные входных каналов
        /// </summary>
        private class HourCnlData
        {
            /// <summary>
            /// Конструктор
            /// </summary>
            public HourCnlData(double hour)
            {
                Hour = hour;
                Modified = false;
                CnlDataExtArr = null;
            }

            /// <summary>
            /// Получить час от начала суток
            /// </summary>
            public double Hour { get; private set; }
            /// <summary>
            /// Получить или установить признак изменения данных относительно метки времени в запросе
            /// </summary>
            public bool Modified { get; set; }
            /// <summary>
            /// Получить или установить массив расширенных данных входных каналов
            /// </summary>
            public CnlDataExt[] CnlDataExtArr { get; set; }
        }


        /// <summary>
        /// Максимальное количество символов в строке данных формата JSON, 10 МБ
        /// </summary>
        private const int MaxJsonLen = 10485760;
        /// <summary>
        /// Обеспечивает сериализацию результатов методов сервиса
        /// </summary>
        private static readonly JavaScriptSerializer JsSerializer = new JavaScriptSerializer() { MaxJsonLength = MaxJsonLen };
        /// <summary>
        /// Общие данные веб-приложения
        /// </summary>
        private static readonly AppData AppData = AppData.GetAppData();
        /// <summary>
        /// Обеспечивает форматирование данных входных каналов и событий
        /// </summary>
        private static DataFormatter DataFormatter = new DataFormatter();


        /// <summary>
        /// Получить представление из кэша с проверкой прав на него
        /// </summary>
        private BaseView GetViewFromCache(int viewID, UserRights userRights)
        {
            if (!userRights.GetUiObjRights(viewID).ViewRight)
                throw new ScadaException(CommonPhrases.NoRights);

            return AppData.ViewCache.GetViewFromCache(viewID, true);
        }

        /// <summary>
        /// Получить множество номеров каналов из условий запроса с проверкой прав
        /// </summary>
        private HashSet<int> GetCnlSet(string cnlNums, string viewIDs, int viewID, UserRights userRights)
        {
            if (!string.IsNullOrWhiteSpace(cnlNums))
            {
                if (!userRights.ViewAllRight)
                {
                    int[] cnlNumArr = ScadaUtils.ParseIntArray(cnlNums);
                    int[] viewIDArr = ScadaUtils.ParseIntArray(viewIDs);

                    if (!userRights.CheckInCnlRights(cnlNumArr, viewIDArr))
                        throw new ScadaException(CommonPhrases.NoRights);
                }

                return ScadaUtils.ParseIntSet(cnlNums);
            }
            else if (viewID > 0)
            {
                BaseView view = GetViewFromCache(viewID, userRights);
                return view.CnlSet;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 从权限验证的请求条件中获取通道号列表
        /// </summary>
        private IList<int> GetCnlList(string cnlNums, string viewIDs, int viewID, UserRights userRights)
        {
            if (!string.IsNullOrWhiteSpace(cnlNums))
            {
                int[] cnlNumArr = ScadaUtils.ParseIntArray(cnlNums);
                int[] viewIDArr = ScadaUtils.ParseIntArray(viewIDs);

                if (!userRights.CheckInCnlRights(cnlNumArr, viewIDArr))
                    throw new ScadaException(CommonPhrases.NoRights);

                return cnlNumArr;
            }
            else if (viewID > 0)
            {
                BaseView view = GetViewFromCache(viewID, userRights);
                return view.CnlList;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Преобразовать параметр запроса в массив 64-битных целых чисел
        ///将查询参数转换为 64 位整数数组
        /// </summary>
        private long[] QueryParamToLongArray(string param)
        {
            try
            {
                string[] elems = (param ?? "").Split(new char[] { ' ', ',' },
                    StringSplitOptions.RemoveEmptyEntries);
                int len = elems.Length;
                long[] arr = new long[len];

                for (int i = 0; i < len; i++)
                    arr[i] = long.Parse(elems[i]);

                return arr;
            }
            catch (FormatException ex)
            {
                throw new FormatException("Query parameter is not array of long integers.", ex);
            }
        }


        private CnlDataExt[] CreateCnlDataExtArrTest(IList<int> cnlList, SrezTableLight.Srez snapshot,
            bool dataVisible, string emptyVal)
        {
            DataAccess dataAccess = AppData.DataAccess;
            int cnlCnt = cnlList == null ? 0 : cnlList.Count;
            CnlDataExt[] cnlDataExtArr = new CnlDataExt[cnlCnt];

            for (int i = 0; i < cnlCnt; i++)
            {
                int cnlNum = cnlList[i];
                CnlDataExt cnlDataExt = new CnlDataExt(cnlNum);
                cnlDataExtArr[i] = cnlDataExt;

                if (dataVisible)
                {
                    double val;
                    int stat;
                    //snapshot.GetCnlData(cnlNum, out val, out stat); //这个 函数需要替换为 从数据局库拿数据

                    val = 1.0;
                    stat = 1;

                    if (!double.IsNaN(val))
                    {
                        cnlDataExt.Val = val;
                        cnlDataExt.Stat = stat;
                    }

                    InCnlProps cnlProps = dataAccess.GetCnlProps(cnlNum);
                    string text;
                    string textWithUnit;
                    DataFormatter.FormatCnlVal(val, stat, cnlProps, out text, out textWithUnit);

                    cnlDataExt.Text = text;
                    cnlDataExt.TextWithUnit = textWithUnit;
                    CnlStatProps cnlStatProps = dataAccess.GetCnlStatProps(stat);
                    cnlDataExt.Color = DataFormatter.GetCnlValColor(val, stat, cnlProps, cnlStatProps);
                }
                else
                {
                    cnlDataExt.Text = cnlDataExt.TextWithUnit = emptyVal;
                }
            }

            return cnlDataExtArr;
        }


        /// <summary>
        /// Создать и заполнить массив расширенных данных входных каналов
        ///创建并填充扩展输入通道数据数组
        /// </summary>
        private CnlDataExt[] CreateCnlDataExtArr(IList<int> cnlList, SrezTableLight.Srez snapshot, 
            bool dataVisible, string emptyVal)
        {
            DataAccess dataAccess = AppData.DataAccess;
            int cnlCnt = cnlList == null ? 0 : cnlList.Count;
            CnlDataExt[] cnlDataExtArr = new CnlDataExt[cnlCnt];

            for (int i = 0; i < cnlCnt; i++)
            {
                int cnlNum = cnlList[i];
                CnlDataExt cnlDataExt = new CnlDataExt(cnlNum);
                cnlDataExtArr[i] = cnlDataExt;

                if (dataVisible)
                {
                    double val;
                    int stat;
                    snapshot.GetCnlData(cnlNum, out val, out stat);

                    if (!double.IsNaN(val))
                    {
                        cnlDataExt.Val = val;
                        cnlDataExt.Stat = stat;
                    }

                    InCnlProps cnlProps = dataAccess.GetCnlProps(cnlNum);
                    string text;
                    string textWithUnit;
                    DataFormatter.FormatCnlVal(val, stat, cnlProps, out text, out textWithUnit);

                    cnlDataExt.Text = text;
                    cnlDataExt.TextWithUnit = textWithUnit;
                    CnlStatProps cnlStatProps = dataAccess.GetCnlStatProps(stat);
                    cnlDataExt.Color = DataFormatter.GetCnlValColor(val, stat, cnlProps, cnlStatProps);
                }
                else
                {
                    cnlDataExt.Text = cnlDataExt.TextWithUnit = emptyVal;
                }
            }

            return cnlDataExtArr;
        }

        /// <summary>
        /// Получить расширенные текущие данные входных каналов
        /// </summary>
        private CnlDataExt[] GetCurCnlDataExtArr(IList<int> cnlList)
        {
            DateTime dataAge;
            SrezTableLight.Srez snapshot = AppData.DataAccess.DataCache.GetCurSnapshot(out dataAge);

            string emptyVal;
            bool dataVisible = DataFormatter.CurDataVisible(dataAge, DateTime.Now, out emptyVal);

            return CreateCnlDataExtArr(cnlList, snapshot, dataVisible, emptyVal);
        }

        /// <summary>
        /// Добавить часовые данные в список
        /// 将每小时数据添加到列表中
        /// </summary>
        private void AppendHourCnlData(List<HourCnlData> hourCnlDataList, double hour, IList<int> cnlList,
            SrezTableLight.Srez snapshot, DateTime snapshotDT, DateTime nowDT)
        {
            HourCnlData hourCnlData = new HourCnlData(hour);
            hourCnlData.Modified = true;

            string emptyVal;
            bool dataVisible = DataFormatter.HourDataVisible(snapshotDT, nowDT, snapshot != null, out emptyVal);

            hourCnlData.CnlDataExtArr = CreateCnlDataExtArr(cnlList, snapshot, dataVisible, emptyVal);
            hourCnlDataList.Add(hourCnlData);
        }

        /// <summary>
        /// Добавить пустые часовые данные в список
        /// </summary>
        private void AppendEmptyHourCnlData(List<HourCnlData> hourCnlDataList, double hour)
        {
            hourCnlDataList.Add(new HourCnlData(hour));
        }


        /***********Guocj************/
        private static TDengineTest taosConnectHour = null;
        private static bool taosIsCntHour;
        private static TDengineTest taosConnect = null;
        private static bool taosIsCnt;


        class hourTableV
        {
            public DateTime     time;
            public CnlDataExt cnlDataExt;
        }

        class hourTableL
        {
            public int cnlNum;
            public List<hourTableV>  hourTableVs;
        }

        private void AppendHourCnlDataTest(List<HourCnlData> hourCnlDataList, double hour, IList<int> cnlList,
            SrezTableLight.Srez snapshot, DateTime snapshotDT, DateTime nowDT)
        {
            HourCnlData hourCnlData = new HourCnlData(hour);
            hourCnlData.Modified = true;

            string emptyVal;
            bool dataVisible = DataFormatter.HourDataVisible(snapshotDT, nowDT, snapshot != null, out emptyVal);  //这个函数 正常能调用
            hourCnlData.CnlDataExtArr = CreateCnlDataExtArrTest(cnlList, snapshot, dataVisible, emptyVal);
            hourCnlDataList.Add(hourCnlData);
        }



        private HourCnlData[] GetHourCnlDataArrTest(int year, int month, int day,
            int startHour, int endHour, IList<int> cnlList, bool existing, ref long[] dataAgeArr)
        {

            IntPtr rowdata;
            int queryRows = 0;

            if (startHour > endHour)
                throw new ArgumentException("Start hour must be less or equal than end hour.");

            DataAccess dataAccess = AppData.DataAccess;
            DataCache dataCache = dataAccess.DataCache;

            DateTime date = new DateTime(year, month, day);
            DateTime startDT = date.AddHours(startHour);
            DateTime startDate = startDT.Date;
            DateTime endDT = date.AddHours(endHour);
            DateTime endDate = endDT.Date;
            int hourCnt = endHour - startHour + 1;
            int dayCnt = (int)(endDate - startDate).TotalDays + 1;

            HourCnlData[] hourCnlDataList = new HourCnlData[hourCnt];
            long[] newDataAgeArr = new long[dayCnt];
            DateTime nowDT = DateTime.Now;
            DateTime curDate = startDate;

            double nowHours = (nowDT- startDT).TotalHours;


            int countN = cnlList == null ? 0 : cnlList.Count;
            hourTableL[] hourTableLs = new hourTableL[countN];
            HourCnlData hourCnlData;


            for (int i = 0; i < hourCnt; i++)
            {
                hourCnlData = new HourCnlData(startHour+i);
                hourCnlData.Modified = true;

                hourCnlData.CnlDataExtArr = new CnlDataExt[countN];
                for (int j = 0; j < countN; j++)
                {
                    hourCnlData.CnlDataExtArr[j] =  new CnlDataExt(cnlList[j]);
                    hourCnlData.CnlDataExtArr[j].Text = "";
                }

                hourCnlDataList[i]=hourCnlData;
            }



            try
            {
                if (taosConnectHour == null)
                {
                    Int16 ret = 0;
                    string[] strdd = new string[10];
                    taosConnectHour = new TDengineTest();
                    ret = taosConnectHour.taosConnet(strdd, "127.0.0.1");
                    if (ret == 0)
                    {
                        taosIsCntHour = true;
                    }
                    else
                    {
                        taosIsCntHour = false;
                        return null;
                    }
                }

                if (taosIsCntHour == true)
                {
                    for (int i = 0; i < cnlList.Count; i++)
                    {

                        IntPtr tdRes;
                        string cnlNumStr = "cnl" + cnlList[i].ToString();


                        tdRes = taosConnect.taosGetNumTimeData("scadadata", cnlNumStr, "  where  datetime>=" + "\"" + startDT.ToString("yyyy-MM-dd HH:mm:ss") + "\"" + " and datetime<" + "\"" + endDT.AddHours(1).ToString("yyyy-MM-dd HH:mm:ss") + "\"" + " interval(1h)   " + " order by datetime desc" );
                    

                        if ((tdRes == IntPtr.Zero) || (TDengine.ErrorNo(tdRes) != 0))
                        {
                           return null;
                        }
               
                        int fieldCount = TDengine.FieldCount(tdRes);
                        List<TDengineMeta> metas = TDengine.FetchFields(tdRes);

                        while ((rowdata = TDengine.FetchRows(tdRes)) != IntPtr.Zero)
                        {
                            int velRealNum = 0;
                            queryRows++;

                            float ValTmp = 0;
                            Int32 statTmp = 0; ;
                            DateTime timeTmp = nowDT;

                            velRealNum = 0;
                            for (int j = 0; j < fieldCount; j++)
                            {
                                TDengineMeta meta = (TDengineMeta)metas[j];
                                int offset = IntPtr.Size * j;

                                IntPtr data = Marshal.ReadIntPtr(rowdata, offset);

                                if (data == IntPtr.Zero)
                                {

                                    continue;
                                }

                                if (meta.name == "last(val)")
                                {
                                    ValTmp = (float)Marshal.PtrToStructure(data, typeof(float));
                                    velRealNum++;
                                }
                                else if (meta.name == "ts")
                                {
                                    timeTmp = timeStampToDateTime(Marshal.ReadInt64(data));
                                    velRealNum++;
                                }
                                else if (meta.name == "last(stat)")
                                {
                                    statTmp = Marshal.ReadInt32(data);
                                    velRealNum++;
                                }

                            }

                            if (velRealNum == 3)
                            {

                                double ldhours = (timeTmp - startDT).TotalHours;
                                if (ldhours > hourCnt)
                                {
                                    break;
                                }

                                hourCnlDataList[(int)ldhours].CnlDataExtArr[i].CnlNum = cnlList[i];
                                CnlDataExt tmpCnl = hourCnlDataList[(int)ldhours].CnlDataExtArr[i];

                                tmpCnl.Val = ValTmp;
                                tmpCnl.Stat = statTmp;

                                InCnlProps cnlProps = dataAccess.GetCnlProps(cnlList[i]);
                                string text;
                                string textWithUnit;
                                DataFormatter.FormatCnlVal(tmpCnl.Val, tmpCnl.Stat, cnlProps, out text, out textWithUnit);

                                tmpCnl.Text = text;
                                tmpCnl.TextWithUnit = textWithUnit;
                                CnlStatProps cnlStatProps = dataAccess.GetCnlStatProps(tmpCnl.Stat);
                                tmpCnl.Color = DataFormatter.GetCnlValColor(tmpCnl.Val, tmpCnl.Stat, cnlProps, cnlStatProps);

                            }

                        }
                    }


                    for (int hour = 0; hour <hourCnt; hour++)
                    {
                        if (nowHours > hour)
                        {
                            for (int j = 0; j < countN; j++)
                            {
                                if (hourCnlDataList[hour].CnlDataExtArr[j].Text == "")
                                {
                                    hourCnlDataList[hour].CnlDataExtArr[j].Text = "---";
                                    hourCnlDataList[hour].CnlDataExtArr[j].TextWithUnit = "---";
                                }
                            }
                        }
                        else if (nowHours == hour)
                        {
                            for (int j = 0; j < countN; j++)
                            {

                                hourCnlDataList[hour].CnlDataExtArr[j].Text = "*";
                                hourCnlDataList[hour].CnlDataExtArr[j].TextWithUnit = "*";

                            }
                        }
                    }

                }

            }
            catch (Exception ex)
            {

            }


            dataAgeArr = newDataAgeArr;
            return hourCnlDataList;
        }

        /***********************/

        /// <summary>
        /// Получить часовые данные входных каналов
        /// </summary>
        private HourCnlData[] GetHourCnlDataArr(int year, int month, int day, 
            int startHour, int endHour, IList<int> cnlList, bool existing, ref long[] dataAgeArr)
        {
            if (startHour > endHour)
                throw new ArgumentException("Start hour must be less or equal than end hour.");

            DataAccess dataAccess = AppData.DataAccess;
            DataCache dataCache = dataAccess.DataCache;

            DateTime date = new DateTime(year, month, day);
            DateTime startDT = date.AddHours(startHour);
            DateTime startDate = startDT.Date;
            DateTime endDT = date.AddHours(endHour);
            DateTime endDate = endDT.Date;
            int hourCnt = endHour - startHour + 1;
            int dayCnt = (int)(endDate - startDate).TotalDays + 1;

            List<HourCnlData> hourCnlDataList = new List<HourCnlData>(hourCnt);
            long[] newDataAgeArr = new long[dayCnt];
            DateTime nowDT = DateTime.Now;
            DateTime curDate = startDate;

            for (int dayInd = 0; dayInd < dayCnt; dayInd++)
            {
                SrezTableLight tblHour = dataCache.GetHourTable(curDate);
                long newDataAge = WebUtils.DateTimeToJs(tblHour.FileModTime);
                long prevDataAge = dayInd < dataAgeArr.Length ? dataAgeArr[dayInd] : 0;
                bool modified = prevDataAge <= 0 || prevDataAge < newDataAge;
                newDataAgeArr[dayInd] = newDataAge;
                DateTime nextDate = curDate.AddDays(1.0);

                if (existing)
                {
                    // получение всех существующих часовых срезов 获取所有现有的小时切片
                    DateTime dayStartDT = curDate > startDate ? curDate : startDT;
                    DateTime dayEndDT = curDate < endDate ? nextDate : endDT;

                    foreach (SrezTableLight.Srez snapshot in tblHour.SrezList.Values)
                    {
                        DateTime snapshotDT = snapshot.DateTime;
                        if (dayStartDT <= snapshotDT && snapshotDT <= dayEndDT)
                        {
                            double hour = (snapshotDT - date).TotalHours;
                            if (modified)
                                AppendHourCnlData(hourCnlDataList, hour, cnlList, snapshot, snapshotDT, nowDT);
                            else
                                AppendEmptyHourCnlData(hourCnlDataList, hour);
                        }
                    }
                }
                else
                {
                    // заполнение данных по целым часам
                    int dayStartHour = curDate > startDate ? 0 : startDT.Hour;
                    int dayEndHour = curDate < endDate ? 23 : endDT.Hour;

                    for (int dayHour = dayStartHour; dayHour <= dayEndHour; dayHour++)
                    {
                        DateTime snapshotDT = curDate.AddHours(dayHour);
                        SrezTableLight.Srez snapshot;
                        tblHour.SrezList.TryGetValue(snapshotDT, out snapshot);
                        double hour = (snapshotDT - date).TotalHours;
                        if (modified)
                            AppendHourCnlData(hourCnlDataList, hour, cnlList, snapshot, snapshotDT, nowDT);
                        else
                            AppendEmptyHourCnlData(hourCnlDataList, hour);
                    }
                }

                curDate = nextDate;
            }

            dataAgeArr = newDataAgeArr;
            return hourCnlDataList.ToArray();
        }

        /// <summary>
        /// Получить объект для передачи данных, содержащий информацию об ошибке, в формате JSON
        /// </summary>
        private string GetErrorDtoJs(Exception ex)
        {
            return JsSerializer.Serialize(new DataTransferObject(false, ex.Message));
        }


        private List<EventTableLight.Event>  GetEventTableTest(DateTime date,int startNum)
        {


            List<EventTableLight.Event> events = new List<EventTableLight.Event>();

            IntPtr rowdata;
            int queryRows = 0;

            try
            {
                if (taosConnect == null)
                {
                    Int16 ret = 0;
                    string[] strdd = new string[10];
                    taosConnect = new TDengineTest();
                    ret = taosConnect.taosConnet(strdd, "127.0.0.1");
                    if (ret == 0)
                    {
                        taosIsCnt = true;
                    }
                    else
                    {
                        taosIsCnt = false;
                        return null;
                    }
                }

                if (taosIsCnt == true)
                {
                    IntPtr tdRes = taosConnect.taosGetNumData("scadadata", "events", "  where  datetime<="+"\""+ date.ToString("yyyy-MM-dd HH:mm:ss")+"\"" + " and number>" + startNum.ToString()+" order by datetime desc limit 100;");

                    if ((tdRes == IntPtr.Zero) || (TDengine.ErrorNo(tdRes) != 0))
                    {
                        return null;
                    }
                    int fieldCount = TDengine.FieldCount(tdRes);
                    List<TDengineMeta> metas = TDengine.FetchFields(tdRes);

                    while ((rowdata = TDengine.FetchRows(tdRes)) != IntPtr.Zero)
                    {
                        queryRows++;
                        EventTableLight.Event eventSub = new EventTableLight.Event();

                        for (int i = 0; i < fieldCount; i++)
                        {
                            TDengineMeta meta = (TDengineMeta)metas[i];
                            int offset = IntPtr.Size * i;
                            

                            IntPtr data = Marshal.ReadIntPtr(rowdata, offset);

                            if (data == IntPtr.Zero)
                            {

                                continue;
                            }

                            if (meta.name == "cnlnum")
                            {
                                eventSub.CnlNum = Marshal.ReadInt32(data);
                            }
                            else if (meta.name == "datetime")
                            {
                                eventSub.DateTime = timeStampToDateTime(Marshal.ReadInt64(data));
                            }
                            else if (meta.name == "d")
                            {
                                eventSub.Checked = Marshal.ReadByte(data) == 0 ? false : true;
                            }
                            else if (meta.name == "data")
                            {
                                eventSub.Data = Marshal.PtrToStringAnsi(data);
                            }
                            else if (meta.name == "descr")
                            {
                                eventSub.Descr = Marshal.PtrToStringAnsi(data);
                            }
                            else if (meta.name == "kpnum")
                            {
                                eventSub.KPNum = Marshal.ReadInt32(data);
                            }
                            else if (meta.name == "newcnlstat")
                            {
                                eventSub.NewCnlStat = Marshal.ReadInt32(data);
                            }
                            else if (meta.name == "newcnlval")
                            {
                                eventSub.NewCnlVal = (float)Marshal.PtrToStructure(data, typeof(float));
                            }
                            else if (meta.name == "number")
                            {
                                eventSub.Number = Marshal.ReadInt32(data);
                            }
                            else if (meta.name == "objnum")
                            {
                                eventSub.ObjNum = Marshal.ReadInt32(data);
                            }
                            else if (meta.name == "oldcnlstat")
                            {
                                eventSub.OldCnlStat = Marshal.ReadInt32(data);
                            }
                            else if (meta.name == "oldcnlvel")
                            {
                                eventSub.OldCnlVal = (float)Marshal.PtrToStructure(data, typeof(float));
                            }
                            else if (meta.name == "paramid")
                            {
                                eventSub.ParamID = Marshal.ReadInt32(data);
                            }
                            else if (meta.name == "userid")
                            {
                                eventSub.UserID = Marshal.ReadInt32(data);
                            }

                        }
                        events.Add(eventSub);
                    }
                    return events;
                }

            }
            catch (Exception ex)
            {
                AppData.Log.WriteException(ex, "guocj db");
                return null;
            }

            return null;
        }

        public static DateTime timeStampToDateTime(Int64 javaTimeStamp)
        {
            // Java timestamp is milliseconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddMilliseconds(javaTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        /// <summary>
        /// Выполнить вход пользователя в систему
        /// </summary>
        /// <remarks>Для работы метода необходимо, чтобы существовал HTTP-контекст. 
        /// Возвращает bool, упакованный в DataTransferObject, в формате в JSON</remarks>
        [OperationContract]
        [WebGet]
        public string Login(string username, string password)
        {
            try
            {
                UserData userData = UserData.GetUserData();
                string errMsg;
                bool loggedOn = userData.Login(username, password, out errMsg);
                return JsSerializer.Serialize(new DataTransferObject(loggedOn));
            }
            catch (Exception ex)
            {
                AppData.Log.WriteException(ex, Localization.UseRussian ?
                    "Ошибка при выполнении входа пользователя в систему" :
                    "Error performing user login");
                return GetErrorDtoJs(ex);
            }
        }

        /// <summary>
        /// Проверить, что пользователь вошёл систему
        /// </summary>
        /// <remarks>Возвращает bool, упакованный в DataTransferObject, в формате в JSON</remarks>
        [OperationContract]
        [WebGet]
        public string CheckLoggedOn()
        {
            try
            {
                bool loggedOn = AppData.CheckLoggedOn(false);
                return JsSerializer.Serialize(new DataTransferObject(loggedOn));
            }
            catch (Exception ex)
            {
                AppData.Log.WriteException(ex, Localization.UseRussian ?
                    "Ошибка при проверке того, что пользователь вошел в систему" :
                    "Error checking that a user is logged on");
                return GetErrorDtoJs(ex);
            }
        }

        /// <summary>
        /// Получить текущие данные входного канала
        /// </summary>
        /// <remarks>Возвращает SrezTableLight.CnlData, упакованный в DataTransferObject, в формате в JSON</remarks>
        [OperationContract]
        [WebGet]
        public string GetCurCnlData(int cnlNum)
        {
            try
            {
                UserRights userRights;
                AppData.CheckLoggedOn(out userRights);

                if (!userRights.ViewAllRight)
                    throw new ScadaException(CommonPhrases.NoRights);

                SrezTableLight.CnlData cnlData = AppData.DataAccess.GetCurCnlData(cnlNum);
                return JsSerializer.Serialize(new DataTransferObject(cnlData));
            }
            catch (Exception ex)
            {
                AppData.Log.WriteException(ex, Localization.UseRussian ?
                    "Ошибка при получении текущих данных входного канала {0}" :
                    "Error getting current data of the input channel {0}", cnlNum);
                return GetErrorDtoJs(ex);
            }
        }

        /// <summary>
        /// Получить расширенные текущие данные по заданному фильтру
        /// </summary>
        /// <remarks>Возвращает CnlDataExt[], упакованный в DataTransferObject, в формате в JSON.
        /// Если задан фильтр по представлению, то оно должно быть уже загружено в кэш</remarks>
        [OperationContract]
        [WebGet]
        public string GetCurCnlDataExt(string cnlNums, string viewIDs, int viewID)
        {
            try
            {
                UserRights userRights;
                AppData.CheckLoggedOn(out userRights);

                IList<int> cnlList = GetCnlList(cnlNums, viewIDs, viewID, userRights);
                CnlDataExt[] cnlDataExtArr = GetCurCnlDataExtArr(cnlList);
                return JsSerializer.Serialize(new DataTransferObject(cnlDataExtArr));
            }
            catch (Exception ex)
            {
                AppData.Log.WriteException(ex, Localization.UseRussian ?
                    "Ошибка при получении расширенных текущих данных по фильтру, где каналы={0}, ид. представления={1}" :
                    "Error getting extended current data by the filter where channels={0}, view id={1}", cnlNums, viewID);
                return GetErrorDtoJs(ex);
            }
        }

        /// <summary>
        /// Получить часовые данные по заданному фильтру
        /// </summary>
        /// <remarks>Возвращает HourCnlDataExt[], упакованный в ArcDTO, в формате в JSON.
        /// Если задан фильтр по представлению, то оно должно быть уже загружено в кэш</remarks>



        [OperationContract]
        [WebGet]
        public string GetHourCnlData(int year, int month, int day, int startHour, int endHour, 
            string cnlNums, string viewIDs, int viewID, bool existing, string dataAge)
        {
            try
            {
                UserRights userRights;
                AppData.CheckLoggedOn(out userRights);

                IList<int> cnlList = GetCnlList(cnlNums, viewIDs, viewID, userRights);
                long[] dataAgeArr = QueryParamToLongArray(dataAge);
                HourCnlData[] hourCnlDataArr = 
                    GetHourCnlDataArrTest(year, month, day, startHour, endHour, cnlList, existing, ref dataAgeArr);
                return JsSerializer.Serialize(new ArcDTO(hourCnlDataArr, dataAgeArr));
            }
            catch (Exception ex)
            {
                AppData.Log.WriteException(ex, Localization.UseRussian ?
                    "Ошибка при получении часовых данных по фильтру, где каналы={0}, ид. представления={1}" :
                    "Error getting hourly data by the filter where channels={0}, view id={1}", cnlNums, viewID);
                return GetErrorDtoJs(ex);
            }
        }

        /// <summary>
        /// Получить события по заданному фильтру
        /// </summary>
        /// <remarks>Возвращает DispEventProps[], упакованный в ArcDTO, в формате в JSON.
        /// Если задан фильтр по представлению, то оно должно быть уже загружено в кэш</remarks>
        [OperationContract]
        [WebGet]
        public string GetEvents(int year, int month, int day, string cnlNums, string viewIDs, int viewID,
            int lastCount, int startEvNum, long dataAge)
        {

            try
            {
        
               UserRights userRights;
               AppData.CheckLoggedOn(out userRights);
 
               // создание фильтра событий
               HashSet<int> cnlSet = GetCnlSet(cnlNums, viewIDs, viewID, userRights);
           
                EventTableLight.EventFilter eventFilter = cnlSet == null ?
                new EventTableLight.EventFilter(EventTableLight.EventFilters.None) :
                new EventTableLight.EventFilter(EventTableLight.EventFilters.Cnls);
              
                eventFilter.CnlNums = cnlSet;

                // получение событий
                DataAccess dataAccess = AppData.DataAccess;
                DateTime date = new DateTime(year, month, day);

                EventTableLight tblEvent = dataAccess.DataCache.GetEventTable(date);

                  long newDataAge = WebUtils.DateTimeToJs(tblEvent.FileModTime);
                  DispEvent[] eventsToSend;

                  if (tblEvent.FileModTime > DateTime.MinValue && dataAge < newDataAge)
                  {
                  // применение фильтра событий
                  bool reversed;
                  List<EventTableLight.Event> events =
                  tblEvent.GetFilteredEvents(eventFilter, lastCount, startEvNum, out reversed);

                  // преобразование событий для передачи
                  int evCnt = events.Count;
                  eventsToSend = new DispEvent[evCnt];
                  if (reversed)
                  {
                  for (int i = 0, j = evCnt - 1; i < evCnt; i++, j--)
                  eventsToSend[i] = dataAccess.GetDispEvent(events[j], DataFormatter);
                  }
                  else
                  {
                  for (int i = 0; i < evCnt; i++)
                  eventsToSend[i] = dataAccess.GetDispEvent(events[i], DataFormatter);
                  }
                  }
                  else
                  {
                  eventsToSend = new DispEvent[0];
                  }

                  return JsSerializer.Serialize(new ArcDTO(eventsToSend, newDataAge));     
            }
            catch (Exception ex)
            {

                AppData.Log.WriteException(ex, Localization.UseRussian ?
                    "Ошибка при получении событий по фильтру, где каналы={0}, ид. представления={1}" :
                    "Error getting events by the filter where channels={0}, view id={1}", cnlNums, viewID);
                return GetErrorDtoJs(ex);
            }

        }

        /// <summary>
        /// Получить метку представления из кэша
        /// </summary>
        /// <remarks>Возвращает long, упакованный в DataTransferObject, в формате в JSON</remarks>
        [OperationContract]
        [WebGet]
        public string GetViewStamp(int viewID)
        {
            try
            {
                AppData.CheckLoggedOn();
                BaseView view = AppData.ViewCache.GetViewFromCache(viewID);
                long stamp = view == null ? 0 : view.Stamp;
                return JsSerializer.Serialize(new DataTransferObject(stamp));
            }
            catch (Exception ex)
            {
                AppData.Log.WriteException(ex, Localization.UseRussian ?
                    "Ошибка при получении метки предсталения с ид.={0} из кэша" :
                    "Error getting stamp of the view with ID={0} from the cache", viewID);
                return GetErrorDtoJs(ex);
            }
        }

        /// <summary>
        /// Преобразовать строку в дату
        /// </summary>
        /// <remarks>Возвращает long или null упакованный в DataTransferObject, в формате в JSON.
        /// Число означает количество миллисекунд для создания даты в Javascript или 0 в случае ошибки</remarks>
        [OperationContract]
        [WebGet]
        public string ParseDateTime(string s)
        {
            try
            {
                AppData.CheckLoggedOn();
                DateTime dateTime;
                object data = ScadaUtils.TryParseDateTime(s, out dateTime) ? 
                    (object)WebUtils.DateTimeToJs(dateTime) : null;
                return JsSerializer.Serialize(new DataTransferObject(data));
            }
            catch (Exception ex)
            {
                AppData.Log.WriteException(ex, Localization.UseRussian ?
                    "Ошибка при преобразовани строки в число" :
                    "Error parsing date and time");
                return GetErrorDtoJs(ex);
            }
        }


        /// <summary>
        /// Пересоздать объект для форматирования
        /// </summary>
        /// <remarks>Необходимо вызвать при смене культуры веб-приложения</remarks>
        public static void RefreshDataFormatter()
        {
            DataFormatter = new DataFormatter();
        }
    }
}