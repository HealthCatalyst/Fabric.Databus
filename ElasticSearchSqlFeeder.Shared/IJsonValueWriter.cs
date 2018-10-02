// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IJsonValueWriter.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the IJsonValueWriter type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Shared
{
    using Newtonsoft.Json;

    /// <summary>
    /// The JsonValueWriter interface.
    /// </summary>
    public interface IJsonValueWriter
    {
        /// <summary>
        /// The write value.
        /// </summary>
        /// <param name="writer">
        /// The writer.
        /// </param>
        /// <param name="elasticSearchType">
        /// The elastic search type.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        void WriteValue(JsonTextWriter writer, string elasticSearchType, object value);
    }
}