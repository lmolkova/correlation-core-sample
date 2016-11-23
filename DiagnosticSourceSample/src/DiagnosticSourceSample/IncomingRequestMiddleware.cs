using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace DiagnosticSourceSample
{
    public class IncomingRequestMiddleware
    {
        private readonly RequestDelegate next;

        public IncomingRequestMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            Stopwatch sw = Stopwatch.StartNew();
            try
            {
                await this.next(context).ConfigureAwait(false);
                MyDiagnosticSource.LogIncomingRequest(context, sw.Elapsed);
            }
            catch (Exception ex)
            {
                MyDiagnosticSource.LogIncomingRequestException(context, ex, sw.Elapsed);
            }
        }
    }
}
