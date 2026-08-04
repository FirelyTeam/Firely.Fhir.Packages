using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Firely.Fhir.Packages.Tests
{
    /// <summary>
    /// A minimal <see cref="IPackageServer"/> stub that resolves whatever <see cref="Versions"/> is configured
    /// for a given package name, without any network or disk access.
    /// </summary>
    internal class FakePackageServer(Dictionary<string, Versions> versions) : IPackageServer
    {
        public Task<Versions?> GetVersions(string name)
        {
            return Task.FromResult(versions.TryGetValue(name, out var v) ? v : null);
        }

        public Task<byte[]> GetPackage(PackageReference reference) => Task.FromResult(System.Array.Empty<byte>());
    }

    [TestClass]
    public class PackageAliasTests
    {
        [TestMethod]
        public void ParsesNpmAliasFromManifestKey()
        {
            PackageDependency dep = new KeyValuePair<string, string?>("uscore610@npm:hl7.fhir.us.core", "6.1.0");

            dep.Alias.Should().Be("uscore610");
            dep.Name.Should().Be("hl7.fhir.us.core", "Name holds the real package, not the alias-qualified key");
            dep.Range.Should().Be("6.1.0");
        }

        [TestMethod]
        public void PlainManifestKeyHasNoAlias()
        {
            PackageDependency dep = new KeyValuePair<string, string?>("hl7.fhir.us.core", "7.0.0");

            dep.Alias.Should().BeNull();
            dep.Name.Should().Be("hl7.fhir.us.core");
            dep.Range.Should().Be("7.0.0");
        }

        [TestMethod]
        public void ManifestDependenciesParseAliases()
        {
            var manifest = new PackageManifest("root", "0.1.0")
            {
                Dependencies = new Dictionary<string, string?>
                {
                    ["hl7.fhir.us.core"] = "7.0.0",
                    ["uscore610@npm:hl7.fhir.us.core"] = "6.1.0",
                }
            };

            var deps = manifest.GetDependencies().ToList();

            deps.Should().HaveCount(2);
            deps.Should().ContainSingle(d => d.Alias == null && d.Name == "hl7.fhir.us.core" && d.Range == "7.0.0");
            deps.Should().ContainSingle(d => d.Alias == "uscore610" && d.Name == "hl7.fhir.us.core" && d.Range == "6.1.0");
        }

        [TestMethod]
        public void PlainReferenceMatchingAnAliasedVersionExactlyIsRejectedAsDuplicate()
        {
            // An alias is not part of a closure entry's identity: if an aliased entry already represents this
            // exact (name, version), a plain copy of it is a redundant duplicate, not a distinct package to
            // keep alongside it.
            var closure = new PackageClosure();

            closure.Add(new PackageReference { Name = "hl7.fhir.us.core", Version = "6.1.0", Alias = "uscore610" })
                .Should().BeTrue();
            closure.Add(new PackageReference("hl7.fhir.us.core", "6.1.0"))
                .Should().BeFalse("this is the same resolved content as the existing aliased entry");

            closure.References.Should().ContainSingle();
        }

        [TestMethod]
        public void PlainMissingMatchingAnAliasedRangeExactlyIsRejectedAsDuplicate()
        {
            // Missing-side twin of PlainReferenceMatchingAnAliasedVersionExactlyIsRejectedAsDuplicate.
            var closure = new PackageClosure();

            closure.AddMissing(new PackageDependency("hl7.fhir.us.core", "6.1.0") { Alias = "uscore610" });
            closure.AddMissing(new PackageDependency("hl7.fhir.us.core", "6.1.0"));

            closure.Missing.Should().ContainSingle();
        }

        [TestMethod]
        public void AliasedReferenceRejectsExactDuplicate()
        {
            var closure = new PackageClosure();
            var aliased = new PackageReference { Name = "hl7.fhir.us.core", Version = "6.1.0", Alias = "uscore610" };

            closure.Add(aliased).Should().BeTrue();
            closure.Add(aliased).Should().BeFalse();

            closure.References.Should().ContainSingle();
        }

        [TestMethod]
        public void TwoDifferentAliasesForSameNameAndVersionAreDeduplicated()
        {
            // Two different aliases resolving to the identical (name, version) convey no extra information -
            // only the first one reached should be kept, to avoid duplicating restore work and index entries.
            var closure = new PackageClosure();

            closure.Add(new PackageReference { Name = "hl7.fhir.us.core", Version = "6.1.0", Alias = "uscoreA" })
                .Should().BeTrue();
            closure.Add(new PackageReference { Name = "hl7.fhir.us.core", Version = "6.1.0", Alias = "uscoreB" })
                .Should().BeFalse("a second alias for the same resolved package is a duplicate, not a new entry");

            closure.References.Should().ContainSingle()
                .Which.Alias.Should().Be("uscoreA", "the first alias to reach this (name, version) wins");
        }

        [TestMethod]
        public void TwoDifferentAliasesForSameNameAndRangeInMissingAreDeduplicated()
        {
            // Missing-side twin of TwoDifferentAliasesForSameNameAndVersionAreDeduplicated.
            var closure = new PackageClosure();

            closure.AddMissing(new PackageDependency("hl7.fhir.us.core", "6.1.0") { Alias = "uscoreA" });
            closure.AddMissing(new PackageDependency("hl7.fhir.us.core", "6.1.0") { Alias = "uscoreB" });

            closure.Missing.Should().ContainSingle()
                .Which.Alias.Should().Be("uscoreA", "the first alias to reach this (name, range) wins");
        }

        [TestMethod]
        public void AcceptMultipleKeepsPlainAndAliasedMissingEntriesTogether()
        {
            // Missing-side twin of PackageAliasCornerCaseTests.AcceptMultipleKeepsPlainAndAliasedVersionsTogether.
            var closure = new PackageClosure();

            closure.AddMissing(new PackageDependency("hl7.fhir.us.core", "5.0.0"));
            closure.AddMissing(new PackageDependency("hl7.fhir.us.core", "7.0.0"));
            closure.AddMissing(new PackageDependency("hl7.fhir.us.core", "6.1.0") { Alias = "uscore610" });

            closure.Missing
                .Where(m => m.Name == "hl7.fhir.us.core")
                .Select(m => m.Range)
                .Should().BeEquivalentTo(new[] { "5.0.0", "7.0.0", "6.1.0" });
        }

        [TestMethod]
        public void FindReturnsHighestVersionRegardlessOfInsertionOrder()
        {
            // Insertion order carries no meaning and cannot be predicted, so Find must not depend on it:
            // it returns the highest version among all matching entries, whichever order they arrived in.
            var addedPlainFirst = new PackageClosure();
            addedPlainFirst.Add(new PackageReference("hl7.fhir.us.core", "7.0.0"));
            addedPlainFirst.Add(new PackageReference { Name = "hl7.fhir.us.core", Version = "6.1.0", Alias = "uscore610" });

            var addedAliasedFirst = new PackageClosure();
            addedAliasedFirst.Add(new PackageReference { Name = "hl7.fhir.us.core", Version = "6.1.0", Alias = "uscore610" });
            addedAliasedFirst.Add(new PackageReference("hl7.fhir.us.core", "7.0.0"));

            addedPlainFirst.Find("hl7.fhir.us.core", out var refA).Should().BeTrue();
            addedAliasedFirst.Find("hl7.fhir.us.core", out var refB).Should().BeTrue();

            refA.Version.Should().Be("7.0.0", "the highest version is returned, regardless of arrival order");
            refB.Version.Should().Be("7.0.0", "the highest version is returned, regardless of arrival order");
        }

        [TestMethod]
        public void FindReturnsHighestVersionEvenWhenItIsTheAliasedOne()
        {
            // Alias status must not matter to Find, only the version does: here the ALIASED entry is the
            // higher version, and it must win over the plain (non-aliased) one.
            var closure = new PackageClosure();
            closure.Add(new PackageReference("hl7.fhir.us.core", "6.1.0"));
            closure.Add(new PackageReference { Name = "hl7.fhir.us.core", Version = "7.0.0", Alias = "uscore700" });

            closure.Find("hl7.fhir.us.core", out var reference).Should().BeTrue();

            reference.Version.Should().Be("7.0.0", "the highest version wins even though it is the aliased entry");
        }

        [TestMethod]
        public void FindReturnsHighestAmongMultipleAliasedVersions()
        {
            var closure = new PackageClosure();
            closure.Add(new PackageReference { Name = "hl7.fhir.us.core", Version = "6.1.0", Alias = "uscore610" });
            closure.Add(new PackageReference { Name = "hl7.fhir.us.core", Version = "7.0.0", Alias = "uscore700" });

            closure.Find("hl7.fhir.us.core", out var reference).Should().BeTrue();

            reference.Version.Should().Be("7.0.0");
        }

        [TestMethod]
        public void FindAllReturnsEveryMatchingEntry()
        {
            var closure = new PackageClosure();
            closure.Add(new PackageReference("hl7.fhir.us.core", "7.0.0"));
            closure.Add(new PackageReference { Name = "hl7.fhir.us.core", Version = "6.1.0", Alias = "uscore610" });
            closure.Add(new PackageReference("hl7.fhir.uv.extensions", "1.0.0"));

            closure.FindAll("hl7.fhir.us.core")
                .Select(r => r.Version)
                .Should().BeEquivalentTo(new[] { "7.0.0", "6.1.0" });
        }

        [TestMethod]
        public void AliasRoundTripsThroughLockFile()
        {
            var folder = createTempFolder();
            var closure = new PackageClosure();
            closure.Add(new PackageReference("hl7.fhir.us.core", "7.0.0"));
            closure.Add(new PackageReference { Name = "hl7.fhir.us.core", Version = "6.1.0", Alias = "uscore610" });

            LockFile.WriteToFolder(closure, folder);

            var content = File.ReadAllText(Path.Combine(folder, PackageFileNames.LOCKFILE));
            content.Should().Contain("\"alias\": \"uscore610\"");
            content.Should().Contain("\"name\": \"hl7.fhir.us.core\"", "the real name is stored, not the alias-qualified key");

            var roundtripped = LockFile.ReadFromFolder(folder);
            var aliased = roundtripped!.References.Single(r => r.Alias == "uscore610"); 
            aliased.Name.Should().Be("hl7.fhir.us.core");
            aliased.Version.Should().Be("6.1.0");
        }

        [TestMethod]
        public void AliasParsesFromATransitivePackagesOwnManifest()
        {
            // GetDependencies() doesn't know or care whose manifest it's reading. Unlike SUSHI/IG Publisher
            // (which only honor @npm: at the top-level dependsOn/config list), our parsing happens uniformly
            // for every manifest the restorer processes - including a transitively-installed package's own
            // manifest. This documents that (intentional) divergence.
            var nestedManifest = new PackageManifest("some.installed.package", "1.0.0")
            {
                Dependencies = new Dictionary<string, string?>
                {
                    ["uscore610@npm:hl7.fhir.us.core"] = "6.1.0"
                }
            };

            var deps = nestedManifest.GetDependencies().ToList();

            deps.Should().ContainSingle();
            deps[0].Alias.Should().Be("uscore610");
            deps[0].Name.Should().Be("hl7.fhir.us.core");
            deps[0].Range.Should().Be("6.1.0");
        }

        // ----- Range dependencies combined with an alias, through the real Resolve() construction site -----

        [TestMethod]
        public async Task AliasedRangeDependencyResolvesToConcreteVersionAndKeepsAlias()
        {
            var server = new FakePackageServer(new()
            {
                ["hl7.fhir.us.core"] = new Versions(["6.1.0", "6.1.1", "6.2.0"])
            });
            var dependency = new PackageDependency("hl7.fhir.us.core", "6.1.x") { Alias = "uscore61x" };

            var reference = await server.Resolve(dependency);

            reference.Found.Should().BeTrue();
            reference.Name.Should().Be("hl7.fhir.us.core");
            reference.Version.Should().Be("6.1.1", "the range must resolve to the highest matching concrete version");
            reference.Alias.Should().Be("uscore61x", "the alias travels with the resolved reference, not just the raw dependency");
        }

        [TestMethod]
        public async Task AliasedRangeThatCannotResolveGoesToMissingWithAlias()
        {
            var server = new FakePackageServer(new()
            {
                ["hl7.fhir.us.core"] = new Versions(["6.1.0"])
            });
            var dependency = new PackageDependency("hl7.fhir.us.core", "9.9.x") { Alias = "doesnotexist" };

            var reference = await server.Resolve(dependency);

            reference.NotFound.Should().BeTrue();

            var closure = new PackageClosure();
            closure.AddMissing(dependency);

            closure.Missing.Should().ContainSingle();
            closure.Missing[0].Alias.Should().Be("doesnotexist");
            closure.Missing[0].Range.Should().Be("9.9.x");
        }

        // ----- Manifest round-trip: AddDependency -> GetDependencies preserves the alias -----

        [TestMethod]
        public void AliasSurvivesManifestAddDependencyRoundTrip()
        {
            var manifest = new PackageManifest("root", "0.1.0");
            manifest.AddDependency("uscore610@npm:hl7.fhir.us.core", "6.1.0");

            var deps = manifest.GetDependencies().ToList();

            deps.Should().ContainSingle();
            deps[0].Alias.Should().Be("uscore610");
            deps[0].Name.Should().Be("hl7.fhir.us.core");
        }

        // ----- Mixing plain and aliased entries for the same real package under AcceptMultiple -----

        [TestMethod]
        public void AcceptMultipleKeepsPlainAndAliasedVersionsTogether()
        {
            var closure = new PackageClosure();

            closure.Add(new PackageReference("hl7.fhir.us.core", "5.0.0")).Should().BeTrue();
            closure.Add(new PackageReference("hl7.fhir.us.core", "7.0.0")).Should().BeTrue();
            closure.Add(new PackageReference { Name = "hl7.fhir.us.core", Version = "6.1.0", Alias = "uscore610" }).Should().BeTrue();

            closure.References
                .Where(r => r.Name == "hl7.fhir.us.core")
                .Select(r => r.Version)
                .Should().BeEquivalentTo(["5.0.0", "7.0.0", "6.1.0"]);
        }

        // ----- Same alias string used for two different real packages: identity is (name, version), not alias -----

        [TestMethod]
        public void SameAliasForTwoDifferentRealPackagesBothSurvive()
        {
            var closure = new PackageClosure();

            closure.Add(new PackageReference { Name = "hl7.fhir.us.core", Version = "6.1.0", Alias = "myalias" }).Should().BeTrue();
            closure.Add(new PackageReference { Name = "hl7.fhir.uv.extensions", Version = "1.0.0", Alias = "myalias" }).Should().BeTrue();

            closure.References.Should().HaveCount(2, "identity is (name, version); the alias string is not required to be globally unique");
        }

        // ----- Malformed / edge-case alias syntax: document current behavior, no crashes -----

        [TestMethod]
        public void EmptyAliasBeforeSeparatorIsParsedAsEmptyStringNotNull()
        {
            PackageDependency dep = new KeyValuePair<string, string?>("@npm:hl7.fhir.us.core", "6.1.0");

            dep.Alias.Should().Be("", "the text before '@npm:' is empty, but Alias is set to empty string, not null");
            dep.Name.Should().Be("hl7.fhir.us.core");
            // Because Alias is "" (not null), this dependency is still treated as aliased downstream
            // (Add/AddMissing check `Alias is not null`), which is worth being aware of.
        }

        [TestMethod]
        public void EmptyRealNameAfterSeparatorIsParsedAsEmptyStringDoesNotThrow()
        {
            PackageDependency dep = new KeyValuePair<string, string?>("uscore610@npm:", "6.1.0");

            dep.Alias.Should().Be("uscore610");
            dep.Name.Should().Be("", "no package name follows '@npm:', but parsing does not throw");
        }

        [TestMethod]
        public void UppercaseNpmMarkerIsNotRecognizedAsAnAlias()
        {
            // The "@npm:" marker is matched case-sensitively, consistent with both SUSHI's regex and
            // IG Publisher's literal indexOf("@npm:") - this is intentional ecosystem-consistent behavior,
            // not a gap.
            PackageDependency dep = new KeyValuePair<string, string?>("uscore610@NPM:hl7.fhir.us.core", "6.1.0");

            dep.Alias.Should().BeNull();
            dep.Name.Should().Be("uscore610@NPM:hl7.fhir.us.core", "an unrecognized marker leaves the whole key as a literal (if unusual) package name");
        }

        private static string createTempFolder()
        {
            var folder = Path.Combine(Path.GetTempPath(), "fhirpkg-alias-tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(folder);
            return folder;
        }
    }
}
