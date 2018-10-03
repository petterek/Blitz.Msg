using Itas.Infrastructure.Context;
using Itas.Infrastructure.MessageHandler;
using Itas.Infrastructure.Messaging.RabbitAdapter;
using Itas.Infrastructure.RabbitServer;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace RabbitHost.Test
{

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

    [TestFixture]
    public class SettingUpTheHost
    {
        [Test]
        public void SetupLooksGood()
        {
            var container = new SimpleFactory.Container();
            container.Register<MessageHandler<SomethingHasHappend>, MyHandler>();
            container.Register<MyGenericEventHandler>();

            IMessageAdapter adapter;
            //adapter = new RabbitMessageAdapter(new RabbitConectionInfo { UserName = "guest", Password = "guest", Server = "localhost", ExchangeName = "Simployer", ClientName = "MyTestingApp" }, new Serializer());

            adapter = new FakeAdapter(new List<object> { new SomethingHasHappend()}, new ClientContext { });
            
            var Server = new MessageHandlerEngine((t, c)=> container.CreateAnonymousInstance(t,c), adapter);
            Server.Register<SomethingHasHappend>();


            adapter.StartAdapter();


            //System.Threading.Thread.Sleep(10000);

            adapter.StopAdapter();
        }

        [Test]
        public void BindingKeyGetsCorrectOnTypes()
        {

            //var info = settings.RegisterBinding<SomethingHasHappend>();

            //Assert.AreEqual("RabbitHost.Test.SomethingHasHappend", info.RoutingKey);
            //Assert.AreEqual(typeof(Itas.Infrastructure.RabbitServer.MessageHandler<SomethingHasHappend>), info.MessageType);

        }

        [Test]
        public void BindingKeyGetsCorrectOnWildCards()
        {

            //var info = settings.RegisterCustomBinding<MyGenericEventHandler>("#");

            //Assert.AreEqual("#", info.RoutingKey);
            //Assert.AreEqual(typeof(MyGenericEventHandler), info.MessageType);

        }


    }

    
    public class MyGenericEventHandler : GenericEventHandlerBase
    {
        public MyGenericEventHandler()
        {

        }

        public override void Handle(RecievedEventInfo param)
        {

        }
    }


    public class MyHandler : Itas.Infrastructure.RabbitServer.MessageHandler<SomethingHasHappend>
    {
        private readonly ClientContext ctx;

        public MyHandler(ClientContext ctx)
        {
            this.ctx = ctx;
        }

        public override void Handle(SomethingHasHappend param)
        {
            
        }
    }

    public class SomethingHasHappend
    {
    }
}
