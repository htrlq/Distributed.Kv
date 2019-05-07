using KvServices.Context;
using KvServices.Repository;
using KvServices.Service;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Connections.Internal;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using System;

using WebApiClient.Extensions.HttpClientFactory;

namespace KvServices
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            #region Web Api Model State
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });
            #endregion

            #region Content
            services.AddDbContext<KvServiceContent>(options=>
            {
                options.UseSqlite("Data Source=kv.db");
            }, ServiceLifetime.Singleton);
            #endregion

            #region CalcHashcode
            services.AddSingleton<ICalcHashcode, CalcHashcode>();
            #endregion

            #region Repository
            services.AddSingleton<IKvServiceRepository, KvServiceRepository>();
            #endregion

            #region WebApiClient
            services.AddHttpClient();
            services.AddHttpApiTypedClient<IAgentService>(options=> {
                options.HttpHost = new Uri(Configuration.GetSection("Service").Get<string>());
            });
            #endregion

            #region Informat
            services.AddSingleton(serviceProvider =>
            {
                var server = serviceProvider.GetRequiredService<IServer>();
                return server.Features.Get<IServerAddressesFeature>();
            });
            services.AddSingleton<IInformat, Informat>();
            services.AddHostedService<AgentServer>();
            #endregion

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                var informat = serviceScope.ServiceProvider.GetRequiredService<IInformat>();
                var url = informat.ServiceUrl;
            }

            app.UseMvc();
        }
    }
}
