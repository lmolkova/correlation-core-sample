using System;
using System.Diagnostics.Tracing;
using System.Net.Http;
using Microsoft.Diagnostics.Correlation.Common;
using Microsoft.Diagnostics.Correlation.Common.Instrumentation;

namespace EventSourceSample
{
    public class OutgoingRequestNotifier : IOutgoingRequestNotifier<CorrelationContext, HttpRequestMessage, HttpResponseMessage>
    {
        public void OnBeforeRequest(CorrelationContext context, HttpRequestMessage request)
        {
            EventSource.SetCurrentThreadActivityId(new Guid(context.CorrelationId));
            MyEventSource.Log.DependencyStart(
                request.Method.ToString(),
                request.RequestUri.AbsoluteUri,
                context.CorrelationId,
                context.RequestId,
                context.ChildRequestId);
        }

        public void OnAfterResponse(CorrelationContext context, HttpResponseMessage response)
        {
            EventSource.SetCurrentThreadActivityId(new Guid(context.CorrelationId));
            MyEventSource.Log.DependencyStop(response.StatusCode.ToString());
        }
    }
}
