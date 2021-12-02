using System.Collections.Generic;
using System.Linq;

namespace Firely.Fhir.Packages
{

    public class FileIndex : List<PackageFileReference>
    {
        public FileIndex()
        {
        }

        public PackageFileReference ResolveCanonical(string canonical)
        {
            return this.FirstOrDefault(r => r.Canonical == canonical);
        }


        public void Add(PackageReference package, ResourceMetadata metadata)
        {
            var reference = new PackageFileReference() { Package = package };
            metadata.CopyTo(reference);
            Add(reference);
        }
    }

}
