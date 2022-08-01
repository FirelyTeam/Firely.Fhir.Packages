/* 
 * Copyright (c) 2022, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/Firely.Fhir.Packages/blob/master/LICENSE
 */


#nullable enable


using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Firely.Fhir.Packages
{
    public static class Platform
    {
        private enum OperatingSystem { Windows, Linux, OSX, Unknown };


        private static OperatingSystem getPlatform()
        {
#if !NET452
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? OperatingSystem.Windows
                : RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? OperatingSystem.Linux
                : RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? OperatingSystem.OSX
                : OperatingSystem.Unknown;
#else
            // RuntimeInformation needs NET471
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT: return OperatingSystem.Windows;
                case PlatformID.Unix: return OperatingSystem.Linux;
                case PlatformID.MacOSX: return OperatingSystem.OSX;
            //  case (PlatformID)128: return OperatingSystem.Linux; // Mono
                default: return OperatingSystem.Unknown;
            }
#endif
        }

        private static string getGenericDataLocation()
        {
            string? path = getPlatform() switch
            {
                OperatingSystem.Windows =>
                    Environment.GetEnvironmentVariable("UserProfile"),

                OperatingSystem.Linux =>
                     Environment.GetEnvironmentVariable("HOME"),

                OperatingSystem.OSX =>
                   Environment.GetEnvironmentVariable("HOME"),

                _ => throw new Exception("Unknown OS")
            };

            return path == null ? throw new Exception("Cannot determine rootpath of operating system") : path;
        }

        /// <summary>
        /// Return the FHIR packages folder location
        /// </summary>
        /// <returns>The path of the package root</returns>
        public static string GetFhirPackageRoot()
        {
            string root = getGenericDataLocation();

            return Path.Combine(root, ".fhir", "packages");
        }
    }
}

#nullable restore