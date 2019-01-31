using System;
using System.IO;

namespace Itas.Infrastructure.Messaging.RabbitConsumer
{
    public interface ISerializer
    {
        object FromStream(Stream stream, Type messageType);
        void ToStream(Stream stream, object message);
    }
}