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
using NLog;

namespace ElasticSearchApiCaller
{
    public class FileUploader
    {
        const string folder = @"c:\Catalyst\demodata\patientjson";

        private static readonly Logger Logger =  LogManager.GetLogger("FileUploader");

        private const int NumberOfParallelUploads = 50;

        readonly Stopwatch _stopwatch = new Stopwatch();

        private SemaphoreSlim maxThread = new SemaphoreSlim(NumberOfParallelUploads);
        private int _totalFiles;

        private int _currentRequests = 0;

        private readonly ConcurrentQueue<string> _queuedFiles = new ConcurrentQueue<string>();
        private int _requestFailures;

        public async Task CreateIndexAndMappings(List<string> hosts)
        {
            using (var client = new HttpClient(new LoggingHandler(new HttpClientHandler())))
            {
                var host = hosts.First();

                // curl -XPOST %ESURL%/_aliases?pretty --data "{\"actions\" : [{ \"remove\" : { \"index\" : \"patients2\", \"alias\" : \"patients\" } }]}"
                await
                    client.PostAsyncString(host + "/_aliases?pretty",
                        "{\"actions\" : [{ \"remove\" : { \"index\" : \"patients2\", \"alias\" : \"patients\" } }]}");

                var requestUri = host + @"/patients2";

                await client.DeleteAsync(requestUri);

                // curl -XPOST 'http://localhost:9200/_forcemerge?only_expunge_deletes=true'
                await client.PostAsync(host + "/_forcemerge?only_expunge_deletes=true", null);

                await client.PutAsyncFile(requestUri, folder + @"\mainmapping.json");

                await InternalUploadAllFilesInFolder(hosts, "mapping*", @"/patients2/_mapping/patient");

                // curl -XPUT %ESURL%/patients2/_settings --data "{ \"index\" : {\"refresh_interval\" : \"-1\" } }"

                // https://www.elastic.co/guide/en/elasticsearch/reference/current/index-modules.html#index-codec

                await
                    client.PutAsyncString(host + "/patients2/_settings",
                        "{ \"index\" : {\"refresh_interval\" : \"30\" } }");

                await
                    client.PutAsyncString(host + "/patients2/_settings",
                        "{ \"index\" : {\"number_of_replicas\" : \"0\" } }");
            }
        }

        public async Task DeleteIndex(List<string> hosts, string relativeUrl)
        {
            using (var client = new HttpClient(new LoggingHandler(new HttpClientHandler())))
            {
                var host = hosts.First();

                // curl -XPOST %ESURL%/_aliases?pretty --data "{\"actions\" : [{ \"remove\" : { \"index\" : \"patients2\", \"alias\" : \"patients\" } }]}"
                await
                    client.PostAsyncString(host + "/_aliases?pretty",
                        "{\"actions\" : [{ \"remove\" : { \"index\" : \"patients2\", \"alias\" : \"patients\" } }]}");

                var requestUri = host + relativeUrl;

                await client.DeleteAsync(requestUri);
            }
        }

        public async Task FinishUpload(List<string> hosts)
        {
            // curl -XPUT %ESURL%/patients2/_settings --data "{ \"index\" : {\"refresh_interval\" : \"1s\" } }"
            using (var client = new HttpClient(new LoggingHandler(new HttpClientHandler())))
            {
                var host = hosts.First();

                await
                    client.PutAsyncString(host + "/patients2/_settings",
                        "{ \"index\" : {\"number_of_replicas\" : \"1\" } }");

                await
                    client.PutAsyncString(host + "/patients2/_settings",
                        "{ \"index\" : {\"refresh_interval\" : \"1s\" } }");

                // curl -XPOST %ESURL%/patients2/_forcemerge?max_num_segments=5
                await client.PostAsync(host + "/patients2/_forcemerge?max_num_segments=5", null);

                // curl -XPOST %ESURL%/_aliases?pretty --data "{\"actions\" : [{ \"remove\" : { \"index\" : \"patients2\", \"alias\" : \"patients\" } }]}"
                await
                    client.PostAsyncString(host + "/_aliases?pretty",
                        "{\"actions\" : [{ \"remove\" : { \"index\" : \"patients2\", \"alias\" : \"patients\" } }]}");

                // curl -XPOST %ESURL%/_aliases?pretty --data "{\"actions\" : [{ \"add\" : { \"index\" : \"patients2\", \"alias\" : \"patients\" } }]}"
                await
                    client.PostAsyncString(host + "/_aliases?pretty",
                        "{\"actions\" : [{ \"add\" : { \"index\" : \"patients2\", \"alias\" : \"patients\" } }]}");
            }
        }

        public async Task SetupAlias(List<string> hosts, string indexName, string aliasName)
        {
            using (var client = new HttpClient(new LoggingHandler(new HttpClientHandler())))
            {
                var host = hosts.First();

                await
                    client.PostAsyncString(host + "/_aliases?pretty",
                        "{\"actions\" : [{ \"add\" : { \"index\" : \"" + indexName + "\", \"alias\" : \"" + aliasName + "\" } }]}");
            }
        }

        public async Task UploadAllFilesInFolder(List<string> hosts)
        {
            // https://www.elastic.co/guide/en/elasticsearch/reference/current/tune-for-indexing-speed.html

            await CreateIndexAndMappings(hosts);
            await InternalUploadAllFilesInFolder(hosts, "patients2-*", @"/patients/patient/_bulk?pretty");
            //await InternalUploadAllFilesInFolder(hosts, "patients2-Diagnoses*", @"/patients/_bulk?pretty");
            await FinishUpload(hosts);
        }

        private async Task InternalUploadAllFilesInFolder(List<string> hosts, string searchPattern, string relativeUrl)
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

            Logger.Trace($"total: {stopwatchElapsed}, per file: {millisecsPerFile}");

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

                //using (var client = new HttpClient(new LoggingHandler(new HttpClientHandler())))
                using (var handler = new HttpClientHandler())
                {
                    handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                    using (var client = new HttpClient(new RetryHandler(handler)))
                    //using (var client = new HttpClient(new RetryHandler(new LoggingHandler(handler))))
                    {
                        var baseUri = url;
                        client.BaseAddress = new Uri(baseUri);
                        client.DefaultRequestHeaders.Accept.Clear();
                        //var fileContent = new FileContent(filepath);

                        //Logger.Trace("posting file" + filepath);

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
                                    Logger.Trace($"Failed: {filepath} status: {response.StatusCode} requests:{_currentRequests} Left:{_queuedFiles.Count}/{_totalFiles}, Speed/file: {millisecsPerFile}, This file: {millisecsForThisFile}");
                                }
                            }
                            else
                            {
                                Logger.Trace($"Finished: {filepath} status: {response.StatusCode} requests:{_currentRequests} Left:{_queuedFiles.Count}/{_totalFiles}, Speed/file: {millisecsPerFile}, This file: {millisecsForThisFile}");
                            }

                        }
                        else
                        {
                            Logger.Trace("========= Error =================");
                            var responseJson = await response.Content.ReadAsStringAsync();

                            Logger.Trace(responseJson);
                            Logger.Trace("========= Error =================");

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Trace(ex);
                throw;
            }

        }

        internal async Task SendStreamToHosts(List<string> hosts, string relativeUrl, int batch, Stream stream)
        {
            var hostNumber = batch % hosts.Count;

            var url = hosts[hostNumber] + relativeUrl;

            await SendStreamToUrl(url,batch, stream);
        }
        internal async Task SendStreamToUrl(string url, int batch, Stream stream)
        {
            try
            {
                Logger.Trace($"Sending file {batch} of size {stream.Length:N0} to {url}");

                // http://stackoverflow.com/questions/30310099/correct-way-to-compress-webapi-post

                //using (var client = new HttpClient(new LoggingHandler(new HttpClientHandler())))
                using (var handler = new HttpClientHandler())
                {
                    handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                    using (var client = new HttpClient(new RetryHandler(handler)))
                    //using (var client = new HttpClient(new RetryHandler(new LoggingHandler(handler))))
                    {
                        var baseUri = url;
                        client.BaseAddress = new Uri(baseUri);
                        client.DefaultRequestHeaders.Accept.Clear();
                        //var fileContent = new FileContent(filepath);

                        //Logger.Trace("posting file" + filepath);
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

                            Logger.Trace($"{requestContent}");
                            }
                        }

                        Interlocked.Increment(ref _currentRequests);
                        var requestStartTimeMillisecs = _stopwatch.ElapsedMilliseconds;

                        var response = await client.PutAsyncStreamCompressed(url, stream);

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
                                Logger.Trace($"Finished: {batch} status: {response.StatusCode} requests:{_currentRequests} Left:{_queuedFiles.Count}/{_totalFiles}, Speed/file: {millisecsPerFile}, This file: {millisecsForThisFile}");
                            }

                        }
                        else
                        {
                            //Logger.Trace("========= Error =================");
                            Logger.Error(requestContent);

                            var responseJson = await response.Content.ReadAsStringAsync();

                            Logger.Error(responseJson);
                            //Logger.Trace("========= Error =================");

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                throw;
            }

        }

        public async Task<string> TestElasticSearchConnection(List<string> hosts)
        {
            using (var client = new HttpClient(new LoggingHandler(new HttpClientHandler())))
            {
                var host = hosts.First();

                return await
                    client.GetStringAsync(host);
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
