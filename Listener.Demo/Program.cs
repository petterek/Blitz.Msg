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
            //Use any container.. 
            var container = new SimpleFactory.Container(LifeTimeEnum.PerGraph);
            container.Register<MessageHandler<MyEventClass>, MyHandler>();
            container.Register<GenericEventHandler>();

            //Connectioninfo to the rabbit server. 
            //The ClientName is important, as it is used in the infrastructure to indentify the host. 
            RabbitConnectionInfo connectionInfo = new RabbitConnectionInfo { UserName = "guest", Password = "guest", Server = "localhost", ExchangeName = "Simployer", ClientName = "MyTestingApp" };

            //Create the RabbitAdapter. This is a spesific implementation for Rabbit.
            IMessageAdapter messageAdapter = new RabbitMessageAdapter(
                connectionInfo, 
                //The serializer that will be used by the adapter. This must implement the ISerializer from Itas.Infrastructure.
                new Serializer(), 
                //This Func<BasicDeliveryEventArgs> gives you the chance to create a context value for your eventhandler.
                //Setting the ClientContext e.g
                (e)=> new ClientContext {
                    CorrelationId =Guid.Parse(e.BasicProperties.CorrelationId),
                    CompanyGuid = Guid.Parse(e.BasicProperties.Headers[Itas.Infrastructure.Context.HeaderNames.User].ToString()) }
                );

            //Then instanciate the MessageHandler.. Passing in the Adapter. 
            var server = new MessageHandlerEngine(
                messageAdapter,
                //This Func<Type,object> is used instead of taking a dependency on a Container. 
                //Here you can create your scope to for your context
                (t,c)=> container.CreateAnonymousInstance(t,c));

            //Register a typed handler for the Engine. 
            //The engine will ask for an instance of  MessageHandle<MyEventClass> using the above Action<Type,object>. 
            server.Register<MyEventClass>();
            
            //Registering an untyped handler. 
            //Will ask for an instance of the type mapped against this bindingkey. 
            server.RegisterExplicit<GenericEventHandler>("#");

            //Start the adapter. 
            //The infrastructure will be created on the rabbit server and the adapter will start to recieve the messages. 
            messageAdapter.StartAdapter();

            Console.ReadLine();

            //Stop the Adapter to dispose the connections to Rabbit. 
            messageAdapter.StopAdapter();            
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
