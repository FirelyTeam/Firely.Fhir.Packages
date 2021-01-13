using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System.IO;

namespace Firely.Fhir.Packages.Tests
{
    [TestClass]
    public class ManifestSerializationTest
    {
        [TestMethod]
        public void DoesNotWriteNulls()
        {
            PackageManifest manif = ManifestFile.Create("a-b-c", "4.0.1");

            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);

            FolderProject proj = new FolderProject(tempDir);
            proj.WriteManifest(manif);

            var json = File.ReadAllText(Path.Combine(tempDir, PackageConsts.Manifest));
            var parsedJson = JObject.Parse(json);

            // we should not find an `'author' : null` property
            Assert.IsFalse(parsedJson.ContainsKey("author"));
        }
    }
}
