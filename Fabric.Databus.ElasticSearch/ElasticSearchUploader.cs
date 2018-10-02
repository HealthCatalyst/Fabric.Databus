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
    using Fabric.Databus.Interfaces;

    using Newtonsoft.Json;

    using Serilog;

    /// <inheritdoc />
    /// <summary>
    /// The file uploader.
    /// </summary>
    public class ElasticSearchUploader : IElasticSearchUploader
    {
        /// <summary>
        /// The number of parallel uploads.
        /// </summary>
        private const int NumberOfParallelUploads = 50;

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// The hosts.
        /// </summary>
        private readonly List<string> hosts;

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
        /// The stopwatch.
        /// </summary>
        private readonly Stopwatch stopwatch = new Stopwatch();

        /// <summary>
        /// The max thread.
        /// </summary>
        private readonly SemaphoreSlim maxThread = new SemaphoreSlim(NumberOfParallelUploads);

        /// <summary>
        /// The queued files.
        /// </summary>
        private readonly ConcurrentQueue<string> queuedFiles = new ConcurrentQueue<string>();

        /// <summary>
        /// The username.
        /// </summary>
        private readonly string username;

        /// <summary>
        /// The password.
        /// </summary>
        private readonly string password;

        /// <summary>
        /// The keep index online.
        /// </summary>
        private readonly bool keepIndexOnline;

        /// <summary>
        /// The http client.
        /// </summary>
        private readonly HttpClient httpClient;

        /// <summary>
        /// The total files.
        /// </summary>
        private int totalFiles;

        /// <summary>
        /// The request failures.
        /// </summary>
        private int requestFailures;

        /// <summary>
        /// The current requests.
        /// </summary>
        private int currentRequests = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="ElasticSearchUploader"/> class.
        /// </summary>
        /// <param name="username">
        /// The username.
        /// </param>
        /// <param name="password">
        /// The password.
        /// </param>
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
        public ElasticSearchUploader(
            string username,
            string password,
            bool keepIndexOnline,
            ILogger logger,
            List<string> hosts,
            string index,
            string alias,
            string entityType,
            IHttpClientFactory httpClientFactory)
        {
            if (httpClientFactory == null)
            {
                throw new ArgumentNullException(nameof(httpClientFactory));
            }

            this.username = username;
            this.password = password;
            this.keepIndexOnline = keepIndexOnline;
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.hosts = hosts ?? throw new ArgumentNullException(nameof(hosts));
            this.index = index ?? throw new ArgumentNullException(nameof(index));
            this.alias = alias ?? throw new ArgumentNullException(nameof(alias));
            this.entityType = entityType ?? throw new ArgumentNullException(nameof(entityType));

            this.httpClient = httpClientFactory.Create();
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

            this.AddAuthorizationToken(this.httpClient);

            // curl -XPOST %ESURL%/_aliases?pretty --data "{\"actions\" : [{ \"remove\" : { \"index\" : \"patients2\", \"alias\" : \"patients\" } }]}"
            await this.httpClient.PostAsyncString(
                host + "/_aliases?pretty",
                "{\"actions\" : [{ \"remove\" : { \"index\" : \"" + this.index + "\", \"alias\" : \"" + this.alias
                + "\" } }]}");

            var requestUri = host + $"/{this.index}";

            await this.httpClient.DeleteAsync(requestUri);

            // curl -XPOST 'http://localhost:9200/_forcemerge?only_expunge_deletes=true'
            await this.httpClient.PostAsync(host + "/_forcemerge?only_expunge_deletes=true", null);

            await this.httpClient.PutAsyncFile(requestUri, folder + @"\mainmapping.json");

            await this.InternalUploadAllFilesInFolder("mapping*", $"/{this.index}/_mapping/{this.entityType}", folder);

            // curl -XPUT %ESURL%/patients2/_settings --data "{ \"index\" : {\"refresh_interval\" : \"-1\" } }"

            // https://www.elastic.co/guide/en/elasticsearch/reference/current/tune-for-indexing-speed.html

            // https://www.elastic.co/guide/en/elasticsearch/reference/current/index-modules.html#index-codec
            if (!this.keepIndexOnline)
            {
                await this.httpClient.PutAsyncString(
                    host + "/" + this.index + "/_settings",
                    "{ \"index\" : {\"refresh_interval\" : \"-1\" } }");

                await this.httpClient.PutAsyncString(
                    host + "/" + this.index + "/_settings",
                    "{ \"index\" : {\"number_of_replicas\" : \"0\" } }");
            }
        }

        /// <summary>
        /// The delete index.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task DeleteIndex()
        {
            var relativeUrl = $"/{this.index}";
            var host = this.hosts.First();

            this.AddAuthorizationToken(this.httpClient);

            // curl -XPOST %ESURL%/_aliases?pretty --data "{\"actions\" : [{ \"remove\" : { \"index\" : \"patients2\", \"alias\" : \"patients\" } }]}"
            await this.httpClient.PostAsyncString(
                host + "/_aliases?pretty",
                "{\"actions\" : [{ \"remove\" : { \"index\" : \"" + this.index + "\", \"alias\" : \"" + this.alias
                + "\" } }]}");

            var requestUri = host + relativeUrl;

            await this.httpClient.DeleteAsync(requestUri);

            // now also delete the alias in case it was pointing to some other index

            // DELETE /logs_20162801/_alias/current_day
            await this.httpClient.DeleteAsync(host + "/_all/_alias/" + this.alias);
        }

        /// <summary>
        /// The start upload.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task StartUploadAsync()
        {
            if (!this.keepIndexOnline)
            {
                var host = this.hosts.First() ?? throw new ArgumentNullException("hosts.First()");

                this.AddAuthorizationToken(this.httpClient);

                // curl -XPOST 'http://localhost:9200/_forcemerge?only_expunge_deletes=true'
                await this.httpClient.PostAsync(host + "/_forcemerge?only_expunge_deletes=true", null);

                // https://www.elastic.co/guide/en/elasticsearch/reference/current/index-modules.html#index-codec
                await this.httpClient.PutAsyncString(
                    host + "/" + this.index + "/_settings",
                    "{ \"index\" : {\"refresh_interval\" : \"-1\" } }");

                await this.httpClient.PutAsyncString(
                    host + "/" + this.index + "/_settings",
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
            // curl -XPUT %ESURL%/patients2/_settings --data "{ \"index\" : {\"refresh_interval\" : \"1s\" } }"

            var host = this.hosts.First();

            this.AddAuthorizationToken(this.httpClient);

            if (!this.keepIndexOnline)
            {
                await this.httpClient.PutAsyncString(
                    host + "/" + this.index + "/_settings",
                    "{ \"index\" : {\"number_of_replicas\" : \"1\" } }");

                await this.httpClient.PutAsyncString(
                    host + "/" + this.index + "/_settings",
                    "{ \"index\" : {\"refresh_interval\" : \"1s\" } }");

                // curl -XPOST %ESURL%/patients2/_forcemerge?max_num_segments=5
                await this.httpClient.PostAsync(host + "/" + this.index + "/_forcemerge?max_num_segments=5", null);

                // curl -XPOST %ESURL%/_aliases?pretty --data "{\"actions\" : [{ \"remove\" : { \"index\" : \"patients2\", \"alias\" : \"patients\" } }]}"
                // await
                // client.PostAsyncString(host + "/_aliases?pretty",
                // "{\"actions\" : [{ \"remove\" : { \"index\" : \"" + index + "\", \"alias\" : \"" + alias + "\" } }]}");
            }

            // curl -XPOST %ESURL%/_aliases?pretty --data "{\"actions\" : [{ \"add\" : { \"index\" : \"patients2\", \"alias\" : \"patients\" } }]}"
            await this.httpClient.PostAsyncString(
                host + "/_aliases?pretty",
                "{\"actions\" : [{ \"add\" : { \"index\" : \"" + this.index + "\", \"alias\" : \"" + this.alias
                + "\" } }]}");
        }

        /// <summary>
        /// The setup alias.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task SetupAliasAsync()
        {
            var host = this.hosts.First();
            this.AddAuthorizationToken(this.httpClient);
            await this.httpClient.PostAsyncString(
                host + "/_aliases?pretty",
                "{\"actions\" : [{ \"add\" : { \"index\" : \"" + this.index + "\", \"alias\" : \"" + this.alias
                + "\" } }]}");
        }

        /// <summary>
        /// The upload all files in folder.
        /// </summary>
        /// <param name="folder">
        /// The folder.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
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
            await this.SendStreamToHosts(relativeUrl, batch, stream, doLogContent, doCompress);
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
            await this.SendStreamToHosts(relativeUrl, batch, stream, doLogContent, doCompress);
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
            await this.SendStreamToHosts(relativeUrl, batch, stream, doLogContent, doCompress);
        }

        /// <inheritdoc />
        public async Task SendStreamToHosts(string relativeUrl, int batch, Stream stream, bool doLogContent, bool doCompress)
        {
            var hostNumber = batch % this.hosts.Count;

            var url = this.hosts[hostNumber] + relativeUrl;

            await this.SendStreamToUrl(url, batch, stream, doLogContent, doCompress);
        }

        /// <inheritdoc />
        public async Task<string> TestElasticSearchConnection()
        {
            var host = this.hosts.First();

            this.AddAuthorizationToken(this.httpClient);

            return await this.httpClient.GetStringAsync(host);
        }

        /// <summary>
        /// The refresh index.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task RefreshIndex()
        {
            // curl -XPUT %ESURL%/patients2/_settings --data "{ \"index\" : {\"refresh_interval\" : \"1s\" } }"

            var host = this.hosts.First();

            this.AddAuthorizationToken(this.httpClient);

            if (!this.keepIndexOnline)
            {
                await this.httpClient.PostAsyncString(host + "/" + this.index + "/_refresh", null);
            }
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
                this.httpClient.BaseAddress = new Uri(baseUri);
                this.httpClient.DefaultRequestHeaders.Accept.Clear();

                this.AddAuthorizationToken(this.httpClient);

                // var fileContent = new FileContent(filepath);

                // logger.Verbose("posting file" + filepath);
                Interlocked.Increment(ref this.currentRequests);
                var requestStartTimeMillisecs = this.stopwatch.ElapsedMilliseconds;

                var response = await this.httpClient.PutAsyncFileCompressed(url, filepath);

                if (response.IsSuccessStatusCode)
                {
                    Interlocked.Decrement(ref this.currentRequests);

                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<ElasticSearchJsonResponse>(responseContent);

                    var stopwatchElapsed = this.stopwatch.ElapsedMilliseconds;
                    var millisecsPerFile =
                        Convert.ToInt32(stopwatchElapsed / (this.totalFiles - this.queuedFiles.Count));

                    var millisecsForThisFile = stopwatchElapsed - requestStartTimeMillisecs;

                    if (result.errors)
                    {
                        if (result.items.Any(i => i.update.status == 429))
                        {
                            // add back to queue for sending
                            this.queuedFiles.Enqueue(filepath);
                            this.requestFailures++;
                            this.logger.Verbose(
                                $"Failed: {filepath} status: {response.StatusCode} requests:{this.currentRequests} Left:{this.queuedFiles.Count}/{this.totalFiles}, Speed/file: {millisecsPerFile}, This file: {millisecsForThisFile}");
                        }
                    }
                    else
                    {
                        this.logger.Verbose(
                            $"Finished: {filepath} status: {response.StatusCode} requests:{this.currentRequests} Left:{this.queuedFiles.Count}/{this.totalFiles}, Speed/file: {millisecsPerFile}, This file: {millisecsForThisFile}");
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
        /// The add authorization token.
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        private void AddAuthorizationToken(HttpClient client)
        {
            if (!string.IsNullOrEmpty(this.username) && !string.IsNullOrEmpty(this.password))
            {
                var byteArray = Encoding.ASCII.GetBytes($"{this.username}:{this.password}");
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
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
            if (!this.queuedFiles.IsEmpty)
            {
                string filename;
                while (this.queuedFiles.TryDequeue(out filename))
                {
                    try
                    {

                        await this.maxThread.WaitAsync();

                        if (this.requestFailures > 0)
                        {
                            await Task.Delay(1000 * this.requestFailures);
                        }

                        // find url to use
                        var hostNumber = this.queuedFiles.Count % this.hosts.Count;

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
            this.stopwatch.Reset();
            this.stopwatch.Start();

            var files = Directory.EnumerateFiles(folder, searchPattern);
            var fileList = files.ToList();

            fileList.ForEach(f => this.queuedFiles.Enqueue(f));

            this.totalFiles = fileList.Count;

            await this.UploadFiles(relativeUrl);

            this.stopwatch.Stop();

            var stopwatchElapsed = this.stopwatch.Elapsed;
            var milliSecsPerFile = stopwatchElapsed.TotalMilliseconds / fileList.Count;

            this.logger.Verbose($"total: {stopwatchElapsed}, per file: {milliSecsPerFile}");
        }
    }
}
