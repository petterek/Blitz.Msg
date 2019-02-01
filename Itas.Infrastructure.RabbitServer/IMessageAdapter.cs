using System;

namespace Itas.Infrastructure.MessageHost
{
    /// <summary>
    /// 
    /// </summary>
    public interface IMessageAdapter : IDisposable
    {

        

        /// <summary>
        /// First argument should be the message,second is the handlertype for this message, and the third is the context. 
        /// The context is injected into the Container when the handler is created.
        /// 
        /// </summary>
        event Action<object,Type, RecievedMessageData> OnMessage;

        /// <summary>
        /// 
        /// </summary>
        void StartAdapter();

        /// <summary>
        /// 
        /// </summary>
        void StopAdapter();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="routingKey"></param>
        /// <param name="messageType"></param>
        /// <param name="handler"></param>
        void Bind(string routingKey,Type messageType, Type handler);
    }

}
