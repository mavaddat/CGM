using codessentials.CGM.Elements;
using codessentials.CGM.Tests.Services;

namespace codessentials.CGM.Tests.Models;


public static class LinkDecisionEngine
{
    public static LinkDecision Decide(
        AtaCode authoritativeAta,
        LinkUriElement link)
    {
        var linkAta = AtaExtractor.FromLinkUri(link);

        // If link explicitly references a different ATA → remove
        if (linkAta != null && linkAta.Value != authoritativeAta.Value)
            return LinkDecision.Remove;

        // Otherwise keep
        return LinkDecision.Keep;
    }
}

public enum LinkDecision
{
    Keep,
    Remove
}
