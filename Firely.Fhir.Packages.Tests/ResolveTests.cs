using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Firely.Fhir.Packages.Tests
{
    [TestClass]
    public class ResolveFilesTest
    {
        internal const string R3_CORE_PACKAGE = "hl7.fhir.r3.core";
        internal const string EXPANSIONS_PACKAGE = "hl7.fhir.r3.expansions";


        [TestMethod]
        public void ResolveBestCandidateTest()
        {
            var FixtureDirectory = TestHelper.InitializeTemporary("integration-test", EXPANSIONS_PACKAGE, R3_CORE_PACKAGE).Result;
            var projectContext = TestHelper.Open(FixtureDirectory, _ => { }).Result;

            var adm_gender = projectContext.Index.ResolveBestCandidateByCanonical("http://hl7.org/fhir/ValueSet/administrative-gender");
            adm_gender.Should().NotBeNull();
            adm_gender.HasExpansion.Should().BeTrue();
        }
    }
}
