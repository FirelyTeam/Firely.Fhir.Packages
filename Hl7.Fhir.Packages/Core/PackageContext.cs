using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Hl7.Fhir.Packages
{
    public class PackageContext
    {
        private readonly PackageCache cache;
        private readonly PackageAssets assets;
        List<Reference> references; // canonical->filename
        
        public PackageContext(PackageCache cache, PackageAssets assets)
        {
            this.cache = cache;
            this.assets = assets;
            references = new List<Reference>();
            
            IndexPackages(); // lazy?
        }

        public PackageContext(PackageCache cache, string folder)
        {
            this.cache = cache;
            Disk.ReadFolderAssets(folder);
        }

        public void IndexPackages()
        {
            foreach (var folder in cache.GetPackageContentFolders(assets.Refs))
            {
                IndexPackage(folder);
            }
        }

        public void IndexPackage(string contentFolder)
        {
            var manifest = Disk.ReadFolderManifest(contentFolder);
            if (manifest != null && manifest.Canonicals != null)
            {
                IndexManifest(contentFolder, manifest);
            }
            else
            {
                var references = Disk.ReadFolderReferences(contentFolder);
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
                    references.Add(new Reference { Package = reference, Canonical = item.Key, FileName = path });
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
                    references.Add(new Reference { Package = packref, Canonical = item.Key, FileName = path });
                }
            }
        }

        public bool TryFindReference(string canonical, out Reference reference)
        {
            reference = references.FirstOrDefault(r => r.Canonical == canonical);
            return reference != null;
        }

        public string GetFile(Reference reference)
        {
            return File.ReadAllText(reference.FileName);

        }
        //public bool TryGetFile(string canonical, out string file)
        //{
        //    if (TryFindReference(canonical, out Reference reference))
        //    {
        //        Console.WriteLine($"Resolved {canonical} in {reference.Package}");
        //        file = File.ReadAllText(reference.FileName);
        //        return true;
        //    }
        //    else
        //    {
        //        file = null;
        //        return false;
        //    }
        //}
    }
}
