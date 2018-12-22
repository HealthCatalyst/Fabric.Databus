// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IEntityJsonWriter.cs" company="">
//   
// </copyright>
// <summary>
//   The EntityJsonWriter interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Interfaces
{
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    using Fabric.Databus.Interfaces.Sql;

    using Newtonsoft.Json.Linq;

    /// <summary>
    /// The EntityJsonWriter interface.
    /// </summary>
    public interface IEntityJsonWriter
    {
        /// <summary>
        /// The write to stream async.
        /// </summary>
        /// <param name="document">
        /// The document.
        /// </param>
        /// <param name="stream">
        /// The stream.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        Task WriteToStreamAsync(JToken document, Stream stream);

        /// <summary>
        /// The write mapping to stream.
        /// </summary>
        /// <param name="columnList">
        /// The column list.
        /// </param>
        /// <param name="propertyPath">
        /// The property path.
        /// </param>
        /// <param name="textWriter">
        /// The text writer.
        /// </param>
        /// <param name="propertyType">
        /// The property type.
        /// </param>
        /// <param name="entity">
        /// The entity.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        Task WriteMappingToStreamAsync(List<ColumnInfo> columnList, string propertyPath, StreamWriter textWriter, string propertyType, string entity);
    }
}