using System.IO;
using System.Collections.Generic;
using System.Linq;
using System;
using Hl7.Fhir.ElementModel;

namespace Firely.Fhir.Packages
{

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
                string? canonical = node.GetString("url"); // node.Children("url").FirstOrDefault()?.Text;

                return new ResourceMetadata
                {
                    FileName = Path.GetFileName(filepath),
                    ResourceType = node.Name,
                    Id = node.GetString("id"),
                    Canonical = node.GetString("url"),
                    Version = node.GetString("version"),
                    Kind = node.GetString("kind"),
                    Type = node.GetString("type")
                };
            }
            catch (Exception)
            {
                return null;
            }
        }


        public static string GetString(this ISourceNode node, string expression)
        {
            var parts = expression.Split('.');
            
            foreach (var part in parts)
            {
                node = node.Children(part).FirstOrDefault();
                if (node is null) return null;
            }
            return node.Text;
        }
    }
}


