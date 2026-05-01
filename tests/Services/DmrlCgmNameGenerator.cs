using System;
using System.Text.RegularExpressions;
using codessentials.CGM.Tests.Models;

namespace codessentials.CGM.Tests.Services;

public static partial class DmrlCgmNameGenerator
{
    public static string Generate(DataModuleRequirementsList record)
    {
        if (string.IsNullOrWhiteSpace(record.DMC))
            throw new ArgumentException("DMC is required to generate CGM name.");


        var match = DmcPattern().Match(record.DMC);
        if (!match.Success)
            throw new InvalidOperationException(
                $"DMC does not match expected pattern: {record.DMC}");

        var ata = match.Groups["ata"].Value;   // 21-20-01
        var rev = match.Groups["rev"].Value;   // AAE

        return $"{ata}_{rev}_Sheet_{record.SHEET_ID}_of_{record.SHEET_TOT}.cgm";
    }
    [GeneratedRegex(@"-(?<ata>\d{2}-\d{2}-\d{2})(?<rev>[A-Z]{3})-", RegexOptions.Compiled)]
    private static partial Regex DmcPattern();
}
