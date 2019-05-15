using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hl7.Fhir.Packages.Tests
{
    [TestClass]
    public class VersionsTest
    {
        [TestMethod]
        public void Resolve_PackageDependencyExistsInVersionList_FixedVersionSearchFindsVersion()
        {
            var target = new Versions(new string[]{"1.0.0", "2.0.0"});

            PackageReference result = target.Resolve(new PackageDependency("SomeName", "1.0.0"));

            Assert.IsTrue(result.Found);
            Assert.AreEqual("1.0.0", result.Version);
        }

        [TestMethod]
        public void Resolve_PackageDependencyExistsInVersionList_RangedVersionSearchFindsVersion()
        {
            var target = new Versions(new string[] { "1.0.0", "2.0.0" });

            PackageReference result = target.Resolve(new PackageDependency("SomeName", "1.x"));

            Assert.IsTrue(result.Found);
            Assert.AreEqual("1.0.0", result.Version);
        }

        [TestMethod]
        public void Resolve_PackageDependencyDoesNotExistInVersionList_FixedVersionSearchReturnsNotFound()
        {
            var target = new Versions(new string[] { "1.0.0", "2.0.0" });

            PackageReference result = target.Resolve(new PackageDependency("SomeName", "3.0.0"));

            Assert.IsFalse(result.Found);
            Assert.IsTrue(result.NotFound);
        }

        [TestMethod]
        public void Resolve_PackageDependencyDoesNotExistInVersionList_RangedVersionSearchReturnsNotFound()
        {
            var target = new Versions(new string[] { "1.0.0", "2.0.0" });

            PackageReference result = target.Resolve(new PackageDependency("SomeName", "3.x"));

            Assert.IsFalse(result.Found);
            Assert.IsTrue(result.NotFound);
        }
    }
}
