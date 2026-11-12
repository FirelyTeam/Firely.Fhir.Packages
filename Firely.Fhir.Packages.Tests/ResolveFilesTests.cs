using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Firely.Fhir.Packages.Tests
{
    [TestClass]
    public class ResolveFilesTest
    {
        private const string R3_CORE_PACKAGE = "hl7.fhir.r3.core";
        private const string R3_EXPANSIONS_PACKAGE = "hl7.fhir.r3.expansions";


        [TestMethod]
        public void ResolveBestCandidateTest()
        {
            var context = TestHelper.GetPackageContext(R3_CORE_PACKAGE, R3_EXPANSIONS_PACKAGE);
            var adm_gender = context.GetIndex().ResolveBestCandidateByCanonical("http://hl7.org/fhir/ValueSet/administrative-gender");
            adm_gender.Should().NotBeNull();
            adm_gender!.HasExpansion.Should().BeTrue();
        }

        [DataRow("1.0.1", "1.0.2", "1.0.2")]
        [DataRow("2.0.1", "1.0.1", "2.0.1")]
        [DataRow("1.1.0", "10.0.2", "10.0.2")]
        [DataRow("2.0.1", "10.0.1", "10.0.1")]
        [DataRow("2024-01-01", "2.0.0", "2024-01-01")] // mixed set: alphabetical, last wins
        [DataTestMethod]
        public void ResolveBestCandidateByVersionTest(string version1, string version2, string expectedResult)
        {
            var url = "http://fire.ly/StructureDefinition/example";
            var index = new FileIndex();
            var files = new List<PackageFileReference>(){
                 new PackageFileReference("file1.xml")
                {
                    Canonical = url,
                    Version = version1
                },
                new PackageFileReference("file2.xml")
                {
                    Canonical = url,
                    Version = version2
                }
            };

            index.AddRange(files);
            index.ResolveBestCandidateByCanonical(url)!.Version.Should().Be(expectedResult);
        }

        [DataRow("1.0.1", "1.0.2", "1.0.2")]
        [DataRow("2.0.1", "1.0.1", "2.0.1")]
        [DataRow("1.1.0", "10.0.2", "10.0.2")]
        [DataRow("2024-01-01", "2.0.0", "2024-01-01")] // mixed set: alphabetical, last wins
        [DataRow("2024-01-01", "2024-02-01", "2024-02-01")] // all dates: later wins
        // rule 1: numeric dotted
        [DataRow("1.9", "1.10", "1.10")]
        [DataRow("1.10", "1.9", "1.10")]
        [DataRow("10.0.2", "1.1.0", "10.0.2")]
        [DataRow("4.0.1", "5.0.0", "5.0.0")]
        [DataRow("4.0.1.2", "4.0.1", "4.0.1.2")]
        // rule 2: SemVer
        [DataRow("1.0.0", "1.0.0-ballot", "1.0.0")]
        [DataRow("1.0.0-ballot", "1.0.0", "1.0.0")]
        [DataRow("1.0.0-ballot", "0.9.0", "1.0.0-ballot")]
        [DataRow("0.9.0", "1.0.0-ballot", "1.0.0-ballot")]
        // rule 3: dates
        [DataRow("2024-02-01", "2024-01-01", "2024-02-01")]
        [DataRow("2024-01-15", "2023-06-01", "2024-01-15")]
        [DataRow("2023-06-01", "2024-01-15", "2024-01-15")]
        [DataRow("2023-06", "2024-01", "2024-01")]
        [DataRow("2024", "2023", "2024")]
        [DataRow("20240201", "20240101", "20240201")]
        // mixed dashed/undashed: date order disagrees with alphabetical order
        [DataRow("2024-02", "202401", "2024-02")]
        [DataRow("202401", "2024-02", "2024-02")]
        [DataRow("2024-02-01", "20240115", "2024-02-01")]
        [DataRow("20240115", "2024-02-01", "2024-02-01")]
        // equal dates written differently: first wins
        [DataRow("2024-01-01", "20240101", "2024-01-01")]
        [DataRow("20240101", "2024-01-01", "20240101")]
        [DataRow("2023-12", "202401", "202401")]
        [DataRow("202401", "2023-12", "202401")]
        // not all dates (invalid month, mixed separator, invalid day): alphabetical
        [DataRow("2024-99", "202401", "202401")]
        [DataRow("202401", "2024-99", "202401")]
        [DataRow("2024-0101", "2024-01-02", "2024-0101")]
        [DataRow("2024-01-02", "2024-0101", "2024-0101")]
        [DataRow("2024-01-32", "20240102", "20240102")]
        [DataRow("20240102", "2024-01-32", "20240102")]
        [DataRow("2024-02\n", "202401", "202401")] // trailing newline: not a date
        [DataRow("202401", "2024-02\n", "202401")]
        // rule 4: alphabetical, ignoring case
        [DataRow("draft", "Final", "Final")]
        [DataRow("Final", "draft", "Final")]
        [DataRow("alpha", "Beta", "Beta")]
        // missing version ranks lowest
        [DataRow(null, "0.0.1", "0.0.1")]
        [DataRow("0.0.1", null, "0.0.1")]
        [DataRow("", "0.0.1", "0.0.1")]
        [DataRow("0.0.1", "", "0.0.1")]
        [DataTestMethod]
        public void SelectBestCandidatePicksHighestVersion(string version1, string version2, string expectedResult)
        {
            var candidates = new[]
            {
                new { Version = version1, Label = "a" },
                new { Version = version2, Label = "b" }
            };

            var best = FileIndex.SelectBestCandidate(candidates, c => c.Version);
            best.Version.Should().Be(expectedResult);
        }

        [TestMethod]
        public void SelectBestCandidateUsesPreferredFilterAsTieBreaker()
        {
            var candidates = new[]
            {
                new { Version = "1.0.0", HasSnapshot = false },
                new { Version = "1.0.0", HasSnapshot = true }
            };

            var best = FileIndex.SelectBestCandidate(candidates, c => c.Version, c => c.HasSnapshot);
            best.HasSnapshot.Should().BeTrue(because: "the preferred item should win the tie-break");
        }

        [TestMethod]
        public void SelectBestCandidatePicksHighestOfThreeSemVerInAnyOrder()
        {
            var versions = new[] { "0.9.0", "1.0.0-ballot", "1.0.0" };
            foreach (var order in new[] { new[] { 0, 1, 2 }, new[] { 2, 1, 0 }, new[] { 1, 0, 2 } })
            {
                var candidates = order.Select(i => new { Version = versions[i] }).ToArray();
                FileIndex.SelectBestCandidate(candidates, c => c.Version).Version.Should().Be("1.0.0");
            }
        }

        [TestMethod]
        public void SelectBestCandidateOrdersWholeSetAlphabeticallyWhenOneVersionIsNotNumeric()
        {
            var versions = new[] { "1.9", "1.10", "0draft" };
            foreach (var order in new[] { new[] { 0, 1, 2 }, new[] { 0, 2, 1 }, new[] { 1, 0, 2 }, new[] { 1, 2, 0 }, new[] { 2, 0, 1 }, new[] { 2, 1, 0 } })
            {
                var candidates = order.Select(i => new { Version = versions[i] }).ToArray();
                FileIndex.SelectBestCandidate(candidates, c => c.Version).Version.Should().Be("1.9");
            }
        }

        [TestMethod]
        public void SelectBestCandidateUsesPreferredFilterWhenDatesAreEqualButWrittenDifferently()
        {
            var candidates = new[]
            {
                new { Version = "2024-01-01", HasSnapshot = false },
                new { Version = "20240101", HasSnapshot = true }
            };

            FileIndex.SelectBestCandidate(candidates, c => c.Version, c => c.HasSnapshot).Version.Should().Be("20240101");
        }

        [TestMethod]
        public void ResolveBestCandidateByCanonicalPrefersSnapshotOnEqualVersion()
        {
            var url = "http://fire.ly/StructureDefinition/tie";
            var index = new FileIndex();
            index.AddRange(new List<PackageFileReference>
            {
                new PackageFileReference("nosnap.json") { Canonical = url, Version = "1.0.0", HasSnapshot = false },
                new PackageFileReference("snap.json") { Canonical = url, Version = "1.0.0", HasSnapshot = true }
            });

            index.ResolveBestCandidateByCanonical(url)!.FileName.Should().Be("snap.json");
        }

        [TestMethod]
        public async Task TestGetCodeSystemByValueSet()
        {
            var context = TestHelper.GetPackageContext(R3_CORE_PACKAGE, R3_EXPANSIONS_PACKAGE);
            var codeSystem = await context.GetCodeSystemByValueSet("http://hl7.org/fhir/ValueSet/address-type");
            codeSystem.Should().NotBeEmpty();
            codeSystem.Should().Contain("\"url\":\"http://hl7.org/fhir/address-type");
        }

        [TestMethod]
        public void TestGetListOfResourceUris()
        {
            var context = TestHelper.GetPackageContext(R3_CORE_PACKAGE, R3_EXPANSIONS_PACKAGE);
            var listUris = context.ListResourceUris();
            listUris.Should().NotBeEmpty();
            listUris.Should().Contain("StructureDefinition/Patient");
            listUris.Should().Contain("ValueSet/administrative-gender");
        }

        [TestMethod]
        public void TestGetListOfCanonicalUris()
        {
            var context = TestHelper.GetPackageContext(R3_CORE_PACKAGE, R3_EXPANSIONS_PACKAGE);
            var listCanonicals = context.ListCanonicalUris();
            listCanonicals.Should().NotBeEmpty();
            listCanonicals.Should().Contain("http://hl7.org/fhir/StructureDefinition/Patient");
            listCanonicals.Should().Contain("http://hl7.org/fhir/administrative-gender");
        }

        [TestMethod]
        public async Task TestGetConceptMap()
        {
            var context = TestHelper.GetPackageContext(R3_CORE_PACKAGE, R3_EXPANSIONS_PACKAGE);
            var conceptMaps = await context.GetConceptMapsBySourceAndTarget(sourceUri: "http://hl7.org/fhir/ValueSet/data-absent-reason", targetUri: "http://hl7.org/fhir/ValueSet/v3-NullFlavor");
            conceptMaps.Should().NotBeEmpty();
            conceptMaps.Should().Contain(i => i.Contains("\"url\":\"http://hl7.org/fhir/ConceptMap/cm-data-absent-reason-v3\""));
            conceptMaps.Should().NotContain(i => i.Contains("\"url\":\"http://hl7.org/fhir/ConceptMap/cm-contact-point-use-v3\""));

            conceptMaps = await context.GetConceptMapsBySourceAndTarget(sourceUri: "http://hl7.org/fhir/ValueSet/data-absent-reason", null);
            conceptMaps.Should().NotBeEmpty();
            conceptMaps.Should().Contain(i => i.Contains("\"url\":\"http://hl7.org/fhir/ConceptMap/cm-data-absent-reason-v3\""));
            conceptMaps.Should().NotContain(i => i.Contains("\"url\":\"http://hl7.org/fhir/ConceptMap/cm-contact-point-use-v3\""));

            conceptMaps = await context.GetConceptMapsBySourceAndTarget(sourceUri: null, targetUri: "http://hl7.org/fhir/ValueSet/v3-NullFlavor");
            conceptMaps.Should().NotBeEmpty();
            conceptMaps.Should().Contain(i => i.Contains("\"url\":\"http://hl7.org/fhir/ConceptMap/cm-data-absent-reason-v3\""));
            conceptMaps.Should().NotContain(i => i.Contains("\"url\":\"http://hl7.org/fhir/ConceptMap/cm-contact-point-use-v3\""));
        }

        [TestMethod]
        public async Task TestGetNamingSystem()
        {
            var context = TestHelper.GetPackageContext(R3_CORE_PACKAGE, R3_EXPANSIONS_PACKAGE);
            var namingSystem = await context.GetNamingSystemByUniqueId("http://snomed.info/sct");
            namingSystem.Should().NotBeEmpty();
            namingSystem.Should().Contain("http://snomed.info/sct");
            namingSystem.Should().Contain("2.16.840.1.113883.6.96");
        }

    }
}
