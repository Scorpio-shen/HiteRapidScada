using HslCommunication.Core.IMessage;
using HslCommunication.ModBus;
using HslCommunication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using KpCommon.Hsl.Core.IMessage;

namespace KpCommon.Hsl.ModBus
{
    public class ModbusAsciiOverTcp : ModbusRtuOverTcp
    {
        //
        // 摘要:
        //     实例化一个Modbus-ascii协议的客户端对象
        //     Instantiate a client object of the Modbus-ascii protocol
        public ModbusAsciiOverTcp()
        {
            LogMsgFormatBinary = false;
        }

        public ModbusAsciiOverTcp(string ipAddress, int port = 502, byte station = 1)
    : base(ipAddress, port, station)
        {
            LogMsgFormatBinary = false;
        }

        protected override INetMessage GetNewNetMessage()
        {
            return new SpecifiedCharacterMessage(13, 10);
        }

        protected override byte[] PackCommandWithHeader(byte[] command)
        {
            return ModbusInfo.TransModbusCoreToAsciiPackCommand(command);
        }

        protected override OperateResult<byte[]> UnpackResponseContent(byte[] send, byte[] response)
        {
            OperateResult<byte[]> operateResult = ModbusInfo.TransAsciiPackCommandToCore(response);
            if (!operateResult.IsSuccess)
            {
                return operateResult;
            }

            if (send[1] + 128 == operateResult.Content[1])
            {
                return new OperateResult<byte[]>(operateResult.Content[2], ModbusInfo.GetDescriptionByErrorCode(operateResult.Content[2]));
            }

            return ModbusInfo.ExtractActualData(operateResult.Content);
        }

        public override string ToString()
        {
            return $"ModbusAsciiOverTcp[{IpAddress}:{Port}]";
        }
    }
}
