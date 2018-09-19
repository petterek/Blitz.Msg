namespace Itas.Infrastructure.RabbitServer
{
    public interface IEventHandler<T>
    {
        void Handle(T param);
    }
}
