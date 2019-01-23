namespace Itas.Infrastructure.Messaging.Shared
{
    public class RabbitConnectionInfo
    {
        public string Server { get; set; }
        public string UserName { get; set; }
		public string Password { get; set; }
		public string ExchangeName { get; set; }
		public string VirtualHost { get; set; }
		public string ClientName { get; set; }

		public string DeadLetterExchange { get => ExchangeName + "_DeadLetterExchange"; }
        public string DeadLetterQueue { get => ClientName + "_DeadLetterQueue"; }
    }
}
