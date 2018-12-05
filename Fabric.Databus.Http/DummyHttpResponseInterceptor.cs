// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DummyHttpResponseInterceptor.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the DummyHttpResponseInterceptor type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Http
{
    using System;
    using System.Net;
    using System.Net.Http;

    using Fabric.Databus.Interfaces.Http;

    /// <inheritdoc />
    public class DummyHttpResponseInterceptor : IHttpResponseInterceptor
    {
        /// <inheritdoc />
        public void InterceptResponse(
            Uri fullUri,
            HttpStatusCode responseStatusCode,
            string responseContent,
            long stopwatchElapsedMilliseconds,
            HttpMethod httpMethod)
        {
        }
    }
}
