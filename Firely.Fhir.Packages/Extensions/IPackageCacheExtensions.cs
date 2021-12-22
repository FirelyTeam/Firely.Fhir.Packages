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
        /// <summary>
        /// Reads manifest file
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="name">Package name</param>
        /// <param name="version">package version</param>
        /// <returns>The package manifest</returns>
        internal static async Task<PackageManifest?> ReadManifest(this IPackageCache cache, string name, string version)
        {
            var reference = new PackageReference(name, version);
            return await cache.ReadManifest(reference).ConfigureAwait(false);
        }

        /// <summary>
        /// Read the firely specific index file from the package
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="name">name of the package</param>
        /// <param name="version">version of the package</param>
        /// <returns></returns>
        internal static async Task<CanonicalIndex> ReadCanonicalIndex(this IPackageCache cache, string name, string version)
        {
            var reference = new PackageReference(name, version);
            return await cache.GetCanonicalIndex(reference).ConfigureAwait(false);
        }

        /// <summary>
        /// Get packages with a certain name
        /// </summary>
        /// <param name="refs"></param>
        /// <param name="name">Package name</param>
        /// <returns>List of package references of packages with a certain name</returns>
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

            await cache.install(reference, path).ConfigureAwait(false);

            return reference;
        }

        /// <summary>
        /// Install a package from a file on disk
        /// </summary>
        /// <param name="cache">Cache in which the package is to be installed</param>
        /// <param name="reference">Reference of the package to be installed</param>
        /// <param name="path">file path of the package to be installed</param>
        /// <returns></returns>
        private static async Task install(this IPackageCache cache, PackageReference reference, string path)
        {
            if (!await cache.IsInstalled(reference))
            {
                var buffer = File.ReadAllBytes(path);
                await cache.Install(reference, buffer).ConfigureAwait(false);
            }
        }


        internal static async Task<string> GetFileContent(this IPackageCache cache, PackageFileReference reference)
        {
            return await cache.GetFileContent(reference.Package, reference.FilePath).ConfigureAwait(false);
        }

        internal static async Task<string> ReadPackageFhirVersion(this IPackageCache cache, PackageReference reference)
        {
            var m = await cache.ReadManifest(reference).ConfigureAwait(false);
            var fhirVersion = m?.GetFhirVersion();

            return fhirVersion ?? throw new ArgumentException($"FHIR Version {reference.Version} from package {reference.Name} is invalid");
        }

    }
}

#nullable restore

