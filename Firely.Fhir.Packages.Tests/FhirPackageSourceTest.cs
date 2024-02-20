using FluentAssertions;
using Hl7.Fhir.Introspection;
using Hl7.Fhir.Specification;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Threading.Tasks;

#nullable enable

namespace Firely.Fhir.Packages.Tests
{

    [TestClass]
    public class CommonFhirPackageSourceTests
    {
        private const string PACKAGESERVER = "http://packages.simplifier.net";
        private const string PACKAGENAME = "TestData/testPackage.tgz";
        private const string US_CORE_PACKAGE = "hl7.fhir.us.core@4.1.0";

        //ModelInspector is needed for _resolver, but doesn't do anything in this case, since common doesn't have access to STU3 type information.
        private readonly FhirPackageSource _resolver = new(new ModelInspector(FhirRelease.STU3), PACKAGENAME);
        private readonly FhirPackageSource _clientResolver = new(new ModelInspector(FhirRelease.STU3), PACKAGESERVER, new string[] { US_CORE_PACKAGE });


        [TestMethod]
        public async Task TestResolveByCanonicalUri()
        {
            //check StructureDefinitions
            var pat = await _resolver.ResolveByCanonicalUriAsyncAsString("http://hl7.org/fhir/StructureDefinition/Patient").ConfigureAwait(false);
            pat.Should().NotBeNull();
            pat.Should().Contain("\"url\":\"http://hl7.org/fhir/StructureDefinition/Patient\"");

            //check expansions
            var adm_gender = await _resolver.ResolveByCanonicalUriAsyncAsString("http://hl7.org/fhir/ValueSet/administrative-gender").ConfigureAwait(false);
            adm_gender.Should().NotBeNull();
            adm_gender.Should().Contain("\"url\":\"http://hl7.org/fhir/ValueSet/administrative-gender\"");
        }


        [TestMethod, TestCategory("IntegrationTest")]
        public async Task TestResolveByCanonicalUriUsingClient()
        {
            //check StructureDefinition from US Core
            var pat = await _clientResolver.ResolveByCanonicalUriAsyncAsString("http://hl7.org/fhir/us/core/StructureDefinition/us-core-patient").ConfigureAwait(false);
            pat.Should().NotBeNull();
            pat.Should().Contain("\"url\":\"http://hl7.org/fhir/us/core/StructureDefinition/us-core-patient\"");
        }

        [TestMethod]
        public void TestListFileNames()
        {
            //check StructureDefinitions
            var names = _resolver.ListArtifactNames();
            names.Should().Contain("StructureDefinition-Patient.json");
        }

        [TestMethod]
        public void TestLoadArtifactByName()
        {

            //check StructureDefinitions
            var stream = _resolver.LoadArtifactByName("StructureDefinition-Patient.json");

            stream.Should().NotBeNull();

            using var reader = new StreamReader(stream!);
            var artifact = reader.ReadToEnd();

            artifact.Should().StartWith("{\"resourceType\":\"StructureDefinition\",\"id\":\"Patient\"");
        }

        [TestMethod]
        public void TestLoadArtifactByPath()
        {
            var stream = _resolver.LoadArtifactByPath("package/StructureDefinition-Patient.json");

            stream.Should().NotBeNull();

            using var reader = new StreamReader(stream!);
            var artifact = reader.ReadToEnd();

            artifact.Should().StartWith("{\"resourceType\":\"StructureDefinition\",\"id\":\"Patient\"");
        }


        [TestMethod]
        public void TestListResourceUris()
        {
            var names = _resolver.ListResourceUris();
            names.Should().Contain("StructureDefinition/Patient");
            names.Should().Contain("ValueSet/administrative-gender");
        }

        [TestMethod]
        public void TestListCanonicalUris()
        {
            var names = _resolver.ListCanonicalUris();
            names.Should().Contain("http://hl7.org/fhir/StructureDefinition/Patient");
            names.Should().Contain("http://hl7.org/fhir/ValueSet/administrative-gender");
        }

        [TestMethod]
        public async Task TestGetCodeSystemByValueSet()
        {
            var cs = await _resolver.FindCodeSystemByValueSetAsString("http://hl7.org/fhir/ValueSet/address-type").ConfigureAwait(false);
            cs.Should().NotBeNull();
            cs.Should().Contain("\"url\":\"http://hl7.org/fhir/address-type\"");
        }

        [TestMethod]
        public async Task TestGetConceptMap()
        {
            var cms = await _resolver.FindConceptMapsAsStrings(sourceUri: "http://hl7.org/fhir/ValueSet/data-absent-reason", targetUri: "http://hl7.org/fhir/ValueSet/v3-NullFlavor").ConfigureAwait(false);
            cms.Should().NotBeEmpty();
            cms.Should().Contain(c => c.Contains("\"url\":\"http://hl7.org/fhir/ConceptMap/cm-data-absent-reason-v3\""));
            cms.Should().NotContain(c => c.Contains("\"url\":\"http://hl7.org/fhir/ConceptMap/cm-contact-point-use-v3\""));
        }

        [TestMethod]
        public async Task TestGetNamingSystem()
        {
            var ns = await _resolver.FindNamingSystemByUniqueIdAsString("http://snomed.info/sct").ConfigureAwait(false);
            ns.Should().NotBeNull();
            ns.Should().Contain("\"value\":\"http://snomed.info/sct\"");
            ns.Should().Contain("\"value\":\"2.16.840.1.113883.6.96\"");
        }

        [TestMethod]
        public async Task TestGetArtifactByUri()
        {
            var pat = await _resolver.ResolveByUriAsyncAsString("StructureDefinition/Patient").ConfigureAwait(false);
            pat.Should().NotBeNull();
            pat.Should().Contain("\"url\":\"http://hl7.org/fhir/StructureDefinition/Patient\"");
        }



        [DataTestMethod]
        [DataRow(FhirRelease.R5, "5.0.0", "http://packages.simplifier.net", DisplayName = "R5")]
        [DataRow(FhirRelease.R4B, "4.3.0", "http://packages.simplifier.net", DisplayName = "R4B")]
        [DataRow(FhirRelease.R4, "4.0.1", "http://packages.simplifier.net", DisplayName = "R4")]
        [DataRow(FhirRelease.STU3, "3.0.2", "http://packages.simplifier.net", DisplayName = "STU3")]
        public async Task TestFhirCorePackages(FhirRelease release, string version, string packageServer)
        {
            var packageSource = FhirPackageSource.CreateCorePackageSource(new ModelInspector(release), release, packageServer);
            var pat = await packageSource!.ResolveByCanonicalUriAsyncAsString("http://hl7.org/fhir/StructureDefinition/Patient");
            pat.Should().NotBeNull();
            pat.Should().Contain($"\"fhirVersion\":\"{version}\"");

            var extension = await packageSource!.ResolveByCanonicalUriAsyncAsString("http://hl7.org/fhir/StructureDefinition/patient-mothersMaidenName");
            extension.Should().NotBeNull();
            extension.Should().Contain("\"url\":\"http://hl7.org/fhir/StructureDefinition/patient-mothersMaidenName\"");

            if (release == FhirRelease.R5)
            {
                var toolingextension = await packageSource!.ResolveByCanonicalUriAsyncAsString("http://hl7.org/fhir/tools/StructureDefinition/elementdefinition-date-format");
                toolingextension.Should().NotBeNull();
                toolingextension.Should().Contain("\"url\":\"http://hl7.org/fhir/tools/StructureDefinition/elementdefinition-date-format\"");
            }
        }
    }
}
