using System.Collections.Generic;
using codessentials.CGM.Tests.Services;
using NUnit.Framework;
using Shouldly;

namespace codessentials.CGM.Tests.IntelligentWiringTests;

[TestFixture]
public class CgmIcNRenamingIntegrationTests
{
    [Test]
    public void Generate_Copy_Plan_For_Icn_Rename()
    {
        var flattenedCgms = new[]
        {
            "21-20-01_AAE_Sheet_1_of_2.cgm",
            "21-20-01_AAE_Sheet_2_of_2.cgm"
        };

        var map = new Dictionary<string, string>
        {
            ["21-20-01_AAE_Sheet_1_of_2.cgm"] =
                "ICN-BD700-A-J912120-W-F7543-00003-A-002-01"
        };

        var plan = CgmIcNRenamer.BuildRenamePlan(
            flattenedCgms, map);

        plan.Matched.Count.ShouldBe(1);
        plan.Unmatched.Count.ShouldBe(1);

        plan.Matched[0].DestinationFile
            .ShouldBe("ICN-BD700-A-J912120-W-F7543-00003-A-002-01.cgm");
    }
}
