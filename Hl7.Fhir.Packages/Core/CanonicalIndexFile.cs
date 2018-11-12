using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Hl7.Fhir.Packages
{
    public static class CanonicalIndexFile
    {
        public static void Write(CanonicalReferences references, string path)
        {
            var content = Parser.WriteReferences(references);
            File.WriteAllText(path, content);
        }

        public static CanonicalReferences Read(string path)
        {
            if (File.Exists(path))
            {
                var content = File.ReadAllText(path);
                return Parser.ReadReferences(content);

            }
            else return null;
        }

        public static CanonicalReferences ReadFromFolder(string folder)
        {
            var path = Path.Combine(folder, DiskNames.References);
            return Read(path);
        }

        public static void WriteToFolder(CanonicalReferences references, string folder)
        {
            var path = Path.Combine(folder, DiskNames.References);
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

        public static Dictionary<string, string> GetCanonicalsFromFiles(IEnumerable<string> filepaths)
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

        public static Dictionary<string, string> GetIndexFromFolderContents(string folder)
        {
            var filenames = Directory.GetFiles(folder);
            return GetCanonicalsFromFiles(filenames);
        }
    }
}


