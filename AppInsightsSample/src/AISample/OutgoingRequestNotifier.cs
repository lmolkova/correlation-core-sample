using System;
using System.Net.Http;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Diagnostics.Context;
using Microsoft.Diagnostics.Correlation.Common.Http;
using Microsoft.Diagnostics.Correlation.Common.Instrumentation;

namespace AISample
{
    public class OutgoingRequestNotifier : IOutgoingRequestNotifier<CorrelationContext, HttpRequestMessage, HttpResponseMessage>
    {
        private readonly TelemetryClient aiClient;

        public OutgoingRequestNotifier()
        {
            this.aiClient = new TelemetryClient();
        }

        public void OnBeforeRequest(CorrelationContext context, HttpRequestMessage request)
        {
            request.Properties.Add("start", DateTimeOffset.UtcNow);
        }

        public void OnAfterResponse(CorrelationContext context, HttpResponseMessage response)
        {
            DateTimeOffset startTime = (DateTimeOffset) response.RequestMessage.Properties["start"];
            DateTimeOffset endTime = DateTimeOffset.Now;
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
