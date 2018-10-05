
using System;
using System.Collections.Generic;

namespace Itas.Infrastructure.MessageHost
{
    /// <summary>
    /// 
    /// </summary>
    public class MessageHandlerEngine
    {

        private readonly Func<Type, object, object> handlerCreator;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="handlerCreator"></param>
        /// <param name="producer"></param>

        public MessageHandlerEngine(IMessageAdapter producer,Func<Type, object, object> handlerCreator )
        {

            this.handlerCreator = handlerCreator;
            this.producer = producer;


            producer.OnMessage += HandleTypedMessages;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TMessageType"></typeparam>
        public void Register<TMessageType>()
        {
            producer.Bind(typeof(TMessageType).FullName, typeof(TMessageType), GetHandlerType(typeof(TMessageType)));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TMessageHandler"></typeparam>
        /// <param name="messageName"></param>
        public void RegisterExplicit<TMessageHandler>(string messageName) where TMessageHandler : GenericMessageHandlerBase
        {
            producer.Bind(messageName, null, typeof(TMessageHandler));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="message">The message revieved</param>
        /// <param name="handler">The handler to handle this</param>
        /// <param name="context">The object holding the context for this call</param>
        public void HandleTypedMessages(object message,Type handler, object context)
        {
            var instance = (IMessageHandler)handlerCreator(handler, context);
            instance.Handle(message);
        }
        
    }
}
