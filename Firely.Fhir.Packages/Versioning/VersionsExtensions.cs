using SemVer;
using System.Collections.Generic;
using System.Linq;

namespace Firely.Fhir.Packages
{

    public static class VersionsExtensions
    {
        /// <summary>
        /// Gets all SemVer versions in this package listing
        /// <returns></returns>
        public static Versions GetVersions(this PackageListing listing)
        {
            var versions = new Versions(listing.Versions.Keys);
            return versions;
        }

        public static Versions ToVersions(this IEnumerable<PackageReference> references)
        {
            var list = new Versions(references.Select(r => r.Version));
            return list;
        }

        /// <summary>
        /// Resolves a NPM version range string in this Versions set to a single Semver Version.
        /// </summary>
        /// <param name="versions"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        [System.CLSCompliant(false)]
        public static Version Resolve(this Versions versions, string pattern)
        {
            if (pattern == "latest" || string.IsNullOrEmpty(pattern))
            {
                return versions.Latest();
            }
            var range = new Range(pattern);
            return versions.Resolve(range);
        }

        /// <summary>
        /// Resolves a Package dependency to a specific non floating reference.
        /// </summary>
        /// <param name="versions"></param>
        /// <param name="dependency"></param>
        /// <returns></returns>
        public static PackageReference Resolve(this Versions versions, PackageDependency dependency)
        {
            var version = versions.Resolve(dependency.Range);
            if (version is null)
            {
                return PackageReference.None;
            }
            return new PackageReference(dependency.Name, version.ToString());
        }

        /// <summary>
        /// Checks if this versions list contains a specific version
        /// </summary>
        /// <param name="versions"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public static bool Contains(this Versions versions, string version)
        {
            var v = new Version(version);
            return versions.Contains(v);
        }
    }
}
