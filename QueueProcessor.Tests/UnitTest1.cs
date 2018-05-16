using System;
using System.Collections.Generic;
using System.Linq;
using ElasticSearchSqlFeeder.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace QueueProcessor.Tests
{
    [TestClass]
    public class NewMergerTester
    {
        [TestMethod]
        public void TestNewMerger()
        {
            var patientEntities = new List<Entity> {
                new Entity()
                {
                    Columns = new List<ColumnInfo>
                    {
                        new ColumnInfo
                        {
                            Name = "PatientID",
                            SqlColumnType = "Int32",
                            ElasticSearchType = "int"
                        },
                        new ColumnInfo
                        {
                            Name = "Name",
                            SqlColumnType = "String",
                            ElasticSearchType = "keyword"
                        }

                    },
                    Rows = new List<EntityRow>
                    {
                        new EntityRow
                        {
                            Data = new object []
                            {
                                1,
                                "imran"
                            },
                        },
                        new EntityRow
                        {
                            Data = new object []
                            {
                                2,
                                "elsie"
                            }
                        }
                    }
                }
            };


            var facilityEntities = new List<Entity> {
                new Entity()
                {
                    PropertyName = "Facility",
                    Columns = new List<ColumnInfo>
                    {
                        new ColumnInfo
                        {
                            Name = "PatientID",
                            SqlColumnType = "Int32",
                            ElasticSearchType = "int"
                        },
                        new ColumnInfo
                        {
                            Name = "FacilityAccountID",
                            SqlColumnType = "String",
                            ElasticSearchType = "keyword"
                        }

                    },
                    Rows = new List<EntityRow>
                    {
                        new EntityRow
                        {
                            Data = new object []
                            {
                                1,
                                "12345"
                            },
                        },
                        new EntityRow
                        {
                            Data = new object []
                            {
                                2,
                                "ABCDEF"
                            }
                        },
                        new EntityRow
                        {
                            Data = new object []
                            {
                                2,
                                "XYTUSFF"
                            }
                        }

                    }
                }
            };


            List<Entity> result = new MyMerger().Merge(patientEntities, facilityEntities);

            Assert.AreEqual(1, result[0].Rows[0].Children.Count);
            Assert.AreEqual(1, result[0].Rows[0].Children.First().Value.Count);
            Assert.AreEqual("12345", result[0].Rows[0].Children.First().Value.First().Data[1]);
            Assert.AreEqual(1, result[0].Rows[1].Children.Count);
            Assert.AreEqual(2, result[0].Rows[1].Children.First().Value.Count);
            Assert.AreEqual("ABCDEF", result[0].Rows[1].Children.First().Value.First().Data[1]);


            string json = JsonConvert.SerializeObject(result, Formatting.Indented, new MyJsonConverter());
        }
    }

    public class MyMerger
    {
        public List<Entity> Merge(List<Entity> parentEntities, List<Entity> childrenEntities)
        {
            foreach (var childEntity in childrenEntities)
            {
                foreach (var childEntityRow in childEntity.Rows)
                {
                    // find the row in parent with the same key
                    foreach (var parentEntity in parentEntities)
                    {
                        foreach (var parentEntityRow in parentEntity.Rows)
                        {
                            var left = (int)parentEntityRow.Data[0];
                            var right = (int) childEntityRow.Data[0];
                            if (left == right)
                            {
                                // see if the children entity with the same name exists
                                if (parentEntityRow.Children.ContainsKey(childEntity.PropertyName))
                                {
                                    // if nyes, add to the list
                                    parentEntityRow.Children[childEntity.PropertyName].Add(childEntityRow);
                                }
                                else
                                {
                                    // if not, add a children entity with the same name
                                    parentEntityRow.Children.Add(childEntity.PropertyName, new List<EntityRow> { childEntityRow });
                                }
                            }
                        }
                    }
                }
            }

            return parentEntities;
        }
    }

    public class Entity
    {
        public List<ColumnInfo> Columns { get; set; }
        public string PropertyName { get; set; }
        public string JoinColumnValue { get; set; }
        public List<EntityRow> Rows { get; set; } = new List<EntityRow>();
        public string PropertyType { get; set; }
        public int BatchNumber { get; set; }


    }

    public class EntityRow
    {
        public object[] Data { get; set; }
        public Dictionary<string, List<EntityRow>> Children { get; set; } = new Dictionary<string, List<EntityRow>>();

    }

    public class MyJsonConverter : JsonConverter<Entity>
    {
        public override void WriteJson(JsonWriter writer, Entity value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("rows");
            writer.WriteStartArray();
            foreach (var row in value.Rows)
            {
                writer.WriteStartObject();
                int i = 0;
                foreach (var col in row.Data)
                {
                    writer.WritePropertyName("dd" + i++);
                    writer.WriteValue(col);
                }
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        public override Entity ReadJson(JsonReader reader, Type objectType, Entity existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
