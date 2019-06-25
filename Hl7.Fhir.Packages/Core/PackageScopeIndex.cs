using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Hl7.Fhir.Packages
{
    public class PackageScopeIndex
    {
        readonly PackageCache cache;
        readonly Dependencies dependencies;
        readonly List<CanonicalFileReference> references; // canonical->filename
        
        public PackageScopeIndex(PackageCache cache, Dependencies dependencies)
        {
            this.cache = cache;
            this.dependencies = dependencies;
            references = new List<CanonicalFileReference>();
            
            IndexPackages(); // lazy?
        }

        public PackageScopeIndex(PackageCache cache, string folder)
        {
            this.cache = cache;
            LockFile.ReadFromFolder(folder);
        }

        public void IndexPackages()
        {
            references.Clear();
            foreach (var reference in dependencies.References)
            {
                AddPackageToIndex(reference);
            }
        }

        public void AddPackageToIndex(PackageReference reference)
        {
            var manifest = cache.ReadManifest(reference);
            if (manifest != null && manifest.Canonicals != null)
            {
                IndexManifest(reference, manifest);
            }
            else
            {
                var index = cache.GetCanonicalIndex(reference);
                AppendCanonicalIndex(reference, index);
            }
            
        }

        public void IndexManifest(PackageReference reference, PackageManifest manifest)
        {
            string folder = cache.PackageContentFolder(reference);
            if (manifest.Canonicals != null)
            {
                foreach (var item in manifest.Canonicals)
                {
                    var path = Path.Combine(folder, item.Value);
                    references.Add(new CanonicalFileReference { Package = reference, Canonical = item.Key, FileName = path });
                }
            }
        }

        public void AppendCanonicalIndex(PackageReference reference, CanonicalIndex index)
        {
            var folder = cache.PackageContentFolder(reference);
            if (index.Canonicals != null)
            {
                foreach (var item in index.Canonicals)
                {
                    var path = Path.Combine(folder, item.Value);
                    references.Add(new CanonicalFileReference { Package = reference, Canonical = item.Key, FileName = path });
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

    }
}
