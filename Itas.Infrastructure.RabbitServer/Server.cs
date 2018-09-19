using RabbitMQ.Client;
using SimpleFactory.Contract;
using System;
using System.Collections.Generic;

namespace Itas.Infrastructure.RabbitServer
{

    /// <summary>
    /// Basic warpper for publishing a message to the server
    /// </summary>
    public class Publisher
    {
        private readonly Context.ISerializer serializer;
        private readonly IModel channel;
        private readonly string exchangeName;
        private readonly string routingKeyPrefix;
        private object padLock = new object();

        public Publisher(Context.ISerializer serializer, IModel channel, string exchangeName, string routingKeyPrefix = "")
        {
            this.serializer = serializer;
            this.channel = channel;
            this.exchangeName = exchangeName;
            this.routingKeyPrefix = routingKeyPrefix;
        }

        /// <summary>
        /// Publishes message to serve.
        /// </summary>
        /// <param name="message"></param>
        public void PublishMessage( object message) 
        {
            var basicProp = channel.CreateBasicProperties();

            basicProp.Type = "text/json";
            basicProp.DeliveryMode = 2;


            var body = new System.IO.MemoryStream();
            serializer.ToStream(body, message);
            body.Position = 0; 
            
            lock (padLock)
            {
                channel.BasicPublish(exchangeName, $"{routingKeyPrefix}_{message.GetType().FullName}",basicProp,body.ToArray());
            }

        }



    }


    public interface IServer
    {
        void Start();
        void Stop();
    }



    /// <summary>
    /// 
    /// </summary>
    public class Server : IDisposable, IServer
    {
        ServerConfig serverConfig;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="container"></param>
        /// <param name="connectionInfo"></param>
        /// <param name="config"></param>
        public Server(IContainer container, ServerConfig.ConectionInfo connectionInfo, Action<ServerConfig> config)
        {
            serverConfig = new ServerConfig(connectionInfo);
            config(serverConfig);
            this.container = container;
        }

        private RabbitMQ.Client.IModel ManagmentChannel;

        private Publisher DLQPublisher = null;

        private ConnectionFactory connectionFactory;
        private IConnection rabbitServerConnection;
        private List<QueueHandler> AllHandlers = new List<QueueHandler>();

        private bool isRunning = false;

        /// <summary>
        /// 
        /// </summary>
        public void Start()
        {
            isRunning = true;

            //Connect to Rabbit. 1 connection pr server. 
            connectionFactory = new ConnectionFactory()
            {
                HostName = serverConfig.connectionInfo.Server,
                UserName = serverConfig.connectionInfo.UserName,
                Password = serverConfig.connectionInfo.Password,
                VirtualHost = serverConfig.connectionInfo.VirtualHost ?? "/"
            };

            rabbitServerConnection = connectionFactory.CreateConnection(serverConfig.connectionInfo.ClientName);

            string exchangeName = serverConfig.connectionInfo.ExchangeName;
            SetupExchange(exchangeName);

            DLQPublisher = new Publisher(ManagmentChannel, exchangeName + "_DLE", $"{serverConfig.connectionInfo.ClientName}_");

            foreach (var info in serverConfig.Registrations)
            {
                AllHandlers.Add(new QueueHandler(info.Value, DLQPublisher, rabbitServerConnection, serverConfig.connectionInfo));
            }
        }

        private void SetupExchange(string exchangeName)
        {
            ManagmentChannel = rabbitServerConnection.CreateModel();
            ManagmentChannel.ExchangeDeclare(exchangeName, "topic", true, false, null);
            ManagmentChannel.ExchangeDeclare(exchangeName + "_DLE", "direct", true, false, null);
        }



        public void Stop()
        {
            ManagmentChannel.Close();
            rabbitServerConnection.Close();

        }

        /// <summary>
        /// Will try to create all instances of handlers to ensure all registration is OK
        /// </summary>
        public void Verify()
        {
            throw new NotImplementedException();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls
        private readonly IContainer container;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    this.Stop();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Server() {
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


    }
}
