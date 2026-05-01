using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using codessentials.CGM.Tests.Models;
using codessentials.CGM.Tests.Services;
using NUnit.Framework;
using Shouldly;

namespace codessentials.CGM.Tests.IntelligentWiringTests;

[TestFixture]
public class DmrlSortingAndDeduplicationTests
{
    [Test]
    public void Deduplicate_By_ControlRef_Prefix()
    {
        var records = new List<DataModuleRequirementsList>
        {
            new DataModuleRequirementsList(PROJECT_ID: "78GXF7543", SHEET_ID: 1, CONTROL_REF: "ICN-BD700-A-J912120-W-F7543-00003-A-001-01", ID: "F0001", SHEET_TOT: 2, DMC: "BD700-A-J91-21-20-01AAE-051A-A", CGM_NEW_NAME: string.Empty, Low: 70006, High: 79999) with
            {
                CONTROL_REF = "ICN-BD700-A-J918010-W-F7543-00180-A-001-01",
                DMC = "X",
                SHEET_ID = 1
            }
,
            new DataModuleRequirementsList(PROJECT_ID: "78GXF7543", SHEET_ID: 2, CONTROL_REF: "ICN-BD700-A-J912120-W-F7543-00004-A-001-01", ID: "F0001", SHEET_TOT: 2, DMC: "BD700-A-J91-21-20-01AAE-051A-A", CGM_NEW_NAME: string.Empty, Low: 70006, High: 79999) with
            {
                CONTROL_REF = "ICN-BD700-A-J918010-W-F7543-00180-A-002-01",
                DMC = "X",
                SHEET_ID = 2
            }
,
            new DataModuleRequirementsList(PROJECT_ID: "78GXF7543", SHEET_ID: 1, CONTROL_REF: "ICN-BD700-A-J912120-W-F7543-00001-A-001-01", ID: "F0001", SHEET_TOT: 1, DMC: "BD700-A-J91-21-20-02AAC-051A-A", CGM_NEW_NAME: string.Empty, Low: 70006, High: 79999) with
            {
                CONTROL_REF = "ICN-BD700-A-J918010-W-F7543-00181-A-001-01",
                DMC = "Y",
                SHEET_ID = 1
            }

        };

        var cleaned = DmrlProcessor.SortAndDeduplicate(records);

        cleaned.Count.ShouldBe(2);
        cleaned.Any(r => r.CONTROL_REF.StartsWith("ICN-BD700-A")).ShouldBeTrue();
    }
}
