using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using codessentials.CGM.Tests.Models;

namespace codessentials.CGM.Tests.Services;

public static partial class DmrlProcessor
{
    public static IReadOnlyList<DataModuleRequirementsList>
        SortAndDeduplicate(IEnumerable<DataModuleRequirementsList> records)
    {
        var ordered = records
            .OrderBy(r => r.DMC)
            .ThenBy(r => r.SHEET_ID)
            .ThenBy(r => r.CONTROL_REF);

        return [.. ordered
            .GroupBy(r => GetIcnBase(r.CONTROL_REF))
            .Select(g => g.First())];
    }
    public static IReadOnlyList<DataModuleRequirementsList>
    FilterFinal(IEnumerable<DataModuleRequirementsList> records)
    {
        return [.. records.Where(r => r.DMC != null && r.DMC.EndsWith("-051A-A"))];
    }
    public static IReadOnlyDictionary<string, string>
    BuildCgmToIcnMap(IEnumerable<DataModuleRequirementsList> finalDmrl)
    {
        return finalDmrl
            .Where(r => !string.IsNullOrWhiteSpace(r.CGM_NEW_NAME))
            .ToDictionary(
                r => r.CGM_NEW_NAME,
                r => r.CONTROL_REF
            );
    }

    public static string GetIcnBase(string controlRef)
    {
        var match = IcnBasePattern().Match(controlRef);
        if (!match.Success)
            throw new InvalidOperationException(
                $"CONTROL_REF does not match expected ICN pattern: {controlRef}");

        return match.Groups["base"].Value;
    }

    [GeneratedRegex(@"^(?<base>ICN-[A-Z0-9]+-[A-Z]-[A-Z0-9]+-[A-Z]-[A-Z0-9]+-\d+-[A-Z])-\d+-\d+$", RegexOptions.Compiled)]
    private static partial Regex IcnBasePattern();
}
