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

    /// <summary>
    /// The HttpRequestInjector interface.
    /// </summary>
    public interface IHttpRequestInterceptor
    {
        /// <summary>
        /// The inject into request.
        /// </summary>
        /// <param name="method">
        /// The method.
        /// </param>
        /// <param name="request">
        /// The request.
        /// </param>
        void InterceptRequest(HttpMethod method, HttpRequestMessage request);
    }
}