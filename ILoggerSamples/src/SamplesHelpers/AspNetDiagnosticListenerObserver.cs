using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace SamplesHelpers
{
    public class AspNetDiagnosticListenerObserver : IObserver<KeyValuePair<string, object>>
    {
        private readonly ILogger logger;

        public AspNetDiagnosticListenerObserver(ILogger logger)
        {
            this.logger = logger;
        }

        public void OnNext(KeyValuePair<string, object> value)
        {
            if (value.Key == "Microsoft.AspNetCore.Hosting.BeginRequest")
            {
                var httpContextInfo = value.Value.GetType().GetProperty("httpContext");
                var httpContext = (DefaultHttpContext) httpContextInfo?.GetValue(value.Value, null);
                httpContext?.Items.Add("start", DateTimeOffset.UtcNow);
            }
            else if (value.Key == "Microsoft.AspNetCore.Hosting.EndRequest")
            {
                var httpContextInfo = value.Value.GetType().GetProperty("httpContext");
                var httpContext = (DefaultHttpContext)httpContextInfo?.GetValue(value.Value, null);
                if (httpContext != null)
                {
                    var start = (DateTimeOffset)httpContext.Items["start"];
                    logger.LogIncomingRequest(httpContext, DateTimeOffset.UtcNow - start);
                }
            }
        }

        public void OnCompleted() { }

        public void OnError(Exception error) { }
    }

}
