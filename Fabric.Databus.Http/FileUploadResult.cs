// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileUploadResult.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the FileUploadResult type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Http
{
    using System;
    using System.Net;
    using System.Net.Http;

    using Fabric.Databus.Interfaces.Http;

    /// <inheritdoc />
    /// <summary>
    /// The file upload result.
    /// </summary>
    public class FileUploadResult : IFileUploadResult
    {
        /// <inheritdoc />
        public Uri Uri { get; set; }

        /// <inheritdoc />
        public HttpMethod HttpMethod { get; set; }

        /// <inheritdoc />
        public HttpStatusCode StatusCode { get; set; }

        /// <inheritdoc />
        public string ResponseContent { get; set; }

        /// <inheritdoc />
        public string RequestContent { get; set; }
    }
}