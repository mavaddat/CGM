using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using codessentials.CGM.Classes;
using codessentials.CGM.Commands;
using codessentials.CGM.Tests.Models;
using codessentials.CGM.Tests.Services;
using CsvHelper;
using CsvHelper.Configuration;
using NUnit.Framework;
using Shouldly;

namespace codessentials.CGM.Tests.IntelligentWiringTests;

[TestFixture]
class RealCgmLinkPruningTests : CgmTest
{
    private readonly CsvConfiguration _csvConfig =
        new(CultureInfo.InvariantCulture)
        {
            Delimiter = "\t",
        };

    [Test]
    public void Real_Cgms_Are_Pruned_By_Dmrl_Ata()
    {
        var assembly = GetType().Assembly;

        // --- Load real DMRL.dt ---
        var dtResource = assembly
            .GetManifestResourceNames()
            .First(n => n.EndsWith(".dt", StringComparison.OrdinalIgnoreCase));

        using var dtStream = assembly.GetManifestResourceStream(dtResource);
        using var reader = new StreamReader(dtStream!);
        using var csv = new CsvReader(reader, _csvConfig);

        var dmrlRecords =
            csv.GetRecords<DataModuleRequirementsList>().ToList();

        dmrlRecords.ShouldNotBeEmpty();

        // --- Pick a few real CGMs ---
        var cgms = ApplicationStructureLinkEnumerationTests
            .GetTestCgms(assembly, take: 3);

        foreach (var resourceName in cgms)
        {
            TestContext.WriteLine($"Processing CGM: {resourceName}");

            var binaryFile =
                ReadBinaryFileByFullResourceName(resourceName, assembly);

            // For integration testing, just pick a valid DMRL ATA
            var dmrl = dmrlRecords.First();

            var ataMatch =
                Regex.Match(dmrl.DMC, @"(?<ata>\d{2}-\d{2}-\d{2})");

            ataMatch.Success.ShouldBeTrue(
                $"Cannot extract ATA from DMC {dmrl.DMC}");

            var authoritativeAta =
                new AtaCode(ataMatch.Groups["ata"].Value);

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
                        var removed = new List<string>();
                        var kept = new List<string>();

                        foreach (var link in aps.LinkUris.ToList())
                        {
                            var decision =
                                LinkDecisionEngine.Decide(
                                    authoritativeAta, link);

                            var path =
                                link.SdrStrings.FirstOrDefault() ?? "<no path>";

                            if (decision == LinkDecision.Remove)
                            {
                                removed.Add(path);
                                aps.LinkUris.Remove(link);
                            }
                            else
                            {
                                kept.Add(path);
                            }
                        }

                        if (removed.Count > 0 || kept.Count > 0)
                        {
                            TestContext.WriteLine($"  APS {aps.ApsId}");

                            foreach (var k in kept)
                                TestContext.WriteLine($"    KEPT:    {k}");

                            foreach (var r in removed)
                            {
                                var ata = AtaExtractor.FromPath(r);
                                TestContext.WriteLine(
                                    $"    REMOVED: {r} (ATA {ata?.Value ?? "<none>"} ≠ {authoritativeAta.Value})");
                            }
                        }

                        // ✅ Invariant: no surviving link may point to a different ATA
                        aps.LinkUris.Any(l =>
                            l.SdrStrings.Any(s =>
                                AtaExtractor.FromPath(s)?.Value != null &&
                                AtaExtractor.FromPath(s)!.Value != authoritativeAta.Value))
                        .ShouldBeFalse(
                            $"Cross-ATA link survived in APS {aps.ApsId}");
                    }

                    aps = null;
                    depth--;
                }
                else if (depth > 0 && aps != null &&
                         cmd is ApplicationStructureAttribute attr &&
                         attr.AttributeType.Equals(
                             "linkuri",
                             StringComparison.OrdinalIgnoreCase))
                {
                    var strings =
                        attr.Data?.Members?
                            .Where(m => m.Type ==
                                StructuredDataRecord.StructuredDataType.S)
                            .SelectMany(m => m.Data)
                            .OfType<string>()
                            .ToList()
                        ?? [];

                    aps.LinkUris.Add(new LinkUriContext(strings));
                }
            }
        }
    }

}
