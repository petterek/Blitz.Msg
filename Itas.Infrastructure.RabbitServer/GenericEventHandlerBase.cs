namespace Itas.Infrastructure.RabbitServer
{
    public abstract class GenericEventHandlerBase : IGenericEventHandler
    {
        public abstract void Handle(RecievedEventInfo param);

    }
}
