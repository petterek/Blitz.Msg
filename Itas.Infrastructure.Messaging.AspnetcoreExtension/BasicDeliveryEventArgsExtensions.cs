using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace Itas.Infrastructure.Messaging.AspnetcoreExtension
{
    public static class BasicDeliveryEventArgsExtensions
    {
        public static string GetCultureCode(this BasicDeliverEventArgs e)
        {
            return GetStringValueFromDictionary(e, "Culture");// HeaderNames.Culture);
        }

        public static Guid GetUserGuid(this BasicDeliverEventArgs e)
        {
            return GetGuidValueFromDictionary(e, "User");// HeaderNames.User);
        }

        public static Guid GetCustomerGuid(this BasicDeliverEventArgs e)
        {
            return GetGuidValueFromDictionary(e, "Company"); //HeaderNames.Company);
        }

        public static Guid GetCorrelationId(this BasicDeliverEventArgs e)
        {
            return GetGuidValue(e.BasicProperties.CorrelationId);
        }
                

        private static Guid GetGuidValue(string value)
        {
	        Guid.TryParse(value, out var ret);

	        return ret;
        }

		private static Guid GetGuidValueFromDictionary(this BasicDeliverEventArgs e, string headerName)
        {
            return e.BasicProperties.Headers.ContainsKey(headerName)
                ? Guid.Parse(Encoding.UTF8.GetString((byte[])e.BasicProperties.Headers[headerName]))
                : Guid.Empty;
        }

        private static string GetStringValueFromDictionary(this BasicDeliverEventArgs e, string headerName)
        {
            return e.BasicProperties.Headers.ContainsKey(headerName)
                ? e.BasicProperties.Headers[headerName].ToString()
                : string.Empty;
        }
    }
}
