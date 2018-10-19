using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Itas.Infrastructure.Consumer
{
  /// <summary>
  /// 
  /// </summary>
  public class RecievedMessageData
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="attributes"></param>
        public RecievedMessageData( byte[] payload, IDictionary<string, object> attributes)
        {
            Payload = payload;
            Attributes = attributes;
        }

        /// <summary>
        /// 
        /// </summary>
        public byte[] Payload { get; }
        
        /// <summary>
        /// 
        /// </summary>
        public IDictionary<string, object> Attributes { get; }
    }
}
