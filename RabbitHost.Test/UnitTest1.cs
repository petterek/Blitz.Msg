using NUnit.Framework;
using Itas.Infrastructure.RabbitServer;
using System;


namespace RabbitHost.Test
{
    [TestFixture]
    public class SettingUpTheHost
    {
        [Test]
        public void SetupLooksGood()
        {
            var container = new SimpleFactory.Container();
            container.Register<IEventHandler<SomethingHasHappend>, MyHandler>();
            container.Register<MyGenericEventHandler>();

            var Server = new Server(container,
                new ServerConfig.ConectionInfo { UserName="guest", Password="guest" , Server="localhost", ExchangeName="Simployer" , ClientName="MyTestingApp" }, c =>
            {

                c.RegisterBinding<SomethingHasHappend>();
                c.RegisterCustomBinding<MyGenericEventHandler>("#");

            });


            Server.Start();

            //System.Threading.Thread.Sleep(10000);

            Server.Stop();
        }

        [Test]
        public void BindingKeyGetsCorrectOnTypes()
        {
            var settings = new ServerConfig();

            var info = settings.RegisterBinding<SomethingHasHappend>();

            Assert.AreEqual("RabbitHost.Test.SomethingHasHappend", info.RoutingKey);
            Assert.AreEqual(typeof(IEventHandler<SomethingHasHappend>), info.Handler);

        }

        [Test]
        public void BindingKeyGetsCorrectOnWildCards()
        {
            var settings = new ServerConfig();

            var info = settings.RegisterCustomBinding<MyGenericEventHandler>("#");

            Assert.AreEqual("#", info.RoutingKey);
            Assert.AreEqual(typeof(MyGenericEventHandler), info.Handler);

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



    public class MyHandler : IEventHandler<SomethingHasHappend>
    {
        public void Handle(SomethingHasHappend param)
        {
            throw new NotImplementedException();
        }
    }

    public class SomethingHasHappend
    {
    }
}
