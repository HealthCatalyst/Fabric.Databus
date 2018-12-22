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
    using System.Threading.Tasks;

    using Fabric.Shared.ReliableHttp.Interfaces;

    /// <inheritdoc />
    public class DummyHttpRequestLogger : IHttpRequestLogger
    {
        /// <inheritdoc />
        public Task LogRequestAsync(string requestId, HttpMethod method, HttpRequestMessage request)
        {
            return Task.CompletedTask;
        }
    }
}
