using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Firely.Fhir.Packages
{
    public class PackageScope
    {
        readonly IPackageCache cache;
        readonly PackageClosure closure;
        readonly List<CanonicalFileReference> references = new List<CanonicalFileReference>(); // canonical->filename
        
        public PackageScope(IPackageCache cache, PackageClosure closure)
        {
            this.cache = cache;
            this.closure = closure;
            IndexPackages(); 
        }

        public PackageScope(IPackageCache cache, string folder)
        {
            this.cache = cache;
            this.closure = LockFile.ReadFromFolder(folder);
            if (closure is null) throw new ArgumentException("The folder does not contain a package lock file.");
            IndexPackages();
        }

        private void IndexPackages()
        {
            references.Clear();
            foreach (var reference in closure.References)
            {
                AddPackageToIndex(reference);
            }
        }

        private void AddPackageToIndex(PackageReference reference)
        {
            var index = cache.GetCanonicalIndex(reference);
            AppendCanonicalIndex(reference, index);
        }

        private void AppendCanonicalIndex(PackageReference reference, CanonicalIndex index)
        {
            //var folder = cache.PackageContentFolder(reference);
            if (index.Canonicals is object)
            {
                foreach (var item in index.Canonicals)
                {
                    references.Add(new CanonicalFileReference { Package = reference, Canonical = item.Key, FileName = item.Value });
                }
            }
        }

        public bool TryFindReference(string canonical, out CanonicalFileReference reference)
        {
            if (references is null)
            {
                // the index was not initialized!
                reference = null;
                return false;
            }

            reference = references.FirstOrDefault(r => r.Canonical == canonical);
            return reference is object;
        }

        public string GetFile(CanonicalFileReference reference)
        {
            return cache.GetFileContent(reference.Package, reference.FileName);

        }

    }
}
