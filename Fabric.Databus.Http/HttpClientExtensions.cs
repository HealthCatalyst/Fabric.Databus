// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HttpClientExtensions.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the HttpClientExtensions type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

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

    /// <summary>
    /// The http client extensions.
    /// </summary>
    public static class HttpClientExtensions
    {
        /// <summary>
        /// The content type header.
        /// </summary>
        private const string ContentTypeHeader = "application/json";

        /// <summary>
        /// The put async file.
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <param name="url">
        /// The url.
        /// </param>
        /// <param name="filename">
        /// The filename.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public static async Task<HttpResponseMessage> PutAsyncFile(this HttpClient client, string url, string filename)
        {
            var allText = File.ReadAllText(filename);
            var stringContent = new StringContent(allText, Encoding.UTF8, ContentTypeHeader);

            return await client.PutAsync(url, stringContent);
        }

        /// <summary>
        /// The put async file compressed.
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <param name="baseUri">
        /// The base uri.
        /// </param>
        /// <param name="relativeUrl">
        /// The relative url.
        /// </param>
        /// <param name="filename">
        /// The filename.
        /// </param>
        /// <returns>
        /// The <see cref="ConfiguredTaskAwaitable"/>.
        /// </returns>
        public static async Task<HttpResponseMessage> PutAsyncFileCompressed(
            this HttpClient client,
            string baseUri,
            string relativeUrl,
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
            content.Headers.ContentType = new MediaTypeHeaderValue(ContentTypeHeader);
            content.Headers.ContentEncoding.Add("gzip");

            var url = new Uri(new Uri(baseUri), relativeUrl);
            return await client.PutAsync(url, content).ConfigureAwait(false);
        }

        /// <summary>
        /// The put async stream compressed.
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <param name="baseUri">
        /// The base uri.
        /// </param>
        /// <param name="relativeUrl">
        /// The relative url.
        /// </param>
        /// <param name="stream">
        /// The stream.
        /// </param>
        /// <returns>
        /// The <see cref="ConfiguredTaskAwaitable"/>.
        /// </returns>
        public static async Task<HttpResponseMessage> PutAsyncStreamCompressed(
            this HttpClient client,
            string baseUri,
            string relativeUrl,
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
            content.Headers.ContentType = new MediaTypeHeaderValue(ContentTypeHeader);
            content.Headers.ContentEncoding.Add("gzip");

            var url = new Uri(new Uri(baseUri), relativeUrl);
            return await client.PutAsync(url, content).ConfigureAwait(false);
        }

        /// <summary>
        /// The put async stream.
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <param name="baseUri">
        /// The base uri.
        /// </param>
        /// <param name="relativeUrl">
        /// The relative url.
        /// </param>
        /// <param name="stream">
        /// The stream.
        /// </param>
        /// <returns>
        /// The <see cref="ConfiguredTaskAwaitable"/>.
        /// </returns>
        public static async Task<HttpResponseMessage> PutAsyncStream(
            this HttpClient client,
            string baseUri,
            string relativeUrl,
            Stream stream)
        {
            stream.Position = 0;
            MemoryStream ms = new MemoryStream();
            stream.CopyTo(ms);

            stream.Dispose();

            ms.Position = 0;
            StreamContent content = new StreamContent(ms);
            content.Headers.ContentType = new MediaTypeHeaderValue(ContentTypeHeader);

            var url = new Uri(new Uri(baseUri), relativeUrl);
            return await client.PutAsync(url, content).ConfigureAwait(false);
        }

        /// <summary>
        /// The put async string.
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <param name="url">
        /// The url.
        /// </param>
        /// <param name="text">
        /// The text.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public static async Task<HttpResponseMessage> PutAsyncString(this HttpClient client, string url, string text)
        {
            var stringContent = new StringContent(text, Encoding.UTF8, ContentTypeHeader);

            return await client.PutAsync(url, stringContent);
        }

        /// <summary>
        /// The post async string.
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <param name="url">
        /// The url.
        /// </param>
        /// <param name="text">
        /// The text.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public static async Task<HttpResponseMessage> PostAsyncString(this HttpClient client, string url, string text)
        {
            var stringContent = new StringContent(text, Encoding.UTF8, ContentTypeHeader);

            return await client.PostAsync(url, stringContent);
        }
    }
}