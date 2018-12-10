// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SendAsyncResult.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the SendAsyncResult type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Shared.ReliableHttp
{
    using System;
    using System.Net;
    using System.Net.Http;

    /// <summary>
    /// The send async result.
    /// </summary>
    public class SendAsyncResult
    {
        /// <summary>
        /// Gets or sets the resource id.
        /// </summary>
        public string ResourceId { get; set; }

        /// <summary>
        /// Gets or sets the method.
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// Gets or sets the uri.
        /// </summary>
        public Uri Uri { get; set; }

        /// <summary>
        /// Gets or sets the status code.
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the response content.
        /// </summary>
        public HttpContent ResponseContent { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is success status code.
        /// </summary>
        public bool IsSuccessStatusCode { get; set; }
    }
}