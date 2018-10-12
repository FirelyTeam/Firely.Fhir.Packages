using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SemVer;

namespace Hl7.Fhir.Packages
{
    public static class VersionHelper
    {
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

        public static List<Version> ToVersions(this IEnumerable<string> list)
        {
            var versions = new List<Version>();

            foreach(var s in list)
            {
                if (TryParseVersion(s, out Version version))
                {
                    versions.Add(version);
                }
            }
            versions.Sort();
            return versions;
        }

        public static Version Resolve(this List<Version> versions, string pattern)
        {
            if (pattern == "latest" || pattern is null)
            {
                return versions.LastOrDefault();
            }

            var range = new Range(pattern);
            return range.MaxSatisfying(versions);
        }

        public static List<Version> ToVersions(this PackageListing listing)
        {
            var list = listing.Versions?.Keys.ToVersions().ToList();
            list?.Sort();
            return list;
        }

        public static bool IsEmpty(this List<Version> list)
        {
            return list is null || list.Count == 0;
        }

    }
}
