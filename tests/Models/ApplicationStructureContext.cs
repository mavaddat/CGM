namespace codessentials.CGM.Tests.Models;

using System.Collections.Generic;
using System.Linq;
using codessentials.CGM.Elements;

public sealed class ApplicationStructureContext
{
    public string ApsId { get; }
    public List<string> Strings { get; }
    public List<LinkUriElement> LinkUris { get; }

    public ApplicationStructureContext(
        string apsId,
        IEnumerable<string>? strings = null,
        IEnumerable<LinkUriElement>? linkUris = null)
    {
        ApsId = apsId;
        Strings = strings?.ToList() ?? [];
        LinkUris = linkUris?.ToList() ?? [];
    }

    public bool IsEmptyAfterFiltering()
        => LinkUris.Count == 0;
}
