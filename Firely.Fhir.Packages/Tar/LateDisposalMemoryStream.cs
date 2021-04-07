using System.IO;

namespace Firely.Fhir.Packages
{
    /// <summary>
    /// Memory stream that handles custom disposing. We should be able to do without.
    /// Hacked for you by WMR. (comment by MH)
    /// </summary>
    internal class LateDisposalMemoryStream : MemoryStream
    {
        protected override void Dispose(bool disposing)
        {
            return;
        }

        public void DisposeAfter()
        {
            base.Dispose(true);
        }
    }

}


