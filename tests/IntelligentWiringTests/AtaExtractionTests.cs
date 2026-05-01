using codessentials.CGM.Tests.Models;
using codessentials.CGM.Tests.Services;
using NUnit.Framework;
using Shouldly;

namespace codessentials.CGM.Tests.IntelligentWiringTests;

[TestFixture]
public class AtaExtractionTests
{
    [Test]
    public void Extracts_Valid_Ata_From_Aps_Strings()
    {
        var aps = new ApplicationStructureContext(
            apsId: "Hs123",
            strings: s_aps_strings);

        var ata = AtaExtractor.FromApplicationStructure(aps);

        ata.ShouldNotBeNull();
        ata!.Value.ShouldBe("21-51-34");
    }

    [Test]
    public void Returns_Null_When_No_Valid_Ata()
    {
        var aps = new ApplicationStructureContext(
            apsId: "Hs124",
            strings: s_novalidata_strings);

        AtaExtractor.FromApplicationStructure(aps)
            .ShouldBeNull();
    }
    [Test]
    public void Extracts_Ata_From_Cgm_Path()
    {
        AtaExtractor.FromPath(
            "../../../21-20-05.AAE/Sheet 1 of 1.cgm")
            .ShouldBe(new AtaCode("21-20-05"));
    }

    [Test]
    public void Returns_Null_When_No_Ata_In_Path()
    {
        AtaExtractor.FromPath(
            "../../../Some/Other/Path/Sheet 1 of 1.cgm")
            .ShouldBeNull();
    }
    private static readonly string[] s_novalidata_strings =
            [
                "Not an ATA",
                "ABC-DEF"
            ];
    private static readonly string[] s_aps_strings =
            [
                "Some text",
                "21-51-34"
            ];
}
