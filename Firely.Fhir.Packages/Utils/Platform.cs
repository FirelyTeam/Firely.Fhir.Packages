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

        /// <summary>
        /// Name of the environment variable that overrides the location of the FHIR package cache.
        /// Its value is used as the package root itself (packages are stored directly in it).
        /// </summary>
        public const string PackageCacheEnvironmentVariable = "FHIR_PACKAGE_CACHE";

        /// <summary>
        /// Optional process-wide override of the FHIR package cache location. When set, it takes precedence over the
        /// <see cref="PackageCacheEnvironmentVariable"/> environment variable and the default location.
        /// Its value is used as the package root itself (packages are stored directly in it).
        /// Note that this is global state, for a per-instance location pass a cache folder to the <see cref="FhirPackageSource"/>
        /// or <see cref="DiskPackageCache"/> instead.
        /// </summary>
        public static string? PackageRoot { get; set; }

        private static string? getUserProfileLocation(Func<string, string?> getEnvironmentVariable, Func<Environment.SpecialFolder, string> getFolderPath)
        {
            string? path = getPlatform() switch
            {
                OperatingSystem.Windows => getEnvironmentVariable("UserProfile"),
                OperatingSystem.Linux => getEnvironmentVariable("HOME"),
                OperatingSystem.OSX => getEnvironmentVariable("HOME"),
                _ => getFolderPath(Environment.SpecialFolder.UserProfile)
            };

            return string.IsNullOrWhiteSpace(path) ? null : path;
        }

        /// <summary>
        /// Return the FHIR packages folder location. The location is determined as follows, the first one available wins:
        /// <list type="number">
        /// <item><see cref="PackageRoot"/></item>
        /// <item>the <c>FHIR_PACKAGE_CACHE</c> environment variable</item>
        /// <item><c>.fhir/packages</c> in the user profile (<c>%UserProfile%</c> on Windows, <c>$HOME</c> elsewhere)</item>
        /// <item><c>.fhir/packages</c> in the machine-wide application data folder (<see cref="Environment.SpecialFolder.CommonApplicationData"/>),
        /// for processes that have no user profile, such as services or IIS application pools.</item>
        /// </list>
        /// </summary>
        /// <returns>The path of the package root</returns>
        public static string GetFhirPackageRoot() =>
            ResolveFhirPackageRoot(PackageRoot, Environment.GetEnvironmentVariable, Environment.GetFolderPath);

        internal static string ResolveFhirPackageRoot(string? packageRootOverride, Func<string, string?> getEnvironmentVariable, Func<Environment.SpecialFolder, string> getFolderPath)
        {
            if (!string.IsNullOrWhiteSpace(packageRootOverride))
                return packageRootOverride!;

            var fromEnvironment = getEnvironmentVariable(PackageCacheEnvironmentVariable);
            if (!string.IsNullOrWhiteSpace(fromEnvironment))
                return fromEnvironment!;

            var root = getUserProfileLocation(getEnvironmentVariable, getFolderPath);
            if (root is null)
            {
                root = getFolderPath(Environment.SpecialFolder.CommonApplicationData);
                if (string.IsNullOrWhiteSpace(root))
                    throw new Exception($"Cannot determine the location of the FHIR package cache. Specify it with the {PackageCacheEnvironmentVariable} environment variable or by passing a cache folder.");
            }

            return Path.Combine(root, ".fhir", "packages");
        }
    }
}

#nullable restore