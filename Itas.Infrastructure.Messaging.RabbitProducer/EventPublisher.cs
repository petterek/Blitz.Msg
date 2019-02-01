using Itas.Infrastructure.Messaging.Shared;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Itas.Infrastructure.Messaging.RabbitProducer
{
    /// <summary>
    /// This is a wrapper class to support publishing events/messages to RabbitMQ
    /// This class should be singleton
    /// </summary>
    public class PublishEventToRabbit : IDisposable
    {
        private readonly IConnection rabbitConnection;
        private readonly IModel channel;
        private readonly RabbitConnectionInfo connectionInfo;
        private readonly ISerializer serializer;

        public PublishEventToRabbit(RabbitConnectionInfo connectionInfo, ISerializer serializer)
        {
            if (connectionInfo == null)
                throw new ArgumentNullException(nameof(connectionInfo));

            if (string.IsNullOrEmpty(connectionInfo.ClientName))
                throw new ArgumentException("Missing connectionInfo.ClientName");

            this.serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            this.connectionInfo = connectionInfo;

            var factory = new ConnectionFactory
            {
                UserName = connectionInfo.UserName,
                Password = connectionInfo.Password,
                HostName = connectionInfo.Server,
                AutomaticRecoveryEnabled = true,
                VirtualHost = connectionInfo.VirtualHost ?? "/"
            };


            rabbitConnection = factory.CreateConnection(connectionInfo.ClientName);
            channel = rabbitConnection.CreateModel();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="ArgumentException">If context.CorrelationId == Guid.Empty</exception>
        /// <param name="context"></param>
        /// <param name="eventData"></param>
        public void Publish(RabbitEventContext context, object eventData)
        {
            if (context.CorrelationId == Guid.Empty)
                throw new ArgumentException("Missing CorrelationId");


            var props = channel.CreateBasicProperties();
            props.Headers = new Dictionary<string, object>();

            props.DeliveryMode = 2;
            props.ContentType = "application/json";
            props.ContentEncoding = "UTF-8";
            props.AppId = connectionInfo.ClientName;
            props.CorrelationId = context.CorrelationId.ToString();


            context.ContextValues.ToList().ForEach(kv => props.Headers.Add(kv.Key, kv.Value));



            var outStream = new System.IO.MemoryStream();
            serializer.ToStream(outStream, eventData);
            outStream.Flush();
            outStream.Position = 0;

            //This is maybe not the best way, but it is an easy way.. this is thread-safe.. 
            lock (channel)
            {
                channel.BasicPublish(connectionInfo.ExchangeName, eventData.GetType().FullName, props, outStream.ToArray());
            }


        }

        #region IDisposable Support
        private bool _disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (_disposedValue)
                return;

            if (disposing)
            {
                // TODO: dispose managed state (managed objects).
                if (channel.IsOpen)
                {
                    channel.Close();
                }
                if (rabbitConnection.IsOpen)
                {
                    rabbitConnection.Close();
                }
            }

            // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
            // TODO: set large fields to null.

            _disposedValue = true;
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~PublishEventToRabbit() {
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

    public interface ISerializer
    {
        void ToStream(Stream outStream, object eventData);
    }
}
