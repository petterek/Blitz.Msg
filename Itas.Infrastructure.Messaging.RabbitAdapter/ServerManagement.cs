using Itas.Infrastructure.Context;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.IO;

namespace Itas.Infrastructure.Messaging.RabbitAdapter
{
    public class ServerManagement : IDisposable
    {
        internal readonly string clientName;
        internal readonly IModel InChannel;
        internal readonly IModel OutChannel;
        internal readonly IConnection IncommingConnection;
        readonly ISerializer serializer;
        readonly IConnection outgoingConnection;

        public ServerManagement(string clientName, IConnection incommongConnection,IConnection outgoingConnection, ISerializer serializer)
        {
            this.outgoingConnection = outgoingConnection;
            this.serializer = serializer;
            this.clientName = clientName;
            this.IncommingConnection = incommongConnection;
            this.InChannel = incommongConnection.CreateModel();
            this.OutChannel = outgoingConnection.CreateModel();
        }

        /// <summary>
        /// Creates a channel to the server
        /// </summary>
        /// <returns></returns>
        public IModel CreateChannel()
        {
            return IncommingConnection.CreateModel();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="exchangeName"></param>
        public ExchangeInfo CreateTopicExchange(string exchangeName)
        {
            InChannel.ExchangeDeclare(exchangeName, "topic", true, false);
            return new ExchangeInfo(this,exchangeName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="exchangeName"></param>
        public void CreateDirectExchange(string exchangeName)
        {
            InChannel.ExchangeDeclare(exchangeName, "direct", true, false);
        }

        /// <summary>
        /// Creates a queue on the server, all queues will be prefixed with the client name. 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public QueueInfo CreateQueueAndBind(string routingKey,string exchangeName, string deadLetterExchange = "")
        {
            string QueueName = $"{clientName}_{exchangeName}_{routingKey}";
            if (!string.IsNullOrEmpty(deadLetterExchange))
            {
                InChannel.QueueDeclare(QueueName, true, false, false, new Dictionary<string, object>() { { "x-dead-letter-exchange", $"{deadLetterExchange}" } });
            }
            else
            {
                InChannel.QueueDeclare(QueueName, true, false, false);
            }

            return new QueueInfo(this, QueueName);
        }
    

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    InChannel.Close(0, "Shutting down");
                    IncommingConnection.Close();
                    outgoingConnection.Close();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                
                disposedValue = true;
            }
        }





        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ServerManagment() {
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

        public class QueueInfo
        {
           
            public string name;
            readonly ServerManagement management;

            public QueueInfo(ServerManagement management, string name)
            {
                this.management = management;
                this.name = name;
            }

            public QueueInfo ConnectToExchange(string exchangeName, string bindingKey)
            {
                management.InChannel.QueueBind(name, exchangeName, bindingKey);
                return this;
            }
        }
        public class ExchangeInfo
        {
            public readonly string name;
            readonly ServerManagement management;

            public ExchangeInfo(ServerManagement management, string name)
            {
                this.management = management;
                this.name = name;
            }

            public QueueInfo CreateAndBindQueue(string queueName, string routingKey)
            {
                management.InChannel.QueueDeclare(queueName, true, false, false);
                management.InChannel.QueueBind(queueName, name, routingKey);
                return new QueueInfo(management, queueName);
            }

            public void SendMessage(object message)
            {
                var props = management.InChannel.CreateBasicProperties();
                props.DeliveryMode = 2;
                management.OutChannel.BasicPublish(name, message.GetType().FullName,props, management.serializeToArray(message));
            }
        }

        private byte[] serializeToArray(object message)
        {
            var m = new MemoryStream();
            serializer.ToStream(m, message);
            return m.ToArray();
        }
    }

    
}
