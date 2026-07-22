
namespace PcMQTT
{
    interface ISensor : IDiscoverable
    {
        Task publish(object state);
    }
}
