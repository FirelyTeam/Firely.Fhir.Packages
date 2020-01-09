using System;
using System.Threading.Tasks;

namespace Firely.Fhir.Packages
{

    public static class PackageRestorer
    {

        public static async Task<Closure> Restore(this PackageInstaller installer, PackageManifest manifest)
        {
            var closure = new Closure();
            await installer.RestoreManifest(manifest, closure);
            return closure; 
        }

        private static async Task RestoreManifest(this PackageInstaller installer, PackageManifest manifest, Closure closure)
        {
            foreach(PackageDependency reference in manifest.GetDependencies())
            {
                await RestoreDependency(installer, reference, closure);
            }
        }

        private static async Task RestoreDependency(this PackageInstaller installer, PackageDependency dependency, Closure closure)
        {
            var reference = await installer.ResolveDependency(dependency);

            if (reference.Found)
            {
                if (closure.Add(reference)) // conflicts are resolved by: highest = winner.
                {
                    var manifest = await installer.InstallPackage(reference);

                    if (manifest is object)
                        await installer.RestoreManifest(manifest, closure);
                }

            }
            else 
            {
                var installed = await installer.IsInstalled(dependency);
                if (!installed)
                {
                    closure.AddMissing(dependency);
                    
                }
            }
        }
    }

}
