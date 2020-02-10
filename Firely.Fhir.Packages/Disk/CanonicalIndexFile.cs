using System.IO;
using System;

namespace Firely.Fhir.Packages
{
    public static class CanonicalIndexFile
    {

        public static CanonicalIndex GetFromFolder(string folder)
        {
            if (!ExistsIn(folder)) return Create(folder);
            return ReadFromFolder(folder);
        }

        public static CanonicalIndex Create(string folder)
        {
            var entries = CanonicalIndexer.IndexFolder(folder);
            var index = new CanonicalIndex { Canonicals = entries, date = DateTimeOffset.Now };
            WriteToFolder(index, folder);
            return index;
        }

        public static CanonicalIndex ReadFromFolder(string folder)
        {
            var path = Path.Combine(folder, PackageConsts.CanonicalIndexFile);
            return Read(path);
        }

        public static CanonicalIndex Read(string path)
        {
            if (File.Exists(path))
            {
                var content = File.ReadAllText(path);
                return Parser.ReadCanonicalIndex(content);

            }
            else return null;
        }

        private static void Write(CanonicalIndex index, string path)
        {
            var content = Parser.WriteCanonicalIndex(index);
            File.WriteAllText(path, content);
        }

        private static bool ExistsIn(string folder)
        {
            var path = Path.Combine(folder, PackageConsts.CanonicalIndexFile);
            return File.Exists(path);
        }

        private static void WriteToFolder(CanonicalIndex index, string folder)
        {
            var path = Path.Combine(folder, PackageConsts.CanonicalIndexFile);
            Write(index, path);
        }

     

      
       
    }
}


