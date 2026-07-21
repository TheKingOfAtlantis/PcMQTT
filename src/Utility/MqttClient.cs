
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

        event Func<Task>? onConnected;
        event Func<Task>? onDisconnecting;

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
                    retain:  true
                );

                if(onConnected is Func<Task> OnConnected)
                    await OnConnected();
            };
            
            var result = await mqttClient.ConnectAsync(options);
        }

        public async Task disconnect()
        {
            if(onDisconnecting is Func<Task> OnDisconnecting)
                await OnDisconnecting();
            await publish(
                topic:   Topics.avaliabilityTopic,
                payload: "offline",
                retain:  true
            );
            await mqttClient.DisconnectAsync();
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

            if(retain is bool Retain) messageBuilder.WithRetainFlag(Retain);
            if(qos is int Qos) messageBuilder.WithQualityOfServiceLevel(
                Qos == 0 ? MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce :
                Qos == 1 ? MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce :
                           MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce
            );

            await mqttClient.PublishAsync(messageBuilder.Build());
        }

        public async Task subscribe(
            string topic
        )
        {
            var topicFilter = new MqttTopicFilterBuilder()
                .WithTopic(topic)
                .Build();
            
            await mqttClient.SubscribeAsync(topicFilter);
        }
    }

    static class MqttClientExtensions {
        public static async Task awaitConnected(this MqttClient client)
        {
            SpinWait.SpinUntil(() => client.mqttClient.IsConnected);
        }
    }
}
