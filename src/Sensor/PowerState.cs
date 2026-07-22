
using System.Text.Json;

namespace PcMQTT
{
    
    enum PowerState
    {
        Unknown,

        Running,
        
        Sleeping,
        Hibernating,
        Suspend,
        Shutdown,
    }

    class PowerStateSensor : ISensor
    {
        public string topic => $"{Topics.sensorTopic}/power_state";

        private PowerState _lastState = PowerState.Running;
        public PowerState lastState { get => _lastState; }

        MqttClient client;

        public PowerStateSensor(MqttClient client)
        {
            this.client = client;
        }

        public async Task discover(
            string discoveryPrefix = "homeassistant"
        )
        {
            var configTopic   = $"{discoveryPrefix}/sensor/{IDiscoverable.deviceId}/power_state/config";
            var configPayload = JsonSerializer.Serialize(new
            {
                name = "Power State",
                unique_id = $"{IDiscoverable.deviceId}_power_state",
                state_topic = topic,
                icon = "mdi:power",
                options = Enum.GetNames<PowerState>(),
                device_class = "enum",
                IDiscoverable.device
            });

            await client.publish(
                topic: configTopic,
                payload: configPayload,
                retain: true
            );
            Console.WriteLine($"Published discovery config for 'Power state' to {configTopic}");
        }

        public async Task publish(object state)
        {
            if(state is PowerState State)
                await publish(State);
        }
        public async Task publish(PowerState state)
        {
            await client.publish(topic, state.ToString(), retain: true);
            Console.WriteLine($"Published power state change: {state} @ {topic}");
            _lastState = state;
        }
    }
}
