using Hl7.Fhir.Specification;
using Hl7.Fhir.Utility;
using System;

namespace Firely.Fhir.Packages
{
#if BUILDFHIRRELEASEENUM
    // SDK does not have FhirRelease before 2.0
    public enum FhirRelease
    {
        DSTU1,
        DSTU2,
        STU3,
        R4,
        R5,
        R4B
    }
#endif

    [Obsolete("Use Hl7.Fhir.Utility.FhirReleaseParser from the SDK if possible")]
    static class FhirVersions
    {
        public static FhirRelease? TryParse(string version)
        {
            return version switch
            {
                "0.01" => FhirRelease.DSTU1,
                "0.05" => FhirRelease.DSTU1,
                "0.06" => FhirRelease.DSTU1,
                "0.11" => FhirRelease.DSTU1,
                "0.0.80" => FhirRelease.DSTU1,
                "0.0.81" => FhirRelease.DSTU1,
                "0.0.82" => FhirRelease.DSTU1,

                "0.4.0" => FhirRelease.DSTU2,
                "0.5.0" => FhirRelease.DSTU2,
                "1.0.0" => FhirRelease.DSTU2,
                "1.0.1" => FhirRelease.DSTU2,
                "1.0.2" => FhirRelease.DSTU2,

                "1.1.0" => FhirRelease.STU3,
                "1.4.0" => FhirRelease.STU3,
                "1.6.0" => FhirRelease.STU3,
                "1.8.0" => FhirRelease.STU3,
                "3.0.0" => FhirRelease.STU3,
                "3.0.1" => FhirRelease.STU3,
                "3.0.2" => FhirRelease.STU3,

                "3.2.0" => FhirRelease.R4,
                "3.3.0" => FhirRelease.R4,
                "3.5.0" => FhirRelease.R4,
                "3.6.0" => FhirRelease.R4,
                "4.0.0" => FhirRelease.R4,
                "4.0.1" => FhirRelease.R4,
                "4.1.0" => FhirRelease.R4B,

                "4.5.0" => FhirRelease.R5,
                "4.6.0" => FhirRelease.R5,
                _ => null
            };
        }

        public static FhirRelease Parse(string version)
        {
            var release = TryParse(version);
            if (release is null) throw new ArgumentException($"{version} is not a known FHIR version.");
            else return release.Value;
        }

        public static string FhirVersionFromRelease(FhirRelease fhirRelease)
        {
            return fhirRelease switch
            {
                FhirRelease.DSTU1 => "0.0.82",
                FhirRelease.DSTU2 => "1.0.2",
                FhirRelease.STU3 => "3.0.1",
                FhirRelease.R4 => "4.0.0",
                FhirRelease.R4B => "4.1.0",
                FhirRelease.R5 => "4.6.0",
                _ => throw new Exception($"Unknown FHIR version {fhirRelease}")
            };
        }
    }
}
