using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LineAdjustment;

public class LineAdjustmentAlgorithm
{
    public string Transform(string input, int lineWidth)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        var linesBuilder = new StringBuilder(input.Length * 2);

        //Span<Range> ranges = stackalloc Range[input.Length];
        var inputSpan = input.AsSpan();
        // var cnt = inputSpan.Split(ranges, [' '], StringSplitOptions.RemoveEmptyEntries);
        var ranges = inputSpan.Split(' ');

        var currentLineRanges = new List<Range>(input.Length);
        var currentLength = 0;

        foreach (var range in ranges)
        {
            var word = inputSpan[range];
            var wordLength = word.Length;
            if (wordLength == 0)
            {
                continue;
            }

            if (currentLength + wordLength + currentLineRanges.Count <= lineWidth)
            {
                currentLineRanges.Add(range);
                currentLength += wordLength;
            }
            else
            {
                var justifyLine = JustifyLine(inputSpan, currentLineRanges, lineWidth);

                linesBuilder.Append(justifyLine).Append('\n');

                currentLineRanges.Clear();
                currentLineRanges.Add(range);
                currentLength = wordLength;
            }
        }

        var lastLine = JustifyLine(inputSpan, currentLineRanges, lineWidth);
        linesBuilder.Append(lastLine);

        return linesBuilder.ToString();
    }

    private static StringBuilder JustifyLine(ReadOnlySpan<char> src, List<Range> ranges, int lineWidth)
    {
        var spacesMax = new string(' ', lineWidth).AsSpan();

        var capacity = ranges.Sum(r => r.End.Value - r.Start.Value);

        var result = new StringBuilder(capacity);

        result.Append(src[ranges[0]]);

        if (ranges.Count == 1)
        {
            return result.Append(spacesMax[..(lineWidth - src[ranges[0]].Length)]);
        }

        var totalSpaces = lineWidth - capacity;
        var spaceSlots = ranges.Count - 1;

        if (spaceSlots <= 0)
        {
            return result.Append(spacesMax[..totalSpaces]);
        }

        var spaceBetweenWords = totalSpaces / spaceSlots;
        var extraSpaces = totalSpaces % spaceSlots;

        for (var i = 1; i < ranges.Count; i++)
        {
            var spaces = spaceBetweenWords + (i <= extraSpaces ? 1 : 0);
            result.Append(spacesMax[..spaces]);
            result.Append(src[ranges[i]]);
        }

        return result;
    }
}