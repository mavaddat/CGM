using System.Linq;
using codessentials.CGM.Tests.Models;
using NUnit.Framework;
using Shouldly;

namespace codessentials.CGM.Tests.IntelligentWiringTests;

[TestFixture]
public class LinkPruningByAtaTests
{
    [Test]
    public void Keeps_Only_Links_That_Match_Authoritative_Ata()
    {
        // Authoritative ATA from DMRL / DMC
        var authoritativeAta = new AtaCode("21-20-02");

        // Synthetic APS with many cross-ATA links
        var aps = new ApplicationStructureContext(
            apsId: "Hs217",
            linkUris:
            [
                // ✅ same ATA → keep
                new LinkUriContext(
                    ["../21-20-02.AAC/Sheet 1 of 1.cgm#id(Hs621,Highlight)"]),

                // ❌ different ATA → remove
                new LinkUriContext(
                    ["../../21-31-03.AAC/Sheet 2 of 2.cgm#id(Hs2745,Highlight)"]),
                new LinkUriContext(
                    ["../../../22-30-01.AAD/Sheet 2 of 2.cgm#id(Hs13100,Highlight)"]),
                new LinkUriContext(
                    ["../../../33-51-02.AAC/Sheet 1 of 3.cgm#id(Hs314921,Highlight)"])
            ]
        );

        // Act: apply pruning logic
        foreach (var link in aps.LinkUris.ToList())
        {
            var decision =
                LinkDecisionEngine.Decide(authoritativeAta, link);

            if (decision == LinkDecision.Remove)
                aps.LinkUris.Remove(link);
        }

        // Assert
        aps.LinkUris.Count.ShouldBe(1);
        aps.LinkUris.Single().SdrStrings.Single()
            .ShouldContain("21-20-02");
    }
}
