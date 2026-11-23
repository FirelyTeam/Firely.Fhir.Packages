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

internal class NumericCanonicalVersionResolver : ICanonicalVersionResolver
{
    public VersionAlgorithm Algorithm => VersionAlgorithm.Numeric;

    public bool MatchThisAlgorithm(IReadOnlyList<string> versions) => versions.All(v => Version.TryParse(v, out _));

    public int Compare(string a, string b) => Version.Parse(a).CompareTo(Version.Parse(b));
}
