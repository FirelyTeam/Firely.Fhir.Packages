using System.Collections.Generic;
using System.Threading.Tasks;

namespace Firely.Fhir.Packages
{
    public static class FileIndexExtensions
    {
        public static async Task Index(this FileIndex index, IPackageCache cache, PackageClosure closure)
        {
            foreach (var reference in closure.References)
            {
                await index.Index(cache, reference);
            }
        }

        public static async Task Index(this FileIndex index, IPackageCache cache, PackageReference reference)
        {
            var idx = await cache.GetCanonicalIndex(reference);

            index.Add(reference, idx);
        }

        public static void Add(this FileIndex index, PackageReference reference, IEnumerable<ResourceMetadata> cindex)
        {
            foreach (var item in cindex)
            {
                index.Add(reference, item);
            }
        }

        public static void Add(this FileIndex index, PackageReference reference, CanonicalIndex cindex)
        {
            if (cindex.Files is object)
            {
                index.Add(reference, cindex.Files);
            }
        }

        public static void Add(this FileIndex index, PackageReference package, ResourceMetadata metadata)
        {
            var reference = new PackageFileReference() { Package = package };
            metadata.CopyTo(reference);
            index.Add(reference);
        }

        public static async Task Index(this FileIndex index, IProject project)
        {
            var entries = await project.GetIndex();

            index.Add(PackageReference.None, entries);
        }

        internal static async Task AddToIndexIndex(this IPackageCache cache, FileIndex index, PackageReference reference)
        {
            var canonicals = await cache.GetCanonicalIndex(reference);
            index.Add(reference, canonicals);
        }
    }
}
