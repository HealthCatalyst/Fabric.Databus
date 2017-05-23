using System.Collections.Generic;

namespace ElasticSearchSqlFeeder.Interfaces
{
    public class MappingItem
    {
        public List<ColumnInfo> Columns { get; set; }
        public int SequenceNumber { get; set; }
        public string PropertyPath { get; set; }
        public string PropertyType { get; set; }
    }
}