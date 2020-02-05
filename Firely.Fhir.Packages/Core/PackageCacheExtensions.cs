using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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

        public async static ValueTask<bool> HasMatch(this IPackageCache cache, PackageDependency dependency)
        {
            var versions = await cache.GetVersionsAsync(dependency.Name);
            var reference = versions.Resolve(dependency);
            return await Task.FromResult(reference.Found);
        }

        public static PackageManifest InstallFromFile(this IPackageCache cache, string path)
        {
            var manifest = Packaging.ExtractManifestFromPackageFile(path);
            var reference = manifest.GetPackageReference();
            var buffer = File.ReadAllBytes(path);
            cache.Install(reference, buffer);
            return manifest;
        }

        public static async ValueTask<PackageReference> Install(this IPackageCache cache, PackageDependency dependency)
        {
            var reference = await cache.Resolve(dependency);
            if (reference.Found)
            {
                return reference;
            }
            else
            {
                var ok = await cache.Install(reference);
                return ok ? reference : PackageReference.None;
            }
        }

        public static string GetFileContent(this IPackageCache cache, PackageFileReference reference)
        {
            return cache.GetFileContent(reference.Package, reference.FileName);
        }
    }


}

