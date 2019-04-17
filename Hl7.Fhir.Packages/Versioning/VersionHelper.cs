using System.Collections.Generic;
using System.Linq;
using SemVer;

namespace Hl7.Fhir.Packages
{

    public static class VersionHelper
    {
        public static Versions ToVersions(this PackageListing listing)
        {
            var versions = new Versions(listing.Versions?.Keys);
            return versions;
        }

        public static Versions ToVersions(this IEnumerable<PackageReference> references)
        {
            var list = new Versions(references.Select(r => r.Version));
            return list;
        }

        public static Version Resolve(this Versions versions, string pattern)
        {
            if (pattern == "latest" || pattern is null)
            {
                return versions.Latest();
            }
            var range = new Range(pattern);
            return versions.Resolve(range);
        }

        public static PackageReference Resolve(this Versions versions, PackageDependency dependency)
        {
            var version = versions.Resolve(dependency.Range).ToString();
            return new PackageReference(dependency.Name, version);
        }
    }
}
