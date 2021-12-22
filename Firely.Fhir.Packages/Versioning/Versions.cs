#nullable enable

using SemVer;
using System.Collections.Generic;
using System.Linq;

namespace Firely.Fhir.Packages
{
    public class Versions
    {
        private readonly List<Version> _list = new();

        internal IReadOnlyCollection<Version> Items => _list;

        public Versions() { }

        public Versions(IEnumerable<string> versions)
        {
            Append(versions);
        }

        /// <summary>
        /// Add an array of version to the current list of versions.
        /// </summary>
        /// <param name="versions">List of versions to be added</param>
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


        internal Version? Latest()
        {
            return _list.LastOrDefault();
        }


        internal static bool TryParseVersion(string s, out Version? v)
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

        internal Version Resolve(Range range)
        {

            return range.MaxSatisfying(_list);
        }

        internal bool Has(Version version)
        {
            foreach (var item in _list)
            {
                if (item == version) return true;
            }
            return false;
        }

        /// <summary>
        /// Boolean that indicated if there are any versions present in this <see cref="Versions"/> object.
        /// </summary>
        public bool IsEmpty => _list is null || _list.Count == 0;
    }
}
#nullable restore