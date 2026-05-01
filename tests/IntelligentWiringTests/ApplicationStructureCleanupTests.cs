using codessentials.CGM.Tests.Models;
using NUnit.Framework;
using Shouldly;

namespace codessentials.CGM.Tests.IntelligentWiringTests;

[TestFixture]
public class ApplicationStructureCleanupTests
{
    [Test]
    public void Removes_Aps_When_No_LinkUris_Remain()
    {
        var aps = new ApplicationStructureContext(
            apsId: "Hs999",
            linkUris: []);

        aps.IsEmptyAfterFiltering()
           .ShouldBeTrue();
    }
}
