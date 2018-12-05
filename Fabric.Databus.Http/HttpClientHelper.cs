﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HttpClientHelper.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the HttpClientHelper type.
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

    using Fabric.Databus.Interfaces.Http;

    /// <summary>
    /// The http client extensions.
    /// </summary>
    public class HttpClientHelper
    {
        /// <summary>
        /// The content type header.
        /// </summary>
        private const string ContentTypeHeader = "application/json";

        /// <summary>
        /// The http client.
        /// </summary>
        private readonly HttpClient httpClient;

        /// <summary>
        /// The http request injector.
        /// </summary>
        private readonly IHttpRequestInterceptor httpRequestInterceptor;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpClientHelper"/> class.
        /// </summary>
        /// <param name="httpClientFactory">
        /// The http Client Factory.
        /// </param>
        /// <param name="httpRequestInterceptor">
        /// The http Request Injector.
        /// </param>
        public HttpClientHelper(IHttpClientFactory httpClientFactory, IHttpRequestInterceptor httpRequestInterceptor)
        {
            if (httpClientFactory == null)
            {
                throw new ArgumentNullException(nameof(httpClientFactory));
            }
            
            this.httpClient = httpClientFactory.Create();
            this.httpClient.DefaultRequestHeaders.Accept.Clear();

            this.httpRequestInterceptor = httpRequestInterceptor ?? throw new ArgumentNullException(nameof(httpRequestInterceptor));
        }

        /// <summary>
        /// The put async file.
        /// </summary>
        /// <param name="uri">
        /// The uri.
        /// </param>
        /// <param name="filename">
        /// The filename.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task<HttpResponseMessage> PutAsyncFile(Uri uri, string filename)
        {
            var allText = File.ReadAllText(filename);
            var stringContent = new StringContent(allText, Encoding.UTF8, ContentTypeHeader);

            return await this.PutAsync(uri, stringContent);
        }

        /// <summary>
        /// The put async file compressed.
        /// </summary>
        /// <param name="uri">
        /// The uri.
        /// </param>
        /// <param name="filename">
        /// The filename.
        /// </param>
        /// <returns>
        /// The <see cref="ConfiguredTaskAwaitable"/>.
        /// </returns>
        public async Task<HttpResponseMessage> PutAsyncFileCompressed(
            Uri uri,
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

            return await this.PutAsync(uri, content).ConfigureAwait(false);
        }

        /// <summary>
        /// The put async stream compressed.
        /// </summary>
        /// <param name="url">
        ///     the url
        /// </param>
        /// <param name="httpMethod">http method</param>
        /// <param name="stream">
        ///     The stream.
        /// </param>
        /// <returns>
        /// The <see cref="ConfiguredTaskAwaitable"/>.
        /// </returns>
        public async Task<HttpResponseMessage> SendAsyncStreamCompressed(Uri url, HttpMethod httpMethod, Stream stream)
        {
            stream.Position = 0;
            var ms = new MemoryStream();
            using (var gzip = new GZipStream(ms, CompressionMode.Compress, true))
            {
                stream.CopyTo(gzip);
            }

            stream.Dispose();

            ms.Position = 0;
            var content = new StreamContent(ms);
            content.Headers.ContentType = new MediaTypeHeaderValue(ContentTypeHeader);
            content.Headers.ContentEncoding.Add("gzip");

            return await this.SendAsync(url, httpMethod, content).ConfigureAwait(false);
        }

        /// <summary>
        /// The put async stream.
        /// </summary>
        /// <param name="url">
        ///     The url.
        /// </param>
        /// <param name="httpMethod">
        ///     http method</param>
        /// <param name="stream">
        ///     The stream.
        /// </param>
        /// <returns>
        /// The <see cref="ConfiguredTaskAwaitable"/>.
        /// </returns>
        public async Task<HttpResponseMessage> SendAsyncStream(Uri url, HttpMethod httpMethod, Stream stream)
        {
            stream.Position = 0;
            MemoryStream ms = new MemoryStream();
            stream.CopyTo(ms);

            stream.Dispose();

            ms.Position = 0;
            StreamContent content = new StreamContent(ms);
            content.Headers.ContentType = new MediaTypeHeaderValue(ContentTypeHeader);

            return await this.SendAsync(url, httpMethod, content).ConfigureAwait(false);
        }

        /// <summary>
        /// The put async string.
        /// </summary>
        /// <param name="url">
        /// The url.
        /// </param>
        /// <param name="text">
        /// The text.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task<HttpResponseMessage> PutAsyncString(Uri url, string text)
        {
            var stringContent = new StringContent(text, Encoding.UTF8, ContentTypeHeader);

            return await this.PutAsync(url, stringContent);
        }

        /// <summary>
        /// The post async string.
        /// </summary>
        /// <param name="url">
        /// The url.
        /// </param>
        /// <param name="text">
        /// The text.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task<HttpResponseMessage> PostAsyncString(Uri url, string text)
        {
            var stringContent = new StringContent(text, Encoding.UTF8, ContentTypeHeader);

            return await this.PostAsync(url, stringContent);
        }

        /// <summary>
        /// The post async.
        /// </summary>
        /// <param name="url">
        /// The url.
        /// </param>
        /// <param name="stringContent">
        /// The string content.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task<HttpResponseMessage> PostAsync(Uri url, HttpContent stringContent)
        {
            return await this.SendAsync(url, HttpMethod.Post, stringContent);
        }

        /// <summary>
        /// The delete async.
        /// </summary>
        /// <param name="requestUri">
        /// The request uri.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task<HttpResponseMessage> DeleteAsync(Uri requestUri)
        {
            var httpMethod = HttpMethod.Delete;
            using (var request = new HttpRequestMessage(httpMethod, requestUri))
            {
                this.httpRequestInterceptor.InterceptRequest(httpMethod, request);

                return await this.httpClient.SendAsync(request);
            }
        }

        /// <summary>
        /// The get string async.
        /// </summary>
        /// <param name="host">
        /// The host.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task<string> GetStringAsync(string host)
        {
            var httpMethod = HttpMethod.Get;
            using (var request = new HttpRequestMessage(httpMethod, host))
            {
                this.httpRequestInterceptor.InterceptRequest(httpMethod, request);

                var result = await this.httpClient.SendAsync(request);

                return await result.Content.ReadAsStringAsync();
            }
        }

        /// <summary>
        /// The put async.
        /// </summary>
        /// <param name="url">
        /// The url.
        /// </param>
        /// <param name="content">
        /// The content.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task<HttpResponseMessage> PutAsync(Uri url, HttpContent content)
        {
            return await this.SendAsync(url, HttpMethod.Put, content);
        }

        /// <summary>
        /// The send async.
        /// </summary>
        /// <param name="uri">
        /// The uri.
        /// </param>
        /// <param name="httpMethod">
        /// The http Method.
        /// </param>
        /// <param name="stringContent">
        /// The string content.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task<HttpResponseMessage> SendAsync(Uri uri, HttpMethod httpMethod, HttpContent stringContent)
        {
            using (var request = new HttpRequestMessage(httpMethod, uri))
            {
                request.Content = stringContent;

                this.httpRequestInterceptor.InterceptRequest(httpMethod, request);

                return await this.httpClient.SendAsync(request);
            }
        }
    }
}