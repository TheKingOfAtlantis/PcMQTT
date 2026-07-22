
namespace PcMQTT
{
    interface ISubscriber
    {
        string topic { get; }
        Task subscribe();
        Task handleSubscription(string payload);
    }
}
