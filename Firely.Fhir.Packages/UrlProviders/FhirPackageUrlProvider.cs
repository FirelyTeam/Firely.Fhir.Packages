#nullable enable

using Hl7.Fhir.Utility;
using System;

namespace Firely.Fhir.Packages
{
    public class FhirPackageUrlProvider : IPackageUrlProvider
    {
        /// <summary>
        /// Package root path
        /// </summary>
        public string Root { get; private set; }

        /// <summary>
        /// FHIR Package URL provider, to determine and create correct package URLs
        /// </summary>
        /// <param name="root">Paclage root path</param>
        public FhirPackageUrlProvider(string root)
        {
            this.Root = root.TrimEnd('/');
        }

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
        public string GetPackageUrl(PackageReference reference) => $"{Root}/{reference.Name}/{reference.Version}";

        public override string ToString() => $"(FHIR) {Root}";

        /// <summary>
        /// Get the publish URL
        /// </summary>
        /// <param name="release">FHIR release version</param>
        /// <param name="mode">Publish mode</param>
        /// <returns>A URL used for publishing packages</returns>
        public string GetPublishUrl(string fhirVersion, PackageReference reference, PublishMode mode)
        {
            if (FhirReleaseParser.TryParse(fhirVersion, out var release))
            {
                string url = $"{Root}/{release}?publishMode={mode}";
                return url;
            }
            else
            {
                throw new ArgumentException($"Unknown FHIR version {fhirVersion}");
            }
        }
    }
}

#nullable restore
