using Demo.Events;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Producer.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            var con = new Itas.Infrastructure.Messaging.RabbitProducer.RabbitConnectionInfo
            {
                ClientName = "Listner.Demo",
                ExchangeName = "Simployer",
                Server = "localhost",
                UserName = "guest",
                Password = "guest",
                VirtualHost = "/"
            };

            var pub = new Itas.Infrastructure.Messaging.RabbitProducer.PublishEventToRabbit(con, new Serializer());

            for (var x = 0; x < 10; x++)
            {
                pub.Publish(
                    new Itas.Infrastructure.Messaging.RabbitProducer.RabbitEventContext { CorrelationId = Guid.NewGuid(), CustomerId = Guid.NewGuid(), UserId = Guid.NewGuid() },
                    new SomethingOccured { Hallo = x, Message = "Heisann" }
                    );
            }

            pub.Dispose();
        }
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
