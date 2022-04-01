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
    /// <summary>
    /// Memory stream that handles custom disposing. We should be able to do without.
    /// </summary>
    internal class LateDisposalMemoryStream : MemoryStream
    {
        protected override void Dispose(bool disposing)
        {
            return;
        }

        internal void DisposeAfter()
        {
            base.Dispose(true);
        }
    }
}

#nullable restore

