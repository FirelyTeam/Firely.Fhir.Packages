/*
 * Copyright (c) 2022, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 *
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/Firely.Fhir.Packages/blob/master/LICENSE
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Firely.Fhir.Packages;

/// <summary>
/// Selection helpers that order versions with a given <see cref="ICanonicalVersionResolver"/>.
/// </summary>
public static class CanonicalVersionResolverExtensions
{
    /// <summary>
    /// Returns the highest version according to <paramref name="resolver"/>; among versions that compare equal the first listed is returned.
    /// A null or empty version is missing: it ranks lowest and is not checked against the resolver.
    /// </summary>
    /// <param name="resolver">The resolver that orders the versions.</param>
    /// <param name="versions">The versions to choose from.</param>
    /// <returns>The highest version, or null when <paramref name="versions"/> is empty or every version is missing.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="resolver"/> or <paramref name="versions"/> is null.</exception>
    /// <exception cref="ArgumentException">The present versions do not fit <paramref name="resolver"/>.</exception>
    public static string? HighestVersion(this ICanonicalVersionResolver resolver, IEnumerable<string?> versions)
    {
        if (resolver is null) throw new ArgumentNullException(nameof(resolver));
        if (versions is null) throw new ArgumentNullException(nameof(versions));

        var items = versions.ToList();
        if (items.All(string.IsNullOrEmpty))
            return null;

        return FileIndex.SelectBestCandidateCore(items, v => v, present => fitting(resolver, present, nameof(versions)), _ => false);
    }

    /// <summary>
    /// Selects the candidate with the highest version according to <paramref name="resolver"/>.
    /// A null or empty version is missing: it ranks lowest and is not checked against the resolver.
    /// Among candidates whose versions compare equal, the first <paramref name="preferred"/> one wins, else the first in input order;
    /// when every version is missing all candidates tie.
    /// </summary>
    /// <param name="resolver">The resolver that orders the versions.</param>
    /// <param name="candidates">The items to choose from.</param>
    /// <param name="getVersion">Returns the item's version, or null when it has none.</param>
    /// <param name="preferred">Tie-breaker among items sharing the highest version; null means no preference.</param>
    /// <exception cref="ArgumentNullException"><paramref name="resolver"/>, <paramref name="candidates"/> or <paramref name="getVersion"/> is null.</exception>
    /// <exception cref="ArgumentException">The present versions do not fit <paramref name="resolver"/>.</exception>
    /// <exception cref="InvalidOperationException"><paramref name="candidates"/> is empty.</exception>
    public static T SelectBestCandidate<T>(this ICanonicalVersionResolver resolver, IEnumerable<T> candidates,
        Func<T, string?> getVersion, Func<T, bool>? preferred = null)
    {
        if (resolver is null) throw new ArgumentNullException(nameof(resolver));
        if (candidates is null) throw new ArgumentNullException(nameof(candidates));
        if (getVersion is null) throw new ArgumentNullException(nameof(getVersion));

        return FileIndex.SelectBestCandidateCore(candidates.ToList(), getVersion,
            present => fitting(resolver, present, nameof(candidates)), preferred ?? (_ => false));
    }

    // Returns the resolver when it fits the present versions; the core calls this before any comparison.
    private static ICanonicalVersionResolver fitting(ICanonicalVersionResolver resolver, IReadOnlyList<string> present, string paramName) =>
        resolver.Fits(present)
            ? resolver
            : throw new ArgumentException($"The versions do not fit the {resolver.Algorithm} version algorithm.", paramName);
}
