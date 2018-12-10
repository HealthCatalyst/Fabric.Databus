// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IHttpResponseInterceptor.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the IHttpResponseInterceptor type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Shared.ReliableHttp.Interfaces
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;

    /// <summary>
    /// The HttpResponseInterceptor interface.
    /// </summary>
    public interface IHttpResponseInterceptor
    {
        /// <summary>
        /// The intercept.
        /// </summary>
        /// <param name="httpMethod">
        ///     The http Method.
        /// </param>
        /// <param name="fullUri">
        ///     The full uri.
        /// </param>
        /// <param name="requestContent">
        ///     request content
        /// </param>
        /// <param name="responseStatusCode">
        ///     The response status code.
        /// </param>
        /// <param name="responseContent">
        ///     The response content.
        /// </param>
        /// <param name="stopwatchElapsedMilliseconds">
        ///     The stopwatch elapsed milliseconds.
        /// </param>
        void InterceptResponse(
            HttpMethod httpMethod,
            Uri fullUri,
            Stream requestContent,
            HttpStatusCode responseStatusCode,
            HttpContent responseContent,
            long stopwatchElapsedMilliseconds);
    }
}
