using Demo.Events;
using Itas.Infrastructure.MessageHost;
using Itas.Infrastructure.Messaging.AspnetcoreExtension;
using Itas.Infrastructure.Messaging.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Listner.Aspnet.Demo
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.ConfigureRabbitMessageConsumer(
                new RabbitConnectionInfo { UserName = "guest", Password = "guest", Server = "localhost", ExchangeName = "Simployer", ClientName = "AspListning" },
                c =>
                {
                    c.ConsumeMessage<SomethingOccured, SomethingOccuredHandler>();
                    c.ConsumeMessage<SomethingElse, SomethingElseHappend>();
                }
            );
        }
        
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
                       
            
            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.Run(ctx => ctx.Response.WriteAsync("Consuming RABBITMQ.."));

        }
    
    }
    
    public class SomethingOccuredHandler : MessageHandler<SomethingOccured>
    {
        readonly ILogger<SomethingOccuredHandler> logger;

        public SomethingOccuredHandler(ILogger<SomethingOccuredHandler> logger)
        {
            this.logger = logger;
        }
        public override void Handle(SomethingOccured param)
        {
            logger.LogInformation("Handle event");
        }
    }

    public class SomethingElseHappend : MessageHandler<SomethingElse>
    {
        readonly IRecivedMessageContext messageContext;
        readonly ILogger<SomethingElseHappend> logger;

        public SomethingElseHappend(IRecivedMessageContext messageContext, ILogger<SomethingElseHappend> logger)
        {
            this.logger = logger;
            this.messageContext = messageContext;
        }

        public override void Handle(SomethingElse param)
        {
            logger.LogInformation(param.Value.ToString());
            logger.LogInformation(System.Text.Encoding.UTF8.GetString(messageContext.RecivedMessageData.Payload));
        }
    }
}
