// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JsonSchemaGenerator.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the JsonSchemaGenerator type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace JsonSchemaValidator
{
    using System;

    using Newtonsoft.Json.Schema;
    using Newtonsoft.Json.Schema.Generation;

    /// <summary>
    /// The json schema generator.
    /// </summary>
    public static class JsonSchemaGenerator
    {
        /// <summary>
        /// The write schema to file.
        /// </summary>
        /// <param name="t">
        /// The t.
        /// </param>
        /// <param name="filename">
        /// The filename.
        /// </param>
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
