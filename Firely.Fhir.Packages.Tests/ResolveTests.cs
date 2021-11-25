using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Firely.Fhir.Packages.Tests
{
    [TestClass]
    public class ResolveFilesTest
    {
        private const string R3_CORE_PACKAGE = "hl7.fhir.r3.core";
        private const string EXPANSIONS_PACKAGE = "hl7.fhir.r3.expansions";

        [TestMethod]
        public void ResolveBestCandidateTest()
        {
            var context = TestHelper.GetPackageContext(R3_CORE_PACKAGE, EXPANSIONS_PACKAGE);
            var adm_gender = context.Index.ResolveBestCandidateByCanonical("http://hl7.org/fhir/ValueSet/administrative-gender");
            adm_gender.Should().NotBeNull();
            adm_gender.HasExpansion.Should().BeTrue();
        }

        [TestMethod]
        public async Task TestGetCodeSystemByValueSet()
        {
            var context = TestHelper.GetPackageContext(R3_CORE_PACKAGE, EXPANSIONS_PACKAGE);
            var codeSystem = await context.GetCodeSystemByValueSet("http://hl7.org/fhir/ValueSet/address-type");
            codeSystem.Should().NotBeEmpty();
            codeSystem.Should().Contain("\"url\":\"http://hl7.org/fhir/address-type");
        }

        [TestMethod]
        public void TestGetListOfResourceUris()
        {
            var context = TestHelper.GetPackageContext(R3_CORE_PACKAGE, EXPANSIONS_PACKAGE);
            var listUris = context.ListCanonicalUris();
            listUris.Should().NotBeEmpty();
            listUris.Should().Contain("http://hl7.org/fhir/StructureDefinition/Patient");
            listUris.Should().Contain("http://hl7.org/fhir/administrative-gender");
        }


        [TestMethod]
        public async Task TestGetConceptMap()
        {
            var context = TestHelper.GetPackageContext(R3_CORE_PACKAGE, EXPANSIONS_PACKAGE);
            var conceptMaps = await context.GetConceptMapsBySourceAndTarget(sourceUri: "http://hl7.org/fhir/ValueSet/data-absent-reason", targetUri: "http://hl7.org/fhir/ValueSet/v3-NullFlavor");
            conceptMaps.Should().NotBeEmpty();
            conceptMaps.Should().Contain(i => i.Contains("\"url\":\"http://hl7.org/fhir/ConceptMap/cm-data-absent-reason-v3\""));
            conceptMaps.Should().NotContain(i => i.Contains("\"url\":\"http://hl7.org/fhir/ConceptMap/cm-contact-point-use-v3\""));
        }

        [TestMethod]
        public async Task TestGetNamingSystem()
        {
            var context = TestHelper.GetPackageContext(R3_CORE_PACKAGE, EXPANSIONS_PACKAGE);
            var namingSystem = await context.GetNamingSystemByUniqueId("http://snomed.info/sct");
            namingSystem.Should().NotBeEmpty();
            namingSystem.Should().Contain("http://snomed.info/sct");
            namingSystem.Should().Contain("2.16.840.1.113883.6.96");
        }

    }
}
