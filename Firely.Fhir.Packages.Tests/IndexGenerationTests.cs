using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Firely.Fhir.Packages.Tests
{
    [TestClass]
    public class IndexGenerationTest
    {
        internal const string HL7_CORE_PACKAGE_R4 = "hl7.fhir.r4.core@4.0.1";

        private const string CORE_PAT_URL = "http://hl7.org/fhir/StructureDefinition/Patient";
        private const string CORE_VS_URL = "http://hl7.org/fhir/ValueSet/administrative-gender";
        private const string CORE_CS_URL = "http://hl7.org/fhir/ConceptMap/101";
        private const string CORE_NS_PATH = "package/NamingSystem-example.json";


        private const string JSON_SCHEMA_PATH = "openapi/Patient.schema.json";
        private const string JSON_SCHEMA_NAME = "Patient.schema.json";

        [TestMethod]
        public void ResourceMetadataIsHarvestedCorrectly()
        {
            var FixtureDirectory = TestHelper.InitializeTemporary("integration-test", HL7_CORE_PACKAGE_R4).Result;
            var projectContext = TestHelper.Open(FixtureDirectory, _ => { }).Result;

            var corePat = projectContext.GetIndex().ResolveCanonical(CORE_PAT_URL);
            corePat.Should().NotBeNull();

            corePat!.Canonical.Should().Be(CORE_PAT_URL);
            corePat!.FhirVersion.Should().Be("4.0.1");
            corePat!.FileName.Should().Be("StructureDefinition-Patient.json");
            corePat!.FilePath.Should().Be("package/StructureDefinition-Patient.json");
            corePat!.Kind.Should().Be("resource");
            corePat!.ResourceType.Should().Be("StructureDefinition");
            corePat!.Type.Should().Be("Patient");
            corePat!.Version.Should().Be("4.0.1");

            var coreValueSet = projectContext.GetIndex().ResolveCanonical(CORE_VS_URL);
            coreValueSet.Should().NotBeNull();
            coreValueSet!.ValueSetCodeSystem = "http://hl7.org/fhir/administrative-gender";

            var coreCodeSystem = projectContext.GetIndex().ResolveCanonical(CORE_CS_URL);
            coreCodeSystem.Should().NotBeNull();
            coreCodeSystem!.ConceptMapUris!.SourceUri.Should().Be("http://hl7.org/fhir/ValueSet/address-use");
            coreCodeSystem!.ConceptMapUris!.TargetUri.Should().Be("http://terminology.hl7.org/ValueSet/v3-AddressUse");

            var coreNamingSystem = projectContext.GetIndex().Where(r => r.FilePath == CORE_NS_PATH).FirstOrDefault();
            coreNamingSystem.Should().NotBeNull();
            if (coreNamingSystem != null)
            {
                coreNamingSystem.NamingSystemUniqueId.Should().Contain("http://snomed.info/sct");
                coreNamingSystem.NamingSystemUniqueId.Should().Contain("2.16.840.1.113883.6.96");
            }
        }

        [TestMethod]
        public void TestIndexJsonOnRoot()
        {
            var FixtureDirectory = TestHelper.InitializeTemporary("integration-test", HL7_CORE_PACKAGE_R4).Result;
            var projectContext = TestHelper.Open(FixtureDirectory, _ => { }).Result;

            var schemaFile = projectContext.GetIndex().Where(i => i.FilePath == JSON_SCHEMA_PATH && i.FileName == JSON_SCHEMA_NAME).FirstOrDefault();
            schemaFile.Should().NotBeNull();
        }

        /// <summary>
        /// Open and restore an NPM package from the given location.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="progressHandler"></param>
        /// <returns></returns>


    }
}
