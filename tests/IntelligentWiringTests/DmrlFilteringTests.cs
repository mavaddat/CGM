using System.Collections.Generic;
using codessentials.CGM.Tests.Models;
using codessentials.CGM.Tests.Services;
using NUnit.Framework;
using Shouldly;

namespace codessentials.CGM.Tests.IntelligentWiringTests;

[TestFixture]
public class DmrlFilteringTests
{
    [Test]
    public void Keep_Only_051A_A_Dmc()
    {
        var records = new List<DataModuleRequirementsList>
        {
            new(PROJECT_ID: "78GXF7543", SHEET_ID: 1, CONTROL_REF: "ICN-BD700-A-J912120-W-F7543-00003-A-001-01", ID: "F0001", SHEET_TOT: 2, DMC: "BD700-A-J91-21-20-01AAE-051A-A", CGM_NEW_NAME: string.Empty, Low: 70006, High: 79999),
            new(PROJECT_ID: "78GXF7543", SHEET_ID: 1, CONTROL_REF: "ICN-BD700-A-J912120-W-F7543-00003-A-001-01", ID: "F0001", SHEET_TOT: 2, DMC: "BD700-A-J91-21-20-01AAE-052A-A", CGM_NEW_NAME: string.Empty, Low: 70006, High: 79999)
        };

        var filtered = DmrlProcessor.FilterFinal(records);

        filtered.Count.ShouldBe(1);
        filtered[0].DMC.ShouldEndWith("-051A-A");
    }
}
