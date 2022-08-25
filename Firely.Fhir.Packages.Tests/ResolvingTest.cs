using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Firely.Fhir.Packages.Tests
{
    [TestClass]
    public class ResolvingTest
    {
        [TestMethod]
        public void ResolveRangeShouldNotReturnUnlisted()
        {
            var listed = new List<string> { "3.0.1", "3.0.2", "4.0.0" };
            var unlisted = new List<string> { "3.0.3" };
            var versions = new Versions(listed, unlisted);
            var version = versions.Resolve("3.0"); 

            Assert.AreEqual("3.0.2", version.ToString());
        }

        [TestMethod]
        public void ResolvingExactShouldReturnUnlisted()
        {
            var listed = new List<string> { "3.0.1", "3.0.2", "4.0.0" };
            var unlisted = new List<string> { "3.0.3" };
            var versions = new Versions(listed, unlisted);

            var version = versions.Resolve("3.0.3");

            Assert.AreEqual("3.0.3", version.ToString());
        }

        [TestMethod]
        public void LatestShouldNotFindPre()
        {
            var listed = new List<string> { "3.0.1", "3.0.2", "3.0.2-beta-1" };
            var versions = new Versions(listed);

            var version = versions.Resolve("latest");

            Assert.AreEqual("3.0.2", version.ToString());
        }
    }
}
