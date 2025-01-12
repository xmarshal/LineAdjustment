using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using LineAdjustment;
using Microsoft.Extensions.ObjectPool;

var summary = BenchmarkRunner.Run<ToCacheOrNotToCache>();

[MemoryDiagnoser]
public class ToCacheOrNotToCache
{
    private const string Input = "Lorem ipsum dolor sit amet consectetur adipiscing elit sed do eiusmod tempor incididunt ut labore et dolore magna aliqua";
    private const int LineWidth = 12;

    private readonly ObjectPool<StringBuilder> _stringBuilderPool;

    public ToCacheOrNotToCache()
    {
        var objectPoolProvider = new DefaultObjectPoolProvider();
        _stringBuilderPool = objectPoolProvider.CreateStringBuilderPool(1024, int.MaxValue);
    }

    [Benchmark]
    public void UseCache()
    {
        var algorithm = new LineAdjustmentAlgorithm(_stringBuilderPool) { UseSpaceCache = true };
        var output = algorithm.Transform(Input, LineWidth);
    }

    [Benchmark]
    public void NotUseCache()
    {
        var algorithm = new LineAdjustmentAlgorithm(_stringBuilderPool) { UseSpaceCache = true };
        var output = algorithm.Transform(Input, LineWidth);
    }
}