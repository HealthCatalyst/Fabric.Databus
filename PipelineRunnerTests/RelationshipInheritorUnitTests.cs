using System;
using System.Collections.Generic;
using System.Linq;
using Fabric.Databus.Config;
using Fabric.Databus.PipelineRunner;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PipelineRunnerTests
{
    [TestClass]
    public class RelationshipInheritorUnitTests
    {
        [TestMethod]
        public void CanAppendRelationshipsSuccessfully()
        {
            var topLevelTableOrView = "HCOSText.Data";
            var dataSources = new List<DataSource>
            {
                new TopLevelDataSource
                {
                    Path = "$",
                    TableOrView = topLevelTableOrView
                },
                new DataSource
                {
                    Path = "$.visit",
                    MyRelationships = new List<SqlRelationship>
                    {
                        new SqlRelationship
                        {
                            MySource = new SqlRelationshipEntity
                            {
                                Entity = topLevelTableOrView,
                                Key = "VisitKEY"
                            },
                            MyDestination = new SqlRelationshipEntity
                            {
                                Entity = "HCOSText.Visit",
                                Key = "VisitKEY"
                            }
                        }
                    }
                },
                new DataSource
                {
                    Path = "$.visit.facility",
                    MyRelationships = new List<SqlRelationship>
                    {
                        new SqlRelationship
                        {
                            MySource = new SqlRelationshipEntity
                            {
                                Entity = "HCOSText.Visit",
                                Key = "VisitFacilityKEY"
                            },
                            MyDestination = new SqlRelationshipEntity
                            {
                                Entity = "HCOSText.VisitFacility",
                                Key = "VisitFacilityKEY"
                            }
                        }
                    }
                }
            };

            var dataSource = dataSources[2];

            RelationshipInheritor.InheritRelationships(dataSources, dataSource, topLevelTableOrView );

            Assert.AreEqual(2, dataSource.MyRelationships.Count);

            Assert.AreEqual(topLevelTableOrView, dataSource.MyRelationships.First().MySource.Entity);
            Assert.AreEqual("VisitKEY", dataSource.MyRelationships.First().MySource.Key);
        }

        [TestMethod]
        public void CanAppendRelationshipsWithDifferentCaseAndBracketsSuccessfully()
        {
            var dataSources = new List<DataSource>
            {
                new TopLevelDataSource
                {
                    Path = "$",
                    TableOrView = "[hcosText].[data]"
                },
                new DataSource
                {
                    Path = "$.visit",
                    MyRelationships = new List<SqlRelationship>
                    {
                        new SqlRelationship
                        {
                            MySource = new SqlRelationshipEntity
                            {
                                Entity = "HCOSText.Data",
                                Key = "VisitKEY"
                            },
                            MyDestination = new SqlRelationshipEntity
                            {
                                Entity = "HCOSText.Visit",
                                Key = "VisitKEY"
                            }
                        }
                    }
                },
                new DataSource
                {
                    Path = "$.visit.facility",
                    MyRelationships = new List<SqlRelationship>
                    {
                        new SqlRelationship
                        {
                            MySource = new SqlRelationshipEntity
                            {
                                Entity = "HCOSText.Visit",
                                Key = "VisitFacilityKEY"
                            },
                            MyDestination = new SqlRelationshipEntity
                            {
                                Entity = "HCOSText.VisitFacility",
                                Key = "VisitFacilityKEY"
                            }
                        }
                    }
                }
            };

            var dataSource = dataSources[2];

            RelationshipInheritor.InheritRelationships(dataSources, dataSource, "[hcosText].[data]");

            Assert.AreEqual(2, dataSource.MyRelationships.Count);

            Assert.AreEqual("HCOSText.Data", dataSource.MyRelationships.First().MySource.Entity);
            Assert.AreEqual("VisitKEY", dataSource.MyRelationships.First().MySource.Key);
        }

        [TestMethod]
        public void DoesNotAppendRelationshipsWhenCaseIsDifferent()
        {
            var dataSources = new List<DataSource>
            {
                new TopLevelDataSource
                {
                    Path = "$",
                    TableOrView = "[hcosText].[data]"
                },
                new DataSource
                {
                    Path = "$.visit",
                    MyRelationships = new List<SqlRelationship>
                    {
                        new SqlRelationship
                        {
                            MySource = new SqlRelationshipEntity
                            {
                                Entity = "[hcosText].[data]",
                                Key = "VisitKEY"
                            },
                            MyDestination = new SqlRelationshipEntity
                            {
                                Entity = "HCOSText.Visit",
                                Key = "VisitKEY"
                            }
                        }
                    }
                },
                new DataSource
                {
                    Path = "$.visit.facility",
                    MyRelationships = new List<SqlRelationship>
                    {
                        new SqlRelationship
                        {
                            MySource = new SqlRelationshipEntity
                            {
                                Entity = "HCOSText.Data",
                                Key = "VisitKEY"
                            },
                            MyDestination = new SqlRelationshipEntity
                            {
                                Entity = "HCOSText.Visit",
                                Key = "VisitKEY"
                            }
                        },
                        new SqlRelationship
                        {
                            MySource = new SqlRelationshipEntity
                            {
                                Entity = "HCOSText.Visit",
                                Key = "VisitFacilityKEY"
                            },
                            MyDestination = new SqlRelationshipEntity
                            {
                                Entity = "HCOSText.VisitFacility",
                                Key = "VisitFacilityKEY"
                            }
                        }
                    }
                }
            };

            var dataSource = dataSources[2];

            RelationshipInheritor.InheritRelationships(dataSources, dataSource, "[hcosText].[data]");

            Assert.AreEqual(2, dataSource.MyRelationships.Count);

            Assert.AreEqual("HCOSText.Data", dataSource.MyRelationships.First().MySource.Entity);
            Assert.AreEqual("VisitKEY", dataSource.MyRelationships.First().MySource.Key);
        }

    }
}
