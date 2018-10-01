// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDataSource.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the IDataSource type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ElasticSearchSqlFeeder.Interfaces
{
    using System.Collections.Generic;

    public interface IDataSource
    {
        string Sql { get; set; }

        string Path { get; set; }

        string PropertyType { get; set; }

        List<IQueryField> Fields { get; set; }

        int SequenceNumber { get; set; }
    }
}