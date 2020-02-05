using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Firely.Fhir.Packages
{
    public static class CanonicalIndexer
    {
        public static Dictionary<string, string> IndexFolder(string folder)
        {
            var filenames = Directory.GetFiles(folder);
            return GetCanonicalsFromResourceFiles(filenames);
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

        public static string GetCanonicalFromResourceFile(string filepath)
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

    }
}


