// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISchemaLoader.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   The SchemaLoader interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Interfaces.Sql
{
    using System.Collections.Generic;

    using Fabric.Databus.Interfaces.Config;

    /// <summary>
    /// The SchemaLoader interface.
    /// </summary>
    public interface ISchemaLoader
    {
        /// <summary>
        /// The get schemas for loads.
        /// </summary>
        /// <param name="workitemLoads">
        /// The workitem loads.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        IList<MappingItem> GetSchemasForLoads(
            IList<IDataSource> workitemLoads);
    }
}