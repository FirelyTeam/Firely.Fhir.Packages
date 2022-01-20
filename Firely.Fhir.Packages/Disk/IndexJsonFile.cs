#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Firely.Fhir.Packages
{
    public static class IndexJsonFile
    {
        public const int VERSION = 1;

        internal static FileEntry GenerateIndexFile(IEnumerable<FileEntry> entries, string folder)
        {
            var files = CanonicalIndexer.GenerateIndexFile(entries);
            var index = new IndexJson { Files = files.ToList(), Version = VERSION, date = DateTimeOffset.Now };
            return convertToFileEntry(index, folder);

        }

        private static FileEntry convertToFileEntry(IndexJson index, string folder)
        {
            var content = PackageParser.WriteIndexJson(index);
            return new FileEntry(Path.Combine(folder, PackageFileNames.INDEXJSONFILE), Encoding.ASCII.GetBytes(content));
        }
    }
}


#nullable restore