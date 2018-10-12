using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Hl7.Fhir.Packages 
{
    public static class Platform
    {
        public enum OperatingSystem { Windows, Linux, OSX, Unknown };


        public static OperatingSystem GetPlatform()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return OperatingSystem.Windows;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return OperatingSystem.Linux;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) return OperatingSystem.OSX;
            else return default;
        }

        public static string GetGenericDataLocation()
        {
            switch(GetPlatform())
            {
                case OperatingSystem.Windows:
                    return Environment.GetEnvironmentVariable("UserProfile");
                    
                case OperatingSystem.Linux:
                    {
                        var path = Environment.GetEnvironmentVariable("HOME");
                        return Path.Combine(path, ".local/share");
                    }
                case OperatingSystem.OSX:
                    {
                        var path = Environment.GetEnvironmentVariable("HOME");
                        return path;
                        //return Path.Combine(path, "/Library/Application Support");
                    }
                default: return null;
            }
        }

        public static string GetFhirPackageRoot()
        {
            string root = GetGenericDataLocation();

            return Path.Combine(root, ".fhir", "packages");
        }

    }
}