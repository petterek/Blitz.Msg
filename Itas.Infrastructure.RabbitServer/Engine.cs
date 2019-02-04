using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Itas.Infrastructure.MessageHost
{
    /// <summary>
    /// 
    /// </summary>
    public class MessageHandlerEngine : IDisposable
    {

        private readonly IMessageAdapter producer;
        private Dictionary<Type, Type> HandlerTypes = new Dictionary<Type, Type>();


        Type GetHandlerType(Type messageType)
        {
            if (!HandlerTypes.ContainsKey(messageType))
            {
                HandlerTypes[messageType] = typeof(MessageHandler<>).MakeGenericType(messageType);
            }
            return HandlerTypes[messageType];
        }

        readonly ILogger<MessageHandlerEngine> logger;
        readonly IServiceProvider serviceProvider;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="producer"></param>
        /// <param name="serviceProvider"></param>
        /// <param name="logger"></param>
        public MessageHandlerEngine(IMessageAdapter producer, IServiceProvider serviceProvider, ILogger<MessageHandlerEngine> logger)
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger;
            this.producer = producer;

            producer.OnMessage += HandleTypedMessages;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <typeparam name="TMessageHandler"></typeparam>
        public void AttachMessageHandler<TMessage, TMessageHandler>() where TMessageHandler : MessageHandler<TMessage>
        {
            AttachMessageHandler(typeof(TMessage), typeof(TMessageHandler));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="messageHandler"></param>
        public void AttachMessageHandler(Type message, Type messageHandler)
        {
            producer.Bind(message.FullName, message, messageHandler);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TMessageHandler"></typeparam>
        /// <param name="messageName"></param>
        public void AttachGenericMessageHandler<TMessageHandler>(string messageName) where TMessageHandler : GenericMessageHandlerBase
        {
            AttachGenericMessageHandler(messageName, typeof(TMessageHandler));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageName"></param>
        /// <param name="handlerType"></param>
        public void AttachGenericMessageHandler(string messageName, Type handlerType)
        {
            producer.Bind(messageName, null, handlerType);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="message">The message revieved</param>
        /// <param name="handlerType">The handler to handle this</param>
        /// <param name="preHandle"></param>
        public void HandleTypedMessages(object message, Type handlerType, RecievedMessageData data)
        {

            var messageScope = serviceProvider.CreateScope().ServiceProvider;
            try
            {
                var c = messageScope.GetService<IRecivedMessageContext>();
                if (c != null)
                {
                    c.RecivedMessageData = data;
                }

                logger.LogTrace("Starting handler");
                var instance = (IMessageHandler)messageScope.GetService(handlerType);
                instance.Handle(message);
                logger.LogTrace("Message handled");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unable to handle message");
            }
            finally
            {
                if (messageScope is IDisposable)
                {
                    ((IDisposable)messageScope).Dispose();
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        public void StartServer()
        {
            producer.StartAdapter();
        }

        /// <summary>
        /// 
        /// </summary>
        public void StopServer()
        {
            producer.StopAdapter();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    StopServer();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~MessageHandlerEngine() {
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
