#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Firely.Fhir.Packages
{
    public static class IPackageCacheExtensions
    {
        public static async Task<PackageManifest?> ReadManifest(this IPackageCache cache, string name, string version)
        {
            var reference = new PackageReference(name, version);
            return await cache.ReadManifest(reference).ConfigureAwait(false);
        }

        public static async Task<CanonicalIndex> ReadCanonicalIndex(this IPackageCache cache, string name, string version)
        {
            var reference = new PackageReference(name, version);
            return await cache.GetCanonicalIndex(reference);
        }

        public static IEnumerable<PackageReference> WithName(this IEnumerable<PackageReference> refs, string name)
        {
            return refs.Where(r => string.Compare(r.Name, name, ignoreCase: true) == 0);
        }

        //public static IEnumerable<PackageReference> GetInstalledVersions(this IPackageCache cache, string pkgname)
        //{
        //    return cache.GetPackageReferences().WithName(pkgname);
        //}

        /// <summary>
        /// Install a package from a file on disk
        /// </summary>
        /// <param name="cache">Cache in which the package is to be installed</param>
        /// <param name="path">file path of the package to be installed</param>
        /// <returns>Reference to the installed package</returns>
        public static async Task<PackageReference> InstallFromFile(this IPackageCache cache, string path)
        {
            var manifest = Packaging.ExtractManifestFromPackageFile(path);
            if (manifest == null)
                throw new InvalidOperationException("Cannot extract manifest from package file");

            var reference = manifest.GetPackageReference();

            await cache.Install(reference, path);

            return reference;
        }

        /// <summary>
        /// Install a package from a file on disk
        /// </summary>
        /// <param name="cache">Cache in which the package is to be installed</param>
        /// <param name="reference">Reference of the package to be installed</param>
        /// <param name="path">file path of the package to be installed</param>
        /// <returns></returns>
        public static async Task Install(this IPackageCache cache, PackageReference reference, string path)
        {
            if (!await cache.IsInstalled(reference))
            {
                var buffer = File.ReadAllBytes(path);
                await cache.Install(reference, buffer).ConfigureAwait(false);
            }
        }


        public static async Task<string> GetFileContent(this IPackageCache cache, PackageFileReference reference)
        {
            return await cache.GetFileContent(reference.Package, reference.FilePath).ConfigureAwait(false);
        }

        public static async Task<string> ReadPackageFhirVersion(this IPackageCache cache, PackageReference reference)
        {
            var m = await cache.ReadManifest(reference).ConfigureAwait(false);
            var fhirVersion = m?.GetFhirVersion();

            if (fhirVersion is null)
            {
                throw new ArgumentException($"FHIR Version {reference.Version} from package {reference.Name} is invalid");
            }

            return fhirVersion;
        }

    }
}

#nullable restore

