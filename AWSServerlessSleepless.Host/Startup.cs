using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using ServerlessSleepless.Awaker.Common.Logging;
using ServerlessSleepless.Host.Services;
using ServerlessSleepless.Host.Services.Interfaces;
using ServerlessSleepless.Logging;
using System;
using System.IO;

namespace ServerlessSleepless.Host
{
    public class Startup
    {
        public const string AppS3BucketKey = "AppS3Bucket";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            CustomLogFactory.LoggerFactory = new LoggerFactory();
            CustomLogFactory.LoggerFactory.AddProvider(new Log4netProvider(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log4net.config")));
        }

        public static IConfiguration Configuration { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                .AddNewtonsoftJson(opt =>
                {
                    opt.SerializerSettings.ContractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new CamelCaseNamingStrategy()
                    };
                });

            // Add S3 to the ASP.NET Core dependency injection framework.
            services.AddAWSService<Amazon.S3.IAmazonS3>();

            // Core Self-awake service
            services.AddSingleton<ISelfAwakeService>(new SelfAwakeService(Configuration,
                typeof(Awaker.AccessS3.SelfAwakeService).Assembly,
                typeof(Awaker.SQSLoop.SelfAwakeService).Assembly,
                typeof(Awaker.BurstCPU.SelfAwakeService).Assembly,
                typeof(Awaker.BurstMEM.SelfAwakeService).Assembly
            ));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapControllerRoute("default",
                    "{controller=Awake}/{action=Index}/{id?}");
            });
        }
    }
}
