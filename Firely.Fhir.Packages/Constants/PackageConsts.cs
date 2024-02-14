/* 
 * Copyright (c) 2022, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/Firely.Fhir.Packages/blob/master/LICENSE
 */


#nullable enable

using System.IO;

namespace Firely.Fhir.Packages
{
    public static class PackageFileNames
    {
        public const string MANIFEST = "package.json";
        public const string LOCKFILE = "fhirpkg.lock.json";
        public const string CANONICALINDEXFILE = ".firely.index.json";
        public const string INDEXJSONFILE = ".index.json";
        public const string PACKAGEFOLDER = "package";
        public const string EXAMPLEFOLDER = "examples";
        public static string EXAMPLEFOLDERPATH => PACKAGEFOLDER + Path.DirectorySeparatorChar + EXAMPLEFOLDER;

        public static readonly string[] ALL_PACKAGE_FILENAMES = { MANIFEST, LOCKFILE, CANONICALINDEXFILE, INDEXJSONFILE };

    }

}

#nullable restore
