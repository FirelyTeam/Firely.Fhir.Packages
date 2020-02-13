using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Firely.Fhir.Packages
{
    public class FolderProject : IProject
    {
        string folder;

        public FolderProject(string folder)
        {
            this.folder = folder;
        }

        public Task<Dictionary<string, string>> GetCanonicalIndexAsync()
        {
            // this should be cached, but we need to bust it on changes.
            return Task.FromResult(CanonicalIndexer.IndexFolder(folder));
        }

        public Task<string> GetFileContentAsync(string filename)
        {
            var path = Path.Combine(folder, filename);
            return Task.FromResult(File.ReadAllText(path));
        }

        public Task<PackageClosure> ReadClosureAsync()
        {
            var closure = LockFile.ReadFromFolder(folder);
            return Task.FromResult(closure);
        }

        public Task<PackageManifest> ReadManifestAsync()
        {
            return Task.FromResult(ManifestFile.ReadFromFolder(folder));
        }

        public Task WriteClosureAsync(PackageClosure closure)
        {
            LockFile.WriteToFolder(closure, folder);

            return Task.FromResult(0); //because in net45 there is no Task.CompletedTask (Paul)
        }

        public Task WriteManifestAsync(PackageManifest manifest)
        {
            ManifestFile.WriteToFolder(manifest, folder, merge: true);

            return Task.FromResult(0); //because in net45 there is no Task.CompletedTask (Paul)
        }
    }

}
