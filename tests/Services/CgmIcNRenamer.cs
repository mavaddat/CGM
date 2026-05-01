using System.Collections.Generic;
using codessentials.CGM.Tests.Models;

namespace codessentials.CGM.Tests.Services;

public static class CgmIcNRenamer
{
    public static CgmRenamePlan BuildRenamePlan(
        IEnumerable<string> flattenedCgms,
        IReadOnlyDictionary<string, string> cgmToIcnMap)
    {
        var matched = new List<CgmRenameAction>();
        var unmatched = new List<string>();

        foreach (var cgm in flattenedCgms)
        {
            if (cgmToIcnMap.TryGetValue(cgm, out var icn))
            {
                matched.Add(
                    new CgmRenameAction(
                        SourceFile: cgm,
                        DestinationFile: $"{icn}.cgm"));
            }
            else
            {
                unmatched.Add(cgm);
            }
        }

        return new CgmRenamePlan(matched, unmatched);
    }
}
