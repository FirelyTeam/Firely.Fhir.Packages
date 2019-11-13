using System;
using System.Threading.Tasks;

namespace Hl7.Fhir.Packages
{

    public static class PackageRestorer
    {

        public static async Task<Dependencies> Restore(this IPackageInstaller installer, PackageManifest manifest)
        {
            var dependencies = new Dependencies();
            await installer.RestoreManifest(manifest, dependencies);
            return dependencies; 
        }

        private static async Task RestoreManifest(this IPackageInstaller installer, PackageManifest manifest, Dependencies dependencies)
        {
            foreach(PackageDependency reference in manifest.GetDependencies())
            {
                await RestoreReference(installer, reference, dependencies);
            }
        }

        private static async Task RestoreReference(this IPackageInstaller installer, PackageDependency dependency, Dependencies dependencies)
        {
            var reference = await installer.ResolveDependency(dependency);

            if (reference.Found)
            {
                if (dependencies.Add(reference)) // conflicts are resolved by: highest = winner.
                {
                    var manifest = await installer.InstallPackage(reference);

                    if (manifest != null)
                        await installer.RestoreManifest(manifest, dependencies);
                }

            }
            else 
            {
                var cachematch = await installer.IsInstalled(dependency);
                if (!cachematch)
                {
                    dependencies.AddMissing(dependency);
                    
                }
            }
        }
    }

}
