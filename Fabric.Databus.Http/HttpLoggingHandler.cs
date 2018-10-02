namespace Fabric.Databus.Http
{
    using System;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using Serilog;
    using Serilog.Core;

    public class HttpLoggingHandler : DelegatingHandler
    {
        private static readonly Logger Logger = new LoggerConfiguration().CreateLogger();
        private readonly bool _doLogContent;

        public HttpLoggingHandler(HttpMessageHandler innerHandler, bool doLogContent)
            : base(innerHandler)
        {
            this._doLogContent = doLogContent;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                var sb = new StringBuilder();

                sb.AppendLine("------------------- REQUEST ----------------------------");
                sb.AppendLine($"{request.Method} {request.RequestUri}");
                sb.AppendLine($"{request.Headers}");

                HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

                if (this._doLogContent)
                {
                    if (this._doLogContent && request.Content != null)
                    {
                        sb.AppendLine(await request.Content.ReadAsStringAsync());
                    }
                }
                else
                {
                    sb.AppendLine("[Content was hidden by app code]");
                }
                //if (request.Headers != null)
                //{
                //    foreach (var header in request.Headers)
                //    {
                //        sb.AppendLine($"{header.Key}={header.Value}");
                //    }
                //}

                Logger.Verbose(sb.ToString());

                sb.Clear();
                sb.AppendLine("------------------- RESPONSE ----------------------------");
                sb.AppendLine($"{response.StatusCode} {response.ReasonPhrase}");
                sb.AppendLine($"{response.Headers}");

                if (response.Content != null)
                {
                    sb.AppendLine(await response.Content.ReadAsStringAsync());
                }

                Logger.Verbose(sb.ToString());

                return response;

            }
            catch (Exception e)
            {
                throw new Exception($"Unable to connect to {request.RequestUri}", e);
            }
        }
    }
}