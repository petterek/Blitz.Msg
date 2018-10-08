namespace Itas.Infrastructure.Messaging.RabbitAdapter
{
    public class RabbitConnectionInfo
    {
        public string Server;
        public string UserName;
        public string Password;
        public string ExchangeName;
        public string VirtualHost;
        public string ClientName;

        public string DeadLetterExchange { get => ExchangeName + "_DeadLetterExchange"; }
        public string DeadLetterQueue { get => ClientName + "_DeadLetterQueue"; }
    }
}
