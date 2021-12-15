using System.Threading.Tasks;

namespace Firely.Fhir.Packages
{

    public static class PackageRestoreExtensions
    {
        public static async Task<PackageReference> CacheInstall(this PackageContext context, PackageDependency dependency)
        {

            PackageReference reference = await context.Resolve(dependency);

            if (reference.NotFound) 
                return PackageReference.None;

            if (await context.Cache.IsInstalled(reference)) return reference;

            var buffer = await context.Server.GetPackage(reference);
            if (buffer is null) return PackageReference.None;

            await context.Cache.Install(reference, buffer);
            context.onInstalled?.Invoke(reference);
            return reference;
        }

        public static async Task<PackageReference> Resolve(this PackageContext context, PackageDependency dependency)
        {
            if (context.Server is object)
            {
                PackageReference reference = await context.Server.Resolve(dependency);
                if (reference.Found) return reference;
            }
            
            return await context.Cache.Resolve(dependency);
        }



        public static async Task<PackageClosure> Restore(this PackageContext context)
        {
            var restorer = new PackageRestorer(context);
            return await restorer.Restore();
        }

    }

}
