// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QueryFieldTransform.cs" company="Health Catalyst">
//   
// </copyright>
// <summary>
//   Defines the QueryFieldTransform type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Interfaces
{
    /// <summary>
    /// The query field transform.
    /// </summary>
    public enum QueryFieldTransform
    {
        None = 0,
        Zip3ToGeocode,
        Zip5ToGeocode
    }
}