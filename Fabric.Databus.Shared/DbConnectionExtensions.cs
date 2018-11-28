// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DbConnectionExtensions.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the DbConnectionExtensions type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Shared
{
    using System.Data;
    using System.Data.Common;

    /// <summary>
    /// The db connection extensions.
    /// </summary>
    internal static class DbConnectionExtensions
    {
        /// <summary>
        /// The add parameter with value.
        /// </summary>
        /// <param name="command">
        /// The command.
        /// </param>
        /// <param name="parameterName">
        /// The parameter name.
        /// </param>
        /// <param name="parameterValue">
        /// The parameter value.
        /// </param>
        public static void AddParameterWithValue(this IDbCommand command, string parameterName, object parameterValue)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = parameterName;
            parameter.Value = parameterValue;
            command.Parameters.Add(parameter);
        }
    }
}
