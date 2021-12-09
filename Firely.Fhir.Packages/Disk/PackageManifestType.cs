#nullable enable

using System;
using System.Linq;

namespace Firely.Fhir.Packages
{
    /// <summary>
    /// Package Manifest Types. Mainly used by the IG build tool.
    /// They are also used by packages.simplifier.net to determine if a FhirVersion is required 
    /// for a package.
    ///
    /// For the list see:
    /// https://confluence.hl7.org/display/FHIR/NPM+Package+Specification
    /// </summary>
    public enum PackageManifestType
    {
        None = 0,

        Conformance,
        IG,
        Core,
        Examples,
        Group,
        Tool,
        IGTemplate,
    }

    public static class PackageManifestTypes
    {
        public static bool TryParse(string value, out PackageManifestType type)
        {
            string typestr = value.Replace("-", "");
            bool ok = Enum.TryParse(typestr, ignoreCase: true, out type);
            return ok;
        }

        public static bool FhirVersionRequired(PackageManifestType type) => !NOFHIRVERSIONREQUIREDTYPES.Contains(type);


        private static readonly PackageManifestType[] NOFHIRVERSIONREQUIREDTYPES = new[] { PackageManifestType.Tool, PackageManifestType.IGTemplate };

    }
}

#nullable restore
