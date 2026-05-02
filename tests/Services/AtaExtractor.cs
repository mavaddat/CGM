using System.Linq;
using System.Text.RegularExpressions;
using codessentials.CGM.Elements;
using codessentials.CGM.Tests.Models;

namespace codessentials.CGM.Tests.Services;

public static partial class AtaExtractor
{

    /// <summary>
    /// Extract ATA from ApplicationStructure strings.
    /// Legacy behavior: last valid ATA wins.
    /// </summary>
    public static AtaCode? FromApplicationStructure(
        ApplicationStructureContext aps)
    {
        var match = aps.Strings
            .Select(s => AtaPattern().Match(s))
            .LastOrDefault(m => m.Success);

        return match is { Success: true }
            ? new AtaCode(match.Value)
            : null;
    }

    /// <summary>
    /// Extract ATA from linkuri CGM path (e.g. 27-04-01.AAD).
    /// </summary>
    public static AtaCode? FromLinkUri(LinkUriElement link)
    {
        var match = AtaPattern().Match(link.Destination);
        if (match.Success)
            return new AtaCode(match.Value);

        return null;
    }


    /// <summary>
    /// Extracts the ATA chapter (NN-NN) from a CGM path, if present.
    /// Example:
    ///   "../../../21-20-05.AAE/Sheet 1 of 1.cgm" → 21-20
    /// </summary>
    public static AtaCode? FromPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;

        var match = AtaPattern().Match(path);

        return match.Success
            ? new AtaCode(match.Groups["ata"].Value)
            : null;
    }

    [GeneratedRegex(@"\b(?<ata>\d{2}-\d{2}-\d{2})\b", RegexOptions.Compiled)]
    private static partial Regex AtaPattern();
}
