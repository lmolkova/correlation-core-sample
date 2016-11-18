using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Diagnostics.Correlation.Common;

namespace CoreSampleApp.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private readonly HttpClient httpClient;

        //http client instrumentation
        public ValuesController(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        // GET api/values
        [HttpGet]
        public async Task<HttpResponseMessage> Get()
        {
            var ctx = ContextResolver.GetRequestContext<CorrelationContext>();

            var url = $"http://correlationsample.cloudapp.net:8080/test/{ctx.CorrelationId}";
            var response = await httpClient.GetAsync(url).ConfigureAwait(false);
            var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var result = new HttpResponseMessage
            {
                StatusCode = response.StatusCode,
                Content = new StringContent($"Called\r\n\tGET {url}\r\nResponse:\r\n\t{body}")
            };

            return result;
        }
    }
}
