using SemVer;
using System.Collections.Generic;
using System.Linq;

namespace Firely.Fhir.Packages
{
    public class Versions
    {
        readonly List<Version> list = new List<Version>();
        readonly List<Version> unlisted = new List<Version>();

        [System.CLSCompliant(false)]
        public IReadOnlyCollection<Version> Items => list;

        public Versions() { }

        public Versions(IEnumerable<string> listed, IEnumerable<string> unlisted = null)
        {
            AppendSorted(this.list, listed);
            
            if (unlisted is not null)
                AppendSorted(this.unlisted, unlisted);
        }

        [System.CLSCompliant(false)]
        public Version Latest(bool stable = true)
        {
            IEnumerable<Version> list = stable ? this.Stable() : this.list;

            return list.LastOrDefault();
        }

        public IEnumerable<Version> Stable()
        {
            return list.Where(v => v.PreRelease is null && v.Build is null);
        }

        [System.CLSCompliant(false)]
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

        private static void AppendSorted(List<Version> list, IEnumerable<string> values)
        {
            foreach (var value in values)
            {
                if (TryParseVersion(value, out Version version))
                {
                    list.Add(version);
                }
            }
            list.Sort();
        }
    

    [System.CLSCompliant(false)]
        public Version Resolve(string pattern, bool stable = true)
        {
            if (pattern == "latest" || string.IsNullOrEmpty(pattern))
                return this.Latest(stable);

            var range = new Range(pattern);
            Version version = range.MaxSatisfying(list);
            if (version is not null) return version;
            
            if (TryParseVersion(pattern, out version) && ExistsUnlisted(version)) 
                return version;
            
            return null;
        }

        private bool ExistsUnlisted(Version version)
        {
            Version v = unlisted.Find(v => v == version);
            return v is not null;
        }

        [System.CLSCompliant(false)]
        public bool Has(Version version)
        {
            foreach (var item in list)
            {
                if (item == version) return true;
            }
            return false;
        }

        public bool IsEmpty => list is null || list.Count == 0;
    }
}
