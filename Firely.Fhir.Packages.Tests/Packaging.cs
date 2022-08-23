using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Firely.Fhir.Packages.Tests
{
    [TestClass]
    public class Packaging
    {
        [TestMethod]
        public void FolderOrganization()
        {
            var file =
                new FileEntry(@"C:\random\project\subfolder\subfolder\myresource.txt", System.Array.Empty<byte>())
                .MakeRelativePath(@"C:\random\project\")
                .OrganizeToPackageStructure();

            Assert.AreEqual(@"package\other\myresource.txt", file.FilePath);


            file =
                new FileEntry(@"C:\random\project\subfolder\subfolder\patient.xml", System.Array.Empty<byte>())
                .MakeRelativePath(@"C:\random\project\")
                .OrganizeToPackageStructure();

            Assert.AreEqual(@"package\patient.xml", file.FilePath);

        }

        [TestMethod]
        public void TestGeneratingIndexFiles()
        {
            var files = new List<FileEntry>
            {
                createFileEntry("{\"resourceType\": \"Patient\",\"id\": \"example\",\"gender\": \"male\",\"birthDate\": \"1974-12-25\"}", @"C:\random\project\subfolder\subfolder\patient.json"),
                createFileEntry("{\"resourceType\": \"Patient\",\"id\": \"example2\",\"gender\": \"female\",\"birthDate\": \"1974-12-25\"}", @"C:\random\project\subfolder\subfolder\patient2.json"),
                createFileEntry("thisisjustanimage", @"C:\random\project\subfolder\subfolder\patient2.png")
            };

            var package = files.Select(FileEntries.OrganizeToPackageStructure)
                               .AddIndexFiles();

            package.Should().Contain(e => e.FilePath == @"package\.index.json");
            package.Should().Contain(e => e.FilePath == @"package\other\.index.json");


        }

        [TestMethod]
        [DataRow(true, 0)]
        [DataRow(true, 1)]
        [DataRow(true, 1)]
        [DataRow(false, 0)]
        [DataRow(false, 1)]
        [DataRow(false, 1)]
        public async Task HandleInvalidPackagesOnRestore(bool includeValidPackage, int invalidPackageCount)
        {
            // Arrange
            const string packageName = "MyInvalidPackage";
            const string packageRange = "invalid";

            var dependencies = new List<string>();

            if (includeValidPackage)
                dependencies.Add(IndexGenerationTest.HL7_CORE_PACKAGE_R4);

            for (int i = 0; i < invalidPackageCount; i++)
                dependencies.Add($"{packageName}{i+1}@{i+1}.{packageRange}");

            var fixtureDirectory = TestHelper.InitializeTemporary("integration-test", dependencies.ToArray()).Result;

            // Act
            try
            {
                // TestHelper.Open calls restore on package context
                await TestHelper.Open(fixtureDirectory, _ => { });
            }
            catch (AggregateException aex)
            {
                if (invalidPackageCount > 0)
                {
                    // Assert
                    aex.InnerExceptions.Should().HaveCount(invalidPackageCount);
                    aex.Message.Should().Be($"One or more errors occurred. (Invalid version string: \"1.{packageRange}\")");

                    for (int i = 0; i < invalidPackageCount; i++)
                    {
                        aex.InnerExceptions[i].Should().BeOfType<PackageRestoreException>();

                        var prex = (PackageRestoreException)aex.InnerExceptions[i];

                        prex.PackageDependencies.Should().HaveCount(1);

                        var dependency = prex.PackageDependencies.Single();

                        dependency.Name.Should().Be($"{packageName}{i + 1}");

                        prex.Message.Should().Be($"Invalid version string: \"{i + 1}.{packageRange}\"");
                    }

                    return;
                }
            }

            if (invalidPackageCount > 0)
                Assert.Fail("Expected AggregateException to be thrown");
        }

        private static FileEntry createFileEntry(string content, string path) => new(path, Encoding.ASCII.GetBytes(content));
    }
}
