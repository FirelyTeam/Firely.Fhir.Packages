/* 
 * Copyright (c) 2022, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/Firely.Fhir.Packages/blob/master/LICENSE
 */


#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Firely.Fhir.Packages
{
    public static class IndexJsonFile
    {
        internal static FileEntry GenerateIndexFile(IEnumerable<FileEntry> entries, string folder)
        {
            var files = CanonicalIndexer.GenerateIndexFile(entries);
            var index = new IndexJson { Files = files.ToList(), Version = CanonicalIndexer.INDEX_JSON_VERSION, date = DateTimeOffset.Now };
            return convertToFileEntry(index, folder);

        }

        private static FileEntry convertToFileEntry(IndexJson index, string folder)
        {
            var content = PackageParser.WriteIndexJson(index);
            return new FileEntry(Path.Combine(folder, PackageFileNames.INDEXJSONFILE), Encoding.ASCII.GetBytes(content));
        }
    }
}


#nullable restore