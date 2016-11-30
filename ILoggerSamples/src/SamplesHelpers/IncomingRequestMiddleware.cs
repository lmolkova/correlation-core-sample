using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace SamplesHelpers
{
    public class IncomingRequestMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger logger;

        public IncomingRequestMiddleware(RequestDelegate next, ILogger<IncomingRequestMiddleware> logger)
        {
            this.next = next;
            this.logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                await this.next(context).ConfigureAwait(false);
                logger.LogIncomingRequest(context, sw.Elapsed);
            }
            catch (Exception ex)
            {
                logger.LogIncomingRequestException(context, ex, sw.Elapsed);
            }
        }
    }
}
