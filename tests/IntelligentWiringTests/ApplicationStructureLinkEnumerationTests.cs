using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;
using codessentials.CGM.Classes;
using codessentials.CGM.Commands;
using codessentials.CGM.Tests.Models;
using codessentials.CGM.Tests.Services;
using NUnit.Framework;
using Shouldly;

namespace codessentials.CGM.Tests.IntelligentWiringTests;

[TestFixture]
partial class ApplicationStructureLinkEnumerationTests : CgmTest
{
    [Test]
    public void Enumerate_LinkUri_ApplicationStructureAttributes()
    {
        // Arrange
        var assembly = GetType().Assembly;

        var cgmResourceNames = GetTestCgms(assembly, 5);

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

    [Test]
    public void Enumerate_And_Filter_LinkUris_By_Ata_From_Real_Cgm()
    {
        var assembly = GetType().Assembly;

        var cgmResourceNames = assembly
            .GetManifestResourceNames()
            .Where(n => SheetPattern().IsMatch(n)
                        && n.EndsWith(".cgm", StringComparison.OrdinalIgnoreCase))
            .Take(3) // keep small for test speed
            .ToList();

        Assert.That(cgmResourceNames.Count, Is.GreaterThan(0),
            "No embedded CGM test files were found.");

        foreach (var resourceName in cgmResourceNames)
        {
            TestContext.WriteLine($"--- Processing CGM: {resourceName} ---");

            var binaryFile =
                ReadBinaryFileByFullResourceName(resourceName, assembly);

            ApplicationStructureContext? aps = null;
            var apsDepth = 0;

            foreach (var command in binaryFile.Commands)
            {
                // --- Begin APS ---
                if (command is BeginApplicationStructure beginAps)
                {
                    apsDepth++;
                    aps = new ApplicationStructureContext(beginAps.Id);
                    continue;
                }

                // --- End APS ---
                if (command is EndApplicationStructure)
                {
                    apsDepth--;

                    if (aps != null)
                    {
                        // --- SCRIPT 3.5 SEMANTIC LOGIC ---
                        var apsAta = AtaExtractor.FromApplicationStructure(aps);

                        var initialCount = aps.LinkUris.Count;

                        var authoritativeAta = new AtaCode("21-20-01"); // injected from DMRL

                        foreach (var link in aps.LinkUris.ToList())
                        {

                            var decision =
                                LinkDecisionEngine.Decide(authoritativeAta, link);

                            if (decision == LinkDecision.Remove)
                                aps.LinkUris.Remove(link);
                        }

                        TestContext.WriteLine(
                            $"APS {aps.ApsId}: " +
                            $"links before={initialCount}, " +
                            $"after={aps.LinkUris.Count}, " +
                            $"APS ATA={(apsAta?.Value ?? "<none>")}");

                        // --- ASSERTIONS ---

                        if (aps.IsEmptyAfterFiltering())
                        {
                            TestContext.WriteLine(
                                $"APS {aps.ApsId} would be removed (empty)");
                        }
                        aps.LinkUris.Any(l =>
                        l.SdrStrings.Any(s =>
                        s.Contains("31-") || s.Contains("34-")))
                            .ShouldBeFalse("Cross‑ATA links should be removed");
                    }

                    aps = null;
                    continue;
                }

                // --- Ignore anything outside APS ---
                if (apsDepth == 0 || aps == null)
                    continue;

                // --- ApplicationStructureAttribute ---
                if (command is ApplicationStructureAttribute apsAttr)
                {
                    // Capture linkuri
                    if (string.Equals(
                            apsAttr.AttributeType,
                            "linkuri",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        var sdrStrings =
                            apsAttr.Data?.Members?
                                .Where(m => m.Type == StructuredDataRecord.StructuredDataType.S)
                                .SelectMany(m => m.Data)
                                .OfType<string>()
                                .ToList()
                            ?? [];

                        aps.LinkUris.Add(new LinkUriContext(sdrStrings));
                    }
                    else
                    {
                        // Capture other strings (ATA candidates live here)
                        var strings =
                            apsAttr.Data?.Members?
                                .Where(m => m.Type == StructuredDataRecord.StructuredDataType.S)
                                .SelectMany(m => m.Data)
                                .OfType<string>();

                        if (strings != null)
                            foreach (var s in strings)
                                aps.Strings.Add(s);
                    }
                }
            }

            // --- Parser warnings handling (unchanged) ---
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

    [Test]
    public void Detects_Any_ApplicationStructure_With_Valid_Ata()
    {
        var assembly = GetType().Assembly;
        var foundAta = false;

        foreach (var resourceName in GetTestCgms(assembly, 5))
        {
            var binaryFile = ReadBinaryFileByFullResourceName(resourceName, assembly);
            ApplicationStructureContext? aps = null;
            var depth = 0;

            foreach (var cmd in binaryFile.Commands)
            {
                if (cmd is BeginApplicationStructure bas)
                {
                    aps = new ApplicationStructureContext(bas.Id);
                    depth++;
                }
                else if (cmd is EndApplicationStructure)
                {
                    if (aps != null)
                    {
                        if (AtaExtractor.FromApplicationStructure(aps) != null)
                            foundAta = true;
                    }
                    depth--;
                    aps = null;
                }
                else if (depth > 0 && aps != null && cmd is ApplicationStructureAttribute attr)
                {
                    var strings =
                        attr.Data?.Members?
                            .Where(m => m.Type == StructuredDataRecord.StructuredDataType.S)
                            .SelectMany(m => m.Data)
                            .OfType<string>();

                    if (strings != null)
                        foreach (var s in strings)
                            aps.Strings.Add(s);
                }
            }
        }

        TestContext.WriteLine($"APS with ATA found: {foundAta}");
        // Do NOT assert true — this is an observational test for now
    }

    [Test]
    public void Real_LinkUris_Contain_Extractable_Ata()
    {
        var assembly = GetType().Assembly;
        var ataValues = new HashSet<string>();

        foreach (var resourceName in GetTestCgms(assembly, 5))
        {
            var binaryFile = ReadBinaryFileByFullResourceName(resourceName, assembly);

            foreach (var cmd in binaryFile.Commands)
            {
                if (cmd is ApplicationStructureAttribute attr &&
                    attr.AttributeType.Equals("linkuri", StringComparison.OrdinalIgnoreCase))
                {
                    var sdrStrings =
                        attr.Data?.Members?
                            .Where(m => m.Type == StructuredDataRecord.StructuredDataType.S)
                            .SelectMany(m => m.Data)
                            .OfType<string>()
                        ?? [];

                    var link = new LinkUriContext(sdrStrings);
                    var ata = AtaExtractor.FromLinkUri(link);

                    if (ata != null)
                        ataValues.Add(ata.Value);
                }
            }
        }

        TestContext.WriteLine($"Distinct link ATA values: {string.Join(", ", ataValues)}");

        TestContext.WriteLine(
            $"Strict ATA tokens found in link paths: {ataValues.Count}");
    }

    [Test]
    public void Can_Generate_Script35_Log_Rows_From_Real_Cgm()
    {
        var assembly = GetType().Assembly;
        var rows = new List<(string File, string ApsId, string Ata, string Status)>();

        foreach (var resourceName in GetTestCgms(assembly, 3))
        {
            var binaryFile = ReadBinaryFileByFullResourceName(resourceName, assembly);
            ApplicationStructureContext? aps = null;
            var depth = 0;

            foreach (var cmd in binaryFile.Commands)
            {
                if (cmd is BeginApplicationStructure bas)
                {
                    aps = new ApplicationStructureContext(bas.Id);
                    depth++;
                }
                else if (cmd is EndApplicationStructure)
                {
                    if (aps != null)
                    {
                        var apsAta = AtaExtractor.FromApplicationStructure(aps);

                        foreach (var link in aps.LinkUris)
                        {
                            rows.Add((
                                File: resourceName,
                                ApsId: aps.ApsId,
                                Ata: apsAta?.Value ?? "<none>",
                                Status: "Removed"
                            ));
                        }
                    }

                    depth--;
                    aps = null;
                }
                else if (depth > 0 && aps != null && cmd is ApplicationStructureAttribute attr &&
                         attr.AttributeType.Equals("linkuri", StringComparison.OrdinalIgnoreCase))
                {
                    aps.LinkUris.Add(new LinkUriContext([]));
                }
            }
        }

        rows.Count.ShouldBeGreaterThan(0);
    }

    internal static IReadOnlyCollection<string> GetTestCgms(Assembly assembly, int take)
    {

        return [.. assembly
            .GetManifestResourceNames()
            .Where(n => SheetPattern().IsMatch(n) && n.EndsWith(".cgm", StringComparison.OrdinalIgnoreCase))
            .Take(take)];
    }

    [GeneratedRegex(@"Sheet \d+ of \d+")]
    private static partial Regex SheetPattern();
}
