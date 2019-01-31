using Itas.Infrastructure.MessageHost;
using RabbitMQ.Client.Events;
using System.Linq;

namespace Itas.Infrastructure.Messaging.RabbitConsumer
{
    public class RabbitRecievedMessageData : RecievedMessageData
    {
        public RabbitRecievedMessageData(BasicDeliverEventArgs eventArgs) : base(eventArgs.Body )
        {
            eventArgs.BasicProperties.Headers.ToList().ForEach(kv => {
                base.Attributes.Add(kv.Key, System.Text.Encoding.UTF8.GetString((byte[])kv.Value));
            });
        }
    }
}
