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

    using Fabric.Shared.ReliableHttp.Interfaces;

    /// <inheritdoc />
    /// <summary>
    /// The dummy http request injector.
    /// </summary>
    public class DummyHttpRequestInterceptor : IHttpRequestInterceptor
    {
        /// <inheritdoc />
        public void InterceptRequest(HttpMethod method, HttpRequestMessage request)
        {
        }
    }
}