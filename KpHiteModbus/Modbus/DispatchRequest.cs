using KpHiteModbus.Modbus.Model;
using Scada.KPModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KpHiteModbus.Modbus
{
    /// <summary>
    /// 调度请求类
    /// </summary>
    public class DispatchRequest
    {
        List<RequestUnit> listUnits;
        TagGroupRequestModel _request;
        ushort _maxlength;
        Action<RequestUnit> _requestMethod;

        public DispatchRequest(Action<RequestUnit> requestMethod)
        {
            listUnits = new List<RequestUnit>();
            _requestMethod= requestMethod;
        }
        
        public bool Request(TagGroupRequestModel request, RegisterTypeEnum registerType,out string errorMsg)
        {
            _request= request;
            if(registerType == RegisterTypeEnum.Coils || registerType == RegisterTypeEnum.DiscretesInputs)
                _maxlength= ModbusRegisterMaxDefine.MaxCoilOrDiRequestLength;
            else
                _maxlength = ModbusRegisterMaxDefine.MaxHrOrIRRequestLength;

            errorMsg = string.Empty;
            if (_request == null)
            {
                errorMsg = "request is null";
                return false;
            }
                
            if(_maxlength <= 0)
            {
                errorMsg = "maxlength长度不能小于等于0";
                return false;
            }

            if(request.Length <= 0)
            {
                errorMsg = "request length不能小于等于0";
                return false;
            }
            //清空原先queue
            if(listUnits.Count > 0)
                listUnits.Clear();
            try
            {
                //进行分包
                var start = request.StartAddress;
                int packageCount = request.Length / _maxlength;
                int i;
                for (i = 0; i < packageCount; i++)
                {
                    listUnits.Add(new RequestUnit
                    {
                        Index = i,
                        StartAddress = start +  _maxlength * i,
                        RequestLength = _maxlength,
                        RegisterType= registerType,
                    });
                }
                var left = request.Length % _maxlength;
                if (left > 0)
                    listUnits.Add(new RequestUnit
                    {
                        Index = i,
                        StartAddress = start + _maxlength * i,
                        RequestLength = (ushort)left,
                        RegisterType= registerType,
                    });


                //开始请求
                for(int j = 0;j < listUnits.Count; j++)
                {
                    _requestMethod?.Invoke(listUnits[j]);
                    Thread.Sleep(1);
                }
                    

                return true;
            }
            catch(Exception ex)
            {
                errorMsg = ex.Message;
                return false;
            }
            

        }

        public List<RequestUnit> Response() => listUnits;
    }

    /// <summary>
    /// 请求单元
    /// </summary>
    public class RequestUnit
    {
        public RegisterTypeEnum RegisterType { get; set; }
        public int Index { get; set; }
        public int StartAddress { get; set; }
        public ushort RequestLength { get; set; }
        public byte[] Buffer { get; set; }
    }
}
