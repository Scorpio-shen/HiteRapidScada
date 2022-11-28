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
 * Module   : ScadaCommCommon
 * Summary  : The base class for device communication logic. Nested types
 * 
 * Author   : Mikhail Shiryaev
 * Created  : 2006
 * Modified : 2019
 */

using Scada.Data.Tables;
using System;
using System.Collections.Generic;

namespace Scada.Comm.Devices
{
    partial class KPLogic
    {
        /// <summary>
        /// CPU统计
        /// </summary>
        public struct KPStats
        {
            /// <summary>
            /// 获取或设置轮询会话数
            /// </summary>
            public int SessCnt { get; set; }
            /// <summary>
            /// 获取或设置失败的轮询会话数
            /// </summary>
            public int SessErrCnt { get; set; }
            /// <summary>
            /// 获取或设置命令数
            /// </summary>
            public int CmdCnt { get; set; }
            /// <summary>
            /// 获取或设置失败的命令数
            /// </summary>
            public int CmdErrCnt { get; set; }
            /// <summary>
            /// 接收或设置请求数
            /// </summary>
            public int ReqCnt { get; set; }
            /// <summary>
            /// 获取或设置失败请求数
            /// </summary>
            public int ReqErrCnt { get; set; }

            /// <summary>
            /// 重置统计数据
            /// </summary>
            public void Reset()
            {
                SessCnt = 0;
                SessErrCnt = 0;
                CmdCnt = 0;
                CmdErrCnt = 0;
                ReqCnt = 0;
                ReqErrCnt = 0;
            }
        }

        /// <summary>
        /// Tag
        /// </summary>
        public class KPTag
        {
            /// <summary>
            /// Конструктор
            /// </summary>
            public KPTag()
                : this(0, "")
            {
            }
            /// <summary>
            /// Конструктор
            /// </summary>
            public KPTag(int signal, string name)
            {
                Signal = signal;
                Index = -1;
                Name = name;
                CnlNum = 0;
                ObjNum = 0;
                ParamID = 0;
                Aux = null;
            }

            /// <summary>
            /// 获取或设置信号（标签号）
            /// </summary>
            public int Signal { get; set; }
            /// <summary>
            /// Gets or sets the tag index.
            /// </summary>
            public int Index { get; set; }
            /// <summary>
            /// Получить или установить наименование
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// 获取或设置与标记关联的配置数据库输入通道号
            /// </summary>
            public int CnlNum { get; set; }
            /// <summary>
            /// 获取或设置输入通道对象编号
            /// </summary>
            public int ObjNum { get; set; }
            /// <summary>
            /// 获取或设置输入通道参数 ID
            /// </summary>
            /// <remarks>Необходим для событий КП</remarks>
            public int ParamID { get; set; }
            /// <summary>
            /// 获取或设置包含有关标记的数据的辅助对象。
            /// </summary>
            public object Aux { get; set; }
        }

        /// <summary>
        /// Группа тегов КП
        /// </summary>
        public class TagGroup
        {
            /// <summary>
            /// Конструктор
            /// </summary>
            public TagGroup()
                : this("")
            {
            }
            /// <summary>
            /// Конструктор
            /// </summary>
            public TagGroup(string name)
            {
                Name = name;
                KPTags = new List<KPTag>();
            }

            /// <summary>
            /// Получить наименование группы
            /// </summary>
            public string Name { get; protected set; }
            /// <summary>
            /// Получить список тегов КП, входящих в группу
            /// </summary>
            public List<KPTag> KPTags { get; protected set; }

            /// <summary>
            /// Creates and adds a new tag to the group.
            /// </summary>
            public KPTag AddNewTag(int signal, string name, object aux = null)
            {
                KPTag kpTag = new KPTag(signal, name) { Aux = aux };
                KPTags.Add(kpTag);
                return kpTag;
            }
        }

        /// <summary>
        /// 特定时间点的标签数据切片
        /// </summary>
        /// <remarks>标签切片的特点是使用KP标签而不是输入通道</remarks>
        public class TagSrez
        {
            /// <summary>
            /// Конструктор
            /// </summary>
            protected TagSrez()
            {
            }
            /// <summary>
            /// Конструктор
            /// </summary>
            public TagSrez(int tagCnt)
            {
                if (tagCnt <= 0)
                    throw new ArgumentException("Tag count must be positive.", "tagCnt");

                DateTime = DateTime.MinValue;
                KPTags = new KPTag[tagCnt];
                TagData = new SrezTableLight.CnlData[tagCnt];
                Descr = "";
            }

            /// <summary>
            /// Получить или установить временную метку
            /// </summary>
            public DateTime DateTime { get; set; }
            /// <summary>
            /// Получить ссылки на теги КП, входящие в срез
            /// </summary>
            public KPTag[] KPTags { get; protected set; }
            /// <summary>
            /// Получить данные тегов
            /// </summary>
            public SrezTableLight.CnlData[] TagData { get; protected set; }
            /// <summary>
            /// Получить или установить описание среза для вывода в журнал
            /// </summary>
            public string Descr { get; set; }

            /// <summary>
            /// 获取与输入通道关联的切片标记索引数组
            /// </summary>
            public List<int> GetBoundTagIndexes()
            {
                List<int> indexes = new List<int>();
                int len = KPTags.Length;
                for (int i = 0; i < len; i++)
                {
                    KPTag kpTag = KPTags[i];
                    if (kpTag != null && kpTag.CnlNum > 0)
                        indexes.Add(i);
                }
                return indexes;
            }
            /// <summary>
            /// Установить данные тега среза
            /// </summary>
            public void SetTagData(int tagIndex, double newVal, int newStat)
            {
                SetTagData(tagIndex, new SrezTableLight.CnlData(newVal, newStat));
            }
            /// <summary>
            /// Установить данные тега среза
            /// </summary>
            public void SetTagData(int tagIndex, SrezTableLight.CnlData newData)
            {
                if (0 <= tagIndex && tagIndex < TagData.Length)
                    TagData[tagIndex] = newData;
            }
        }

        /// <summary>
        /// Событие КП
        /// </summary>
        /// <remarks>Особенность события КП заключается в использовании тега КП вместо входного канала</remarks>
        public class KPEvent
        {
            /// <summary>
            /// Конструктор
            /// </summary>
            public KPEvent()
                : this(DateTime.MinValue, 0, null)
            {
            }
            /// <summary>
            /// Конструктор
            /// </summary>
            public KPEvent(DateTime dateTime, int kpNum, KPTag kpTag)
            {
                DateTime = dateTime;
                KPNum = kpNum;
                KPTag = kpTag;
                OldData = SrezTableLight.CnlData.Empty;
                NewData = SrezTableLight.CnlData.Empty;
                Checked = false;
                UserID = 0;
                Descr = "";
                Data = "";
            }

            /// <summary>
            /// 获取或设置时间戳
            /// </summary>
            public DateTime DateTime { get; set; }
            /// <summary>
            /// 从配置数据库获取或设置 CP 编号
            /// </summary>
            public int KPNum { get; set; }
            /// <summary>
            /// 获取或设置指向 KP 参数的链接
            /// </summary>
            public KPTag KPTag { get; set; }
            /// <summary>
            /// 获取或设置旧的 KP 参数数据
            /// </summary>
            public SrezTableLight.CnlData OldData { get; set; }
            /// <summary>
            /// 获取或设置新的 KP 参数数据
            /// </summary>
            public SrezTableLight.CnlData NewData { get; set; }
            /// <summary>
            /// 获取或设置活动握手功能
            /// </summary>
            public bool Checked { get; set; }
            /// <summary>
            /// 从配置数据库中获取或设置事件失败的用户 ID
            /// </summary>
            public int UserID { get; set; }
            /// <summary>
            /// 获取或设置事件描述
            /// </summary>
            public string Descr { get; set; }
            /// <summary>
            /// 获取或设置其他事件数据
            /// </summary>
            public string Data { get; set; }
        }

        /// <summary>
        /// Kp 操作条件
        /// </summary>
        public enum WorkStates
        {
            /// <summary>
            /// 未定义
            /// </summary>
            Undefined,
            /// <summary>
            /// 正常
            /// </summary>
            Normal,
            /// <summary>
            /// 异常
            /// </summary>
            Error
        }
    }
}
