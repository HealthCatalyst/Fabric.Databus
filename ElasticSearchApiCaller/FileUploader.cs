using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ElasticSearchApiCaller
{
    using ElasticSearchSqlFeeder.Interfaces;

    using Serilog;
    using Serilog.Core;

    public class FileUploader : IFileUploader
    {
        private static readonly Logger Logger = new LoggerConfiguration().CreateLogger();

        private const int NumberOfParallelUploads = 50;

        readonly Stopwatch _stopwatch = new Stopwatch();

        private readonly SemaphoreSlim maxThread = new SemaphoreSlim(NumberOfParallelUploads);
        private int _totalFiles;

        private int _currentRequests = 0;

        private readonly ConcurrentQueue<string> _queuedFiles = new ConcurrentQueue<string>();
        private int _requestFailures;
        private readonly string _username;
        private readonly string _password;
        private readonly bool _keepIndexOnline;

        public FileUploader(string username, string password, bool keepIndexOnline)
        {
            _username = username;
            _password = password;
            _keepIndexOnline = keepIndexOnline;
        }

        public async Task CreateIndexAndMappings(List<string> hosts, string index, string alias, string entity, string folder)
        {
            using (var client = new HttpClient(new HttpLoggingHandler(new HttpClientHandler(), doLogContent: true)))
            {
                var host = hosts.First();

                AddAuthorizationToken(client);

                // curl -XPOST %ESURL%/_aliases?pretty --data "{\"actions\" : [{ \"remove\" : { \"index\" : \"patients2\", \"alias\" : \"patients\" } }]}"
                await
                    client.PostAsyncString(host + "/_aliases?pretty",
                        "{\"actions\" : [{ \"remove\" : { \"index\" : \"" + index + "\", \"alias\" : \"" + alias +
                        "\" } }]}");

                var requestUri = host + $"/{index}";

                await client.DeleteAsync(requestUri);

                // curl -XPOST 'http://localhost:9200/_forcemerge?only_expunge_deletes=true'
                await client.PostAsync(host + "/_forcemerge?only_expunge_deletes=true", null);

                await client.PutAsyncFile(requestUri, folder + @"\mainmapping.json");

                await InternalUploadAllFilesInFolder(hosts, "mapping*", $"/{index}/_mapping/{entity}", folder);

                // curl -XPUT %ESURL%/patients2/_settings --data "{ \"index\" : {\"refresh_interval\" : \"-1\" } }"

                // https://www.elastic.co/guide/en/elasticsearch/reference/current/tune-for-indexing-speed.html

                // https://www.elastic.co/guide/en/elasticsearch/reference/current/index-modules.html#index-codec

                if (!_keepIndexOnline)
                {
                    await
                        client.PutAsyncString(host + "/" + index + "/_settings",
                            "{ \"index\" : {\"refresh_interval\" : \"-1\" } }");

                    await
                        client.PutAsyncString(host + "/" + index + "/_settings",
                            "{ \"index\" : {\"number_of_replicas\" : \"0\" } }");
                }
            }
        }

        private void AddAuthorizationToken(HttpClient client)
        {
            if (!string.IsNullOrEmpty(_username) && !string.IsNullOrEmpty(_password))
            {
                var byteArray = Encoding.ASCII.GetBytes($"{_username}:{_password}");
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            }

        }

        public async Task DeleteIndex(List<string> hosts, string relativeUrl, string index, string alias)
        {
            using (var client = new HttpClient(new HttpLoggingHandler(new HttpClientHandler(), doLogContent: true)))
            {
                var host = hosts.First();

                AddAuthorizationToken(client);

                // curl -XPOST %ESURL%/_aliases?pretty --data "{\"actions\" : [{ \"remove\" : { \"index\" : \"patients2\", \"alias\" : \"patients\" } }]}"
                await
                    client.PostAsyncString(host + "/_aliases?pretty",
                        "{\"actions\" : [{ \"remove\" : { \"index\" : \"" + index + "\", \"alias\" : \"" + alias + "\" } }]}");

                var requestUri = host + relativeUrl;

                await client.DeleteAsync(requestUri);

                // now also delete the alias in case it was pointing to some other index

                // DELETE /logs_20162801/_alias/current_day
                await client.DeleteAsync(host + "/_all/_alias/" + alias);

            }
        }

        public async Task StartUpload(List<string> hosts, string index, string alias)
        {
            if (!_keepIndexOnline)
            {
                using (var client = new HttpClient(new HttpLoggingHandler(new HttpClientHandler(), doLogContent: true)))
                {
                    var host = hosts.First() ?? throw new ArgumentNullException("hosts.First()");

                    AddAuthorizationToken(client);
                    // curl -XPOST 'http://localhost:9200/_forcemerge?only_expunge_deletes=true'
                    await client.PostAsync(host + "/_forcemerge?only_expunge_deletes=true", null);

                    // https://www.elastic.co/guide/en/elasticsearch/reference/current/index-modules.html#index-codec
                    await
                        client.PutAsyncString(host + "/" + index + "/_settings",
                            "{ \"index\" : {\"refresh_interval\" : \"-1\" } }");

                    await
                        client.PutAsyncString(host + "/" + index + "/_settings",
                            "{ \"index\" : {\"number_of_replicas\" : \"0\" } }");
                }
            }
        }

        public async Task FinishUpload(List<string> hosts, string index, string alias)
        {
            // curl -XPUT %ESURL%/patients2/_settings --data "{ \"index\" : {\"refresh_interval\" : \"1s\" } }"
            using (var client = new HttpClient(new HttpLoggingHandler(new HttpClientHandler(), doLogContent: true)))
            {
                var host = hosts.First();

                AddAuthorizationToken(client);

                if (!_keepIndexOnline)
                {
                    await
                        client.PutAsyncString(host + "/" + index + "/_settings",
                            "{ \"index\" : {\"number_of_replicas\" : \"1\" } }");

                    await
                        client.PutAsyncString(host + "/" + index + "/_settings",
                            "{ \"index\" : {\"refresh_interval\" : \"1s\" } }");

                    // curl -XPOST %ESURL%/patients2/_forcemerge?max_num_segments=5
                    await client.PostAsync(host + "/" + index + "/_forcemerge?max_num_segments=5", null);

                    // curl -XPOST %ESURL%/_aliases?pretty --data "{\"actions\" : [{ \"remove\" : { \"index\" : \"patients2\", \"alias\" : \"patients\" } }]}"
                    //await
                    //    client.PostAsyncString(host + "/_aliases?pretty",
                    //        "{\"actions\" : [{ \"remove\" : { \"index\" : \"" + index + "\", \"alias\" : \"" + alias + "\" } }]}");
                }

                // curl -XPOST %ESURL%/_aliases?pretty --data "{\"actions\" : [{ \"add\" : { \"index\" : \"patients2\", \"alias\" : \"patients\" } }]}"
                await
                    client.PostAsyncString(host + "/_aliases?pretty",
                        "{\"actions\" : [{ \"add\" : { \"index\" : \"" + index + "\", \"alias\" : \"" + alias + "\" } }]}");
            }
        }

        public async Task SetupAlias(List<string> hosts, string indexName, string aliasName)
        {
            using (var client = new HttpClient(new HttpLoggingHandler(new HttpClientHandler(), doLogContent: true)))
            {
                var host = hosts.First();
                AddAuthorizationToken(client);
                await
                    client.PostAsyncString(host + "/_aliases?pretty",
                        "{\"actions\" : [{ \"add\" : { \"index\" : \"" + indexName + "\", \"alias\" : \"" + aliasName + "\" } }]}");
            }
        }

        public async Task UploadAllFilesInFolder(List<string> hosts, string index, string alias, string entity, string folder)
        {
            // https://www.elastic.co/guide/en/elasticsearch/reference/current/tune-for-indexing-speed.html

            await CreateIndexAndMappings(hosts, index, alias, entity, folder);
            await InternalUploadAllFilesInFolder(hosts, index + "-*", $"/{index}/{entity}/_bulk?pretty", folder);
            //await InternalUploadAllFilesInFolder(hosts, "patients2-Diagnoses*", @"/patients/_bulk?pretty");
            await FinishUpload(hosts, index, alias);
        }

        private async Task InternalUploadAllFilesInFolder(List<string> hosts, string searchPattern, string relativeUrl, string folder)
        {
            _currentRequests = 0;
            _stopwatch.Reset();
            _stopwatch.Start();

            var files = Directory.EnumerateFiles(folder, searchPattern);
            var fileList = files.ToList();

            fileList.ForEach(f => _queuedFiles.Enqueue(f));

            _totalFiles = fileList.Count;

            await UploadFiles(hosts, relativeUrl);

            _stopwatch.Stop();

            var stopwatchElapsed = _stopwatch.Elapsed;
            var millisecsPerFile = stopwatchElapsed.TotalMilliseconds / fileList.Count;

            Logger.Verbose($"total: {stopwatchElapsed}, per file: {millisecsPerFile}");

        }

        private async Task UploadFiles(List<string> hosts, string relativeUrl)
        {

            await RunFiles(hosts, relativeUrl);

            //await Task.WhenAll(tasks);
        }

        private async Task RunFiles(List<string> hosts, string relativeUrl)
        {
            if (!_queuedFiles.IsEmpty)
            {
                string filename;
                while (_queuedFiles.TryDequeue(out filename))
                {
                    try
                    {

                        await maxThread.WaitAsync();

                        if (_requestFailures > 0)
                        {
                            await Task.Delay(1000 * _requestFailures);
                        }

                        //find url to use
                        var hostNumber = _queuedFiles.Count % hosts.Count;

                        var url = hosts[hostNumber] + relativeUrl;
                        //var url = hosts.First() + @"/_cluster/health?pretty";
                        await SendFileToUrl(url, filename);
                    }
                    finally
                    {
                        maxThread.Release();
                    }
                }
            }
        }

        private async Task SendFileToUrl(string url, string filepath)
        {
            try
            {
                // http://stackoverflow.com/questions/30310099/correct-way-to-compress-webapi-post

                using (var handler = new HttpClientHandler())
                {
                    handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                    using (var client = new HttpClient(new RetryHandler(new HttpLoggingHandler(handler, doLogContent: false))))
                    {
                        var baseUri = url;
                        client.BaseAddress = new Uri(baseUri);
                        client.DefaultRequestHeaders.Accept.Clear();

                        AddAuthorizationToken(client);

                        //var fileContent = new FileContent(filepath);

                        //Logger.Verbose("posting file" + filepath);

                        Interlocked.Increment(ref _currentRequests);
                        var requestStartTimeMillisecs = _stopwatch.ElapsedMilliseconds;

                        var response = await client.PutAsyncFileCompressed(url, filepath);

                        if (response.IsSuccessStatusCode)
                        {
                            Interlocked.Decrement(ref _currentRequests);

                            var responseContent = await response.Content.ReadAsStringAsync();
                            var result = JsonConvert.DeserializeObject<ElasticSearchJsonResponse>(responseContent);

                            var stopwatchElapsed = _stopwatch.ElapsedMilliseconds;
                            var millisecsPerFile = Convert.ToInt32(stopwatchElapsed / (_totalFiles - _queuedFiles.Count));

                            var millisecsForThisFile = stopwatchElapsed - requestStartTimeMillisecs;

                            if (result.errors)
                            {
                                if (result.items.Any(i => i.update.status == 429))
                                {
                                    // add back to queue for sending
                                    _queuedFiles.Enqueue(filepath);
                                    _requestFailures++;
                                    Logger.Verbose($"Failed: {filepath} status: {response.StatusCode} requests:{_currentRequests} Left:{_queuedFiles.Count}/{_totalFiles}, Speed/file: {millisecsPerFile}, This file: {millisecsForThisFile}");
                                }
                            }
                            else
                            {
                                Logger.Verbose($"Finished: {filepath} status: {response.StatusCode} requests:{_currentRequests} Left:{_queuedFiles.Count}/{_totalFiles}, Speed/file: {millisecsPerFile}, This file: {millisecsForThisFile}");
                            }

                        }
                        else
                        {
                            Logger.Verbose("========= Error =================");
                            var responseJson = await response.Content.ReadAsStringAsync();

                            Logger.Verbose(responseJson);
                            Logger.Verbose("========= Error =================");

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Verbose("{Exception}", ex);
                throw;
            }

        }

        public async Task SendStreamToHosts(List<string> hosts, string relativeUrl, int batch, Stream stream, bool doLogContent, bool doCompress)
        {
            var hostNumber = batch % hosts.Count;

            var url = hosts[hostNumber] + relativeUrl;

            await SendStreamToUrl(url, batch, stream, doLogContent, doCompress);
        }

        internal async Task SendStreamToUrl(string url, int batch, Stream stream, bool doLogContent, bool doCompress)
        {
            try
            {
                Logger.Verbose($"Sending file {batch} of size {stream.Length:N0} to {url}");

                // http://stackoverflow.com/questions/30310099/correct-way-to-compress-webapi-post

                //using (var client = new HttpClient(new LoggingHandler(new HttpClientHandler())))
                using (var handler = new HttpClientHandler())
                {
                    handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                    using (var client = new HttpClient(new RetryHandler(new HttpLoggingHandler(handler, doLogContent))))
                    {
                        var baseUri = url;
                        client.BaseAddress = new Uri(baseUri);
                        client.DefaultRequestHeaders.Accept.Clear();
                        //var fileContent = new FileContent(filepath);

                        AddAuthorizationToken(client);

                        //Logger.Verbose("posting file" + filepath);
                        string requestContent;

                        using (var newMemoryStream = new MemoryStream())
                        {
                            stream.Position = 0;
                            stream.CopyTo(newMemoryStream);
                            newMemoryStream.Position = 0;
                            using (var reader = new StreamReader(newMemoryStream, Encoding.UTF8))
                            {
                                requestContent = reader.ReadToEnd();
                                // Do something with the value

                                Logger.Verbose($"{requestContent}");
                            }
                        }

                        Interlocked.Increment(ref _currentRequests);
                        var requestStartTimeMillisecs = _stopwatch.ElapsedMilliseconds;

                        var response = doCompress
                            ? await client.PutAsyncStreamCompressed(url, stream)
                            : await client.PutAsyncStream(url, stream);

                        if (response.IsSuccessStatusCode)
                        {
                            Interlocked.Decrement(ref _currentRequests);

                            var responseContent = await response.Content.ReadAsStringAsync();
                            var result = JsonConvert.DeserializeObject<ElasticSearchJsonResponse>(responseContent);

                            var stopwatchElapsed = _stopwatch.ElapsedMilliseconds;
                            var millisecsPerFile = 0;// Convert.ToInt32(stopwatchElapsed / (_totalFiles - _queuedFiles.Count));

                            var millisecsForThisFile = stopwatchElapsed - requestStartTimeMillisecs;

                            if (result.errors)
                            {
                                if (result.items.Any(i => i.update.status == 429))
                                {
                                    // add back to queue for sending
                                    //_queuedFiles.Enqueue(filepath);
                                    _requestFailures++;
                                    Logger.Error($"Failed: {batch} status: {response.StatusCode} requests:{_currentRequests} Left:{_queuedFiles.Count}/{_totalFiles}, Speed/file: {millisecsPerFile}, This file: {millisecsForThisFile}");
                                }
                            }
                            else
                            {
                                Logger.Verbose($"Finished: {batch} status: {response.StatusCode} requests:{_currentRequests} Left:{_queuedFiles.Count}/{_totalFiles}, Speed/file: {millisecsPerFile}, This file: {millisecsForThisFile}");
                            }

                        }
                        else
                        {
                            //Logger.Verbose("========= Error =================");
                            Logger.Error(requestContent);

                            var responseJson = await response.Content.ReadAsStringAsync();

                            Logger.Error(responseJson);
                            //Logger.Verbose("========= Error =================");

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, url);
                throw;
            }

        }

        public async Task<string> TestElasticSearchConnection(List<string> hosts)
        {
            using (var client = new HttpClient(new HttpLoggingHandler(new HttpClientHandler(), doLogContent: true)))
            {
                var host = hosts.First();

                AddAuthorizationToken(client);

                return await
                    client.GetStringAsync(host);
            }
        }

        public async Task RefreshIndex(List<string> hosts, string index, string alias)
        {
            // curl -XPUT %ESURL%/patients2/_settings --data "{ \"index\" : {\"refresh_interval\" : \"1s\" } }"
            using (var client = new HttpClient(new HttpLoggingHandler(new HttpClientHandler(), doLogContent: true)))
            {
                var host = hosts.First();

                AddAuthorizationToken(client);

                if (!_keepIndexOnline)
                {
                    await
                        client.PostAsyncString(host + "/" + index + "/_refresh",null);
                }
            }
        }
    }

    public class ElasticSearchJsonResponse
    {
        // ReSharper disable once InconsistentNaming
        public int took { get; set; }
        // ReSharper disable once InconsistentNaming
        public bool errors { get; set; }
        // ReSharper disable once InconsistentNaming
        public List<ElasticSearchJsonResponseItem> items { get; set; }
    }

    public class ElasticSearchJsonResponseItem
    {
        // ReSharper disable once InconsistentNaming
        public ElasticSearchJsonResponseUpdate update { get; set; }
    }

    public class ElasticSearchJsonResponseUpdate
    {
        // ReSharper disable once InconsistentNaming
        public string _index { get; set; }
        // ReSharper disable once InconsistentNaming
        public string _type { get; set; }
        // ReSharper disable once InconsistentNaming
        public int status { get; set; }
    }

}
