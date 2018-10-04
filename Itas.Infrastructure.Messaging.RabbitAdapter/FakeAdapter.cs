using Itas.Infrastructure.Context;
using Itas.Infrastructure.MessageHost;
using System;
using System.Collections.Generic;

namespace Itas.Infrastructure.Messaging.RabbitAdapter
{
    public class FakeAdapter : IMessageAdapter
    {

        public FakeAdapter(List<object> messages, ClientContext ctx)
        {
            this.messages = messages;
            this.ctx = ctx;
        }

        public event Action<object, Type, object> OnMessage;

        private Dictionary<string, Type> handlerBindings = new Dictionary<string, Type>();

        public void Bind(string routingKey, Type messageType, Type handler)
        {
            if (!handlerBindings.ContainsKey(routingKey))
            {
                handlerBindings.Add(routingKey, handler);
            }
        }


        public void StartAdapter()
        {
            foreach (var o in messages)
            {
                if (handlerBindings.ContainsKey(o.GetType().FullName))
                {
                    OnMessage(o, handlerBindings[o.GetType().FullName], ctx);
                }
                //We are missing support for wildcards.. 
            }
        }

        public void StopAdapter()
        {

        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls
        private readonly List<object> messages;
        private readonly ClientContext ctx;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~FakeAdapter() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }


        #endregion
    }

}
