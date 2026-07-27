/*
 * Copyright (c) 2022, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 *
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/Firely.Fhir.Packages/blob/master/LICENSE
 */


#nullable enable

using System;

namespace Firely.Fhir.Packages
{
    /// <summary>
    /// Determines how a <see cref="PackageClosure"/> handles adding a package reference
    /// whose package name is already present in the closure.
    /// </summary>
    public enum ConflictResolutionStrategy
    {
        /// <summary>
        /// Keep a single reference per package name, retaining the highest version, silently discarding
        /// every other version encountered.
        /// </summary>
        /// <remarks>
        /// A FHIR dependency graph can legitimately require several different versions of the same package
        /// at once - e.g. via canonical references pinned to a specific version, or reuse-wrapper packages.
        /// Discarding all but the highest version can therefore cause resolution to fail for anything that
        /// was pinned to a version other than the highest. This strategy is retained only so that consumers
        /// who are not yet ready to handle multiple package versions can opt back into the historic,
        /// single-version behaviour; prefer <see cref="AcceptMultiple"/>, which is now the default.
        /// </remarks>
        [Obsolete("HighestWins silently discards package versions that a FHIR dependency graph may legitimately " +
            "need (e.g. canonical references pinned to a specific version). It is kept only for backward " +
            "compatibility with consumers not yet updated to handle multiple package versions. Prefer " +
            "AcceptMultiple, which is now the default.")]
        HighestWins,

        /// <summary>
        /// Allow multiple versions of the same package to coexist in the closure.
        /// Only exact duplicates (same name and version) are rejected.
        /// </summary>
        /// <remarks>This is the default.</remarks>
        AcceptMultiple
    }
}

#nullable restore
