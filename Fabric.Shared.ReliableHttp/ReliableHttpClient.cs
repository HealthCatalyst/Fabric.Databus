// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReliableHttpClient.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the ReliableHttpClient type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Shared.ReliableHttp
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;

    using Fabric.Shared.ReliableHttp.Events;

    using Polly;
    using Polly.Retry;

    /// <summary>
    /// The reliable http client.
    /// </summary>
    public class ReliableHttpClient
    {

        /// <summary>
        /// The seconds between retries.
        /// </summary>
        private const int SecondsBetweenRetries = 2;

        /// <summary>
        /// The max retry count.
        /// </summary>
        private const int MaxRetryCount = 5;

        /// <summary>
        /// The application json media type.
        /// </summary>
        private const string ApplicationJsonMediaType = "application/json";

        /// <summary>
        /// The http timeout.
        /// </summary>
        private static readonly TimeSpan HttpTimeout = TimeSpan.FromMinutes(5);

        /// <summary>
        /// The _http client.
        /// make HttpClient static per https://aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/
        /// </summary>
        private static HttpClient httpClient;

        /// <summary>
        /// The http status codes worth retrying.
        /// </summary>
        private readonly HttpStatusCode[] httpStatusCodesWorthRetrying =
            {
                HttpStatusCode.Unauthorized, // 401
                HttpStatusCode.RequestTimeout, // 408
                HttpStatusCode.InternalServerError, // 500
                HttpStatusCode.BadGateway, // 502
                HttpStatusCode.ServiceUnavailable, // 503
                HttpStatusCode.GatewayTimeout, // 504
                HttpStatusCode.Conflict, // 409
            };

        /// <summary>
        /// The cancellation token.
        /// </summary>
        private readonly CancellationToken cancellationToken;


        /// <summary>
        /// Initializes a new instance of the <see cref="ReliableHttpClient"/> class.
        /// </summary>
        /// <param name="httpClientHandler">
        /// The http Client Handler.
        /// </param>
        /// <param name="cancellationToken">
        /// The cancellation token.
        /// </param>
        public ReliableHttpClient(HttpMessageHandler httpClientHandler, CancellationToken cancellationToken)
        {
            this.cancellationToken = cancellationToken;
            if (httpClient == null)
            {
                httpClient = CreateHttpClient(httpClientHandler);
            }
        }

        /// <summary>
        /// The navigating.
        /// </summary>
        public event NavigatingEventHandler Navigating;

        /// <summary>
        /// The navigated.
        /// </summary>
        public event NavigatedEventHandler Navigated;

        /// <summary>
        /// The transient error.
        /// </summary>
        public event TransientErrorEventHandler TransientError;

        /// <summary>
        /// The clear http client.
        /// </summary>
        public static void ClearHttpClient()
        {
            httpClient = null;
        }

        /// <summary>
        /// The Send Async
        /// </summary>
        /// <param name="httpMethod">
        ///     the http method</param>
        /// <param name="resourceId">
        ///     The resource id.
        /// </param>
        /// <param name="fullUri">
        ///     The full uri.
        /// </param>
        /// <param name="stream">
        ///     The stream.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task SendAsync(HttpMethod httpMethod, string resourceId, Uri fullUri, Stream stream)
        {
            var method = Convert.ToString(httpMethod);

            this.OnNavigating(new NavigatingEventArgs(resourceId, method, fullUri));

            var policy = this.GetRetryPolicy(resourceId, method, fullUri);

            var httpResponse = await policy.ExecuteAsync(
                                   async () =>
                                       {
                                           using (var httpRequestMessage = new HttpRequestMessage(httpMethod, fullUri))
                                           {
                                               using (var requestContent = new MultipartFormDataContent())
                                               {
                                                   // StreamContent disposes the stream when it is done so we need to keep a copy for retries
                                                   var memoryStream = new MemoryStream();
                                                   // ReSharper disable once AccessToDisposedClosure
                                                   stream.Seek(0, SeekOrigin.Begin);
                                                   // ReSharper disable once AccessToDisposedClosure
                                                   await stream.CopyToAsync(memoryStream);

                                                   memoryStream.Seek(0, SeekOrigin.Begin);

                                                   var fileContent = new StreamContent(memoryStream);

                                                   requestContent.Add(fileContent);

                                                   httpRequestMessage.Content = requestContent;

                                                   return await httpClient.SendAsync(
                                                              httpRequestMessage,
                                                              this.cancellationToken);
                                               }
                                           }
                                       });

            var content = await httpResponse.Content.ReadAsStringAsync();
            this.OnNavigated(new NavigatedEventArgs(resourceId, method, fullUri, httpResponse.StatusCode.ToString(), content));
        }

        /// <summary>
        /// The create http client.
        /// </summary>
        /// <param name="httpClientHandler">
        /// The http client handler.
        /// </param>
        /// <returns>
        /// The <see cref="HttpClient"/>.
        /// </returns>
        private static HttpClient CreateHttpClient(HttpMessageHandler httpClientHandler)
        {
            var client = new HttpClient(httpClientHandler, false);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(ApplicationJsonMediaType));
            client.DefaultRequestHeaders.Connection.Clear();
            client.DefaultRequestHeaders.ConnectionClose = true;
            client.Timeout = HttpTimeout;
            return client;
        }

        /// <summary>
        /// The on navigating.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        private void OnNavigating(NavigatingEventArgs e)
        {
            this.Navigating?.Invoke(this, e);
        }

        /// <summary>
        /// The on navigated.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        private void OnNavigated(NavigatedEventArgs e)
        {
            this.Navigated?.Invoke(this, e);
        }

        /// <summary>
        /// The on transient error.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        private void OnTransientError(TransientErrorEventArgs e)
        {
            this.TransientError?.Invoke(this, e);
        }


        /// <summary>
        /// The get retry policy.
        /// </summary>
        /// <param name="resourceId">
        /// The resource id.
        /// </param>
        /// <param name="method">
        /// The method.
        /// </param>
        /// <param name="fullUri">
        /// The full uri.
        /// </param>
        /// <returns>
        /// The <see cref="RetryPolicy"/>.
        /// </returns>
        private RetryPolicy<HttpResponseMessage> GetRetryPolicy(string resourceId, string method, Uri fullUri)
        {
            return Policy.Handle<HttpRequestException>().Or<TaskCanceledException>()
                .OrResult<HttpResponseMessage>(
                    message => this.httpStatusCodesWorthRetrying.Contains(message.StatusCode))
                .WaitAndRetryAsync(
                    MaxRetryCount,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(SecondsBetweenRetries, retryAttempt)),
                    async (result, timeSpan, retryCount, context) =>
                        {
                            this.cancellationToken.ThrowIfCancellationRequested();

                            if (result.Result != null)
                            {
                                if (result.Result.StatusCode == HttpStatusCode.Unauthorized)
                                {
                                    // await this.SetAuthorizationHeaderInHttpClientWithNewBearerTokenAsync(resourceId);
                                }

                                var errorContent = await result.Result.Content.ReadAsStringAsync();
                                this.OnTransientError(
                                    new TransientErrorEventArgs(
                                        resourceId,
                                        method,
                                        fullUri,
                                        result.Result.StatusCode.ToString(),
                                        errorContent,
                                        retryCount,
                                        MaxRetryCount));
                            }
                            else
                            {
                                this.OnTransientError(
                                    new TransientErrorEventArgs(
                                        resourceId,
                                        method,
                                        fullUri,
                                        "Exception",
                                        result.Exception?.ToString(),
                                        retryCount,
                                        MaxRetryCount));
                            }
                        });
        }
    }
}
