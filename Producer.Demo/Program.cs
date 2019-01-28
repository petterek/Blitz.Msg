using Demo.Events;
using Itas.Infrastructure.Messaging.RabbitProducer;
using System;
using System.IO;
using System.Threading.Tasks;
using Itas.Infrastructure.Messaging.Shared;

namespace Producer.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = new SimpleFactory.Container();
            var con = new RabbitConnectionInfo
            {
                ClientName = "Listner.Demo",
                ExchangeName = "Simployer",
                Server = "localhost",
                UserName = "guest",
                Password = "guest",
                VirtualHost = "/"
            };

            var pub = new PublishEventToRabbit(con, new Serializer());

            container.Register<PublishEventToRabbit>(() => pub).Singleton(); //This is singleton to hold the connection stuff for rabbit. Must be disposed
            container.Register<CustomPublisher>().Transient(); //This is the wrapper to capture the context of the current call
            container.Register<ApplicationContext>(); // this is the actual context.. Very simplefied :) 

            for (var x = 0; x < 10; x++)
            {
                var sender = container.CreateInstance<CustomPublisher>();
                sender.Publish(new SomethingOccured {Message=$"This is message number{x}" });
            }

            pub.Dispose();
        }
    }



    public class CustomPublisher
    {
        readonly PublishEventToRabbit toRabbit;
        readonly ApplicationContext context;

        public CustomPublisher(PublishEventToRabbit toRabbit, ApplicationContext context)
        {
            this.context = context;
            this.toRabbit = toRabbit;
        }


        public void Publish(object message)
        {
            var ctx = new RabbitEventContext {CorrelationId=context.CorrelationId, CustomerId=context.CompanyGuid, UserId=context.UserId};

            toRabbit.Publish(ctx, message);
        }

    }

    public class ApplicationContext
    {
        public Guid CorrelationId = Guid.NewGuid();
        public Guid UserId = Guid.NewGuid();
        public Guid CompanyGuid = Guid.NewGuid();
    }

    public class MyMessage
    {
        public int Number;
    }

    public class Serializer : Itas.Infrastructure.Context.ISerializer
    {
        public T FromStream<T>(Stream input) where T : new()
        {
            throw new NotImplementedException();
        }

        public object FromStream(Stream input, Type type)
        {
            throw new NotImplementedException();
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
}
