using Itas.Infrastructure.Context;
using Itas.Infrastructure.Messaging.RabbitAdapter;
using Itas.Infrastructure.RabbitServer;
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

            IMessageAdapter messageProducer = new RabbitMessageAdapter(new RabbitConectionInfo(), null, (e)=> new ClientContext());

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
