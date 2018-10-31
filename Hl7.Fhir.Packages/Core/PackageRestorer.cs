using System;
using System.Threading.Tasks;

namespace Hl7.Fhir.Packages
{

    public class PackageRestorer
    {
        readonly PackageClient client;
        readonly PackageInstaller installer;

        public PackageRestorer(PackageClient client, PackageInstaller installer)
        {
            this.client = client;
            this.installer = installer;
        }
 
        public async Task<Dependencies> Restore(PackageManifest manifest)
        {
            var dependencies = new Dependencies();
            await RestoreManifest(manifest, dependencies);
            return dependencies; 
        }

        private async Task RestoreManifest(PackageManifest manifest, Dependencies dependencies)
        {
            foreach(PackageReference dep in manifest.GetDependencies())
            {
                await RestoreReference(dep, dependencies);
            }
        }

        private async Task RestoreReference(PackageReference reference, Dependencies dependencies)
        {
            var actual = await installer.ResolveReference(reference);

            if (actual.IsEmpty)
            {
                dependencies.AddMissing(reference);
                return; throw new Exception($"Package {reference} was not found.");
            }

            if (dependencies.AddRef(actual)) // conflicts are resolved by: highest = winner.
            {
                var manifest = await installer.InstallPackage(actual);

                if (manifest != null)
                    await RestoreManifest(manifest, dependencies);
            }
            
            
        }
    }

}
