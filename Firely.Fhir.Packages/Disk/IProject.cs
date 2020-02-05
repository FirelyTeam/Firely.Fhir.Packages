using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Firely.Fhir.Packages
{
    /// <summary>
    /// Only used to get access to the project I/O, this is not about scope
    /// </summary>
    public interface IProject 
    {
        PackageManifest ReadManifest();
        void WriteManifest(PackageManifest manifest);
        PackageClosure ReadClosure();
        void WriteClosure(PackageClosure closure);

        public string GetFileContent(string filename);
        public Dictionary<string, string> GetCanonicalIndex();
    }

    public static class IProjectExtensions
    {
        public static void Install(this IProject project, PackageDependency dependency)
        {
            var manifest = project.ReadManifest();
            manifest.AddDependency(dependency);
            project.WriteManifest(manifest);
        }

        public static void Remove(this IProject project, PackageReference dependency)
        {
            var manifest = project.ReadManifest();
            manifest.RemoveDependency(dependency.Name);
            project.WriteManifest(manifest);
        }

        public static void Init(this IProject project, string pkgname, string version, string fhirVersion)
        {
            var manifest = project.ReadManifest();

            if (manifest != null)
                throw new Exception($"A Package manifests already exists in this folder.");

            if (!ManifestFile.ValidPackageName(pkgname))
                throw new Exception($"Invalid package name {pkgname}");

            manifest = ManifestFile.Create(pkgname, fhirVersion);
            project.WriteManifest(manifest);
        }
    }
}
