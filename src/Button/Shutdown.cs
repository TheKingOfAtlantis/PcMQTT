
using System.Text.Json;

namespace PcMQTT
{
    class ShutdownCommand : IButton
    {
        MqttClient client;
        ISensor sensor;

        public ShutdownCommand(
            MqttClient client,
            ISensor sensor
        )
        {
            this.client = client;
            this.sensor = sensor;
        }

        public string topic => $"{Topics.commandTopic}/sleep";
        public async Task handleSubscription(string payload)
        {
            await sensor.publish(PowerState.Shutdown);
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
