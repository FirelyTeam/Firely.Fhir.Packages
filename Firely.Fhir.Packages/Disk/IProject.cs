using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Firely.Fhir.Packages
{
    /// <summary>
    /// Only used to get access to the project I/O, this is not about scope
    /// </summary>
    public interface IProject 
    {
        Task<PackageManifest> ReadManifestAsync();
        Task WriteManifestAsync(PackageManifest manifest);
        Task<PackageClosure> ReadClosureAsync();
        Task WriteClosureAsync(PackageClosure closure);

        public Task<string> GetFileContentAsync(string filename);
        public Task<Dictionary<string, string>> GetCanonicalIndexAsync();
    }

    public static class IProjectExtensions
    {
        public static async Task InstallAsync(this IProject project, PackageDependency dependency)
        {
            var manifest = await project.ReadManifestAsync();
            manifest.AddDependency(dependency);
            await project.WriteManifestAsync(manifest);
        }

        public static async Task<bool> RemoveAsync(this IProject project, PackageReference dependency)
        {
            return await project.RemoveAsync(dependency.Name);
        }

        public static async Task<bool> RemoveAsync(this IProject project, string name)
        {
            var manifest = await project.ReadManifestAsync();
            var result = manifest.RemoveDependency(name);
            await project.WriteManifestAsync(manifest);

            return result;
        }

        public static async Task Init(this IProject project, string pkgname, string version, string fhirVersion)
        {
            var manifest = await project.ReadManifestAsync();

            if (manifest != null)
                throw new Exception($"A Package manifests already exists in this folder.");

            if (!ManifestFile.ValidPackageName(pkgname))
                throw new Exception($"Invalid package name {pkgname}");

            manifest = ManifestFile.Create(pkgname, fhirVersion);

            await project.WriteManifestAsync(manifest);
        }
    }
}
