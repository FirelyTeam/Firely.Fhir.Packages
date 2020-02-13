using System.Threading.Tasks;

namespace Firely.Fhir.Packages
{

    public static class PackageRestorer
    {
        public static async Task<PackageReference> InstallAsync(this PackageScope scope, PackageDependency dependency)
        {
            PackageReference reference;

            if (scope.Server is object)
            {
                reference = await scope.Server.ResolveAsync(dependency);
                if (!reference.Found) return reference;
            }
            else
            {
                reference = await scope.Cache.ResolveAsync(dependency);
                if (!reference.Found) return reference;
            }

            if (await scope.Cache.IsInstalledAsync(reference)) return reference;

            var buffer = await scope.Server.GetPackageAsync(reference);
            if (buffer is null) return PackageReference.None;

            await scope.Cache.InstallAsync(reference, buffer);
            scope.Report?.Invoke($"Installed {reference}.");
            return reference;
        }

        public static async Task<PackageClosure> RestoreAsync(this PackageScope scope)
        {
            var closure = new PackageClosure();
            var manifest = await scope.Project.ReadManifestAsync();
            await RestoreManifestAsync(scope, manifest, closure);
            await scope.Project.WriteClosureAsync(closure);

            return closure; 
        }
 
        private static async Task RestoreManifestAsync(PackageScope scope, PackageManifest manifest, PackageClosure closure)
        {
            foreach(PackageDependency dependency in manifest.GetDependencies())
            { 
                await RestoreDependencyAsync(scope, dependency, closure);
            }
        }

        private static async Task RestoreDependencyAsync(this PackageScope scope, PackageDependency dependency, PackageClosure closure)
        {
            var reference = await scope.InstallAsync(dependency);

            if (reference.Found)
            {
                if (closure.Add(reference)) // conflicts are resolved by: highest = winner.
                {
                    var manifest = await scope.Cache.ReadManifestAsync(reference);

                    if (manifest is object)
                        await RestoreManifestAsync(scope, manifest, closure);
                }
            }
            else 
            {
                closure.AddMissing(dependency);
            }
        }
    }

}
