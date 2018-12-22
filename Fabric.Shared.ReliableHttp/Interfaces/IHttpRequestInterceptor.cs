// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IHttpRequestInterceptor.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the IHttpRequestInterceptor type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Shared.ReliableHttp.Interfaces
{
    using System.Net.Http;
    using System.Threading.Tasks;

    /// <summary>
    /// The HttpRequestInjector interface.
    /// </summary>
    public interface IHttpRequestInterceptor
    {
        /// <summary>
        /// The inject into request.
        /// </summary>
        /// <param name="requestId">
        ///     The request Id.
        /// </param>
        /// <param name="method">
        ///     The method.
        /// </param>
        /// <param name="request">
        ///     The request.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        Task InterceptRequestAsync(string requestId, HttpMethod method, HttpRequestMessage request);
    }
}