using System;
using System.Text.RegularExpressions;

namespace codessentials.CGM.Tests.Models;

public sealed partial record AtaCode
{

    public string Value { get; }

    public AtaCode(string value)
    {
        if (!AtaPattern().IsMatch(value))
            throw new ArgumentException(
                $"Invalid ATA format: {value}", nameof(value));

        Value = value;
    }

    public override string ToString() => Value;
    [GeneratedRegex(@"\b\d{2}-\d{2}-\d{2}\b", RegexOptions.Compiled)]
    private static partial Regex AtaPattern();
}
public sealed record CgmAtaContext(
    string CgmFileName,
    AtaCode Ata
);
