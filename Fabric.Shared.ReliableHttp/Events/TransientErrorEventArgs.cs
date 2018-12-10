// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TransientErrorEventArgs.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the TransientErrorEventArgs type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Shared.ReliableHttp.Events
{
    using System;

    /// <inheritdoc />
    /// <summary>
    /// The transient error event args.
    /// </summary>
    public class TransientErrorEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TransientErrorEventArgs"/> class.
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
        /// <param name="retryCount">
        /// The retry count.
        /// </param>
        /// <param name="maxRetryCount">
        /// The max retry count.
        /// </param>
        public TransientErrorEventArgs(
            string resourceId,
            string method,
            Uri fullUri,
            string statusCode,
            string response,
            int retryCount,
            int maxRetryCount)
        {
            this.Method = method;
            this.FullUri = fullUri;
            this.StatusCode = statusCode;
            this.Response = response;
            this.RetryCount = retryCount;
            this.MaxRetryCount = maxRetryCount;
            this.ResourceId = resourceId;
        }

        /// <summary>
        /// Gets the method.
        /// </summary>
        public string Method { get; }

        /// <summary>
        /// Gets the full uri.
        /// </summary>
        public Uri FullUri { get; }

        /// <summary>
        /// Gets the status code.
        /// </summary>
        public string StatusCode { get; }

        /// <summary>
        /// Gets the response.
        /// </summary>
        public string Response { get; }

        /// <summary>
        /// Gets the retry count.
        /// </summary>
        public int RetryCount { get; }

        /// <summary>
        /// Gets the max retry count.
        /// </summary>
        public int MaxRetryCount { get; }

        /// <summary>
        /// Gets the resource id.
        /// </summary>
        public string ResourceId { get; }
    }
}