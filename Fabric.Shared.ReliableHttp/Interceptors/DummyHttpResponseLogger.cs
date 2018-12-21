﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DummyHttpResponseLogger.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the DummyHttpResponseLogger type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Shared.ReliableHttp.Interceptors
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;

    using Fabric.Shared.ReliableHttp.Interfaces;

    /// <inheritdoc />
    public class DummyHttpResponseLogger : IHttpResponseLogger
    {
        /// <inheritdoc />
        public void InterceptResponse(
            HttpMethod httpMethod,
            Uri fullUri,
            Stream requestContent,
            HttpStatusCode responseStatusCode,
            HttpContent responseContent,
            long stopwatchElapsedMilliseconds)
        {
        }
    }
}