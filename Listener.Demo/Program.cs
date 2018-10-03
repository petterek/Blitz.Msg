using Itas.Infrastructure.Context;
using Itas.Infrastructure.MessageHost;
using Itas.Infrastructure.Messaging.RabbitAdapter;
using SimpleFactory.Contract;
using System;

namespace Listener.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = new SimpleFactory.Container(LifeTimeEnum.PerGraph);
            container.Register<MessageHandler<MyEventClass>, MyHandler>();

            RabbitConectionInfo connectionInfo = new RabbitConectionInfo { UserName = "guest", Password = "guest", Server = "localhost", ExchangeName = "Simployer", ClientName = "MyTestingApp" };
            IMessageAdapter messageProducer = new RabbitMessageAdapter(connectionInfo, null, (e)=> new ClientContext());

            var server = new MessageHandlerEngine((t,c)=> container.CreateAnonymousInstance(t,c), messageProducer);
            server.Register<MyEventClass>();

            messageProducer.StartAdapter();

            Console.ReadLine();

            messageProducer.StopAdapter();            
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
    }



  

   
        
    
}
