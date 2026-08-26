using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;

namespace Firely.Fhir.Packages.Tests
{
    [TestClass]
    public class DiskPackageCacheTests
    {
        [TestMethod]
        public void TestIgnoreInvalidPackageFolder()
        {

            //Create new packageCache folder:
            //  --testcache
            //  ---- validpackage#1.0.0
            //  ---- invalidpackage
            //  ---- #invalidpackage
            //  ---- invalidpackage#

            string root = "testCache";
            string validPackage = "validpackage#2.0.0";
            string invalidPackage1 = "invalidpackage";
            string invalidPackage2 = "#invalidpackage";
            string invalidPackage3 = "invalidpackage#";
            string invalidPackage4 = "invalidpackage#2.0.0#invalid";


            Directory.CreateDirectory(root);
            Directory.CreateDirectory($"{root}/{validPackage}");
            Directory.CreateDirectory($"{root}/{invalidPackage1}");
            Directory.CreateDirectory($"{root}/{invalidPackage2}");
            Directory.CreateDirectory($"{root}/{invalidPackage3}");
            Directory.CreateDirectory($"{root}/{invalidPackage4}");

            //Test
            var packageCache = new DiskPackageCache(root);
            var packages = packageCache.GetPackageReferences().Result;
            packages.Should().OnlyContain(x => x.Name == validPackage.Split('#', System.StringSplitOptions.None).First());


            //Cleanup
            Directory.Delete(root, true);


        }

        [TestMethod]
        public void MixedCaseCacheFolderNameIsParsedToLowercaseReference()
        {
            // A cache folder created by an older version of the library (or manually) may
            // still have mixed-case casing in its name. Parsing it back into a PackageReference
            // must normalise the name to lowercase, just like constructing one directly.

            string root = "testCacheMixedCase";
            string mixedCasePackage = "MixedCasePackage#2.0.0";

            Directory.CreateDirectory(root);
            Directory.CreateDirectory($"{root}/{mixedCasePackage}");

            //Test
            var packageCache = new DiskPackageCache(root);
            var packages = packageCache.GetPackageReferences().Result;
            packages.Should().OnlyContain(x => x.Name == "mixedcasepackage" && x.Version == "2.0.0");

            //Cleanup
            Directory.Delete(root, true);
        }

    }
}
