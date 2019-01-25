using System;
using System.Collections.Generic;

namespace Itas.Infrastructure.MessageHost
{
	/// <summary>
	/// 
	/// </summary>
	public class MessageHandlerEngine : IDisposable
	{
		private readonly Func<Type, object, object, object> _handlerCreator;
		private readonly IMessageAdapter _producer;
		private readonly Dictionary<Type, Type> _handlerTypes = new Dictionary<Type, Type>();

		Type GetHandlerType(Type messageType)
		{
			if (!_handlerTypes.ContainsKey(messageType))
				_handlerTypes[messageType] = typeof(MessageHandler<>).MakeGenericType(messageType);

			return _handlerTypes[messageType];
		}

		readonly Func<IServiceProvider> _createScope;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="producer"></param>
		/// <param name="createScope"></param>

		public MessageHandlerEngine(IMessageAdapter producer, Func<IServiceProvider> createScope)
		{
			_createScope = createScope;
			_producer = producer;
			producer.OnMessage += HandleTypedMessages;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="TMessage"></typeparam>
		/// <typeparam name="TMessageHandler"></typeparam>
		public void AttachMessageHandler<TMessage, TMessageHandler>() where TMessageHandler : MessageHandler<TMessage>
		{
			Type messageHandlerType = typeof(TMessageHandler);
			var messageType = typeof(TMessage);
			_producer.Bind(messageType.FullName, messageType, messageHandlerType);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="TMessageHandler"></typeparam>
		/// <param name="messageName"></param>
		public void AttachGenericMessageHandler<TMessageHandler>(string messageName) where TMessageHandler : GenericMessageHandlerBase
		{
			_producer.Bind(messageName, null, typeof(TMessageHandler));
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="message">The message received</param>
		/// <param name="handler">The handler to handle this</param>
		/// <param name="preHandle"></param>
		public void HandleTypedMessages(object message, Type handler, Action<IServiceProvider> preHandle)
		{

			var s = _createScope();
			try
			{
				preHandle(s);
				var instance = (IMessageHandler)s.GetService(handler);
				instance.Handle(message);
			}
			finally
			{
				if (s is IDisposable disposable)
				{
					disposable.Dispose();
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void StartServer()
		{
			_producer.StartAdapter();
		}

		/// <summary>
		/// 
		/// </summary>
		public void StopServer()
		{
			_producer.StopAdapter();
		}

		#region IDisposable Support
		private bool _disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposedValue)
			{
				if (disposing)
				{
					// TODO: dispose managed state (managed objects).
					StopServer();
				}

				// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
				// TODO: set large fields to null.

				_disposedValue = true;
			}
		}

		// TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
		// ~MessageHandlerEngine() {
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
