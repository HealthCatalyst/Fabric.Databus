// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IHttpResponseInterceptor.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the IHttpResponseInterceptor type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Interfaces.Http
{
    using System;
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
        /// <param name="fullUri">
        /// The full uri.
        /// </param>
        /// <param name="responseStatusCode">
        /// The response status code.
        /// </param>
        /// <param name="responseContent">
        /// The response content.
        /// </param>
        /// <param name="stopwatchElapsedMilliseconds">
        /// The stopwatch elapsed milliseconds.
        /// </param>
        /// <param name="httpMethod">
        /// The http Method.
        /// </param>
        void InterceptResponse(Uri fullUri, HttpStatusCode responseStatusCode, string responseContent, long stopwatchElapsedMilliseconds, HttpMethod httpMethod);
    }
}
