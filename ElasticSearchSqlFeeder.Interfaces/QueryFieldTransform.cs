// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QueryFieldTransform.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the QueryFieldTransform type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ElasticSearchSqlFeeder.Interfaces
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