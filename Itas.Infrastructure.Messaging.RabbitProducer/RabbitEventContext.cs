using System;

namespace Itas.Infrastructure.Messaging.RabbitProducer
{
    /// <summary>
    /// Data needed to ensure 
    /// </summary>
    public class RabbitEventContext
    {
        public Guid CorrelationId;
        public Guid CustomerId;
        public Guid UserId;
    }
}
