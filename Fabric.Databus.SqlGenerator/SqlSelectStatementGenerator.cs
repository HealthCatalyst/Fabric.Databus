// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SqlSelectStatementGenerator.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the SqlSelectStatementGenerator type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.SqlGenerator
{
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;

    using Fabric.Databus.Config;
    using Fabric.Databus.Interfaces.Config;

    /// <summary>
    /// The sql select statement generator.
    /// </summary>
    public static class SqlSelectStatementGenerator
    {
        /// <summary>
        /// The get sql statement.
        /// </summary>
        /// <param name="entityName">
        /// The entity Name.
        /// </param>
        /// <param name="topLevelKey">
        /// The top Level Key.
        /// </param>
        /// <param name="sqlRelationships">
        /// The sql Relationships.
        /// </param>
        /// <param name="entityColumnMappings">
        /// entity column mappings
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        [Pure]
        public static string GetSqlStatement(
            string entityName,
            string topLevelKey,
            IEnumerable<ISqlRelationship> sqlRelationships,
            IEnumerable<ISqlEntityColumnMapping> entityColumnMappings)
        {
            var sqlRelationships1 = sqlRelationships.Reverse().ToList();

            var sqlEntityColumnMappings = entityColumnMappings.ToList();

            if (!sqlRelationships1.Any())
            {
                var generator = new SqlGenerator(entityName);
                if (sqlEntityColumnMappings.Any())
                {
                    foreach (var sqlEntityColumnMapping in sqlEntityColumnMappings)
                    {
                        generator.AddColumn(
                            sqlEntityColumnMapping.Entity ?? entityName,
                            sqlEntityColumnMapping.Name,
                            sqlEntityColumnMapping.Alias);
                    }
                }
                else
                {
                    generator.AddColumn(entityName, "*", null);
                }

                generator.AddColumn(entityName, topLevelKey, "KeyLevel1");

                return generator.ToString();
            }

            var sqlRelationshipsCount = sqlRelationships1.Count();

            var destinationEntity = sqlRelationships1.First().DestinationEntity;

            var sqlGenerator = new SqlGenerator(destinationEntity);
            if (sqlEntityColumnMappings.Any())
            {
                foreach (var sqlEntityColumnMapping in sqlEntityColumnMappings)
                {
                    sqlGenerator.AddColumn(
                        sqlEntityColumnMapping.Entity ?? entityName,
                        sqlEntityColumnMapping.Name,
                        sqlEntityColumnMapping.Alias);
                }
            }
            else
            {
                sqlGenerator.AddColumn(destinationEntity, "*", null);
            }

            int relationshipIndex = sqlRelationshipsCount + 2;
            foreach (var sqlRelationship in sqlRelationships1)
            {
                relationshipIndex--;
                sqlGenerator.AddColumn(sqlRelationship.DestinationEntity, sqlRelationship.DestinationEntityKey, $"KeyLevel{relationshipIndex}");
                sqlGenerator.AddJoin(
                    new SqlGeneratorJoin
                    {
                        SourceEntity = sqlRelationship.SourceEntity,
                        SourceEntityKey = sqlRelationship.SourceEntityKey,
                        DestinationEntity = sqlRelationship.DestinationEntity,
                        DestinationEntityKey = sqlRelationship.DestinationEntityKey
                    });
            }

            sqlGenerator.AddColumn(sqlRelationships1.Last().SourceEntity, topLevelKey, "KeyLevel1");

            string result = sqlGenerator.ToString();
            return result;
        }
    }
}
