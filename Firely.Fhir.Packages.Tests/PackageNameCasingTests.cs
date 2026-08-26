using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Firely.Fhir.Packages.Tests
{
    /// <summary>
    /// FHIR package ids are lowercase. Whatever casing reaches the library - typed on a command
    /// line, or published in a third-party manifest such as kbv.ita.erp's "KBV.Basis" dependency -
    /// must not survive into a manifest, a lock file, a cache folder name, or package identity.
    /// </summary>
    [TestClass]
    public class PackageNameCasingTests
    {
        private const string MIXED = "NiCtIz.FhIr.Nl.StU3.ZiB2017";
        private const string LOWER = "nictiz.fhir.nl.stu3.zib2017";

        [TestMethod]
        public void ReferenceAndDependencyLowercaseTheName()
        {
            new PackageReference(MIXED, "2.1.4").Name.Should().Be(LOWER);
            new PackageDependency(MIXED, "2.x").Name.Should().Be(LOWER);

            PackageReference.Parse($"{MIXED}@2.1.4").Name.Should().Be(LOWER);
            ((PackageReference)$"{MIXED}@2.1.4").Name.Should().Be(LOWER);
            ((PackageDependency)$"{MIXED}@2.x").Name.Should().Be(LOWER);
        }

        [TestMethod]
        public void ReferencesDifferingOnlyInCaseAreTheSamePackage()
        {
            var mixed = new PackageReference(MIXED, "2.1.4");
            var lower = new PackageReference(LOWER, "2.1.4");

            mixed.Should().Be(lower);
            (mixed == lower).Should().BeTrue();
            mixed.GetHashCode().Should().Be(lower.GetHashCode());
            mixed.Moniker.Should().Be($"{LOWER}@2.1.4");
        }

        [TestMethod]
        public void MixedCaseTransitiveDependencyIsNormalised()
        {
            // A manifest published by someone else, declaring its dependency in mixed case.
            var manifest = new PackageManifest("kbv.ita.erp", "1.1.2");
            manifest.AddDependency(new PackageDependency("KBV.Basis", "1.3.0"));

            manifest.Dependencies.Should().ContainKey("kbv.basis");
            manifest.GetDependencies().Should().OnlyContain(d => d.Name == "kbv.basis");
        }

        [TestMethod]
        public void CachedContentFolderIsLowercase()
        {
            new DiskPackageCache("cache")
                .PackageContentFolder(new PackageReference(MIXED, "2.1.4"))
                .Should().Contain($"{LOWER}#2.1.4");
        }
    }
}
