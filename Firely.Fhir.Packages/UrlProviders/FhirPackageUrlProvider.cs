﻿/* 
 * Copyright (c) 2022, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/Firely.Fhir.Packages/blob/master/LICENSE
 */


#nullable enable


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
        public string GetPublishUrl(PackageReference reference, PublishMode mode)
        {
            string url = $"{Root}/publish?publishMode={mode}";
            return url;
        }
    }
}

#nullable restore
