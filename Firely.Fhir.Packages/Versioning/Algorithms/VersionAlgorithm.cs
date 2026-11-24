/* 
 * Copyright (c) 2022, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/Firely.Fhir.Packages/blob/master/LICENSE
 */

namespace Firely.Fhir.Packages;

/// <summary>
/// The FHIR version algorithms (http://hl7.org/fhir/version-algorithm) used to order versions of a canonical resource.
/// </summary>
public enum VersionAlgorithm
{
    /// <summary>Semantic versioning (FHIR code "semver").</summary>
    SemVer,

    /// <summary>Versions are whole numbers compared numerically (FHIR code "integer").</summary>
    Integer,

    /// <summary>Versions are compared alphabetically, ignoring case (FHIR code "alpha").</summary>
    Alpha,

    /// <summary>Versions are dates (YYYY, YYYY-MM, YYYY-MM-DD, YYYYMM or YYYYMMDD, ASCII digits only) compared chronologically (FHIR code "date").</summary>
    Date,

    /// <summary>Numeric parts compared as numbers, other parts as text (FHIR code "natural").</summary>
    Natural
}
