// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileUploader.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the FileUploader type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Http
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Fabric.Databus.Interfaces.Http;

    using Serilog;

    /// <inheritdoc />
    /// <summary>
    /// The file uploader.
    /// </summary>
    public class FileUploader : IFileUploader
    {
        /// <summary>
        /// The stopwatch.
        /// </summary>
        protected readonly Stopwatch Stopwatch = new Stopwatch();

        /// <summary>
        /// The queued files.
        /// </summary>
        protected readonly ConcurrentQueue<string> QueuedFiles = new ConcurrentQueue<string>();

        /// <summary>
        /// The http client helper.
        /// </summary>
        protected readonly HttpClientHelper HttpClientHelper;

        /// <summary>
        /// The logger.
        /// </summary>
        protected ILogger logger;

        /// <summary>
        /// The hosts.
        /// </summary>
        protected List<string> hosts;

        /// <summary>
        /// The http response interceptor.
        /// </summary>
        private readonly IHttpResponseInterceptor httpResponseInterceptor;

        protected readonly HttpMethod httpMethod;

        /// <summary>
        /// The request failures.
        /// </summary>
        protected int requestFailures;

        /// <summary>
        /// The current requests.
        /// </summary>
        protected int currentRequests = 0;

        /// <summary>
        /// The total files.
        /// </summary>
        protected int totalFiles;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileUploader"/> class.
        /// </summary>
        /// <param name="logger">
        ///     The logger.
        /// </param>
        /// <param name="hosts">
        ///     The hosts.
        /// </param>
        /// <param name="httpClientFactory">
        ///     The http client factory.
        /// </param>
        /// <param name="httpRequestInterceptor">
        ///     The http Request Injector.
        /// </param>
        /// <param name="httpResponseInterceptor">
        ///     http response interceptor</param>
        /// <param name="httpMethod">http method</param>
        public FileUploader(
            ILogger logger,
            List<string> hosts,
            IHttpClientFactory httpClientFactory,
            IHttpRequestInterceptor httpRequestInterceptor,
            IHttpResponseInterceptor httpResponseInterceptor,
            HttpMethod httpMethod)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.hosts = hosts;
            this.httpResponseInterceptor = httpResponseInterceptor ?? throw new ArgumentNullException(nameof(httpResponseInterceptor));
            this.httpMethod = httpMethod ?? throw new ArgumentNullException(nameof(httpMethod));

            this.HttpClientHelper = new HttpClientHelper(httpClientFactory, httpRequestInterceptor);
        }

        /// <inheritdoc />
        public async Task<IFileUploadResult> SendStreamToHostsAsync(
            string relativeUrl,
            int batch,
            Stream stream,
            bool doLogContent,
            bool doCompress)
        {
            var hostNumber = batch % this.hosts.Count;

            var url = new Uri(new Uri(this.hosts[hostNumber]), relativeUrl);

            return await this.SendStreamToUrlAsync(url, batch, stream, doLogContent, doCompress);
        }

        /// <summary>
        /// The send stream to url.
        /// </summary>
        /// <param name="url">
        ///     The url.
        /// </param>
        /// <param name="batch">
        ///     The batch.
        /// </param>
        /// <param name="stream">
        ///     The stream.
        /// </param>
        /// <param name="doLogContent">
        ///     The do log content.
        /// </param>
        /// <param name="doCompress">
        ///     The do compress.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        protected virtual async Task<IFileUploadResult> SendStreamToUrlAsync(
            Uri url,
            int batch,
            Stream stream,
            bool doLogContent,
            bool doCompress)
        {
            try
            {
                this.logger.Verbose("Sending file {batch} of size {Length} to {url}", batch, stream.Length, url);

                // http://stackoverflow.com/questions/30310099/correct-way-to-compress-webapi-post
                string requestContent;

                using (var newMemoryStream = new MemoryStream())
                {
                    stream.Position = 0;
                    await stream.CopyToAsync(newMemoryStream);
                    newMemoryStream.Position = 0;
                    using (var reader = new StreamReader(newMemoryStream, Encoding.UTF8))
                    {
                        requestContent = reader.ReadToEnd();

                        // TODO: Do something with the value
                        this.logger.Verbose("Http Sending {@requestContent}", requestContent);
                    }
                }

                Interlocked.Increment(ref this.currentRequests);
                this.Stopwatch.Reset();

                var response = doCompress
                                   ? await this.HttpClientHelper.SendAsyncStreamCompressed(url, this.httpMethod, stream)
                                   : await this.HttpClientHelper.SendAsyncStream(url, this.httpMethod, stream);

                var responseContent = await response.Content.ReadAsStringAsync();

                this.httpResponseInterceptor.InterceptResponse(this.httpMethod, url, requestContent, response.StatusCode, responseContent, this.Stopwatch.ElapsedMilliseconds);

                if (response.IsSuccessStatusCode)
                {
                    Interlocked.Decrement(ref this.currentRequests);


                }
                else
                {
                    this.logger.Error(requestContent);
                }

                return new FileUploadResult
                           {
                               Uri = url,
                               HttpMethod = this.httpMethod,
                               RequestContent = requestContent,
                               StatusCode = response.StatusCode,
                               ResponseContent = responseContent
                           };
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, url.ToString());
                throw;
            }
        }
    }
}