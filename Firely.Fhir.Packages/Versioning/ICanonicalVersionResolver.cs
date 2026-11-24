/* 
 * Copyright (c) 2022, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/Firely.Fhir.Packages/blob/master/LICENSE
 */

using System.Collections.Generic;

namespace Firely.Fhir.Packages;

/// <summary>
/// Orders the versions of a canonical resource according to one FHIR version algorithm.
/// </summary>
public interface ICanonicalVersionResolver
{
    /// <summary>
    /// The version algorithm this resolver implements.
    /// </summary>
    VersionAlgorithm Algorithm { get; }

    /// <summary>
    /// Returns whether every version in <paramref name="versions"/> can be ordered by this algorithm; true for an empty list.
    /// </summary>
    /// <param name="versions">The versions to check; every element must be non-null. Behaviour for a null list or a null element is not defined by this interface.</param>
    bool Fits(IReadOnlyList<string> versions);

    /// <summary>
    /// Compares two versions: negative when <paramref name="a"/> ranks lower than <paramref name="b"/>, zero when equal, positive when higher.
    /// Both arguments must be non-null and accepted by <see cref="Fits"/>. Behaviour for a null argument is not defined by this interface;
    /// for a non-null argument that <see cref="Fits"/> rejects, the result is undefined and may be an exception.
    /// </summary>
    int Compare(string a, string b);
}
