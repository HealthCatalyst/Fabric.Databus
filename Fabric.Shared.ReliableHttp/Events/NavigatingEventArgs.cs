// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NavigatingEventArgs.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the NavigatingEventArgs type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Shared.ReliableHttp.Events
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// The navigating event args.
    /// </summary>
    public class NavigatingEventArgs : CancelEventArgs
    {
        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Fabric.Shared.ReliableHttp.Events.NavigatingEventArgs" /> class.
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
        public NavigatingEventArgs(string resourceId, string method, Uri fullUri)
        {
            this.FullUri = fullUri;
            this.Method = method;
            this.ResourceId = resourceId;
        }

        /// <summary>
        /// Gets or sets the method.
        /// </summary>
        public string Method { get; set; }

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