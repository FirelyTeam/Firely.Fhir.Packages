using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace Firely.Fhir.Packages.Tests
{
    [TestClass]
    public class PartialVersionMatchingTests
    {
        private const string TEST_URL = "http://example.org/StructureDefinition/TestProfile";

        [TestMethod]
        // ResolveCanonical is obsolete, but still supported: this pins down exactly the behaviour that made it
        // obsolete, so callers relying on it keep working until it is removed. CS0618 is expected here.
#pragma warning disable CS0618
        public void ObsoleteResolveCanonical_ReturnsFirstMatchNotHighest()
        {
            var index = CreateTestIndex();

            var first = index.ResolveCanonical(TEST_URL, "1.5");
            var best = index.ResolveBestCandidateByCanonical(TEST_URL, "1.5");

            first!.Version.Should().Be("1.5.0", "ResolveCanonical returns the first 1.5.x in index order");
            best!.Version.Should().Be("1.5.1", "ResolveBestCandidateByCanonical returns the highest 1.5.x");
        }
#pragma warning restore CS0618

        [TestMethod]
        public void ResolveBestCandidateByCanonical_ExactVersionMatch_ShouldWork()
        {
            // Test that exact version matching works with best candidate resolution
            var index = CreateTestIndex();

            var result = index.ResolveBestCandidateByCanonical(TEST_URL, "1.5.0");

            result.Should().NotBeNull();
            result!.Version.Should().Be("1.5.0");
            result.FileName.Should().Be("profile-1.5.0.json");
        }

        [TestMethod]
        public void ResolveBestCandidateByCanonical_PartialVersionMatch_ShouldWork()
        {
            // Test that partial version "1.5" matches the highest 1.5.x, and not 1.4.9
            var index = CreateTestIndex();

            var result = index.ResolveBestCandidateByCanonical(TEST_URL, "1.5");

            result.Should().NotBeNull();
            result!.Version.Should().Be("1.5.1", "Should match the highest 1.5.x version");
            result.FileName.Should().Be("profile-1.5.1.json");
        }

        [TestMethod]
        public void ResolveBestCandidateByCanonical_NoVersionSpecified_ShouldReturnHighest()
        {
            // Test that not specifying a version returns the highest version available
            var index = CreateTestIndex();

            var result = index.ResolveBestCandidateByCanonical(TEST_URL);

            result.Should().NotBeNull();
            result!.Version.Should().Be("1.5.1");
        }

        [TestMethod]
        public void ResolveBestCandidateByCanonical_NonExistentPartialVersion_ShouldReturnNull()
        {
            // Test that requesting a non-existent partial version returns null
            var index = CreateTestIndex();

            var result = index.ResolveBestCandidateByCanonical(TEST_URL, "2.0");

            result.Should().BeNull("No 2.0.x versions exist");
        }

        [TestMethod]
        public void ResolveBestCandidateByCanonical_NonExistentExactVersion_ShouldReturnNull()
        {
            // Test that requesting a non-existent exact version returns null
            var index = CreateTestIndex();

            var result = index.ResolveBestCandidateByCanonical(TEST_URL, "1.6.0");

            result.Should().BeNull("Version 1.6.0 does not exist");
        }

        [TestMethod]
        public void ResolveBestCandidateByCanonical_PartialVersionWithMultipleCandidates_ShouldReturnBest()
        {
            // Test that when multiple candidates match a partial version, the best one is returned
            var index = CreateIndexWithMultipleVersions();
            
            var result = index.ResolveBestCandidateByCanonical(TEST_URL, "1.5");
            
            result.Should().NotBeNull();
            // Should return the highest version in the 1.5.x series
            result!.Version.Should().Be("1.5.2", "Should return the highest 1.5.x version");
        }

        [TestMethod]
        public void PartialVersionMatching_ScenarioTest_GematikLikeCase()
        {
            // Test the specific scenario reported in the gematik issue
            const string bundleProfileUrl = "http://fhir.abda.de/eRezeptAbgabedaten/StructureDefinition/DAV-PR-ERP-AbgabedatenBundle";
            
            var index = new FileIndex();
            
            // Add the profile with full version as it would exist in the package
            var profileFile = new PackageFileReference("DAV-PR-ERP-AbgabedatenBundle.json")
            {
                Canonical = bundleProfileUrl,
                Version = "1.5.0",
                ResourceType = "StructureDefinition"
            };
            
            index.Add(profileFile);
            
            // This mimics the issue: profile reference uses partial version "1.5"
            // but the actual profile has full version "1.5.0"
            var result = index.ResolveBestCandidateByCanonical(bundleProfileUrl, "1.5");
            
            result.Should().NotBeNull("Should resolve partial version 1.5 to full version 1.5.0");
            result!.Version.Should().Be("1.5.0", "Should find the 1.5.0 version when requesting 1.5");
            result.Canonical.Should().Be(bundleProfileUrl);
        }

        [DataRow("1.5", "1.5.0", true)]
        [DataRow("1.5", "1.5.1", true)]
        [DataRow("1.5", "1.5.2", true)]
        [DataRow("1.5", "1.4.9", false)]
        [DataRow("1.5", "1.6.0", false)]
        [DataRow("1.5", "2.0.0", false)]
        [DataRow("1.5", "1.50", false)]  // Should not match "1.50" 
        [DataRow("1.5", "1.51", false)]  // Should not match "1.51"
        [DataRow("1", "1.0.0", true)]
        [DataRow("1", "1.5.0", true)]
        [DataRow("1", "2.0.0", false)]
        [DataRow("1.0", "1.0.0", true)]
        [DataRow("1.0", "1.0.1", true)]
        [DataRow("1.0", "1.1.0", false)]
        [DataTestMethod]
        public void PartialVersionMatching_VariousScenarios(string requestedVersion, string candidateVersion, bool shouldMatch)
        {
            // Test various partial version matching scenarios
            var index = new FileIndex();
            
            var testFile = new PackageFileReference("test.json")
            {
                Canonical = TEST_URL,
                Version = candidateVersion,
                ResourceType = "StructureDefinition"
            };
            
            index.Add(testFile);
            
            var result = index.ResolveBestCandidateByCanonical(TEST_URL, requestedVersion);
            
            if (shouldMatch)
            {
                result.Should().NotBeNull($"Version '{requestedVersion}' should match '{candidateVersion}'");
                result!.Version.Should().Be(candidateVersion);
            }
            else
            {
                result.Should().BeNull($"Version '{requestedVersion}' should not match '{candidateVersion}'");
            }
        }

        [TestMethod]
        public void ResolveAllCanonical_ReturnsEveryVersion()
        {
            // A closure can hold several versions of the same package, so one canonical can legitimately
            // resolve to more than one artifact - ResolveAllCanonical hands all of them to the caller
            // instead of picking one.
            var index = CreateTestIndex();

            var results = index.ResolveAllCanonical(TEST_URL);

            results.Select(r => r.Version).Should().BeEquivalentTo(["1.4.9", "1.5.0", "1.5.1"]);
        }

        [TestMethod]
        public void ResolveAllCanonical_PartialVersionMatch_ReturnsOnlyMatchingVersions()
        {
            var index = CreateTestIndex();

            var results = index.ResolveAllCanonical(TEST_URL, "1.5");

            results.Select(r => r.Version).Should().BeEquivalentTo(["1.5.0", "1.5.1"], "1.4.9 is not a 1.5.x version");
        }

        [TestMethod]
        public void ResolveAllCanonical_NoMatch_ReturnsEmpty()
        {
            var index = CreateTestIndex();

            index.ResolveAllCanonical(TEST_URL, "2.0").Should().BeEmpty();
            index.ResolveAllCanonical("http://example.org/StructureDefinition/Unknown").Should().BeEmpty();
        }

        private static FileIndex CreateTestIndex()
        {
            var index = new FileIndex();
            
            // Add multiple versions of the same profile using the same pattern as existing tests
            var files = new List<PackageFileReference>
            {
                new PackageFileReference("profile-1.4.9.json")
                {
                    Canonical = TEST_URL,
                    Version = "1.4.9",
                    ResourceType = "StructureDefinition"
                },
                new PackageFileReference("profile-1.5.0.json")
                {
                    Canonical = TEST_URL,
                    Version = "1.5.0",
                    ResourceType = "StructureDefinition"
                },
                new PackageFileReference("profile-1.5.1.json")
                {
                    Canonical = TEST_URL,
                    Version = "1.5.1",
                    ResourceType = "StructureDefinition"
                }
            };
            
            index.AddRange(files);
            return index;
        }

        private static FileIndex CreateIndexWithMultipleVersions()
        {
            var index = new FileIndex();
            var versions = new[] { "1.5.0", "1.5.1", "1.5.2", "1.4.9", "1.6.0" };
            
            var files = new List<PackageFileReference>();
            
            foreach (var version in versions)
            {
                files.Add(new PackageFileReference($"profile-{version}.json")
                {
                    Canonical = TEST_URL,
                    Version = version,
                    ResourceType = "StructureDefinition"
                });
            }
            
            index.AddRange(files);
            return index;
        }
    }
}