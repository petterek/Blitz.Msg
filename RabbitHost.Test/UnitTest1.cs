using Itas.Infrastructure.Consumer;
using Itas.Infrastructure.Context;
using Itas.Infrastructure.Messaging.RabbitConsumer;
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
            container.Register<MyHandler>().Singleton();

            container.Register<MyGenericEventHandler>();

            IMessageAdapter adapter;

            adapter = new FakeAdapter(new List<object> { new SomethingHasHappend() }, new ClientContext { });

            var Server = new MessageHandlerEngine(adapter,
                () => new SimpleFactory.SimplefactoryProvider(container) 
                );

            Server.AttachMessageHandler<SomethingHasHappend, MyHandler>();
            Server.AttachGenericMessageHandler<MyGenericEventHandler>("#");


            adapter.StartAdapter();

            //System.Threading.Thread.Sleep(10000);

            adapter.StopAdapter();

            Assert.IsNotNull(((MyHandler)container.CreateInstance<MessageHandler<SomethingHasHappend>>()).input);
            Assert.AreEqual(1, ((MyHandler)container.CreateInstance<MessageHandler<SomethingHasHappend>>()).Counter);
        }

    }



    public class MyGenericEventHandler : GenericMessageHandlerBase
    {
        public MyGenericEventHandler()
        {

        }

        public override void Handle(RecievedMessageData param)
        {

        }
    }


    public class MyHandler : MessageHandler<SomethingHasHappend>
    {
        private readonly ClientContext ctx;
        public SomethingHasHappend input;
        public int Counter = 0;

        //public MyHandler(ClientContext ctx)
        //{
        //    this.ctx = ctx;
        //}

        public override void Handle(SomethingHasHappend param)
        {
            input = param;
            Counter++;
        }
    }

    public class SomethingHasHappend
    {
    }
}
