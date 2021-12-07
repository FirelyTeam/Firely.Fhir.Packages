using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Firely.Fhir.Packages.Tests
{
    [TestClass]
    public class Packaging
    {
        [TestMethod]
        public void FolderOrganization()
        {
            var file =
                new FileEntry { FilePath = @"C:\random\project\subfolder\subfolder\myresource.txt" }
                .MakeRelativePath(@"C:\random\project\")
                .OrganizeToPackageStructure();

            Assert.AreEqual(@"package\other\myresource.txt", file.FilePath);


            file =
                new FileEntry { FilePath = @"C:\random\project\subfolder\subfolder\patient.xml" }
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

        private FileEntry createFileEntry(string content, string path) => new()
        {
            FilePath = path,
            Buffer = Encoding.ASCII.GetBytes(content)
        };
    }
}
