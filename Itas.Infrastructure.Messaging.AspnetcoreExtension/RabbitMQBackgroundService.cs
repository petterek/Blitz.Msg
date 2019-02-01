using Itas.Infrastructure.MessageHost;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Itas.Infrastructure.Messaging.AspnetcoreExtension
{
    public class RabbitMQBackgroundService : BackgroundService
    {
        readonly MessageConfig messageConfig;
        readonly ILogger<RabbitMQBackgroundService> logger;
        readonly MessageHandlerEngine server;

        public RabbitMQBackgroundService(MessageConfig messageConfig, MessageHandlerEngine server, ILogger<RabbitMQBackgroundService> logger)
        {
            this.server = server;
            this.logger = logger;
            this.messageConfig = messageConfig;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {

            stoppingToken.Register(Shutdown);
            
            messageConfig.MessageHandlers.ForEach(kv => {
                if(kv.Message == null)
                {
                    server.AttachGenericMessageHandler(kv.BindingKey, kv.Handler);
                }else
                {
                    server.AttachMessageHandler(kv.Message, kv.Handler);
                }
            });

            server.StartServer();

            return Task.CompletedTask;
        }

        void Shutdown()
        {
            server.StopServer();
            server.Dispose();
        }
    }
}
