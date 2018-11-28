// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDataSource.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the IDataSource type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Interfaces.Config
{
    using System.Collections.Generic;

    using Fabric.Databus.Interfaces.ElasticSearch;

    /// <summary>
    /// The DataSource interface.
    /// </summary>
    public interface IDataSource
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the sql.
        /// </summary>
        string Sql { get; set; }

        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        string Path { get; set; }

        /// <summary>
        /// Gets or sets the property type.
        /// </summary>
        string PropertyType { get; set; }

        /// <summary>
        /// Gets or sets the fields.
        /// </summary>
        List<IQueryField> Fields { get; set; }

        /// <summary>
        /// Gets or sets the sequence number.
        /// </summary>
        int SequenceNumber { get; set; }

        /// <summary>
        /// Gets or sets the key levels.
        /// </summary>
        IList<string> KeyLevels { get; set; }
    }
}