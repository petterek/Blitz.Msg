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

        readonly Func<IServiceProvider> createScope;
               
        /// <summary>
        /// 
        /// </summary>
        /// <param name="producer"></param>
        /// <param name="createScope"></param>
        public MessageHandlerEngine(IMessageAdapter producer, Func<IServiceProvider> createScope)
        {
            this.createScope = createScope;
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
            Type messageHandlerType = typeof(TMessageHandler);
            var messageType = typeof(TMessage);
            producer.Bind(messageType.FullName, messageType, messageHandlerType);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TMessageHandler"></typeparam>
        /// <param name="messageName"></param>
        public void AttachGenericMessageHandler<TMessageHandler>(string messageName) where TMessageHandler : GenericMessageHandlerBase
        {
            producer.Bind(messageName, null, typeof(TMessageHandler));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="message">The message revieved</param>
        /// <param name="handler">The handler to handle this</param>
        /// <param name="preHandle"></param>
        public void HandleTypedMessages(object message, Type handler, Action<IServiceProvider> preHandle )
        {
                       
            var s = createScope();
            try
            {
                preHandle(s);
                var instance = (IMessageHandler)s.GetService(handler) ;
                instance.Handle(message);
            }
            finally
            {
                if (s is IDisposable)
                {
                    ((IDisposable)s).Dispose();
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
