using System.Collections.Generic;

namespace Firely.Fhir.Packages
{
    /// <summary>
    /// A package reference is a reference to a very specific version of a package. When you want to define a a range of versions that may quality, like 3.x, use PackageDependency
    /// A PackageReference is used in a Scope or Closure while a PackageDependency is used in a Manifest.
    /// </summary>
    public struct PackageReference
    {
        public string? Scope;
        public string? Name; // null means empty reference
        public string? Version;

        public PackageReference(string name, string? version = null)
        {
            if (name.StartsWith("@"))
            {
                var segments = name.Split('/');
                Scope = segments[0].Substring(1);
                Name = segments[1];
            }
            else
            {
                Name = name;
                Scope = null;
            }

            this.Version = version;
        }

        public string Moniker => $"{Name}@{Version}";
        public override string ToString()
        {
            string s = $"{Name}@{Version}";
            if (!Found) s += " (NOT FOUND)";
            return s;
        }

        public static PackageReference None => new PackageReference { Name = null, Version = null };

        public bool NotFound => !Found;

        public bool Found => !(Name is null || Version is null);

        public static implicit operator PackageReference (KeyValuePair<string, string> kvp)
        {
            return new PackageReference(kvp.Key, kvp.Value);
        }

        public static bool operator == (PackageReference A, PackageReference B)
        {
            return (A.Name == B.Name && A.Version == B.Version);
        }

        public static bool operator != (PackageReference A, PackageReference B)
        {
            return !(A == B);
        }

        public void Deconstruct(out string name, out string version)
        {
            name = Name;
            version = Version;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is PackageReference))
            {
                return false;
            }

            var reference = (PackageReference)obj;
            return this.Name == reference.Name &&
                   this.Version == reference.Version;
        }

        public override int GetHashCode()
        {
            return (Name, Version).GetHashCode();
        }


    }
}
