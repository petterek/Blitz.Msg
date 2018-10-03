namespace Itas.Infrastructure.MessageHost
{
    /// <summary>
    /// Inherit from this to handle generic messages.. 
    /// </summary>
    public abstract class GenericMessageHandlerBase : MessageHandler<RecievedMessageData>
    {
    }
}
