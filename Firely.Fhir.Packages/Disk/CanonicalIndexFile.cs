#nullable enable

using System;
using System.IO;

namespace Firely.Fhir.Packages
{
    internal static class CanonicalIndexFile
    {
        internal const int VERSION = 7;

        internal static CanonicalIndex GetFromFolder(string folder, bool recurse)
        {
            if (existsIn(folder))
            {
                var index = readFromFolder(folder);
                if (index?.Version == VERSION) return index;
            }
            // otherwise:
            return Create(folder, recurse);
        }

        internal static CanonicalIndex Create(string folder, bool recurse)
        {
            var entries = CanonicalIndexer.IndexFolder(folder, recurse);
            var index = new CanonicalIndex { Files = entries, Version = VERSION, date = DateTimeOffset.Now };
            writeToFolder(index, folder);
            return index;
        }

        private static CanonicalIndex? readFromFolder(string folder)
        {
            var path = Path.Combine(folder, PackageFileNames.CANONICALINDEXFILE);
            return read(path);
        }

        private static CanonicalIndex? read(string path)
        {
            if (File.Exists(path))
            {
                var content = File.ReadAllText(path);
                return PackageParser.ReadCanonicalIndex(content);

            }
            else return null;
        }

        private static void write(CanonicalIndex index, string path)
        {
            var content = PackageParser.WriteCanonicalIndex(index);
            File.WriteAllText(path, content);
        }

        private static bool existsIn(string folder)
        {
            var path = Path.Combine(folder, PackageFileNames.CANONICALINDEXFILE);
            return File.Exists(path);
        }

        private static void writeToFolder(CanonicalIndex index, string folder)
        {
            var path = Path.Combine(folder, PackageFileNames.CANONICALINDEXFILE);
            write(index, path);
        }
    }
}


#nullable restore

