using System;
using System.Diagnostics;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Diagnostics.Correlation.Common;
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
                Context = ContextResolver.GetRequestContext<CorrelationContext>()
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
                Context = ContextResolver.GetRequestContext<CorrelationContext>()
            });
        }

        public static void LogOutgoigRequest(HttpResponseMessage response, TimeSpan elapsed)
        {
            var baseRequestContext = ContextResolver.GetRequestContext<CorrelationContext>();
            var childRequestId = response.RequestMessage.GetChildRequestId();

            Log.Write("outgoig_request", new
            {
                Request = response.RequestMessage,
                Response = response,
                Elapsed = elapsed,
                Context = baseRequestContext.GetChildRequestContext(childRequestId)
            });
        }
    }
}
