using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Hl7.Fhir.Packages
{
    public static class PackageCacheExtensions
    {
        public static PackageManifest ReadManifest(this PackageCache cache, string name, string version)
        {
            var reference = new PackageReference(name, version);
            return cache.ReadManifest(reference);
        }

        public static CanonicalIndex ReadCanonicalIndex(this PackageCache cache, string name, string version)
        {
            var reference = new PackageReference(name, version);
            return cache.GetCanonicalIndex(reference);
        }

        public static IEnumerable<PackageReference> WithName(this IEnumerable<PackageReference> refs, string name)
        {
            return refs.Where(r => string.Compare(r.Name, name, ignoreCase: true) == 0);
        }

        public static IEnumerable<PackageReference> GetInstalledVersions(this PackageCache cache, string pkgname)
        {
            return cache.GetPackageReferences().WithName(pkgname);
        }

        public async static ValueTask<bool> HasMatch(this PackageCache cache, PackageDependency dependency)
        {
            var references = cache.GetInstalledVersions(dependency.Name);
            var versions = references.ToVersions();
            var reference = versions.Resolve(dependency);
            return await Task.FromResult(reference.Found);
        }

        public static PackageManifest InstallFromFile(this PackageCache cache, string path)
        {
            var manifest = Packaging.ExtractManifestFromPackageFile(path);
            var reference = manifest.GetPackageReference();
            var buffer = File.ReadAllBytes(path);
            cache.Install(reference, buffer);
            return manifest;
        }
    }


}

