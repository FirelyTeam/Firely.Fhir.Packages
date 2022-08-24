using System;
using System.Threading.Tasks;

namespace Firely.Fhir.Packages
{
    public class PackageRestorer
    {
        private PackageContext context;
        private PackageClosure closure;

        public PackageRestorer(PackageContext context)
        {
            this.context = context;
        }

        public async Task<PackageClosure> Restore()
        {
            closure = new();
            var manifest = await context.Project.ReadManifest();
            if (manifest is null) throw new Exception("This context does not have a package manifest (package.json)");

            await RestoreManifest(manifest);
            await SaveClosure();
            return closure;
        }

        public async Task SaveClosure()
        {
            await context.Project.WriteClosure(closure);
        }

        private async Task RestoreManifest(PackageManifest manifest)
        {
            foreach (PackageDependency dependency in manifest.GetDependencies())
            {
                await RestoreDependency(dependency);
            }
        }

        private async Task RestoreDependency(PackageDependency dependency)
        {
            var reference = await context.CacheInstall(dependency);
            if (reference.Found)
            {
                bool added = closure.Add(reference);
                if (added)
                    await RestoreReference(reference);
            }
            else
            {
                closure.AddMissing(dependency);
            }
        }

        private async Task RestoreReference(PackageReference reference)
        {
            var manifest = await context.Cache.ReadManifest(reference);
            if (manifest is object)
            {
                await RestoreManifest(manifest);
            }
        }

    }

}
