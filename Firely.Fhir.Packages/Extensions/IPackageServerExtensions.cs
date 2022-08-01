/* 
 * Copyright (c) 2022, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/Firely.Fhir.Packages/blob/master/LICENSE
 */


#nullable enable

using System.Threading.Tasks;

namespace Firely.Fhir.Packages
{
    public static class PackageServerExtensions
    {
        /// <summary>
        /// Gets the version of a package dependency
        /// </summary>
        /// <param name="server"></param>
        /// <param name="dependency">The package dependency of which the versions need to be returned</param>
        /// <returns>The known version of the package dependency</returns>
        public static async ValueTask<Versions?> GetVersions(this IPackageServer server, PackageDependency dependency)
        {
            return await server.GetVersions(dependency.Name);
        }


        internal static async ValueTask<PackageReference> Resolve(this IPackageServer server, PackageDependency dependency)
        {
            var versions = await server.GetVersions(dependency.Name);
            var version = versions?.Resolve(dependency.Range)?.ToString(); //null => NotFound
            return version is null ? PackageReference.None : new PackageReference(dependency.Name, version);
        }

        /// <summary>
        /// Get latest version of a certain package
        /// </summary>
        /// <param name="server"></param>
        /// <param name="name">Package name</param>
        /// <returns>The package reference of the latest version of the package</returns>
        public static async ValueTask<PackageReference> GetLatest(this IPackageServer server, string name)
        {
            var versions = await server.GetVersions(name);
            var version = versions?.Latest()?.ToString();
            return version is null ? PackageReference.None : new PackageReference(name, version);
        }

        /// <summary>
        /// Check whether the server can find a certain dependency
        /// </summary>
        /// <param name="server"></param>
        /// <param name="dependency">The dependency to be search for</param>
        /// <returns>whether the server can find a certain dependency</returns>
        public async static ValueTask<bool> HasMatch(this IPackageServer server, PackageDependency dependency)
        {
            var reference = await server.Resolve(dependency);
            return await Task.FromResult(reference.Found);
        }
    }
}

#nullable restore