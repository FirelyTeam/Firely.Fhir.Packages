#nullable enable

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Firely.Fhir.Packages
{
    public static class FileIndexExtensions
    {
        /// <summary>
        /// Adds multiple files to the fileindex    
        /// </summary>
        /// <param name="index">The index the file entry is added ti</param>
        /// <param name="package">Package that is indexed</param>
        /// <param name="cindex">List of metadata of files to be added to the index</param>
        public static void Add(this FileIndex index, PackageReference package, CanonicalIndex cindex)
        {
            if (cindex.Files is not null)
            {
                index.Add(package, cindex.Files);
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

        /// <summary>
        /// Adds a file entry to the file index
        /// </summary>
        /// <param name="index">The index the file entry is added ti</param>
        /// <param name="package">Package that is indexed</param>
        /// <param name="metadata">Metadata of the file that's added to the index</param>
        public static void Add(this FileIndex index, PackageReference package, ResourceMetadata metadata)
        {
            var reference = new PackageFileReference(metadata.FileName, metadata.FilePath) { Package = package };
            metadata.CopyTo(reference);
            index.Add(reference);
        }

        /// <summary>
        /// This method is currently used by Firely Terminal, to create ad hoc scopes.
        /// Not that it is the same as the Index(index, cache, ref) above. 
        /// Let's re-evaluate what to make public.
        /// </summary>
        public static async Task AddToFileIndex(this IPackageCache cache, FileIndex index, PackageReference reference)
        {
            var canonicals = await cache.GetCanonicalIndex(reference);
            index.Add(reference, canonicals);
        }
    }
}

#nullable restore