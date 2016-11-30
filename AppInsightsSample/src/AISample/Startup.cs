using System.Net.Http;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Diagnostics.Context;
using Microsoft.Diagnostics.Correlation.AspNetCore;
using Microsoft.Diagnostics.Correlation.Common.Instrumentation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AISample
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsEnvironment("Development"))
            {
                // This will push telemetry data through Application Insights pipeline faster, allowing you to view results immediately.
                builder.AddApplicationInsightsSettings(developerMode: true);
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddApplicationInsightsTelemetry(Configuration);
            services.AddSingleton(new HttpClient());
            services.AddSingleton<ITelemetryInitializer, CorrelationTelemetryInitializer>();
            services.AddSingleton<IOutgoingRequestNotifier<CorrelationContext, HttpRequestMessage, HttpResponseMessage>>(new OutgoingRequestNotifier());
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IApplicationLifetime applicationLifetime)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseCorrelationInstrumentation(Configuration.GetSection("Correlation"));
            app.UseApplicationInsightsRequestTelemetry();
            app.UseApplicationInsightsExceptionTelemetry();

            app.UseMvc();
        }
    }

    public class CorrelationTelemetryInitializer : ITelemetryInitializer
    {
        public void Initialize(ITelemetry telemetry)
        {
            //add request id to every event
            var ctx = ContextResolver.GetContext<CorrelationContext>();
            if (ctx != null)
            {
                telemetry.Context.Operation.Id = ctx.CorrelationId;
                telemetry.Context.Operation.ParentId = ctx.RequestId;
            }
        }
    }
}
