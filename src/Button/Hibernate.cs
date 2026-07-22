
using System.Text.Json;

namespace PcMQTT
{
    class HibernateCommand : IButton
    {
        MqttClient client;

        public HibernateCommand(
            MqttClient client
        )
        {
            this.client = client;
        }

        public string topic => $"{Topics.commandTopic}/sleep";
        public async Task handleSubscription(string payload)
        {
            PowerManager.hibernate();
        }
        public async Task subscribe()
        {
            await client.subscribe(topic);
            Console.WriteLine($"Subscribed to 'Hibernate' button @ {topic}");
        }

        public async Task discover(
            string discoveryPrefix = "homeassistant"
        )
        {
            var configTopic = $"{discoveryPrefix}/button/{IDiscoverable.deviceId}/hibernate/config";
            var configPayload = JsonSerializer.Serialize(new
            {
                name = "Hibernate",
                unique_id = $"{IDiscoverable.deviceId}_hibernate",
                command_topic = topic,
                payload_press = "PRESS",
                IDiscoverable.device,
                availability_topic = Topics.avaliabilityTopic,
                payload_available = "online",
                payload_not_available = "offline"
            });
        
            await client.publish(configTopic, configPayload, retain: true);
            Console.WriteLine($"Published discovery config for 'Hibernate' to {configTopic}");
        }
    }
}
