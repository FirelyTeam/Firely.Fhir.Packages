using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace Firely.Fhir.Packages.Tests
{
    [TestClass]
    public class PackageClosureTests
    {
        [TestMethod]
        public void DefaultStrategyIsHighestWins()
        {
            var closure = new PackageClosure();
            closure.ConflictResolution.Should().Be(ConflictResolutionStrategy.HighestWins);
        }

        [TestMethod]
        public void AddingExactDuplicateIsRejected()
        {
            var closure = new PackageClosure();

            closure.Add("example@1.0.0").Should().BeTrue();
            closure.Add("example@1.0.0").Should().BeFalse();

            closure.References.Should().ContainSingle();
        }

        [TestMethod]
        public void HighestWinsKeepsSingleReferencePerName()
        {
            var closure = new PackageClosure(ConflictResolutionStrategy.HighestWins);

            closure.Add("example@1.0.0").Should().BeTrue();
            closure.Add("example@2.0.0").Should().BeTrue("a higher version replaces the existing one");

            closure.References.Should().ContainSingle()
                .Which.Version.Should().Be("2.0.0");
        }

        [TestMethod]
        public void HighestWinsIgnoresLowerVersion()
        {
            var closure = new PackageClosure(ConflictResolutionStrategy.HighestWins);

            closure.Add("example@2.0.0").Should().BeTrue();
            closure.Add("example@1.0.0").Should().BeFalse("a lower version does not replace the existing one");

            closure.References.Should().ContainSingle()
                .Which.Version.Should().Be("2.0.0");
        }

        [TestMethod]
        public void AcceptMultipleKeepsAllVersions()
        {
            var closure = new PackageClosure(ConflictResolutionStrategy.AcceptMultiple);

            closure.Add("example@1.0.0").Should().BeTrue();
            closure.Add("example@2.0.0").Should().BeTrue("a different version is kept alongside the existing one");

            closure.References.Should().HaveCount(2);
            closure.References.Select(r => r.Version).Should().BeEquivalentTo(new[] { "1.0.0", "2.0.0" });
        }

        [TestMethod]
        public void AcceptMultipleStillRejectsExactDuplicates()
        {
            var closure = new PackageClosure(ConflictResolutionStrategy.AcceptMultiple);

            closure.Add("example@1.0.0").Should().BeTrue();
            closure.Add("example@1.0.0").Should().BeFalse();

            closure.References.Should().ContainSingle();
        }

        [TestMethod]
        public void AcceptMultipleTreatsNameCaseInsensitively()
        {
            var closure = new PackageClosure(ConflictResolutionStrategy.AcceptMultiple);

            closure.Add("Example@1.0.0").Should().BeTrue();
            closure.Add("example@1.0.0").Should().BeFalse("names differing only by case are the same package");

            closure.References.Should().ContainSingle();
        }

        [TestMethod]
        public void AlwaysWritesListFormAndRoundTrips()
        {
            var folder = createTempFolder();
            var closure = new PackageClosure();
            closure.Add("example@1.0.0");
            closure.Add("other@2.0.0");

            LockFile.WriteToFolder(closure, folder);

            var content = File.ReadAllText(Path.Combine(folder, PackageFileNames.LOCKFILE));
            // list form even for a single-version closure
            content.Should().Contain("\"name\": \"example\"");
            content.Should().Contain("\"lockFileVersion\": 2");
            content.Should().NotContain("\"example\": \"1.0.0\"", "the object form should no longer be written");

            var roundtripped = LockFile.ReadFromFolder(folder);
            roundtripped.Should().NotBeNull();
            roundtripped!.References.Should().HaveCount(2);
        }

        [TestMethod]
        public void MultipleVersionsWriteListFormAndRoundTrip()
        {
            var folder = createTempFolder();
            var closure = new PackageClosure(ConflictResolutionStrategy.AcceptMultiple);
            closure.Add("example@1.0.0");
            closure.Add("example@2.0.0");
            closure.Add("other@3.0.0");

            LockFile.WriteToFolder(closure, folder);

            var content = File.ReadAllText(Path.Combine(folder, PackageFileNames.LOCKFILE));
            // list form: entries carry explicit name/version fields
            content.Should().Contain("\"name\": \"example\"");

            var roundtripped = LockFile.ReadFromFolder(folder);
            roundtripped.Should().NotBeNull();
            roundtripped!.References
                .Where(r => string.Equals(r.Name, "example", StringComparison.OrdinalIgnoreCase))
                .Select(r => r.Version)
                .Should().BeEquivalentTo(new[] { "1.0.0", "2.0.0" });
        }

        [TestMethod]
        public void ReadsLegacyObjectFormLockFile()
        {
            var folder = createTempFolder();
            var legacy = """
                {
                  "updated": "2024-01-01T00:00:00+00:00",
                  "dependencies": {
                    "hl7.fhir.r4.core": "4.0.1",
                    "hl7.fhir.us.core": "6.1.0"
                  },
                  "missing": {}
                }
                """;
            File.WriteAllText(Path.Combine(folder, PackageFileNames.LOCKFILE), legacy);

            var closure = LockFile.ReadFromFolder(folder);

            closure.Should().NotBeNull();
            closure!.References.Should().HaveCount(2);
            closure.Find("hl7.fhir.us.core", out var reference).Should().BeTrue();
            reference.Version.Should().Be("6.1.0");
        }

        [TestMethod]
        public void CarinBbClosureWithTwoUsCoreVersionsRoundTrips()
        {
            // Modelled on the real IG hl7.fhir.us.carin-bb@2.2.0, which depends on two versions of
            // hl7.fhir.us.core simultaneously:
            //  - 7.0.0 directly, and
            //  - 6.1.0 transitively, via the reuse-wrapper package hl7.fhir.us.core.v610 (an empty shim
            //    whose only dependency is the real hl7.fhir.us.core@6.1.0).
            // The resolved closure therefore contains the real name hl7.fhir.us.core at BOTH versions,
            // which only the list-form lock file can represent.
            // See the dependency table at https://hl7.org/fhir/us/carin-bb/STU2.2/ (Dependencies section),
            // and the package manifest at https://packages.simplifier.net/hl7.fhir.us.carin-bb/2.2.0.
            var folder = createTempFolder();
            var closure = new PackageClosure(ConflictResolutionStrategy.AcceptMultiple);
            closure.Add("hl7.fhir.us.carin-bb@2.2.0");
            closure.Add("hl7.fhir.r4.core@4.0.1");
            closure.Add("hl7.fhir.us.core@7.0.0");        // direct
            closure.Add("hl7.fhir.us.core.v610@6.1.0");   // empty reuse wrapper (distinct package name)
            closure.Add("hl7.fhir.us.core@6.1.0");        // real 6.1.0 content, pulled in via the wrapper
            closure.Add("hl7.terminology.r4@6.1.0");

            closure.References.Should().HaveCount(6, "AcceptMultiple keeps both real us.core versions");

            LockFile.WriteToFolder(closure, folder);
            var roundtripped = LockFile.ReadFromFolder(folder);

            roundtripped.Should().NotBeNull();

            // both versions of the real us.core package survive the round trip
            roundtripped!.References
                .Where(r => string.Equals(r.Name, "hl7.fhir.us.core", StringComparison.OrdinalIgnoreCase))
                .Select(r => r.Version)
                .Should().BeEquivalentTo(["7.0.0", "6.1.0"]);

            // the empty wrapper is preserved as its own distinct entry
            roundtripped.References
                .Any(r => string.Equals(r.Name, "hl7.fhir.us.core.v610", StringComparison.OrdinalIgnoreCase))
                .Should().BeTrue();

            roundtripped.References.Should().HaveCount(closure.References.Count, "nothing is lost in the round trip");
        }

        [TestMethod]
        public void HighestWinsCollapsesMissingToHighestPerName()
        {
            // Regression for issue #183 (System.ArgumentException "An item with the same key has already
            // been added. Key: hl7.terminology.r4"), reproduced with the NGS TW IG (tw.gov.mohw.nhi.ngs@1.0.0,
            // https://nhicore.nhi.gov.tw/ngs/package.tgz): with no package server, its unresolved transitive
            // deps land in "missing" with the same package name at multiple ranges (6.5.0 direct, 6.1.0 + 5.0.0
            // transitive). Under HighestWins the closure keeps one entry per name (highest) in Missing just as
            // it does in References, so the content matches the historic single-version-per-name model.
            var closure = new PackageClosure(ConflictResolutionStrategy.HighestWins);
            closure.AddMissing(new PackageDependency("hl7.terminology.r4", "6.1.0"));
            closure.AddMissing(new PackageDependency("hl7.terminology.r4", "6.5.0"));
            closure.AddMissing(new PackageDependency("hl7.terminology.r4", "5.0.0"));

            closure.Missing.Should().ContainSingle()
                .Which.Range.Should().Be("6.5.0", "HighestWins keeps a single entry per name, the highest range");
        }

        [TestMethod]
        public void AcceptMultipleKeepsEveryMissingRangeAndRoundTrips()
        {
            // Under AcceptMultiple the same package name is kept at every range in Missing, and the list-form
            // lock serializes all of them (the old name-keyed map threw on the duplicate key - see #183).
            var folder = createTempFolder();
            var closure = new PackageClosure(ConflictResolutionStrategy.AcceptMultiple);
            closure.AddMissing(new PackageDependency("hl7.terminology.r4", "6.5.0"));
            closure.AddMissing(new PackageDependency("hl7.terminology.r4", "6.1.0"));
            closure.AddMissing(new PackageDependency("hl7.terminology.r4", "5.0.0"));

            var write = () => LockFile.WriteToFolder(closure, folder);
            write.Should().NotThrow("the list-form lock represents duplicate names, unlike the old name-keyed map");

            var roundtripped = LockFile.ReadFromFolder(folder);
            roundtripped.Should().NotBeNull();
            roundtripped!.Missing
                .Where(m => m.Name == "hl7.terminology.r4")
                .Select(m => m.Range)
                .Should().BeEquivalentTo(["6.5.0", "6.1.0", "5.0.0"]);
        }

        [TestMethod]
        public void UnimplementedStrategyThrowsOnAddInsteadOfSilentlyDefaulting()
        {
            // A future ConflictResolutionStrategy value that Add()/AddMissing() haven't been updated to
            // handle must fail loudly, not silently behave like HighestWins (see the exhaustive switch).
            var closure = new PackageClosure((ConflictResolutionStrategy)999);

            var add = () => closure.Add(new PackageReference("example", "1.0.0"));

            add.Should().Throw<NotImplementedException>().WithMessage("*999*");
        }

        [TestMethod]
        public void UnimplementedStrategyThrowsOnAddMissingInsteadOfSilentlyDefaulting()
        {
            var closure = new PackageClosure((ConflictResolutionStrategy)999);

            var addMissing = () => closure.AddMissing(new PackageDependency("example", "1.0.0"));

            addMissing.Should().Throw<NotImplementedException>().WithMessage("*999*");
        }

        [TestMethod]
        public void ReadingNewerLockFileVersionThrows()
        {
            var folder = createTempFolder();
            var future = """
                {
                  "updated": "2024-01-01T00:00:00+00:00",
                  "lockFileVersion": 999,
                  "dependencies": {}
                }
                """;
            File.WriteAllText(Path.Combine(folder, PackageFileNames.LOCKFILE), future);

            var read = () => LockFile.ReadFromFolder(folder);

            read.Should().Throw<NotSupportedException>().WithMessage("*999*");
        }

        private static string createTempFolder()
        {
            var folder = Path.Combine(Path.GetTempPath(), "fhirpkg-tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(folder);
            return folder;
        }
    }
}
