using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Firely.Fhir.Packages.Tests
{
    [TestClass]
    public class IndexGenerationTest
    {
        internal const string HL7_CORE_PACKAGE_R4 = "hl7.fhir.r4.core@4.0.1";
        internal const string US_CORE_TESTPACKAGE = "hl7.fhir.us.core@3.2.0";
        private const string US_CORE_PAT_URL = "http://hl7.org/fhir/us/core/StructureDefinition/us-core-patient";
        private const string JSON_SCHEMA_PATH = "openapi/Patient.schema.json";
        private const string JSON_SCHEMA_NAME = "Patient.schema.json";

        [TestMethod]
        public void ResourceMetadataIsHarvestedCorrectly()
        {
            var FixtureDirectory = TestHelper.InitializeTemporary("integration-test", US_CORE_TESTPACKAGE).Result;
            var projectContext = TestHelper.Open(FixtureDirectory, _ => { }).Result;

            var usCorePat = projectContext.Index.ResolveCanonical(US_CORE_PAT_URL);
            usCorePat.Should().NotBeNull();

            usCorePat.Canonical.Should().Be(US_CORE_PAT_URL);
            usCorePat.FhirVersion.Should().Be("4.0.1");
            usCorePat.FileName.Should().Be("StructureDefinition-us-core-patient.json");
            usCorePat.FilePath.Should().Be("package/StructureDefinition-us-core-patient.json");
            usCorePat.Kind.Should().Be("resource");
            usCorePat.ResourceType.Should().Be("StructureDefinition");
            usCorePat.Type.Should().Be("Patient");
            usCorePat.Version.Should().Be("3.2.0");
        }

        [TestMethod]
        public void TestIndexJsonOnRoot()
        {
            var FixtureDirectory = TestHelper.InitializeTemporary("integration-test", HL7_CORE_PACKAGE_R4).Result;
            var projectContext = TestHelper.Open(FixtureDirectory, _ => { }).Result;

            var schemaFile = projectContext.Index.Where(i => i.FilePath == JSON_SCHEMA_PATH && i.FileName == JSON_SCHEMA_NAME).FirstOrDefault();
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
