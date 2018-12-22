// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IHttpRequestLogger.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the IHttpRequestLogger type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Shared.ReliableHttp.Interfaces
{
    using System.Net.Http;
    using System.Threading.Tasks;

    /// <summary>
    /// The HttpRequestLogger interface.
    /// </summary>
    public interface IHttpRequestLogger
    {
        /// <summary>
        /// The intercept request.
        /// </summary>
        /// <param name="method">
        /// The method.
        /// </param>
        /// <param name="request">
        /// The request.
        /// </param>
        /// <param name="requestId">
        /// The requestId.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        Task LogRequestAsync(HttpMethod method, HttpRequestMessage request, string requestId);
    }
}