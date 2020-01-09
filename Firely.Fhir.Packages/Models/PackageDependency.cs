using System.Collections.Generic;

namespace Firely.Fhir.Packages
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
}
