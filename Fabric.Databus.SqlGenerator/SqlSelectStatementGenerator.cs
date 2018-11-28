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

    using Fabric.Databus.Interfaces.Config;

    /// <summary>
    /// The sql select statement generator.
    /// </summary>
    public static class SqlSelectStatementGenerator
    {
        /// <summary>
        /// The get sql statement.
        /// </summary>
        /// <param name="sqlRelationships">
        /// The sql Relationships.
        /// </param>
        /// <param name="topLevelKey">
        /// The top Level Key.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        [Pure]
        public static string GetSqlStatement(IList<ISqlRelationship> sqlRelationships, string topLevelKey)
        {
            var sqlRelationshipsCount = sqlRelationships.Count;

            var destinationEntity = sqlRelationships.Last().DestinationEntity;

            var sqlGenerator = new SqlGenerator(destinationEntity)
                .AddColumn(destinationEntity, "*", null);

            int relationshipIndex = sqlRelationshipsCount + 2;
            foreach (var sqlRelationship in sqlRelationships.Reverse())
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

            sqlGenerator.AddColumn(sqlRelationships.First().SourceEntity, topLevelKey, "KeyLevel1");

            string result = sqlGenerator.ToString();
            return result;
        }
    }
}
