using Itas.Infrastructure.Context;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;

namespace Itas.Infrastructure.Messaging.RabbitProducer
{


    /// <summary>
    /// This is a wrapper class to support publishing events/messages to RabbitMQ
    /// This class should be singleton
    /// </summary>
    public class PublishEventToRabbit : IDisposable
    {


        ConnectionFactory factory;
        IConnection rabbitConnection;
        IModel Channel;
        readonly RabbitConnectionInfo connectionInfo;
        readonly ISerializer serializer;

        public PublishEventToRabbit(RabbitConnectionInfo connectionInfo, ISerializer serializer)
        {
            if (serializer == null)
                throw new ArgumentNullException(nameof(serializer));
            if (connectionInfo == null)
                throw new ArgumentNullException(nameof(connectionInfo));

            if (string.IsNullOrEmpty(connectionInfo.ClientName)) throw new ArgumentException("Missing connectionInfo.ClientName");


            this.serializer = serializer;
            this.connectionInfo = connectionInfo;
            factory = new RabbitMQ.Client.ConnectionFactory();
            factory.UserName = connectionInfo.UserName;
            factory.Password = connectionInfo.Password;
            factory.HostName = connectionInfo.Server;
            factory.AutomaticRecoveryEnabled = true;

            factory.VirtualHost = connectionInfo.VirtualHost;

            rabbitConnection = factory.CreateConnection(connectionInfo.ClientName);
            Channel = rabbitConnection.CreateModel();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="ArgumentException">If context.CorrelationId == Guid.Empty</exception>
        /// <param name="context"></param>
        /// <param name="eventData"></param>
        public void Publish(RabbitEventContext context, object eventData)
        {
            if (context.CorrelationId == Guid.Empty) throw new ArgumentException("Missing correlationid");
            if (context.CustomerId == Guid.Empty) throw new ArgumentException("Missing CustomerId");
            if (context.UserId == Guid.Empty) throw new ArgumentException("Missing UserId");


            

            var props = Channel.CreateBasicProperties();
            props.Headers = new Dictionary<string, object>();

            props.DeliveryMode = 2;
            props.ContentType = "application/json";
            props.ContentEncoding = "UTF-8";

            props.Headers.Add(HeaderNames.Company, context.CustomerId.ToString());
            props.Headers.Add(HeaderNames.User, context.UserId.ToString());

            props.AppId = connectionInfo.ClientName;
            props.CorrelationId = context.CorrelationId.ToString();

            var outStream = new System.IO.MemoryStream();
            serializer.ToStream(outStream, eventData);
            outStream.Flush();
            outStream.Position = 0;

            var body = outStream.ToArray();

            

            //This is maybe not the best way, but it is an easy way.. this is threadsafe.. 
            lock (Channel)
            {
                Channel.BasicPublish(connectionInfo.ExchangeName, eventData.GetType().FullName, props,body );
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    if (Channel.IsOpen)
                    {
                        Channel.Close();
                    }
                    if (rabbitConnection.IsOpen)
                    {
                        rabbitConnection.Close();
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
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

}
