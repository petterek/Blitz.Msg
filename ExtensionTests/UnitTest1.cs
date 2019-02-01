using Itas.Infrastructure.MessageHost;
using Itas.Infrastructure.Messaging.AspnetcoreExtension;
using NUnit.Framework;

namespace Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            var cnf = new MessageConfig();

            Assert.DoesNotThrow(()=> cnf.ConsumeMessage<AMessage, AMessageHandler>());


        }
    }


    public class AMessage
    {

    }


    public class AMessageHandler : MessageHandler<AMessage>
    {
        public override void Handle(AMessage param)
        {
            throw new System.NotImplementedException();
        }
    }
}