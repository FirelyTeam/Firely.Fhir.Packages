using System.Collections.Generic;

namespace Hl7.Fhir.Packages
{
    
    public struct PackageDependency
    {
        public string Name;
        public string Range;  // 3.x, 3.1 - 3.3, 1.1 | 1.2

        public PackageDependency(string name, string range)
        {
            this.Name = name;
            this.Range = range;
        }

        public static implicit operator PackageDependency(KeyValuePair<string, string> pair)
        {
            return new PackageDependency(pair.Key, pair.Value);
        }
    }


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

        public string NpmName
        {
            get
            {
                return (Scope == null) ? Name : $"@{Scope}%2F{Name}";
            }
        }

        public override string ToString()
        {
            return $"{Name} {Version}";
        }

        public static PackageReference NotFound => new PackageReference { Name = null };

        public bool IsEmpty => (Name is null);

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
