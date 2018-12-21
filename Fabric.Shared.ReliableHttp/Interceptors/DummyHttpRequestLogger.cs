// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DummyHttpRequestLogger.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the DummyHttpRequestLogger type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Shared.ReliableHttp.Interceptors
{
    using System.Net.Http;
    using Fabric.Shared.ReliableHttp.Interfaces;

    /// <inheritdoc />
    public class DummyHttpRequestLogger : IHttpRequestLogger
    {
        /// <inheritdoc />
        public void InterceptRequest(HttpMethod method, HttpRequestMessage request)
        {
        }
    }
}
