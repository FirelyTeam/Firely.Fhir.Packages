using System.IO;
using Hl7.Fhir.Serialization;
using Hl7.Fhir.ElementModel;

namespace Hl7.Fhir.Packages
{
    public static class Nav
    {
        
        public static IElementNavigator GetNavigatorForFile(string filepath)
        {
            var text = File.ReadAllText(filepath);
            var extension = Path.GetExtension(filepath).ToLower();

            switch (extension)
            {
                case ".xml": return XmlDomFhirNavigator.Create(text);
                case ".json": return JsonDomFhirNavigator.Create(text);
                default: return null;
            }

        }
    }


}


