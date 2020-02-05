using System.Collections.Generic;
using System.Linq;

namespace Firely.Fhir.Packages
{

    public class FileIndex : List<PackageFileReference>
    {

        public FileIndex()
        {
        }

        public PackageFileReference ResolveCanonical(string canonical)
        {
            return this.FirstOrDefault(r => r.Canonical == canonical);
        }

        public void Add(PackageReference package, string canonical, string filename)
        {
            this.Add(new PackageFileReference { Canonical = canonical, Package = package, FileName = filename });
        }

    }

    public static class FileIndexExtensions
    {

        internal static void Index(this FileIndex index, IPackageCache cache, PackageClosure closure)
        {

            foreach (var reference in closure.References)
            {
                index.Index(cache, reference);
            }
        }

        internal static void Index(this FileIndex index, IPackageCache cache, PackageReference reference)
        {
            var idx = cache.GetCanonicalIndex(reference);
            index.Add(reference, idx);
        }

        internal static void Add(this FileIndex index, PackageReference reference, Dictionary<string, string> cindex)
        {
            foreach (var item in cindex)
            {
                index.Add(reference, item.Key, item.Value);
            }
        }

        internal static void Add(this FileIndex index, PackageReference reference, CanonicalIndex cindex)
        {
            if (cindex.Canonicals is object)
            {
                index.Add(reference, cindex.Canonicals);
            }
        }

        internal static void Index(this FileIndex index, IProject project)
        {
            var dict = project.GetCanonicalIndex();
            index.Add(PackageReference.None, dict);
        }
    }
}
