using System;
using System.Collections.Concurrent;
using System.Net.Http;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Diagnostics.Correlation.Common;
using Microsoft.Diagnostics.Correlation.Common.Http;
using Microsoft.Diagnostics.Correlation.Common.Instrumentation;

namespace AISample
{
    public class OutgoingRequestNotifier : IOutgoingRequestNotifier<CorrelationContext, HttpRequestMessage, HttpResponseMessage>
    {
        private readonly TelemetryClient aiClient;
        private readonly ConcurrentDictionary<HttpRequestMessage, DateTimeOffset> requests;

        public OutgoingRequestNotifier()
        {
            this.aiClient = new TelemetryClient();
            requests = new ConcurrentDictionary<HttpRequestMessage, DateTimeOffset>();
        }

        public void OnBeforeRequest(CorrelationContext context, HttpRequestMessage request)
        {
            requests[request] = DateTime.UtcNow;
        }

        public void OnAfterResponse(CorrelationContext context, HttpResponseMessage response)
        {
            DateTimeOffset startTime;
            DateTimeOffset endTime = DateTimeOffset.Now;
            requests.TryRemove(response.RequestMessage, out startTime);
            DependencyTelemetry telemetry = new DependencyTelemetry(
                response.RequestMessage.RequestUri.Host,
                response.RequestMessage.RequestUri.LocalPath,
                DateTimeOffset.Now,
                endTime - startTime,
                response.IsSuccessStatusCode) {Id = response.RequestMessage.GetChildRequestId()};
            aiClient.TrackDependency(telemetry);
        }
    }
}
