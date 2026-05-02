using codessentials.CGM.Elements;
using codessentials.CGM.Tests.Models;
using codessentials.CGM.Tests.Services;
using NUnit.Framework;
using Shouldly;

namespace codessentials.CGM.Tests.IntelligentWiringTests;

[TestFixture]
public class LinkUriAtaExtractionTests
{
    [Test]
    public void Extracts_Ata_From_Cgm_Path()
    {
        var link = new LinkUriElement(
            destination: "../../../27-04-01.AAD/Sheet 2 of 2.cgm#id(Hs123,Highlight)",
            title: null,
            behavior: null);

        var ata = AtaExtractor.FromLinkUri(link);

        ata.ShouldNotBeNull();
        ata!.Value.ShouldBe("27-04-01");
    }

    [Test]
    public void Returns_Null_When_No_Ata_In_Path()
    {
        var link = new LinkUriElement(
            destination: "../../../Some/Other/Path/file.cgm",
            title: null,
            behavior: null);

        AtaExtractor.FromLinkUri(link)
            .ShouldBeNull();
    }
}
