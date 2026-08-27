using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

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
        public void AddDependencyStringOverloadLowercasesTheKey()
        {
            // Public overload that writes the dictionary key directly, bypassing PackageDependency.
            var manifest = new PackageManifest("ft56.e2e", "0.1.0");
            manifest.AddDependency("KBV.Basis", "1.3.0");

            manifest.Dependencies.Should().ContainKey("kbv.basis");
            manifest.HasDependency("KBV.Basis").Should().BeTrue("lookups match case-insensitively");
            manifest.RemoveDependency("KBV.Basis").Should().BeTrue();
        }

        [TestMethod]
        public void ScopeIsLowercasedToo()
        {
            // An npm scope is part of the package id, and ends up in the registry URL.
            var scoped = PackageReference.Parse("@MyScope/MyPkg@1.0.0");

            scoped.Scope.Should().Be("myscope");
            scoped.Name.Should().Be("mypkg");
            scoped.GetNpmName().Should().Be("@myscope%2Fmypkg");
        }

        [TestMethod]
        public void ReferenceEqualityIgnoresCaseOnScopeAndName()
        {
            // Identity must not depend on where the name came from. A name read out of a third-party
            // manifest or a cache folder never passes through the normalising constructor.
            var mixed = new PackageReference(MIXED, "2.1.4");
            var lower = new PackageReference(LOWER, "2.1.4");

            mixed.Equals(lower).Should().BeTrue();
            (mixed == lower).Should().BeTrue();
            (mixed != lower).Should().BeFalse();
            mixed.GetHashCode().Should().Be(lower.GetHashCode(), "GetHashCode must agree with Equals");

            PackageReference.Parse("@MyScope/pkg@1.0.0")
                .Should().Be(PackageReference.Parse("@myscope/PKG@1.0.0"));
        }

        [TestMethod]
        public void ReferenceEqualityStillDistinguishesScopeAndVersion()
        {
            // Scope is part of the package id, and was previously excluded from equality entirely.
            PackageReference.Parse("@scopea/pkg@1.0.0")
                .Should().NotBe(PackageReference.Parse("@scopeb/pkg@1.0.0"));

            // Versions are compared case-sensitively: semver pre-release identifiers are.
            new PackageReference("pkg", "1.0.0-RC1")
                .Should().NotBe(new PackageReference("pkg", "1.0.0-rc1"));
        }

        [TestMethod]
        public void HashSetDeduplicatesCaseVariantReferences()
        {
            var set = new HashSet<PackageReference>
            {
                new PackageReference(MIXED, "2.1.4"),
                new PackageReference(LOWER, "2.1.4"),
            };

            set.Should().HaveCount(1);
        }

        [TestMethod]
        public void DependencyEqualityIgnoresCaseOnName()
        {
            // PackageDependency had no Equals at all, so it fell back to ordinal struct equality.
            new PackageDependency(MIXED, "2.x").Equals(new PackageDependency(LOWER, "2.x"))
                .Should().BeTrue();
            new PackageDependency(MIXED, "2.x").GetHashCode()
                .Should().Be(new PackageDependency(LOWER, "2.x").GetHashCode());

            new PackageDependency(LOWER, "2.x").Equals(new PackageDependency(LOWER, "3.x"))
                .Should().BeFalse("the range is still part of identity");
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
