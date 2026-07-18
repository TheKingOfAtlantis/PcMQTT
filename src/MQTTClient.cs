
namespace PcMQTT
{

    class MqttClient
    {
        
        public const int defaultPort = 1883;

        public MqttClient(
            string hostname,
            Int16 port       = defaultPort,
            string clientId  = "PcMQTTClient",
            string? username = null,
            string? password = null
        )
        {
        }
    }
}
