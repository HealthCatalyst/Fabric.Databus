using Nest;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fabric.Databus.ElasticSearchConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var settings = new ConnectionSettings(new Uri("http://localhost:9200"))
                .DefaultIndex("metadata1")
                .DefaultFieldNameInferrer(i => i)
                .EnableDebugMode();

            var client = new ElasticClient(settings);

            ISearchResponse<DataMart> response;
            response = client.Search<DataMart>(s => s
                .MatchAll()
            );

            var hits = response.Hits;

            var debugInfo = response.DebugInformation;

            response = client.Search<DataMart>(query => query
                .From(0)
                .Size(10)
                .Query(q => q
                    .Terms(m => m
                        .Field(f => f.DataMartNM)
                        .Terms("R4")
                        )
                    )
                );

            hits = response.Hits;

            debugInfo = response.DebugInformation;

            response = client.Search<DataMart>(query => query
                .From(0)
                .Size(10)
                .Query(q1 => q1
                .Nested(c => c
                    .Path(p => p.Entities)
                    .Query(q => q
                        .Terms(m => m
                            .Field(f => f.Entities.First().EntityNM)
                            .Terms("R4PopulationMySql")
                                )
                            )
                        )
                    )
                );

            hits = response.Hits;

            debugInfo = response.DebugInformation;
        }
    }

    public class DataMart
    {
        public int DataMartID { get; set; }
        public string ContentID { get; set; }
        public string DataMartNM { get; set; }

        public IList<Entity> Entities { get; set; }
    }

    public class Entity
    {
        public int EntityID { get; set; }
        public int DataMartID { get; set; }
        public string EntityNM { get; set; }
    }
}
