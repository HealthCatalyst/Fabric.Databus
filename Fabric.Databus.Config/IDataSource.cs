namespace Fabric.Databus.Config
{
    using System.Collections.Generic;

    public interface IDataSource
    {
        string Sql { get; set; }

        string Path { get; set; }

        string PropertyType { get; set; }

        List<QueryField> Fields { get; set; }

        int SequenceNumber { get; set; }
    }
}