/* 
 * Copyright (c) 2022, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/Firely.Fhir.Packages/blob/master/LICENSE
 */


#nullable enable

namespace Firely.Fhir.Packages
{
    /// <summary>
    /// PublishMode is used to prevent accidental publication
    /// of package under a new name. If with package a new name is published 
    /// with mode Existing, or an existing package is published with mode New, the
    /// package server will return an error.
    /// </summary>
    public enum PublishMode
    {
        New,
        Existing,
        Any
    }

    public static class PackageUrlProviders
    {
        public static IPackageUrlProvider Npm => new NodePackageUrlProvider("https://registry.npmjs.org");
        public static IPackageUrlProvider Simplifier => new FhirPackageUrlProvider("https://packages.simplifier.net");
        public static IPackageUrlProvider SimplifierNpm => new NodePackageUrlProvider("https://packages.simplifier.net");
        public static IPackageUrlProvider Staging => new FhirPackageUrlProvider("https://packages-staging.simplifier.net");
        public static IPackageUrlProvider StagingNpm => new NodePackageUrlProvider("https://packages-staging.simplifier.net");
        public static IPackageUrlProvider Localhost => new FhirPackageUrlProvider("http://packages.simplifier.ro/");

    }
}

#nullable restore