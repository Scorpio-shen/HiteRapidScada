using KpCommon.Model;

namespace KpHiteModbus.Modbus.Model
{
    /// <summary>
    /// 西门子数据请求Model
    /// </summary>
    public class TagGroupRequestModel : RequestModel
    {
        public int StartAddress { get; set; }
    }
}
