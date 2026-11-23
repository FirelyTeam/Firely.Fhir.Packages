/* 
 * Copyright (c) 2022, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/Firely.Fhir.Packages/blob/master/LICENSE
 */

using System.Collections.Generic;

namespace Firely.Fhir.Packages;

internal interface ICanonicalVersionResolver
{
    VersionAlgorithm Algorithm { get; }

    bool MatchThisAlgorithm(IReadOnlyList<string> versions);

    int Compare(string a, string b);
}
