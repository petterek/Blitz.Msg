using Itas.Infrastructure.MessageHost;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using Itas.Infrastructure.Messaging.Shared;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Itas.Infrastructure.Messaging.RabbitConsumer
{
    public class RabbitMessageAdapter : IMessageAdapter
    {
        private readonly ConnectionFactory connectionFactory;
        private ServerManagement management;
        private List<IModel> channels = new List<IModel>();
        private RabbitConnectionInfo connectionInfo;
        private readonly ISerializer serializer;
        private List<BindingInfo> bindingInfos = new List<BindingInfo>();
        private ServerManagement.ExchangeInfo GlobaleErrorExchange;

        public event Action<object,Type, RecievedMessageData> OnMessage;

        readonly ILogger<RabbitMessageAdapter> logger;

        public RabbitMessageAdapter(RabbitConnectionInfo connectionInfo, ISerializer serializer, ILogger<RabbitMessageAdapter> logger)
        {
            this.logger = logger;
            this.connectionFactory = new ConnectionFactory()
            {
                HostName = connectionInfo.Server,
                UserName = connectionInfo.UserName ,
                Password = connectionInfo.Password ,
                VirtualHost = connectionInfo.VirtualHost ?? "/"
            };
            connectionFactory.AutomaticRecoveryEnabled = true;

            this.connectionInfo = connectionInfo;
            this.serializer = serializer;
        }

        public void StartAdapter()
        {
            management = new ServerManagement(connectionInfo.ClientName, connectionFactory.CreateConnection(connectionInfo.ClientName), connectionFactory.CreateConnection(connectionInfo.ClientName), serializer);

            //Create globale Error Exchange, if not exists
            GlobaleErrorExchange = management.CreateTopicExchange(connectionInfo.ExchangeName + "_GlobalErrorsExchange");

            //Assert Dead Letter Exchange and Queue for this consumer.. 
            var dle = management.CreateTopicExchange(connectionInfo.ClientName + "_DeadLetterExchange");
            dle.CreateAndBindQueue(connectionInfo.ClientName + "_DeadLetterQueue", "#");
            
            foreach (var bindingInfo in bindingInfos)
            {
                var theQueue = management.CreateQueueAndBind(bindingInfo.RoutingKey, connectionInfo.ExchangeName, dle.name).ConnectToExchange(connectionInfo.ExchangeName, bindingInfo.RoutingKey);

                IModel model = management.CreateChannel();
                model.BasicQos(0, 1, false);
                var consumer = new EventingBasicConsumer(model);
                channels.Add(model);

                if (bindingInfo.MessageType != null)
                {
                    consumer.Received += (a, theMessageRecieved) =>
                    {
                        var param = serializer.FromStream(new System.IO.MemoryStream(theMessageRecieved.Body), bindingInfo.MessageType);
                        try
                        {
                            OnMessage(param,bindingInfo.HandlerType,new RabbitRecievedMessageData(theMessageRecieved)  );
                            model.BasicAck(theMessageRecieved.DeliveryTag, false);
                        }
                        catch (Exception ex)
                        {
                            model.BasicNack(theMessageRecieved.DeliveryTag, false, false);
                            SendToError(theMessageRecieved);
                        }
                    };
                }
                else //Anonymouse handler.. Need to handle the 
                {
                    consumer.Received += (a, theMessageRecieved) =>
                    {
                        var param = new RabbitRecievedMessageData(theMessageRecieved);
                        try
                        {
                            OnMessage(param,bindingInfo.HandlerType,new RabbitRecievedMessageData(theMessageRecieved));
                            model.BasicAck(theMessageRecieved.DeliveryTag, false);
                        }
                        catch (Exception ex)
                        {
                            model.BasicNack(theMessageRecieved.DeliveryTag, false, false);
                        }
                    };
                }
                model.BasicConsume(theQueue.name, false, consumer);
            }
        }

        private void SendToError(BasicDeliverEventArgs faildMessage)
        {
            var msg = new ErrorModel();
            msg.CorrelationId = faildMessage.BasicProperties.CorrelationId;
            msg.MessageType = faildMessage.RoutingKey;
            msg.HandlerName = connectionInfo.ClientName;
            msg.ServerName = Environment.MachineName;

            GlobaleErrorExchange.SendMessage(msg);
        }

        public void StopAdapter()
        {
            management.Dispose();
            foreach (var model in channels)
            {
                model.Close();
            }
        }

        public void Bind(string routingKey, Type messageType, Type handlerType)
        {
            bindingInfos.Add(new BindingInfo { RoutingKey = routingKey, MessageType = messageType, HandlerType = handlerType });
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls
                
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    StopAdapter();

                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~RabbitMessageProducer() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }



        #endregion

        internal class BindingInfo
        {
            internal string RoutingKey;
            internal Type MessageType;
            internal Type HandlerType;
        }
    }
}
