using System.Collections.Generic;
using System.IO;

namespace Firely.Fhir.Packages
{
    public class FolderProject : IProject
    {
        string folder;

        public FolderProject(string folder)
        {
            this.folder = folder;
        }

        public Dictionary<string, string> GetCanonicalIndex()
        {
            // this should be cached, but we need to bust it on changes.
            return CanonicalIndexer.IndexFolder(folder);
        }

        public string GetFileContent(string filename)
        {
            var path = Path.Combine(folder, filename);
            return File.ReadAllText(path);
        }

        public PackageClosure ReadClosure()
        {
            var closure = LockFile.ReadFromFolder(folder);
            return closure;
        }

        public PackageManifest ReadManifest()
        {
            return ManifestFile.ReadFromFolder(folder);
        }

        public void WriteClosure(PackageClosure closure)
        {
            LockFile.WriteToFolder(closure, folder);
        }

        public void WriteManifest(PackageManifest manifest)
        {
            ManifestFile.WriteToFolder(manifest, folder, merge: true);
        }
    }

}
