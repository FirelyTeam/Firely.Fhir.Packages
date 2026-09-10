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
            foreach (PackageDependency dependency in upgradeDependencies(manifest.GetDependencies()))
            {
                dependencyChain.Push(dependency);
                await restoreDependency(dependency, errors, dependencyChain).ConfigureAwait(false);
                dependencyChain.Pop();
            }
        }

        // Even when we don't use version ranges, HL7 expects us to upgrade core 4.0.0 dependencies to
        // 4.0.1 due to a publication error. This is a temporary fix until the next release, so we'll have
        // to manually fix this here.
        private static IEnumerable<PackageDependency> upgradeDependencies(IEnumerable<PackageDependency> original)
        {
            foreach (var dep in original)
            {
                if (dep is { Name: "hl7.fhir.r4.core", Range: "4.0.0" })
                    yield return new PackageDependency(dep.Name, "4.0.1");
                else
                    yield return dep;
            }
        }

        private async Task restoreDependency(PackageDependency dependency, List<Exception> errors, Stack<PackageDependency> dependencyChain)
        {
            try
            {
                validateVersionPattern(dependency);

                var reference = await _context.CacheInstall(dependency).ConfigureAwait(false);

                if (reference.Found)
                {
                    bool added = _closure.Add(reference);
                    if (added)
                    {
                        await restoreReference(reference, errors, dependencyChain).ConfigureAwait(false);
                    }
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

        // Versions.Resolve is total: it returns null for version patterns that cannot be interpreted.
        // Restore stays strict: an invalid version pattern in a manifest is reported here as an
        // explicit ArgumentException (wrapped in a PackageRestoreException by the caller) instead of
        // ending up as a silently missing dependency.
        private static void validateVersionPattern(PackageDependency dependency)
        {
            var pattern = dependency.Range;
            if (pattern is not null && pattern != "latest" && pattern.Length > 0
                && !SemanticVersioning.Range.TryParse(pattern, out _))
            {
                throw new ArgumentException($"Invalid version string: \"{pattern}\"");
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