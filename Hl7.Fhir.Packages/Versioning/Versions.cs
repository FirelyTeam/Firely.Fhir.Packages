using System.Collections.Generic;
using System.Linq;
using SemVer;

namespace Hl7.Fhir.Packages
{
    public class Versions
    {
        readonly List<Version> list = new List<Version>();

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

        public bool Has(Version version)
        {
            foreach(var item in list)
            {
                if (item == version) return true;
            }
            return false;
        }

        public bool IsEmpty => list is null || list.Count == 0;
    }
}
