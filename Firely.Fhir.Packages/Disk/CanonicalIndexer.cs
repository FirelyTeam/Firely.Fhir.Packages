using System.IO;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Firely.Fhir.Packages
{
    public class ResourceMetadata
    {
        public string FileName;
        public string? Canonical;
        public string ResourceType;
    }

    public static class CanonicalIndexer
    {
        public static List<ResourceMetadata> IndexFolder(string folder)
        {
            var filenames = Directory.GetFiles(folder);
            return EnumerateMetadata(filenames).ToList();
        }

        private static IEnumerable<ResourceMetadata> EnumerateMetadata(IEnumerable<string> filepaths)
        {
            foreach (var filepath in filepaths)
            {
                var meta = GetFileMetadata(filepath);
                if (meta is object)
                    yield return meta;
            }
        }

        public static ResourceMetadata? GetFileMetadata(string filepath)
        {
            try
            {
                var node = ElementNavigation.ParseToSourceNode(filepath);
                string? canonical = node.Children("url").FirstOrDefault()?.Text;

                return new ResourceMetadata
                {
                    Canonical = canonical,
                    FileName = Path.GetFileName(filepath),
                    ResourceType = node.Name
                };
            }
            catch (Exception)
            {
                return null;
            }
        }

    }
}


