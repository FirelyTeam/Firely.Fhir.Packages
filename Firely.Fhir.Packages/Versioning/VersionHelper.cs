/* 
 * Copyright (c) 2022, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/Firely.Fhir.Packages/blob/master/LICENSE
 */


#nullable enable

using SemVer;
using System.Collections.Generic;
using System.Linq;

namespace Firely.Fhir.Packages
{

    public static class VersionsExtensions
    {
        /// <summary>
        /// Returns version information based on this listing
        /// </summary>
        /// <param name="listing"></param>
        /// <returns>A list of package versions</returns>
        public static Versions ToVersions(this PackageListing listing)
        {
            var listed = listing.GetListedVersionStrings();
            var unlisted = listing.GetUnlistedVersionStrings();
            return new Versions(listed, unlisted);
        }

        internal static IEnumerable<string> GetUnlistedVersionStrings(this PackageListing listing)
        {
            return listing.Versions?.Where(v => v.Value.Unlisted == "false").Select(v => v.Key) ?? new List<string> { };
        }

        internal static IEnumerable<string> GetListedVersionStrings(this PackageListing listing)
        {
            return listing.Versions?.Where(v => v.Value.Unlisted == "true").Select(v => v.Key) ?? new List<string> { };
        }

        /// <summary>
        /// Retuns version information based on this list of package references
        /// </summary>
        /// <param name="references"></param>
        /// <returns>A list of package versions</returns>
        public static Versions ToVersions(this IEnumerable<PackageReference> references)
        {
            var versions = references.Select(r => r.Version).Where(v => v is not null);

            return versions is null || !versions.Any() ? new Versions() : new Versions(versions!);
        }

        internal static Version? Resolve(this Versions versions, string? pattern)
        {
            if (pattern == null)
                return null;

            if (pattern == "latest" || string.IsNullOrEmpty(pattern))
            {
                return versions.Latest();
            }
            var range = new Range(pattern);
            return versions.Resolve(range);
        }

        /// <summary>
        /// Resolve a package dependency
        /// </summary>
        /// <param name="versions"></param>
        /// <param name="dependency">The package reference to be resolves</param>
        /// <param name="stable">Indication of allowing only non-preview versions</param>
        /// <returns>A package reference describing the dependency</returns>
        public static PackageReference Resolve(this Versions versions, PackageDependency dependency, bool stable = false)
        {
            var version = (dependency.Range is null) ? null : versions.Resolve(dependency.Range, stable);
            return version is null ? PackageReference.None : new PackageReference(dependency.Name, version.ToString());
        }

        /// <summary>
        /// A boolean cheching if this <see cref="Versions"/> object contains a specific version 
        /// </summary>
        /// <param name="versions"></param>
        /// <param name="version">Soecific version to be checked</param>
        /// <returns>Whether of not a specific version is present in the <see cref="Versions"/> object </returns>
        public static bool Has(this Versions versions, string version)
        {
            var v = new Version(version);
            return versions.Has(v);
        }
    }
}

#nullable restore
