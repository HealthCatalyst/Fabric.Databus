namespace Fabric.Databus.Interfaces
{
    using System.Collections.Generic;

    public class MappingItem
    {
        public List<ColumnInfo> Columns { get; set; }
        public int SequenceNumber { get; set; }
        public string PropertyPath { get; set; }
        public string PropertyType { get; set; }
    }
}