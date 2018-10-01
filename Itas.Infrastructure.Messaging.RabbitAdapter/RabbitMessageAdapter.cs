using Itas.Infrastructure.Context;
using Itas.Infrastructure.RabbitServer;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;

namespace Itas.Infrastructure.Messaging.RabbitAdapter
{
    public class RabbitMessageAdapter : IMessageAdapter
    {

        private readonly ConnectionFactory connectionFactory;
        private IConnection rabbitServerConnection;
        private ServerManagement management;
        private List<IModel> channels = new List<IModel>();
        private RabbitConectionInfo connectionInfo;
        private readonly Context.ISerializer serializer;
        private readonly Func<BasicDeliverEventArgs, object> contextCreator;
        private List<BindingInfo> bindingInfos = new List<BindingInfo>();

        public event Action< object, object> OnMessage;
        public RabbitMessageAdapter(RabbitConectionInfo connectionInfo, ISerializer serializer, Func<BasicDeliverEventArgs, object> contextCreator)
        {

            this.connectionFactory = new ConnectionFactory()
            {
                HostName = connectionInfo.Server,
                UserName = connectionInfo.UserName,
                Password = connectionInfo.Password,
                VirtualHost = connectionInfo.VirtualHost ?? "/"
            };
            this.connectionInfo = connectionInfo;
            this.serializer = serializer;
            this.contextCreator = contextCreator;
        }

        public void StartAdapter()
        {
            rabbitServerConnection = connectionFactory.CreateConnection(connectionInfo.ClientName);
            management = new ServerManagement(connectionInfo.ClientName, rabbitServerConnection);

            //Create Exchange and Dead Letter System.. 
            management.CreateTopicExchange(connectionInfo.ExchangeName);
            management.CreateDirectExchange(connectionInfo.ExchangeName + "_DeadLetterExchange");
            //We need to create the deadletter queue as well
            //
            //
            //


            foreach (var bindingInfo in bindingInfos)
            {
                management.CreateQueue(bindingInfo.routingKey).ConnectToExchange(connectionInfo.ExchangeName, bindingInfo.routingKey);

                IModel model = management.CreateChannel();
                var consumer = new EventingBasicConsumer(model);
                channels.Add(model);

                consumer.Received += (a, b) =>
                {
                    
                    var param = serializer.FromStream(new System.IO.MemoryStream(b.Body), bindingInfo.messageType);
                    
                    try
                    {
                        OnMessage(param, contextCreator(b));
                        model.BasicAck(b.DeliveryTag, false);
                    }
                    catch (Exception ex)
                    {

                    }
                };
            }
        }

        public void StopAdapter()
        {
            foreach (var model in channels)
            {
                model.Close();
            }
        }

        public void Bind(string routingKey, Type messageType)
        {
            bindingInfos.Add(new BindingInfo { routingKey = routingKey, messageType = messageType });
        }

        public void BindAnonymouse(string routingKey, Type handledBy)
        {
            throw new NotImplementedException();
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
            internal string routingKey;
            internal Type messageType;
        }
    }
}
