﻿#nullable enable

using System;
using System.Threading.Tasks;

namespace Firely.Fhir.Packages
{
    public static class IProjectExtensions
    {
        public static async Task AddDependency(this IProject project, PackageDependency dependency)
        {
            var manifest = await project.ReadManifest().ConfigureAwait(false);
            if (manifest is not null)
            {
                manifest.AddDependency(dependency);
                await project.WriteManifest(manifest).ConfigureAwait(false);
            }
        }

        public static async Task<bool> RemoveDependency(this IProject project, PackageReference dependency)
        {
            if (dependency.Name is null)
                return false;

            return await project.RemoveDependency(dependency.Name).ConfigureAwait(false);
        }

        public static async Task<bool> RemoveDependency(this IProject project, string name)
        {
            var manifest = await project.ReadManifest();
            if (manifest is null) return false;

            var result = manifest.RemoveDependency(name);
            await project.WriteManifest(manifest).ConfigureAwait(false);

            return result;
        }

        public static async Task Init(this IProject project, string pkgname, string version, string fhirVersion)
        {
            var manifest = await project.ReadManifest().ConfigureAwait(false);

            if (manifest != null)
                throw new Exception($"A Package manifests already exists in this folder.");

            if (!ManifestFile.ValidPackageName(pkgname))
                throw new Exception($"Invalid package name {pkgname}");

            manifest = ManifestFile.Create(pkgname, fhirVersion);
            manifest.Version = version;

            await project.WriteManifest(manifest).ConfigureAwait(false);
        }

        public static async Task<bool> HasManifest(this IProject project)
        {
            var manifest = await project.ReadManifest().ConfigureAwait(false);
            return manifest is not null;
        }

        public static async Task<bool> HasClosure(this IProject project)
        {
            var closure = await project.ReadClosure().ConfigureAwait(false);
            return closure is not null;
        }

    }
}

#nullable restore