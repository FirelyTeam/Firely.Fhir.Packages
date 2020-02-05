using System;
using System.Threading.Tasks;

namespace Firely.Fhir.Packages
{

    public static class PackageRestorer
    {

        public static async Task<PackageClosure> Restore(this PackageScope scope)
        {
            var closure = new PackageClosure();
            var manifest = scope.Project.ReadManifest();
            await RestoreManifest(scope.Cache, manifest, closure);
            scope.Project.WriteClosure(closure);
            return closure; 
        }
 
        private static async Task RestoreManifest(IPackageCache cache, PackageManifest manifest, PackageClosure closure)
        {
            foreach(PackageDependency dependency in manifest.GetDependencies())
            {
                await RestoreDependency(cache, dependency, closure);
            }
        }

        private static async Task RestoreDependency(this IPackageCache cache, PackageDependency dependency, PackageClosure closure)
        {
            var reference = await cache.Install(dependency);

            if (reference.Found)
            {
                if (closure.Add(reference)) // conflicts are resolved by: highest = winner.
                {
                    var manifest = cache.ReadManifest(reference);
                    if (manifest is object)
                        await RestoreManifest(cache, manifest, closure);
                }
            }
            else 
            {
                closure.AddMissing(dependency);
            }
        }
    }

}
