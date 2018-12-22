// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EntityJsonWriter.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the EntityJsonWriter type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Json
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Fabric.Databus.Interfaces;
    using Fabric.Databus.Interfaces.Sql;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <inheritdoc />
    public class EntityJsonWriter : IEntityJsonWriter
    {
        /// <summary>
        /// The UTF8 encoding without BOM.
        /// </summary>
        private readonly UTF8Encoding utf8EncodingWithoutBOM = new UTF8Encoding(false, true);

        /// <inheritdoc />
        public async Task WriteMappingToStreamAsync(List<ColumnInfo> columnList, string propertyPath, StreamWriter textWriter, string propertyType, string entity)
        {
            using (var writer = new JsonTextWriter(textWriter))
            {
                await writer.WriteStartObjectAsync(); // begin

                using (new JsonPropertyWrapper(writer, "mappings", propertyPath != null))
                {
                    using (new JsonPropertyWrapper(writer, entity, propertyPath != null))
                    {
                        using (new JsonPropertyWrapper(writer, "properties"))
                        {
                            // see if propertyPath was specified
                            if (!string.IsNullOrEmpty(propertyPath))
                            {
                                var queue = new Queue<string>();
                                foreach (var property in propertyPath.Split('.'))
                                {
                                    queue.Enqueue(property);
                                }

                                await this.WriteNestedPropertyAsync(columnList, queue, writer, propertyType);
                            }
                            else
                            {
                                await this.InternalWriteColumnsToJson(columnList, writer);
                            }
                        }
                    }
                }

                await writer.WriteEndObjectAsync(); // end
            }
        }

        /// <inheritdoc />
        public async Task WriteToStreamAsync(JToken document, Stream stream)
        {
            using (var textWriter = new StreamWriter(stream, this.utf8EncodingWithoutBOM, 1024, true))
            using (var writer = new JsonTextWriter(textWriter))
            {
                await document.WriteToAsync(writer);
            }
        }

        /// <summary>
        /// The write nested property.
        /// </summary>
        /// <param name="columnList">
        /// The column list.
        /// </param>
        /// <param name="properties">
        /// The properties.
        /// </param>
        /// <param name="writer">
        /// The writer.
        /// </param>
        /// <param name="propertyType">
        /// The property type.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task WriteNestedPropertyAsync(List<ColumnInfo> columnList, Queue<string> properties, JsonTextWriter writer, string propertyType)
        {
            if (properties.Any())
            {
                var propertyName = properties.Dequeue();

                using (new JsonPropertyWrapper(writer, propertyName))
                {
                    await writer.WritePropertyNameAsync("type");
                    var type = propertyType != null && propertyType.Equals("object", StringComparison.OrdinalIgnoreCase)
                        ? "object"
                        : "nested";

                    await writer.WriteValueAsync(type);
                    using (new JsonPropertyWrapper(writer, "properties"))
                    {
                        await this.WriteNestedPropertyAsync(columnList, properties, writer, propertyType);
                    }
                }
            }
            else
            {
                await this.InternalWriteColumnsToJson(columnList, writer);
            }
        }

        /// <summary>
        /// The internal write columns to JSON.
        /// </summary>
        /// <param name="columnList">
        /// The column list.
        /// </param>
        /// <param name="writer">
        /// The writer.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task InternalWriteColumnsToJson(List<ColumnInfo> columnList, JsonTextWriter writer)
        {
            foreach (var column in columnList)
            {
                using (new JsonPropertyWrapper(writer, column.Name))
                {
                    await writer.WritePropertyNameAsync("type");
                    await writer.WriteValueAsync(column.ElasticSearchType);
                }
            }
        }
    }
}
