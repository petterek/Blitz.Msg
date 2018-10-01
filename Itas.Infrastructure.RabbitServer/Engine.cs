
using System;
using System.Collections.Generic;

namespace Itas.Infrastructure.RabbitServer
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

        public MessageHandlerEngine(Func<Type,object, object> handlerCreator, IMessageAdapter producer)
        {
            
            this.handlerCreator = handlerCreator;
            this.producer = producer;


            producer.OnMessage += HandleTypedMessages;
        }


        public void Register<TMessageType>()
        {
            producer.Bind(typeof(TMessageType).FullName, typeof(TMessageType));
        }
        public void RegisterExplicit<TMessageHandler>(string messageName) where TMessageHandler : GenericEventHandlerBase
        {
            producer.BindAnonymouse(messageName, typeof(TMessageHandler));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="context"></param>
        public void HandleTypedMessages(object message, object context)
        {
            var instance = (IMessageHandler)handlerCreator(GetHandlerType(message.GetType()), context);
            instance.Handle(message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void HandleAnonymousMessage(object message)
        {

        }


    }
}
