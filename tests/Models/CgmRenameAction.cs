using System.Collections.Generic;

namespace codessentials.CGM.Tests.Models;


public record CgmRenameAction(string SourceFile, string DestinationFile);
public record CgmRenamePlan(
    IReadOnlyList<CgmRenameAction> Matched,
    IReadOnlyList<string> Unmatched
);
