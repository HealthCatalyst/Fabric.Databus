// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DummyHttpRequestInterceptor.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the DummyHttpRequestInterceptor type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Http
{
    using System.Net.Http;

    using Fabric.Databus.Interfaces.Http;

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