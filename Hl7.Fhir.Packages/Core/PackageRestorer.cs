using System;
using System.Linq;
using System.Threading.Tasks;

namespace Hl7.Fhir.Packages
{

    public class PackageRestorer
    {
        private readonly PackageClient client;
        private readonly PackageInstaller installer;

        public PackageRestorer(PackageClient client, PackageInstaller installer)
        {
            this.client = client;
            this.installer = installer;
        }
 
        public async Task<PackageAssets> Restore(PackageManifest manifest)
        {
            var assets = new PackageAssets();
            await RestoreManifest(manifest, assets);
            return assets; 
        }

        private async Task RestoreManifest(PackageManifest manifest, PackageAssets assets)
        {
            foreach(PackageReference dep in manifest.GetDependencies())
            {
                await RestoreReference(dep, assets);
            }
        }

        private async Task RestoreReference(PackageReference reference, PackageAssets assets)
        {
            var actual = await installer.ResolveReference(reference);

            if (actual.IsEmpty)
            {
                assets.AddMissing(reference);
                return; throw new Exception($"Package {reference} was not found.");
            }

            if (assets.AddRef(actual))
            {
                var manifest = await installer.InstallPackage(actual);

                if (manifest != null)
                    await RestoreManifest(manifest, assets);
            }
            // later, we should have a algoritm for detecting and resolving conflicts. --mh
            
        }
    }

}
