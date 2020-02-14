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
        Task<PackageManifest> ReadManifest();
        Task WriteManifest(PackageManifest manifest);
        Task<PackageClosure> ReadClosure();
        Task WriteClosure(PackageClosure closure);

        public Task<string> GetFileContent(string filename);
        public Task<Dictionary<string, string>> GetCanonicalIndex();
    }

    public static class IProjectExtensions
    {
        public static async Task Install(this IProject project, PackageDependency dependency)
        {
            var manifest = await project.ReadManifest();
            manifest.AddDependency(dependency);
            await project.WriteManifest(manifest);
        }

        public static async Task<bool> Remove(this IProject project, PackageReference dependency)
        {
            return await project.Remove(dependency.Name);
        }

        public static async Task<bool> Remove(this IProject project, string name)
        {
            var manifest = await project.ReadManifest();
            var result = manifest.RemoveDependency(name);
            await project.WriteManifest(manifest);

            return result;
        }

        public static async Task Init(this IProject project, string pkgname, string version, string fhirVersion)
        {
            var manifest = await project.ReadManifest();

            if (manifest != null)
                throw new Exception($"A Package manifests already exists in this folder.");

            if (!ManifestFile.ValidPackageName(pkgname))
                throw new Exception($"Invalid package name {pkgname}");

            manifest = ManifestFile.Create(pkgname, fhirVersion);

            await project.WriteManifest(manifest);
        }
    }
}
