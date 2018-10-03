using System.Collections.ObjectModel;

namespace Itas.Infrastructure.MessageHandler
{
    public class RecievedEventInfo
    {
        public string RoutingKey { get; }
        public string Body { get; }
        public ReadOnlyDictionary<string, string> Attributes { get; }
    }

}
