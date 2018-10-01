// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IElasticSearchUploaderFactory.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the IElasticSearchUploaderFactory type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ElasticSearchSqlFeeder.Interfaces
{
    using System.Collections.Generic;

    using Serilog;

    /// <summary>
    /// The FileUploaderFactory interface.
    /// </summary>
    public interface IElasticSearchUploaderFactory
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
        IElasticSearchUploader Create(string userName, string password, bool keepIndexOnline, List<string> urls, string index, string alias, string entityType);
    }
}
