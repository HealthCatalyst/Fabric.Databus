// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GeoCode.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the GeoCode type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.ZipCodeToGeoCode
{
    /// <summary>
    /// The geo code.
    /// </summary>
    public class GeoCode
    {
        public decimal lat { get; set; }
        public decimal lon { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public string ZipCode { get; internal set; }
    }
}