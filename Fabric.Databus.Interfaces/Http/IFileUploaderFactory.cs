// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IElasticSearchUploaderFactory.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the IElasticSearchUploaderFactory type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Interfaces.Http
{
    using System.Collections.Generic;

    using Fabric.Databus.Http;

    /// <summary>
    /// The FileUploaderFactory interface.
    /// </summary>
    public interface IFileUploaderFactory
    {
        /// <summary>
        /// The create.
        /// </summary>
        /// <param name="userName">
        /// The config elastic search user name.
        /// </param>
        /// <param name="password">
        /// The config elastic search password.
        /// </param>
        /// <param name="urls">
        /// The config Urls.
        /// </param>
        /// <param name="httpRequestInterceptor">
        /// The http Request Injector.
        /// </param>
        /// <returns>
        /// The <see cref="FileUploader"/>.
        /// </returns>
        IFileUploader Create(string userName, string password, List<string> urls, IHttpRequestInterceptor httpRequestInterceptor);
    }
}
