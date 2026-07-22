
using System.Text.Json;

namespace PcMQTT
{
    class ShutdownCommand : IButton
    {
        MqttClient client;

        public ShutdownCommand(
            MqttClient client
        )
        {
            this.client = client;
        }

        public string topic => $"{Topics.commandTopic}/sleep";
        public async Task handleSubscription(string payload)
        {
            PowerManager.sleep();
        }
        public async Task subscribe()
        {
            await client.subscribe(topic);
            Console.WriteLine($"Subscribed to 'Shutdown' button @ {topic}");
        }

        public async Task discover(
            string discoveryPrefix = "homeassistant"
        )
        {
            var configTopic = $"{discoveryPrefix}/button/{IDiscoverable.deviceId}/shutdown/config";
            var configPayload = JsonSerializer.Serialize(new
            {
                name = "Shutdown",
                unique_id = $"{IDiscoverable.deviceId}_shutdown",
                command_topic = topic,
                payload_press = "PRESS",
                IDiscoverable.device,
                availability_topic = Topics.avaliabilityTopic,
                payload_available = "online",
                payload_not_available = "offline"
            });
        
            await client.publish(configTopic, configPayload, retain: true);
            Console.WriteLine($"Published discovery config for 'Shutdown' to {configTopic}");
        }
    }
}
