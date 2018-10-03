// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MappingItem.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the MappingItem type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Interfaces.Sql
{
    using System.Collections.Generic;

    /// <summary>
    /// The mapping item.
    /// </summary>
    public class MappingItem
    {
        public List<ColumnInfo> Columns { get; set; }
        public int SequenceNumber { get; set; }
        public string PropertyPath { get; set; }
        public string PropertyType { get; set; }
    }
}