#nullable enable

using System;
using System.Threading.Tasks;

namespace Firely.Fhir.Packages
{
    public class PackageRestorer
    {
        private readonly PackageContext _context;
        private PackageClosure _closure;

        public PackageRestorer(PackageContext context)
        {
            this._context = context;
            _closure = new PackageClosure();
        }

        public async Task<PackageClosure> Restore()
        {
            _closure = new();
            var manifest = await _context.Project.ReadManifest().ConfigureAwait(false);
            if (manifest is null) throw new Exception("This context does not have a package manifest (package.json)");

            await restoreManifest(manifest).ConfigureAwait(false);
            await SaveClosure().ConfigureAwait(false);
            return _closure;
        }

        public async Task SaveClosure()
        {
            await _context.Project.WriteClosure(_closure).ConfigureAwait(false);
        }

        private async Task restoreManifest(PackageManifest manifest)
        {
            foreach (PackageDependency dependency in manifest.GetDependencies())
            {
                await restoreDependency(dependency).ConfigureAwait(false);
            }
        }

        private async Task restoreDependency(PackageDependency dependency)
        {
            var reference = await _context.CacheInstall(dependency);
            if (reference.Found)
            {
                _closure.Add(reference);
                await restoreReference(reference).ConfigureAwait(false);
            }
            else
            {
                _closure.AddMissing(dependency);
            }
        }

        private async Task restoreReference(PackageReference reference)
        {
            var manifest = await _context.Cache.ReadManifest(reference);
            if (manifest is not null)
            {
                await restoreManifest(manifest).ConfigureAwait(false);
            }
        }
    }
}

#nullable restore