using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Firely.Fhir.Packages
{
    public static class PackageCacheExtensions
    {
        public static PackageManifest ReadManifest(this IPackageCache cache, string name, string version)
        {
            var reference = new PackageReference(name, version);
            return cache.ReadManifest(reference);
        }

        public static CanonicalIndex ReadCanonicalIndex(this IPackageCache cache, string name, string version)
        {
            var reference = new PackageReference(name, version);
            return cache.GetCanonicalIndex(reference);
        }

        public static IEnumerable<PackageReference> WithName(this IEnumerable<PackageReference> refs, string name)
        {
            return refs.Where(r => string.Compare(r.Name, name, ignoreCase: true) == 0);
        }

        //public static IEnumerable<PackageReference> GetInstalledVersions(this IPackageCache cache, string pkgname)
        //{
        //    return cache.GetPackageReferences().WithName(pkgname);
        //}

        

        public static PackageReference InstallFromFile(this IPackageCache cache, string path)
        {
            var manifest = Packaging.ExtractManifestFromPackageFile(path);
            var reference = manifest.GetPackageReference();
            var buffer = File.ReadAllBytes(path);
            cache.Install(reference, buffer);
            return reference; 
        }

        public static string GetFileContent(this IPackageCache cache, PackageFileReference reference)
        {
            return cache.GetFileContent(reference.Package, reference.FileName);
        }
    }


}

