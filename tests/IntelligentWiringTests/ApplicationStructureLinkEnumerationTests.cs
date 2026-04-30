using System;
using System.Linq;
using System.Text.RegularExpressions;
using codessentials.CGM.Classes;
using codessentials.CGM.Commands;
using NUnit.Framework;

namespace codessentials.CGM.Tests.IntelligentWiringTests;

[TestFixture]
partial class ApplicationStructureLinkEnumerationTests : CgmTest
{
    [Test]
    public void Enumerate_LinkUri_ApplicationStructureAttributes()
    {
        // Arrange
        var assembly = GetType().Assembly;

        var cgmResourceNames = assembly
            .GetManifestResourceNames()
            .Where(n => SheetPattern().IsMatch(n) && n.EndsWith(".cgm", StringComparison.OrdinalIgnoreCase))
            .Take(5)
            .ToList();

        Assert.That(cgmResourceNames.Count, Is.GreaterThan(0),
            "No embedded CGM test files were found.");

        foreach (var resourceName in cgmResourceNames)
        {
            TestContext.WriteLine($"--- Processing CGM: {resourceName} ---");

            var binaryFile = ReadBinaryFileByFullResourceName(resourceName, assembly);

            // Track current Application Structure context
            BeginApplicationStructure currentAps = null;
            var apsDepth = 0;

            foreach (var command in binaryFile.Commands)
            {
                // Enter Application Structure
                if (command is BeginApplicationStructure beginAps)
                {
                    apsDepth++;
                    currentAps = beginAps;

                    TestContext.WriteLine(
                        $"ENTER APS: Id='{beginAps.Id}', Type='{beginAps.Type}', Depth={apsDepth}");
                    continue;
                }

                // Exit Application Structure
                if (command is EndApplicationStructure)
                {
                    TestContext.WriteLine(
                        $"EXIT APS: Id='{currentAps?.Id}', Depth={apsDepth}");

                    apsDepth--;
                    if (apsDepth == 0)
                        currentAps = null;

                    continue;
                }

                // We only care about attributes inside APS
                if (currentAps == null)
                    continue;

                // Look for ApplicationStructureAttribute
                if (command is ApplicationStructureAttribute apsAttr)
                {
                    if (!string.Equals(
                            apsAttr.AttributeType,
                            "linkuri",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    TestContext.WriteLine(
                        $"  LINKURI found in APS '{currentAps.Id}'");

                    // Inspect Structured Data Record
                    var sdr = apsAttr.Data;
                    if (sdr == null || sdr.Members == null)
                    {
                        TestContext.WriteLine("    (No SDR data)");
                        continue;
                    }

                    foreach (var member in sdr.Members)
                    {
                        // String entries correspond to <sdr_string>
                        if (member.Type == StructuredDataRecord.StructuredDataType.S)
                        {
                            foreach (var value in member.Data)
                            {
                                TestContext.WriteLine(
                                    $"    SDR String: '{value}'");
                            }
                        }
                    }
                }
            }

            if (binaryFile.Messages.Any())
            {
                TestContext.WriteLine("CGM parser messages:");
                foreach (var msg in binaryFile.Messages)
                    TestContext.WriteLine(msg.ToString());
            }
            var fatalNonApsErrors = binaryFile.Messages
                .Where(m => !m.ToString().Contains("ApplicationStructureAttribute"))
                .ToList();

            if (fatalNonApsErrors.Count != 0)
            {
                Assert.Fail(
                    $"Unexpected CGM parser errors:{Environment.NewLine}" +
                    string.Join(Environment.NewLine, fatalNonApsErrors));
            }

        }
    }

    [GeneratedRegex(@"Sheet \d+ of \d+")]
    private static partial Regex SheetPattern();
}
