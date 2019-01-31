using Demo.Events;
using Itas.Infrastructure.MessageHost;
using Itas.Infrastructure.Messaging.AspnetcoreExtension;
using Itas.Infrastructure.Messaging.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;

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
            
            app.Run(ctx => ctx.Response.WriteAsync("Consuming RABBITMQ.."));

        }
    }


    public class SomethingOccuredHandler : MessageHandler<SomethingOccured>
    {
        public override void Handle(SomethingOccured param)
        {
            throw new NotImplementedException();
        }
    }
}
