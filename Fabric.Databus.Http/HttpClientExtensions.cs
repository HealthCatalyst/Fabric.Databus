namespace Fabric.Databus.Http
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading.Tasks;

    public static class HttpClientExtensions
    {
        private static string _contentTypeHeader = "application/json";

        public static Task<HttpResponseMessage> PutAsyncFile(this HttpClient client, string url,
            string filename)
        {
            var allText = File.ReadAllText(filename);
            var stringcontent = new StringContent(allText, Encoding.UTF8,
                _contentTypeHeader);

            return client.PutAsync(url, stringcontent);
        }

        public static ConfiguredTaskAwaitable<HttpResponseMessage> PutAsyncFileCompressed(this HttpClient client, string baseUri, string relativeUrl,
                                                                        string filename)
        {
            var allText = File.ReadAllText(filename);
            byte[] jsonBytes = Encoding.UTF8.GetBytes(allText);
            MemoryStream ms = new MemoryStream();
            using (GZipStream gzip = new GZipStream(ms, CompressionMode.Compress, true))
            {
                gzip.Write(jsonBytes, 0, jsonBytes.Length);
            }
            ms.Position = 0;
            StreamContent content = new StreamContent(ms);
            content.Headers.ContentType = new MediaTypeHeaderValue(_contentTypeHeader);
            content.Headers.ContentEncoding.Add("gzip");

            var url = new Uri(new Uri(baseUri), relativeUrl);
            return client.PutAsync(url, content).ConfigureAwait(false); ;
        }
        public static ConfiguredTaskAwaitable<HttpResponseMessage> PutAsyncStreamCompressed(this HttpClient client, string baseUri, string relativeUrl,
                                                                        Stream stream)
        {
            stream.Position = 0;
            MemoryStream ms = new MemoryStream();
            using (GZipStream gzip = new GZipStream(ms, CompressionMode.Compress, true))
            {
                stream.CopyTo(gzip);
            }

            stream.Dispose();

            ms.Position = 0;
            StreamContent content = new StreamContent(ms);
            content.Headers.ContentType = new MediaTypeHeaderValue(_contentTypeHeader);
            content.Headers.ContentEncoding.Add("gzip");

            var url = new Uri(new Uri(baseUri), relativeUrl);
            return client.PutAsync(url, content).ConfigureAwait(false);
        }

        public static ConfiguredTaskAwaitable<HttpResponseMessage> PutAsyncStream(this HttpClient client, string baseUri, string relativeUrl,
            Stream stream)
        {
            stream.Position = 0;
            MemoryStream ms = new MemoryStream();
            stream.CopyTo(ms);

            stream.Dispose();

            ms.Position = 0;
            StreamContent content = new StreamContent(ms);
            content.Headers.ContentType = new MediaTypeHeaderValue(_contentTypeHeader);

            var url = new Uri(new Uri(baseUri), relativeUrl);
            return client.PutAsync(url, content).ConfigureAwait(false);
        }

        public static Task<HttpResponseMessage> PutAsyncString(this HttpClient client, string url,
            string text)
        {
            var stringcontent = new StringContent(text, Encoding.UTF8,
                _contentTypeHeader);

            return client.PutAsync(url, stringcontent);
        }

        public static Task<HttpResponseMessage> PostAsyncString(this HttpClient client, string url,
            string text)
        {
            var stringcontent = new StringContent(text, Encoding.UTF8,
                _contentTypeHeader);

            return client.PostAsync(url, stringcontent);
        }
    }
}