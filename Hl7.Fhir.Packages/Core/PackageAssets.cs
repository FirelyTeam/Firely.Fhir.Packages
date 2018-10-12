using System.Collections.Generic;

namespace Hl7.Fhir.Packages
{
    public class PackageAssets
    {
        public bool Complete => Missing.Count == 0;

        public List<PackageReference> Refs = new List<PackageReference>();
        public List<PackageReference> Missing = new List<PackageReference>();

        public bool AddRef(PackageReference reference)
        {
            if (!Refs.Contains(reference))
            {
                Refs.Add(reference);
                return true;
            }
            return false;
        }

        public void AddMissing(PackageReference reference)
        {
            if (!Missing.Contains(reference)) Missing.Add(reference);
        }
        
        public AssetsFile CreateAssetsFile()
        {
            return new AssetsFile
            {
                PackageReferences = Refs.ToDictionary(),
                MissingReferences = Missing.ToDictionary()
            };
        }

    }

}
