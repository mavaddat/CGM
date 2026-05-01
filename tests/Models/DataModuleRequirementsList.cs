namespace codessentials.CGM.Tests.Models;

public sealed partial record DataModuleRequirementsList(
     string PROJECT_ID,
     int SHEET_ID,
     string CONTROL_REF,
     string ID,
     int SHEET_TOT,
     string DMC,
     string CGM_NEW_NAME,
     int Low,
     int High
    )
{
}
