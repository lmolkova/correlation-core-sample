using System;
using System.Net.Http;
using Microsoft.Diagnostics.Context;
using Microsoft.Diagnostics.Correlation.Common.Instrumentation;

namespace DiagnosticSourceSample
{
    public class OutgoingRequestNotifier : IOutgoingRequestNotifier<CorrelationContext, HttpRequestMessage, HttpResponseMessage>
    {
        public void OnBeforeRequest(CorrelationContext context, HttpRequestMessage request)
        {
            request.Properties.Add("start", DateTimeOffset.UtcNow);
        }

        public void OnAfterResponse(CorrelationContext context, HttpResponseMessage response)
        {
            DateTimeOffset startTime = (DateTimeOffset)response.RequestMessage.Properties["start"];
            MyDiagnosticSource.LogOutgoigRequest(response, DateTimeOffset.Now - startTime);
        }
    }
}
