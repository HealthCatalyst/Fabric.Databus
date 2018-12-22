// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DummyHttpRequestInterceptor.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the DummyHttpRequestInterceptor type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Shared.ReliableHttp.Interceptors
{
    using System.Net.Http;
    using System.Threading.Tasks;

    using Fabric.Shared.ReliableHttp.Interfaces;

    /// <inheritdoc />
    /// <summary>
    /// The dummy http request injector.
    /// </summary>
    public class DummyHttpRequestInterceptor : IHttpRequestInterceptor
    {
        /// <inheritdoc />
        public Task InterceptRequestAsync(string requestId, HttpMethod method, HttpRequestMessage request)
        {
            return Task.CompletedTask;
        }
    }
}