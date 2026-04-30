using System;
using System.Globalization;
using System.IO;
using System.Linq;
using codessentials.CGM.Tests.Models;
using CsvHelper;
using CsvHelper.Configuration;
using NUnit.Framework;
using Shouldly;

namespace codessentials.CGM.Tests.IntelligentWiringTests;


[TestFixture]
public class DmrlParsingTests
{
    private readonly CsvConfiguration _config = new(CultureInfo.InvariantCulture)
    {
        Delimiter = "\t",
    };

    [Test]
    public void Parse_Dmrl_And_Build_CgmIdentityMap()
    {
        var assembly = GetType().Assembly;

        var dtResource = assembly
            .GetManifestResourceNames()
            .Where(n => n.EndsWith(".dt", StringComparison.OrdinalIgnoreCase))
            .Take(1)
            .First();

        Assert.That(dtResource, Is.Not.Null,
            "No embedded DT test files were found.");

        var stream = assembly.GetManifestResourceStream(dtResource);
        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, _config);

        var records = csv.GetRecords<DataModuleRequirementsList>().ToList();

        records.ShouldHaveCount(1114);
        records[0].CONTROL_REF.ShouldStartWith("ICN-");
        records[0].Low.ShouldBe(70006);
        records[0].High.ShouldBe(79999);
    }
}

