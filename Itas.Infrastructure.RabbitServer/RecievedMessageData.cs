using System.Collections.Generic;

namespace Itas.Infrastructure.MessageHost
{
  /// <summary>
  /// 
  /// </summary>
  public class RecievedMessageData
    {

        /// <summary>
        /// 
        /// </summary>
        public RecievedMessageData()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="payload"></param>
        public RecievedMessageData( byte[] payload)
        {
            Payload = payload;
        }


        /// <summary>
        /// 
        /// </summary>
        public string MessageName { get; protected set; }
        
        /// <summary>
        /// 
        /// </summary>
        public long Timestamp { get; protected set; }
        
        /// <summary>
        /// 
        /// </summary>
        public string CorrelationId { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public byte[] Payload { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();
    }
}
