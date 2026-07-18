
using MQTTnet;

namespace PcMQTT
{

    class MqttClient
    {
        public const int defaultPort = 1883;
        
        private string hostname;
        private Int16 port;
        private string clientID;
        private string? username;
        private string? password;

        public IMqttClient mqttClient;

        public MqttClient(
            string hostname,
            Int16 port       = defaultPort,
            string clientID  = "PcMQTTClient",
            string? username = null,
            string? password = null
        )
        {
            this.hostname = hostname;
            this.port     = port;
            this.clientID = clientID;
            this.username = username;
            this.password = password;

            var mqttClientFactory = new MqttClientFactory();
            mqttClient = mqttClientFactory.CreateMqttClient();
        }

        event Func<Task> onConnected;
        event Func<Task> onDisconnecting;

        public async Task connect()
        {
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(hostname, port)
                .WithClientId(clientID)
                .WithCredentials(username, password)
                .WithCleanSession()
                .WithWillTopic(Topics.avaliabilityTopic)
                .WithWillPayload("offline")
                .Build();

            mqttClient.ConnectedAsync += async _ => {
                await publish(
                    topic:   Topics.avaliabilityTopic,
                    payload: "online",
                    retain:   true
                );

                await onConnected();
            };
            
            var result = await mqttClient.ConnectAsync(options);
        }

        
        public async Task publish(
            string topic,
            string payload,
            bool? retain = null,
            int? qos = null
        )
        {
            var messageBuilder = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload);

            if(retain is not null) messageBuilder.WithRetainFlag(retain!);
            if(qos is not null) messageBuilder.WithQualityOfServiceLevel(qos!);

            await mqttClient.PublishAsync(messageBuilder.Build());
        }
    }
}
