using System;
using System.ComponentModel;

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
        event Action<object,Type, object> OnMessage;

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


    public class Example
    {
        private readonly IContainer container;

        public Example(IContainer container)
        {
            this.container = container;
        }

        public void DoSomething()
        {
            
        }
    }
}
