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
    public static class PackageRestoreExtensions
    {
        internal static async Task<PackageReference> CacheInstall(this PackageContext context, PackageDependency dependency)
        {

            PackageReference reference = await context.Resolve(dependency);

            if (reference.NotFound)
                return PackageReference.None;

            if (await context.Cache.IsInstalled(reference)) return reference;

            var buffer = (context.Server != null) ? await context.Server.GetPackage(reference) : null;
            if (buffer is null) return PackageReference.None;

            await context.Cache.Install(reference, buffer);
            context.onInstalled?.Invoke(reference);
            return reference;
        }

        /// <summary>
        /// Resolve asks the configured server of this context to resolve a package and if it can't find it, fallback to the 
        /// configured cache to resolve it.
        /// </summary>
        public static async Task<PackageReference> Resolve(this PackageContext context, PackageDependency dependency)
        {
            if (context.Server is object)
            {
                PackageReference reference = await context.Server.Resolve(dependency);
                if (reference.Found) return reference;
            }

            return await context.Cache.Resolve(dependency);
        }


        /// <summary>
        /// Restores a package
        /// </summary>
        /// <param name="context"></param>
        /// <returns>Package lock file</returns>
        public static async Task<PackageClosure> Restore(this PackageContext context)
        {
            var restorer = new PackageRestorer(context);
            return await restorer.Restore();
        }
    }
}

#nullable restore