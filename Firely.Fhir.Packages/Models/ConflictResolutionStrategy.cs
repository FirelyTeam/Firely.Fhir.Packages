/*
 * Copyright (c) 2022, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 *
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/Firely.Fhir.Packages/blob/master/LICENSE
 */


#nullable enable

namespace Firely.Fhir.Packages
{
    /// <summary>
    /// Determines how a <see cref="PackageClosure"/> handles adding a package reference
    /// whose package name is already present in the closure.
    /// </summary>
    public enum ConflictResolutionStrategy
    {
        /// <summary>
        /// Keep a single reference per package name, retaining the highest version.
        /// This is the default and preserves the historic behaviour.
        /// </summary>
        HighestWins,

        /// <summary>
        /// Allow multiple versions of the same package to coexist in the closure.
        /// Only exact duplicates (same name and version) are rejected.
        /// </summary>
        AcceptMultiple
    }
}

#nullable restore
