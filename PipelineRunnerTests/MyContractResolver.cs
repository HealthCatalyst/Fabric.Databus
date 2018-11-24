// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MyContractResolver.cs" company="">
//   
// </copyright>
// <summary>
//   The my contract resolver.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PipelineRunnerTests
{
    using System;
    using System.Runtime.Serialization;

    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// The my contract resolver.
    /// </summary>
    public class MyContractResolver : DefaultContractResolver
    {
        /// <summary>
        /// The create contract.
        /// </summary>
        /// <param name="objectType">
        /// The object type.
        /// </param>
        /// <returns>
        /// The <see cref="JsonContract"/>.
        /// </returns>
        protected override JsonContract CreateContract(Type objectType)
        {
            if (typeof(ISerializable).IsAssignableFrom(objectType))
            {
                return this.CreateISerializableContract(objectType);
            }

            return base.CreateContract(objectType);
        }
    }
}