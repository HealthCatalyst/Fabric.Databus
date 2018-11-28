// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SqlTypeToElasticSearchTypeConvertor.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the SqlTypeToElasticSearchTypeConvertor type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Shared
{
    using System;

    /// <summary>
    /// The sql type to elastic search type convertor.
    /// </summary>
    public static class SqlTypeToElasticSearchTypeConvertor
    {
        /// <summary>
        /// The get elastic search type.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">not implemented
        /// </exception>
        public static string GetElasticSearchType(Type type)
        {
            if (type == typeof(string))
            {
                return "keyword"; // TODO; use text or keyword
            }

            if (type == typeof(int))
            {
                return "integer";
            }

            if (type == typeof(DateTime))
            {
                return "date";
            }

            if (type == typeof(decimal))
            {
                return "double";
            }

            if (type == typeof(double))
            {
                return "double";
            }

            if (type == typeof(long))
            {
                return "integer";
            }

            if (type == typeof(float))
            {
                return "integer";
            }

            if (type == typeof(bool))
            {
                return "boolean";
            }

            if (type == typeof(short))
            {
                return "short";
            }

            if (type == typeof(Guid))
            {
                return "keyword";
            }

            throw new NotImplementedException("No Elastic Search type found for type=" + type);
        }
    }
}
