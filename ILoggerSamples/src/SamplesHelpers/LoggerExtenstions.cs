using System;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Diagnostics.Correlation.Common;
using Microsoft.Diagnostics.Correlation.Common.Http;
using Microsoft.Extensions.Logging;

namespace SamplesHelpers
{
    public static class LoggerExtenstions
    {
        public static void LogIncomingRequest(this ILogger logger, HttpContext context, TimeSpan elapsed)
        {
            logger.LogInformation(
                new EventId(0, "incoming_request"),
                "incoming request {method}, {url}, {status}, {elapsed}",
                context.Request.Method,
                context.Request.Path.Value,
                context.Response.StatusCode,
                elapsed.TotalMilliseconds);
        }

        public static void LogIncomingRequestException(this ILogger logger, HttpContext context, Exception ex, TimeSpan elapsed)
        {
            logger.LogError(
                new EventId(0, "incoming_request_exception"),
                "incoming request {method}, {url}, {exception}, {elapsed}",
                context.Request.Method,
                context.Request.Path.Value,
                ex,
                elapsed.TotalMilliseconds);
        }

        public static void LogOutgoingRequest(this ILogger logger, HttpResponseMessage response, TimeSpan elapsed)
        {
            var baseRequestContext = ContextResolver.GetRequestContext<CorrelationContext>();
            var childRequestId = response.RequestMessage.GetChildRequestId();
            using (logger.BeginScope(baseRequestContext.GetChildRequestContext(childRequestId)))
            {
                logger.LogInformation(
                    new EventId(0, "outgoing_request"),
                    "outgoing request {method}, {url}, {status} {elapsed}",
                    response.RequestMessage.Method.ToString(),
                    response.RequestMessage.RequestUri.AbsoluteUri,
                    response.StatusCode,
                    elapsed.TotalMilliseconds);
            }
        }
    }
}
