using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Hl7.Fhir.Packages
{
    public class PackageIndex
    {
        readonly PackageCache cache;
        readonly Dependencies dependencies;
        readonly List<CanonicalFileReference> references; // canonical->filename
        
        public PackageIndex(PackageCache cache, Dependencies dependencies)
        {
            this.cache = cache;
            this.dependencies = dependencies;
            references = new List<CanonicalFileReference>();
            
            IndexPackages(); // lazy?
        }

        public PackageIndex(PackageCache cache, string folder)
        {
            this.cache = cache;
            LockFile.ReadFromFolder(folder);
        }

        public void IndexPackages()
        {
            references.Clear();
            foreach (var folder in GetPackageContentFolders())
            {
                IndexPackage(folder);
            }
        }

        public IEnumerable<string> GetPackageContentFolders()
        {
            return cache.GetPackageContentFolders(dependencies.Refs);
        }

        public void IndexPackage(string contentFolder)
        {
            var manifest = ManifestFile.ReadFromFolder(contentFolder);
            if (manifest != null && manifest.Canonicals != null)
            {
                IndexManifest(contentFolder, manifest);
            }
            else
            {
                var references = CanonicalIndexFile.ReadFromFolder(contentFolder);
                var packref = manifest.GetPackageReference();
                IndexReferences(contentFolder, packref, references);
            }
            
        }

        public void IndexManifest(string folder, PackageManifest manifest)
        {
            if (manifest.Canonicals != null)
            {
                foreach (var item in manifest.Canonicals)
                {
                    var path = Path.Combine(folder, item.Value);
                    var reference = manifest.GetPackageReference();
                    references.Add(new CanonicalFileReference { Package = reference, Canonical = item.Key, FileName = path });
                }
            }
        }

        public void IndexReferences(string contentFolder, PackageReference packref, CanonicalReferences refs)
        {
            if (refs.Canonicals != null)
            {
                foreach (var item in refs.Canonicals)
                {
                    var path = Path.Combine(contentFolder, item.Value);
                    references.Add(new CanonicalFileReference { Package = packref, Canonical = item.Key, FileName = path });
                }
            }
        }

        public bool TryFindReference(string canonical, out CanonicalFileReference reference)
        {
            reference = references.FirstOrDefault(r => r.Canonical == canonical);
            return reference != null;
        }

        public string GetFile(CanonicalFileReference reference)
        {
            return File.ReadAllText(reference.FileName);

        }

        public IEnumerable<CanonicalFileReference> References => references.AsReadOnly();

    }
}
