#nullable enable

using System.Threading.Tasks;

namespace Firely.Fhir.Packages
{
    public static class PackageRestoreExtensions
    {
        internal static async Task<PackageReference> CacheInstall(this PackageContext context, PackageDependency dependency)
        {
            PackageReference reference;

            if (context.Server is not null)
            {
                reference = await context.Server.Resolve(dependency);
                if (!reference.Found) return reference;
            }
            else
            {
                reference = await context.Cache.Resolve(dependency);
                if (!reference.Found) return reference;
            }

            if (await context.Cache.IsInstalled(reference)) return reference;


            byte[]? buffer = null;

            if (context.Server is not null)
                buffer = await context.Server.GetPackage(reference);

            if (buffer is null) return PackageReference.None;

            await context.Cache.Install(reference, buffer);
            context.onInstalled?.Invoke(reference);
            return reference;
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