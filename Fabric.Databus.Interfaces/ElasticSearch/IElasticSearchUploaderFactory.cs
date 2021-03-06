﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IElasticSearchUploaderFactory.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the IElasticSearchUploaderFactory type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Interfaces.ElasticSearch
{
    using System.Collections.Generic;
    using System.Net.Http;

    /// <summary>
    /// The FileUploaderFactory interface.
    /// </summary>
    public interface IElasticSearchUploaderFactory
    {
        /// <summary>
        /// The create.
        /// </summary>
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
        /// <param name="method">
        /// The method.
        /// </param>
        /// <returns>
        /// The <see cref="IElasticSearchUploader"/>.
        /// </returns>
        IElasticSearchUploader Create(bool keepIndexOnline, List<string> urls, string index, string alias, string entityType, HttpMethod method);
    }
}
