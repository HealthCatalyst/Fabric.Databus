namespace Fabric.Databus.PipelineRunner
{
    using System.Collections.Generic;
    using System.Linq;
    using Fabric.Databus.Config;
    using Fabric.Databus.Interfaces.Config;

    public static class RelationshipInheritor
    {

        public static void InheritRelationships(
            IEnumerable<IDataSource> dataDataSources,
            IDataSource dataSource,
            string topLevelTableOrView)
        {
            // see if dataSource is nested
            if (dataSource.Path.GetNestedLevel() <= 0 || !dataSource.Relationships.Any()) return;

            // see if dataSource goes all the way to top level data source
            var firstRelationship = dataSource.Relationships.First();

            if (firstRelationship.Source.Entity.EntityNameEquals(topLevelTableOrView)) return;

            // if not, then find its parent
            var pathOfParent = dataSource.Path.GetPathOfParent();
            var parent = dataDataSources.First(d => d.Path.EntityNameEquals(pathOfParent));

            // append the relationships of the parent
            dataSource.PrependRelationships(parent.Relationships);
        }
    }
}
