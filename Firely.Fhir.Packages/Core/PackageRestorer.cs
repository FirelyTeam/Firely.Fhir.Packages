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
        private readonly Func<PackageDependency, PackageDependency> _dependencyMapper;
        private PackageClosure _closure;

        /// <summary>
        /// Restores package dependencies, using <see cref="DefaultDependencyMapper"/> to map the dependencies
        /// found in package manifests before they are resolved.
        /// </summary>
        /// <param name="context">Package context of the package to be restored</param>
        public PackageRestorer(PackageContext context) : this(context, null)
        {
        }

        /// <summary>
        /// Restores package dependencies
        /// </summary>
        /// <param name="context">Package context of the package to be restored</param>
        /// <param name="dependencyMapper">A function that is called for every dependency found in a package manifest, before it
        /// is resolved. It can return the same dependency, or a different one to redirect the restore to another package or version.
        /// When <c>null</c>, <see cref="DefaultDependencyMapper"/> is used. Pass <c>dependency =&gt; dependency</c> to
        /// disable any redirection.</param>
        public PackageRestorer(PackageContext context, Func<PackageDependency, PackageDependency>? dependencyMapper)
        {
            this._context = context;
            this._dependencyMapper = dependencyMapper ?? DefaultDependencyMapper;
            _closure = new PackageClosure();
        }

        /// <summary>
        /// The mapping that is applied to dependencies by default: even when we don't use version ranges, HL7 expects
        /// us to upgrade core 4.0.0 dependencies (<c>hl7.fhir.r4.core@4.0.0</c>) to 4.0.1 due to a publication error.
        /// All other dependencies are returned unchanged.
        /// </summary>
        public static PackageDependency DefaultDependencyMapper(PackageDependency dependency)
        {
            return dependency is { Name: "hl7.fhir.r4.core", Range: "4.0.0" }
                ? new PackageDependency(dependency.Name, "4.0.1")
                : dependency;
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
            foreach (PackageDependency dependency in manifest.GetDependencies().Select(_dependencyMapper))
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