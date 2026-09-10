using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

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

        [DataRow("current", null)]     // non-compliant pattern: null, not ArgumentException (#76)
        [DataRow("2024-01-01", null)]  // non-compliant pattern: null, not ArgumentException (#76)
        [DataRow("1.0", "1.0.2")]      // partial version resolves as a range
        [DataRow("latest", "1.0.2")]
        [DataRow("1.0.0", "1.0.0")]
        [DataTestMethod]
        public void ResolveNonCompliantPatternReturnsNullInsteadOfThrowing(string pattern, string? expected)
        {
            var versions = new Versions(new[] { "1.0.0", "1.0.2", "1.0.0-beta-1" });

            var result = versions.Resolve(pattern, stable: false)?.ToString();

            result.Should().Be(expected);
        }

        [TestMethod]
        public void ResolveIgnoresBuildMetadata()
        {
            // SemVer §10: build metadata is ignored when determining precedence, so a pin carrying
            // build metadata resolves to the precedence-equal version (#114 is not a bug).
            var versions = new Versions(new[] { "1.3.0", "1.6.0" });

            versions.Resolve("1.6.0+001", stable: false)!.ToString().Should().Be("1.6.0");
        }

        [TestMethod]
        public void UnparsableVersionsAreReportedAsInvalid()
        {
            var versions = new Versions(new[] { "1.0.0", "1.0", "2024-01-01" }, unlisted: new[] { "also-bad" });

            versions.Items.Should().HaveCount(1);
            versions.Invalid.Should().BeEquivalentTo("1.0", "2024-01-01");
            versions.InvalidUnlisted.Should().BeEquivalentTo("also-bad");
        }

        [TestMethod]
        public void StableIncludesVersionsWithBuildMetadata()
        {
            // Build metadata says nothing about a version being a pre-release
            var versions = new Versions(new[] { "1.0.0+001", "1.1.0-beta" });

            versions.Stable().Select(v => v.ToString()).Should().BeEquivalentTo("1.0.0+001");
            versions.Latest(stable: true)!.ToString().Should().Be("1.0.0+001");
        }
    }
}
