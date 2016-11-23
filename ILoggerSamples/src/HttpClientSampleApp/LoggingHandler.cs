using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SamplesHelpers;

namespace HttpClientSampleApp
{
    public class LoggingHandler : DelegatingHandler
    {
        private readonly ILogger logger;
        public LoggingHandler(ILogger logger)
        {
            this.logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            logger.LogOutgoingRequest(response, sw.Elapsed);
            return response;
        }
    }
}
