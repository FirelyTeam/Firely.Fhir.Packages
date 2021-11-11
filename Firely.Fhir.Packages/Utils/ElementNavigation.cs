using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Serialization;
using System;
using System.IO;

namespace Firely.Fhir.Packages
{
    public static class ElementNavigation
    {
        static readonly FhirJsonParsingSettings _jsonParsingSettings = new FhirJsonParsingSettings()
        {
            PermissiveParsing = true,
            ValidateFhirXhtml = false,
            AllowJsonComments = true
        };

        static readonly FhirXmlParsingSettings _xmlParsingSettings = new FhirXmlParsingSettings()
        {
            PermissiveParsing = true,
            ValidateFhirXhtml = false
        };

        public static ISourceNode ParseToSourceNode(string filepath)
        {
            if (FhirFileFormats.HasXmlExtension(filepath))
            {
                return FhirXmlNode.Parse(File.ReadAllText(filepath), _xmlParsingSettings);
            }

            if (FhirFileFormats.HasJsonExtension(filepath))
            {
                return FhirJsonNode.Parse(File.ReadAllText(filepath), null, _jsonParsingSettings);
            }

            return null;
        }

        public static ISourceNode ParseToSourceNode(Stream stream)
        {
            StreamReader reader = new(stream);
            string text = reader.ReadToEnd();

            if (text.TrimStart().StartsWith("{"))
            {
                return FhirJsonNode.Parse(text, null, _jsonParsingSettings);
            }

            if (text.TrimStart().StartsWith("<"))
            {
                return FhirXmlNode.Parse(text, _xmlParsingSettings);
            }

            return null;
        }

        internal static bool TryParseToSourceNode(string filepath, out ISourceNode node)
        {
            try
            {
                node = ParseToSourceNode(filepath);
                if (node is null)
                {
                    return false;
                }
            }
            catch (Exception)
            {
                node = null;
                return false;
            }
            return true;
        }

        internal static bool TryParseToSourceNode(Stream stream, out ISourceNode node)
        {
            try
            {
                node = ParseToSourceNode(stream);
                if (node is null)
                {
                    return false;
                }
            }
            catch (Exception)
            {
                node = null;
                return false;
            }
            return true;
        }

        //public static IElementNavigator GetNavigatorForFile(string filepath)
        //{
        //    var text = File.ReadAllText(filepath);
        //    var extension = Path.GetExtension(filepath).ToLower();

        //    switch (extension)
        //    {
        //        case ".xml": return XmlDomFhirNavigator.Create(text);
        //        case ".json": return JsonDomFhirNavigator.Create(text);
        //        default: return null;
        //    }
        //}

    }


}


