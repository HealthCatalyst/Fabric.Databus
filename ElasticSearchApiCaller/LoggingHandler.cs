using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace ElasticSearchApiCaller
{
    public class LoggingHandler : DelegatingHandler
    {
        private static readonly Logger Logger = LogManager.GetLogger("LoggingHandler");

        public LoggingHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Logger.Trace($"request:{request}");
            if (request.Content != null)
            {
                //Console.WriteLine(await request.Content.ReadAsStringAsync());
            }

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            Logger.Trace($"Response: {response}");

            if (response.Content != null)
            {
                Logger.Trace(await response.Content.ReadAsStringAsync());
            }

            return response;
        }
    }
}