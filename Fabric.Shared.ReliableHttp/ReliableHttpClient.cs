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
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Events;
    using Exceptions;
    using Interfaces;
    using Polly;
    using Polly.Retry;

    /// <summary>
    /// The reliable http client.
    /// </summary>
    public class ReliableHttpClient
    {
        /// <summary>
        /// The content type header.
        /// </summary>
        private const string ContentTypeHeader = "application/json";

        /// <summary>
        /// The seconds between retries.
        /// </summary>
        private const int SecondsBetweenRetries = 2;

        /// <summary>
        /// The max retry count.
        /// </summary>
        private const int MaxRetryCount = 5;

        /// <summary>
        /// The http request injector.
        /// </summary>
        private readonly IHttpRequestInterceptor httpRequestInterceptor;

        private readonly IHttpRequestLogger httpRequestLogger;
        private readonly IHttpResponseLogger httpResponseLogger;

        /// <summary>
        /// The http response interceptor.
        /// </summary>
        private readonly IHttpResponseInterceptor httpResponseInterceptor;

        /// <summary>
        /// The http client factory.
        /// </summary>
        private readonly IHttpClientFactory httpClientFactory;

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
                HttpStatusCode.Conflict // 409
            };

        /// <summary>
        /// The cancellation token.
        /// </summary>
        private readonly CancellationToken cancellationToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReliableHttpClient"/> class.
        /// </summary>
        /// <param name="cancellationToken">
        /// The cancellation token.
        /// </param>
        /// <param name="httpClientFactory">
        /// The http Client Factory.
        /// </param>
        /// <param name="httpRequestInterceptor">
        /// The http Request Interceptor.
        /// </param>
        /// <param name="httpRequestLogger"></param>
        /// <param name="httpResponseLogger"></param>
        /// <param name="httpResponseInterceptor">
        /// The http Response Interceptor.
        /// </param>
        public ReliableHttpClient(
            CancellationToken cancellationToken,
            IHttpClientFactory httpClientFactory,
            IHttpRequestInterceptor httpRequestInterceptor,
            IHttpRequestLogger httpRequestLogger,
            IHttpResponseLogger httpResponseLogger,
            IHttpResponseInterceptor httpResponseInterceptor)
        {
            this.cancellationToken = cancellationToken;
            this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            this.httpRequestInterceptor = httpRequestInterceptor ?? throw new ArgumentNullException(nameof(httpRequestInterceptor));
            this.httpRequestLogger = httpRequestLogger ?? throw new ArgumentNullException(nameof(httpRequestLogger));
            this.httpResponseLogger = httpResponseLogger ?? throw new ArgumentNullException(nameof(httpResponseLogger));
            this.httpResponseInterceptor = httpResponseInterceptor ?? throw new ArgumentNullException(nameof(httpResponseInterceptor));
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
        public async Task<SendAsyncResult> SendAsync(HttpMethod httpMethod, string resourceId, Uri fullUri, Stream stream)
        {
            var method = Convert.ToString(httpMethod);

            OnNavigating(new NavigatingEventArgs(resourceId, method, fullUri));

            var policy = GetRetryPolicy(resourceId, method, fullUri);

            var stopwatch = new Stopwatch();

            try
            {
                var httpResponse = await policy.ExecuteAsync(
                                       async () =>
                                           {
                                               using (var httpRequestMessage = new HttpRequestMessage(httpMethod, fullUri))
                                               {
                                                   // StreamContent disposes the stream when it is done so we need to keep a copy for retries
                                                   var memoryStream = new MemoryStream();
                                                   // ReSharper disable once AccessToDisposedClosure
                                                   stream.Seek(0, SeekOrigin.Begin);
                                                   // ReSharper disable once AccessToDisposedClosure
                                                   await stream.CopyToAsync(memoryStream);

                                                   memoryStream.Seek(0, SeekOrigin.Begin);

                                                   using (var requestContent = new StreamContent(memoryStream))
                                                   {
                                                       requestContent.Headers.ContentType = new MediaTypeHeaderValue(ContentTypeHeader);

                                                       httpRequestMessage.Content = requestContent;

                                                       httpRequestInterceptor.InterceptRequest(
                                                           httpMethod,
                                                           httpRequestMessage);

                                                       httpRequestLogger.InterceptRequest(httpMethod,
                                                           httpRequestMessage);

                                                       return await httpClientFactory.Create().SendAsync(
                                                                  httpRequestMessage,
                                                                  cancellationToken);
                                                   }
                                               }
                                           });


                OnNavigated(
                    new NavigatedEventArgs(resourceId, method, fullUri, httpResponse.StatusCode.ToString(), httpResponse.Content));

                httpResponseInterceptor.InterceptResponse(
                    httpMethod,
                    fullUri,
                    stream,
                    httpResponse.StatusCode,
                    httpResponse.Content,
                    stopwatch.ElapsedMilliseconds);

                httpResponseLogger.InterceptResponse(
                    httpMethod,
                    fullUri,
                    stream,
                    httpResponse.StatusCode,
                    httpResponse.Content,
                    stopwatch.ElapsedMilliseconds);

                return new SendAsyncResult
                {
                    ResourceId = resourceId,
                    Method = method,
                    Uri = fullUri,
                    StatusCode = httpResponse.StatusCode,
                    IsSuccessStatusCode = httpResponse.IsSuccessStatusCode,
                    ResponseContent = httpResponse.Content
                };
            }
            catch (Exception e)
            {
                throw new ReliableHttpException(fullUri, method, e);
            }
        }

        /// <summary>
        /// The send async.
        /// </summary>
        /// <param name="uri">
        /// The uri.
        /// </param>
        /// <param name="httpMethod">
        /// The http Method.
        /// </param>
        /// <param name="stringContent">
        /// The string content.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task<SendAsyncResult> SendAsync(Uri uri, HttpMethod httpMethod, HttpContent stringContent)
        {
            var ms = new MemoryStream();
            await stringContent.CopyToAsync(ms);
            ms.Seek(0, SeekOrigin.Begin);

            return await SendAsync(httpMethod, string.Empty, uri, ms);
        }

        /// <summary>
        /// The put async file.
        /// </summary>
        /// <param name="uri">
        /// The uri.
        /// </param>
        /// <param name="filename">
        /// The filename.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task<SendAsyncResult> PutAsyncFile(Uri uri, string filename)
        {
            var allText = File.ReadAllText(filename);
            var stringContent = new StringContent(allText, Encoding.UTF8, ContentTypeHeader);

            return await PutAsync(uri, stringContent);
        }

        /// <summary>
        /// The put async file compressed.
        /// </summary>
        /// <param name="uri">
        /// The uri.
        /// </param>
        /// <param name="filename">
        /// The filename.
        /// </param>
        /// <returns>
        /// The <see cref="ConfiguredTaskAwaitable"/>.
        /// </returns>
        public async Task<SendAsyncResult> PutAsyncFileCompressed(
            Uri uri,
            string filename)
        {
            var allText = File.ReadAllText(filename);
            byte[] jsonBytes = Encoding.UTF8.GetBytes(allText);
            var ms = new MemoryStream();
            using (var gzip = new GZipStream(ms, CompressionMode.Compress, true))
            {
                gzip.Write(jsonBytes, 0, jsonBytes.Length);
            }

            ms.Position = 0;
            var content = new StreamContent(ms);
            content.Headers.ContentType = new MediaTypeHeaderValue(ContentTypeHeader);
            content.Headers.ContentEncoding.Add("gzip");

            return await PutAsync(uri, content).ConfigureAwait(false);
        }

        /// <summary>
        /// The put async stream compressed.
        /// </summary>
        /// <param name="url">
        ///     the url
        /// </param>
        /// <param name="httpMethod">http method</param>
        /// <param name="stream">
        ///     The stream.
        /// </param>
        /// <returns>
        /// The <see cref="ConfiguredTaskAwaitable"/>.
        /// </returns>
        public async Task<SendAsyncResult> SendAsyncStreamCompressed(Uri url, HttpMethod httpMethod, Stream stream)
        {
            stream.Position = 0;
            var ms = new MemoryStream();
            using (var gzip = new GZipStream(ms, CompressionMode.Compress, true))
            {
                stream.CopyTo(gzip);
            }

            stream.Dispose();

            ms.Position = 0;
            var content = new StreamContent(ms);
            content.Headers.ContentType = new MediaTypeHeaderValue(ContentTypeHeader);
            content.Headers.ContentEncoding.Add("gzip");

            return await SendAsync(url, httpMethod, content).ConfigureAwait(false);
        }

        /// <summary>
        /// The put async stream.
        /// </summary>
        /// <param name="url">
        ///     The url.
        /// </param>
        /// <param name="httpMethod">
        ///     http method</param>
        /// <param name="stream">
        ///     The stream.
        /// </param>
        /// <returns>
        /// The <see cref="ConfiguredTaskAwaitable"/>.
        /// </returns>
        public async Task<SendAsyncResult> SendAsyncStream(Uri url, HttpMethod httpMethod, Stream stream)
        {
            stream.Position = 0;
            MemoryStream ms = new MemoryStream();
            stream.CopyTo(ms);

            stream.Dispose();

            ms.Position = 0;
            StreamContent content = new StreamContent(ms);
            content.Headers.ContentType = new MediaTypeHeaderValue(ContentTypeHeader);

            return await SendAsync(url, httpMethod, content).ConfigureAwait(false);
        }

        /// <summary>
        /// The put async string.
        /// </summary>
        /// <param name="url">
        /// The url.
        /// </param>
        /// <param name="text">
        /// The text.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task<SendAsyncResult> PutAsyncString(Uri url, string text)
        {
            var stringContent = new StringContent(text, Encoding.UTF8, ContentTypeHeader);

            return await PutAsync(url, stringContent);
        }

        /// <summary>
        /// The post async string.
        /// </summary>
        /// <param name="url">
        /// The url.
        /// </param>
        /// <param name="text">
        /// The text.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task<SendAsyncResult> PostAsyncString(Uri url, string text)
        {
            var stringContent = new StringContent(text, Encoding.UTF8, ContentTypeHeader);

            return await PostAsync(url, stringContent);
        }

        /// <summary>
        /// The post async.
        /// </summary>
        /// <param name="url">
        /// The url.
        /// </param>
        /// <param name="stringContent">
        /// The string content.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task<SendAsyncResult> PostAsync(Uri url, HttpContent stringContent)
        {
            return await SendAsync(url, HttpMethod.Post, stringContent);
        }

        /// <summary>
        /// The delete async.
        /// </summary>
        /// <param name="requestUri">
        /// The request uri.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task<HttpResponseMessage> DeleteAsync(Uri requestUri)
        {
            var httpMethod = HttpMethod.Delete;
            using (var request = new HttpRequestMessage(httpMethod, requestUri))
            {
                httpRequestInterceptor.InterceptRequest(httpMethod, request);

                return await httpClientFactory.Create().SendAsync(request, cancellationToken);
            }
        }

        /// <summary>
        /// The get string async.
        /// </summary>
        /// <param name="host">
        /// The host.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task<string> GetStringAsync(string host)
        {
            var httpMethod = HttpMethod.Get;
            using (var request = new HttpRequestMessage(httpMethod, host))
            {
                httpRequestInterceptor.InterceptRequest(httpMethod, request);

                var result = await httpClientFactory.Create().SendAsync(request, cancellationToken);

                return await result.Content.ReadAsStringAsync();
            }
        }

        /// <summary>
        /// The on navigating.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        private void OnNavigating(NavigatingEventArgs e)
        {
            Navigating?.Invoke(this, e);
        }

        /// <summary>
        /// The on navigated.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        private void OnNavigated(NavigatedEventArgs e)
        {
            Navigated?.Invoke(this, e);
        }

        /// <summary>
        /// The on transient error.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        private void OnTransientError(TransientErrorEventArgs e)
        {
            TransientError?.Invoke(this, e);
        }

        /// <summary>
        /// The put async.
        /// </summary>
        /// <param name="url">
        /// The url.
        /// </param>
        /// <param name="content">
        /// The content.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task<SendAsyncResult> PutAsync(Uri url, HttpContent content)
        {
            return await SendAsync(url, HttpMethod.Put, content);
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
                    message => httpStatusCodesWorthRetrying.Contains(message.StatusCode))
                .WaitAndRetryAsync(
                    MaxRetryCount,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(SecondsBetweenRetries, retryAttempt)),
                    async (result, timeSpan, retryCount, context) =>
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            if (result.Result != null)
                            {
                                if (result.Result.StatusCode == HttpStatusCode.Unauthorized)
                                {
                                    // await this.SetAuthorizationHeaderInHttpClientWithNewBearerTokenAsync(resourceId);
                                }

                                var errorContent = await result.Result.Content.ReadAsStringAsync();
                                OnTransientError(
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
                                OnTransientError(
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
