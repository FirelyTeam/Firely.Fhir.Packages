using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Firely.Fhir.Packages.Tests
{
    [TestClass]
    public class ManifestSerializationTest
    {
        [TestMethod]
        public void DoesNotWriteNulls()
        {
            PackageManifest manif = ManifestFile.Create("a-b-c", 4);

            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);

            FolderProject proj = new FolderProject(tempDir);
            proj.WriteManifest(manif);

            var json = File.ReadAllText(Path.Combine(tempDir, PackageConsts.Manifest));
            var parsedJson = JObject.Parse(json);

            // we should not find an `'author' : null` property
            Assert.IsFalse(parsedJson.ContainsKey("author"));
        }

        [TestMethod]
        public void WritesAndRetrievesAllManifestProperties()
        {
            PackageManifest manif = new PackageManifest()
            {
                Name = "hl7.fhir.us.acme",
                Version = "0.1.0",
                Author = "hl7",
            };

            var dyn = (dynamic)manif;

            dyn.license = "CC0-1.0";
            dyn.keywords = new string[] { "us", "United States", "ACME" };
            dyn.maintainers = new Dictionary<string, string>[]
            {
                new Dictionary<string, string>()
                {
                    ["name"] = "US Steering Committee",
                    ["email"] = "ussc@lists.hl7.com"
                }
            };

            var json = JsonConvert.SerializeObject(dyn, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            var restored = (PackageManifest)JsonConvert.DeserializeObject<PackageManifest>(json);
            Assert.AreEqual(manif.Name, restored.Name);
            Assert.AreEqual(manif.Version, restored.Version);
            Assert.AreEqual(manif.Author, restored.Author);

            dynamic drestored = restored;
            Assert.AreEqual(drestored.license, dyn.license);
            CollectionAssert.AreEqual(((JArray)drestored.keywords).ToObject<string[]>(), dyn.keywords);

            var t = ((JArray)drestored.maintainers).Children()
                .Select(c => c.ToObject<Dictionary<string, string>>());

            CollectionAssert.AreEqual(t.Single(), dyn.maintainers[0]);
        }
    }   
}

