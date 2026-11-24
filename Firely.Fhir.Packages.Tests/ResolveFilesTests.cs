using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Firely.Fhir.Packages.Tests
{
    [TestClass]
    public class ResolveFilesTest
    {
        private const string R3_CORE_PACKAGE = "hl7.fhir.r3.core";
        private const string R3_EXPANSIONS_PACKAGE = "hl7.fhir.r3.expansions";


        [TestMethod]
        public void ResolveBestCandidateTest()
        {
            var context = TestHelper.GetPackageContext(R3_CORE_PACKAGE, R3_EXPANSIONS_PACKAGE);
            var adm_gender = context.GetIndex().ResolveBestCandidateByCanonical("http://hl7.org/fhir/ValueSet/administrative-gender");
            adm_gender.Should().NotBeNull();
            adm_gender!.HasExpansion.Should().BeTrue();
        }

        [DataRow("1.0.1", "1.0.2", "1.0.2")]
        [DataRow("2.0.1", "1.0.1", "2.0.1")]
        [DataRow("1.1.0", "10.0.2", "10.0.2")]
        [DataRow("2.0.1", "10.0.1", "10.0.1")]
        [DataRow("2024-01-01", "2.0.0", "2.0.0")] //dates are not allowed as version, a legal version will always be resolved in favor of an illegal version.
        [DataTestMethod]
        public void ResolveBestCandidateByVersionTest(string version1, string version2, string expectedResult)
        {
            var url = "http://fire.ly/StructureDefinition/example";
            var index = new FileIndex();
            var files = new List<PackageFileReference>(){
                 new PackageFileReference("file1.xml")
                {
                    Canonical = url,
                    Version = version1
                },
                new PackageFileReference("file2.xml")
                {
                    Canonical = url,
                    Version = version2
                }
            };

            index.AddRange(files);
            index.ResolveBestCandidateByCanonical(url)!.Version.Should().Be(expectedResult);
        }

        [DataRow("1.0.1", "1.0.2", "1.0.2")]
        [DataRow("2.0.1", "1.0.1", "2.0.1")]
        [DataRow("1.1.0", "10.0.2", "10.0.2")]
        [DataRow("2024-01-01", "2.0.0", "2.0.0")] // non-parseable versions lose to any parseable version
        [DataRow("2024-01-01", "2024-02-01", "2024-01-01")] // both non-parseable: first is returned
        [DataTestMethod]
        public void SelectBestCandidatePicksHighestVersion(string version1, string version2, string expectedResult)
        {
            var candidates = new[]
            {
                new { Version = version1, Label = "a" },
                new { Version = version2, Label = "b" }
            };

            var best = FileIndex.SelectBestCandidate(candidates, c => c.Version);
            best.Version.Should().Be(expectedResult);
        }

        [TestMethod]
        public void SelectBestCandidateUsesPreferredFilterAsTieBreaker()
        {
            var candidates = new[]
            {
                new { Version = "1.0.0", HasSnapshot = false },
                new { Version = "1.0.0", HasSnapshot = true }
            };

            var best = FileIndex.SelectBestCandidate(candidates, c => c.Version, c => c.HasSnapshot);
            best.HasSnapshot.Should().BeTrue(because: "the preferred item should win the tie-break");
        }

        [TestMethod]
        public async Task TestGetCodeSystemByValueSet()
        {
            var context = TestHelper.GetPackageContext(R3_CORE_PACKAGE, R3_EXPANSIONS_PACKAGE);
            var codeSystem = await context.GetCodeSystemByValueSet("http://hl7.org/fhir/ValueSet/address-type");
            codeSystem.Should().NotBeEmpty();
            codeSystem.Should().Contain("\"url\":\"http://hl7.org/fhir/address-type");
        }

        [TestMethod]
        public void TestGetListOfResourceUris()
        {
            var context = TestHelper.GetPackageContext(R3_CORE_PACKAGE, R3_EXPANSIONS_PACKAGE);
            var listUris = context.ListResourceUris();
            listUris.Should().NotBeEmpty();
            listUris.Should().Contain("StructureDefinition/Patient");
            listUris.Should().Contain("ValueSet/administrative-gender");
        }

        [TestMethod]
        public void TestGetListOfCanonicalUris()
        {
            var context = TestHelper.GetPackageContext(R3_CORE_PACKAGE, R3_EXPANSIONS_PACKAGE);
            var listCanonicals = context.ListCanonicalUris();
            listCanonicals.Should().NotBeEmpty();
            listCanonicals.Should().Contain("http://hl7.org/fhir/StructureDefinition/Patient");
            listCanonicals.Should().Contain("http://hl7.org/fhir/administrative-gender");
        }

        [TestMethod]
        public async Task TestGetConceptMap()
        {
            var context = TestHelper.GetPackageContext(R3_CORE_PACKAGE, R3_EXPANSIONS_PACKAGE);
            var conceptMaps = await context.GetConceptMapsBySourceAndTarget(sourceUri: "http://hl7.org/fhir/ValueSet/data-absent-reason", targetUri: "http://hl7.org/fhir/ValueSet/v3-NullFlavor");
            conceptMaps.Should().NotBeEmpty();
            conceptMaps.Should().Contain(i => i.Contains("\"url\":\"http://hl7.org/fhir/ConceptMap/cm-data-absent-reason-v3\""));
            conceptMaps.Should().NotContain(i => i.Contains("\"url\":\"http://hl7.org/fhir/ConceptMap/cm-contact-point-use-v3\""));

            conceptMaps = await context.GetConceptMapsBySourceAndTarget(sourceUri: "http://hl7.org/fhir/ValueSet/data-absent-reason", null);
            conceptMaps.Should().NotBeEmpty();
            conceptMaps.Should().Contain(i => i.Contains("\"url\":\"http://hl7.org/fhir/ConceptMap/cm-data-absent-reason-v3\""));
            conceptMaps.Should().NotContain(i => i.Contains("\"url\":\"http://hl7.org/fhir/ConceptMap/cm-contact-point-use-v3\""));

            conceptMaps = await context.GetConceptMapsBySourceAndTarget(sourceUri: null, targetUri: "http://hl7.org/fhir/ValueSet/v3-NullFlavor");
            conceptMaps.Should().NotBeEmpty();
            conceptMaps.Should().Contain(i => i.Contains("\"url\":\"http://hl7.org/fhir/ConceptMap/cm-data-absent-reason-v3\""));
            conceptMaps.Should().NotContain(i => i.Contains("\"url\":\"http://hl7.org/fhir/ConceptMap/cm-contact-point-use-v3\""));
        }

        [TestMethod]
        public async Task TestGetNamingSystem()
        {
            var context = TestHelper.GetPackageContext(R3_CORE_PACKAGE, R3_EXPANSIONS_PACKAGE);
            var namingSystem = await context.GetNamingSystemByUniqueId("http://snomed.info/sct");
            namingSystem.Should().NotBeEmpty();
            namingSystem.Should().Contain("http://snomed.info/sct");
            namingSystem.Should().Contain("2.16.840.1.113883.6.96");
        }

    }

    [TestClass]
    public class ResolveBestCandidateByIdTest
    {
        private static PackageFileReference entry(string file, string type, string id, string? pkg = null, string? version = null)
        {
            var r = new PackageFileReference(file) { ResourceType = type, Id = id };
            if (pkg is not null) r.Package = new PackageReference(pkg, version);
            return r;
        }

        private static PackageClosure closure(params (string name, string version)[] pkgs)
        {
            var c = new PackageClosure();
            foreach (var (name, version) in pkgs) c.Add(new PackageReference(name, version));
            return c;
        }

        private static FileIndex index(params PackageFileReference[] entries)
        {
            var i = new FileIndex();
            i.AddRange(entries);
            return i;
        }

        [TestMethod]
        public void Prefers_highest_version_when_lower_is_first_in_index()
        {
            var idx = index(entry("a.json", "Basic", "shared", "pkg", "1.0.0"), entry("b.json", "Basic", "shared", "pkg", "2.0.0"));
            idx.ResolveBestCandidateById("Basic", "shared", closure(("pkg", "1.0.0"), ("pkg", "2.0.0")))!.FilePath.Should().Be("b.json");
        }

        [TestMethod]
        public void Prefers_highest_version_when_higher_is_first_in_index()
        {
            var idx = index(entry("b.json", "Basic", "shared", "pkg", "2.0.0"), entry("a.json", "Basic", "shared", "pkg", "1.0.0"));
            idx.ResolveBestCandidateById("Basic", "shared", closure(("pkg", "1.0.0"), ("pkg", "2.0.0")))!.FilePath.Should().Be("b.json");
        }

        [TestMethod]
        public void Project_file_wins_over_package_entries_even_when_last_in_index()
        {
            var idx = index(entry("a.json", "Basic", "shared", "pkg", "1.0.0"), entry("b.json", "Basic", "shared", "pkg", "2.0.0"), entry("proj.json", "Basic", "shared"));
            idx.ResolveBestCandidateById("Basic", "shared", closure(("pkg", "1.0.0"), ("pkg", "2.0.0")))!.FilePath.Should().Be("proj.json");
        }

        [TestMethod]
        public void Returns_first_in_index_order_across_different_packages()
        {
            var idx = index(entry("x.json", "Basic", "shared", "pkgx", "1.0.0"), entry("y.json", "Basic", "shared", "pkgy", "1.0.0"));
            idx.ResolveBestCandidateById("Basic", "shared", closure(("pkgx", "1.0.0"), ("pkgy", "1.0.0")))!.FilePath.Should().Be("x.json");
        }

        [TestMethod]
        public void Returns_null_when_match_exists_only_in_lower_version()
        {
            var idx = index(entry("a.json", "Basic", "shared", "pkg", "1.0.0"), entry("b.json", "Basic", "other", "pkg", "2.0.0"));
            idx.ResolveBestCandidateById("Basic", "shared", closure(("pkg", "1.0.0"), ("pkg", "2.0.0"))).Should().BeNull();
        }

        [TestMethod]
        public void Returns_match_in_single_version_scope()
        {
            var idx = index(entry("a.json", "Basic", "shared", "pkg", "1.0.0"));
            idx.ResolveBestCandidateById("Basic", "shared", closure(("pkg", "1.0.0")))!.FilePath.Should().Be("a.json");
        }

        [TestMethod]
        public void Returns_null_when_no_entry_has_that_id()
        {
            var idx = index(entry("a.json", "Basic", "other", "pkg", "1.0.0"));
            idx.ResolveBestCandidateById("Basic", "shared", closure(("pkg", "1.0.0"))).Should().BeNull();
        }

        [TestMethod]
        public void Returns_null_when_id_matches_but_resource_type_differs()
        {
            var idx = index(entry("a.json", "Patient", "shared", "pkg", "1.0.0"));
            idx.ResolveBestCandidateById("Basic", "shared", closure(("pkg", "1.0.0"))).Should().BeNull();
        }

        [TestMethod]
        public void Returns_null_when_entry_package_is_not_in_closure()
        {
            var idx = index(entry("a.json", "Basic", "shared", "stranger", "1.0.0"));
            idx.ResolveBestCandidateById("Basic", "shared", closure(("pkg", "1.0.0"))).Should().BeNull();
        }
    }

    [TestClass]
    public class GetFileContentByIdTest
    {
        private string _root = null!;

        [TestInitialize]
        public void Setup() => _root = Path.Combine(Path.GetTempPath(), $"packages-lib-tests-byid-{Guid.NewGuid()}");

        [TestCleanup]
        public void Cleanup()
        {
            try { Directory.Delete(_root, true); } catch (DirectoryNotFoundException) { }
        }

        private static string basic(string marker) => $"{{\"resourceType\":\"Basic\",\"id\":\"shared\",\"text\":\"{marker}\"}}";

        private void writePackage(string name, string version, string marker)
        {
            var dir = Path.Combine(_root, "cache", $"{name}#{version}", "package");
            Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, "package.json"), $"{{\"name\":\"{name}\",\"version\":\"{version}\"}}");
            File.WriteAllText(Path.Combine(dir, "Basic-shared.json"), basic(marker));
        }

        private async Task<PackageContext> context(bool lockFile, string? projectMarker, params (string name, string version)[] pkgs)
        {
            var projectDir = Path.Combine(_root, "project");
            Directory.CreateDirectory(projectDir);
            if (projectMarker is not null) File.WriteAllText(Path.Combine(projectDir, "Basic-shared.json"), basic(projectMarker));
            var project = new FolderProject(projectDir);
            if (lockFile)
            {
                var closure = new PackageClosure();
                foreach (var (n, v) in pkgs) closure.Add(new PackageReference(n, v));
                await project.WriteClosure(closure);
            }
            return new PackageContext(new DiskPackageCache(Path.Combine(_root, "cache")), project, null);
        }

        [TestMethod]
        public async Task Returns_content_of_highest_version_when_two_versions_hold_the_id()
        {
            writePackage("pkg", "1.0.0", "low");
            writePackage("pkg", "2.0.0", "high");
            var ctx = await context(true, null, ("pkg", "1.0.0"), ("pkg", "2.0.0"));
            (await ctx.GetFileContentById("Basic", "shared")).Should().Contain("high");
        }

        [TestMethod]
        public async Task Returns_project_file_content_over_package_versions()
        {
            writePackage("pkg", "1.0.0", "low");
            writePackage("pkg", "2.0.0", "high");
            var ctx = await context(true, "project", ("pkg", "1.0.0"), ("pkg", "2.0.0"));
            (await ctx.GetFileContentById("Basic", "shared")).Should().Contain("project");
        }

        [TestMethod]
        public async Task Returns_content_with_one_version_per_package()
        {
            writePackage("pkga", "1.0.0", "a");
            writePackage("pkgb", "1.0.0", "b");
            var ctx = await context(true, null, ("pkga", "1.0.0"), ("pkgb", "1.0.0"));
            (await ctx.GetFileContentById("Basic", "shared")).Should().Contain("\"a\"");
        }

        [TestMethod]
        public async Task Throws_missing_lock_file_error_when_project_has_no_lock_file()
        {
            var ctx = await context(false, null);
            var act = () => ctx.GetFileContentById("Basic", "shared");
            (await act.Should().ThrowAsync<Exception>()).Which.ToString().Should().Contain("The folder does not contain a package lock file.");
        }
    }
}
