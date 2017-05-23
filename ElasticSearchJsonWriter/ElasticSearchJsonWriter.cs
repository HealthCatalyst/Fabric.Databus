using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ElasticSearchSqlFeeder.Interfaces;

namespace ElasticSearchJsonWriter
{
    public class EsJsonWriter
    {
        //const string folder = @"c:\Catalyst\demodata\patientjson";
        public const int Batchsize = 1000;

        private readonly ConcurrentDictionary<string, string> _idToFileNameLookup = new ConcurrentDictionary<string, string>();


        public static void WriteMappingToStream(List<ColumnInfo> columnList, string propertyPath, StreamWriter textWriter, string propertyType)
        {
            using (var writer = new JsonTextWriter(textWriter))
            {
                writer.WriteStartObject(); // begin

                using (new JsonPropertyWrapper(writer, "mappings", propertyPath != null))
                {
                    using (new JsonPropertyWrapper(writer, "patient", propertyPath != null))
                    {
                        using (new JsonPropertyWrapper(writer, "properties"))
                        {
                            // see if propertyPath was specified
                            if (!String.IsNullOrEmpty(propertyPath))
                            {
                                var queue = new Queue<string>();
                                foreach (var property in propertyPath.Split('.'))
                                {
                                    queue.Enqueue(property);
                                }

                                WriteNestedProperty(columnList, queue, writer, propertyType);
                            }
                            else
                            {
                                InternalWriteColumnsToJson(columnList, writer);
                            }
                        }
                    }
                }

                writer.WriteEndObject(); // end
            }
        }

        private static void WriteNestedProperty(List<ColumnInfo> columnList, Queue<string> properties, JsonTextWriter writer, string propertyType)
        {
            if (properties.Any())
            {
                var propertyName = properties.Dequeue();

                using (new JsonPropertyWrapper(writer, propertyName))
                {
                    writer.WritePropertyName("type");
                    var type = propertyType != null && propertyType.Equals("object", StringComparison.OrdinalIgnoreCase)
                        ? "object"
                        : "nested";

                    writer.WriteValue(type);
                    using (new JsonPropertyWrapper(writer, "properties"))
                    {
                        WriteNestedProperty(columnList, properties, writer, propertyType);
                    }
                }
            }
            else
            {
                InternalWriteColumnsToJson(columnList, writer);
            }
        }

        private static void InternalWriteColumnsToJson(List<ColumnInfo> columnList, JsonTextWriter writer)
        {
            foreach (var column in columnList)
            {
                using (new JsonPropertyWrapper(writer, column.Name))
                {
                    writer.WritePropertyName("type");
                    writer.WriteValue(column.ElasticSearchType);
                }
            }
        }


    }

    public class JsonPropertyWrapper : IDisposable
    {
        private readonly JsonTextWriter _writer;
        private readonly bool _skip;

        public JsonPropertyWrapper(JsonTextWriter writer, string propertyname, bool skip = false)
        {
            _writer = writer;
            _skip = skip;

            if (!_skip)
            {
                writer.WritePropertyName(propertyname);

                writer.WriteStartObject();
            }


        }

        public void Dispose()
        {
            if (!_skip)
            {
                _writer.WriteEndObject();
            }
        }
    }

    public interface IJsonValueWriter
    {
        void WriteValue(JsonTextWriter writer, string elasticSearchType, object value);
    }
}
