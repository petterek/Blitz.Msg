namespace Itas.Infrastructure.MessageHost
{

    /// <summary>
    /// 
    /// </summary>
    public interface IRecivedMessageContext
    {
        /// <summary>
        /// 
        /// </summary>
        RecievedMessageData RecivedMessageData { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class MessageContextHolder : IRecivedMessageContext
    {
        /// <summary>
        /// 
        /// </summary>
        public RecievedMessageData RecivedMessageData { get; set; }
    }

}
