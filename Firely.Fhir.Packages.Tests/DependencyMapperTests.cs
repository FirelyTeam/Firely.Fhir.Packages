using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Firely.Fhir.Packages.Tests
{
    [TestClass]
    public class DependencyMapperTests
    {
        // The cache and the project are empty and there is no server, so nothing can be resolved: the
        // dependencies that are reported as missing are the ones that the restore tried to resolve,
        // i.e. after mapping. This keeps these tests independent of the network.
        private static async Task<PackageContext> createEmptyContext(params string[] dependencies)
        {
            var projectDir = await TestHelper.InitializeTemporary("dependency-mapper", dependencies);
            var cacheDir = Path.Combine(Path.GetTempPath(), $"packages-lib-tests-cache-{Guid.NewGuid()}");
            Directory.CreateDirectory(cacheDir);

            return new PackageContext(new DiskPackageCache(cacheDir), new FolderProject(projectDir), server: null);
        }

        [TestMethod]
        public async Task RestoreRedirectsR4CoreByDefault()
        {
            var context = await createEmptyContext("hl7.fhir.r4.core@4.0.0", "some.other.package@1.0.0");

            var closure = await context.Restore();

            closure.Missing.Select(d => d.ToString()).Should().BeEquivalentTo(
                "hl7.fhir.r4.core (4.0.1)", "some.other.package (1.0.0)");
        }

        [TestMethod]
        public async Task RestoreKeepsAliasWhenRedirectingByDefault()
        {
            // Manifest dependencies are keyed by their (aliased) name, so write the aliased entry directly.
            var context = await createEmptyContext();
            var manifest = (await context.Project.ReadManifest())!;
            manifest.Dependencies = new() { ["core400@npm:hl7.fhir.r4.core"] = "4.0.0" };
            await context.Project.WriteManifest(manifest);

            var closure = await context.Restore();

            var missing = closure.Missing.Should().ContainSingle().Subject;
            missing.ToString().Should().Be("hl7.fhir.r4.core (4.0.1)");
            missing.Alias.Should().Be("core400");
        }

        [TestMethod]
        public async Task RestoreWithIdentityMapperDoesNotRedirect()
        {
            var context = await createEmptyContext("hl7.fhir.r4.core@4.0.0", "some.other.package@1.0.0");

            var closure = await context.Restore(dependency => dependency);

            closure.Missing.Select(d => d.ToString()).Should().BeEquivalentTo(
                "hl7.fhir.r4.core (4.0.0)", "some.other.package (1.0.0)");
        }

        [TestMethod]
        public async Task RestoreWithCustomMapperRedirects()
        {
            var context = await createEmptyContext("hl7.fhir.r4.core@4.0.0", "some.other.package@1.0.0");

            var closure = await context.Restore(dependency =>
                dependency.Name == "some.other.package" ? new PackageDependency("some.renamed.package", "2.0.0") : dependency);

            closure.Missing.Select(d => d.ToString()).Should().BeEquivalentTo(
                "hl7.fhir.r4.core (4.0.0)", "some.renamed.package (2.0.0)");
        }

        [TestMethod]
        public async Task RestoreWithNullMapperUsesDefault()
        {
            var context = await createEmptyContext("hl7.fhir.r4.core@4.0.0");

            var closure = await context.Restore(dependencyMapper: null);

            closure.Missing.Select(d => d.ToString()).Should().BeEquivalentTo("hl7.fhir.r4.core (4.0.1)");
        }

        [TestMethod]
        public void DefaultMapperOnlyTouchesR4Core400()
        {
            PackageRestorer.DefaultDependencyMapper(new PackageDependency("hl7.fhir.r4.core", "4.0.0")).Range.Should().Be("4.0.1");
            PackageRestorer.DefaultDependencyMapper(new PackageDependency("hl7.fhir.r4.core", "4.0.1")).Range.Should().Be("4.0.1");
            PackageRestorer.DefaultDependencyMapper(new PackageDependency("hl7.fhir.r4b.core", "4.0.0")).Range.Should().Be("4.0.0");
        }

        [TestMethod]
        public void DefaultMapperPreservesAlias()
        {
            var mapped = PackageRestorer.DefaultDependencyMapper(new PackageDependency("hl7.fhir.r4.core", "4.0.0") { Alias = "core400" });

            mapped.Range.Should().Be("4.0.1");
            mapped.Alias.Should().Be("core400");
        }
    }
}
