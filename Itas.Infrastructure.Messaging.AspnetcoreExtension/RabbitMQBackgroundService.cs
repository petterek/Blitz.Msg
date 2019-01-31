using Itas.Infrastructure.MessageHost;
using Itas.Infrastructure.Messaging.RabbitConsumer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
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
            
            messageConfig.MessageHandlers.ToList().ForEach(kv => {
                server.AttachMessageHandler(kv.Key, kv.Value);
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
