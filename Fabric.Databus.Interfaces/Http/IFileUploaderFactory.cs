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
        /// <param name="keepIndexOnline">
        /// The config keep index online.
        /// </param>
        /// <param name="urls">
        /// The config Urls.
        /// </param>
        /// <param name="index">
        /// The config Index.
        /// </param>
        /// <param name="alias">
        /// The config Alias.
        /// </param>
        /// <param name="entityType">
        /// The entity Type.
        /// </param>
        /// <returns>
        /// The <see cref="FileUploader"/>.
        /// </returns>
        IFileUploader Create(string userName, string password, List<string> urls);
    }
}
