using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System.IO;

namespace Firely.Fhir.Packages.Tests
{
    [TestClass]
    public class ManifestSerializationTest
    {
        [TestMethod]
        public void RoundtripsAllCommonProperties()
        {
            PackageManifest manif = new(name: "roundtrip.test", version: "1.0.1")
            {
                Description = "Does this roundtrip all properties?",
                Author = "Ewout",
                Dependencies = new() { ["dep.a"] = "=1.0.0", ["dep.b"] = ">=2.0.1" },
                DevDependencies = new() { ["ddep.a"] = "=1.0.0", ["ddep.b"] = "latest" },
                Keywords = new() { "just", "a", "test" },
                License = "No license",
                Homepage = "http://nu.nl",
                Directories = new() { [PackageManifest.DirectoryKeys.DIRECTORY_KEY_LIB] = "package" },
                Title = "Package with tests",
                FhirVersions = new() { "3.0.2", "4.0.1" },
                FhirVersionList = new() { "3.0.1", "4.0.2" },
                Maintainers = new()
                {
                    new() { Name = "Ewout", Email = "ewout@fire.ly" },
                    new() { Email = "someone@somehwere.net " }
                },
                Canonical = "http://nu.nl/package",
                Url = "http://nu.nl",
                Jurisdiction = "urn:iso:std:iso:3166#US"
            };

            var json = PackageParser.SerializeManifest(manif);
            var manif2 = PackageParser.ParseManifest(json);

            manif2.Should().BeEquivalentTo(manif);
        }

        [TestMethod]
        public void TestAuthorSerialization()
        {
            PackageManifest manif = new(name: "author.test", version: "1.0.1")
            {
                Author = "Marten"
            };

            var json = PackageParser.SerializeManifest(manif);

            json.Should().Contain("\"author\": \"Marten\"");
            var manif2 = PackageParser.ParseManifest(json);
            manif2.Should().BeEquivalentTo(manif);
            manif2?.AuthorInformation?.Name.Should().Be("Marten");

            //Test the other way around

            manif = new(name: "author.test", version: "1.0.1")
            {
                AuthorInformation = new() { Name = "Marten", Url = "https://foo.bar", Email = "foo@bar.nl" }
            };

            json = PackageParser.SerializeManifest(manif);

            json.Should().Contain("  \"author\": {\r\n    \"name\": \"Marten\",\r\n    \"email\": \"foo@bar.nl\",\r\n    \"url\": \"https://foo.bar\"\r\n  }");
            manif2 = PackageParser.ParseManifest(json);
            manif2.Should().BeEquivalentTo(manif);
            manif2?.Author?.Should().Be("Marten <foo@bar.nl> (https://foo.bar)");
        }

        [TestMethod]
        public void TestReadingAComplexAuthorProperty()
        {
            var json = File.ReadAllText($"TestData/unknown-properties-package.json");
            var manifest = PackageParser.ParseManifest(json);
            //manifest.AuthorInformation()
            manifest.Should().NotBeNull();
        }

        [DataRow("foo <foo@bar> (http://foo.bar)", "foo", "foo@bar", "http://foo.bar")]
        [DataRow("foo (http://foo.bar)", "foo", null, "http://foo.bar")]
        [DataRow("foo <foo@bar>", "foo", "foo@bar", null)]
        [DataRow("<foo@bar> (http://foo.bar)", null, "foo@bar", "http://foo.bar")]
        [DataTestMethod]
        public void TestAuthorStringParsing(string? fullAuthor, string? name, string? email, string? url)
        {
            PackageManifest manif = new(name: "authot.test", version: "1.0.1")
            {
                Author = fullAuthor
            };

            manif.AuthorInformation?.Name.Should().Be(name);
            manif.AuthorInformation?.Email.Should().Be(email);
            manif.AuthorInformation?.Url.Should().Be(url);
        }

        [DataRow("foo <foo@bar> (http://foo.bar)", "foo", "foo@bar", "http://foo.bar")]
        [DataRow("foo (http://foo.bar)", "foo", null, "http://foo.bar")]
        [DataRow("foo <foo@bar>", "foo", "foo@bar", null)]
        [DataRow("<foo@bar> (http://foo.bar)", null, "foo@bar", "http://foo.bar")]
        [DataTestMethod]
        public void TestAuthorStringSerializing(string? fullAuthor, string? name, string? email, string? url)
        {
            PackageManifest manif = new(name: "authot.test", version: "1.0.1")
            {
                AuthorInformation = new()
                {
                    Name = name,
                    Email = email,
                    Url = url
                }
            };

            manif.Author.Should().Be(fullAuthor);
        }

        [TestMethod]
        public void DoesNotWriteNulls()
        {
            PackageManifest manif = ManifestFile.Create("a-b-c", "4.0.1");
            var json = writeManifest(manif);

            var parsedJson = JObject.Parse(json);

            // we should not find an `'author' : null` property
            Assert.IsFalse(parsedJson.ContainsKey("author"));
        }

        [TestMethod]
        public void MergeDoesNotWriteNulls()
        {
            _ = createTempDir();
            var manifest1 = new PackageManifest("a-b-c", "4.0.1");
            var manifest2 = new PackageManifest("a-b-c", "4.0.1") { Author = "Turing" };

            var serialized1 = PackageParser.SerializeManifest(manifest1);
            var serialized2 = PackageParser.JsonMergeManifest(manifest2, serialized1);

            var roundtrip = JObject.Parse(serialized2);

            Assert.IsFalse(roundtrip.ContainsKey("keywords"));
            Assert.IsFalse(roundtrip.ContainsKey("license"));
            Assert.IsFalse(roundtrip.ContainsKey("homepage"));
        }


        private static string writeManifest(PackageManifest manif)
        {
            var tempDir = createTempDir();
            FolderProject proj = new(tempDir);
            proj.WriteManifest(manif);

            return File.ReadAllText(Path.Combine(tempDir, PackageFileNames.MANIFEST));
        }

        private static string createTempDir()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);
            return tempDir;
        }
    }
}
