using Itas.Infrastructure.MessageHost;
using Itas.Infrastructure.Messaging.RabbitConsumer;
using Itas.Infrastructure.Messaging.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Linq;

namespace Itas.Infrastructure.Messaging.AspnetcoreExtension
{
    public static class Extension
    {

        /// <summary>
        /// <para>Register all events and its handlers.</para>
        /// <para>Registers an IMessageContext as scoped that can be used to retrive context information.</para>
        /// <para>Registers an ISerializer so you do not need to implement it, JSON is assumed.</para>
        /// <para>Registers a background service for listening to messages from rabbit mq</para>
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="connectionInfo"></param>
        /// <param name="config"></param>
        public static void ConfigureRabbitMessageConsumer(this IServiceCollection serviceCollection, RabbitConnectionInfo connectionInfo, Action<MessageConfig> config)
        {
            var cnfg = new MessageConfig();
            config(cnfg);

            serviceCollection.AddScoped<IRecivedMessageContext, MessageContextHolder>();

            cnfg.MessageHandlers.ForEach(kv =>
            {
                serviceCollection.AddScoped(kv.Handler);
            });

            serviceCollection.AddSingleton(cnfg);
            serviceCollection.TryAddSingleton<RabbitConnectionInfo>(connectionInfo);
            serviceCollection.TryAddSingleton<ISerializer>(new Serializer());
            serviceCollection.TryAddSingleton<IMessageAdapter, RabbitMessageAdapter>();
            serviceCollection.TryAddSingleton<MessageHandlerEngine>();                        

            serviceCollection.AddHostedService<RabbitMQBackgroundService>();
        }



    }
}
