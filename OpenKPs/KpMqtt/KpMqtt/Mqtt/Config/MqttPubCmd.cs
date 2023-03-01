namespace Scada.Comm.Devices.Mqtt.Config
{
    /// <summary>
    /// Represents a command that publishes a topic when a telecommand is sent.
    /// </summary>
    public class MqttPubCmd : MqttPubParam
    {
        public int NumCmd { get; set; }
    }
}
