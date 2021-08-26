using System.Collections.Generic;
using System.Threading.Tasks;

namespace Firely.Fhir.Packages
{
    public static class FileIndexExtensions
    {
        public static void Add(this FileIndex index, PackageReference reference, CanonicalIndex cindex)
        {
            if (cindex.Files is object)
            {
                index.Add(reference, cindex.Files);
            }
        }

        internal static async Task Index(this FileIndex index, IPackageCache cache, PackageClosure closure)
        {
            foreach (var reference in closure.References)
            {
                await index.Index(cache, reference);
            }
        }

        internal static async Task Index(this FileIndex index, IPackageCache cache, PackageReference reference)
        {
            var idx = await cache.GetCanonicalIndex(reference);

            index.Add(reference, idx);
        }

        internal static void Add(this FileIndex index, PackageReference reference, IEnumerable<ResourceMetadata> cindex)
        {
            foreach (var item in cindex)
            {
                index.Add(reference, item);
            }
        }

        internal static async Task Index(this FileIndex index, IProject project)
        {
            var entries = await project.GetIndex();

            index.Add(PackageReference.None, entries);
        }
         
        public static async Task AddToFileIndex(this IPackageCache cache, FileIndex index, PackageReference reference)
        {
            var canonicals = await cache.GetCanonicalIndex(reference);
            index.Add(reference, canonicals);
        }
    }
}
