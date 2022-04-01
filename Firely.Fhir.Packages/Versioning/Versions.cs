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
    /// <summary>
    /// List of SemVer versions
    /// </summary>
    public class Versions
    {
        private readonly List<Version> _list = new();

        /// <summary>
        /// Return the versions from the list
        /// </summary>
        [System.CLSCompliant(false)]
        public IReadOnlyCollection<Version> Items => _list;

        /// <summary>
        /// Create an empty list if versions
        /// </summary>
        public Versions() { }

        /// <summary>
        /// Create a list of versions
        /// </summary>
        /// <param name="versions">versions from a list has to be created</param>
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

        /// <summary>
        /// Get the latest version from the list
        /// </summary>
        /// <returns>the latest version from the list</returns>
        [System.CLSCompliant(false)]
        public Version? Latest()
        {
            return _list.LastOrDefault();
        }

        /// <summary>
        /// Try to parse a SemVer version from a string
        /// </summary>
        /// <param name="s">string to be parsed</param>
        /// <param name="v">Semver version object</param>
        /// <returns>Whether the string was succesfully parsed to a SemVer version object</returns>
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

        /// <summary>
        /// Resolve the best mathing version from a range
        /// </summary>
        /// <param name="range">Range of versions to be used during the resolving</param>
        /// <returns>Semver Version object if the best matching version</returns>
        [System.CLSCompliant(false)]
        public Version Resolve(Range range)
        {

            return range.MaxSatisfying(_list);
        }

        /// <summary>
        /// Check whether the list of SemVer version objects contains a specific version    
        /// </summary>
        /// <param name="version">verison to be checked for</param>
        /// <returns>Whether the list of SemVer version objects contains a specific version</returns>
        [System.CLSCompliant(false)]
        public bool Has(Version version)
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