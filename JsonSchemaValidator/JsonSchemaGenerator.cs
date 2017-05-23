using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JsonSchemaValidator
{
    public static class JsonSchemaGenerator
    {
        public static void WriteSchemaToFile(Type t, string filename)
        {
            // change Zone enum to generate a string property
            JSchemaGenerator stringEnumGenerator = new JSchemaGenerator();
            stringEnumGenerator.GenerationProviders.Add(new StringEnumGenerationProvider());
            
            JSchema schema = stringEnumGenerator.Generate(t);

            string schemaJson = schema.ToString();

            System.IO.File.WriteAllText(filename, schemaJson);
        }
    }
}
