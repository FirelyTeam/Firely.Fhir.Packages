﻿using System;
using System.Threading.Tasks;

namespace Firely.Fhir.Packages
{

    public static class PackageRestoreExtensions
    {
        public static async Task<PackageReference> CacheInstall(this PackageContext scope, PackageDependency dependency)
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

            if (await scope.Cache.IsInstalled(reference)) return reference;

            var buffer = await scope.Server.GetPackage(reference);
            if (buffer is null) return PackageReference.None;

            await scope.Cache.Install(reference, buffer);
            scope.onInstalled?.Invoke(reference);
            return reference;
        }

        public static async Task<PackageClosure> Restore(this PackageContext context)
        {
            context.Closure = new PackageClosure(); // reset
            var manifest = await context.Project.ReadManifest();
            if (manifest is null) throw new Exception("This context does not have a package manifest (package.json)");

            await context.RestoreManifest(manifest);
            return await context.SaveClosure();
        }

        public static async Task<PackageClosure> SaveClosure(this PackageContext scope)
        {
            await scope.Project.WriteClosure(scope.Closure);
            return scope.Closure;
        }

        private static async Task RestoreManifest(this PackageContext scope, PackageManifest manifest)
        {
            foreach(PackageDependency dependency in manifest.GetDependencies())
            { 
                await scope.RestoreDependency(dependency);
            }
        }

        private static async Task RestoreDependency(this PackageContext scope, PackageDependency dependency)
        {
            var reference = await scope.CacheInstall(dependency);
            if (reference.Found)
            {
                scope.Closure.Add(reference);
                await scope.RestoreReference(reference);
            }
            else
            {
                scope.Closure.AddMissing(dependency);
            }
        }

        private static async Task RestoreReference(this PackageContext scope, PackageReference reference)
        {
            var manifest = await scope.Cache.ReadManifest(reference);
            if (manifest is object)
            {
                await scope.RestoreManifest(manifest);
            }
        }

    }

}
