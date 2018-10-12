using System.Collections.Generic;

namespace Hl7.Fhir.Packages
{
    public static class PackageReferenceExtensions
    {
        
        public static Dictionary<string, string> ToDictionary(this IEnumerable<PackageReference> references)
        {
            var dict = new Dictionary<string, string>();
            foreach(var reference in references)
            {
                dict.Add(reference.Name, reference.Version);
            }
            return dict;
        }

        public static List<PackageReference> ToPackageReferences(this Dictionary<string, string> dict)
        {
            var list = new List<PackageReference>();
            foreach(var item in dict)
            {
                list.Add(item); // implicit converion
            }
            return list;
        }


        public static IEnumerable<PackageReference> GetDependencies(this PackageManifest manifest)
        {
            if (manifest.Dependencies is null) yield break;

            foreach (PackageReference dep in manifest.Dependencies)
            {
                yield return dep;
            }
        }


    }
}
