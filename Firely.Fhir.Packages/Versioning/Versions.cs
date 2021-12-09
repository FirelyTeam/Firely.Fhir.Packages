#nullable enable

using SemVer;
using System.Collections.Generic;
using System.Linq;

namespace Firely.Fhir.Packages
{
    public class Versions
    {
        private readonly List<Version> _list = new();

        [System.CLSCompliant(false)]
        public IReadOnlyCollection<Version> Items => _list;

        public Versions() { }

        public Versions(IEnumerable<string> versions)
        {
            Append(versions);
        }

        public void Append(IEnumerable<string> versions)
        {
            foreach (var s in versions)
            {
                if (TryParseVersion(s, out Version? version))
                {
                    if (version != null)
                        _list.Add(version);
                }
            }
            _list.Sort();
        }

        [System.CLSCompliant(false)]
        public Version? Latest()
        {
            return _list.LastOrDefault();
        }

        [System.CLSCompliant(false)]
        public static bool TryParseVersion(string s, out Version? v)
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

        [System.CLSCompliant(false)]
        public Version Resolve(Range range)
        {

            return range.MaxSatisfying(_list);
        }

        [System.CLSCompliant(false)]
        public bool Has(Version version)
        {
            foreach (var item in _list)
            {
                if (item == version) return true;
            }
            return false;
        }

        public bool IsEmpty => _list is null || _list.Count == 0;
    }
}
#nullable restore