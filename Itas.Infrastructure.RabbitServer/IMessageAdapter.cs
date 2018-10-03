using System;

namespace Itas.Infrastructure.MessageHost
{
    /// <summary>
    /// 
    /// </summary>
    public interface IMessageAdapter : IDisposable
    {
        /// <summary>
        /// First argument should be the message, and the second one should be the context. 
        /// The context is injected into the Container when the handler is created.
        /// </summary>
        event Action<object, object> OnMessage;

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
