using System;
using System.Collections.Generic;

namespace Itas.Infrastructure.RabbitServer
{
 /// <summary>
 /// Configures the server...
 /// </summary>
 public class ServerConfig
    {
        /// <summary>
        /// 
        /// </summary>
        public class Info
        {
            /// <summary>
            /// 
            /// </summary>
            public string RoutingKey;
            
            /// <summary>
            /// 
            /// </summary>
            public Type Handler;
        }

        /// <summary>
        /// 
        /// </summary>
        public class ConectionInfo
        {
            public string Server;
            public string UserName;
            public string Password;
            public string ExchangeName;
            public string VirtualHost;
            public string ClientName;

            public string DeadLetterExchange { get => ExchangeName + "_DLE"; }
        }




        /// <summary>
        /// 
        /// </summary>
        public ServerConfig() { }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionInfo"></param>
        public ServerConfig(ConectionInfo connectionInfo)
        {
            this.connectionInfo = connectionInfo;

            if (string.IsNullOrWhiteSpace(connectionInfo.ExchangeName)) throw new MissingFieldException("ExchangeName can not be empty;");
            connectionInfo.ClientName = !string.IsNullOrWhiteSpace(connectionInfo.ClientName) ? connectionInfo.ClientName : System.Reflection.Assembly.GetEntryAssembly().FullName;

        }

        internal Dictionary<string, Info> Registrations = new Dictionary<string, Info>();

        /// <summary>
        /// 
        /// </summary>
        public readonly ConectionInfo connectionInfo;
        
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bindingKey"></param>
        /// <returns></returns>
        public Info RegisterCustomBinding<T>(string bindingKey) where T : IGenericEventHandler
        {
            var ret = new Info { RoutingKey = bindingKey, Handler = typeof(T) };
            Registrations.Add(ret.RoutingKey, ret);

            return ret;
        }





        /// <summary>
        /// Expects that the container contains an IEventHandler/<T/> registration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Info RegisterBinding<T>()
        {
            var i = new Info { RoutingKey = typeof(T).FullName, Handler = typeof(IEventHandler<>).MakeGenericType(typeof(T)) };
            Registrations.Add(i.RoutingKey, i);

            return i;
        }
    }
}
