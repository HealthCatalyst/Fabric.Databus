// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SqlExtensions.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the SqlExtensions type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Shared.Sql
{
    using System.Data;

    /// <summary>
    /// The sql extensions.
    /// </summary>
    public static class SqlExtensions
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
        /// <param name="type">
        /// The type.
        /// </param>
        /// <param name="parameterValue">
        /// The parameter value.
        /// </param>
        public static void AddParameterWithValueAndType(this IDbCommand command, string parameterName, DbType type, object parameterValue)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = parameterName;
            parameter.Value = parameterValue;
            parameter.DbType = type;
            command.Parameters.Add(parameter);
        }
    }
}
