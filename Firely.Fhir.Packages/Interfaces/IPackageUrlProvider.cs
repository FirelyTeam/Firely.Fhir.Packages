/* 
 * Copyright (c) 2022, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/Firely.Fhir.Packages/blob/master/LICENSE
 */


namespace Firely.Fhir.Packages
{

    public interface IPackageUrlProvider
    {
        /// <summary>
        /// Package root path
        /// </summary>
        string Root { get; }


        /// <summary>
        /// Get the Package listings url
        /// </summary>
        /// <param name="name">Package name</param>
        /// <returns>the package listings url</returns>
        string GetPackageListingUrl(string name);

        /// <summary>
        /// Get the version specific package url
        /// </summary>
        /// <param name="reference">Package reference</param>
        /// <returns>Package url</returns>
        string GetPackageUrl(PackageReference reference);

        /// <summary>
        /// Get the publish URL
        /// </summary>
        /// <param name="fhirVersion">FHIR release version</param>
        /// <param name="reference">Package url</param>
        /// <param name="mode">Publish mode</param>
        /// <returns>A URL used for publishing packages</returns>
        string GetPublishUrl(PackageReference reference, PublishMode mode);
    }

}
