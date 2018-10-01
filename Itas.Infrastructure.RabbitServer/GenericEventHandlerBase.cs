namespace Itas.Infrastructure.RabbitServer
{
    /// <summary>
    /// Inherit from this to handle generic messages.. 
    /// </summary>
    public abstract class GenericEventHandlerBase : MessageHandler<RecievedEventInfo>
    {
    }
}
