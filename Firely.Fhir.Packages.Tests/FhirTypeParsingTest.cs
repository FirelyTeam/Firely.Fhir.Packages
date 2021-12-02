using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Firely.Fhir.Packages.Tests
{
    [TestClass]
    public class FhirTypeParsingTest
    {
        [TestMethod]
        public void TestFhirTypeParsing()
        {
            // Testing case insensitivity
            PackageManifestTypes.TryParse("Conformance", out var type);
            Assert.AreEqual(PackageManifestType.Conformance, type);

            PackageManifestTypes.TryParse("conformance", out type);
            Assert.AreEqual(PackageManifestType.Conformance, type);

            // Testing dashes
            PackageManifestTypes.TryParse("IG-Template", out type);
            Assert.AreEqual(PackageManifestType.IGTemplate, type);

            // Testing invalid
            var ok = PackageManifestTypes.TryParse("IG-Temp-verylate", out type);
            Assert.AreEqual(PackageManifestType.None, type);
            Assert.AreEqual(false, ok);
        }

    }
}
