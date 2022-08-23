/* 
 * Copyright (c) 2022, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/Firely.Fhir.Packages/blob/master/LICENSE
 */


#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Firely.Fhir.Packages
{
    public class PackageRestorer
    {
        private readonly PackageContext _context;
        private PackageClosure _closure;

        /// <summary>
        /// Restores package dependencies
        /// </summary>
        /// <param name="context">Package context of the package to be restored</param>
        public PackageRestorer(PackageContext context)
        {
            this._context = context;
            _closure = new PackageClosure();
        }

        /// <summary>
        /// Restore packages dependencies
        /// </summary>
        /// <returns>Package closure</returns>
        /// <exception cref="AggregateException">AggregateException thrown when a package doesn't have a manifest file and/or a package is invalid.
        /// The inner exceptions can be examined for more detailed information. The inner exceptions can contain exceptions of type
        /// <see cref="PackageRestoreException"/> that contain additional information on the erroneous package.
        /// </exception>
        public async Task<PackageClosure> Restore()
        {
            _closure = new();
            var manifest = await _context.Project.ReadManifest().ConfigureAwait(false);

            if (manifest is null) 
                throw new Exception("This context does not have a package manifest (package.json)");

            var errors = new List<Exception>();

            await restoreManifest(manifest, errors, new Stack<PackageDependency>()).ConfigureAwait(false);

            if (errors.Any())
                throw new AggregateException(errors);

            await SaveClosure().ConfigureAwait(false);
            return _closure;
        }

        /// <summary>
        /// Save closure file to disk
        /// </summary>
        /// <returns></returns>
        public async Task SaveClosure()
        {
            await _context.Project.WriteClosure(_closure).ConfigureAwait(false);
        }

        private async Task restoreManifest(PackageManifest manifest, List<Exception> errors, Stack<PackageDependency> dependencyChain)
        {
            foreach (PackageDependency dependency in manifest.GetDependencies())
            {
                dependencyChain.Push(dependency);
                await restoreDependency(dependency, errors, dependencyChain).ConfigureAwait(false);
                dependencyChain.Pop();
            }
        }

        private async Task restoreDependency(PackageDependency dependency, List<Exception> errors, Stack<PackageDependency> dependencyChain)
        {
            try
            {
                var reference = await _context.CacheInstall(dependency).ConfigureAwait(false);

                if (reference.Found)
                {
                    _closure.Add(reference);
                    await restoreReference(reference, errors, dependencyChain).ConfigureAwait(false);
                }
                else
                {
                    _closure.AddMissing(dependency);
                }
            }
            catch (Exception e)
            {
                errors.Add(new PackageRestoreException(dependencyChain.Reverse(), e));
            }
        }

        private async Task restoreReference(PackageReference reference, List<Exception> errors, Stack<PackageDependency> dependencyChain)
        {
            var manifest = await _context.Cache.ReadManifest(reference);
            if (manifest is not null)
            {
                await restoreManifest(manifest, errors, dependencyChain).ConfigureAwait(false);
            }
        }
    }
}

#nullable restore