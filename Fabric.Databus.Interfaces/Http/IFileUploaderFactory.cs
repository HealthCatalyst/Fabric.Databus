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
    using System.Net.Http;
    using System.Threading;

    /// <summary>
    /// The FileUploaderFactory interface.
    /// </summary>
    public interface IFileUploaderFactory
    {
        /// <summary>
        /// The create.
        /// </summary>
        /// <param name="urls">
        /// The config Urls.
        /// </param>
        /// <param name="method">
        /// The method.
        /// </param>
        /// <param name="cancellationToken"></param>
        /// <returns>
        /// The <see cref="IFileUploader"/>.
        /// </returns>
        IFileUploader Create(List<string> urls, HttpMethod method, CancellationToken cancellationToken);
    }
}
