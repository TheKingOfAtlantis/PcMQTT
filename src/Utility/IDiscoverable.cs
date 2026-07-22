
using DeviceId;

namespace PcMQTT
{
    record HADevice(
        string[] identifiers,
        string name,
        // string manufacturer,
        // string model,
        // string hw_version = "",
        string sw_version
    );

    interface IDiscoverable
    {
        public static readonly string deviceId = new DeviceIdBuilder()
                .OnWindows(windows => windows.AddMachineGuid())
                .OnLinux(linux => linux.AddMachineId())
                .ToString();

        static HADevice device = new HADevice(
            identifiers: [deviceId],
            name: Environment.MachineName,
            // manufacturer = "",
            // model = "",
            sw_version: "1.0.0"
        );

        Task discover(
            string discoveryPrefix = "homeassistant"
        );
    }

}
