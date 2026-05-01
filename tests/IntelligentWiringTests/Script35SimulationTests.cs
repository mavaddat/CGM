using System.Linq;
using codessentials.CGM.Tests.Models;
using codessentials.CGM.Tests.Services;
using NUnit.Framework;
using Shouldly;

namespace codessentials.CGM.Tests.IntelligentWiringTests;

[TestFixture]
public class Script35SimulationTests
{
    private static readonly string[] s_aps_ata_invalid_strings =
            [
                "Not an ATA",
                "ABC-DEF"
            ];
    private static readonly string[] s_sdrStrings =
                [
                    "../../../27-04-01.AAD/Sheet 2 of 2.cgm#id(Hs1,Highlight)"
                ];
}
