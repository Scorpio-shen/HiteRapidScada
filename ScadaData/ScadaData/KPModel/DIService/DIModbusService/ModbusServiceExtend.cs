using Autofac;
using HslCommunication.ModBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace Scada.DIService.DIModbusService
{
    public static class ModbusServiceExtend
    {
        /// <summary>
        /// 注入Modbus服务
        /// </summary>
        /// <param name="container"></param>
        /// <param name="ipaddress"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        public static ContainerBuilder DIModbusTcpService(this ContainerBuilder container,string modbusServiceName = null)
        {
            ModbusTcpNet modbusTcp = new ModbusTcpNet();
            var bulider = container.RegisterInstance(modbusTcp);
            if (modbusServiceName != null)
            {
                bulider.Named<ModbusTcpNet>("MyModbus");
            }
            return container;
        }
    }
}
