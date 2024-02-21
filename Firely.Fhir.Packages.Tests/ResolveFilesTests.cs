using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
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
        [DataRow("2024-01-01", "2.0.0", "2.0.0")] //dates are not allowed as version, a legal version will always be resolved in favor of an illegal version.
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
