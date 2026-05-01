namespace codessentials.CGM.Tests.Models;

using System.Collections.Generic;
using System.Linq;

public sealed class ApplicationStructureContext
{
    public string ApsId { get; }
    public List<string> Strings { get; }
    public List<LinkUriContext> LinkUris { get; }

    public ApplicationStructureContext(
        string apsId,
        IEnumerable<string>? strings = null,
        IEnumerable<LinkUriContext>? linkUris = null)
    {
        ApsId = apsId;
        Strings = strings?.ToList() ?? [];
        LinkUris = linkUris?.ToList() ?? [];
    }

    public bool IsEmptyAfterFiltering()
        => LinkUris.Count == 0;
}
