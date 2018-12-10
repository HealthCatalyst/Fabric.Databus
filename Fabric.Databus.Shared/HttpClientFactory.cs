// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HttpClientFactory.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the HttpClientFactory type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Shared
{
    using System;
    using System.Net.Http;

    using Fabric.Shared.ReliableHttp.Interfaces;

    /// <inheritdoc />
    /// <summary>
    /// The http client factory.
    /// </summary>
    public class HttpClientFactory : IHttpClientFactory
    {
        /// <summary>
        /// The http timeout.
        /// </summary>
        private static readonly TimeSpan HttpTimeout = TimeSpan.FromMinutes(5);        
        
        /// <summary>
        /// The _http client.
        /// make HttpClient static per https://aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/
        /// </summary>
        private static HttpClient httpClient;

        /// <inheritdoc />
        public HttpClient Create()
        {
            return httpClient ?? (httpClient = CreateHttpClient(new HttpClientHandler()));
        }

        /// <summary>
        /// The create http client.
        /// </summary>
        /// <param name="httpClientHandler">
        /// The http client handler.
        /// </param>
        /// <returns>
        /// The <see cref="HttpClient"/>.
        /// </returns>
        private static HttpClient CreateHttpClient(HttpMessageHandler httpClientHandler)
        {
            var client = new HttpClient(httpClientHandler, false);
            client.DefaultRequestHeaders.Connection.Clear();
            client.DefaultRequestHeaders.ConnectionClose = true;
            client.Timeout = HttpTimeout;
            return client;
        }
    }
}
