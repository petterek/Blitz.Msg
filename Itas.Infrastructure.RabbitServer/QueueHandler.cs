using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;

namespace Itas.Infrastructure.RabbitServer
{
    internal class QueueHandler
    {
        private readonly ServerConfig.Info info;
        private readonly IConnection connection;
        private readonly ServerConfig.ConectionInfo conInfo;
        private IModel incommingChannel,sendingChannel;
        

        internal QueueHandler(ServerConfig.Info info,Publisher dlqPublisher, IConnection connection, ServerConfig.ConectionInfo conInfo)
        {
            this.info = info;
            this.connection = connection;
            this.conInfo = conInfo;

            incommingChannel = connection.CreateModel();

            var queueName = $"{conInfo.ClientName}_{info.RoutingKey}";
            var deadLetterQName = $"{queueName}_DLQ";

            CreateQ(queueName);
            incommingChannel.QueueBind(queueName, conInfo.ExchangeName, info.RoutingKey);

            var consumer = new EventingBasicConsumer(incommingChannel);

            consumer.Received += (ch, theEvent) =>
            {
                try
                {
                    HandleMessage();
                }
                catch (Exception ex)
                {
                    if(theEvent.Redelivered)
                    {
                        incommingChannel.BasicNack(theEvent.DeliveryTag, false, false);
                        dlqPublisher.PublishMessage(new HandlingOfEventFaild());
                    }
                    else
                    {
                        incommingChannel.BasicNack(theEvent.DeliveryTag, false,true); //We requeue once.. 
                    }

                }
            };

        }

        private void HandleMessage()
        {

        }

        private void CreateQ(string queueName)
        {
            incommingChannel.QueueDeclare(queueName, true, false, false);
        }

    }
}