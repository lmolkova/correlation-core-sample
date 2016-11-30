using System;
using System.Diagnostics;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Diagnostics.Context;
using Microsoft.Diagnostics.Correlation.Common.Http;

namespace DiagnosticSourceSample
{
    public class MyDiagnosticSource
    {
        public static DiagnosticSource Log = new DiagnosticListener("Microsoft-Diagnostics-Correlation-DiagnosticSource");

        public static void LogIncomingRequest(HttpContext context, TimeSpan elapsed)
        {
            Log.Write("incoming_request", new
            {
                context.Request,
                context.Response,
                Elapsed = elapsed,
                Context = ContextResolver.GetContext<CorrelationContext>()
            });
        }

        public static void LogIncomingRequestException(HttpContext context, Exception ex, TimeSpan elapsed)
        {
            Log.Write("incoming_request_exception", new
            {
                context.Request,
                context.Response,
                Elapsed = elapsed,
                Exception = ex,
                Context = ContextResolver.GetContext<CorrelationContext>()
            });
        }

        public static void LogOutgoigRequest(HttpResponseMessage response, TimeSpan elapsed)
        {
            var baseRequestContext = ContextResolver.GetContext<CorrelationContext>();
            var childRequestId = response.RequestMessage.GetChildRequestId();

            Log.Write("outgoing_request", new
            {
                Request = response.RequestMessage,
                Response = response,
                Elapsed = elapsed,
                Context = baseRequestContext.GetChildRequestContext(childRequestId)
            });
        }
    }
}
