#nullable enable    

using System.Security.Cryptography;
using System.Text;

namespace Firely.Fhir.Packages
{
    /// <summary>
    /// A package server that serves packages, needs to provide a checksum in the package listing.
    /// </summary>
    public static class CheckSum
    {
        /// <summary>
        /// A package server that serves packages, needs to provide a checksum in the package listing.
        /// This method can be used for that purpose.
        /// </summary>
        public static byte[] ShaSum(byte[] buffer)
        {
            using var sha = SHA1.Create();
            var hash = sha.ComputeHash(buffer);
            return hash;
        }

        /// <summary>
        /// Create a Hexadecimal string of a binary hash.
        /// </summary>
        public static string HashToHexString(byte[] hash)
        {
            var builder = new StringBuilder();
            foreach (var @byte in hash)
            {
                builder.Append(@byte.ToString("x2"));
            }
            return builder.ToString();
        }
    }
}


#nullable restore