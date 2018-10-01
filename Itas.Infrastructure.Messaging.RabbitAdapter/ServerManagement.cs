using RabbitMQ.Client;
using System;

namespace Itas.Infrastructure.Messaging.RabbitAdapter
{
    public class ServerManagement :  IDisposable
    {
        private readonly string clientName;
        private readonly IModel channel;
        private readonly IConnection connection;

        public ServerManagement(string clientName, IConnection connection)
        {
            this.clientName = clientName;
            this.connection = connection;
            this.channel = CreateChannel(); //creates an mgmnt channel--
        }

        /// <summary>
        /// Creates a channel to the server
        /// </summary>
        /// <returns></returns>
        public IModel CreateChannel()
        {
            return connection.CreateModel();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="exchangeName"></param>
        public void CreateTopicExchange(string exchangeName)
        {
            channel.ExchangeDeclare(exchangeName, "topic", true, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="exchangeName"></param>
        public void CreateDirectExchange(string exchangeName)
        {
            channel.ExchangeDeclare(exchangeName, "direct", true, false);
        }

        /// <summary>
        /// Creates a queue on the server. 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public QueueInfo CreateQueue(string name)
        {
            channel.QueueDeclare($"{clientName}_{name}", true, false, false);
            return new QueueInfo(channel, name);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    channel.Close(0, "Shutting down");
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
            private IModel channel;
            private string name;

            public QueueInfo(IModel channel, string name)
            {
                this.channel = channel;
                this.name = name;
            }

            public void ConnectToExchange(string exchangeName, string bindingKey)
            {
                channel.ExchangeBind(name, exchangeName, bindingKey);
            }
        }

    }
}
