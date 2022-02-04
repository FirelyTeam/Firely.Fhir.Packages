#nullable enable


using Hl7.Fhir.Utility;
using System;

namespace Firely.Fhir.Packages
{
    public class NodePackageUrlProvider : IPackageUrlProvider
    {
        /// <summary>
        /// Package root path
        /// </summary>
        public string Root { get; private set; }

        /// <summary>
        /// Initiates a new node package url provider
        /// </summary>
        /// <param name="root">Root path of the node package</param>
        public NodePackageUrlProvider(string root)
        {
            this.Root = root.TrimEnd('/');
        }


        /// <summary>
        /// Get the Package listings url
        /// </summary>
        /// <param name="scope">Scope</param>
        /// <param name="name">Package name</param>
        /// <returns>the package listings url</returns>
        public string GetPackageListingUrl(string scope, string name) => $"{Root}/@{scope}%2F{name}";

        /// <summary>
        /// Get the Package listings url
        /// </summary>
        /// <param name="name">Package name</param>
        /// <returns>the package listings url</returns>
        public string GetPackageListingUrl(string name) => $"{Root}/{name}";

        /// <summary>
        /// Get the version specific package url
        /// </summary>
        /// <param name="reference">Package reference</param>
        /// <returns>Package url</returns>
        public string GetPackageUrl(PackageReference reference)
        {
            return
                (reference.Scope == null)
                ? $"{Root}/{reference.Name}/-/{reference.Name}-{reference.Version}.tgz"
                : $"{Root}/@{reference.Scope}/{reference.Name}/-/{reference.Name}-{reference.Version}.tgz";
        }

        /// <summary>
        /// Get the publish URL
        /// </summary>
        /// <param name="reference">Package url</param>
        /// <param name="mode">Publish mode</param>
        /// <returns>A URL used for publishing packages</returns>
        public string GetPublishUrl(PackageReference reference, PublishMode mode)
        {
            // This is not NPM compliant, but Simplifier doesn't have an NPM compliant endpoint yet anyway.
            string url = $"{Root}/publish?publishMode={mode}";
            return url;
        }
        public override string ToString() => $"(NPM) {Root}";

    }
}

#nullable restore