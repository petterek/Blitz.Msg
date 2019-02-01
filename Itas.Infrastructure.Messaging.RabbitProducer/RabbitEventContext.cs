using System;
using System.Collections.Generic;

namespace Itas.Infrastructure.Messaging.RabbitProducer
{
    /// <summary>
    /// Data needed to ensure 
    /// </summary>
    public class RabbitEventContext
    {
        public Guid CorrelationId { get; set; }
        public Dictionary<string, string> ContextValues = new Dictionary<string, string>();
    }
}
