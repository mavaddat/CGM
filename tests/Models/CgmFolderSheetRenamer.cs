using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace codessentials.CGM.Tests.Models;


public static partial class CgmFolderSheetRenamer
{

    /// <summary>
    /// Computes a flattened CGM filename from a Capital Publisher
    /// folder/sheet relative path.
    ///
    /// Example:
    ///   21 - Environmental control\21-20 Distribution\21-20-01.AAE\Sheet 1 of 2.cgm
    /// → 21-20-01_AAE_Sheet_1_of_2.cgm
    /// </summary>
    public static string ComputeFlattenedName(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            throw new ArgumentException("Relative path cannot be null or empty.", nameof(relativePath));

        // Normalize separators (defensive)
        var parts = relativePath
            .Split(['\\', '/'], StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 2)
            throw new InvalidOperationException(
                $"Path does not contain enough segments: {relativePath}");

        // --- Find diagram folder (NN-NN-NN.AAA) ---
        var diagramFolder = parts.FirstOrDefault(p => DiagramFolderPattern().IsMatch(p));
        if (diagramFolder == null)
            throw new InvalidOperationException(
                $"No diagram folder (NN-NN-NN.AAA) found in path: {relativePath}");

        // --- Sheet file must be last ---
        var sheetFile = parts.Last();
        if (!SheetFilePattern().IsMatch(sheetFile))
            throw new InvalidOperationException(
                $"Last path segment is not a Sheet CGM: {relativePath}");

        // --- Normalize diagram folder ---
        // 21-20-01.AAE → 21-20-01_AAE
        var normalizedDiagram =
            diagramFolder.Replace('.', '_');

        // --- Normalize sheet file ---
        // Sheet 1 of 2.cgm → Sheet_1_of_2.cgm
        var normalizedSheet =
            sheetFile.Replace(' ', '_');

        return $"{normalizedDiagram}_{normalizedSheet}";
    }

    // Matches diagram folders like: 21-20-01.AAE
    [GeneratedRegex(@"^\d{2}-\d{2}-\d{2}\.[A-Z]{3}$", RegexOptions.Compiled)]
    private static partial Regex DiagramFolderPattern();

    // Matches sheet files like: Sheet 1 of 2.cgm
    [GeneratedRegex(@"^Sheet\s+\d+\s+of\s+\d+\.cgm$", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-CA")]
    private static partial Regex SheetFilePattern();
}

