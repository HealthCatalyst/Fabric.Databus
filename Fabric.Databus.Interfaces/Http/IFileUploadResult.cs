// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IFileUploadResult.cs" company="">
//   
// </copyright>
// <summary>
//   The FileUploadResult interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Interfaces.Http
{
    using System;
    using System.Net;
    using System.Net.Http;

    /// <summary>
    /// The FileUploadResult interface.
    /// </summary>
    public interface IFileUploadResult
    {
        /// <summary>
        /// Gets or sets the uri.
        /// </summary>
        Uri Uri { get; set; }

        /// <summary>
        /// Gets or sets the http method.
        /// </summary>
        HttpMethod HttpMethod { get; set; }

        /// <summary>
        /// Gets or sets the status code.
        /// </summary>
        HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the response content.
        /// </summary>
        string ResponseContent { get; set; }

        /// <summary>
        /// Gets or sets the request content.
        /// </summary>
        string RequestContent { get; set; }
    }
}