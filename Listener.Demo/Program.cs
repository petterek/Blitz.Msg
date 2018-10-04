using Itas.Infrastructure.Context;
using Itas.Infrastructure.MessageHost;
using Itas.Infrastructure.Messaging.RabbitAdapter;
using SimpleFactory.Contract;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Listener.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = new SimpleFactory.Container(LifeTimeEnum.PerGraph);
            container.Register<MessageHandler<MyEventClass>, MyHandler>();
            container.Register<GenericEventHandler>();

            RabbitConectionInfo connectionInfo = new RabbitConectionInfo { UserName = "guest", Password = "guest", Server = "localhost", ExchangeName = "Simployer", ClientName = "MyTestingApp" };
            IMessageAdapter messageProducer = new RabbitMessageAdapter(connectionInfo, new Serializer(), (e)=> new ClientContext());

            var server = new MessageHandlerEngine((t,c)=> container.CreateAnonymousInstance(t,c), messageProducer);
            server.Register<MyEventClass>();
            server.RegisterExplicit<GenericEventHandler>("#");

            messageProducer.StartAdapter();

            Console.ReadLine();

            messageProducer.StopAdapter();            
        }
                     
       
    }

    public class Serializer : ISerializer
    {
        public T FromStream<T>(Stream input) where T : new()
        {
            return miniJson.Parser.StringToObject<T>(input);
        }

        public object FromStream(Stream input, Type type)
        {
            return miniJson.Parser.StringToObject(input, type);
        }

        public Task<T> FromStreamAsync<T>(Stream input) where T : new()
        {
            throw new NotImplementedException();
        }

        public Task<object> FromStreamAsync(Stream input, Type type)
        {
            throw new NotImplementedException();
        }

        public void ToStream(Stream output, object data)
        {
            miniJson.Writer.ObjectToString(output, data);
        }

        public Task ToStreamAsync(Stream output, object data)
        {
            throw new NotImplementedException();
        }
    }

    public class MyEventClass
    {
        public string Data;
    }

    internal class MyHandler : MessageHandler<MyEventClass>
    {

        public override void Handle(MyEventClass param)
        {
            throw new NotImplementedException();
        }
    }


    public class GenericEventHandler : GenericMessageHandlerBase
    {
        public override void Handle(RecievedMessageData param)
        {
            Console.WriteLine(param.GetType());
        }
    }


}
