// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CustomJsonSerializer.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the CustomJsonSerializer type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.API
{
    using Newtonsoft.Json;

    /// <inheritdoc />
    public sealed class CustomJsonSerializer : JsonSerializer
    {
        /// <inheritdoc />
        public CustomJsonSerializer()
        {
            this.Formatting = Formatting.Indented;
        }
    }
}
