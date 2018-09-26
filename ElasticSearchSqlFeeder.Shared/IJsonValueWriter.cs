namespace ElasticSearchSqlFeeder.Shared
{
    using Newtonsoft.Json;

    public interface IJsonValueWriter
    {
        void WriteValue(JsonTextWriter writer, string elasticSearchType, object value);
    }
}