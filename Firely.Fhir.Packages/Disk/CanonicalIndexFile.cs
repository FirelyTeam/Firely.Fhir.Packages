using System.IO;
using System.Collections.Generic;
using System.Linq;
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
            var entries = BuildIndexFromFolder(folder);
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
                return Parser.ReadReferences(content);

            }
            else return null;
        }

        private static void Write(CanonicalIndex references, string path)
        {
            var content = Parser.WriteCanonicalIndex(references);
            File.WriteAllText(path, content);
        }

        private static bool ExistsIn(string folder)
        {
            var path = Path.Combine(folder, PackageConsts.CanonicalIndexFile);
            return File.Exists(path);
        }

        private static void WriteToFolder(CanonicalIndex references, string folder)
        {
            var path = Path.Combine(folder, PackageConsts.CanonicalIndexFile);
            Write(references, path);
        }

        private static string GetCanonicalFromResourceFile(string filepath)
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

        private static Dictionary<string, string> GetCanonicalsFromResourceFiles(IEnumerable<string> filepaths)
        {
            var dictionary = new Dictionary<string, string>();
            foreach (var filepath in filepaths)
            {
                var canonical = GetCanonicalFromResourceFile(filepath);
                if (canonical != null)
                {
                    var filename = Path.GetFileName(filepath);
                    dictionary[canonical] = filename;
                }
            }
            return dictionary;
        }

        private static Dictionary<string, string> BuildIndexFromFolder(string folder)
        {
            var filenames = Directory.GetFiles(folder);
            return GetCanonicalsFromResourceFiles(filenames);
        }
    }
}


