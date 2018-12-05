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
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using Fabric.Databus.Http;
    using Fabric.Databus.Interfaces.ElasticSearch;
    using Fabric.Databus.Interfaces.Http;

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
            HttpMethod method)
        : base(logger, hosts, httpClientFactory, httpRequestInterceptor, httpResponseInterceptor, method)
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

        /// <summary>
        /// The create index and mappings.
        /// </summary>
        /// <param name="folder">
        /// The folder.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task CreateIndexAndMappings(string folder)
        {
            var host = this.hosts.First();

            //// curl -XPOST %ESURL%/_aliases?pretty --data "{\"actions\" : [{ \"remove\" : { \"index\" : \"patients2\", \"alias\" : \"patients\" } }]}"

            var text =
                $"{{\"actions\" : [{{ \"remove\" : {{ \"index\" : \"{this.index}\", \"alias\" : \"{this.alias}\" }} }}]}}";

            await this.HttpClientHelper.PostAsyncString(
                new Uri(new Uri(host), "/_aliases?pretty"),
                text);

            var requestUri = new Uri(new Uri(host), $"/{this.index}");

            await this.HttpClientHelper.DeleteAsync(requestUri);

            // curl -XPOST 'http://localhost:9200/_forcemerge?only_expunge_deletes=true'
            await this.HttpClientHelper.PostAsync(new Uri(new Uri(host), "/_forcemerge?only_expunge_deletes=true"), null);

            await this.HttpClientHelper.PutAsyncFile(requestUri, folder + @"\mainmapping.json");

            await this.InternalUploadAllFilesInFolder("mapping*", $"/{this.index}/_mapping/{this.entityType}", folder);

            // curl -XPUT %ESURL%/patients2/_settings --data "{ \"index\" : {\"refresh_interval\" : \"-1\" } }"

            // https://www.elastic.co/guide/en/elasticsearch/reference/current/tune-for-indexing-speed.html

            // https://www.elastic.co/guide/en/elasticsearch/reference/current/index-modules.html#index-codec
            if (!this.keepIndexOnline)
            {
                await this.HttpClientHelper.PutAsyncString(
                    new Uri(new Uri(host), "/" + this.index + "/_settings"),
                    "{ \"index\" : {\"refresh_interval\" : \"-1\" } }");

                await this.HttpClientHelper.PutAsyncString(
                    new Uri(new Uri(host), "/" + this.index + "/_settings"),
                    "{ \"index\" : {\"number_of_replicas\" : \"0\" } }");
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

            await this.HttpClientHelper.PostAsyncString(
                new Uri(new Uri(host), "/_aliases?pretty"),
                text);

            var requestUri = new Uri(new Uri(host), relativeUrl);

            await this.HttpClientHelper.DeleteAsync(requestUri);

            // now also delete the alias in case it was pointing to some other index

            // DELETE /logs_20162801/_alias/current_day
            await this.HttpClientHelper.DeleteAsync(new Uri(new Uri(host), "/_all/_alias/" + this.alias));
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
                await this.HttpClientHelper.PostAsync(new Uri(new Uri(host), "/_forcemerge?only_expunge_deletes=true"), null);

                // https://www.elastic.co/guide/en/elasticsearch/reference/current/index-modules.html#index-codec
                await this.HttpClientHelper.PutAsyncString(
                    new Uri(new Uri(host), "/" + this.index + "/_settings"),
                    "{ \"index\" : {\"refresh_interval\" : \"-1\" } }");

                await this.HttpClientHelper.PutAsyncString(
                    new Uri(new Uri(host), "/" + this.index + "/_settings"),
                    "{ \"index\" : {\"number_of_replicas\" : \"0\" } }");
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
                await this.HttpClientHelper.PutAsyncString(
                    new Uri(new Uri(host), "/" + this.index + "/_settings"),
                    "{ \"index\" : {\"number_of_replicas\" : \"1\" } }");

                await this.HttpClientHelper.PutAsyncString(
                    new Uri(new Uri(host), "/" + this.index + "/_settings"),
                    "{ \"index\" : {\"refresh_interval\" : \"1s\" } }");

                // curl -XPOST %ESURL%/patients2/_forcemerge?max_num_segments=5
                await this.HttpClientHelper.PostAsync(new Uri(new Uri(host), "/" + this.index + "/_forcemerge?max_num_segments=5"), null);

                // curl -XPOST %ESURL%/_aliases?pretty --data "{\"actions\" : [{ \"remove\" : { \"index\" : \"patients2\", \"alias\" : \"patients\" } }]}"
                // await
                // client.PostAsyncString(host + "/_aliases?pretty",
                // "{\"actions\" : [{ \"remove\" : { \"index\" : \"" + index + "\", \"alias\" : \"" + alias + "\" } }]}");
            }

            // curl -XPOST %ESURL%/_aliases?pretty --data "{\"actions\" : [{ \"add\" : { \"index\" : \"patients2\", \"alias\" : \"patients\" } }]}"
            var text = "{\"actions\" : [{ \"add\" : { \"index\" : \"" + this.index + "\", \"alias\" : \"" + this.alias
                       + "\" } }]}";

            await this.HttpClientHelper.PostAsyncString(
                new Uri(new Uri(host), "/_aliases?pretty"),
                text);
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
            await this.HttpClientHelper.PostAsyncString(
                new Uri(new Uri(host), "/_aliases?pretty"),
                text);
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
        /// The <see cref="T:System.Threading.Tasks.Task" />.
        /// </returns>
        public async Task SendDataToHostsAsync(
            int batch,
            Stream stream,
            bool doLogContent,
            bool doCompress)
        {
            var relativeUrl = $"/{this.index}/{this.entityType}/_bulk?pretty";
            await this.SendStreamToHostsAsync(relativeUrl, batch, stream, doLogContent, doCompress);
        }

        /// <inheritdoc />
        /// <summary>
        /// The send main mapping file to hosts.
        /// </summary>
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
        /// The <see cref="T:System.Threading.Tasks.Task" />.
        /// </returns>
        public async Task SendMainMappingFileToHostsAsync(
            int batch,
            Stream stream,
            bool doLogContent,
            bool doCompress)
        {
            var relativeUrl = $"/{this.index}";
            await this.SendStreamToHostsAsync(relativeUrl, batch, stream, doLogContent, doCompress);
        }

        /// <inheritdoc />
        /// <summary>
        /// The send nested mapping file to hosts.
        /// </summary>
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
        /// The <see cref="T:System.Threading.Tasks.Task" />.
        /// </returns>
        public async Task SendNestedMappingFileToHostsAsync(
            int batch,
            Stream stream,
            bool doLogContent,
            bool doCompress)
        {
            var relativeUrl = $"/{this.index}/_mapping/{this.entityType}";
            await this.SendStreamToHostsAsync(relativeUrl, batch, stream, doLogContent, doCompress);
        }

        /// <inheritdoc />
        public async Task<string> TestElasticSearchConnection()
        {
            var host = this.hosts.First();

            return await this.HttpClientHelper.GetStringAsync(host);
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
                await this.HttpClientHelper.PostAsyncString(new Uri(new Uri(host), "/" + this.index + "/_refresh"), null);
            }
        }

        /// <inheritdoc />
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
        /// The <see cref="T:System.Threading.Tasks.Task" />.
        /// </returns>
        protected override async Task<HttpStatusCode> SendStreamToUrlAsync(string url, int batch, Stream stream, bool doLogContent, bool doCompress)
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
                var requestStartTimeMillisecs = this.Stopwatch.ElapsedMilliseconds;


                var fullUri = new Uri(new Uri(baseUri), url);

                var response = doCompress
                                   ? await this.HttpClientHelper.SendAsyncStreamCompressed(fullUri, this.httpMethod, stream)
                                   : await this.HttpClientHelper.SendAsyncStream(fullUri, this.httpMethod, stream);

                if (response.IsSuccessStatusCode)
                {
                    Interlocked.Decrement(ref this.currentRequests);

                    var responseContent = await response.Content.ReadAsStringAsync();
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
                                $"Failed: {batch} status: {response.StatusCode} requests:{this.currentRequests} Left:{this.QueuedFiles.Count}/{this.totalFiles}, Speed/file: {milliSecondsPerFile}, This file: {millisecsForThisFile}");
                        }
                    }
                    else
                    {
                        this.logger.Verbose(
                            $"Finished: {batch} status: {response.StatusCode} requests:{this.currentRequests} Left:{this.QueuedFiles.Count}/{this.totalFiles}, Speed/file: {milliSecondsPerFile}, This file: {millisecsForThisFile}");
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

                return response.StatusCode;
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, url);
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
        /// The filepath.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task SendFileToUrl(string url, string filepath)
        {
            try
            {
                // http://stackoverflow.com/questions/30310099/correct-way-to-compress-webapi-post

                var baseUri = url;

                // var fileContent = new FileContent(filepath);

                // logger.Verbose("posting file" + filepath);
                Interlocked.Increment(ref this.currentRequests);
                var requestStartTimeMillisecs = this.Stopwatch.ElapsedMilliseconds;

                var response = await this.HttpClientHelper.PutAsyncFileCompressed(new Uri(new Uri(baseUri), url), filepath);

                if (response.IsSuccessStatusCode)
                {
                    Interlocked.Decrement(ref this.currentRequests);

                    var responseContent = await response.Content.ReadAsStringAsync();
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
                            this.logger.Verbose(
                                $"Failed: {filepath} status: {response.StatusCode} requests:{this.currentRequests} Left:{this.QueuedFiles.Count}/{this.totalFiles}, Speed/file: {millisecsPerFile}, This file: {millisecsForThisFile}");
                        }
                    }
                    else
                    {
                        this.logger.Verbose(
                            $"Finished: {filepath} status: {response.StatusCode} requests:{this.currentRequests} Left:{this.QueuedFiles.Count}/{this.totalFiles}, Speed/file: {millisecsPerFile}, This file: {millisecsForThisFile}");
                    }
                }
                else
                {
                    this.logger.Verbose("========= Error =================");
                    var responseJson = await response.Content.ReadAsStringAsync();

                    this.logger.Verbose(responseJson);
                    this.logger.Verbose("========= Error =================");
                }
            }
            catch (Exception ex)
            {
                this.logger.Verbose("{Exception}", ex);
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
                string filename;
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
                        await this.SendFileToUrl(url, filename);
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

            this.logger.Verbose($"total: {stopwatchElapsed}, per file: {milliSecsPerFile}");
        }
    }
}
