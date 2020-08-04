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

        public Task<List<ResourceMetadata>> GetIndex()
        {
            // this should be cached, but we need to bust it on changes.
            return Task.FromResult(CanonicalIndexer.IndexFolder(folder));
        }

        public Task<string> GetFileContent(string filename)
        {
            var path = Path.Combine(folder, filename);
            return Task.FromResult(File.ReadAllText(path));
        }

        public Task<PackageClosure> ReadClosure()
        {
            var closure = LockFile.ReadFromFolder(folder);
            return Task.FromResult(closure);
        }

        public Task<PackageManifest> ReadManifest()
        {
            return Task.FromResult(ManifestFile.ReadFromFolder(folder));
        }

        public Task WriteClosure(PackageClosure closure)
        {
            LockFile.WriteToFolder(closure, folder);

            return Task.FromResult(0); //because in net45 there is no Task.CompletedTask (Paul)
        }

        public Task WriteManifest(PackageManifest manifest)
        {
            ManifestFile.WriteToFolder(manifest, folder, merge: true);

            return Task.FromResult(0); //because in net45 there is no Task.CompletedTask (Paul)
        }
    }

}
