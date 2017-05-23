using Newtonsoft.Json;

namespace Fabric.Databus.API
{
    public class CustomJsonSerializer : JsonSerializer
    {
        public CustomJsonSerializer()
        {
            Formatting = Formatting.Indented;
        }
    }
}
