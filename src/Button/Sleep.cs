
using System.Text.Json;

namespace PcMQTT
{
    class SleepCommand : IButton
    {
        MqttClient client;
        ISensor sensor;
        
        public SleepCommand(
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
            await sensor.publish(PowerState.Sleeping);
            PowerManager.sleep();
        }
        public async Task subscribe()
        {
            await client.subscribe(topic);
            Console.WriteLine($"Subscribed to 'Sleep' button @ {topic}");
        }

        public async Task discover(
            string discoveryPrefix = "homeassistant"
        )
        {
            var configTopic = $"{discoveryPrefix}/button/{IDiscoverable.deviceId}/sleep/config";
            var configPayload = JsonSerializer.Serialize(new
            {
                name = "Sleep",
                unique_id = $"{IDiscoverable.deviceId}_sleep",
                command_topic = topic,
                payload_press = "PRESS",
                IDiscoverable.device,
                availability_topic = Topics.avaliabilityTopic,
                payload_available = "online",
                payload_not_available = "offline"
            });
        
            await client.publish(configTopic, configPayload, retain: true);
            Console.WriteLine($"Published discovery config for 'Sleep' to {configTopic}");
        }
    }
}
