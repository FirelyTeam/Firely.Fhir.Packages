using FluentAssertions;
using Hl7.Fhir.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Firely.Fhir.Packages.Tests
{
    [TestClass]
    public class VersionAlgorithmTests
    {
        private const string EXT_URL = "http://hl7.org/fhir/StructureDefinition/artifact-versionAlgorithm";
        private const string CANONICAL = "http://fire.ly/ValueSet/algo-example";

        private static string? Harvest(string resourceBody)
        {
            var json = "{\"resourceType\":\"ValueSet\",\"url\":\"" + CANONICAL + "\",\"version\":\"1\"" + resourceBody + "}";
            var node = FhirJsonNode.Parse(json);
            return CanonicalIndexer.BuildResourceMetadata("package/ValueSet-x.json", node).VersionAlgorithm;
        }

        [TestMethod]
        public void Harvests_versionAlgorithmString_element()
            => Harvest(",\"versionAlgorithmString\":\"alpha\"").Should().Be("alpha");

        [TestMethod]
        public void Harvests_versionAlgorithmCoding_code()
            => Harvest(",\"versionAlgorithmCoding\":{\"system\":\"http://hl7.org/fhir/version-algorithm\",\"code\":\"semver\"}")
                .Should().Be("semver");

        [TestMethod]
        public void Harvests_R4_extension_valueString()
            => Harvest($",\"extension\":[{{\"url\":\"{EXT_URL}\",\"valueString\":\"integer\"}}]").Should().Be("integer");

        [TestMethod]
        public void Harvests_R4_extension_valueCoding_code()
            => Harvest($",\"extension\":[{{\"url\":\"{EXT_URL}\",\"valueCoding\":{{\"code\":\"date\"}}}}]").Should().Be("date");

        [TestMethod]
        public void Harvests_extension_when_not_first_on_resource()
            => Harvest($",\"extension\":[{{\"url\":\"http://example.org/other\",\"valueString\":\"x\"}},{{\"url\":\"{EXT_URL}\",\"valueString\":\"natural\"}}]")
                .Should().Be("natural");

        [TestMethod]
        public void Element_wins_over_extension_when_both_present()
            => Harvest($",\"versionAlgorithmString\":\"alpha\",\"extension\":[{{\"url\":\"{EXT_URL}\",\"valueString\":\"semver\"}}]")
                .Should().Be("alpha");

        [TestMethod]
        public void Harvest_is_null_when_nothing_declared()
            => Harvest("").Should().BeNull();

        [TestMethod]
        public void CopyTo_carries_VersionAlgorithm()
        {
            var source = new ResourceMetadata("a.json", "package/a.json") { VersionAlgorithm = "alpha" };
            var target = new ResourceMetadata("b.json", "package/b.json");
            source.CopyTo(target);
            target.VersionAlgorithm.Should().Be("alpha");
        }

        [TestMethod]
        public void Index_serialization_uses_versionAlgorithm_name_and_round_trips()
        {
            var index = new CanonicalIndex
            {
                Version = CanonicalIndexer.FIRELY_INDEX_VERSION,
                Files = new List<ResourceMetadata>
                {
                    new("a.json", "package/a.json") { VersionAlgorithm = "alpha" }
                }
            };

            var text = PackageParser.SerializeCanonicalIndex(index);
            JObject.Parse(text)["files"]![0]!["versionAlgorithm"]!.Value<string>().Should().Be("alpha");

            PackageParser.ParseCanonicalIndex(text)!.Files![0].VersionAlgorithm.Should().Be("alpha");
        }

        [TestMethod]
        public void Index_version_is_9()
            => CanonicalIndexer.FIRELY_INDEX_VERSION.Should().Be(9);

        private static string? Resolve(params (string version, string? algorithm)[] entries)
        {
            var index = new FileIndex();
            var i = 0;
            foreach (var (version, algorithm) in entries)
            {
                index.Add(new PackageFileReference($"file{i++}.json")
                {
                    Canonical = CANONICAL,
                    Version = version,
                    VersionAlgorithm = algorithm
                });
            }
            return index.ResolveBestCandidateByCanonical(CANONICAL)?.Version;
        }

        [TestMethod]
        public void Undeclared_versions_use_natural_order()
            => Resolve(("2", null), ("10", null)).Should().Be("10");

        [TestMethod]
        public void Declared_alpha_changes_winner_over_natural_order()
            => Resolve(("2", "alpha"), ("10", "alpha")).Should().Be("2");

        [TestMethod]
        public void Unknown_declared_value_keeps_inference()
            => Resolve(("2", "version.toString().length()"), ("10", "version.toString().length()")).Should().Be("10");

        [TestMethod]
        public void Declared_algorithm_that_versions_do_not_fit_keeps_inference()
            => Resolve(("2", "semver"), ("10", "semver")).Should().Be("10");

        [TestMethod]
        public void Disagreeing_declarations_keep_inference()
            => Resolve(("2", "alpha"), ("10", "integer")).Should().Be("10");
    }
}
