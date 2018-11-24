// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FastMergeUnitTests.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the FastMergeUnitTests type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable InconsistentNaming
namespace PipelineRunnerTests
{
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Newtonsoft.Json;

    using PipelineRunnerTests.Sources;
    using PipelineRunnerTests.Types;

    /// <summary>
    /// The fast merge unit tests.
    /// </summary>
    [TestClass]
    public partial class FastMergeUnitTests
    {
        /// <summary>
        /// The test method 1.
        /// </summary>
        [TestMethod]
        public void TestFastMerge()
        {
            var textSources = new List<TextSource>
                               {
                                   new TextSource { TextID = "1", EDWPatientId = "100", TextTXT = "This is my test", EncounterID = "301" },
                                   new TextSource { TextID = "2", EDWPatientId = "100", TextTXT = "This is my second test", EncounterID = "302" },
                                   new TextSource { TextID = "3", EDWPatientId = "101", TextTXT = "This is my third test", EncounterID = "303" }
                               };

            var patientSources = new List<PatientSource>
                                  {
                                      new PatientSource { EDWPatientId = "100", MRN = "Mrn100" },
                                      new PatientSource { EDWPatientId = "101", MRN = "Mrn101" }
                                  };

            var encounterSources = new List<EncounterSource>
                                {
                                    new EncounterSource
                                        {
                                            EncounterID = "301",
                                            FacilityAccountID = "401"
                                        },
                                    new EncounterSource
                                        {
                                            EncounterID = "302",
                                            FacilityAccountID = "402"
                                        }
                                };

            var facilitySources = new List<FacilitySource>
                                   {
                                       new FacilitySource { FacilityAccountID = "401", EDWAttendingProviderID = "501" },
                                       new FacilitySource { FacilityAccountID = "402", EDWAttendingProviderID = "502" }
                                   };

            var providerSources = new List<ProviderSource>
                                   {
                                       new ProviderSource { EDWProviderID = "501", last_name = "Jones" },
                                       new ProviderSource { EDWProviderID = "502", last_name = "Smith" }
                                   };

            var output = textSources.Select(text =>
                    new Text
                    {
                        root = text.TextID,
                        data = text.TextTXT,
                        Patient = patientSources.Where(patient => patient.EDWPatientId == text.EDWPatientId)
                            .Select(patient => new Patient
                            {
                                root = patient.EDWPatientId,
                                MRN = patient.MRN
                            })
                            .FirstOrDefault(),
                        Visit = encounterSources.Where(visit => visit.EncounterID == text.EncounterID)
                            .Select(visit => new Visit
                            {
                                root = visit.EncounterID,
                                People = providerSources
                                    .Where(provider => provider.EDWProviderID
                                                       // ReSharper disable once ReplaceWithSingleCallToFirstOrDefault
                                                       == facilitySources
                                                           .Where(facility => facility.FacilityAccountID
                                                                              // ReSharper disable once ReplaceWithSingleCallToFirstOrDefault
                                                                              == encounterSources.Where(encounter => encounter.EncounterID == text.EncounterID)
                                                                                  .FirstOrDefault()?.FacilityAccountID)
                                                           .FirstOrDefault()?.EDWAttendingProviderID)
                                    .Select(provider => new People
                                    {
                                        root = provider.EDWProviderID,
                                        last_name = provider.last_name
                                    })
                                    .ToList()
                            })
                            .FirstOrDefault()
                    })
                .ToList();

            var serializeObject = JsonConvert.SerializeObject(output);
        }
    }
}
