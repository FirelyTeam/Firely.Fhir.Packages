using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Firely.Fhir.Packages.Tests
{
    [TestClass]
    public class ParsingTest
    {
        [TestMethod]
        public void ParseReferences()
        {
            var reference = (PackageReference)"hl7.fhir.r4.core@4.0.1";

            Assert.AreEqual(null, reference.Scope);
            Assert.AreEqual("hl7.fhir.r4.core", reference.Name);
            Assert.AreEqual("4.0.1", reference.Version);

            reference = "hl7.fhir.r4.core@4.0.1";

            Assert.AreEqual(null, reference.Scope);
            Assert.AreEqual("hl7.fhir.r4.core", reference.Name);
            Assert.AreEqual("4.0.1", reference.Version);
        }

        [TestMethod]
        public void ParseDependencie()
        {
            var dependencie = (PackageDependency)"hl7.fhir.r4.core@4.0.1";

            Assert.AreEqual("hl7.fhir.r4.core", dependencie.Name);
            Assert.AreEqual("4.0.1", dependencie.Range);

            dependencie = "hl7.fhir.r4.core@4.0.1";

            Assert.AreEqual("hl7.fhir.r4.core", dependencie.Name);
            Assert.AreEqual("4.0.1", dependencie.Range);
        }

        [TestMethod]
        public void ParseDependencieWithoutVersion()
        {
            var dependencie = (PackageDependency)"hl7.fhir.r4.core";

            Assert.AreEqual("hl7.fhir.r4.core", dependencie.Name);
            Assert.AreEqual("latest", dependencie.Range);

            dependencie = "hl7.fhir.r4.core";

            Assert.AreEqual("hl7.fhir.r4.core", dependencie.Name);
            Assert.AreEqual("latest", dependencie.Range);
        }
    }

    [TestClass]
    public class VersionsTest
    {
        [DataRow("1.0.0", "1.0.0")]
        [DataRow("1.x", "1.0.0")]
        [DataRow("latest", "2.0.0")]
        [DataRow(null, "2.0.0")]
        [DataRow("3.0.0", null)]
        [DataRow("3.x", null)]
        [DataTestMethod]
        public void ResolveVersionTest(string? version, string? versionReturned)
        {
            var target = new Versions(new string[] { "1.0.0", "2.0.0" });

            PackageReference result = target.Resolve(new PackageDependency("SomeName", version));

            bool found = versionReturned is not null;
            result.Found.Should().Be(found);
            result.NotFound.Should().NotBe(found);

            if (found)
            {
                result.Version.Should().Be(versionReturned);
            }
        }
    }
}
