#nullable enable

using System;
using System.Threading.Tasks;

namespace Firely.Fhir.Packages
{
    public static class IProjectExtensions
    {
        /// <summary>
        /// Add a dependency
        /// </summary>
        /// <param name="project"></param>
        /// <param name="dependency">Dependency to be added</param>
        /// <returns></returns>
        public static async Task AddDependency(this IProject project, PackageDependency dependency)
        {
            var manifest = await project.ReadManifest().ConfigureAwait(false);
            if (manifest is not null)
            {
                manifest.AddDependency(dependency);
                await project.WriteManifest(manifest).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Remove a dependency
        /// </summary>
        /// <param name="project"></param>
        /// <param name="dependency">Dependency to be removed</param>
        /// <returns>Whether to removal was successful</returns>
        public static async Task<bool> RemoveDependency(this IProject project, PackageDependency dependency)
        {
            if (dependency.Name is null)
                return false;

            return await project.RemoveDependency(dependency.Name).ConfigureAwait(false);
        }

        /// <summary>
        /// Remove a dependency by dependency by name
        /// </summary>
        /// <param name="project"></param>
        /// <param name="name">Dependency name</param>
        /// <returns></returns>
        public static async Task<bool> RemoveDependency(this IProject project, string name)
        {
            var manifest = await project.ReadManifest();
            if (manifest is null) return false;

            var result = manifest.RemoveDependency(name);
            await project.WriteManifest(manifest).ConfigureAwait(false);

            return result;
        }

        /// <summary>
        /// Initialize a package manifest 
        /// </summary>
        /// <param name="project"></param>
        /// <param name="pkgname">Name of the package</param>
        /// <param name="version">Version of the package</param>
        /// <param name="fhirVersion">FhIr version</param>
        /// <returns></returns>
        /// <exception cref="Exception">Function throws an exception if a manifest file already exists, or if the package name is invalid</exception>
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

        /// <summary>
        /// Checks whether a manifest file already exists
        /// </summary>
        /// <param name="project"></param>
        /// <returns>whether a manifest file already exists</returns>
        public static async Task<bool> HasManifest(this IProject project)
        {
            var manifest = await project.ReadManifest().ConfigureAwait(false);
            return manifest is not null;
        }

        /// <summary>
        /// Checks whether a lock file already exists
        /// </summary>
        /// <param name="project"></param>
        /// <returns>whether a lock file already exists</returns>
        public static async Task<bool> HasClosure(this IProject project)
        {
            var closure = await project.ReadClosure().ConfigureAwait(false);
            return closure is not null;
        }

    }
}

#nullable restore