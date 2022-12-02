using KpHiteModbus.Modbus.Model;

namespace KpHiteModbus.Modbus.Extend
{
    public static class RegisterTypeEnumExtend
    {
        public static byte GetFunctionCode(this RegisterTypeEnum register, bool iswrite = false, bool ismultiple = true)
        {
            byte code = default;
            if (!iswrite)
            {
                switch (register)
                {
                    case RegisterTypeEnum.Coils:
                        code = FunctionCodes.ReadCoils;
                        break;
                    case RegisterTypeEnum.DiscretesInputs:
                        code = FunctionCodes.ReadDiscreteInputs;
                        break;
                    case RegisterTypeEnum.HoldingRegisters:
                        code = FunctionCodes.ReadHoldingRegisters;
                        break;
                    case RegisterTypeEnum.InputRegisters:
                        code = FunctionCodes.ReadInputRegisters;
                        break;
                }
            }
            else
            {
                switch (register)
                {
                    case RegisterTypeEnum.Coils:
                        if (!ismultiple)
                            code = FunctionCodes.WriteSingleCoil;
                        else
                            code = FunctionCodes.WriteMultipleCoils;
                        break;
                    case RegisterTypeEnum.HoldingRegisters:
                        if (!ismultiple)
                            code = FunctionCodes.WriteSingleRegister;
                        else
                            code = FunctionCodes.WriteMultipleRegisters;
                        break;
                }
            }

            return code;
        }
    }
}
