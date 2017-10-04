using System;
using System.Net.Http;
using System.Text;
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

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            if (request.Content != null)
            {
                Logger.Trace(await request.Content.ReadAsStringAsync());
            }

            if (request.Headers != null)
            {
                foreach (var header in request.Headers)
                {
                    Logger.Trace($"{header.Key}={header.Value}");
                }
            }

            Logger.Trace($"Response: {response}");

            if (response.Content != null)
            {
                Logger.Trace(await response.Content.ReadAsStringAsync());
            }

            return response;
        }
    }
}