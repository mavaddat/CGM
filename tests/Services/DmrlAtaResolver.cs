using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using codessentials.CGM.Tests.Models;

namespace codessentials.CGM.Tests.Services;

public static partial class DmrlAtaResolver
{
    public static AtaCode ResolveAtaForCgm(
        string cgmFileName,
        IReadOnlyDictionary<string, DataModuleRequirementsList> dmrlByCgmName)
    {
        if (!dmrlByCgmName.TryGetValue(cgmFileName, out var record))
            throw new InvalidOperationException(
                $"No DMRL entry found for CGM {cgmFileName}");

        // You already implemented this logic
        var cgmName = DmrlCgmNameGenerator.Generate(record);

        // Extract ATA from DMC
        var match = AtaPattern().Match(record.DMC);

        if (!match.Success)
            throw new InvalidOperationException(
                $"Cannot extract ATA from DMC: {record.DMC}");

        return new AtaCode(match.Groups["ata"].Value);
    }

    [GeneratedRegex(@"(?<ata>\d{2}-\d{2}-\d{2})")]
    private static partial Regex AtaPattern();
}
