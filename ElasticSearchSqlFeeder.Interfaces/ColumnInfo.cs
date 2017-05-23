using System;
using System.Text;

namespace ElasticSearchSqlFeeder.Interfaces
{
    public class ColumnInfo
    {
        public int? sourceIndex;

        public int index { get; set; }
        public string Name { get; set; }
        public string ElasticSearchType { get; set; }
        public bool IsCalculated { get; set; }
        public string Transform { get; set; }
        public bool IsJoinColumn { get; set; }
    }
}
