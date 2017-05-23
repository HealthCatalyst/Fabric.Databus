using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ElasticSearchApiCaller
{
    public class RetryHandler : DelegatingHandler
    {
        // Strongly consider limiting the number of retries - "retry forever" is
        // probably not the most user friendly way you could respond to "the
        // network cable got pulled out."
        private const int MaxRetries = 3;

        public RetryHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        { }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            HttpResponseMessage response = null;
            for (int i = 0; i < MaxRetries; i++)
            {
                response = await base.SendAsync(request, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    if (response.Content != null)
                    {
                        //var responseContent = await response.Content.ReadAsStringAsync();
                        //if (responseContent.Contains("errors"))
                        //{
                        //    var jObject = JObject.Parse(responseContent);
                        //    JArray items = (JArray)jObject.Root["items"];
                        //    var itemsFirst = items.First;
                        //}
                    }
                    return response;
                }

                Console.WriteLine($"Retry: {request}, response={response}");
            }

            return response;
        }
    }
}