using System.Collections.Generic;

namespace codessentials.CGM.Tests.Models;

public sealed class LinkUriContext(IEnumerable<string> sdrStrings, string? hotspotId = null)
{
    public IReadOnlyList<string> SdrStrings { get; } = [.. sdrStrings];
    public string? HotspotId { get; } = hotspotId;
}
