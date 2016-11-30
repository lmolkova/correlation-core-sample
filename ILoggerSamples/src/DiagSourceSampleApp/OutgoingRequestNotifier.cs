using System;
using System.Net.Http;
using Microsoft.Diagnostics.Context;
using Microsoft.Diagnostics.Correlation.Common.Instrumentation;
using Microsoft.Extensions.Logging;
using SamplesHelpers;

namespace DiagSourceSampleApp
{
    public class OutgoingRequestNotifier : IOutgoingRequestNotifier<CorrelationContext, HttpRequestMessage, HttpResponseMessage>
    {
        private readonly ILogger logger;

        public OutgoingRequestNotifier(ILogger logger)
        {
            this.logger = logger;
        }

        public void OnBeforeRequest(CorrelationContext context, HttpRequestMessage request)
        {
            request.Properties.Add("start", DateTimeOffset.UtcNow);
        }

        public void OnAfterResponse(CorrelationContext context, HttpResponseMessage response)
        {
            DateTimeOffset startTime = (DateTimeOffset)response.RequestMessage.Properties["start"];
            logger.LogOutgoingRequest(response, DateTime.UtcNow - startTime);
        }
    }
}
