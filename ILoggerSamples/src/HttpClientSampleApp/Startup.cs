using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Diagnostics.Correlation.AspNetCore.Middleware;
using Microsoft.Diagnostics.Correlation.Common.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SamplesHelpers;

namespace HttpClientSampleApp
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();
            services.AddLogging();
            services.AddSingleton(serviceProvider =>
            {
                var logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger<LoggingHandler>();
                return CorrelationHttpClientBuilder.CreateClient(new LoggingHandler(logger));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.WithFilter(new FilterLoggerSettings
                {
//                    {"Microsoft", LogLevel.Warning},
//                    {"System", LogLevel.Warning}
                })
                .AddDebug()
                .AddElasicSearch();
            app.UseMiddleware<CorrelationContextTracingMiddleware>();
            app.UseMiddleware<IncomingRequestMiddleware>();
            app.UseMvc();
        }
    }
}
