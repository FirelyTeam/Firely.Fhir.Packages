using System;

namespace Hl7.Fhir.Packages
{
    public static class SpanExtensions
    {
        public static ReadOnlySpan<char> SliceTo(this ReadOnlySpan<char> span, char item)
        {
            int i = span.IndexOf(item);
            return span.Slice(0, i);
        }
    }
}
