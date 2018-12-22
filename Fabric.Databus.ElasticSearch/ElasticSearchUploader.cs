// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElasticSearchUploader.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   The file uploader.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.ElasticSearch
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using Fabric.Databus.Http;
    using Fabric.Databus.Interfaces.ElasticSearch;
    using Fabric.Databus.Interfaces.Http;
    using Fabric.Shared.ReliableHttp.Interfaces;

    using Newtonsoft.Json;

    using Serilog;

    /// <summary>
    /// The file uploader.
    /// </summary>
    public class ElasticSearchUploader : FileUploader, IElasticSearchUploader
    {
        /// <summary>
        /// The number of parallel uploads.
        /// </summary>
        private const int NumberOfParallelUploads = 50;

        /// <summary>
        /// The index.
        /// </summary>
        private readonly string index;

        /// <summary>
        /// The alias.
        /// </summary>
        private readonly string @alias;

        /// <summary>
        /// The entity type.
        /// </summary>
        private readonly string entityType;

        /// <summary>
        /// The max thread.
        /// </summary>
        private readonly SemaphoreSlim maxThread = new SemaphoreSlim(NumberOfParallelUploads);

        /// <summary>
        /// The keep index online.
        /// </summary>
        private readonly bool keepIndexOnline;

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Fabric.Databus.ElasticSearch.ElasticSearchUploader" /> class.
        /// </summary>
        /// <param name="keepIndexOnline">
        /// The keep index online.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <param name="hosts">
        /// The hosts.
        /// </param>
        /// <param name="index">
        /// The index.
        /// </param>
        /// <param name="alias">
        /// The alias.
        /// </param>
        /// <param name="entityType">
        /// The entity type
        /// </param>
        /// <param name="httpClientFactory">
        /// The http Client Factory.
        /// </param>
        /// <param name="httpRequestInterceptor"></param>
        /// <param name="httpResponseInterceptor"></param>
        /// <param name="method"></param>
        /// <param name="httpResponseLogger"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="httpRequestLogger"></param>
        public ElasticSearchUploader(
            bool keepIndexOnline,
            ILogger logger,
            List<string> hosts,
            string index,
            string alias,
            string entityType,
            IHttpClientFactory httpClientFactory,
            IHttpRequestInterceptor httpRequestInterceptor,
            IHttpResponseInterceptor httpResponseInterceptor,
            HttpMethod method,
            IHttpRequestLogger httpRequestLogger,
            IHttpResponseLogger httpResponseLogger,
            CancellationToken cancellationToken)
        : base(logger, hosts, httpClientFactory, httpRequestInterceptor, httpResponseInterceptor, method, httpRequestLogger, httpResponseLogger,  cancellationToken)
        {
            if (httpClientFactory == null)
            {
                throw new ArgumentNullException(nameof(httpClientFactory));
            }

            this.keepIndexOnline = keepIndexOnline;
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.hosts = hosts ?? throw new ArgumentNullException(nameof(hosts));
            this.index = index ?? throw new ArgumentNullException(nameof(index));
            this.alias = alias ?? throw new ArgumentNullException(nameof(alias));
            this.entityType = entityType ?? throw new ArgumentNullException(nameof(entityType));
        }

        /// <inheritdoc />
        public async Task CreateIndexAndMappings(string folder)
        {
            var host = this.hosts.First();

            //// curl -XPOST %ESURL%/_aliases?pretty --data "{\"actions\" : [{ \"remove\" : { \"index\" : \"patients2\", \"alias\" : \"patients\" } }]}"

            var text =
                $"{{\"actions\" : [{{ \"remove\" : {{ \"index\" : \"{this.index}\", \"alias\" : \"{this.alias}\" }} }}]}}";

            await this.reliableHttpClient.PostAsyncString(
                new Uri(new Uri(host), "/_aliases?pretty"),
                text,
                "GetAliases");

            var requestUri = new Uri(new Uri(host), $"/{this.index}");

            await this.reliableHttpClient.DeleteAsync(requestUri);

            // curl -XPOST 'http://localhost:9200/_forcemerge?only_expunge_deletes=true'
            await this.reliableHttpClient.PostAsync(new Uri(new Uri(host), "/_forcemerge?only_expunge_deletes=true"), null, "ForceMerge");

            await this.reliableHttpClient.PutAsyncFile(requestUri, folder + @"\mainmapping.json", "MainMapping");

            await this.InternalUploadAllFilesInFolder("mapping*", $"/{this.index}/_mapping/{this.entityType}", folder);

            // curl -XPUT %ESURL%/patients2/_settings --data "{ \"index\" : {\"refresh_interval\" : \"-1\" } }"

            // https://www.elastic.co/guide/en/elasticsearch/reference/current/tune-for-indexing-speed.html

            // https://www.elastic.co/guide/en/elasticsearch/reference/current/index-modules.html#index-codec
            if (!this.keepIndexOnline)
            {
                await this.reliableHttpClient.PutAsyncString(
                    new Uri(new Uri(host), "/" + this.index + "/_settings"),
                    "{ \"index\" : {\"refresh_interval\" : \"-1\" } }",
                    "DisableRefresh");

                await this.reliableHttpClient.PutAsyncString(
                    new Uri(new Uri(host), "/" + this.index + "/_settings"),
                    "{ \"index\" : {\"number_of_replicas\" : \"0\" } }",
                    "DisableReplica");
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// The delete index.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Threading.Tasks.Task" />.
        /// </returns>
        public async Task DeleteIndex()
        {
            var relativeUrl = $"/{this.index}";
            var host = this.hosts.First();

            //// curl -XPOST %ESURL%/_aliases?pretty --data "{\"actions\" : [{ \"remove\" : { \"index\" : \"patients2\", \"alias\" : \"patients\" } }]}"
            var text = "{\"actions\" : [{ \"remove\" : { \"index\" : \"" + this.index + "\", \"alias\" : \"" + this.alias
                       + "\" } }]}";

            await this.reliableHttpClient.PostAsyncString(
                new Uri(new Uri(host), "/_aliases?pretty"),
                text,
                "PostAliases");

            var requestUri = new Uri(new Uri(host), relativeUrl);

            await this.reliableHttpClient.DeleteAsync(requestUri);

            // now also delete the alias in case it was pointing to some other index

            // DELETE /logs_20162801/_alias/current_day
            await this.reliableHttpClient.DeleteAsync(new Uri(new Uri(host), "/_all/_alias/" + this.alias));
        }

        /// <inheritdoc />
        /// <summary>
        /// The start upload.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Threading.Tasks.Task" />.
        /// </returns>
        public async Task StartUploadAsync()
        {
            if (!this.keepIndexOnline)
            {
                var host = this.hosts.First() ?? throw new ArgumentNullException("hosts.First()");

                // curl -XPOST 'http://localhost:9200/_forcemerge?only_expunge_deletes=true'
                await this.reliableHttpClient.PostAsync(new Uri(new Uri(host), "/_forcemerge?only_expunge_deletes=true"), null, "ForceMerge");

                // https://www.elastic.co/guide/en/elasticsearch/reference/current/index-modules.html#index-codec
                await this.reliableHttpClient.PutAsyncString(
                    new Uri(new Uri(host), "/" + this.index + "/_settings"),
                    "{ \"index\" : {\"refresh_interval\" : \"-1\" } }",
                    "DisableRefresh");

                await this.reliableHttpClient.PutAsyncString(
                    new Uri(new Uri(host), "/" + this.index + "/_settings"),
                    "{ \"index\" : {\"number_of_replicas\" : \"0\" } }",
                    "DisableReplicas");
            }
        }

        /// <summary>
        /// The finish upload.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task FinishUploadAsync()
        {
            //// curl -XPUT %ESURL%/patients2/_settings --data "{ \"index\" : {\"refresh_interval\" : \"1s\" } }"

            var host = this.hosts.First();

            if (!this.keepIndexOnline)
            {
                await this.reliableHttpClient.PutAsyncString(
                    new Uri(new Uri(host), "/" + this.index + "/_settings"),
                    "{ \"index\" : {\"number_of_replicas\" : \"1\" } }",
                    "1");

                await this.reliableHttpClient.PutAsyncString(
                    new Uri(new Uri(host), "/" + this.index + "/_settings"),
                    "{ \"index\" : {\"refresh_interval\" : \"1s\" } }",
                    "EnableRefresh");

                // curl -XPOST %ESURL%/patients2/_forcemerge?max_num_segments=5
                await this.reliableHttpClient.PostAsync(new Uri(new Uri(host), "/" + this.index + "/_forcemerge?max_num_segments=5"), null, "ForceMerge");

                // curl -XPOST %ESURL%/_aliases?pretty --data "{\"actions\" : [{ \"remove\" : { \"index\" : \"patients2\", \"alias\" : \"patients\" } }]}"
                // await
                // client.PostAsyncString(host + "/_aliases?pretty",
                // "{\"actions\" : [{ \"remove\" : { \"index\" : \"" + index + "\", \"alias\" : \"" + alias + "\" } }]}");
            }

            // curl -XPOST %ESURL%/_aliases?pretty --data "{\"actions\" : [{ \"add\" : { \"index\" : \"patients2\", \"alias\" : \"patients\" } }]}"
            var text = "{\"actions\" : [{ \"add\" : { \"index\" : \"" + this.index + "\", \"alias\" : \"" + this.alias
                       + "\" } }]}";

            await this.reliableHttpClient.PostAsyncString(
                new Uri(new Uri(host), "/_aliases?pretty"),
                text,
                "PostAliases");
        }

        /// <inheritdoc />
        /// <summary>
        /// The setup alias.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Threading.Tasks.Task" />.
        /// </returns>
        public async Task SetupAliasAsync()
        {
            var host = this.hosts.First();
            var text = "{\"actions\" : [{ \"add\" : { \"index\" : \"" + this.index + "\", \"alias\" : \"" + this.alias
                       + "\" } }]}";
            await this.reliableHttpClient.PostAsyncString(
                new Uri(new Uri(host), "/_aliases?pretty"),
                text,
                "PostAliases");
        }

        /// <inheritdoc />
        /// <summary>
        /// The upload all files in folder.
        /// </summary>
        /// <param name="folder">
        /// The folder.
        /// </param>
        /// <returns>
        /// The <see cref="T:System.Threading.Tasks.Task" />.
        /// </returns>
        public async Task UploadAllFilesInFolder(string folder)
        {
            // https://www.elastic.co/guide/en/elasticsearch/reference/current/tune-for-indexing-speed.html
            await this.CreateIndexAndMappings(folder);

            await this.InternalUploadAllFilesInFolder(this.index + "-*", $"/{this.index}/{this.entityType}/_bulk?pretty", folder);

            // await InternalUploadAllFilesInFolder(hosts, "patients2-Diagnoses*", @"/patients/_bulk?pretty");
            await this.FinishUploadAsync();
        }

        /// <inheritdoc />
        /// <summary>
        /// The send data to hosts.
        /// </summary>
        /// <param name="batch">
        /// The requestId.
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
        /// The <see cref="T:System.Threading.Tasks.Task" />.
        /// </returns>
        public async Task SendDataToHostsAsync(
            int batch,
            Stream stream,
            bool doLogContent,
            bool doCompress)
        {
            var relativeUrl = $"/{this.index}/{this.entityType}/_bulk?pretty";
            await this.SendStreamToHostsAsync(relativeUrl, Convert.ToString(batch), stream, doLogContent, doCompress);
        }

        /// <inheritdoc />
        /// <summary>
        /// The send main mapping file to hosts.
        /// </summary>
        /// <param name="batch">
        /// The requestId.
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
        /// The <see cref="T:System.Threading.Tasks.Task" />.
        /// </returns>
        public async Task SendMainMappingFileToHostsAsync(
            int batch,
            Stream stream,
            bool doLogContent,
            bool doCompress)
        {
            var relativeUrl = $"/{this.index}";
            await this.SendStreamToHostsAsync(relativeUrl, Convert.ToString(batch), stream, doLogContent, doCompress);
        }

        /// <inheritdoc />
        /// <summary>
        /// The send nested mapping file to hosts.
        /// </summary>
        /// <param name="batch">
        /// The requestId.
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
        /// The <see cref="T:System.Threading.Tasks.Task" />.
        /// </returns>
        public async Task SendNestedMappingFileToHostsAsync(
            int batch,
            Stream stream,
            bool doLogContent,
            bool doCompress)
        {
            var relativeUrl = $"/{this.index}/_mapping/{this.entityType}";
            await this.SendStreamToHostsAsync(relativeUrl, Convert.ToString(batch), stream, doLogContent, doCompress);
        }

        /// <inheritdoc />
        public async Task<string> TestElasticSearchConnection()
        {
            var host = this.hosts.First();

            return await this.reliableHttpClient.GetStringAsync(host);
        }

        /// <inheritdoc />
        /// <summary>
        /// The refresh index.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Threading.Tasks.Task" />.
        /// </returns>
        public async Task RefreshIndex()
        {
            //// curl -XPUT %ESURL%/patients2/_settings --data "{ \"index\" : {\"refresh_interval\" : \"1s\" } }"

            var host = this.hosts.First();

            if (!this.keepIndexOnline)
            {
                await this.reliableHttpClient.PostAsyncString(new Uri(new Uri(host), "/" + this.index + "/_refresh"), null, "RefreshIndex");
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// The send stream to url.
        /// </summary>
        /// <param name="url">
        ///     The url.
        /// </param>
        /// <param name="requestId">
        ///     The requestId.
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
        /// The <see cref="T:System.Threading.Tasks.Task" />.
        /// </returns>
        protected override async Task<IFileUploadResult> SendStreamToUrlAsync(
            Uri url,
            string requestId,
            Stream stream,
            bool doLogContent,
            bool doCompress)
        {
            try
            {
                this.logger.Verbose("Sending file {requestId} of size {streamLength} to {url}", requestId, stream.Length, url);

                // http://stackoverflow.com/questions/30310099/correct-way-to-compress-webapi-post
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
                        this.logger.Verbose("Http Send {requestContent}", requestContent);
                    }
                }

                Interlocked.Increment(ref this.currentRequests);
                var requestStartTimeMillisecs = this.Stopwatch.ElapsedMilliseconds;



                var response = doCompress
                                   ? await this.reliableHttpClient.SendAsyncStreamCompressed(url, this.httpMethod, stream, requestId)
                                   : await this.reliableHttpClient.SendAsyncStream(url, this.httpMethod, stream, requestId);

                var responseContent = await response.ResponseContent.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    Interlocked.Decrement(ref this.currentRequests);

                    var result = JsonConvert.DeserializeObject<ElasticSearchJsonResponse>(responseContent);

                    var stopwatchElapsed = this.Stopwatch.ElapsedMilliseconds;
                    var milliSecondsPerFile = 0; // Convert.ToInt32(stopwatchElapsed / (_totalFiles - _queuedFiles.Count));

                    var millisecsForThisFile = stopwatchElapsed - requestStartTimeMillisecs;

                    if (result.errors)
                    {
                        if (result.items.Any(i => i.update.status == 429))
                        {
                            // add back to queue for sending

                            // _queuedFiles.Enqueue(filepath);
                            this.requestFailures++;
                            this.logger.Error(
                                "ElasticSearchUpload Failed: {requestId} status: {StatusCode} requests:{currentRequests} Left:{QueuedFiles}/{totalFiles}, Speed/file: {milliSecondsPerFile}, This file: {millisecsForThisFile}",
                                requestId,
                                response.StatusCode,
                                this.currentRequests,
                                this.QueuedFiles.Count,
                                this.totalFiles,
                                milliSecondsPerFile,
                                millisecsForThisFile);
                        }
                    }
                    else
                    {
                        this.logger.Verbose(
                            "ElasticSearchUpload Succeeded: {requestId} status: {StatusCode} requests:{currentRequests} Left:{QueuedFiles}/{totalFiles}, Speed/file: {milliSecondsPerFile}, This file: {millisecsForThisFile}",
                            requestId,
                            response.StatusCode,
                            this.currentRequests,
                            this.QueuedFiles.Count,
                            this.totalFiles,
                            milliSecondsPerFile,
                            millisecsForThisFile);
                    }
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
                               ResponseContent = response.ResponseContent
                };
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, url.ToString());
                throw;
            }
        }

        /// <summary>
        /// The send file to url.
        /// </summary>
        /// <param name="url">
        /// The url.
        /// </param>
        /// <param name="filepath">
        /// The file path.
        /// </param>
        /// <param name="batch">
        /// The requestId.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task SendFileToUrl(string url, string filepath, int batch)
        {
            try
            {
                //// http://stackoverflow.com/questions/30310099/correct-way-to-compress-webapi-post

                var baseUri = url;

                // var fileContent = new FileContent(filepath);

                // logger.Verbose("posting file" + filepath);
                Interlocked.Increment(ref this.currentRequests);
                var requestStartTimeMillisecs = this.Stopwatch.ElapsedMilliseconds;

                var response = await this.reliableHttpClient.PutAsyncFileCompressed(new Uri(new Uri(baseUri), url), filepath, Convert.ToString(batch));

                if (response.IsSuccessStatusCode)
                {
                    Interlocked.Decrement(ref this.currentRequests);

                    var responseContent = await response.ResponseContent.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<ElasticSearchJsonResponse>(responseContent);

                    var stopwatchElapsed = this.Stopwatch.ElapsedMilliseconds;
                    var millisecsPerFile =
                        Convert.ToInt32(stopwatchElapsed / (this.totalFiles - this.QueuedFiles.Count));

                    var millisecsForThisFile = stopwatchElapsed - requestStartTimeMillisecs;

                    if (result.errors)
                    {
                        if (result.items.Any(i => i.update.status == 429))
                        {
                            // add back to queue for sending
                            this.QueuedFiles.Enqueue(filepath);
                            this.requestFailures++;

                            this.logger.Error(
                                "ElasticSearchUpload Failed: {filepath} status: {StatusCode} requests:{currentRequests} Left:{QueuedFiles}/{totalFiles}, This file: {millisecsForThisFile}",
                                filepath,
                                response.StatusCode,
                                this.currentRequests,
                                this.QueuedFiles.Count,
                                this.totalFiles,
                                millisecsForThisFile);
                        }
                    }
                    else
                    {
                        this.logger.Verbose(
                            "ElasticSearchUpload Succeeded: {file} status: {StatusCode} requests:{currentRequests} Left:{QueuedFiles}/{totalFiles}, This file: {millisecsForThisFile}",
                            filepath,
                            response.StatusCode,
                            this.currentRequests,
                            this.QueuedFiles.Count,
                            this.totalFiles,
                            millisecsForThisFile);
                    }
                }
                else
                {
                    this.logger.Verbose("========= Error =================");
                    var responseJson = response.ResponseContent;

                    this.logger.Verbose(await responseJson.ReadAsStringAsync());
                    this.logger.Verbose("========= Error =================");
                }
            }
            catch (Exception ex)
            {
                this.logger.Error("{Exception}", ex);
                throw;
            }
        }

        /// <summary>
        /// The run files.
        /// </summary>
        /// <param name="relativeUrl">
        /// The relative url.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task RunFiles(string relativeUrl)
        {
            if (!this.QueuedFiles.IsEmpty)
            {
                int i = 0;
#pragma warning disable IDE0018 // Inline variable declaration
                // ReSharper disable once InlineOutVariableDeclaration
                string filename;
#pragma warning restore IDE0018 // Inline variable declaration
                while (this.QueuedFiles.TryDequeue(out filename))
                {
                    try
                    {
                        await this.maxThread.WaitAsync();

                        if (this.requestFailures > 0)
                        {
                            await Task.Delay(1000 * this.requestFailures);
                        }

                        // find url to use
                        var hostNumber = this.QueuedFiles.Count % this.hosts.Count;

                        var url = this.hosts[hostNumber] + relativeUrl;

                        // var url = hosts.First() + @"/_cluster/health?pretty";
                        await this.SendFileToUrl(url, filename, ++i);
                    }
                    finally
                    {
                        this.maxThread.Release();
                    }
                }
            }
        }

        /// <summary>
        /// The upload files.
        /// </summary>
        /// <param name="relativeUrl">
        /// The relative url.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task UploadFiles(string relativeUrl)
        {
            await this.RunFiles(relativeUrl);
        }

        /// <summary>
        /// The internal upload all files in folder.
        /// </summary>
        /// <param name="searchPattern">
        /// The search pattern.
        /// </param>
        /// <param name="relativeUrl">
        /// The relative url.
        /// </param>
        /// <param name="folder">
        /// The folder.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task InternalUploadAllFilesInFolder(string searchPattern, string relativeUrl, string folder)
        {
            this.currentRequests = 0;
            this.Stopwatch.Reset();
            this.Stopwatch.Start();

            var files = Directory.EnumerateFiles(folder, searchPattern);
            var fileList = files.ToList();

            fileList.ForEach(f => this.QueuedFiles.Enqueue(f));

            this.totalFiles = fileList.Count;

            await this.UploadFiles(relativeUrl);

            this.Stopwatch.Stop();

            var stopwatchElapsed = this.Stopwatch.Elapsed;
            var milliSecsPerFile = stopwatchElapsed.TotalMilliseconds / fileList.Count;

            this.logger.Verbose(
                "total: {stopwatchElapsed}, per file: {milliSecsPerFile}",
                stopwatchElapsed,
                milliSecsPerFile);
        }
    }
}
