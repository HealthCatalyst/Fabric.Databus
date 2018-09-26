namespace ElasticSearchJsonWriter
{
    using System;

    using Newtonsoft.Json;

    public class JsonPropertyWrapper : IDisposable
    {
        private readonly JsonTextWriter _writer;
        private readonly bool _skip;

        public JsonPropertyWrapper(JsonTextWriter writer, string propertyname, bool skip = false)
        {
            this._writer = writer;
            this._skip = skip;

            if (!this._skip)
            {
                writer.WritePropertyName(propertyname);

                writer.WriteStartObject();
            }


        }

        public void Dispose()
        {
            if (!this._skip)
            {
                this._writer.WriteEndObject();
            }
        }
    }
}