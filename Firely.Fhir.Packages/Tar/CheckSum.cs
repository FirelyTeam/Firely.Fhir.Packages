/* 
 * Copyright (c) 2022, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/Firely.Fhir.Packages/blob/master/LICENSE
 */


#nullable enable    

using System;
using System.Security.Cryptography;
using System.Text;

namespace Firely.Fhir.Packages
{
    /// <summary>
    /// A package server that serves packages, needs to provide a checksum in the package listing.
    /// </summary>

    [Obsolete("Users should implement their own SHA checksum")]
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