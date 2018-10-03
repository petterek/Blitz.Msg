namespace Itas.Infrastructure.MessageHost
{

    /// <summary>
    /// Use this clss as the baseclass for your handlers. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class MessageHandler<T> : IMessageHandler
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="param"></param>
        public void Handle(object param)
        {
            Handle((T)param);
        }

        /// <summary>
        /// Must be overridden in your class. This is where you handle your event.. 
        /// </summary>
        /// <param name="param"></param>
        public abstract void Handle(T param);

    }

    /// <summary>
    /// 
    /// </summary>
    public interface IMessageHandler
    {
        /// <summary>
        /// Called internaly only.
        /// </summary>
        /// <param name="param"></param>
        void Handle(object param);
    }
}
