/* 
 * Copyright (c) 2022, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/Firely.Fhir.Packages/blob/master/LICENSE
 */


#nullable enable

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Firely.Fhir.Packages
{
    public static class FileIndexExtensions
    {

        private static void add(this FileIndex index, PackageReference reference, CanonicalIndex cindex)
        {
            if (cindex.Files is not null)
            {
                index.add(reference, cindex.Files);
            }
        }

        internal static async Task Index(this FileIndex index, IPackageCache cache, PackageClosure closure)
        {
            foreach (var reference in closure.References)
            {
                await index.index(cache, reference);
            }
        }

        private static async Task index(this FileIndex index, IPackageCache cache, PackageReference reference)
        {
            var idx = await cache.GetCanonicalIndex(reference);

            index.add(reference, idx);
        }

        private static void add(this FileIndex index, PackageReference reference, IEnumerable<ResourceMetadata> cindex)
        {
            foreach (var item in cindex)
            {
                index.Add(reference, item);
            }
        }

        internal static async Task Index(this FileIndex index, IProject project)
        {
            var entries = await project.GetIndex();
            index.add(PackageReference.None, entries);
        }

        /// <summary>
        /// Add a file to the index
        /// </summary>
        /// <param name="index"></param>
        /// <param name="package">Package that the file belongs to</param>
        /// <param name="metadata">Metadata of the file to be added</param>
        public static void Add(this FileIndex index, PackageReference package, ResourceMetadata metadata)
        {
            var reference = new PackageFileReference(metadata.FileName, metadata.FilePath) { Package = package };
            metadata.CopyTo(reference);
            index.Add(reference);
        }

        /// <summary>
        /// Adds all items of package of a package reference to a file index
        /// </summary>
        /// <param name="index">Index the files are added to</param>
        /// <param name="metadata">Metadata of the file to be added</param>
        public static async Task AddToFileIndex(this IPackageCache cache, FileIndex index, PackageReference reference)
        {
            var canonicals = await cache.GetCanonicalIndex(reference);
            index.add(reference, canonicals);
        }
    }
}

#nullable restore