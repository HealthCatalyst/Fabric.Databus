// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SqlJsonValueWriter.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the SqlJsonValueWriter type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Shared
{
    using Fabric.Databus.Interfaces.ElasticSearch;
    using Fabric.Databus.ZipCodeToGeoCode;

    using Newtonsoft.Json;

    /// <inheritdoc />
    public class SqlJsonValueWriter : IJsonValueWriter
    {
        /// <summary>
        /// The geocode type.
        /// </summary>
        private static readonly string GeocodeType = ElasticSearchTypes.geo_point.ToString();

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
        public void WriteValue(JsonTextWriter writer, string elasticSearchType, object value)
        {
            if (elasticSearchType == GeocodeType)
            {
                var geoCode = value as GeoCode;
                if (geoCode != null)
                {
                    writer.WriteStartObject();

                    writer.WritePropertyName("lat");
                    writer.WriteValue(geoCode.lat);

                    writer.WritePropertyName("lon");
                    writer.WriteValue(geoCode.lon);

                    writer.WriteEndObject();
                }
            }
            else
            {
                writer.WriteValue(value);
            }
        }
    }
}