// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileUploader.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the FileUploader type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.ElasticSearch
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using Fabric.Databus.Http;
    using Fabric.Databus.Interfaces.Http;

    using Newtonsoft.Json;

    using Serilog;

    /// <inheritdoc />
    /// <summary>
    /// The file uploader.
    /// </summary>
    public class FileUploader : IFileUploader
    {
        /// <summary>
        /// The logger.
        /// </summary>
        protected ILogger logger;

        /// <summary>
        /// The hosts.
        /// </summary>
        protected List<string> hosts;

        private readonly string username;

        private readonly string password;

        /// <summary>
        /// The http client.
        /// </summary>
        protected HttpClient httpClient;

        /// <summary>
        /// The request failures.
        /// </summary>
        protected int requestFailures;

        /// <summary>
        /// The current requests.
        /// </summary>
        protected int currentRequests = 0;

        /// <summary>
        /// The stopwatch.
        /// </summary>
        protected readonly Stopwatch stopwatch = new Stopwatch();

        /// <summary>
        /// The queued files.
        /// </summary>
        protected readonly ConcurrentQueue<string> queuedFiles = new ConcurrentQueue<string>();

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
        /// <param name="username">
        /// The username.
        /// </param>
        /// <param name="password">
        /// The password.
        /// </param>
        public FileUploader(ILogger logger, List<string> hosts, IHttpClientFactory httpClientFactory, string username, string password)
        {
            this.logger = logger;
            this.hosts = hosts;
            this.username = username;
            this.password = password;

            this.httpClient = httpClientFactory.Create();
        }

        /// <inheritdoc />
        public async Task SendStreamToHosts(string relativeUrl, int batch, Stream stream, bool doLogContent, bool doCompress)
        {
            var hostNumber = batch % this.hosts.Count;

            var url = this.hosts[hostNumber] + relativeUrl;

            await this.SendStreamToUrl(url, batch, stream, doLogContent, doCompress);
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
        internal async Task SendStreamToUrl(string url, int batch, Stream stream, bool doLogContent, bool doCompress)
        {
            try
            {
                this.logger.Verbose($"Sending file {batch} of size {stream.Length:N0} to {url}");

                // http://stackoverflow.com/questions/30310099/correct-way-to-compress-webapi-post

                var baseUri = url;
                this.httpClient.BaseAddress = new Uri(baseUri);
                this.httpClient.DefaultRequestHeaders.Accept.Clear();

                this.AddAuthorizationToken(this.httpClient);

                string requestContent;

                using (var newMemoryStream = new MemoryStream())
                {
                    stream.Position = 0;
                    stream.CopyTo(newMemoryStream);
                    newMemoryStream.Position = 0;
                    using (var reader = new StreamReader(newMemoryStream, Encoding.UTF8))
                    {
                        requestContent = reader.ReadToEnd();

                        // TODO: Do something with the value
                        this.logger.Verbose($"{requestContent}");
                    }
                }

                Interlocked.Increment(ref this.currentRequests);
                var requestStartTimeMillisecs = this.stopwatch.ElapsedMilliseconds;

                var response = doCompress
                                   ? await this.httpClient.PutAsyncStreamCompressed(url, stream)
                                   : await this.httpClient.PutAsyncStream(url, stream);

                if (response.IsSuccessStatusCode)
                {
                    Interlocked.Decrement(ref this.currentRequests);

                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<ElasticSearchJsonResponse>(responseContent);

                    var stopwatchElapsed = this.stopwatch.ElapsedMilliseconds;
                    var millisecsPerFile = 0; // Convert.ToInt32(stopwatchElapsed / (_totalFiles - _queuedFiles.Count));

                    var millisecsForThisFile = stopwatchElapsed - requestStartTimeMillisecs;

                    if (result.errors)
                    {
                        if (result.items.Any(i => i.update.status == 429))
                        {
                            // add back to queue for sending

                            // _queuedFiles.Enqueue(filepath);
                            this.requestFailures++;
                            this.logger.Error(
                                $"Failed: {batch} status: {response.StatusCode} requests:{this.currentRequests} Left:{this.queuedFiles.Count}/{this.totalFiles}, Speed/file: {millisecsPerFile}, This file: {millisecsForThisFile}");
                        }
                    }
                    else
                    {
                        this.logger.Verbose(
                            $"Finished: {batch} status: {response.StatusCode} requests:{this.currentRequests} Left:{this.queuedFiles.Count}/{this.totalFiles}, Speed/file: {millisecsPerFile}, This file: {millisecsForThisFile}");
                    }
                }
                else
                {
                    // logger.Verbose("========= Error =================");
                    this.logger.Error(requestContent);

                    var responseJson = await response.Content.ReadAsStringAsync();

                    this.logger.Error(responseJson);

                    // logger.Verbose("========= Error =================");
                }
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, url);
                throw;
            }
        }

        /// <summary>
        /// The add authorization token.
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        protected void AddAuthorizationToken(HttpClient client)
        {
            if (!string.IsNullOrEmpty(this.username) && !string.IsNullOrEmpty(this.password))
            {
                var byteArray = Encoding.ASCII.GetBytes($"{this.username}:{this.password}");
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            }
        }
    }
}