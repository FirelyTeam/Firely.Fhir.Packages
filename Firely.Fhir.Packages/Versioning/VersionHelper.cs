using SemVer;
using System.Collections.Generic;
using System.Linq;

namespace Firely.Fhir.Packages
{

    public static class VersionsExtensions
    {
        public static Versions ToVersions(this PackageListing listing)
        {
            var listed = listing.GetlistedVersionStrings();
            var unlisted = listing.GetUnlistedVersionStrings();
            return new Versions(listed, unlisted);
        }

        public static IEnumerable<string> GetUnlistedVersionStrings(this PackageListing listing)
        {
            return listing.Versions.Where(v => v.Value.Unlisted == "false").Select(v => v.Key);
        }

        public static IEnumerable<string> GetlistedVersionStrings(this PackageListing listing)
        {
            return listing.Versions.Where(v => v.Value.Unlisted == "true").Select(v => v.Key);
        }

        public static Versions ToVersions(this IEnumerable<PackageReference> references)
        {
            var list = new Versions(references.Select(r => r.Version));
            return list;
        }

        public static PackageReference Resolve(this Versions versions, PackageDependency dependency)
        {
            var version = versions.Resolve(dependency.Range);
            if (version is null)
            {
                return PackageReference.None;
            }
            return new PackageReference(dependency.Name, version.ToString());
        }

        public static bool Has(this Versions versions, string version)
        {
            var v = new Version(version);
            return versions.Has(v);
        }

        
    }
}
