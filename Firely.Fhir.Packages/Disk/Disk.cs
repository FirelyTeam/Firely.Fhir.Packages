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
    public static class Disk
    {
        /// <summary>
        /// Get name of a folder
        /// </summary>
        /// <param name="path">the path of the folder</param>
        /// <returns>the name of the folder</returns>
        public static string GetFolderName(string path)
        {
            return new DirectoryInfo(path).Name;
        }

    }
}


#nullable restore