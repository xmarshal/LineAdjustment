using System;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.ObjectPool;
using NUnit.Framework;

namespace LineAdjustment.Tests;

public class LineAdjustmentAlgorithmTests
{
    private readonly ObjectPool<StringBuilder> _stringBuilderPool;

    public LineAdjustmentAlgorithmTests()
    {
        var objectPoolProvider = new DefaultObjectPoolProvider();
        _stringBuilderPool = objectPoolProvider.CreateStringBuilderPool(1024, int.MaxValue);
    }

    [Test]
    [TestCase(null, 5, "")]
    [TestCase("", 5, "")]
    [TestCase("test", 5, "test ")]
    [TestCase("Lorem ipsum dolor sit amet consectetur adipiscing elit sed do eiusmod tempor incididunt ut labore et dolore magna aliqua", 12, "Lorem  ipsum\ndolor    sit\namet        \nconsectetur \nadipiscing  \nelit  sed do\neiusmod     \ntempor      \nincididunt  \nut labore et\ndolore magna\naliqua      ")]
    [TestCase("Lorem     ipsum    dolor", 17, "Lorem ipsum dolor")]
    [TestCase("a a a a", 7, "a a a a")]
    [TestCase("a a a a", 8, "a  a a a")]
    [TestCase("Привет мир", 10, "Привет мир")]
    public void All_Cases_Should_Be_Success(string input, int lineWidth, string expected)
    {
        var algorithm = new LineAdjustmentAlgorithm(_stringBuilderPool);
        var output = algorithm.Transform(input, lineWidth);
        output.Should().Be(expected);
    }

    [Test]
    [TestCase(null, 5, "")]
    [TestCase("", 5, "")]
    [TestCase("test", 5, "test ")]
    [TestCase("Lorem ipsum dolor sit amet consectetur adipiscing elit sed do eiusmod tempor incididunt ut labore et dolore magna aliqua", 12, "Lorem  ipsum\ndolor    sit\namet        \nconsectetur \nadipiscing  \nelit  sed do\neiusmod     \ntempor      \nincididunt  \nut labore et\ndolore magna\naliqua      ")]
    [TestCase("Lorem     ipsum    dolor", 17, "Lorem ipsum dolor")]
    [TestCase("a a a a", 7, "a a a a")]
    [TestCase("a a a a", 8, "a  a a a")]
    [TestCase("Привет мир", 10, "Привет мир")]
    public void UseSpaceCahce_All_Cases_Should_Be_Success(string input, int lineWidth, string expected)
    {
        var algorithm = new LineAdjustmentAlgorithm(_stringBuilderPool) { UseSpaceCache = true };
        var output = algorithm.Transform(input, lineWidth);
        output.Should().Be(expected);
    }

    [Test]
    [TestCase("Loremasdfasdddsddfasdffasdfff     ipsum    dolor", 17)]
    [TestCase("Привет мир", 5)]
    public void All_Cases_Should_Throw(string input, int lineWidth)
    {
        var algorithm = new LineAdjustmentAlgorithm(_stringBuilderPool);

        var act = () => algorithm.Transform(input, lineWidth);
        act.Should().Throw<InvalidOperationException>();
    }
}