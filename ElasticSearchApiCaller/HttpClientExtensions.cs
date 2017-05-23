using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ElasticSearchApiCaller
{
    public static class HttpClientExtensions
    {
        public static Task<HttpResponseMessage> PutAsyncFile(this HttpClient client, string url,
            string filename)
        {
            var allText = File.ReadAllText(filename);
            var stringcontent = new StringContent(allText, Encoding.UTF8,
                "application/x-www-form-urlencoded");

            return client.PutAsync(url, stringcontent);
        }

        public static ConfiguredTaskAwaitable<HttpResponseMessage> PutAsyncFileCompressed(this HttpClient client, string url,
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
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            content.Headers.ContentEncoding.Add("gzip");


            return client.PutAsync(url, content).ConfigureAwait(false); ;
        }
        public static ConfiguredTaskAwaitable<HttpResponseMessage> PutAsyncStreamCompressed(this HttpClient client, string url,
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
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            content.Headers.ContentEncoding.Add("gzip");

            return client.PutAsync(url, content).ConfigureAwait(false); 
        }

        public static Task<HttpResponseMessage> PutAsyncString(this HttpClient client, string url,
            string text)
        {
            var stringcontent = new StringContent(text, Encoding.UTF8,
                "application/x-www-form-urlencoded");

            return client.PutAsync(url, stringcontent);
        }

        public static Task<HttpResponseMessage> PostAsyncString(this HttpClient client, string url,
            string text)
        {
            var stringcontent = new StringContent(text, Encoding.UTF8,
                "application/x-www-form-urlencoded");

            return client.PostAsync(url, stringcontent);
        }
    }
}