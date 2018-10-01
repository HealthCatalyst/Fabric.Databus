// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IQueryConfig.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the IQueryConfig type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ElasticSearchSqlFeeder.Interfaces
{
    using System.Collections.Generic;

    /// <summary>
    /// The QueryConfig interface.
    /// </summary>
    public interface IQueryConfig : IJobConfig
    {
        string Url { get; set; }

        string ElasticSearchUserName { get; set; }

        string ElasticSearchPassword { get; set; }

        List<string> Urls { get; }

        string ConnectionString { get; set; }

        bool DropAndReloadIndex { get; set; }

        string Index { get; set; }

        string Alias { get; set; }

        bool UploadToElasticSearch { get; set; }

        bool KeepIndexOnline { get; set; }
    }
}