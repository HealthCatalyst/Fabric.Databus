// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DummyHttpResponseInterceptor.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the DummyHttpResponseInterceptor type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Shared.ReliableHttp.Interceptors
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Fabric.Shared.ReliableHttp.Interfaces;

    /// <inheritdoc />
    public class DummyHttpResponseInterceptor : IHttpResponseInterceptor
    {
        /// <inheritdoc />
        public Task InterceptResponseAsync(
            string requestId,
            HttpMethod httpMethod,
            Uri fullUri,
            Stream requestContent,
            HttpStatusCode responseStatusCode,
            HttpContent responseContent,
            long stopwatchElapsedMilliseconds)
        {
            return Task.CompletedTask;
        }
    }
}
