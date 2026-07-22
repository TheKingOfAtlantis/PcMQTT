
namespace PcMQTT
{
    
    static class Topics {
        public const string rootTopic = "pcmqtt";

        public const string avaliabilityTopic = $"{rootTopic}/availability";

        public const string commandTopic = $"{rootTopic}/command";
        public const string sensorTopic  = $"{rootTopic}/sensor";
    };

}
