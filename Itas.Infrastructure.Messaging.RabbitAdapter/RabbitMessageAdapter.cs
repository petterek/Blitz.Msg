using Itas.Infrastructure.Context;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.IO;
using Itas.Infrastructure.MessageHost;
using Itas.Infrastructure.Messaging.Shared;

namespace Itas.Infrastructure.Messaging.RabbitConsumer
{
	public class RabbitMessageAdapter : IMessageAdapter
	{
		private readonly ConnectionFactory _connectionFactory;
		private ServerManagement _management;
		private readonly List<IModel> _channels = new List<IModel>();
		private readonly RabbitConnectionInfo _connectionInfo;
		private readonly ISerializer _serializer;
		private readonly Action<IServiceProvider, BasicDeliverEventArgs, object> _contextCreator;
		private readonly List<BindingInfo> _bindingInfos = new List<BindingInfo>();
		private ServerManagement.ExchangeInfo _globalErrorExchange;

		public event Action<object, Type, Action<IServiceProvider>> OnMessage;

		public RabbitMessageAdapter(RabbitConnectionInfo connectionInfo, ISerializer serializer, Action<IServiceProvider, BasicDeliverEventArgs, object> contextCreator)
		{
			_connectionFactory = new ConnectionFactory
			{
				HostName = connectionInfo.Server,
				UserName = connectionInfo.UserName,
				Password = connectionInfo.Password,
				VirtualHost = connectionInfo.VirtualHost ?? "/",
				AutomaticRecoveryEnabled = true
			};

			_connectionInfo = connectionInfo;
			_serializer = serializer;
			_contextCreator = contextCreator;
		}

		public void StartAdapter()
		{
			_management = new ServerManagement(
				_connectionInfo.ClientName,
				_connectionFactory.CreateConnection(_connectionInfo.ClientName),
				_connectionFactory.CreateConnection(_connectionInfo.ClientName),
				_serializer);

			//Create global Error Exchange, if not exists
			_globalErrorExchange = _management.CreateTopicExchange(_connectionInfo.ExchangeName + "_GlobalErrorsExchange");

			//Assert Dead Letter Exchange and Queue for this consumer.. 
			var dle = _management.CreateTopicExchange(_connectionInfo.ClientName + "_DeadLetterExchange");
			dle.CreateAndBindQueue(_connectionInfo.ClientName + "_DeadLetterQueue", "#");


			foreach (var bindingInfo in _bindingInfos)
			{
				var theQueue = _management.CreateQueueAndBind(
					bindingInfo.RoutingKey, 
					_connectionInfo.ExchangeName, 
					dle.name)
					.ConnectToExchange(_connectionInfo.ExchangeName, bindingInfo.RoutingKey);

				var model = _management.CreateChannel();
				model.BasicQos(0, 1, false);

				var consumer = new EventingBasicConsumer(model);
				_channels.Add(model);

				if (bindingInfo.MessageType != null)
				{
					consumer.Received += (a, eventArgs) =>
					{
						var message = _serializer.FromStream(new MemoryStream(eventArgs.Body), bindingInfo.MessageType);
						try
						{
							OnMessage(message, bindingInfo.HandlerType, sp => _contextCreator(sp, eventArgs, message));
							model.BasicAck(eventArgs.DeliveryTag, false);
						}
						catch (Exception)
						{
							model.BasicNack(eventArgs.DeliveryTag, false, false);
							SendToError(eventArgs);
						}
					};
				}
				else //Anonymouse handler.. Need to handle the 
				{
					consumer.Received += (a, eventArgs) =>
					{
						var message = new RecievedMessageData(eventArgs.Body, eventArgs.BasicProperties.Headers);
						try
						{
							OnMessage(message, bindingInfo.HandlerType, sp => _contextCreator(sp, eventArgs, message));
							model.BasicAck(eventArgs.DeliveryTag, false);
						}
						catch (Exception)
						{
							model.BasicNack(eventArgs.DeliveryTag, false, false);
						}
					};
				}
				model.BasicConsume(theQueue.name, false, consumer);
			}
		}

		private void SendToError(BasicDeliverEventArgs failedMessage)
		{
			var errorModel = new ErrorModel
			{
				CorrelationId = failedMessage.BasicProperties.CorrelationId,
				MessageType = failedMessage.RoutingKey,
				HandlerName = _connectionInfo.ClientName,
				ServerName = Environment.MachineName
			};

			_globalErrorExchange.SendMessage(errorModel);
		}

		public void StopAdapter()
		{
			_management.Dispose();
			foreach (var model in _channels)
			{
				model.Close();
			}
		}

		public void Bind(string routingKey, Type messageType, Type handlerType)
		{
			_bindingInfos.Add(new BindingInfo { RoutingKey = routingKey, MessageType = messageType, HandlerType = handlerType });
		}

		#region IDisposable Support
		private bool _disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposedValue)
			{
				if (disposing)
				{
					StopAdapter();

				}

				// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
				// TODO: set large fields to null.

				_disposedValue = true;
			}
		}

		// TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
		// ~RabbitMessageProducer() {
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

		private class BindingInfo
		{
			internal string RoutingKey;
			internal Type MessageType;
			internal Type HandlerType;
		}
	}

	public class ErrorModel
	{
		public string CorrelationId;
		public string MessageType;
		public string HandlerName;
		public string ServerName;
	}
}
