using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace DiagnosticSourceSample.Controllers
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
            var response = await httpClient.GetAsync("https://www.bing.com").ConfigureAwait(false);
            return new HttpResponseMessage
            {
                StatusCode = response.StatusCode,
            };
        }
    }
}
