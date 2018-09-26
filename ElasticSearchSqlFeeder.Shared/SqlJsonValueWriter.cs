namespace ElasticSearchSqlFeeder.Shared
{
    using ElasticSearchSqlFeeder.Interfaces;

    using Fabric.Databus.Config;

    using Newtonsoft.Json;

    using ZipCodeToGeoCodeConverter;

    public class SqlJsonValueWriter : IJsonValueWriter
    {
        private static readonly string GeocodeType = ElasticSearchTypes.geo_point.ToString();

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