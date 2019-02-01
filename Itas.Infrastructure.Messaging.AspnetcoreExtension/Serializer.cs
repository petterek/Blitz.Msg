using Itas.Infrastructure.Messaging.RabbitConsumer;
using System;
using System.IO;

namespace Itas.Infrastructure.Messaging.AspnetcoreExtension
{
    public class Serializer : ISerializer
    {

        public object FromStream(Stream input, Type type)
        {
            return miniJson.Parser.StringToObject(input, type);
        }

        public void ToStream(Stream output, object data)
        {
            miniJson.Writer.ObjectToString(output, data);
        }

    }
}
