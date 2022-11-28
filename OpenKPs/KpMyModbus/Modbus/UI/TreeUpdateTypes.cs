using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpMyModbus.Modbus.UI
{
    [Flags]
    public enum TreeUpdateTypes
    {
        /// <summary>
        /// Обновление не требуется
        /// </summary>
        None = 0,

        /// <summary>
        /// Текущий узел
        /// </summary>
        CurrentNode = 1,

        /// <summary>
        /// Дочерние узлы
        /// </summary>
        ChildNodes = 2,

        /// <summary>
        /// Узлы того же уровня, следующие за текущим
        /// </summary>
        NextSiblings = 4,

        /// <summary>
        /// Обновить сигналы
        /// </summary>
        UpdateSignals = 8
    }
}
