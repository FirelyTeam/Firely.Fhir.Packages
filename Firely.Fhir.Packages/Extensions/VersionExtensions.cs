using SemanticVersioning;
using System.Collections.Generic;

namespace Firely.Fhir.Packages.Extensions
{
    public static class VersionExtensions
    {
        /// <summary>
        /// Determines if the <see cref="Version"/> is listed in the collection
        /// </summary>
        /// <param name="versions">The <see cref="List{Version}"/> to check against</param>
        /// <param name="version">The <see cref="Version"/> under check</param>
        /// <returns>True/False depending on if the <see cref="Version"/> is checked or not</returns>
        internal static bool IsListed(this List<Version> versions, Version version)
        {
            return versions.Contains(version);
        }
    }
}
