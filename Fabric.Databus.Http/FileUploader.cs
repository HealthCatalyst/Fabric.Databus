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
        /// The logger.
        /// </param>
        /// <param name="hosts">
        /// The hosts.
        /// </param>
        /// <param name="httpClientFactory">
        /// The http client factory.
        /// </param>
        /// <param name="httpRequestInterceptor">
        /// The http Request Injector.
        /// </param>
        public FileUploader(
            ILogger logger,
            List<string> hosts,
            IHttpClientFactory httpClientFactory,
            IHttpRequestInterceptor httpRequestInterceptor)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.hosts = hosts;

            this.HttpClientHelper = new HttpClientHelper(httpClientFactory, httpRequestInterceptor);
        }

        /// <inheritdoc />
        public async Task<HttpStatusCode> SendStreamToHosts(string relativeUrl, int batch, Stream stream, bool doLogContent, bool doCompress)
        {
            var hostNumber = batch % this.hosts.Count;

            var url = this.hosts[hostNumber] + relativeUrl;

            return await this.SendStreamToUrl(url, batch, stream, doLogContent, doCompress);
        }

        /// <summary>
        /// The send stream to url.
        /// </summary>
        /// <param name="url">
        /// The url.
        /// </param>
        /// <param name="batch">
        /// The batch.
        /// </param>
        /// <param name="stream">
        /// The stream.
        /// </param>
        /// <param name="doLogContent">
        /// The do log content.
        /// </param>
        /// <param name="doCompress">
        /// The do compress.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        protected virtual async Task<HttpStatusCode> SendStreamToUrl(string url, int batch, Stream stream, bool doLogContent, bool doCompress)
        {
            try
            {
                this.logger.Verbose($"Sending file {batch} of size {stream.Length:N0} to {url}");

                // http://stackoverflow.com/questions/30310099/correct-way-to-compress-webapi-post
                var baseUri = url;
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
                        this.logger.Verbose($"{requestContent}");
                    }
                }

                Interlocked.Increment(ref this.currentRequests);
                var requestStartTimeMilliseconds = this.Stopwatch.ElapsedMilliseconds;

                var response = doCompress
                                   ? await this.HttpClientHelper.PutAsyncStreamCompressed(baseUri, url, stream)
                                   : await this.HttpClientHelper.PutAsyncStream(baseUri, url, stream);

                if (response.IsSuccessStatusCode)
                {
                    Interlocked.Decrement(ref this.currentRequests);

                    var responseContent = await response.Content.ReadAsStringAsync();

                    var stopwatchElapsed = this.Stopwatch.ElapsedMilliseconds;

                    // var millisecsPerFile = 0; // Convert.ToInt32(stopwatchElapsed / (_totalFiles - _queuedFiles.Count));
                    var millisecsForThisFile = stopwatchElapsed - requestStartTimeMilliseconds;
                }
                else
                {
                    // logger.Verbose("========= Error =================");
                    this.logger.Error(requestContent);

                    var responseJson = await response.Content.ReadAsStringAsync();

                    this.logger.Error(responseJson);

                    // logger.Verbose("========= Error =================");
                }

                return response.StatusCode;
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, url);
                throw;
            }
        }

    }
}