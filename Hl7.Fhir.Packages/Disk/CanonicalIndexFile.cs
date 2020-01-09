using System.IO;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Firely.Fhir.Packages
{
    public static class CanonicalIndexFile
    {

        public static CanonicalIndex GetIndexFromFolder(string folder)
        {
            if (!IndexExistsIn(folder)) return CreateIndexFile(folder);
            return ReadFromFolder(folder);
        }

        public static CanonicalIndex CreateIndexFile(string folder)
        {
            var entries = GetIndexFromFolderContents(folder);
            var index = new CanonicalIndex { Canonicals = entries, date = DateTimeOffset.Now };
            WriteToFolder(index, folder);
            return index;
        }

        public static CanonicalIndex ReadFromFolder(string folder)
        {
            var path = Path.Combine(folder, DiskNames.CanonicalIndexFile);
            return Read(path);
        }

        public static CanonicalIndex Read(string path)
        {
            if (File.Exists(path))
            {
                var content = File.ReadAllText(path);
                return Parser.ReadReferences(content);

            }
            else return null;
        }

        private static void Write(CanonicalIndex references, string path)
        {
            var content = Parser.WriteCanonicalIndex(references);
            File.WriteAllText(path, content);
        }

        private static bool IndexExistsIn(string folder)
        {
            var path = Path.Combine(folder, DiskNames.CanonicalIndexFile);
            return File.Exists(path);
        }

        private static void WriteToFolder(CanonicalIndex references, string folder)
        {
            var path = Path.Combine(folder, DiskNames.CanonicalIndexFile);
            Write(references, path);
        }

        public static string GetCanonicalFromFile(string filepath)
        {
            try
            {
                var node = ElementNavigation.ParseToSourceNode(filepath);
                var canonical = (string)node.Children("url").FirstOrDefault().Text;
                return canonical;
            }
            catch
            {
                return null;
            }
        }

        private static Dictionary<string, string> GetCanonicalsFromFiles(IEnumerable<string> filepaths)
        {
            var dictionary = new Dictionary<string, string>();
            foreach (var filepath in filepaths)
            {
                var canonical = GetCanonicalFromFile(filepath);
                if (canonical != null)
                {
                    var filename = Path.GetFileName(filepath);
                    dictionary[canonical] = filename;
                }
            }
            return dictionary;
        }

        private static Dictionary<string, string> GetIndexFromFolderContents(string folder)
        {
            var filenames = Directory.GetFiles(folder);
            return GetCanonicalsFromFiles(filenames);
        }
    }
}


