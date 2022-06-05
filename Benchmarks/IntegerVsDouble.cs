using BenchmarkDotNet.Attributes;
using Ngb.Geographic;

namespace Benchmarks;

[MemoryDiagnoser]
public class IntegerVsDouble {
    [Benchmark]
    public long WithInteger() {
        var coord = new GeoCoordinate(106.709437, -6.329094);
        return coord.GetHash();
    }

    // [Benchmark]
    // public long WithDouble() {
    //     var coord = new GeoCoordinate(106.709437, -6.329094);
    //     return coord.GetHash2();
    // }
}
