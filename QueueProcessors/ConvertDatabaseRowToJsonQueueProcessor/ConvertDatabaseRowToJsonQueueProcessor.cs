// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConvertDatabaseRowToJsonQueueProcessor.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the ConvertDatabaseRowToJsonQueueProcessor type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ConvertDatabaseRowToJsonQueueProcessor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;

    using BaseQueueProcessor;

    using ElasticSearchSqlFeeder.Interfaces;
    using ElasticSearchSqlFeeder.Shared;

    using Fabric.Shared;

    using Newtonsoft.Json.Linq;

    using QueueItems;

    public class ConvertDatabaseRowToJsonQueueProcessor : BaseQueueProcessor<ConvertDatabaseToJsonQueueItem, JsonDocumentMergerQueueItem>
    {
        private static readonly SequenceBarrier SequenceBarrier = new SequenceBarrier();

        private readonly string _folder;


        public ConvertDatabaseRowToJsonQueueProcessor(IQueueContext queueContext)
            : base(queueContext)
        {
            this._folder = Path.Combine(Config.LocalSaveFolder, $"{UniqueId}-ConvertToJson");
        }

        private JObject[] GetJsonForRowForMerge(List<ColumnInfo> columns, List<object[]> rows, string propertyName)
        {
            var jObjects = new List<JObject>();

            foreach (var row in rows)
            {
                var jObjectOuter = new JObject();

                var jObject = jObjectOuter;

                if (!string.IsNullOrEmpty(propertyName))
                {
                    var properties = propertyName.Split('.');


                    var jObject1 = jObjectOuter;
                    int level = 1;

                    string currentFullPropertyName = null;

                    foreach (var property in properties)
                    {
                        currentFullPropertyName = currentFullPropertyName != null
                            ? currentFullPropertyName + '.' + property
                            : property;

                        jObject = new JObject();

                        var propertyType = QueueContext.PropertyTypes[currentFullPropertyName];

                        if (propertyType != null && propertyType.Equals("object", StringComparison.OrdinalIgnoreCase))
                        {
                            jObject1[property] = jObject;
                            jObject1 = jObject;
                        }
                        else //it is a nested list
                        {
                            // add a key column so we know what key this object corresponds to
                            var key = "KeyLevel" + level++;
                            var keyValue =
                                row[columns.FirstOrDefault(c => c.Name.Equals(key, StringComparison.OrdinalIgnoreCase))
                                    .Verify($"{key} column not found")
                                    .index];

                            jObject.Add(key, GetJToken(keyValue));
                            var jArray = new JArray { jObject };
                            jObject1[property] = jArray;
                            jObject1 = jObject;
                        }
                    }
                }

                foreach (var col in columns)
                {
                    var value = row[col.index];

                    var shouldWriteColumn = value != null && value != DBNull.Value;

                    //only write if it is not the default
                    if (col.ElasticSearchType.Equals("keyword", StringComparison.OrdinalIgnoreCase)
                        || col.ElasticSearchType.Equals("text", StringComparison.OrdinalIgnoreCase)
                    )
                    {
                        if (value == null
                            || value == DBNull.Value
                            || string.IsNullOrEmpty((string)value)
                            || ((string)value).Equals("NULL", StringComparison.OrdinalIgnoreCase))
                        {
                            shouldWriteColumn = false;
                        }
                    }

                    if (shouldWriteColumn)
                    {
                        JToken tempToken;
                        if (!jObject.TryGetValue(col.Name, out tempToken))
                        {
                            jObject.Add(col.Name, GetJToken(value));
                        }

                        //jsonValueWriter.WriteValue(writer, col.ElasticSearchType, value);
                    }
                }
                jObjects.Add(jObjectOuter);
            }

            return jObjects.ToArray();
        }

        private static JToken GetJToken(object value)
        {
            return value != null ? JToken.FromObject(value) : null;
        }

        private JObject[] GetJsonForRows(List<ColumnInfo> columns, IJsonValueWriter jsonValueWriter, List<object[]> rows, string propertyName)
        {
            var jObjects = new List<JObject>();

            foreach (var row in rows)
            {
                var jObject = new JObject();
                foreach (var col in columns)
                {
                    var value = row[col.index];

                    var shouldWriteColumn = value != null && value != DBNull.Value;

                    //only write if it is not the default
                    if (col.ElasticSearchType.Equals("keyword", StringComparison.OrdinalIgnoreCase)
                        || col.ElasticSearchType.Equals("text", StringComparison.OrdinalIgnoreCase)
                    )
                    {
                        if (value == null
                            || value == DBNull.Value
                            || string.IsNullOrEmpty((string)value)
                            || ((string)value).Equals("NULL", StringComparison.OrdinalIgnoreCase))
                        {
                            shouldWriteColumn = false;
                        }
                    }

                    if (shouldWriteColumn)
                    {
                        jObject.Add(col.Name, value != null ? JToken.FromObject(value) : null);

                        //jsonValueWriter.WriteValue(writer, col.ElasticSearchType, value);
                    }
                }

                jObjects.Add(jObject);
            }

            return jObjects.ToArray();
        }

        private void WriteObjectToJson(ConvertDatabaseToJsonQueueItem wt)
        {
            var id = this.GetId(wt);

            var jsonForRows = this.GetJsonForRowForMerge(wt.Columns, wt.Rows, wt.PropertyName);

            if (Config.WriteDetailedTemporaryFilesToDisk)
            {
                var path = Path.Combine(this._folder, wt.PropertyName ?? "main");

                Directory.CreateDirectory(path);

                var sb = new StringBuilder();

                foreach (var jsonForRow in jsonForRows)
                {
                    sb.AppendLine(jsonForRow.ToString());
                }

                File.AppendAllText(Path.Combine(path, $"{id}.json"), sb.ToString());
            }

            AddToOutputQueue(new JsonDocumentMergerQueueItem
            {
                BatchNumber = wt.BatchNumber,
                QueryId = wt.QueryId,
                Id = id,
                PropertyName = wt.PropertyName,
                //JoinColumnValue = workItem.JoinColumnValue,
                NewJObjects = jsonForRows
            });

            var minimumEntityIdProcessed = SequenceBarrier.UpdateMinimumEntityIdProcessed(wt.QueryId, id);

            MyLogger.Trace($"Add to queue: {id}");

            this.CleanListIfNeeded(wt.QueryId, minimumEntityIdProcessed);
        }

        private void CleanListIfNeeded(string queryId, string minimumEntityIdProcessed)
        {

        }

        protected override void Handle(ConvertDatabaseToJsonQueueItem workItem)
        {
            this.WriteObjectToJson(workItem);

        }

        protected override void Begin(bool isFirstThreadForThisTask)
        {
        }

        protected override void Complete(string queryId, bool isLastThreadForThisTask)
        {
            SequenceBarrier.CompleteQuery(queryId);
        }

        protected override string GetId(ConvertDatabaseToJsonQueueItem workItem)
        {
            return Convert.ToString(workItem.JoinColumnValue);
        }

        protected override string LoggerName => "ConvertDatabaseRow";

    }
}
