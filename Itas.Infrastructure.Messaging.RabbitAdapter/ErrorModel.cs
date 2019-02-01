namespace Itas.Infrastructure.Messaging.RabbitConsumer
{
    public class ErrorModel
    {
        public string CorrelationId;
        public string MessageType;
        public string HandlerName;
        public string ServerName;
    }
}
