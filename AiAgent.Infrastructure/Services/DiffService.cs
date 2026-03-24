using AiAgent.Core.Models;

namespace AiAgent.Infrastructure.Services;

public sealed class DiffService
{
    public DiffResult CreateDiff(string originalCode, string modifiedCode)
    {
        var originalLines = SplitLines(originalCode);
        var updatedLines = SplitLines(modifiedCode);
        var diffLines = BuildDiff(originalLines, updatedLines);

        return new DiffResult
        {
            Lines = diffLines
        };
    }

    private static IReadOnlyList<string> SplitLines(string content)
        => content.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');

    private static IReadOnlyList<DiffLine> BuildDiff(IReadOnlyList<string> original, IReadOnlyList<string> updated)
    {
        var lcs = BuildLcsTable(original, updated);
        var diff = new List<DiffLine>();
        BuildDiffRecursive(original, updated, original.Count, updated.Count, lcs, diff);
        return diff;
    }

    private static int[,] BuildLcsTable(IReadOnlyList<string> original, IReadOnlyList<string> updated)
    {
        var table = new int[original.Count + 1, updated.Count + 1];
        for (var i = 1; i <= original.Count; i++)
        {
            for (var j = 1; j <= updated.Count; j++)
            {
                if (original[i - 1] == updated[j - 1])
                {
                    table[i, j] = table[i - 1, j - 1] + 1;
                }
                else
                {
                    table[i, j] = Math.Max(table[i - 1, j], table[i, j - 1]);
                }
            }
        }

        return table;
    }

    private static void BuildDiffRecursive(
        IReadOnlyList<string> original,
        IReadOnlyList<string> updated,
        int i,
        int j,
        int[,] lcs,
        List<DiffLine> diff)
    {
        if (i > 0 && j > 0 && original[i - 1] == updated[j - 1])
        {
            BuildDiffRecursive(original, updated, i - 1, j - 1, lcs, diff);
            diff.Add(new DiffLine { Prefix = " ", Text = original[i - 1] });
        }
        else if (j > 0 && (i == 0 || lcs[i, j - 1] >= lcs[i - 1, j]))
        {
            BuildDiffRecursive(original, updated, i, j - 1, lcs, diff);
            diff.Add(new DiffLine { Prefix = "+", Text = updated[j - 1] });
        }
        else if (i > 0 && (j == 0 || lcs[i, j - 1] < lcs[i - 1, j]))
        {
            BuildDiffRecursive(original, updated, i - 1, j, lcs, diff);
            diff.Add(new DiffLine { Prefix = "-", Text = original[i - 1] });
        }
    }
}
