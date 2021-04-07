using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Serialization;
using System.IO;

namespace Firely.Fhir.Packages
{
    internal static class ElementNavigation
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

        internal static ISourceNode ParseToSourceNode(string filepath)
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

    }


}


