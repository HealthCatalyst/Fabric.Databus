// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISqlGeneratorFactory.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the ISqlGeneratorFactory type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Interfaces.Sql
{
    /// <summary>
    /// The SqlGeneratorFactory interface.
    /// </summary>
    public interface ISqlGeneratorFactory
    {
        /// <summary>
        /// The create.
        /// </summary>
        /// <returns>
        /// The <see cref="ISqlGenerator"/>.
        /// </returns>
        ISqlGenerator Create();
    }
}
