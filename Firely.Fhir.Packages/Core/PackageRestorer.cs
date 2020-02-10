using System.Threading.Tasks;

namespace Firely.Fhir.Packages
{

    public static class PackageRestorer
    {
        public static async Task<PackageReference> Install(this PackageScope scope, PackageDependency dependency)
        {
            PackageReference reference;

            if (scope.Server is object)
            {
                reference = await scope.Server.Resolve(dependency);
                if (!reference.Found) return reference;
            }
            else
            {
                reference = await scope.Cache.Resolve(dependency);
                if (!reference.Found) return reference;

            }

            if (scope.Cache.IsInstalled(reference)) return reference;

            var buffer = await scope.Server.GetPackageAsync(reference);
            if (buffer is null) return PackageReference.None;

            await scope.Cache.Install(reference, buffer);
            scope.Report?.Invoke($"Installed {reference}.");
            return reference;
        }

        public static async Task<PackageClosure> Restore(this PackageScope scope)
        {
            var closure = new PackageClosure();
            var manifest = scope.Project.ReadManifest();
            await RestoreManifest(scope, manifest, closure);
            scope.Project.WriteClosure(closure);
            return closure; 
        }
 
        private static async Task RestoreManifest(PackageScope scope, PackageManifest manifest, PackageClosure closure)
        {
            foreach(PackageDependency dependency in manifest.GetDependencies())
            { 
                await RestoreDependency(scope, dependency, closure);
            }
        }

        private static async Task RestoreDependency(this PackageScope scope, PackageDependency dependency, PackageClosure closure)
        {
            var reference = await scope.Install(dependency);

            if (reference.Found)
            {
                if (closure.Add(reference)) // conflicts are resolved by: highest = winner.
                {
                    var manifest = scope.Cache.ReadManifest(reference);
                    if (manifest is object)
                        await RestoreManifest(scope, manifest, closure);
                }
            }
            else 
            {
                closure.AddMissing(dependency);
            }
        }
    }

}
