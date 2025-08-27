using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Firely.Fhir.Packages.Tests
{
    [TestClass]
    public class PartialVersionMatchingTests
    {
        private const string TEST_URL = "http://example.org/StructureDefinition/TestProfile";

        [TestMethod]
        public void ResolveCanonical_ExactVersionMatch_ShouldWork()
        {
            // Test that exact version matching still works (backward compatibility)
            var index = CreateTestIndex();
            
            var result = index.ResolveCanonical(TEST_URL, "1.5.0");
            
            result.Should().NotBeNull();
            result!.Version.Should().Be("1.5.0");
            result.FileName.Should().Be("profile-1.5.0.json");
        }

        [TestMethod]
        public void ResolveCanonical_PartialVersionMatch_ShouldWork()
        {
            // Test that partial version "1.5" matches "1.5.0"
            var index = CreateTestIndex();
            
            var result = index.ResolveCanonical(TEST_URL, "1.5");
            
            result.Should().NotBeNull();
            result!.Version.Should().Be("1.5.0", "Should match the first 1.5.x version found");
            result.FileName.Should().Be("profile-1.5.0.json");
        }

        [TestMethod]
        public void ResolveCanonical_NoVersionSpecified_ShouldReturnAny()
        {
            // Test that not specifying a version returns any version
            var index = CreateTestIndex();
            
            var result = index.ResolveCanonical(TEST_URL);
            
            result.Should().NotBeNull();
            // Should return the first match found (any version)
        }

        [TestMethod]
        public void ResolveCanonical_NonExistentPartialVersion_ShouldReturnNull()
        {
            // Test that requesting a non-existent partial version returns null
            var index = CreateTestIndex();
            
            var result = index.ResolveCanonical(TEST_URL, "2.0");
            
            result.Should().BeNull("No 2.0.x versions exist");
        }

        [TestMethod]
        public void ResolveCanonical_NonExistentExactVersion_ShouldReturnNull()
        {
            // Test that requesting a non-existent exact version returns null
            var index = CreateTestIndex();
            
            var result = index.ResolveCanonical(TEST_URL, "1.6.0");
            
            result.Should().BeNull("Version 1.6.0 does not exist");
        }

        [TestMethod]
        public void ResolveBestCandidateByCanonical_ExactVersionMatch_ShouldWork()
        {
            // Test that exact version matching works with best candidate resolution
            var index = CreateTestIndex();
            
            var result = index.ResolveBestCandidateByCanonical(TEST_URL, "1.5.0");
            
            result.Should().NotBeNull();
            result!.Version.Should().Be("1.5.0");
        }

        [TestMethod]
        public void ResolveBestCandidateByCanonical_PartialVersionMatch_ShouldWork()
        {
            // Test that partial version matching works with best candidate resolution
            var index = CreateTestIndex();
            
            var result = index.ResolveBestCandidateByCanonical(TEST_URL, "1.5");
            
            result.Should().NotBeNull();
            result!.Version.Should().StartWith("1.5.", "Should match a 1.5.x version");
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
            var result = index.ResolveCanonical(bundleProfileUrl, "1.5");
            
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
            
            var result = index.ResolveCanonical(TEST_URL, requestedVersion);
            
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