using Itas.Infrastructure.MessageHost;
using Itas.Infrastructure.Messaging.Shared;
using System;
using System.Collections.Generic;

namespace Itas.Infrastructure.Messaging.AspnetcoreExtension
{
    public class MessageConfig
    {
        
        public Dictionary<Type, Type> MessageHandlers = new Dictionary<Type, Type>();

        public void ConsumeMessage<T, THandler>() where THandler : MessageHandler<T>
        {
            ConsumeMessage(typeof(T), typeof(THandler));
        }

        /// <summary>
        /// Register a message without a handler, the handler has to be registerd in the container manually, has to be of type <seealso cref="MessageHandler{T}"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void ConsumeMessage<T>() 
        {
            ConsumeMessage(typeof(T), null);
        }

        void ConsumeMessage(Type message, Type handler)
        {
            if(handler != null)
            {
                if (!(typeof(MessageHandler<>).MakeGenericType(message)).IsAssignableFrom(handler))
                {
                    throw new ArgumentOutOfRangeException(nameof(handler), $"Must be of type {typeof(MessageHandler<>).MakeGenericType(message).FullName} ");
                }
            }
            

            MessageHandlers.Add(message, handler);
        }
    }
}
