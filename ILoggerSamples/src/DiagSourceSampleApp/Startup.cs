using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Diagnostics.Context;
using Microsoft.Diagnostics.Correlation.Common.Instrumentation;
using Microsoft.Diagnostics.Correlation.AspNetCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SamplesHelpers;

namespace DiagSourceSampleApp
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();
            // Add framework services.
            services.AddMvc();
            services.AddSingleton(new HttpClient());
            services.AddSingleton<IOutgoingRequestNotifier<CorrelationContext, HttpRequestMessage, HttpResponseMessage>>(sp =>
            {
                var factory = sp.GetService<ILoggerFactory>();
                return new OutgoingRequestNotifier(factory.CreateLogger(nameof(OutgoingRequestNotifier)));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IApplicationLifetime applicationLifetime)
        {
            loggerFactory.WithFilter(new FilterLoggerSettings
                {
//                    {"Microsoft", LogLevel.Warning},
//                    {"System", LogLevel.Warning}
                })
                .AddDebug()
                .AddElasicSearch();

            app.UseCorrelationInstrumentation(Configuration.GetSection("Correlation"));

            var incomingRequestsHandler = registerIncomingRequestHandler(loggerFactory);
            applicationLifetime.ApplicationStopped.Register(() => {incomingRequestsHandler?.Dispose();});
            app.UseMvc();
        }

        private IDisposable registerIncomingRequestHandler(ILoggerFactory loggerFactory)
        {
            var observers = new Dictionary<string, IObserver<KeyValuePair<string, object>>>
                    {{"Microsoft.AspNetCore", new AspNetDiagnosticListenerObserver(loggerFactory.CreateLogger<AspNetDiagnosticListenerObserver>())}};

            return DiagnosticListener.AllListeners.Subscribe(new DiagnosticListenersObserver(observers));
        }
    }
}
