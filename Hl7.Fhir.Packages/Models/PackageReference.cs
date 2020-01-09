using System.Collections.Generic;

namespace Firely.Fhir.Packages
{


    public struct PackageReference
    {
        public string Scope;
        public string Name;
        public string Version;

        public PackageReference(string name, string version = null)
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

        public override string ToString()
        {
            return $"{Name} {Version}";
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
