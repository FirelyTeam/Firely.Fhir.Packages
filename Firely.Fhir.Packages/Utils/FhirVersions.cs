using System;

namespace Firely.Fhir.Packages
{
    public static class FhirVersions
    {
        public static int Parse(string version)
        {
            return version switch
            {
                "0.01" => 1, 
                "0.05" => 1, 
                "0.06" => 1,
                "0.11" => 1,
                "0.0.80" => 1,
                "0.0.81" => 1,
                "0.0.82" => 1,

                "0.4.0" => 2,   
                "0.5.0" => 2,   
                "1.0.0" => 2, 
                "1.0.1" => 2,
                "1.0.2" => 2,

                "1.1.0" => 3,
                "1.4.0" => 3,
                "1.6.0" => 3,   
                "1.8.0" => 3,   
                "3.0.0" => 3,
                "3.0.1" => 3,
                "3.0.2" => 3,

                "3.2.0" => 4,
                "3.3.0" => 4,
                "3.5.0" => 4,
                "3.6.0" => 4,
                "4.0.0" => 4,
                "4.0.1" => 4,
                "4.5.0" => 5,
                _ => -1
            };
        }

        public static string GetFhirVersion(int fhirRelease)
        {
            return fhirRelease switch
            {
                1 => "0.0.82",
                2 => "1.0.2",
                3 => "3.0.1",
                4 => "4.0.0",
                5 => "4.5.0",
                _ => throw new Exception($"Unknown FHIR version {fhirRelease}")
            };
        }
    }
}
