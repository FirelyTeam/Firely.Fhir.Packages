using SemVer;
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
            if (Find(reference.Name, out var existing))
            {
                if (existing == reference) return false;
                
                var highest = Highest(reference, existing);
                if (highest != existing)
                {
                    Refs.Remove(existing);
                    Refs.Add(highest);
                    return true;
                }
                else
                {
                    return false;
                }
                
            }
            else 
            {
                Refs.Add(reference);
                return true;
            }
        }

        public PackageReference Highest(PackageReference A, PackageReference B)
        {
            var versionA = new Version(A.Version);
            var versionB = new Version(B.Version);
            var highest = (versionA > versionB) ? A : B;

            return highest;
        }

        public bool Find(string pkgname, out PackageReference reference)
        {
            foreach(var refx in Refs)
            {
                if (string.Compare(refx.Name, pkgname, ignoreCase: true) == 0)
                {
                    reference = refx;
                    return true;
                }
            }
            reference = default;
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
