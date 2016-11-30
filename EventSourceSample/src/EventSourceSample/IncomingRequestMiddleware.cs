using System;
using System.Diagnostics.Tracing;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Diagnostics.Context;

namespace EventSourceSample
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
            var ctx = ContextResolver.GetContext<CorrelationContext>();
            var relatedActivityId = new Guid(ctx.CorrelationId);
            try
            {
                EventSource.SetCurrentThreadActivityId(relatedActivityId);
                MyEventSource.Log.RequestStart(
                   context.Request.Method,
                   context.Request.Path.Value,
                   ctx.CorrelationId,
                   ctx.RequestId);
                await this.next(context).ConfigureAwait(false);
                EventSource.SetCurrentThreadActivityId(relatedActivityId);
                MyEventSource.Log.RequestStop(context.Response.StatusCode.ToString());
            }
            catch (Exception ex)
            {
                EventSource.SetCurrentThreadActivityId(relatedActivityId);
                MyEventSource.Log.RequestStop(ex.ToString());
            }
        }
    }
}
