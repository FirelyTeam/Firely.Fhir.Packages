/* 
 * Copyright (c) 2022, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/Firely.Fhir.Packages/blob/master/LICENSE
 */

namespace Firely.Fhir.Packages;

internal enum VersionAlgorithm
{
    SemVer,
    Integer,
    Alpha,
    Date,
    Natural
}
