using System.Collections.Generic;
using System.Linq;
using SemVer;

namespace Hl7.Fhir.Packages
{
    public class Versions
    {
        List<Version> list = new List<Version>();

        public IReadOnlyCollection<Version> Items => list;

        public Versions(IEnumerable<string> versions)
        {
            Append(versions);
        }

        public void Append(IEnumerable<string> versions)   
        {
            foreach (var s in versions)
            {
                if (TryParseVersion(s, out Version version))
                {
                    list.Add(version);
                }
            }
            list.Sort();
        }

        public Version Latest()
        {
            return list.LastOrDefault();
        }

        public static bool TryParseVersion(string s, out Version v)
        {
            try
            {
                v = new Version(s, loose: true);
                // Loose = true, prevents most errors. 
                // But just to make sure we'll contain any exceptions, since it doesn't have a TryParse. 
                return true;
            }
            catch
            {
                v = null;
                return false;
            }
        }

        public Version Resolve(Range range)
        {
            return range.MaxSatisfying(list);
        }

        public Version Resolve(string pattern)
        {
            if (pattern == "latest" || pattern is null)
            {
                return Latest();
            }
            var range = new Range(pattern);
            return Resolve(range);
        }

        public bool IsEmpty => list is null || list.Count == 0;
    }

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
    }
}
