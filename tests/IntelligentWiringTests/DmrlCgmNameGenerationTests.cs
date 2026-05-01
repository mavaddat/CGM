using codessentials.CGM.Tests.Models;
using codessentials.CGM.Tests.Services;
using NUnit.Framework;
using Shouldly;

namespace codessentials.CGM.Tests.IntelligentWiringTests;

[TestFixture]
public class DmrlCgmNameGenerationTests
{
    [Test]
    public void GenerateCgmNewName_FromDmrlFields()
    {
        var record = new DataModuleRequirementsList(PROJECT_ID: "78GXF7543", SHEET_ID: 1, CONTROL_REF: "ICN-BD700-A-J912120-W-F7543-00003-A-001-01", ID: "F0001", SHEET_TOT: 2, DMC: "BD700-A-J91-21-20-01AAE-051A-A", CGM_NEW_NAME: string.Empty, Low: 70006, High: 79999);

        var cgmName = DmrlCgmNameGenerator.Generate(record);

        cgmName.ShouldBe("21-20-01_AAE_Sheet_1_of_2.cgm");
    }
}
