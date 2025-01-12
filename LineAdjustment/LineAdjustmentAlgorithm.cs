using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.ObjectPool;

namespace LineAdjustment;

public class LineAdjustmentAlgorithm
{
    private readonly ObjectPool<StringBuilder> _stringBuilderPool;

    public LineAdjustmentAlgorithm(ObjectPool<StringBuilder> stringBuilderPool)
    {
        _stringBuilderPool = stringBuilderPool;
    }

    public bool UseSpaceCache { get; init; }

    /// <exception cref="ArgumentOutOfRangeException">Если заданная длинна отрицательная</exception>
    /// <exception cref="InvalidOperationException">Длина слова превышает максимальную ширину стоки</exception>
    public string Transform(string input, int lineWidth)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(lineWidth);

        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        return Transform(input.AsSpan(), lineWidth);
    }

    /// <exception cref="InvalidOperationException">Длина слова превышает максимальную ширину стоки</exception>
    private string Transform(ReadOnlySpan<char> input, int lineWidth)
    {
        var linesBuilder = _stringBuilderPool.Get();
        linesBuilder.Clear();
        linesBuilder.Capacity = input.Length * 2;

        try
        {
            var ranges = input.Split(' ');

            var currentLineRanges = new List<Range>();
            var currentLength = 0;

            foreach (var range in ranges)
            {
                var wordLength = range.GetOffsetAndLength(input.Length).Length;
                if (wordLength == 0)
                {
                    continue;
                }

                if (wordLength > lineWidth)
                {
                    throw new InvalidOperationException($"Word length exceeds Line width: {input[range]}");
                }

                if (currentLength + wordLength + currentLineRanges.Count <= lineWidth)
                {
                    currentLineRanges.Add(range);
                    currentLength += wordLength;
                }
                else
                {
                    linesBuilder = JustifyLine(input, currentLineRanges, linesBuilder, lineWidth)
                        .Append('\n');

                    currentLineRanges.Clear();
                    currentLineRanges.Add(range);
                    currentLength = wordLength;
                }
            }

            return JustifyLine(input, currentLineRanges, linesBuilder, lineWidth)
                .ToString();
        }
        finally
        {
            _stringBuilderPool.Return(linesBuilder);
        }
    }

    private StringBuilder JustifyLine(ReadOnlySpan<char> src, List<Range> ranges, StringBuilder dst, int lineWidth)
    {
        if (UseSpaceCache)
        {
            return JustifyLineCache(src, ranges, dst, lineWidth);
        }

        return JustifyLineNoCache(src, ranges, dst, lineWidth);
    }

    private static StringBuilder JustifyLineNoCache(ReadOnlySpan<char> src, List<Range> ranges, StringBuilder dst, int lineWidth)
    {
        dst.Append(src[ranges[0]]);

        if (ranges.Count == 1)
        {
            return dst.Append(' ', lineWidth - src[ranges[0]].Length);
        }

        var srcLenght = src.Length;
        var capacity = ranges.Sum(r => r.GetOffsetAndLength(srcLenght).Length);
        var totalSpaces = lineWidth - capacity;
        var spaceSlots = ranges.Count - 1;

        if (spaceSlots <= 0)
        {
            return dst.Append(' ', totalSpaces);
        }

        var spaceBetweenWords = totalSpaces / spaceSlots;
        var extraSpaces = totalSpaces % spaceSlots;

        for (var i = 1; i < ranges.Count; i++)
        {
            var spaces = spaceBetweenWords + (i <= extraSpaces ? 1 : 0);
            dst.Append(' ', spaces);
            dst.Append(src[ranges[i]]);
        }

        return dst;
    }

    private static StringBuilder JustifyLineCache(ReadOnlySpan<char> src, List<Range> ranges, StringBuilder dst, int lineWidth)
    {
        Span<char> spacesMax = stackalloc char[lineWidth];
        spacesMax.Fill(' ');

        dst.Append(src[ranges[0]]);

        if (ranges.Count == 1)
        {
            return dst.Append(spacesMax[..(lineWidth - src[ranges[0]].Length)]);
        }

        var srcLenght = src.Length;
        var capacity = ranges.Sum(r => r.GetOffsetAndLength(srcLenght).Length);
        var totalSpaces = lineWidth - capacity;
        var spaceSlots = ranges.Count - 1;

        if (spaceSlots <= 0)
        {
            return dst.Append(spacesMax[..totalSpaces]);
        }

        var spaceBetweenWords = totalSpaces / spaceSlots;
        var extraSpaces = totalSpaces % spaceSlots;

        for (var i = 1; i < ranges.Count; i++)
        {
            var spaces = spaceBetweenWords + (i <= extraSpaces ? 1 : 0);
            dst.Append(spacesMax[..spaces]);
            dst.Append(src[ranges[i]]);
        }

        return dst;
    }
}