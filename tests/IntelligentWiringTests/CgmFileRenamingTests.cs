using System;
using System.Linq;
using codessentials.CGM.Tests.Models;
using NUnit.Framework;
using Shouldly;

namespace codessentials.CGM.Tests.IntelligentWiringTests;

[TestFixture]
class CgmFileRenamingTests
{

    [Test]
    public void ComputeFlattenedName_BasicFolderAndSheet()
    {
        var relativePath =
            @"21 - Environmental control\21-20 Distribution\21-20-01.AAE\Sheet 1 of 2.cgm";

        var flattened =
            CgmFolderSheetRenamer.ComputeFlattenedName(relativePath);

        flattened.ShouldBe("21-20-01_AAE_Sheet_1_of_2.cgm");
    }

    [Test]
    public void ComputeFlattenedName_DifferentSheetNumber()
    {
        var relativePath =
            @"Some System\More Stuff\34-56-78.ZZZ\Sheet 12 of 34.cgm";

        var flattened =
            CgmFolderSheetRenamer.ComputeFlattenedName(relativePath);

        flattened.ShouldBe("34-56-78_ZZZ_Sheet_12_of_34.cgm");
    }

    [Test]
    public void ComputeFlattenedName_Throws_When_NoDiagramFolder()
    {
        var relativePath =
            @"Invalid\Path\NoDiagramHere\Sheet 1 of 2.cgm";

        Assert.Throws<InvalidOperationException>(() =>
            CgmFolderSheetRenamer.ComputeFlattenedName(relativePath));
    }

    [Test]
    public void ComputeFlattenedName_Throws_When_NoSheetFile()
    {
        var relativePath =
            @"21-20-01.AAE\NotASheet.cgm";

        Assert.Throws<InvalidOperationException>(() =>
            CgmFolderSheetRenamer.ComputeFlattenedName(relativePath));
    }

    internal static readonly string[] Expected =
        [
            "21-20-01_AAE_Sheet_1_of_2.cgm",
            "21-20-01_AAE_Sheet_2_of_2.cgm"
        ];

    [Test]
    public void FlattenMultiplePaths()
    {
        var inputs = new[]
        {
            @"21-20-01.AAE\Sheet 1 of 2.cgm",
            @"21-20-01.AAE\Sheet 2 of 2.cgm"
        };

        var outputs = inputs
            .Select(CgmFolderSheetRenamer.ComputeFlattenedName)
            .ToArray();

        outputs.ShouldBeEquivalentTo(Expected);
    }
}
