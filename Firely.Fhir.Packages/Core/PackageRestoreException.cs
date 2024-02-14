using System;
using System.Collections.Generic;

namespace Firely.Fhir.Packages
{
    /// <summary>
    /// The exception that is thrown when a package dependency could not be restored. 
    /// </summary>
    public class PackageRestoreException : Exception
    {
        /// <summary>
        /// Package dependency chain for this exception.
        /// The exception is linked to the last dependency in the list.
        /// The first dependency in the list represents the root of the dependency chain.
        /// </summary>
        public IReadOnlyCollection<PackageDependency> PackageDependencies { get; }

        internal PackageRestoreException(IEnumerable<PackageDependency> dependencies, Exception exception) : base(exception.Message)
        {
            PackageDependencies = new List<PackageDependency>(dependencies);
        }
    }
}
