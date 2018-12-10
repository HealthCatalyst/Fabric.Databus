// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NavigatedEventArgs.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the NavigatedEventArgs type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Shared.ReliableHttp.Events
{
    using System;
    using System.Net.Http;

    /// <summary>
    /// The navigated event args.
    /// </summary>
    public class NavigatedEventArgs : EventArgs
    {
        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Fabric.Shared.ReliableHttp.Events.NavigatedEventArgs" /> class.
        /// </summary>
        /// <param name="resourceId">
        /// The resource id.
        /// </param>
        /// <param name="method">
        /// The method.
        /// </param>
        /// <param name="fullUri">
        /// The full uri.
        /// </param>
        /// <param name="statusCode">
        /// The status code.
        /// </param>
        /// <param name="response">
        /// The response.
        /// </param>
        public NavigatedEventArgs(string resourceId, string method, Uri fullUri, string statusCode, HttpContent response)
        {
            this.Method = method;
            this.FullUri = fullUri;
            this.StatusCode = statusCode;
            this.Response = response;
            this.ResourceId = resourceId;
        }

        /// <summary>
        /// Gets or sets the method.
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// Gets or sets the status code.
        /// </summary>
        public string StatusCode { get; set; }

        /// <summary>
        /// Gets the response.
        /// </summary>
        public HttpContent Response { get; }

        /// <summary>
        /// Gets the resource id.
        /// </summary>
        public string ResourceId { get; }

        /// <summary>
        /// Gets or sets the full uri.
        /// </summary>
        public Uri FullUri { get; set; }
    }
}