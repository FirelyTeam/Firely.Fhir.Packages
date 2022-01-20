#nullable enable

using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Serialization;
using System;
using System.IO;

namespace Firely.Fhir.Packages
{
    internal static class ElementNavigation
    {
        private static readonly FhirJsonParsingSettings _jsonParsingSettings = new FhirJsonParsingSettings()
        {
            PermissiveParsing = true,
            ValidateFhirXhtml = false,
            AllowJsonComments = true
        };

        private static readonly FhirXmlParsingSettings _xmlParsingSettings = new FhirXmlParsingSettings()
        {
            PermissiveParsing = true,
            ValidateFhirXhtml = false
        };

        internal static ISourceNode? ParseToSourceNode(string filepath)
        {
            if (FhirFileFormats.HasXmlExtension(filepath))
            {
                return FhirXmlNode.Parse(File.ReadAllText(filepath), _xmlParsingSettings);
            }

            if (FhirFileFormats.HasJsonExtension(filepath))
            {
                var content = File.ReadAllText(filepath);
                return FhirJsonNode.Parse(content, null, _jsonParsingSettings);
            }

            return null;
        }

        private static ISourceNode? parseToSourceNode(Stream stream)
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

        internal static bool TryParseToSourceNode(string filepath, out ISourceNode? node)
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

        /// <summary>
        /// Try to parse a stream to a SourceNode
        /// </summary>
        /// <param name="stream">Stream to be parsed</param>
        /// <param name="node">Newly parsed SourceNode</param>
        /// <returns>Whether the stream has been successfully parsed to a SourceNode</returns>
        internal static bool TryParseToSourceNode(Stream stream, out ISourceNode? node)
        {
            try
            {
                node = parseToSourceNode(stream);
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
    }
}

#nullable restore
