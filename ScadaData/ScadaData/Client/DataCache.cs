﻿/*
 * Copyright 2017 Mikhail Shiryaev
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
 * Module   : ScadaData
 * Summary  : Cache of the data received from SCADA-Server for clients usage
 * 
 * Author   : Mikhail Shiryaev
 * Created  : 2016
 * Modified : 2017
 */

using Scada.Data.Configuration;
using Scada.Data.Models;
using Scada.Data.Tables;
using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.InteropServices;
using System.Threading;

using Utils;
using TDengineDriver;

namespace Scada.Client
{
    /// <summary>
    /// Cache of the data received from SCADA-Server for clients usage
    /// <para>Кэш данных, полученных от SCADA-Сервера, для использования клиентами</para>
    /// </summary>
    /// <remarks>All the returned data are not thread safe
    /// <para>Все возвращаемые данные не являются потокобезопасными</para></remarks>
    public class DataCache
    {
        /// <summary>
        /// Вместимость кэша таблиц часовых срезов
        /// </summary>
        protected const int HourCacheCapacity = 100;
        /// <summary>
        /// Вместимость кэша таблиц событий
        /// </summary>
        protected const int EventCacheCapacity = 100;

        /// <summary>
        /// Период хранения таблиц часовых срезов в кэше с момента последнего доступа
        /// </summary>
        protected static readonly TimeSpan HourCacheStorePeriod = TimeSpan.FromMinutes(10);
        /// <summary>
        /// Период хранения таблиц событий в кэше с момента последнего доступа
        /// </summary>
        protected static readonly TimeSpan EventCacheStorePeriod = TimeSpan.FromMinutes(10);
        /// <summary>
        /// Время актуальности таблиц базы конфигурации
        /// </summary>
        protected static readonly TimeSpan BaseValidSpan = TimeSpan.FromSeconds(5);
        /// <summary>
        /// Время актуальности текущих и архивных данных
        /// </summary>
        protected static readonly TimeSpan DataValidSpan = TimeSpan.FromMilliseconds(500);
        /// <summary>
        /// Время ожидания снятия блокировки базы конфигурации
        /// </summary>
        protected static readonly TimeSpan WaitBaseLock = TimeSpan.FromSeconds(5);
        /// <summary>
        /// Разделитель значений внутри поля таблицы
        /// </summary>
        protected static readonly char[] FieldSeparator = new char[] { ';' };


        /// <summary>
        /// Объект для обмена данными со SCADA-Сервером
        /// </summary>
        protected readonly ServerComm serverComm;
        /// <summary>
        /// Журнал
        /// </summary>
        protected readonly Log log;

        /// <summary>
        /// Объект для синхронизации доступа к таблицам базы конфигурации
        /// </summary>
        protected readonly object baseLock;
        /// <summary>
        /// Объект для синхронизации достапа к текущим данным
        /// </summary>
        protected readonly object curDataLock;

        /// <summary>
        /// Время последего успешного обновления таблиц базы конфигурации
        /// </summary>
        protected DateTime baseRefrDT;
        /// <summary>
        /// Таблица текущего среза
        /// </summary>
        protected SrezTableLight tblCur;
        /// <summary>
        /// Время последнего успешного обновления таблицы текущего среза
        /// </summary>
        protected DateTime curDataRefrDT;


        /// <summary>
        /// Конструктор, ограничивающий создание объекта без параметров
        /// </summary>
        protected DataCache()
        {
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        public DataCache(ServerComm serverComm, Log log)
        {
            if (serverComm == null)
                throw new ArgumentNullException("serverComm");
            if (log == null)
                throw new ArgumentNullException("log");

            this.serverComm = serverComm;
            this.log = log;

            baseLock = new object();
            curDataLock = new object();

            baseRefrDT = DateTime.MinValue;
            tblCur = new SrezTableLight();
            curDataRefrDT = DateTime.MinValue;

            BaseTables = new BaseTables();
            CnlProps = new InCnlProps[0];
            CtrlCnlProps = new CtrlCnlProps[0];
            CnlStatProps = new SortedList<int, CnlStatProps>();
            HourTableCache = new Cache<DateTime, SrezTableLight>(HourCacheStorePeriod, HourCacheCapacity);
            EventTableCache = new Cache<DateTime, EventTableLight>(EventCacheStorePeriod, EventCacheCapacity);
        }


        /// <summary>
        /// Получить таблицы базы конфигурации
        /// </summary>
        /// <remarks>При обновлении объект таблиц пересоздаётся, обеспечивая целостность.
        /// Таблицы после загрузки не изменяются экземпляром данного класса и не должны изменяться извне,
        /// таким образом, чтение данных из таблиц является потокобезопасным.
        /// Однако, при использовании DataTable.DefaultView небходимо синхронизировать доступ к таблицам 
        /// с помощью вызова lock (BaseTables.SyncRoot)</remarks>
        public BaseTables BaseTables { get; protected set; }

        /// <summary>
        /// Получить свойства входных каналов, упорядоченные по возрастанию номеров каналов
        /// </summary>
        /// <remarks>Массив пересоздаётся после обновления таблиц базы конфигурации.
        /// Массив после инициализации не изменяется экземпляром данного класса и не должен изменяться извне,
        /// таким образом, чтение его данных является потокобезопасным
        /// </remarks>
        public InCnlProps[] CnlProps { get; protected set; }

        /// <summary>
        /// Получить свойства каналов управления, упорядоченные по возрастанию номеров каналов
        /// </summary>
        /// <remarks>Массив пересоздаётся после обновления таблиц базы конфигурации.
        /// Массив после инициализации не изменяется экземпляром данного класса и не должен изменяться извне,
        /// таким образом, чтение его данных является потокобезопасным
        /// </remarks>
        public CtrlCnlProps[] CtrlCnlProps { get; protected set; }

        /// <summary>
        /// Получить свойства статусов входных каналов
        /// </summary>
        /// <remarks>Список пересоздаётся после обновления таблиц базы конфигурации.
        /// Список после инициализации не изменяется экземпляром данного класса и не должен изменяться извне,
        /// таким образом, чтение его данных является потокобезопасным
        /// </remarks>
        public SortedList<int, CnlStatProps> CnlStatProps { get; protected set; }

        /// <summary>
        /// Получить кэш таблиц часовых срезов
        /// </summary>
        /// <remarks>Использовать вне данного класса только для получения состояния кэша</remarks>
        public Cache<DateTime, SrezTableLight> HourTableCache { get; protected set; }

        /// <summary>
        /// Получить кэш таблиц событий
        /// </summary>
        /// <remarks>Использовать вне данного класса только для получения состояния кэша</remarks>
        public Cache<DateTime, EventTableLight> EventTableCache { get; protected set; }


        /// <summary>
        /// Заполнить свойства входных каналов
        /// </summary>
        protected void FillCnlProps()
        {
            try
            {
                log.WriteAction(Localization.UseRussian ?
                    "Заполнение свойств входных каналов" :
                    "Fill input channels properties");

                ConfDAO confDAO = new ConfDAO(BaseTables);
                List<InCnlProps> cnlPropsList = confDAO.GetInCnlProps();
                CnlProps = cnlPropsList.ToArray();
            }
            catch (Exception ex)
            {
                log.WriteException(ex, (Localization.UseRussian ? 
                    "Ошибка при заполнении свойств входных каналов: " :
                    "Error filling input channels properties"));
            }
        }

        /// <summary>
        /// Заполнить свойства каналов управления
        /// </summary>
        protected void FillCtrlCnlProps()
        {
            try
            {
                log.WriteAction(Localization.UseRussian ?
                    "Заполнение свойств каналов управления" :
                    "Fill output channels properties");

                ConfDAO confDAO = new ConfDAO(BaseTables);
                List<CtrlCnlProps> ctrlCnlPropsList = confDAO.GetCtrlCnlProps();
                CtrlCnlProps = ctrlCnlPropsList.ToArray();
            }
            catch (Exception ex)
            {
                log.WriteException(ex, (Localization.UseRussian ?
                    "Ошибка при заполнении свойств каналов управления: " :
                    "Error filling output channels properties"));
            }
        }

        /// <summary>
        /// Заполнить свойства статусов входных каналов
        /// </summary>
        protected void FillCnlStatProps()
        {
            try
            {
                log.WriteAction(Localization.UseRussian ?
                    "Заполнение свойств статусов входных каналов" :
                    "Fill input channel statuses properties");

                ConfDAO confDAO = new ConfDAO(BaseTables);
                CnlStatProps = confDAO.GetCnlStatProps();
            }
            catch (Exception ex)
            {
                log.WriteException(ex, (Localization.UseRussian ?
                    "Ошибка при заполнении свойств статусов входных каналов: " :
                    "Error filling input channel statuses properties"));
            }
        }

        /// <summary>
        /// Обновить текущие данные
        ///更新当前数据
        /// </summary>
        protected void RefreshCurData()
        {
            try
            {
                DateTime utcNowDT = DateTime.UtcNow;
                if (utcNowDT - curDataRefrDT > DataValidSpan) // данные устарели 数据已过时
                {
                    curDataRefrDT = utcNowDT;
                    DateTime newCurTableAge = serverComm.ReceiveFileAge(ServerComm.Dirs.Cur, SrezAdapter.CurTableName);

                    if (newCurTableAge == DateTime.MinValue) // файл среза не существует или нет связи с сервером 切片文件不存在或与服务器没有连接
                    {
                        tblCur.Clear();
                        tblCur.FileModTime = DateTime.MinValue;
                        log.WriteError(Localization.UseRussian ?
                            "Не удалось принять время изменения файла текущих данных." :
                            "Unable to receive the current data file modification time.");
                    }
                    else if (tblCur.FileModTime != newCurTableAge) // файл среза изменён 切片文件已更改
                    {
                        if (serverComm.ReceiveSrezTable(SrezAdapter.CurTableName, tblCur))
                        {
                            tblCur.FileModTime = newCurTableAge;
                            tblCur.LastFillTime = utcNowDT;
                        }
                        else
                        {
                            tblCur.FileModTime = DateTime.MinValue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                tblCur.FileModTime = DateTime.MinValue;
                log.WriteException(ex, Localization.UseRussian ?
                    "Ошибка при обновлении текущих данных" :
                    "Error refreshing the current data");
            }
        }


        /// <summary>
        /// Обновить таблицы базы конфигурации, свойства каналов и статусов
        /// </summary>
        public void RefreshBaseTables()
        {
            lock (baseLock)
            {
                try
                {
                    DateTime utcNowDT = DateTime.UtcNow;

                    if (utcNowDT - baseRefrDT > BaseValidSpan) // данные устарели
                    {
                        baseRefrDT = utcNowDT;
                        DateTime newBaseAge = serverComm.ReceiveFileAge(ServerComm.Dirs.BaseDAT,
                            BaseTables.GetFileName(BaseTables.InCnlTable));

                        if (newBaseAge == DateTime.MinValue) // база конфигурации не существует или нет связи с сервером
                        {
                            throw new ScadaException(Localization.UseRussian ?
                                "Не удалось принять время изменения базы конфигурации." :
                                "Unable to receive the configuration database modification time.");
                        }
                        else if (BaseTables.BaseAge != newBaseAge) // база конфигурации изменена
                        {
                            log.WriteAction(Localization.UseRussian ? 
                                "Обновление таблиц базы конфигурации" :
                                "Refresh the tables of the configuration database");

                            // ожидание снятия возможной блокировки базы конфигурации
                            DateTime t0 = utcNowDT;
                            while (serverComm.ReceiveFileAge(ServerComm.Dirs.BaseDAT, "baselock") > DateTime.MinValue &&
                                DateTime.UtcNow - t0 <= WaitBaseLock)
                            {
                                Thread.Sleep(ScadaUtils.ThreadDelay);
                            }

                            // загрузка данных в таблицы
                            BaseTables newBaseTables = new BaseTables() { BaseAge = newBaseAge };
                            foreach (DataTable dataTable in newBaseTables.AllTables)
                            {
                                string tableName = BaseTables.GetFileName(dataTable);

                                if (!serverComm.ReceiveBaseTable(tableName, dataTable))
                                {
                                    throw new ScadaException(string.Format(Localization.UseRussian ?
                                        "Не удалось принять таблицу {0}" :
                                        "Unable to receive the table {0}", tableName));
                                }
                            }
                            BaseTables = newBaseTables;

                            // заполнение свойств каналов и статусов
                            lock (BaseTables.SyncRoot)
                            {
                                FillCnlProps();
                                FillCtrlCnlProps();
                                FillCnlStatProps();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    BaseTables.BaseAge = DateTime.MinValue;
                    log.WriteException(ex, Localization.UseRussian ?
                        "Ошибка при обновлении таблиц базы конфигурации" :
                        "Error refreshing the tables of the configuration database");
                }
            }
        }

        /// <summary>
        /// Получить текущий срез из кэша или от сервера 从缓存或服务器获取当前切片
        /// </summary>
        /// <remarks>Возвращаемый срез после загрузки не изменяется экземпляром данного класса,
        /// таким образом, чтение его данных является потокобезопасным</remarks>
        public SrezTableLight.Srez GetCurSnapshot(out DateTime dataAge)
        {
            lock (curDataLock)
            {
                try
                {
                    RefreshCurData();
                    dataAge = tblCur.FileModTime;
                    return tblCur.SrezList.Count > 0 ? tblCur.SrezList.Values[0] : null;
                }
                catch (Exception ex)
                {
                    log.WriteException(ex, Localization.UseRussian ?
                        "Ошибка при получении текущего среза из кэша или от сервера" :
                        "Error getting the current snapshot the cache or from the server");
                    dataAge = DateTime.MinValue;
                    return null;
                }
            }
        }

        /// <summary>
        /// Получить таблицу часовых данных за сутки из кэша или от сервера
        /// 从缓存或服务器获取每天每小时的数据表
        /// </summary>
        /// <remarks>Возвращаемая таблица после загрузки не изменяется экземпляром данного класса,
        /// таким образом, чтение её данных является потокобезопасным. 
        /// Метод всегда возвращает объект, не равный null 加载后返回的表不会被该类的实例修改，因此读取其数据是线程安全的。 该方法总是返回一个非空对象</remarks>
        public SrezTableLight GetHourTable(DateTime date)
        {
            try
            {
                // получение таблицы часовых срезов из кэша 从缓存中获取小时切片表
                date = date.Date;
                DateTime utcNowDT = DateTime.UtcNow;
                Cache<DateTime, SrezTableLight>.CacheItem  cacheItem = HourTableCache.GetOrCreateItem(date, utcNowDT);

                // блокировка доступа только к одной таблице часовых срезов 阻止仅访问一张小时切片表
                lock (cacheItem)
                {
                    SrezTableLight table = cacheItem.Value; // таблица, которую необходимо получить 表得到
                    DateTime tableAge = cacheItem.ValueAge; // время изменения файла таблицы 表文件修改时间
                    bool tableIsNotValid = utcNowDT - cacheItem.ValueRefrDT > DataValidSpan; // таблица могла устареть 表可能已过时

                    // получение таблицы часовых срезов от сервера 从服务器获取小时切片表
                    if (table == null || tableIsNotValid)
                    {
                        string tableName = SrezAdapter.BuildHourTableName(date);
                        DateTime newTableAge = serverComm.ReceiveFileAge(ServerComm.Dirs.Hour, tableName);

                        if (newTableAge == DateTime.MinValue) // файл таблицы не существует или нет связи с сервером 表文件不存在或与服务器没有连接
                        {
                            table = null;
                            // не засорять лог
                            /*log.WriteError(string.Format(Localization.UseRussian ?
                                "Не удалось принять время изменения таблицы часовых данных {0}" :
                                "Unable to receive modification time of the hourly data table {0}", tableName));*/
                        }
                        else if (newTableAge != tableAge) // файл таблицы изменён 表文件已更改
                        {
                            table = new SrezTableLight();
                            if (serverComm.ReceiveSrezTable(tableName, table))
                            {
                                table.FileModTime = newTableAge;
                                table.LastFillTime = utcNowDT;
                            }
                            else
                            {
                                throw new ScadaException(Localization.UseRussian ?
                                    "Не удалось принять таблицу часовых срезов." :
                                    "Unable to receive hourly data table.");
                            }
                        }

                        if (table == null)
                            table = new SrezTableLight();

                        // обновление таблицы в кэше
                        HourTableCache.UpdateItem(cacheItem, table, newTableAge, utcNowDT);
                    }

                    return table;
                }
            }
            catch (Exception ex)
            {
                log.WriteException(ex, Localization.UseRussian ?
                    "Ошибка при получении таблицы часовых данных за {0} из кэша или от сервера" :
                    "Error getting hourly data table for {0} from the cache or from the server", 
                    date.ToLocalizedDateString());
                return new SrezTableLight();
            }
        }

        /// <summary>
        /// Получить таблицу событий за сутки из кэша или от сервера
        /// </summary>
        /// <remarks>Возвращаемая таблица после загрузки не изменяется экземпляром данного класса,
        /// таким образом, чтение её данных является потокобезопасным.
        /// Метод всегда возвращает объект, не равный null</remarks>
        public EventTableLight GetEventTable(DateTime date)
        {

            try
            {

                // получение таблицы событий из кэша 从缓存中获取事件表
                date = date.Date;
                DateTime utcNowDT = DateTime.UtcNow;

                Cache<DateTime, EventTableLight>.CacheItem cacheItem = EventTableCache.GetOrCreateItem(date, utcNowDT);

                // блокировка доступа только к одной таблице событий 阻止仅访问一个事件表
                lock (cacheItem)
                {
                    EventTableLight table = cacheItem.Value; // таблица, которую необходимо получить
                    DateTime tableAge = cacheItem.ValueAge;  // время изменения файла таблицы 表文件修改时间
                    bool tableIsNotValid = utcNowDT - cacheItem.ValueRefrDT > DataValidSpan; // таблица могла устареть 表可能已过时

                    // получение таблицы событий от сервера
                    if (table == null || tableIsNotValid)
                    {
         
                        string tableName = EventAdapter.BuildEvTableName(date);
                        DateTime newTableAge = serverComm.ReceiveFileAge(ServerComm.Dirs.Events, tableName);

                        if (newTableAge == DateTime.MinValue) // файл таблицы не существует или нет связи с сервером
                        {
                            table = null;
                            // не засорять лог
                            /*log.WriteError(string.Format(Localization.UseRussian ?
                                "Не удалось принять время изменения таблицы событий {0}" :
                                "Unable to receive modification time of the event table {0}", tableName));*/
                        }
                        else if (newTableAge != tableAge) // файл таблицы изменён
                        {
                            table = new EventTableLight();
                            if (serverComm.ReceiveEventTable(tableName, table))
                            {
                                table.FileModTime = newTableAge;
                                table.LastFillTime = utcNowDT;
                            }
                            else
                            {
                                throw new ScadaException(Localization.UseRussian ?
                                    "Не удалось принять таблицу событий." :
                                    "Unable to receive event table.");
                            }
                        }
           
                        if (table == null)
                            table = new EventTableLight();

                        // обновление таблицы в кэше
                        EventTableCache.UpdateItem(cacheItem, table, newTableAge, utcNowDT);
                    }

                    return table;
                }


            }
            catch (Exception ex)
            {
                log.WriteException(ex, Localization.UseRussian ?
                    "Ошибка при получении таблицы событий за {0} из кэша или от сервера" :
                    "Error getting event table for {0} from the cache or from the server",
                    date.ToLocalizedDateString());
                return new EventTableLight();
            }


        }

        /// <summary>
        /// Получить тренд минутных данных заданного канала за сутки
        /// </summary>
        /// <remarks>Возвращаемый тренд после загрузки не изменяется экземпляром данного класса,
        /// таким образом, чтение его данных является потокобезопасным.
        /// Метод всегда возвращает объект, не равный null</remarks>
        /// guocj ++ 这里是 chart 获取数据的接口点  需要 更改接口的话 就需要把这一个函数重写
        public Trend GetMinTrendOld(DateTime date, int cnlNum)
        {
            Trend trend = new Trend(cnlNum);

            try
            {
                if (serverComm.ReceiveTrend(SrezAdapter.BuildMinTableName(date), date, trend))
                {
                    trend.LastFillTime = DateTime.UtcNow; // единообразно с часовыми данными и событиями
                }
                else
                {
                    throw new ScadaException(Localization.UseRussian ?
                        "Не удалось принять тренд." :
                        "Unable to receive trend.");
                }
            }
            catch (Exception ex)
            {
                log.WriteException(ex, Localization.UseRussian ?
                    "Ошибка при получении тренда минутных данных за {0}" :
                    "Error getting minute data trend for {0}", date.ToLocalizedDateString());
            }

            return trend;
        }

        /// <summary>
        /// </summary>
        /// <remarks> </remarks>
        /// guocj ++ 这里是 chart 获取数据的接口点  需要 更改接口的话 就需要把这一个函数重写
        /// 
        public static DateTime timeStampToDateTime(Int64 javaTimeStamp)
        {
            // Java timestamp is milliseconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddMilliseconds(javaTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        private static TDengineTest taosConnect = null;
        private static bool taosIsCnt;
        /// <summary>
        /// </summary>
        /// <remarks> </remarks>
        /// guocj ++ 这里是 chart 获取数据的接口点  需要 更改接口的话 就需要把这一个函数重写
        /// 
        public Trend GetMinTrend(DateTime date, int cnlNum)
        {
            Trend trend = new Trend(cnlNum);


            IntPtr rowdata;
            int queryRows = 0;

            string cnlName = "cnl" + cnlNum.ToString();


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
                    IntPtr tdRes = taosConnect.taosGetNumTimeData("scadadata", cnlName, "  where  datetime>=" + "\"" + date.ToString("yyyy-MM-dd HH:mm:ss") + "\"" + " interval(1m)   " + " limit 1440;");

                    if ((tdRes == IntPtr.Zero) || (TDengine.ErrorNo(tdRes) != 0))
                    {
                        return null;
                    }
                    int fieldCount = TDengine.FieldCount(tdRes);
                    List<TDengineMeta> metas = TDengine.FetchFields(tdRes);

           

                    while ((rowdata = TDengine.FetchRows(tdRes)) != IntPtr.Zero)
                    {
                        queryRows++;

                        Trend.Point point = new Trend.Point(timeStampToDateTime(0), 0, 0);


                        for (int i = 0; i < fieldCount; i++)
                        {
                            TDengineMeta meta = (TDengineMeta)metas[i];
                            int offset = IntPtr.Size * i;


                            IntPtr data = Marshal.ReadIntPtr(rowdata, offset);

                            if (data == IntPtr.Zero)
                            {

                                continue;
                            }

                            if (meta.name == "last(val)")
                            {
                                point.Val = (float)Marshal.PtrToStructure(data, typeof(float));
                            }
                            else if (meta.name == "ts")
                            {
                                point.DateTime = timeStampToDateTime(Marshal.ReadInt64(data));
                            }
                            else if (meta.name == "last(stat)")
                            {
                                point.Stat = Marshal.ReadInt32(data);
                            }


                        }
                        //LogHelpter.AddLog("fgh: " + point.DateTime.ToString() +"  " + point.Val.ToString());
                        trend.Points.Add(point);
                    }
                    trend.Sort();
                    return trend;
                }

            }
            catch (Exception ex)
            {
                return null;
            }

            return trend;
        }


    }
}