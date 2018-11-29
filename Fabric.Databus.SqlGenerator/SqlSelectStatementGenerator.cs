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
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;

    using Fabric.Databus.Config;
    using Fabric.Databus.Interfaces.Config;
    using Fabric.Databus.Interfaces.Sql;

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
        /// <param name="sqlGeneratorFactory">
        /// sql Generator factory</param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        [Pure]
        public static string GetSqlStatement(
            string entityName,
            string topLevelKey,
            IEnumerable<ISqlRelationship> sqlRelationships,
            IEnumerable<ISqlEntityColumnMapping> entityColumnMappings,
            ISqlGeneratorFactory sqlGeneratorFactory)
        {
            if (sqlRelationships == null)
            {
                throw new ArgumentNullException(nameof(sqlRelationships));
            }

            if (entityColumnMappings == null)
            {
                throw new ArgumentNullException(nameof(entityColumnMappings));
            }

            var sqlRelationships1 = sqlRelationships.Reverse().ToList();

            var sqlEntityColumnMappings = entityColumnMappings.ToList();

            if (!sqlRelationships1.Any())
            {
                var generator = sqlGeneratorFactory.Create().SetEntity(entityName);

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

                return generator.ToSqlString();
            }

            var sqlRelationshipsCount = sqlRelationships1.Count();

            var destinationEntity = sqlRelationships1.First().Destination.Entity;

            var sqlGenerator = sqlGeneratorFactory.Create().SetEntity(destinationEntity);

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
                sqlGenerator.AddColumn(sqlRelationship.Destination.Entity, sqlRelationship.Destination.Key, $"KeyLevel{relationshipIndex}");
                sqlGenerator.AddJoin(
                    new SqlGeneratorJoin
                    {
                        SourceEntity = sqlRelationship.Source.Entity,
                        SourceEntityKey = sqlRelationship.Source.Key,
                        DestinationEntity = sqlRelationship.Destination.Entity,
                        DestinationEntityKey = sqlRelationship.Destination.Key
                    });
            }

            sqlGenerator.AddColumn(sqlRelationships1.Last().Source.Entity, topLevelKey, "KeyLevel1");

            string result = sqlGenerator.ToSqlString();
            return result;
        }
    }
}
