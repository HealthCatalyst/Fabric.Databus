// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SerilogConfig.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the SerilogRootobject type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Client.Config
{
    using Newtonsoft.Json;

    public class SerilogRootobject
    {
        [JsonProperty("Serilog")]
        public SerilogConfig SerilogConfig { get; set; }
    }

    public class SerilogConfig
    {
        public Minimumlevel MinimumLevel { get; set; }
        public Writeto[] WriteTo { get; set; }
        public string[] Enrich { get; set; }
        public Properties Properties { get; set; }
    }

    public class Minimumlevel
    {
        public string Default { get; set; }
    }

    public class Properties
    {
        public string Application { get; set; }
        public string Environment { get; set; }
    }

    public class Writeto
    {
        public string Name { get; set; }
        public Args Args { get; set; }
    }

    public class Args
    {
        public string pathFormat { get; set; }
        public string serverUrl { get; set; }
    }

}
