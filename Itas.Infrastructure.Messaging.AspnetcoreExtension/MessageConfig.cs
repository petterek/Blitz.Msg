using Itas.Infrastructure.MessageHost;
using Itas.Infrastructure.Messaging.Shared;
using System;
using System.Collections.Generic;

namespace Itas.Infrastructure.Messaging.AspnetcoreExtension
{
    public class MessageConfig
    {
        
        public class HandlerInfo
        {
            public Type Message;
            public Type Handler;
            public string BindingKey;
        }

        public List<HandlerInfo> MessageHandlers = new List<HandlerInfo>();
             
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <typeparam name="THandler"></typeparam>
        public void ConsumeMessage<TMessage, THandler>() where THandler : MessageHandler<TMessage>
        {
            ConsumeMessage(typeof(TMessage).FullName, typeof(TMessage), typeof(THandler));
        }
        

        /// <summary>
        /// Register a message without a handler, the handler has to be registerd in the container manually, has to be of type <seealso cref="MessageHandler{T}"/>
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        public void ConsumeMessage<TMessage>() 
        {
            ConsumeMessage(typeof(TMessage).FullName,typeof(TMessage), null);
        }

        

        /// <summary>
        /// Will attache to the given exchange, with the routing key as binding. 
        /// The message will be transformed into a generic message containing the body as <seealso cref="byte[]"/>
        /// </summary>
        /// <typeparam name="THandler"></typeparam>
        /// <param name="routingKey"></param>
        public void ConsumeAnonymouseMessage<THandler>(string routingKey) where THandler : GenericMessageHandlerBase
        {
            ConsumeMessage(routingKey, null, typeof(THandler));
        }


        void ConsumeMessage(string bindingKey, Type message, Type handler)
        {
            if(handler != null & message != null)
            {
                if (!(typeof(MessageHandler<>).MakeGenericType(message)).IsAssignableFrom(handler))
                {
                    throw new ArgumentOutOfRangeException(nameof(handler), $"Must be of type {typeof(MessageHandler<>).MakeGenericType(message).FullName} ");
                }
            }
            

            MessageHandlers.Add(new HandlerInfo {BindingKey = bindingKey, Handler= handler, Message= message });
        }
    }
}
